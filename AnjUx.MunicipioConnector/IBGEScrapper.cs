using ExcelDataReader;
using FluentFTP;
using HtmlAgilityPack;
using System.Globalization;
using System.Data;
using AnjUx.Shared.Extensions;
using AnjUx.Shared.Models.Data;

class IbgeScraper
{
    private readonly HttpClient _client;

    public IbgeScraper()
    {
        _client = new HttpClient();
    }

    public async Task<IEnumerable<Dictionary<string, string>>> ParseHtmlTableList(string url)
    {
        var response = await _client.GetStringAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var table = new List<Dictionary<string, string>>();

        var rows = doc.DocumentNode.SelectNodes("//table//tr");
       
        if (rows == null)
            return table;

        foreach (var row in rows.Skip(1)) // Skip header row
        {
            var cells = row.SelectNodes(".//td");
            if (cells == null) continue;

            var data = new Dictionary<string, string>
            {
                ["url"] = cells[1].SelectSingleNode(".//a")?.GetAttributeValue("href", "").Trim(),
                ["date"] = cells[2].InnerText.Trim(),
                ["size"] = cells[3].InnerText.Trim()
            };

            if (string.IsNullOrEmpty(data["url"]))
                continue;

            table.Add(data);
        }

        return table;
    }

    public async Task<Dictionary<int, Dictionary<string, string>>> ListXlsUrls(int? anoInicial = null, int? anoFinal = null)
    {
        anoInicial ??= 2018;
        anoFinal ??= DateTime.Now.Year;

        var anosCenso = new[] { 1980, 1991, 2000, 2010, 2022 };
        var urls = new Dictionary<int, Dictionary<string, string>>();

        for (int year = anoInicial.Value; year <= anoFinal; year++)
        {
            try
            {
                urls[year] = new Dictionary<string, string>();

                string baseUrl = anosCenso.Contains(year)
                    ? $"https://ftp.ibge.gov.br/Censos/Censo_Demografico_{year}/Previa_da_Populacao/"
                    : $"https://ftp.ibge.gov.br/Estimativas_de_Populacao/Estimativas_{year}/";

                var rows = await ParseHtmlTableList(baseUrl);
                foreach (var row in rows)
                {
                    if (row == null) continue;
                    if (!row.ContainsKey("url") || !row.ContainsKey("date")) continue;
                    if (row["url"] == null || row["date"] == null) continue;

                    if (row["url"].EndsWith(".xls") && !Path.GetFileName(row["url"]).StartsWith("serie"))
                    {
                        var fullUrl = new Uri(new Uri(baseUrl), row["url"]).ToString();
                        urls[year][row["date"]] = fullUrl;
                    }
                }
            }
            catch (Exception)
            {
                // Se cair aqui significa que o ano não tem dados
            }
        }

        return urls;
    }

    public static MemoryStream DownloadFtpFileToMemory(string url)
    {
        var uri = new Uri(url);
        var host = uri.Host;
        var path = uri.AbsolutePath;

        using var client = new FtpClient(host);
        client.Connect();

        var memoryStream = new MemoryStream();
        client.DownloadStream(memoryStream, path);
        memoryStream.Position = 0; // Reset stream position for reading
        client.Disconnect();

        return memoryStream;
    }

    public static List<Dictionary<string, string>> ConvertFileFromMemory(Stream inputStream)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using var reader = ExcelReaderFactory.CreateReader(inputStream);

        var result = new List<Dictionary<string, string>>();

        DataSet dataset = reader.AsDataSet();
        int tableIndex = dataset.Tables.Count > 1 ? 1 : 0;
        DataTable table = dataset.Tables[tableIndex];
        List<string> headers = table.Rows[1].ItemArray.Select(x => x!.ToString()!).Where(x => x.IsNotNullOrWhiteSpace()).ToList();

        for (int i = 2; i < table.Rows.Count; i++)
        {
            List<string> row = table.Rows[i].ItemArray.Select(x => x!.ToString()!).ToList();
            Dictionary<string, string> record = headers.Zip(row, (h, v) => new { h, v }).ToDictionary(x => x.h, x => x.v);
            result.Add(record!);
        }

        return result;
    }

    public async Task<Dictionary<int, Dictionary<string, MunicipioDado>>> Buscar(int? anoInicial = null, int? anoFinal = null, string? codigoMunicipio = null)
    {
        var urls = await ListXlsUrls(anoInicial, anoFinal);

        Dictionary<int, Dictionary<string, MunicipioDado>> resultado = [];

        foreach (var year in urls.Keys)
        {
            foreach (var date in urls[year].Keys)
            {
                var url = urls[year][date];

                using var memoryStream = DownloadFtpFileToMemory(url);

                var records = ConvertFileFromMemory(memoryStream);

                // Se cair aqui é uma das tabelas do censo que é somente por estado, então ignoramos
                if (!records[0].ContainsKey("COD. MUNIC"))
                    break;

                string chavePopulacao = records[0].ContainsKey("POPULAÇÃO") ? "POPULAÇÃO" : "POPULAÇÃO ESTIMADA";

                var recordsFiltrados = records.Where(r => r["COD. MUNIC"].IsNotNullOrWhiteSpace());

                if (codigoMunicipio.IsNotNullOrWhiteSpace())
                    recordsFiltrados = recordsFiltrados.Where(r => $"{r["COD. UF"]}{r["COD. MUNIC"]}" == codigoMunicipio);

                var dict = recordsFiltrados.ToDictionary(r => $"{r["COD. UF"]}{r["COD. MUNIC"]}", r => new MunicipioDado()
                {
                    Municipio = new Municipio() { CodigoIBGE = $"{r["COD. UF"]}{r["COD. MUNIC"]}" },
                    Ano = year,
                    Mes = 1,
                    DataBase = new DateTime(year, 1, 1),
                    TipoDado = TipoDado.Populacao,
                    Valor = GetPopulacao(r, chavePopulacao)
                });

                foreach(string key in dict.Keys)
                {
                    if (!resultado.ContainsKey(year))
                        resultado[year] = [];

                    resultado[year][key] = dict[key];
                }
            }
        }
        
        return resultado;

        static decimal GetPopulacao(Dictionary<string, string> dict, string chave) 
        {
            try
            {
                string valor = dict[chave];

                if (valor.Contains('('))
                    valor = valor[..valor.IndexOf('(')].Trim();

                return decimal.Parse(valor, CultureInfo.GetCultureInfo("pt-BR"));
            }
            catch (Exception ex)
            {
                throw ex.ReThrow();
            }
        }
    }

}
