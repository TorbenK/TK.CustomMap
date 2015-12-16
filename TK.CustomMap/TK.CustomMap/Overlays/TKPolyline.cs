using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    public class TKPolyline : TKOverlay
    {
        public const string LineCoordinatesPropertyName = "LineCoordinates";
        public const string LineWidthProperty = "LineWidth";

        private List<Position> _lineCoordinates;
        private float _lineWidth;

        /// <summary>
        /// Coordinates of the line
        /// </summary>
        public List<Position> LineCoordinates
        {
            get { return this._lineCoordinates; }
            set { this.SetField(ref this._lineCoordinates, value); }
        }
        /// <summary>
        /// Gets/Sets the width of the line
        /// </summary>
        public float LineWidth
        {
            get { return this._lineWidth; }
            set { this.SetField(ref this._lineWidth, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKPolyline"/>
        /// </summary>
        public TKPolyline()
        {
            this._lineCoordinates = new List<Position>();
        }
    }
}
