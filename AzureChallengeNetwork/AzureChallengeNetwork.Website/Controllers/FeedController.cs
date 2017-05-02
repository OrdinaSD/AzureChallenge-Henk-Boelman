using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using AzureChallengeNetwork.Website.Context;
using AzureChallengeNetwork.Website.Entities;
using AzureChallengeNetwork.Website.Models;
using B2CGraphShell;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzureChallengeNetwork.Website.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _storageConnectionString;
        private readonly Guid _userObjectId;
        private readonly B2CGraphClient _graphClient;

       
        public FeedController()
        {
            // Loading the connection strings
            _storageConnectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            _serviceBusConnectionString = ConfigurationManager.ConnectionStrings["ServiceBusConnectionString"].ConnectionString;

            // Getting the ObjectId from the current user
            if(System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                _userObjectId = Guid.Parse(ClaimsPrincipal.Current.Claims.FirstOrDefault(a => a.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value);
            }

            // Connecting to the Graph API
            string clientId = ConfigurationManager.AppSettings["b2c:ClientId"];
            string tenant = ConfigurationManager.AppSettings["b2c:Tenant"];
            string clientSecret = ConfigurationManager.AppSettings["b2c:ClientSecret"];
            _graphClient = new B2CGraphClient(clientId, clientSecret, tenant);
        }

        public async Task<ActionResult> Index()
        {
            var viewModel = new FeedViewModel();

            // Using the Graph database to get all the users
            var users = await _graphClient.GetAllUsers(null);
            viewModel.Friends = JsonConvert.DeserializeObject<RootObject>(users).Value;

            // Connect to the Azure Table
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("images");
            table.CreateIfNotExists();

            using (var db = new AzureChallengeContext())
            {
                // Loading the last 100 posts by creation date
                List<Userpost> posts =  db.Userposts.OrderByDescending(a => a.CreationDateTime).Take(100).ToList();
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

                    // Looking up the user in the friends list (to safe some calls)
                    postModel.PostedByUser = viewModel.Friends.First(a => a.ObjectId == post.UserObjectId);

                    // Using the Graph database to get the user 
                    /*
                    var user = await _graphClient.GetUserByObjectId(post.UserObjectId.ToString());
                    UserProfile profile = JsonConvert.DeserializeObject<UserProfile>(user);
                    post.UserName = profile.DisplayName;
                    */
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
                    CreationDateTime = DateTime.Now,
                    UserObjectId = _userObjectId
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

                // Add message to Service Bus Topic
                MessagingFactory messagingFactory = MessagingFactory.CreateFromConnectionString(_serviceBusConnectionString);
                MessageSender messageSender = messagingFactory.CreateMessageSender("social");
                messageSender.Send(new BrokeredMessage($"{postId}|{imageId}"));

            }

            return RedirectToAction("Index");
        }

    }

    public class RootObject
    {
        public List<UserProfile> Value { get; set; }
    }
}