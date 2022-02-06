using System;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    /// <summary>
    /// Represents a lock on an article because it is being edited.
    /// </summary>
    public class ArticleLockViewModel
    {
        /// <summary>
        /// Unique ID of the lock
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// User email for this lock
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        /// Article ID for this lock
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// When the lock was set
        /// </summary>
        public DateTimeOffset LockSetDateTime { get; set; }

    }
}
