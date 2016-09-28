using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BAIproject1.Models
{
    public class AllowedMessages
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MessageId { get; set; }
    }
}