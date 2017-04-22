namespace SemanticWebNPLSearchEngine.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MovieUserSearches",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    SearchedFor = c.String(nullable: false),
                    MovieLink = c.String(),
                    Title = c.String(),
                    GenreLink = c.String(),
                    Genre = c.String(),
                    ReleaseDate = c.DateTime(nullable: false),
                    LastUpdated = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.ID);
        }

        public override void Down()
        {
            DropTable("dbo.MovieUserSearches");
        }
    }
}