using System;
using System.Collections.Generic;


namespace BAIproject1.Models
{
    public class User
    {        
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }        
        public DateTime? SuccessfulLogin { get; set; }
        public DateTime? FailedLogin { get; set; }
        public bool Activated { get; set; }
        public int FailedAttempts { get; set; }
        public int MaxFailedAttempts { get; set; }
        public string LoginToken { get; set; }
        //public string Mask { get; set; }
        public virtual List<Mask> Masks { get; set; }
        public virtual List<Message> Messages { get; set; }
    }
}