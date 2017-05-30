using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallengeNetwork.Shared.Models
{
    public class ImageInsights
    {
        public string ImageId { get; set; }

        public string Caption { get; set; }

        public string[] Tags { get; set; }
    }
}
