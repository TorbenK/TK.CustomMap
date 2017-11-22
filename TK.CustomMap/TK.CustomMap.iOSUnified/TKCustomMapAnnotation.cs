using System;
using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// Custom map annotation
    /// </summary>
    [Preserve(AllMembers = true)]
    internal class TKCustomMapAnnotation : MKAnnotation
    {
        CLLocationCoordinate2D _coordinate;
         readonly TKCustomMapPin _formsPin;

        ///<inheritdoc/>
        public override string Title
        {
            get
            {
                return _formsPin.Title;
            }
        }
        ///<inheritdoc/>
        public override string Subtitle
        {
            get
            {
                return _formsPin.Subtitle;
            }
        }
        ///<inheritdoc/>
        public override CLLocationCoordinate2D Coordinate
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
        public override void SetCoordinate(CLLocationCoordinate2D value)
        {
            if (!value.IsValid()) return;
            
            _formsPin.Position = value.ToPosition();
            _coordinate = value;
        }
        /// <summary>
        /// xamarin.ios does (still) not export <value>_original_setcoordinate</value>
        /// </summary>
        /// <param name="value">the coordinate</param>
        [Export("_original_setCoordinate:")]
        public void SetCoordinateOriginal(CLLocationCoordinate2D value)
        {
            SetCoordinate(value);
        }
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomMapAnnotation"/>
        /// </summary>
        /// <param name="pin">The forms pin</param>
        public TKCustomMapAnnotation(TKCustomMapPin pin)
        {
            _formsPin = pin;
            _coordinate = pin.Position.ToLocationCoordinate();
            _formsPin.PropertyChanged += FormsPinPropertyChanged;
        }
        /// <summary>
        /// Forwards to <see cref="SetCoordinate(CLLocationCoordinate2D)"/> while only triggering the observer if <paramref name="triggerObserver"/> is true
        /// </summary>
        /// <param name="value">The coordinate</param>
        /// <param name="triggerObserver">True to trigger the observer</param>
        internal void SetCoordinateInternal(CLLocationCoordinate2D value, bool triggerObserver)
        {
            if(triggerObserver)
                WillChangeValue("coordinate");
            SetCoordinate(value);
            if (triggerObserver)
                DidChangeValue("coordinate");
        }
        /// <summary>
        /// When property of the custom pin changes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        void FormsPinPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TKCustomMapPin.SubititlePropertyName)
            {
                WillChangeValue("subtitle");
                DidChangeValue("subtitle");
            }
            if (e.PropertyName == TKCustomMapPin.TitlePropertyName)
            {
                WillChangeValue("title");
                DidChangeValue("title");
            }
        }
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _formsPin.PropertyChanged -= FormsPinPropertyChanged;

            
        }
    }
}
