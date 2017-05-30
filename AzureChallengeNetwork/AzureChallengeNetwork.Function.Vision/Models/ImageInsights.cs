namespace AzureChallengeNetwork.Function.Vision.Models
{
    public class ImageInsights
    {
        public string ImageId { get; set; }

        public string Caption { get; set; }

        public string[] Tags { get; set; }
    }
}
