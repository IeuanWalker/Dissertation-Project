using System;
using System.ComponentModel.DataAnnotations;

namespace SemanticWebNPLSearchEngine.Models
{
    public class MovieUserSearch
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string SearchedFor { get; set; }

        public string MovieLink { get; set; }
        public string Title { get; set; }
        public string GenreLink { get; set; }
        public string Genre { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}