using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api
{
    /// <summary>
    /// Android result set
    /// </summary>
    public class TKNativeAndroidPlaceResult : IPlaceResult
    {
        /// <summary>
        /// Gets/Sets the Place Id
        /// </summary>
        public string PlaceId { get; set; }
        ///<inheritdoc/>
        public string Description { get; set; }
    }
}
