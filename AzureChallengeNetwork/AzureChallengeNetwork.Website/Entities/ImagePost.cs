using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureChallengeNetwork.Website.Entities
{
    public class ImagePost : TableEntity
    {

        public ImagePost(string postId, string imageId)
        {
            this.PartitionKey = postId;
            this.RowKey = imageId;
        }

        public ImagePost()
        {
        }

        public string Filename { get; set; }

    }
}