using Newtonsoft.Json;

namespace RevitMCPCommandSet.Models.Common
{
    /// <summary>
    /// Geolocation and project position info for the active project.
    /// Latitude/Longitude are the site's true geographic anchor (from SiteLocation).
    /// EastWest/NorthSouth/Elevation/Angle describe the transform between internal
    /// (project) coordinates and shared/survey coordinates at the internal origin.
    /// </summary>
    public class ProjectLocationInfo
    {
        /// <summary>
        /// Latitude of the site location, in decimal degrees.
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of the site location, in decimal degrees.
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Elevation of the site location (mm).
        /// </summary>
        [JsonProperty("siteElevation")]
        public double SiteElevation { get; set; }

        /// <summary>
        /// Time zone offset used by the site location (hours from UTC).
        /// </summary>
        [JsonProperty("timeZone")]
        public double TimeZone { get; set; }

        /// <summary>
        /// Place name associated with the site location, if any.
        /// </summary>
        [JsonProperty("placeName")]
        public string PlaceName { get; set; } = "";

        /// <summary>
        /// East/West offset (mm) of the shared coordinates origin relative to the
        /// internal origin (project base point at internal (0,0,0)).
        /// </summary>
        [JsonProperty("eastWest")]
        public double EastWest { get; set; }

        /// <summary>
        /// North/South offset (mm) of the shared coordinates origin relative to the
        /// internal origin.
        /// </summary>
        [JsonProperty("northSouth")]
        public double NorthSouth { get; set; }

        /// <summary>
        /// Elevation offset (mm) of the shared coordinates origin relative to the
        /// internal origin.
        /// </summary>
        [JsonProperty("elevation")]
        public double Elevation { get; set; }

        /// <summary>
        /// Rotation angle (radians) from project (internal) north to true north,
        /// applied at the internal origin.
        /// </summary>
        [JsonProperty("angle")]
        public double Angle { get; set; }
    }
}
