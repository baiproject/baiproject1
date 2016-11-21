using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BAIproject1.Models
{
    public class Mask
    {
        public int id { get; set; }
        public int UserId { get; set; }
        public string MaskString { get; set; }
        public string HashPassword { get; set; }
    }
}