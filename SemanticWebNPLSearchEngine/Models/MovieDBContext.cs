using System.Data.Entity;

namespace SemanticWebNPLSearchEngine.Models
{
    public class MovieDBContext : DbContext
    {
        public DbSet<MovieUserSearch> movieUserSearchTable { get; set; }
    }
}