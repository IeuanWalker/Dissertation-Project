using System.Data.Entity;

namespace SemanticWebNPLSearchEngine.Models
{
    public class MovieDbContext : DbContext
    {
        public DbSet<MovieUserSearch> MovieUserSearchTable { get; set; }
    }
}