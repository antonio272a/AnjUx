using FluentMigrator;

namespace AnjUx.Migrator.Migrations
{
    [Migration(202405261252)]
    public class TarefasEFonte : BaseMigration
    {
        public override void Up()
        {
            Alter.Table("MunicipioDados")
                .AddColumn("Fonte").AsString().Nullable();

            Create.Table("Tarefas")
                .CreateBaseFields()
                .WithColumn("Descricao").AsString().Nullable()
                .WithColumn("Status").AsInt32().Nullable().Indexed()
                .WithColumn("Finalizada").AsDateTime().Nullable().Indexed()
                .WithColumn("Erro").AsString().Nullable();
        }
        
        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
