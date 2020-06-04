using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using BackEnd.Validators;

namespace BackEnd.Models
{
    public class TreePutRequestDTO
    {
        public long Id { get; set; }

        [TreeDescription]
        public string Description { get; set; }
    }
}
