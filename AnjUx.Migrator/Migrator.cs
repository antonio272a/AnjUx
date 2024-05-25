using AnjUx.Migrator.Migrations;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace AnjUx.Migrator
{
	public class Migrator
	{
		private readonly string DefaultConnectionString;
		private readonly string DbName;

		public Migrator(string defaultConnectionString, string dbName)
		{
			DefaultConnectionString = defaultConnectionString;
			DbName = dbName;
		}

		public void Executar()
		{
			// Verifica se existe uma base com o nome
			// informado e a cria caso não exista.
				CriarBaseDeDadosSeNaoExistir();

			// Cria os serviços e atualiza a base de dados
			var serviceProvider = CriarServicos();

			using (var scope = serviceProvider.CreateScope())
				AtualizarBaseDeDados(scope.ServiceProvider);
		}

		private void CriarBaseDeDadosSeNaoExistir()
		{
			var connectionString = DefaultConnectionString.Replace($"Initial Catalog = {DbName}", "Initial Catalog = master");

			using (var conexao = new SqlConnection(connectionString))
			{
				conexao.Open();

				try
				{
					var registros = conexao.Query($"SELECT * FROM sys.databases WHERE name = '{DbName}'");

					if (!registros.Any())
						conexao.Execute($"CREATE DATABASE {DbName}");
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					conexao.Close();
				}
			}
		}

		private IServiceProvider CriarServicos()
		{
			return new ServiceCollection()
				.AddFluentMigratorCore()
				.ConfigureRunner(c => c
					.AddSqlServer2012()
					.WithGlobalConnectionString(DefaultConnectionString)
					.ScanIn(typeof(FirstMigration).Assembly)
					.For.Migrations()
				)
				.AddLogging(c => c
					.AddFluentMigratorConsole()
				)
				.BuildServiceProvider(false);
		}

		private void AtualizarBaseDeDados(IServiceProvider serviceProvider)
		{
			var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

			runner.ListMigrations();
			runner.MigrateUp();
		}
	}
}
