using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    public class UpgradeIndexViewModel
    {
        /// <summary>
        ///     Administrator authorizes upgrade
        /// </summary>
        [Display(Name = "Upgrade Authorized")]
        [Required(ErrorMessage = "Authorization is required.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Authorization is required.")]
        public bool Authorized { get; set; } = false;

        [Display(Name = "Upgrade Authorized")]
        [Required(ErrorMessage = "Acknowledgement is required.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Acknowledgement is required.")]
        public bool IsBackedUp { get; set; } = false;

        [Display(Name = "Software Upgrade Required")]
        [Required(ErrorMessage = "Acknowledgement is required.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Acknowledgement is required.")]
        public bool SoftwareUpgrade { get; set; } = false;

        [Display(Name = "Pending Upgrades")] public List<string> PendingMigrations { get; set; }
    }
}