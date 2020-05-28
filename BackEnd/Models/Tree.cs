using System;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models
{
    public class Tree
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Photo { get; set; }

        public string Description { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public virtual User User { get; set; }
    }
}
