using System;

namespace CDT.Cosmos.Cms.Common.Models
{
    /// <summary>
    /// Table of Contents (TOC) Item
    /// </summary>
    public class TOCItem
    {
        /// <summary>
        /// URL Path to page
        /// </summary>
        public string UrlPath { get; set; }

        /// <summary>
        /// Title of page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Published date and time
        /// </summary>
        public DateTime Published { get; set; }

        /// <summary>
        /// When last updated
        /// </summary>
        public DateTime Updated { get; set; }
    }
}
