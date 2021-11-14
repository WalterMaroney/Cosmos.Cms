using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    /// <summary>
    /// Setup controller index view model.
    /// </summary>
    public class SetupIndexViewModel
    {   
        /// <summary>
        /// Setup state we are in.
        /// </summary>
        public SetupState SetupState { get; set; }

        [Display(Name = "Administrator Email Address")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string AdminEmail { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }

    /// <summary>
    /// Set
    /// </summary>
    public enum SetupState
    {
        /// <summary>
        /// Database has not been setup yet.
        /// </summary>
        NewInstall,
        /// <summary>
        /// Database is installed but needs updates.
        /// </summary>
        Update
    }
}
