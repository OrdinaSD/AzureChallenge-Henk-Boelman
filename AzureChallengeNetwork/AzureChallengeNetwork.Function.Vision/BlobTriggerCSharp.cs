using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using AzureChallengeNetwork.Function.Vision.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;


namespace AzureChallengeNetwork.Function.Vision
{
    public static class BlobTriggerCSharp
    {

        [FunctionName("BlobTriggerCSharp")]        
        public static async void Run([BlobTrigger("images-test/{name}", Connection = "blobConn")]Stream myBlob, string name, TraceWriter log)
        {
            string cosmosDbEndpoint = ConfigurationManager.AppSettings["cosmosDbEndpoint"];
            string cosmosDbAuthKey = ConfigurationManager.AppSettings["cosmosDbAuthKey"];
            string cosmosDbDatabaseName = "Insights";
            string cosmosDbCollectionName = "ImageInsights";

            string visionKey = ConfigurationManager.AppSettings["visionKey"];
            string visionEndpoint = ConfigurationManager.AppSettings["visionEndpoint"];

            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            //
            // Vision DB
            //

            var client = new VisionServiceClient(visionKey, visionEndpoint);

            var result =  client.AnalyzeImageAsync(myBlob, new[] { VisualFeature.Description, VisualFeature.Tags });
            result.Wait();

            var visionResult = result.Result;

            var insights = new ImageInsights()
            {
                ImageId = name,
                Tags = visionResult.Tags.Select(a => a.Name).ToArray(),
                Caption = visionResult.Description.Captions.First().Text
            };


            //
            // Cosmos DB
            //

            // Setup the connection
            DocumentClient cosmosClient = new DocumentClient(new Uri(cosmosDbEndpoint), cosmosDbAuthKey);

            // Create the database
            await cosmosClient.CreateDatabaseIfNotExistsAsync(new Database { Id = cosmosDbDatabaseName });

            // Create the collection
            DocumentCollection myCollection = new DocumentCollection { Id = cosmosDbCollectionName };
            myCollection.PartitionKey.Paths.Add("/ImageId");

            await cosmosClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(cosmosDbDatabaseName),
                myCollection,
                new RequestOptions { OfferThroughput = 400 });

            // Storing the document
            await cosmosClient.CreateDocumentAsync(
                UriFactory.CreateDocumentCollectionUri(cosmosDbDatabaseName, cosmosDbCollectionName),
                insights);


            //
            // Just some logging info
            //

            var json = new JavaScriptSerializer().Serialize(visionResult);

            log.Info(json);

            foreach (Caption caption in visionResult.Description.Captions)
            {
                log.Info($"{caption.Text} | Confidence:{caption.Confidence}");
            }
            
            foreach (Tag resultTag in visionResult.Tags)
            {
                log.Info($"Name:{resultTag.Name} | Hint:{resultTag.Hint} | Confidence:{resultTag.Confidence}");
            }

            log.Info("Done");
        }
    }
}