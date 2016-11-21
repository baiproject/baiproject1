using BAIproject1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BAIproject1.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [AllowAnonymous]
        public ActionResult Index(string login, string password)
        {
            TimeSpan time = TimeSpan.FromSeconds(30.0);
            if (login == null || password == null)
            {
                ModelState.AddModelError("error", "login i hasło nie mogą być puste"); return View();
            }
            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (ctx.Users.Any(u => u.Name == login))
                {
                    User user = ctx.Users.Single(u => u.Name == login);
                    if (DateTime.Now - user.FailedLogin > time || user.FailedLogin == null)
                    {
                        if (user.Activated)
                        {
                            if (user.Password == password)
                            {
                                string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                                user.LoginToken = token;
                                ctx.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                                ctx.SaveChanges();
                                Response.Cookies.Add(new HttpCookie("token", token));
                                Response.Cookies.Add(new HttpCookie("username", login));
                                return RedirectToAction("Info", new { username = login });
                            }
                            else
                            {
                                user.FailedLogin = DateTime.Now;
                                user.FailedAttempts++;
                                ctx.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                                ctx.SaveChanges();
                                ModelState.AddModelError("attempts", "pozostałe próby logowania: " + (user.MaxFailedAttempts - user.FailedAttempts));
                            }
                        }
                        else
                        {
                            if (user.MaxFailedAttempts - user.FailedAttempts <= 0)
                            {
                                ModelState.AddModelError("blocked", "zbyt wiele nieudanych prób, zapraszamay do administratora");
                            }
                            else
                            {
                                user.FailedLogin = DateTime.Now;
                                user.FailedAttempts++;
                                ctx.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                                ctx.SaveChanges();
                                ModelState.AddModelError("attempts", "pozostałe próby logowania: " + (user.MaxFailedAttempts - user.FailedAttempts));

                            }
                        }
                    }
                    else
                    {
                        TimeSpan? delay = time - (DateTime.Now - user.FailedLogin);
                        ModelState.AddModelError("delay", "poczekaj trochę: " + ((TimeSpan)delay).ToString(@"mm\:ss"));
                    }
                }
                else
                {
                    User user = new User()
                    {
                        Name = login,
                        Activated = false,
                        FailedAttempts = 1,
                        SuccessfulLogin = null,
                        FailedLogin = DateTime.Now,
                        MaxFailedAttempts = 3,
                        LoginToken = ""
                    };
                    ctx.Users.Add(user);
                    ctx.SaveChanges();
                    ModelState.AddModelError("attempts", "pozostałe próby logowania: " + (user.MaxFailedAttempts - user.FailedAttempts));
                }
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult Info()
        {
            User user = null;

            if (!Request.Cookies.AllKeys.Contains("token") || !Request.Cookies.AllKeys.Contains("username"))
                return RedirectToAction("index");

            string token = Request.Cookies.Get("token").Value;
            string username = Request.Cookies.Get("username").Value;
            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (!ctx.Users.Any(u => u.Name == username && u.LoginToken == token))
                {
                    return RedirectToAction("index");
                }
                user = ctx.Users.Single(u => u.Name == username);
                ViewBag.SuccessfulLogin = user.SuccessfulLogin;
                ViewBag.FailedAttempts = user.FailedAttempts;
                user.SuccessfulLogin = DateTime.Now;
                user.FailedAttempts = 0;
                ctx.Entry(user).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
            }
            ViewBag.LastSuccessfulLogon = user.SuccessfulLogin;
            return View(user);
        }
        public ActionResult Register(string login = "", string password = "")
        {
            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (login == "" || password == "")
                {
                    ModelState.AddModelError("error", "login i hasło nie mogą być puste");
                    return View();
                }
                if (password.Length < 8)
                {
                    ModelState.AddModelError("error", "hasło musi mieć długość co najmniej 8 znaków");
                    return View();
                }
                if (ctx.Users.Any(u => u.Name.ToLower() == login.ToLower()))
                {
                    ModelState.AddModelError("exists", "użytkownik o podanym loginie istnieje");
                    return View();
                }
                Random rand = new Random();
                User currentUser = new User() { Name = login, Activated = true, Password = password, MaxFailedAttempts = 3 };
                ctx.Entry(currentUser).State = System.Data.Entity.EntityState.Added;
                ctx.SaveChanges();

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
                    string mask = sb.ToString();
                    StringBuilder passwdHash = new StringBuilder();
                    for (int i = 0; i < lengthPasswd; i++)
                    {
                        if(mask[i] == '1')
                        {
                            passwdHash.Append(i + password[i].ToString());
                        }
                    }
                    if (currentUser.Masks == null)
                        currentUser.Masks = new List<Mask>();
                    currentUser.Masks.Add(new Mask() { MaskString = mask, HashPassword = Utils.getHashSha256(passwdHash.ToString()) });
                }
                ctx.Entry(currentUser).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();

                ModelState.AddModelError("created", string.Format("Gratulacje użytkownik {0} został utworzony",currentUser.Name));
            }

            
            return View();
        }

        
    }
}