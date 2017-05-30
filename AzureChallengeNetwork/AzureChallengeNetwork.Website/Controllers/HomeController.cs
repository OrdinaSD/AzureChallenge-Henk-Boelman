using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AzureChallengeNetwork.Website.Models;
using B2CGraphShell;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace AzureChallengeNetwork.Website.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Claims()
        {
            Claim displayName = ClaimsPrincipal.Current.FindFirst(ClaimsPrincipal.Current.Identities.First().NameClaimType);
            ViewBag.DisplayName = displayName != null ? displayName.Value : string.Empty;
            return View();
        }

     
        public ActionResult ImageSearch(string keyword="", string tag="")
        {
            string searchServiceName = ConfigurationManager.AppSettings["searchServiceName"];
            string searchServiceApiKey = ConfigurationManager.AppSettings["searchServiceApiKey"];
            string indexName = ConfigurationManager.AppSettings["indexName"];


            StringBuilder filter = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(tag))
            {
                filter.Append($"Tags/any(t: t eq '{tag}')");
            }
           
            SearchServiceClient searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceApiKey));
            ISearchIndexClient indexClient = searchClient.Indexes.GetClient(indexName);

            var parameters = new SearchParameters
            {
                Top = 50,
                Facets = new List<string> { "Tags" },
                IncludeTotalResultCount = true,
                Filter = filter.ToString()
            };

            DocumentSearchResult<ImageInsight> results = indexClient.Documents.Search<ImageInsight>(keyword, parameters);
           

            return View(results);
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }


    }
}