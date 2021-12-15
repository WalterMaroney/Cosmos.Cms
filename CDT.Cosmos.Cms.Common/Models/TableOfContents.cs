using System;
using System.Collections.Generic;

namespace CDT.Cosmos.Cms.Common.Models
{
    public class TableOfContents
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNo { get; set; }
        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Total number of items.
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// Items in the current page
        /// </summary>
        public List<TOCItem> Items { get; set; }
    }

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
