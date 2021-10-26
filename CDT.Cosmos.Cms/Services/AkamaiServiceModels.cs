using Newtonsoft.Json;

namespace CDT.Cosmos.Cms.Services
{
    public class AkamaiPurgeObjects
    {
        /// <summary>
        /// Host name of the end point to be purged
        /// </summary>
        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        /// <summary>
        ///     An array of CP codes or URLs to purge.
        /// </summary>
        [JsonProperty("objects")]
        public string[] Objects { get; set; }
    }

    public static class PurgeEndPoints
    {
        public static string CpCodeProductionEndpoint = "/ccu/v3/invalidate/cpcode/production";
        public static string UrlProductionEndpoint = "/ccu/v3/invalidate/url/production";
    }
}