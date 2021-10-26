using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Models;
using CDT.Cosmos.Cms.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CDT.Cosmos.Cms.Services
{
    /// <summary>
    ///     Logic for determining a user's team membership.
    /// </summary>
    public class TeamIdentityLogic
    {
        private readonly List<Team> _teams;
        private readonly string _userId;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="user"></param>
        public TeamIdentityLogic(ApplicationDbContext dbContext, ClaimsPrincipal user)
        {
            var nameIdentifier = user.Claims.FirstOrDefault(f => f.Type == ClaimTypes.NameIdentifier);
            _userId = nameIdentifier.Value;

            _teams = dbContext.Teams.Include(i => i.Members).Where(t => t.Members.Any(m => m.UserId == _userId))
                .ToListAsync().Result;

            foreach (var team in _teams)
                // Don't track this entity by the context.
                dbContext.Entry(team).State = EntityState.Detached;
        }

        /// <summary>
        ///     Gets a user's role for a team.
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public TeamRoleEnum? GetRoleForTeam(string teamName)
        {
            var team = _teams.FirstOrDefault(
                f => f.TeamName.Equals(teamName, StringComparison.CurrentCultureIgnoreCase));
            if (team == null)
                return null;

            var role = team.Members.FirstOrDefault(m => m.UserId == _userId);
            return (TeamRoleEnum)role.TeamRole;
        }

        /// <summary>
        ///     Gets a list of teams that the user belongs to.
        /// </summary>
        public List<TeamViewModel> GetTeams()
        {
            return _teams.Select(t => new TeamViewModel
            {
                Id = t.Id,
                TeamDescription = t.TeamDescription,
                TeamName = t.TeamName
            }).ToList();
        }

        /// <summary>
        ///     Checks to see if a user is in a role for a team
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsInRole(TeamRoleEnum role, string teamName)
        {
            return GetRoleForTeam(teamName) == role;
        }

        /// <summary>
        ///     User can create a page
        /// </summary>
        /// <returns></returns>
        public bool CanCreatePage()
        {
            var roles = new[] { (int)TeamRoleEnum.Author, (int)TeamRoleEnum.Editor };
            return _teams.Any(a => a.Members.Any(m => roles.Contains(m.TeamRole)));
        }
    }
}