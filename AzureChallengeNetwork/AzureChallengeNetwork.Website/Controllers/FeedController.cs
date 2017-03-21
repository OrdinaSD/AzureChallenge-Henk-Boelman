using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AzureChallengeNetwork.Website.Context;
using AzureChallengeNetwork.Website.Entities;
using AzureChallengeNetwork.Website.Models;

namespace AzureChallengeNetwork.Website.Controllers
{
    public class FeedController : Controller
    {
        public ActionResult Index()
        {
            var viewModel = new FeedViewModel();
       
            // Loading the last 100 posts
            using (var db = new AzureChallengeContext())
            {
                viewModel.UserPosts =  db.Userposts.OrderByDescending(a => a.CreationDateTime).Take(100).ToList();
            }

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Save(ShareUpdateForm shareUpdateForm)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            
            using (var db = new AzureChallengeContext())
            {
                var userPost = new Userpost
                {
                    Text = shareUpdateForm.Message,
                    CreationDateTime = DateTime.Now
                };

                db.Userposts.Add(userPost);
                db.SaveChanges();
            }
            

            return RedirectToAction("Index");
        }
    }
}