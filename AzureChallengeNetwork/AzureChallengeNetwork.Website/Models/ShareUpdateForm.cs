using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AzureChallengeNetwork.Website.Models
{
    public class ShareUpdateForm
    {
        [Required]
        public string Message { get; set; }
    }
}