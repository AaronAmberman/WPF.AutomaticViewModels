using System.Collections.Generic;

namespace Testing
{
    public class UserGroup
    {
        public int Id { get; set; }
        public Dictionary<string, User> Users { get; set; } = new Dictionary<string, User>();
    }
}
