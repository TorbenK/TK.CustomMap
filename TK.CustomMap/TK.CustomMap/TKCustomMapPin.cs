using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// A custom map pin
    /// </summary>
    public class TKCustomMapPin : TKBase
    {
        private bool _isVisible;
        private string _title;
        private string _subtitle;
        private bool _showCallout;
        private Position _position;
        private ImageSource _image;
        private bool _isDraggable;

        public const string TitlePropertyName = "Title";
        public const string SubititlePropertyName = "Subtitle";
        public const string PositionPropertyName = "Position";
        public const string ImagePropertyName = "Image";
        public const string IsVisiblePropertyName = "IsVisible";
        public const string IsDraggablePropertyName = "IsDraggable";
        public const string ShowCalloutPropertyName = "ShowCallout";

        /// <summary>
        /// Gets/Sets visibility of a pin
        /// </summary>
        public bool IsVisible 
        {
            get { return this._isVisible; }
            set { this.SetField(ref this._isVisible, value); }
        }
        /// <summary>
        /// Gets/Sets title of the pin displayed in the callout
        /// </summary>
        public string Title 
        {
            get { return this._title; }
            set { this.SetField(ref this._title, value); }
        }
        /// <summary>
        /// Gets/Sets the subtitle of the pin displayed in the callout
        /// </summary>
        public string Subtitle 
        {
            get { return this._subtitle; }
            set { this.SetField(ref this._subtitle, value); }
        }
        /// <summary>
        /// Gets/Sets if the callout should be displayed when a pin gets selected
        /// </summary>
        public bool ShowCallout 
        {
            get { return this._showCallout; }
            set { this.SetField(ref this._showCallout, value); }
        }
        /// <summary>
        /// Gets/Sets the position of the pin
        /// </summary>
        public Position Position 
        {
            get { return this._position; }
            set { this.SetField(ref this._position, value); }
        }
        /// <summary>
        /// Gets/Sets the image of the pin. If null the default is used
        /// </summary>
        public ImageSource Image 
        {
            get { return this._image; }
            set { this.SetField(ref this._image, value); }
        }
        /// <summary>
        /// Gets/Sets if the pin is draggable
        /// </summary>
        public bool IsDraggable 
        {
            get { return this._isDraggable; }
            set { this.SetField(ref this._isDraggable, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomMapPin" />
        /// </summary>
        public TKCustomMapPin()
        {
            this.IsVisible = true;
        }
    }
}
