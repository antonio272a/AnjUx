using AnjUx.Shared.Models.Data;
using FluentMigrator;

namespace AnjUx.Migrator.Migrations
{
	[Migration(202405251145)]
	internal class FirstMigration : BaseMigration
	{
		public override void Up()
		{
			Create.Table("Municipios")
				.CreateBaseFields()
				.WithColumn(nameof(Municipio.Nome)).AsString().Indexed().Nullable()
				.WithColumn(nameof(Municipio.UF)).AsString(2).Indexed().Nullable();

			Create.Table("MunicipioDados")
				.CreateBaseFields()
				.WithColumn(nameof(MunicipioDado.Municipio)).AsInt64().ForeignKey("FK_MunicipioDados_Municipios", "Municipios", "ID").Indexed().Nullable()
				.WithColumn(nameof(MunicipioDado.TipoDado)).AsInt32().Indexed().Nullable()
				.WithColumn(nameof(MunicipioDado.Valor)).AsDecimal().Indexed().Nullable()
				.WithColumn(nameof(MunicipioDado.DataBase)).AsDateTime().Indexed().Nullable()
				.WithColumn(nameof(MunicipioDado.Ano)).AsInt32().Indexed().Nullable()
				.WithColumn(nameof(MunicipioDado.Mes)).AsInt32().Indexed().Nullable();
		}

		public override void Down()
		{
			throw new NotImplementedException();
		}
	}
}
