﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDT.Cosmos.Cms.Common.Data
{
    /// <summary>
    ///     Website layout content.
    /// </summary>
    [Serializable]
    public class Layout
    {
        /// <summary>
        ///     Identity key for this entity.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Community Layout Id")]
        public string CommunityLayoutId { get; set; }

        /// <summary>
        ///     If true this is the default layout for website.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        ///     Friendly name of layout
        /// </summary>
        [Display(Name = "Layout Name")]
        [StringLength(128)]
        public string LayoutName { get; set; }

        /// <summary>
        ///     Notes about the layout
        /// </summary>
        [Display(Name = "Notes")]
        [DataType(DataType.Html)]
        public string Notes { get; set; }

        /// <summary>
        ///     Content injected into the web page HEAD
        /// </summary>
        [Display(Name = "HEAD Content")]
        [DataType(DataType.Html)]
        public string Head { get; set; }

        /// <summary>
        ///     Body tag attributes
        /// </summary>
        [Display(Name = "BODY Html Attributes", GroupName = "Body")]
        [StringLength(256)]
        public string BodyHtmlAttributes { get; set; }

        /// <summary>
        ///     Web page header content
        /// </summary>
        [Display(Name = "Header Html Content", GroupName = "Header")]
        [DataType(DataType.Html)]
        public string HtmlHeader { get; set; }

        /// <summary>
        ///     Content injected into the web site footer.
        /// </summary>
        [Display(Name = "Footer Html Content", GroupName = "Footer")]
        [DataType(DataType.Html)]
        public string FooterHtmlContent { get; set; }

        /// <summary>
        ///     Articles assocated with this layout (soon to be depreciated)
        /// </summary>
        public virtual ICollection<Article> Articles { get; set; }
    }
}