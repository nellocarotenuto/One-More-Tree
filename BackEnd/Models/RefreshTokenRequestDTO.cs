using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class RefreshTokenRequestDTO
    {
        public string Token { get; set; }
        public string Provider { get; set; }
    }
}
