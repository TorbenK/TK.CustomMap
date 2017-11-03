using System;
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
        private string _id;
        private string _title;
        private string _subtitle;
        private bool _showCallout;
        private Position _position;
        private ImageSource _image;
        private bool _isDraggable;
        private Color _defaultPinColor;
        private Point _anchor;
        private double _rotation;
        private bool _isCalloutClickable;

        public const string IDPropertyName = "ID";
        public const string TitlePropertyName = "Title";
        public const string SubititlePropertyName = "Subtitle";
        public const string PositionPropertyName = "Position";
        public const string ImagePropertyName = "Image";
        public const string IsVisiblePropertyName = "IsVisible";
        public const string IsDraggablePropertyName = "IsDraggable";
        public const string ShowCalloutPropertyName = "ShowCallout";
        public const string DefaultPinColorPropertyName = "DefaultPinColor";
        public const string AnchorPropertyName = "Anchor";
        public const string RotationPropertyName = "Rotation";
        public const string IsCalloutClickablePropertyName = "IsCalloutClickable";

        /// <summary>
        /// Gets the id of the <see cref="TKCustomMapPin"/>
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        /// <summary>
        /// Gets/Sets visibility of a pin
        /// </summary>
        public bool IsVisible 
        {
            get { return this._isVisible; }
            set { this.SetField(ref this._isVisible, value); }
        }
        /// <summary>
        /// Gets/Sets ID of the pin, used for client app reference (optional)
        /// </summary>
        public string ID
        {
            get { return this._id; }
            set { this.SetField(ref this._id, value); }
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
        /// Gets/Sets the color of the default pin. Only applies when no <see cref="Image"/> is set
        /// </summary>
        public Color DefaultPinColor
        {
            get { return this._defaultPinColor; }
            set { this.SetField(ref this._defaultPinColor, value); }
        }
        /// <summary>
        /// Gets/Sets the anchor point of the pin when using a custom pin image
        /// </summary>
        public Point Anchor
        {
            get { return this._anchor; }
            set { this.SetField(ref this._anchor, value); }
        }
        /// <summary>
        /// Gets/Sets the rotation angle of the pin in degrees
        /// </summary>
        public double Rotation
        {
            get { return this._rotation; }
            set { this.SetField(ref this._rotation, value); }
        }
        /// <summary>
        /// Gets/Sets whether the callout is clickable or not. This adds/removes the accessory control on iOS
        /// </summary>
        public bool IsCalloutClickable
        {
            get { return this._isCalloutClickable; }
            set { this.SetField(ref this._isCalloutClickable, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomMapPin" />
        /// </summary>
        public TKCustomMapPin()
        {
            this.IsVisible = true;
        }

        /// <summary>
        /// Checks whether the <see cref="Id"/> of the pins match
        /// </summary>
        /// <param name="obj">The <see cref="TKCustomMapPin"/> to compare</param>
        /// <returns>true of the ids match</returns>
        public override bool Equals(object obj)
        {
            var pin = obj as TKCustomMapPin;

            if (pin == null) return false;

            return Id.Equals(pin.Id);
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
