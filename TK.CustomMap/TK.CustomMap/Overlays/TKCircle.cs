using Xamarin.Forms;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Displaying a circle on the map
    /// </summary>
    public class TKCircle : TKOverlay
    {
        public const string RadiusPropertyName = "Radius";
        public const string StrokeColorPropertyName = "StrokeColor";
        public const string CenterPropertyName = "Center";
        public const string StrokeWidthPropertyName = "StrokeWidth";

         double _radius;
         Color _strokeColor;
         Position _center;
         float _strokeWidth;
        /// <summary>
        /// Gets/Sets the radius of the circle
        /// </summary>
        public double Radius
        {
            get { return _radius; }
            set { SetField(ref _radius, value); }
        }
        /// <summary>
        /// Gets/Sets the stroke color of the circle
        /// </summary>
        public Color StrokeColor
        {
            get { return _strokeColor; }
            set { SetField(ref _strokeColor, value); }
        }
        /// <summary>
        /// Gets/Sets the center position of the circle
        /// </summary>
        public Position Center
        {
            get { return _center; }
            set { SetField(ref _center, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the stroke
        /// </summary>
        public float StrokeWidth
        {
            get { return _strokeWidth; }
            set { SetField(ref _strokeWidth, value); }
        }
    }
}
