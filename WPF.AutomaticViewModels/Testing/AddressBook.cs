using System;
using System.Collections.Generic;

namespace Testing
{
    public class AddressBook
    {
        public DateTime CreatedDate { get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; } = new List<User>();
        public List<int> RandomNumbersBecause { get; set; } = new List<int>();
    }
}
