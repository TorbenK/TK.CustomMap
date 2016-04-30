using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap
{
    /// <summary>
    /// <see cref="EventArgs"/> providing a <see cref="Position"/>
    /// </summary>
    public class TKGenericEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the value of the <see cref="TKGenericEventArgs"/>
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// Creates a new instance of <see cref="TKGenericEventArgs"/>
        /// </summary>
        /// <param name="value">The value</param>
        public TKGenericEventArgs(T value)
        {
            this.Value = value;
        }
    }
}
