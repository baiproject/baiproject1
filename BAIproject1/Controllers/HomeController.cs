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
        public ActionResult Index()
        {
            List<Message> messages;
            using (BaiDbContext ctx = new BaiDbContext())
            {
                
                ctx.Users.Add(new User() { Name = "admin", Password = "admin", Messages = new List<Message>() { new Message() { Text = "nothing" } } });
                
                ctx.SaveChanges();
                messages = ctx.Messages.ToList();
            }
            return View(messages);
        }

        [HttpPost]
        public ActionResult Create(string text)
        {

            return RedirectToAction("index");
        }
    }
}