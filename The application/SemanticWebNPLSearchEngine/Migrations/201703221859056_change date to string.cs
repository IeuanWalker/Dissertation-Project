namespace SemanticWebNPLSearchEngine.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class changedatetostring : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.MovieUserSearches", "ReleaseDate", c => c.String());
        }

        public override void Down()
        {
            AlterColumn("dbo.MovieUserSearches", "ReleaseDate", c => c.DateTime(nullable: false));
        }
    }
}