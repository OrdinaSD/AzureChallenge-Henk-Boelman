using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using AzureChallengeNetwork.WebJob.ImageResizer.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureChallengeNetwork.WebJob.ImageResizer
{
    public class Functions
    {
        private const string TopicName = "social";
        private const string SubscriptionName = "ImageResizer";

        public static void ProcessQueueMessage([ServiceBusTrigger(TopicName, SubscriptionName)] BrokeredMessage message,
            TextWriter logger)
        {
            var storageConnectionString = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;

            string topicMessage = message.GetBody<string>();

            string partitionKey = topicMessage.Split('|')[0];
            string rowKey = topicMessage.Split('|')[1];

            // Connection to Table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("images");
            table.CreateIfNotExists();

            // Get the row by rowkey and partitionkey
            TableOperation retrieveOperation = TableOperation.Retrieve<ImagePost>(partitionKey, rowKey);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            ImagePost imagePost = (ImagePost) retrievedResult.Result;

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("images");
           
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(imagePost.Filename);
            CloudBlockBlob blockBlobOut = container.GetBlockBlobReference("thumb_" + imagePost.Filename);

            // Open the blob, Scale it, Write it back.
            using (var inputStream = new MemoryStream())
            {
                blockBlob.DownloadToStream(inputStream);
                using (Stream output = blockBlobOut.OpenWrite())
                {
                    ConvertImageToThumbnailJpg(inputStream, output);
                    blockBlobOut.Properties.ContentType = "image/jpeg";
                }
            }

            // Update the Azure Table
            imagePost.HasThumbnail = true;
            imagePost.ThumbnailFilename = "thumb_" + imagePost.Filename;

            TableOperation replaceOperation = TableOperation.Replace(imagePost);
            table.Execute(replaceOperation);

            logger.WriteLine($"partitionKey: {partitionKey} | rowKey: {rowKey}  | {imagePost.Filename}");
        }

        public static void ConvertImageToThumbnailJpg(Stream input, Stream output)
        {
            int thumbnailsize = 80;
            int width;
            int height;
            var originalImage = new Bitmap(input);

            if (originalImage.Width > originalImage.Height)
            {
                width = thumbnailsize;
                height = thumbnailsize * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = thumbnailsize;
                width = thumbnailsize * originalImage.Width / originalImage.Height;
            }

            Bitmap thumbnailImage = null;
            try
            {
                thumbnailImage = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(originalImage, 0, 0, width, height);
                }

                thumbnailImage.Save(output, ImageFormat.Jpeg);
            }
            finally
            {
                if (thumbnailImage != null)
                {
                    thumbnailImage.Dispose();
                }
            }
        }
    }

}
