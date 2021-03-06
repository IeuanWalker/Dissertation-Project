﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DissertationOriginal.Models
{
    public class MovieUserSearch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SearchedFor { get; set; }

        public string MovieLink { get; set; }
        public string Title { get; set; }
        public string GenreLink { get; set; }
        public string Genre { get; set; }
        public string ReleaseDate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}