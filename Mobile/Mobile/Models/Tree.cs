using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Mobile.Models
{
    [Table("Trees")]
    public class Tree
    {
        [PrimaryKey]
        public long Id { get; set; }

        public string Photo { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey(typeof(User))]
        public long UserId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert | CascadeOperation.CascadeRead)]
        public User User { get; set; }
    }
}
