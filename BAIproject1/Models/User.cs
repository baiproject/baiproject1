using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BAIproject1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public virtual List<Message> Messages { get; set; }
    }
}