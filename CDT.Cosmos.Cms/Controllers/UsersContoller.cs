﻿using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Services.Configurations;
using CDT.Cosmos.Cms.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Controllers
{

    // See: https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/areas?view=aspnetcore-3.1
    /// <summary>
    /// User management controller
    /// </summary>
    [Authorize(Roles = "Administrators, Editors")]
    public class UsersController : Controller
    {
        /// <summary>
        ///     These roles are hard-wired, and cannot be altered here.
        /// </summary>
        private static readonly string[] RestrictedRoles =
            {"reviewers", "administrators", "editors", "team members", "authors"};

        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="roleManager"></param>
        /// <param name="options"></param>
        /// <param name="dbContext"></param>
        /// <param name="syncContext"></param>
        public UsersController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<CosmosConfig> options,
            ApplicationDbContext dbContext,
            SqlDbSyncContext syncContext)
        {
            if (options.Value.SiteSettings.AllowSetup ?? true)
            {
                throw new Exception("Permission denied. Website in setup mode.");
            }
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _dbContext = dbContext;

            if (syncContext.IsConfigured())
                dbContext.LoadSyncContext(syncContext);
        }

        /// <summary>
        ///     User manager home page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///     Gets the role membership for a user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> RoleMembership(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            ViewData["saved"] = null;

            var roleList = (await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(id))).ToList();

            var model = new UserRolesViewModel
            {
                UserEmailAddress = user.Email,
                UserId = user.Id
            };

            if (roleList.Any(a => a.Contains("Administrator")))
                model.Administrator = true;
            else if (roleList.Any(a => a.Contains("Editors")))
                model.Editor = true;
            else if (roleList.Any(a => a.Contains("Authors")))
                model.Author = true;
            else if (roleList.Any(a => a.Contains("Reviewers")))
                model.Reviewer = true;
            else if (roleList.Any(a => a.Contains("Team Members")))
                model.TeamMember = true;
            else
                model.NoRole = true;

            return View(model);
        }

        /// <summary>
        ///     Updates role membership
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RoleMembership(UserRolesViewModel model)
        {
            if (model == null)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                ViewData["saved"] = null;

                var user = await _userManager.FindByIdAsync(model.UserId);

                var roleList = await _userManager.GetRolesAsync(user);

                if (await _userManager.IsInRoleAsync(user, "Administrators"))
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Administrators");
                    if (admins.Count == 1)
                    {
                        ModelState.AddModelError("UserRole",
                            "Cannot change permissions of the only administrator.");
                        return View(model);
                    }
                }

                if (await _userManager.IsInRoleAsync(user, "Editors"))
                    if (roleList.Contains("Administrators"))
                    {
                        ModelState.AddModelError("UserRole",
                            "Cannot change role membershp of an administrator.");
                        return View(model);
                    }

                var result = await _userManager.RemoveFromRolesAsync(user, roleList);

                if (result.Succeeded)
                {
                    switch (model.UserRole)
                    {
                        case "Administrator":
                            result = await _userManager.AddToRoleAsync(user, "Administrators");
                            break;
                        case "Editor":
                            result = await _userManager.AddToRoleAsync(user, "Editors");
                            break;
                        case "Author":
                            result = await _userManager.AddToRoleAsync(user, "Authors");
                            break;
                        case "Reviewer":
                            result = await _userManager.AddToRoleAsync(user, "Reviewers");
                            break;
                        case "TeamMember":
                            // Team member role is a late addition, existing sites may not have this yet.
                            if (!await _roleManager.RoleExistsAsync("Team Members"))
                                await _roleManager.CreateAsync(new IdentityRole("Team Members"));
                            result = await _userManager.AddToRoleAsync(user, "Team Members");
                            break;
                        case "RemoveAccount":
                            result = await _userManager.DeleteAsync(user);
                            if (result.Succeeded)
                                return RedirectToAction("Index");
                            break;
                    }

                    if (result.Succeeded)
                    {
                        roleList = await _userManager.GetRolesAsync(user);

                        model = new UserRolesViewModel
                        {
                            UserEmailAddress = user.Email,
                            UserId = user.Id
                        };

                        if (roleList.Any(a => a.Contains("Administrator")))
                            model.Administrator = true;
                        else if (roleList.Any(a => a.Contains("Editors")))
                            model.Editor = true;
                        else if (roleList.Any(a => a.Contains("Authors")))
                            model.Author = true;
                        else if (roleList.Any(a => a.Contains("Reviewers")))
                            model.Reviewer = true;
                        else if (roleList.Any(a => a.Contains("Team Members")))
                            model.TeamMember = true;
                        else
                            model.NoRole = true;
                        ViewData["saved"] = true;

                        model.UserRole = string.Empty;
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError("UserEmailAddress",
                                $"Code: {error.Code}. Description: {error.Description}");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("UserEmailAddress",
                            $"Code: {error.Code}. Description: {error.Description}");
                }
            }

            return View(model);
        }

        /// <summary>
        ///     Gets a list of roles
        /// </summary>
        /// <returns></returns>
        public IActionResult Roles()
        {
            return View();
        }

        /// <summary>
        ///     Lists users in a role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UsersAndRoles(string id)
        {
            var identityRole = await _roleManager.FindByIdAsync(id);
            return View("UsersAndRoles", identityRole);
        }

        /// <summary>
        ///     Sign out of the website
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public new async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        #region DATA FOR ROLE BASED ACCESS CONTROLL

        #region ROLE CRUD

        /// <summary>
        ///     Gets a list of roles
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IActionResult> Read_Roles([DataSourceRequest] DataSourceRequest request)
        {
            var query = _roleManager.Roles.Select(s => new RoleItemViewModel
            {
                Id = s.Id,
                RoleName = s.Name,
                RoleNormalizedName = s.NormalizedName
            }).Where(w => RestrictedRoles.Contains(w.RoleName.ToLower()) == false).OrderBy(r => r.RoleName);

            return Json(await query.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<ActionResult> Roles_Create([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<RoleItemViewModel> roles)
        {
            var results = new List<RoleItemViewModel>();

            if (roles != null && ModelState.IsValid)
                foreach (var role in roles)
                    if (RestrictedRoles.Contains(role.RoleName.ToLower()))
                    {
                        ModelState.AddModelError("", $"{role.RoleName} is a reserved role, and cannot be altered.");
                    }
                    else
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role.RoleName));
                        if (result.Succeeded)
                        {
                            var newRole = await _roleManager.FindByNameAsync(role.RoleName);
                            role.Id = newRole.Id;
                            role.RoleNormalizedName = newRole.NormalizedName;
                            results.Add(role);
                        }
                        else
                        {
                            foreach (var identityError in result.Errors)
                                ModelState.AddModelError("",
                                    $"Code: {identityError.Code} | {identityError.Description}");
                        }
                    }

            return Json(await results.ToDataSourceResultAsync(request, ModelState));
        }

        [HttpPost]
        public async Task<ActionResult> Roles_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<RoleItemViewModel> roles)
        {
            if (roles != null && ModelState.IsValid)
                foreach (var role in roles)
                    if (RestrictedRoles.Contains(role.RoleNormalizedName.ToLower()))
                    {
                        ModelState.AddModelError("", $"{role.RoleName} is a reserved role, and cannot be altered.");
                    }
                    else
                    {
                        var identityRole = await _roleManager.FindByIdAsync(role.Id);
                        identityRole.Name = role.RoleName;

                        var result = await _roleManager.UpdateAsync(identityRole);

                        if (result.Succeeded)
                        {
                            var updatedRole = await _roleManager.FindByIdAsync(role.Id);
                            role.RoleName = updatedRole.Name;
                            role.RoleNormalizedName = updatedRole.NormalizedName;
                        }
                        else
                        {
                            foreach (var identityError in result.Errors)
                                ModelState.AddModelError("",
                                    $"Code: {identityError.Code} | {identityError.Description}");
                        }
                    }

            return Json(await roles.ToDataSourceResultAsync(request, ModelState));
        }

        [HttpPost]
        public async Task<ActionResult> Roles_Destroy([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<RoleItemViewModel> roles)
        {
            var results = new List<RoleItemViewModel>();

            if (roles != null && ModelState.IsValid)
                foreach (var role in roles)
                    if (RestrictedRoles.Contains(role.RoleNormalizedName.ToLower()))
                    {
                        ModelState.AddModelError("", $"{role.RoleName} is a reserved role, and cannot be altered.");
                    }
                    else
                    {
                        var doomedRole = await _roleManager.FindByIdAsync(role.Id);
                        var result = await _roleManager.DeleteAsync(doomedRole);

                        if (result.Succeeded)
                            results.Add(role);
                        else
                            foreach (var identityError in result.Errors)
                                ModelState.AddModelError("",
                                    $"Code: {identityError.Code} | {identityError.Description}");
                    }

            return Json(await results.ToDataSourceResultAsync(request, ModelState));
        }

        #endregion

        #region USERS CRUD

        public async Task<IActionResult> Read_UsersIndexViewModel([DataSourceRequest] DataSourceRequest request)
        {
            //UsersIndexViewModel
            var roles = await _dbContext.Roles.ToListAsync();
            var usersAndRoles = await _dbContext.UserRoles.ToListAsync();
            var users = await _dbContext.Users.ToListAsync();
            var userLogins = await _dbContext.UserLogins.ToListAsync();

            var queryUsersAndRoles = (from ur in usersAndRoles
                                      join r in roles on ur.RoleId equals r.Id
                                      select new
                                      {
                                          ur.UserId,
                                          RoleName = r.Name
                                      }).ToList();

            var model = new List<UsersIndexViewModel>();

            foreach (var user in users)
            {
                var userRoles = queryUsersAndRoles.Where(f => f.UserId == user.Id).ToList();
                var login = userLogins.FirstOrDefault(f => f.UserId == user.Id);

                model.Add(new UsersIndexViewModel
                {
                    UserId = user.Id,
                    EmailConfirmed = user.EmailConfirmed,
                    EmailAddress = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = userRoles.Count == 0 ? "No Role" : string.Join(", ", userRoles.Select(s => s.RoleName)),
                    LoginProvider = login == null ? "Local Acct." : login.ProviderDisplayName
                });
            }

            return Json(await model.ToDataSourceResultAsync(request));
        }

        public async Task<ActionResult> Update_UsersIndexViewModel([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<UsersIndexViewModel> users)
        {
            if (users != null && ModelState.IsValid)
                foreach (var user in users)
                {
                    var identityUser = await _userManager.FindByIdAsync(user.UserId);

                    //
                    // The following fields are updateable
                    //

                    identityUser.EmailConfirmed = user.EmailConfirmed;

                    await _userManager.UpdateAsync(identityUser);
                }

            return Json(await users.ToDataSourceResultAsync(request, ModelState));
        }

        public async Task<IActionResult> Read_Users([DataSourceRequest] DataSourceRequest request, string roleId = "")
        {
            if (string.IsNullOrEmpty(roleId))
            {
                var query = _userManager.Users.Select(s => new UserItemViewModel
                {
                    Id = s.Id,
                    Email = s.Email,
                    EmailConfirmed = s.EmailConfirmed,
                    PhoneNumber = s.PhoneNumber,
                    Selected = false
                }).OrderBy(o => o.Email);

                return Json(await query.ToDataSourceResultAsync(request));
            }

            var users = await _userManager.Users.Select(s => new UserItemViewModel
            {
                Id = s.Id,
                Email = s.Email,
                EmailConfirmed = s.EmailConfirmed,
                PhoneNumber = s.PhoneNumber,
                Selected = false
            }).ToListAsync();

            var identityRole = await _roleManager.FindByIdAsync(roleId);

            var usersInRole = await _userManager.GetUsersInRoleAsync(identityRole.Name);

            if (usersInRole.Any())
            {
                var userIds = usersInRole.Select(s => s.Id).ToList();

                foreach (var user in users) user.Selected = userIds.Contains(user.Id);
            }

            return Json(await users.ToDataSourceResultAsync(request));
        }

        [HttpPost]
        public async Task<ActionResult> Users_Update([DataSourceRequest] DataSourceRequest request,
            [Bind(Prefix = "models")] IEnumerable<UserItemViewModel> users, string roleId)
        {
            if (users != null && ModelState.IsValid)
            {
                var identityRole = await _roleManager.FindByIdAsync(roleId);

                foreach (var user in users)
                {
                    var identityUser = await _userManager.FindByIdAsync(user.Id);

                    if (identityRole.Name.ToLower() != "administrators")
                    {
                        //
                        // If a role is given, add or remove people from that role
                        //
                        if (user.Selected)
                        {
                            //
                            // This person should be in the role.
                            //
                            if (!await _userManager.IsInRoleAsync(identityUser, identityRole.Name))
                                //
                                // If the person is not, then add that person to the role.
                                await _userManager.AddToRoleAsync(identityUser, identityRole.Name);
                        }
                        else
                        {
                            //
                            // This person should NOT be in the role anymore.
                            //
                            if (!await _userManager.IsInRoleAsync(identityUser, identityRole.Name))
                                //
                                // If the person is in the role, then remove that person.
                                await _userManager.RemoveFromRoleAsync(identityUser, identityRole.Name);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cannot manage the administrator role from here.");
                    }

                    //
                    // Allow a user, who's email isn't confirmed, to be "manually" confirmed here.
                    //
                    if (user.EmailConfirmed && identityUser.EmailConfirmed == false)
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                        await _userManager.ConfirmEmailAsync(identityUser, token);
                    }
                }
            }

            return Json(await users.ToDataSourceResultAsync(request, ModelState));
        }

        #endregion

        #endregion
    }
}