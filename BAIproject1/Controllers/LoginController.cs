using BAIproject1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult Settings()
        {
            return View();
        }
    }
}