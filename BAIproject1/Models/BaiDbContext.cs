using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BAIproject1.Models
{
    public class BaiDbContext:DbContext
    {
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AllowedMessages> AllowedMessages { get; set; }
    }
}