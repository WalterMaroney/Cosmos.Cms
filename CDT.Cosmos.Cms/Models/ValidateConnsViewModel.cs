using System.Collections.Generic;

namespace CDT.Cosmos.Cms.Models
{
    public class ValConViewModel
    {
        public ValConViewModel()
        {
            Results = new List<ConnectionResult>();
        }

        public List<ConnectionResult> Results { get; set; }
    }

    public class ConnectionResult
    {
        public string Host { get; set; }
        public string ServiceType { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}