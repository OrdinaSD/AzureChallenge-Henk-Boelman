using System.Collections.Generic;
using AzureChallengeNetwork.Website.Entities;

namespace AzureChallengeNetwork.Website.Models
{
    public class FeedViewModel
    {

        public FeedViewModel()
        {
            ShareUpdateForm = new ShareUpdateForm();
            Posts = new List<PostModel>();
        }

        public ShareUpdateForm ShareUpdateForm { get; internal set; }

        public List<PostModel> Posts { get; internal set; }
        public List<UserProfile> Friends { get; set; }
    }
}