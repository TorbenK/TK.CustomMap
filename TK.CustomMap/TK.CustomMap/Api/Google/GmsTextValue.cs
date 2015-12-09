using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.Google
{
    /// <summary>
    /// A Google Result class holding a value and a corresponding formatted text
    /// </summary>
    public struct GmsTextValue
    {
        /// <summary>
        /// Gets/Sets the value
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Gets/Sets the text
        /// </summary>
        public string Text { get; set; }
    }
}
