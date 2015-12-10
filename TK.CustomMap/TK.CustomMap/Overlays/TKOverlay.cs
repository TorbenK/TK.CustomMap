using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Base overlay class
    /// </summary>
    public abstract class TKOverlay : TKBase
    {
        public const string ColorPropertyName = "Color";

        private Color _color;

        /// <summary>
        /// Gets/Sets the main color of the overlay.
        /// </summary>
        public Color Color 
        {
            get { return this._color; }
            set { this.SetField(ref this._color, value); }
        }
    }
}
