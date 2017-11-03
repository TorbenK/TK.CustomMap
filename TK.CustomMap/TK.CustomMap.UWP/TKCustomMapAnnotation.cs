using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Xamarin.Forms.Internals;

namespace TK.CustomMap.UWP
{
    // TODO needed? Or just add MapIcons directly?
    /// <summary>
    /// Custom map annotation
    /// </summary>
    [Preserve(AllMembers = true)]
    internal class TKCustomMapAnnotation : MapElement
    {
        private Geopoint _coordinate;
        private readonly TKCustomMapPin _formsPin;

        ///<inheritdoc/>
        public string Title
        {
            get
            {
                return _formsPin.Title;
            }
        }

        ///<inheritdoc/>
        public string Subtitle
        {
            get
            {
                return _formsPin.Subtitle;
            }
        }

        ///<inheritdoc/>
        public Geopoint Coordinate
        {
            get { return _coordinate; }
        }

        /// <summary>
        /// Gets the forms pin
        /// </summary>
        public TKCustomMapPin CustomPin
        {
            get { return _formsPin; }
        }

        ///<inheritdoc/>
        public void SetCoordinate(Geopoint value)
        {
            //WillChangeValue("coordinate");
            _coordinate = value;
            //DidChangeValue("coordinate");
        }

        public MapIcon MapIcon { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="TKCustomMapAnnotation"/>
        /// </summary>
        /// <param name="pin">The forms pin</param>
        public TKCustomMapAnnotation(TKCustomMapPin pin)
        {
            _formsPin = pin;
            _coordinate = pin.Position.ToLocationCoordinate();
            _formsPin.PropertyChanged += formsPin_PropertyChanged;
            
            MapIcon = new MapIcon
            {
                Title = Title,
                //mapIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///pin.png"));
                CollisionBehaviorDesired = MapElementCollisionBehavior.RemainVisible,
                Location = Coordinate,
                NormalizedAnchorPoint = new Windows.Foundation.Point(0.5, 1.0)
            };
        }
        
        private void formsPin_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TKCustomMapPin.SubititlePropertyName)
            {
                OnPropertyChanged(nameof(TKCustomMapPin.SubititlePropertyName));
            }
            if (e.PropertyName == TKCustomMapPin.TitlePropertyName)
            {
                OnPropertyChanged(nameof(TKCustomMapPin.TitlePropertyName));
            }
        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    if (disposing)
        //        this._formsPin.PropertyChanged -= formsPin_PropertyChanged;

        //}
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}