using System.Collections.Generic;
using Newtonsoft.Json;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// Google Polyline class
    /// </summary>
    public class GmsPolyline
    {
        /// <summary>
        /// Gets the points as string
        /// </summary>
        public string Points { get; set; }
        /// <summary>
        /// Gets the converted positions
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Position> Positions
        {
            get
            {
                return GooglePoints.Decode(this.Points);
            }
        }
    }
}
