using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AzureChallengeNetwerk.Bot.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace AzureChallengeNetwerk.Bot.Dialogs
{
    [Serializable]
    public class SearchDialog : IDialog<object>
    {
        private readonly string _searchText;
        private readonly string _searchServiceName;
        private readonly string _searchServiceApiKey;
        private readonly string _indexName;
        private readonly string _imageBaseUrl;

        public SearchDialog(string facet)
        {
             _searchText = facet;
             _searchServiceName = ConfigurationManager.AppSettings["searchServiceName"];
             _searchServiceApiKey = ConfigurationManager.AppSettings["searchServiceApiKey"];
             _indexName = ConfigurationManager.AppSettings["indexName"];
             _imageBaseUrl = ConfigurationManager.AppSettings["imageBaseUrl"];
        }

        public async Task StartAsync(IDialogContext context)
        {
            // Connect to Azure search
            SearchServiceClient searchClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_searchServiceApiKey));
            ISearchIndexClient indexClient = searchClient.Indexes.GetClient(_indexName);
            DocumentSearchResult<ImageInsight> results = indexClient.Documents.Search<ImageInsight>(_searchText);

            await SendResults(context, results);
        }

        private async Task SendResults(IDialogContext context, DocumentSearchResult<ImageInsight> results)
        {
            // Creating the response message
            var message = context.MakeMessage();

            if (results.Results.Count == 0)
            {
                await context.PostAsync("There were no results found for \"" + _searchText + "\".");
                context.Done<object>(null);
            }
            else
            {
                var cards = results.Results.Select(h => new HeroCard
                {
                    Title = h.Document.ImageId,
                    Images = new[] { new CardImage(_imageBaseUrl + h.Document.ImageId) },
                    Text = h.Document.Caption
                });
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = cards.Select(c => c.ToAttachment()).ToList();
                message.Text = "Here are the results that I found:";

                await context.PostAsync(message);
                context.Done<object>(null);
            }
        }
    }
}