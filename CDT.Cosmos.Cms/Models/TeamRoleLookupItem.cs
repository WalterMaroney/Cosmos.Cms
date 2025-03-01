﻿using CDT.Cosmos.Cms.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    public class TeamRoleLookupItem
    {
        public TeamRoleLookupItem()
        {
        }

        public TeamRoleLookupItem(TeamRoleEnum role)
        {
            TeamRoleId = (int)role;
            TeamRoleName = Enum.GetName(typeof(TeamRoleEnum), role);
        }

        [Key] public int TeamRoleId { get; set; }

        public string TeamRoleName { get; set; }
    }
}