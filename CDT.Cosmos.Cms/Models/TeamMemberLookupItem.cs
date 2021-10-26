using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    public class TeamMemberLookupItem
    {
        public TeamMemberLookupItem()
        {
        }

        public TeamMemberLookupItem(IdentityUser user)
        {
            UserEmail = user.Email;
            UserId = user.Id;
        }

        public string UserId { get; set; }

        [Display(Name = "User Email")]
        [EmailAddress]
        public string UserEmail { get; set; }
    }
}