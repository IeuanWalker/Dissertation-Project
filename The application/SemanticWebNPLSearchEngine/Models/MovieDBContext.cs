using System.Data.Entity;

namespace DissertationOriginal.Models
{
    public class MovieDbContext : DbContext
    {
        public DbSet<MovieUserSearch> MovieUserSearchTable { get; set; }
    }
}