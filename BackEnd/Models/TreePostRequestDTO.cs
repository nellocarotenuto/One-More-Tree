using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using BackEnd.Validators;

namespace BackEnd.Models
{
    public class TreePostRequestDTO
    {
        [Required]
        [TreePhoto]
        public IFormFile Photo { get; set; }

        [TreeDescription]
        public string Description { get; set; }

        [Required]
        [TreeLocation(nameof(City), nameof(State))]
        public string Coordinates { get; set; }

        public string City { get; set; }
        public string State { get; set; }
    }
}
