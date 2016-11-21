using BAIproject1.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BAIproject1.Controllers
{
    public class MaskLoginController : Controller
    {
        // GET: MaskLogin
        public ActionResult Index(string login)
        {
            bool exists = false;
            string mask = "";
            Random rand = new Random();

            if (login == null)
            {
                ModelState.AddModelError("error", "login nie może być pusty"); return View();
            }

            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (ctx.Users.Any(u => u.Name == login))
                {
                    exists = true;
                }
                else
                {
                    ctx.Users.Add(new User() { Name = login, Password = "", MaxFailedAttempts = 3, FailedAttempts = 1, FailedLogin = DateTime.Now });
                    ctx.SaveChanges();
                }
                User currentUser = ctx.Users.Single(u => u.Name == login);



                if (currentUser.Masks == null || currentUser.Masks.Count == 0)
                {
                    for (int k = 0; k < 15; k++)
                    {
                        int lengthMask = rand.Next(5, 8);
                        int lengthPasswd = 0;
                        if (currentUser.Activated)
                        {
                            lengthPasswd = currentUser.Password.Length;
                            if (lengthMask > lengthPasswd / 2) lengthMask = lengthPasswd / 2;
                            if (lengthMask < 5) lengthMask = 5;
                        }
                        else
                            lengthPasswd = rand.Next(8, 16);

                        List<int> rest = new List<int>();
                        for (int i = 0; i < lengthPasswd; i++)
                        {
                            rest.Add(i);
                        }

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < lengthPasswd; i++)
                        {
                            sb.Append('0');
                        }
                        for (int i = 0; i < lengthMask; i++)
                        {
                            int r = rand.Next(rest.Count);
                            sb[rest[r]] = '1';
                            rest.RemoveAt(r);
                        }
                        if (currentUser.Masks == null)
                            currentUser.Masks = new List<Mask>();
                        currentUser.Masks.Add(new Mask() { MaskString = sb.ToString() });
                        ctx.Entry(currentUser).State = System.Data.Entity.EntityState.Modified;
                        ctx.SaveChanges();
                    }
                }
                mask = currentUser.Masks.First().MaskString;
            }

            var PasswordMask = new List<bool>(16);

            for (int i = 0; i < 16; i++)
            {
                PasswordMask.Add((i < mask.Length && mask[i] == '1'));
            }

            ViewBag.PasswordMask = PasswordMask;

            return View();
        }

        public ActionResult Mask()
        {




            return View();
        }

        public ActionResult Login()
        {
            NameValueCollection collection = Request.QueryString;
            string[] values = collection.GetValues("p1");

            Dictionary<int, char> signs = new Dictionary<int, char>();
            int number;
            foreach (var key in collection.AllKeys)
            {
                if (key.StartsWith("p") && Int32.TryParse(key.TrimStart('p'), out number))
                {
                    string a = collection.Get(key);
                    signs.Add(number, collection.Get(key)[0]);
                }
            }

            return View();
        }
    }
}