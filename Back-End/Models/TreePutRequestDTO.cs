using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Back_End.Validators;

namespace Back_End.Models
{
    public class TreePutRequestDTO
    {
        public long Id { get; set; }

        [StringLength(1024, ErrorMessage = "The description is too long.")]
        [TreeDescription]
        public string Description { get; set; }
    }
}
