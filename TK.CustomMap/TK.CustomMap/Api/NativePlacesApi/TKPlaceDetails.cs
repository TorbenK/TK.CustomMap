using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Api
{
    /// <summary>
    /// Details of a place
    /// </summary>
    public class TKPlaceDetails
    {
        /// <summary>
        /// Gets/Sets the coordinate of the place
        /// </summary>
        public Position Coordinate { get; set; }
    }
}
