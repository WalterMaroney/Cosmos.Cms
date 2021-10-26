using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CDT.Cosmos.Cms.Models
{
    public class FileManagerViewModel
    {
        public int? TeamId { get; set; }
        public IEnumerable<SelectListItem> TeamFolders { get; set; }
    }
}