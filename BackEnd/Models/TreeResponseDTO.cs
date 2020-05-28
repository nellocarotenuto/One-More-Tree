using System;

namespace BackEnd.Models
{
    public class TreeResponseDTO
    {
        public long Id { get; set; }

        public string Photo { get; set; }
        
        public string Description { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public DateTime Date { get; set; }

        public UserResponseDTO User { get; set; }

    }
}
