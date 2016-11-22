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
            string mask = "";
            Random rand = new Random();
            User currentUser;
            TimeSpan time = TimeSpan.FromSeconds(30.0);

            if (login == null)
            {
                ModelState.AddModelError("error", "login nie może być pusty"); return View();
            }
            

            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (ctx.Users.Any(u => u.Name == login))
                {
                }
                else
                {
                    ctx.Users.Add(new User() { Name = login, Password = "", MaxFailedAttempts = 3, FailedAttempts = 0, Activated = false });
                    ctx.SaveChanges();
                }
                currentUser = ctx.Users.Single(u => u.Name == login);



                if (TempData["error"] as string != null)
                {
                    currentUser.FailedAttempts++;
                    currentUser.FailedLogin = DateTime.Now;
                    ctx.Entry(currentUser).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();                    
                                       
                    ModelState.AddModelError("attempts", "pozostałe próby logowania: " + (currentUser.MaxFailedAttempts - currentUser.FailedAttempts));
                    ModelState.AddModelError("error", TempData["error"] as string);
                }

                if (currentUser.MaxFailedAttempts - currentUser.FailedAttempts <= 0)
                {
                    ModelState.AddModelError("blocked", "zbyt wiele nieudanych prób, zapraszamay do administratora");
                }

                TimeSpan? delay = time - (DateTime.Now - currentUser.FailedLogin);
                if (delay > TimeSpan.FromSeconds(0.0))
                {
                    ModelState.AddModelError("delay", "poczekaj trochę: " + ((TimeSpan)delay).ToString(@"mm\:ss"));
                    return View();
                }
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

        public ActionResult Login(string login = "")
        {
            if (login == "")
                return RedirectToAction("index");

            NameValueCollection collection = Request.QueryString;
            string[] values = collection.GetValues("p1");

            Dictionary<int, char> signs = new Dictionary<int, char>();
            int number;
            foreach (var key in collection.AllKeys)
            {
                if (key.StartsWith("p") && Int32.TryParse(key.TrimStart('p'), out number))
                {
                    string value = collection.Get(key);
                    if(value.Length == 1)
                        signs.Add(number, value[0]);
                }
            }

            User currentUser = null;
            string mask = "";
            string hashPassword;
            using (BaiDbContext ctx = new BaiDbContext())
            {
                currentUser = ctx.Users.SingleOrDefault(cu => cu.Name == login);
                if (currentUser == null)
                {
                    TempData["error"] = "nieprawidłowe hasło";                    
                    return RedirectToAction("Index", new { login = login });
                }
                mask = currentUser.Masks.FirstOrDefault().MaskString;
                hashPassword = currentUser.Masks.FirstOrDefault().HashPassword;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == '1')
                {
                    if (signs.ContainsKey(i + 1))
                    {
                        sb.Append((i + 1) + signs[i + 1].ToString() );
                    }
                    else
                    {
                        TempData["error"] = "nieprawidłowe hasło";
                        return RedirectToAction("Index", new { login = login });
                    }
                }
            }

            bool equals = Utils.getHashSha256(sb.ToString()) == hashPassword;

            if (!equals)
            {
                TempData["error"] = "nieprawidłowe hasło";
                return RedirectToAction("Index", new { login = login });
            }

            using (BaiDbContext ctx = new BaiDbContext())
            {
                string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                currentUser.LoginToken = token;                

                currentUser.SuccessfulLogin = DateTime.Now;
                ctx.Entry<User>(currentUser).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
                ctx.Masks.Remove(currentUser.Masks.FirstOrDefault());
                ctx.SaveChanges();
                Response.Cookies.Add(new HttpCookie("token", token));
                Response.Cookies.Add(new HttpCookie("username", login));
                
            }


            return RedirectToAction("Info", "login");
        }
    }
}