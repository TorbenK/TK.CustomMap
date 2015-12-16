using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api
{
    /// <summary>
    /// iOS Result set
    /// </summary>
    public class TKNativeiOSPlaceResult : IPlaceResult
    {
        ///<inheritdoc/>
        public string Description { get; set; }
        /// <summary>
        /// Gets/Sets the details of the place
        /// </summary>
        public TKPlaceDetails Details { get; set; }
    }
}
