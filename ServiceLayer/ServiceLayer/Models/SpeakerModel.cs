using MyCodeCamp.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.Models
{
    public class SpeakerModel
    {
        public string Url { get; set; }
        [Required]
        [MinLength(5)]
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string WebsiteUrl { get; set; }
        public string TwitterName { get; set; }
        public string GitHubName { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(25)]
        public string Bio { get; set; }
        public string HeadShotUrl { get; set; }

        public ICollection<TalkModel> Talks { get; set; }
    }
}
