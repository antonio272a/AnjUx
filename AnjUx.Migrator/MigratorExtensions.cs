using AnjUx.ORM.Interfaces;
using AnjUx.Shared.Interfaces;
using FluentMigrator.Builders.Create.Table;

namespace AnjUx.Migrator
{
	internal static class MigratorExtensions
	{
		/// <summary>
		/// Cria os campos "ID", "Inserted" e "InsertUser".
		/// </summary>
		/// <param name="ictwcs"></param>
		/// <returns></returns>
		internal static ICreateTableColumnOptionOrWithColumnSyntax CreateDBFields(this ICreateTableWithColumnSyntax ictwcs)
		{
			ictwcs
				.WithColumn(nameof(IDbModel.ID)).AsInt64().PrimaryKey().Identity()
				.WithColumn(nameof(IBaseModel.Inserted)).AsDateTime().Indexed().Nullable()
				.WithColumn(nameof(IBaseModel.InsertUser)).AsString(64).Indexed().Nullable();

			return (ICreateTableColumnOptionOrWithColumnSyntax)ictwcs;
		}

		/// <summary>
		/// Cria campos "Updated" e "UpdateUser" em adição aos DBFields.
		/// </summary>
		/// <param name="ictwcs"></param>
		/// <param name="activeFields"></param>
		/// <returns></returns>
		internal static ICreateTableColumnOptionOrWithColumnSyntax CreateBaseFields(this ICreateTableWithColumnSyntax ictwcs)
		{
			// Adiciona os campos do BaseModel
			ictwcs
				.CreateDBFields()
				.WithColumn(nameof(IBaseModel.Updated)).AsDateTime().Indexed().Nullable()
				.WithColumn(nameof(IBaseModel.UpdateUser)).AsString(64).Indexed().Nullable();

			return (ICreateTableColumnOptionOrWithColumnSyntax)ictwcs;
		}

		/// <summary>
		/// Cria campo "Ativo" em adição aos BaseFields.
		/// </summary>
		/// <param name="ictwcs"></param>
		/// <returns></returns>
		internal static ICreateTableColumnOptionOrWithColumnSyntax CreateActiveField(this ICreateTableWithColumnSyntax ictwcs)
		{
			ictwcs
				.CreateBaseFields()
				.WithColumn(nameof(IActiveModel.Active)).AsBoolean().Indexed().Nullable();

			return (ICreateTableColumnOptionOrWithColumnSyntax)ictwcs;
		}
	}
}
