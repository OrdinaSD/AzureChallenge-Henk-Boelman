using System.ComponentModel.DataAnnotations;
using System.Web;

namespace AzureChallengeNetwork.Website.Models
{
    public class ShareUpdateForm
    {
        [Required]
        public string Message { get; set; }

        public HttpPostedFileBase Image { get; set; }

    }
}