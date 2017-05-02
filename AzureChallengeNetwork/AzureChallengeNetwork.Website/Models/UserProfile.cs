using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureChallengeNetwork.Website.Models
{
    public class UserProfile
    {
        public string DisplayName { get; set; }

        public string GivenName { get; set; }

        public string Surname { get; set; }

        public Guid ObjectId { get; set; }
    }
}