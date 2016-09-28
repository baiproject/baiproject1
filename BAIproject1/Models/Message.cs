using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BAIproject1.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }        
        List<int> PermitedUserIdList { get; set;}

        public int UserId { get; set; }
        public User User { get; set; }
    }
}