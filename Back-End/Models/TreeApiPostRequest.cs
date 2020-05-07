using Back_End.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Back_End.Models
{
    public class TreeApiPostRequest
    {
        [Required]
        [TreePhoto]
        public IFormFile Photo { get; set; }

        [StringLength(1024, ErrorMessage = "The description is too long.")]
        [TreeDescription]
        public string Description { get; set; }

        [Required]
        [TreeLocation(nameof(City), nameof(State))]
        public string Coordinates { get; set; }

        public string City { get; set; }
        public string State { get; set; }
    }
}
