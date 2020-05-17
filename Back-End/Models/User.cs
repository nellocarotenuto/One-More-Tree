using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Back_End.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Picture { get; set; }

        [Required]
        public string Email { get; set; }

        public string FacebookId { get; set; }

        public virtual ICollection<Tree> Trees { get; set; }
    }
}
