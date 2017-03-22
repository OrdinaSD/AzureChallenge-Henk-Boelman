using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AzureChallengeNetwork.Website.Entities;

namespace AzureChallengeNetwork.Website.Models
{
    public class PostModel
    {
        public PostModel()
        {
            ImagePosts = new List<ImagePost>();
        }

        public List<ImagePost> ImagePosts { get; set; }

        public Userpost UserPost { get; set; }

    }
}