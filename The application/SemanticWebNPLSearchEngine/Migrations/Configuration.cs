using System.Data.Entity.Migrations;
using DissertationOriginal.Models;

namespace DissertationOriginal.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MovieDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "DissertationOriginal.Models.MovieDBContext";
        }

        protected override void Seed(MovieDbContext context)
        {
            // This method will be called after migrating to the latest version.

            // You can use the DbSet<T>.AddOrUpdate() helper extension method to avoid creating
            // duplicate seed data. E.g.
            //
            // context.People.AddOrUpdate( p => p.FullName, new Person { FullName = "Andrew Peters"
            // }, new Person { FullName = "Brice Lambson" }, new Person { FullName = "Rowan Miller" } );
        }
    }
}