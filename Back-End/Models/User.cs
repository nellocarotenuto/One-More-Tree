using System;
using System.Collections.Generic;

namespace Back_End.Models
{
    public class User
    {
        public long ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public string Email { get; set; }
        public string FacebookID { get; set; }

        public List<Tree> Trees { get; set; }
        public HashSet<User> Friends { get; set; }
    }
}
