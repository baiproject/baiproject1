using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BAIproject1.Models
{
    public static class Utils
    {
        public static string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte x in hash)
            {
                sb.Append(String.Format("{0:x2}", x));
            }
            return sb.ToString();
        }
        
        public static bool IsLogged(string token, string username)
        {
            using (BaiDbContext ctx = new BaiDbContext())
            {
                return ctx.Users.Any(u => u.Name == username && u.LoginToken == token);
            }
        }        
    }
}