using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureChallengeNetwork.Website.Entities
{

    public class Userpost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid UserObjectId { get; set; }

        public DateTime CreationDateTime { get; set; }

        public string Text { get; set; }

        [NotMapped]
        public string UserName { get; set; }
    }
}