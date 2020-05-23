using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Front_End_Mobile.Models
{
    [Table("Users")]
    public class User
    {
        [PrimaryKey]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Picture { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Tree> Tree { get; set; }
    }
}
