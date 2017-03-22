using Microsoft.WindowsAzure.Storage.Table;

namespace AzureChallengeNetwork.WebJob.ImageResizer.Models
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

        public bool HasThumbnail { get; set; }

        public string ThumbnailFilename { get; set; }
    }
}