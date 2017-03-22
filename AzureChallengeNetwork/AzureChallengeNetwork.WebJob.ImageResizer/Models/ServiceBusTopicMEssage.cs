using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureChallengeNetwork.WebJob.ImageResizer.Models
{
    public class ServiceBusTopicMessage
    {
        public string ImageId { get; set; }
    }
}
