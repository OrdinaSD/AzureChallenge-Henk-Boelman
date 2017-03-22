using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using AzureChallengeNetwork.Website.Context;
using AzureChallengeNetwork.Website.Entities;
using AzureChallengeNetwork.Website.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureChallengeNetwork.Website.Controllers
{
    public class FeedController : Controller
    {
        private string _storageConnectionString { get; set; }

        public FeedController()
        {
            _storageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
        }

        public ActionResult Index()
        {
            var viewModel = new FeedViewModel();

            

            // Connect to the Azure Table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("images");
            table.CreateIfNotExists();

            using (var db = new AzureChallengeContext())
            {
                // Loading the last 100 posts by creation date
                var posts =  db.Userposts.OrderByDescending(a => a.CreationDateTime).Take(100).ToList();
                foreach (var post in posts)
                {

                    var searchQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,post.Id.ToString());
                    TableQuery<ImagePost> exQuery = new TableQuery<ImagePost>().Where(searchQuery);

                    List<ImagePost> results = table.ExecuteQuery(exQuery).ToList();


                    var postModel = new PostModel
                    {
                        UserPost = post,
                        ImagePosts = results
                    };

                    viewModel.Posts.Add(postModel);

                }
            }

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult Save(ShareUpdateForm shareUpdateForm)
        {
            // If model is not valid just redirect it back
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            // Saving the data from the form
            Guid postId;
            using (var db = new AzureChallengeContext())
            {
                var userPost = new Userpost
                {
                    Text = shareUpdateForm.Message,
                    CreationDateTime = DateTime.Now
                };

                db.Userposts.Add(userPost);
                db.SaveChanges();

                postId = userPost.Id;
            }


            if (shareUpdateForm.Image != null)
            {
                var imageId = Guid.NewGuid();
                string imageFilename = $"{imageId}_{shareUpdateForm.Image.FileName}";

                // Create the table entity
                var imagePost = new ImagePost(postId.ToString(), imageId.ToString())
                {
                    Filename = imageFilename
                };

                // Save the Image to a Blob
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("images");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(imageFilename);
                blockBlob.UploadFromStream(shareUpdateForm.Image.InputStream);

                // Save the result in a Azure Table
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("images");
                table.CreateIfNotExists();
                TableOperation insertOperation = TableOperation.Insert(imagePost);
                TableResult result = table.Execute(insertOperation);

            }

            return RedirectToAction("Index");
        }

    }
}