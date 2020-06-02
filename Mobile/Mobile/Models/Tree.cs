using System;
using Plugin.Media.Abstractions;
using SQLite;
using SQLiteNetExtensions.Attributes;
using Xamarin.Forms.Maps;

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

        [Ignore]
        public Position Position { get => new Position(Latitude, Longitude); }

        [Ignore]
        public string Location { get => string.Format("{0}, {1}", City, State); }

        [Ignore]
        public bool HasDescription { get => Description != null && Description != string.Empty; }
    }
}
