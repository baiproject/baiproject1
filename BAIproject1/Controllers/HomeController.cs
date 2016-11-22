using BAIproject1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BAIproject1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int editId = -1)
        {
            List<Message> messages = null;
            List<User> users = null;
            List<string> mod = new List<string>();
            List<string> mod2 = new List<string>();
            User user = null;
            List<AllowedMessages> am = null;

            string username = "";
            string token = "";
            try
            {
                username = Request.Cookies["username"].Value;
                token = Request.Cookies["token"].Value;
            }
            catch (Exception)
            {
            }
            using (BaiDbContext ctx = new BaiDbContext())
            {
                try
                {
                    user = ctx.Users.Single(u => u.Name == username && u.LoginToken == token);
                }catch (Exception)
                {                                        
                }
                am = ctx.AllowedMessages.ToList();
                if(user != null)
                    users = ctx.Users.Where(u => u.Activated == true && u.Id != user.Id).ToList();
                messages = ctx.Messages.Include("user").ToList();
            }
            List<SelectListItem> selectList = new List<SelectListItem>();
            if (users != null)
            {
                foreach (var item in users)
                {
                    selectList.Add(new SelectListItem() { Text = item.Name, Value = item.Id + "" });
                }
            }
            ViewBag.users = selectList;
            if (editId != -1)
            {
                ViewBag.textEdit = messages.Single(m => m.Id == editId).Text;
                ViewBag.editId = editId;
            }
            foreach (var m in messages)
            {
                if (user == null)
                {
                    mod.Add("hidden=\"hidden\"");
                    mod2.Add("hidden=\"hidden\"");
                }
                else if (user.Id == m.UserId)
                {
                    mod.Add("");
                    mod2.Add("");
                }
                else
                {
                    mod.Add("hidden=\"hidden\"");
                    if(am.Any(a => a.MessageId == m.Id && a.UserId==user.Id))
                        mod2.Add("");
                    else
                        mod2.Add("hidden=\"hidden\"");
                }
            }
            ViewBag.mod = mod;
            ViewBag.mod2 = mod2;
            return View(messages);
        }

        
        public ActionResult Create(string text)
        {
            string username = "";
            string token = "";
            try
            {
                username = Request.Cookies["username"].Value;
                token = Request.Cookies["token"].Value;
            }
            catch (Exception)
            {
            }            
            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (ctx.Users.Any<User>(u => u.Name == username && u.LoginToken == token))
                {
                    User user = ctx.Users.Single(u => u.Name == username);
                    user.Messages.Add(new Message() { Text = text });
                    ctx.Entry<User>(user).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
                return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string login, string password)
        {
            using (BaiDbContext ctx = new BaiDbContext()) {
                if (ctx.Users.Any<User>(u => u.Name == login && u.Password == password))
                {
                    HttpCookie cookie = new HttpCookie("username",login);
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("index");
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (Request.Cookies["username"] == null || Request.Cookies["token"] == null)
                return RedirectToAction("index");
            string username = Request.Cookies["username"].Value;
            string token = Request.Cookies["token"].Value;
            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (!ctx.Users.Any(u => u.Name == username && u.LoginToken == token))
                    return RedirectToAction("index");
                User user = ctx.Users.Single(u => u.Name == username);
                Message message = ctx.Messages.Include("user").Where(m => m.Id == id).First();

                if (message.User.Name == username)
                {
                    ctx.Entry(message).State = System.Data.Entity.EntityState.Deleted;
                    ctx.SaveChanges();
                }
            }
                return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Permission(string permission, int messageId = -1, int userId = -1)
        {
            
            if (messageId == -1 || userId == -1)
            {
                return RedirectToAction("index");
            }

            string username = "";
            string token = "";
            try
            {
                username = Request.Cookies["username"].Value;
                token = Request.Cookies["token"].Value;
            }
            catch (Exception)
            {
            }

            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (!ctx.Users.Any(u => u.Name == username && u.LoginToken == token))
                    return RedirectToAction("index");
                int ownerId = ctx.Users.Single(u => u.Name == username).Id;
                if(!ctx.Messages.Any(m => m.UserId == ownerId && m.Id == messageId))
                {
                    return RedirectToAction("index");
                }
                
                
                if (permission == "grant")
                {
                    ctx.AllowedMessages.Add(new AllowedMessages { MessageId = messageId, UserId = userId});
                    ctx.SaveChanges();
                }
                else if (permission == "deny")
                {
                    AllowedMessages am = null;
                    try
                    {
                        am = ctx.AllowedMessages.Single(u => u.UserId == userId && u.MessageId == messageId);
                    }
                    catch (Exception)
                    {
                        
                    }
                    
                    if(am == null)
                        return RedirectToAction("Index");

                    ctx.Entry(am).State = System.Data.Entity.EntityState.Deleted;
                    ctx.SaveChanges();
                }
            }            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(string text, int id = -1)
        {
            if(id == -1)
                return RedirectToAction("index");
            if (Request.Cookies["username"] == null || Request.Cookies["token"] == null)
                return RedirectToAction("index");
            string username = Request.Cookies["username"].Value;
            string token = Request.Cookies["token"].Value;
            using (BaiDbContext ctx = new BaiDbContext())
            {
                if (!ctx.Users.Any(u => u.Name == username && u.LoginToken == token))
                    return RedirectToAction("index");
                User user = ctx.Users.Single(u => u.Name == username);
                Message message = ctx.Messages.Include("user").Where(m => m.Id == id).First();
                message.Text = text;

                if (message.User.Name == username ||
                    ctx.AllowedMessages.Any(m => m.MessageId == id && m.UserId == user.Id))
                {
                    ctx.Entry(message).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }
            }
            return RedirectToAction("index");
        }
        public ActionResult Logout()
        {
            if (Request.Cookies["username"] != null)
            {
                Response.Cookies["username"].Expires = DateTime.Now.AddDays(-1);
            }
            if (Request.Cookies["token"] != null)
            {
                Response.Cookies["token"].Expires = DateTime.Now.AddDays(-1);
            }
            return RedirectToAction("index");
        }

    }
}