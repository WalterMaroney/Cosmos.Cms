using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CDT.Cosmos.Cms.Models
{
    public class LayoutCatalogViewModel
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Layout Name")]
        public string Name { get; set; }
        [Display(Name = "Description/Notes")]
        public string Description { get; set; }
        [Display(Name = "License")]
        public string License { get; set; }
    }
}
