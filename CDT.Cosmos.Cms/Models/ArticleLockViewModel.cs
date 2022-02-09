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
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// User email for this lock
        /// </summary>
        public string UserEmail { get; set; } = "";

        /// <summary>
        /// Article Record ID for this lock
        /// </summary>
        public int ArticleRecordId { get; set; } = -1;

        /// <summary>
        /// ID of the Signal R Connection with lock.
        /// </summary>
        public string ConnectionId { get; set; } = "";

        /// <summary>
        /// When the lock was set
        /// </summary>
        public DateTimeOffset LockSetDateTime { get; set; } = DateTimeOffset.Now;

    }
}
