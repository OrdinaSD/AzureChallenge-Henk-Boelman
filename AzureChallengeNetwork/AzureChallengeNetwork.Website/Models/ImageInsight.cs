using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace AzureChallengeNetwork.Website.Models
{
    [SerializePropertyNamesAsCamelCase]
    public class ImageInsight
    {
        [Key, IsFilterable, IsSearchable]
        public string ImageId { get; set; }

        [IsSearchable, IsFilterable]
        public string Caption { get; set; }

        [IsSearchable, IsFilterable, IsFacetable]
        public string[] Tags { get; set; }
    }
}
