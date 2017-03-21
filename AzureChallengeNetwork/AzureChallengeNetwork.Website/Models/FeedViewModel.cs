using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AzureChallengeNetwork.Website.Entities;

namespace AzureChallengeNetwork.Website.Models
{
    public class FeedViewModel
    {

        public FeedViewModel()
        {
            UserPosts = new List<Userpost>();
            ShareUpdateForm = new ShareUpdateForm();
        }

        public ShareUpdateForm ShareUpdateForm { get; internal set; }
        public List<Userpost> UserPosts { get; set; }
    }
}