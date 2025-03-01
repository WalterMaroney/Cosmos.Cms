﻿using CDT.Cosmos.Cms.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    public class TeamMemberViewModel
    {
        [Key]
        [Display(Name = "Team Member Id")]
        public int Id { get; set; }

        /// <summary>
        ///     The role ID of this team member as defined by <see cref="TeamRoleEnum" />
        /// </summary>
        [Display(Name = "Team Role")]
        [UIHint("TeamMemberRole")]
        public TeamRoleLookupItem TeamRole { get; set; } = new(TeamRoleEnum.Reviewer);

        public TeamViewModel Team { get; set; }

        [Display(Name = "Member")]
        [UIHint("TeamMember")]
        public TeamMemberLookupItem Member { get; set; }
    }
}