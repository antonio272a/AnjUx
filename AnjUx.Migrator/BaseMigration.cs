using FluentMigrator;
using Microsoft.IdentityModel.Tokens;

namespace AnjUx.Migrator
{
	public abstract class BaseMigration : Migration
	{
		protected void ColunaIndexadaComMultiplosNulos(string tabela, string coluna, string? esquema = null)
		{
			string endereco = esquema.IsNullOrEmpty()
				? tabela
				: $"{esquema}.{tabela}";

			var sql = $"CREATE UNIQUE INDEX IX_{tabela}_{coluna} ON {endereco} ({coluna}) WHERE {coluna} IS NOT NULL";

			Execute.Sql(sql);
		}
	}
}
