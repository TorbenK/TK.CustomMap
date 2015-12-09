using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
