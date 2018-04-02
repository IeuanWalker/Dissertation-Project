using System.Data.Entity.Migrations;

namespace DissertationOriginal.Migrations
{
    public partial class Changedatetostring : DbMigration
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