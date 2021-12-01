using CDT.Cosmos.Cms.Common.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Models
{
    /// <summary>
    ///     Article version list info item
    /// </summary>
    [Serializable]
    public class ArticleVersionInfo
    {
        /// <inheritdoc cref="Article.Id" />
        [Key]
        public int Id { get; set; }

        /// <inheritdoc cref="Article.VersionNumber" />
        public int VersionNumber { get; set; }

        /// <inheritdoc cref="Article.Title" />
        public string Title { get; set; }

        /// <inheritdoc cref="Article.Updated" />
        public DateTime Updated { get; set; }

        /// <inheritdoc cref="Article.Published" />
        public DateTime? Published { get; set; }

        /// <inheritdoc cref="Article.Expires" />
        public DateTime? Expires { get; set; }

        /// <summary>
        /// Can use HTML Editor
        /// </summary>
        public bool UsesHtmlEditor { get; set; }
    }
}