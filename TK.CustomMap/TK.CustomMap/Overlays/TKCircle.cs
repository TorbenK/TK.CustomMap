using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Displaying a circle on the map
    /// </summary>
    public class TKCircle : TKBase
    {
        public const string RadiusPropertyName = "Radius";
        public const string ColorPropertyName = "Color";
        public const string StrokeColorPropertyName = "StrokeColor";
        public const string CenterPropertyName = "Center";
        public const string StrokeWidthPropertyName = "StrokeWidth";

        private double _radius;
        private Color _color;
        private Color _strokeColor;
        private Position _center;
        private float _strokeWidth;
        /// <summary>
        /// Gets/Sets the radius of the circle
        /// </summary>
        public double Radius
        {
            get { return this._radius; }
            set { this.SetField(ref this._radius, value); }
        }
        /// <summary>
        /// Gets/Sets the color of the circle
        /// </summary>
        public Color Color
        {
            get { return this._color; }
            set { this.SetField(ref this._color, value); }
        }
        /// <summary>
        /// Gets/Sets the stroke color of the circle
        /// </summary>
        public Color StrokeColor
        {
            get { return this._strokeColor; }
            set { this.SetField(ref this._strokeColor, value); }
        }
        /// <summary>
        /// Gets/Sets the center position of the circle
        /// </summary>
        public Position Center
        {
            get { return this._center; }
            set { this.SetField(ref this._center, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the stroke
        /// </summary>
        public float StrokeWidth
        {
            get { return this._strokeWidth; }
            set { this.SetField(ref this._strokeWidth, value); }
        }
    }
}
