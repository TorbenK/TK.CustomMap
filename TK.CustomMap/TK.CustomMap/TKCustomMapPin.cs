using System;
using Xamarin.Forms;

namespace TK.CustomMap
{
    /// <summary>
    /// A custom map pin
    /// </summary>
    public class TKCustomMapPin : TKBase
    {
         bool _isVisible;
         string _id;
         string _title;
         string _subtitle;
         string _group;
         bool _showCallout;
         Position _position;
         ImageSource _image;
         bool _isDraggable;
         Color _defaultPinColor;
         Point _anchor = new Point(0.5, 0.5);
         double _rotation;
         bool _isCalloutClickable;

        /// <summary>
        /// Gets the id of the <see cref="TKCustomMapPin"/>
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        /// <summary>
        /// Gets/Sets visibility of a pin
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetField(ref _isVisible, value); }
        }
        /// <summary>
        /// Gets/Sets ID of the pin, used for client app reference (optional)
        /// </summary>
        public string ID
        {
            get { return _id; }
            set { SetField(ref _id, value); }
        }
        /// <summary>
        /// Gets/Sets title of the pin displayed in the callout
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value); }
        }
        /// <summary>
        /// Gets/Sets the subtitle of the pin displayed in the callout
        /// </summary>
        public string Subtitle
        {
            get { return _subtitle; }
            set { SetField(ref _subtitle, value); }
        }
        /// <summary>
        /// Gets/Sets if the callout should be displayed when a pin gets selected
        /// </summary>
        public bool ShowCallout
        {
            get { return _showCallout; }
            set { SetField(ref _showCallout, value); }
        }
        /// <summary>
        /// Gets/Sets the position of the pin
        /// </summary>
        public Position Position
        {
            get { return _position; }
            set { SetField(ref _position, value); }
        }
        /// <summary>
        /// Gets/Sets the image of the pin. If null the default is used
        /// </summary>
        public ImageSource Image
        {
            get { return _image; }
            set { SetField(ref _image, value); }
        }
        /// <summary>
        /// Gets/Sets if the pin is draggable
        /// </summary>
        public bool IsDraggable
        {
            get { return _isDraggable; }
            set { SetField(ref _isDraggable, value); }
        }
        /// <summary>
        /// Gets/Sets the color of the default pin. Only applies when no <see cref="Image"/> is set
        /// </summary>
        public Color DefaultPinColor
        {
            get { return _defaultPinColor; }
            set { SetField(ref _defaultPinColor, value); }
        }
        /// <summary>
        /// Gets/Sets the anchor point of the pin when using a custom pin image
        /// </summary>
        public Point Anchor
        {
            get { return _anchor; }
            set { SetField(ref _anchor, value); }
        }
        /// <summary>
        /// Gets/Sets the rotation angle of the pin in degrees
        /// </summary>
        public double Rotation
        {
            get { return _rotation; }
            set { SetField(ref _rotation, value); }
        }
        /// <summary>
        /// Gets/Sets whether the callout is clickable or not. This adds/removes the accessory control on iOS
        /// </summary>
        public bool IsCalloutClickable
        {
            get { return _isCalloutClickable; }
            set { SetField(ref _isCalloutClickable, value); }
        }
        /// <summary>
        /// Gets/Sets the group identifier
        /// </summary>
        public string Group
        {
            get => _group;
            set { SetField(ref _group, value); }
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomMapPin" />
        /// </summary>
        public TKCustomMapPin()
        {
            IsVisible = true;
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
