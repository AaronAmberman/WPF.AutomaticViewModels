using System;
using System.Collections.Generic;

namespace Testing
{
    public class AgeGroups
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Tuple<int, List<User>>> Users { get; set; } = new List<Tuple<int,List<User>>>();
    }
}
