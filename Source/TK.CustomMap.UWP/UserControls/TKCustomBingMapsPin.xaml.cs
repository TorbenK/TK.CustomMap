using System;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace TK.CustomMap.UWP
{
    /// <summary>
    /// Defines the <see cref="UserControl"/> used to display a pin on the map
    /// </summary>
    public sealed partial class TKCustomBingMapsPin : UserControl
    {
        /// <summary>
        /// When the dragging completed
        /// </summary>
        public event EventHandler<TKGenericEventArgs<TKCustomMapPin>> DragEnd;
        /// <summary>
        /// When the pin was tapped
        /// </summary>
        public event EventHandler<TKGenericEventArgs<TKCustomMapPin>> Clicked;

        private TKCustomMapPin _formsPin;

        private MapControl _map;
        private bool _isDragging = false;
        private ImageSource _imageSource;
        private Color _defaultPinColor;

        private DispatcherTimer _dragStartTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };

        /// <summary>
        /// Gets title to display in the callout
        /// </summary>
        public string Title => _formsPin.Title;
        /// <summary>
        /// Gets the subtitle to display in the callout
        /// </summary>
        public string Subtitle => _formsPin.Subtitle;
        /// <summary>
        /// Gets/Sets a custom image to display as pin
        /// </summary>
        public ImageSource Image
        {
            get { return _imageSource; }
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;

                    if (_imageSource != null)
                    {
                        pinImage.Source = _imageSource;
                        pinImage.Visibility = Visibility.Visible;

                        defaultPinEllipse.Visibility = Visibility.Collapsed;
                    }
                    else
                    {


                        pinImage.Visibility = Visibility.Collapsed;
                        defaultPinEllipse.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        /// <summary>
        /// Gets/Sets the default pin color to use when no <see cref="Image"/> is defined
        /// </summary>
        public Color DefaultPinColor
        {
            get { return _defaultPinColor; }
            set
            {
                _defaultPinColor = value;

                if (_defaultPinColor != null)
                {
                    defaultPinEllipse.Fill = new SolidColorBrush(_defaultPinColor);
                }
                else
                {
                    defaultPinEllipse.Fill = new SolidColorBrush(Colors.Blue);
                }
            }
        }
        /// <summary>
        /// Gets whether the pin is draggable
        /// </summary>
        public bool IsDraggable => _formsPin.IsDraggable;
        /// <summary>
        /// Gets whether the pin is currently beeing dragged
        /// </summary>
        public bool IsDragging => _isDragging;
        /// <summary>
        /// Gets whether the callout should shown
        /// </summary>
        public bool ShowCallout => _formsPin.ShowCallout;
        /// <summary>
        /// Gets whether the callout is clickable
        /// </summary>
        public bool IsCalloutClickable => _formsPin.IsCalloutClickable;
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomBingMapsPin"/>
        /// </summary>
        /// <param name="pin">The <see cref="TKCustomMapPin"/></param>
        /// <param name="map">The <see cref="MapControl"/></param>
        public TKCustomBingMapsPin(TKCustomMapPin pin, MapControl map)
        {
            _formsPin = pin;
            _map = map;
            InitializeComponent();

            _dragStartTimer.Tick += (o, e) => 
            {
                StartDragging();
                _dragStartTimer.Stop();
            };

            Initialize();
        }
        /// <summary>
        /// Wenn der Pin getapped wird
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            _dragStartTimer.Stop();

            OnClicked();
            base.OnTapped(e);
        }
        /// <summary>
        /// Raises <see cref="Clicked"/>
        /// </summary>
        private void OnClicked()
        {
            Clicked?.Invoke(this, new TKGenericEventArgs<TKCustomMapPin>(_formsPin));
        }
        /// <summary>
        /// Raises <see cref="DragEnd"/>
        /// </summary>
        private void OnDragEnd()
        {
            DragEnd?.Invoke(this, new TKGenericEventArgs<TKCustomMapPin>(_formsPin));
        }
        /// <summary>
        /// When the pin is pressed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (IsDraggable)
            {
                _dragStartTimer.Start();
            }
        }
        protected override void OnPointerCanceled(PointerRoutedEventArgs e)
        {
            _dragStartTimer.Stop();
            base.OnPointerCanceled(e);
        }
        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            _dragStartTimer.Stop();
            base.OnPointerCaptureLost(e);
        }
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            _dragStartTimer.Stop();
            base.OnPointerReleased(e);
        }
        /// <summary>
        /// When the pointer leaves the map while pressing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapPointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            _dragStartTimer.Stop();

            if (_isDragging)
            {
                EndDragging(e);
            }
        }
        /// <summary>
        /// When the pointer is released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _dragStartTimer.Stop();

            if (_isDragging)
            {
                EndDragging(e);
            }
        }
        /// <summary>
        /// When the pointer is moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isDragging)
            {
                var pointerPosition = e.GetCurrentPoint(_map);

                Geopoint location;
                _map.GetLocationFromOffset(pointerPosition.Position, out location);

                MapControl.SetLocation(this, location);
                _formsPin.Position = location.ToPosition();
            }
        }
        /// <summary>
        /// Starts the dragging
        /// </summary>
        private void StartDragging()
        {
            if (_map != null)
            {
                _map.PanInteractionMode = MapPanInteractionMode.Disabled;

                _map.PointerReleased += MapPointerReleased;
                _map.PointerMoved += MapPointerMoved;
                _map.PointerCanceled += MapPointerCanceled;
                _map.PointerCaptureLost += _map_PointerCaptureLost;
            }
            _isDragging = true;
        }

        private void _map_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            _dragStartTimer.Stop();
        }

        /// <summary>
        /// Sets the rotation transformation. Only if a custom image is applied
        /// </summary>
        /// <param name="angle">The angle in degrees(clock-wise)</param>
        public void SetRotation(double angle)
        {
            if (_imageSource != null)
            {
                pinImage.RenderTransform = new RotateTransform { Angle = angle };
            }
        }
        /// <summary>
        /// Sets the anchor point
        /// </summary>
        /// <param name="x">X anchor point(0-1)</param>
        /// <param name="y">Y anchor point(0-1)</param>
        public void SetAnchor(double x, double y)
        {
            if (_imageSource != null)
            {
                PinGrid.Margin = new Thickness(
                    x > 0 ? (PinGrid.RenderSize.Width - (PinGrid.RenderSize.Width * x)) * -1 : 0,
                    y > 0 ? (PinGrid.RenderSize.Height - (PinGrid.RenderSize.Height * y)) * -1 : 0,
                    0,
                    0);
            }
        }
        /// <summary>
        /// Start/Stop observing the <see cref="TKCustomMapPin"/>
        /// </summary>
        /// <param name="start">True to start</param>
        internal void Observe(bool start)
        {
            if (start)
            {
                _formsPin.PropertyChanged += FormsPinPropertyChanged;
            }
            else
            {
                _formsPin.PropertyChanged -= FormsPinPropertyChanged;
            }
        }
        /// <summary>
        /// When a property of the <see cref="TKCustomMapPin"/> changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FormsPinPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_formsPin.Image):
                    Image = await _formsPin.Image.ToUWPImageSource();
                    break;
                case nameof(_formsPin.Position):
                    if (!_isDragging)
                        MapControl.SetLocation(this, _formsPin.Position.ToLocationCoordinate());
                    break;
                case nameof(_formsPin.IsVisible):
                    Visibility = _formsPin.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case nameof(_formsPin.Rotation):
                    SetRotation(_formsPin.Rotation);
                    break;
                case nameof(_formsPin.Anchor):
                    SetAnchor(_formsPin.Anchor.X, _formsPin.Anchor.Y);
                    break;
                case nameof(_formsPin.DefaultPinColor):
                    if (_formsPin.DefaultPinColor == Xamarin.Forms.Color.Default)
                        DefaultPinColor = Colors.Blue;
                    else
                        DefaultPinColor = _formsPin.DefaultPinColor.ToUWPColor();
                    break;
            }
        }
        /// <summary>
        /// Initializes the pin
        /// </summary>
        private async void Initialize()
        {
            if (_formsPin.Image != null)
                Image = await _formsPin.Image.ToUWPImageSource();

            MapControl.SetLocation(this, _formsPin.Position.ToLocationCoordinate());
            Visibility = _formsPin.IsVisible ? Visibility.Visible : Visibility.Collapsed;

            if (_formsPin.Rotation > 0)
                SetRotation(_formsPin.Rotation);

            SetAnchor(_formsPin.Anchor.X, _formsPin.Anchor.Y);

            if (_formsPin.DefaultPinColor == Xamarin.Forms.Color.Default)
                DefaultPinColor = Colors.Blue;
            else
                DefaultPinColor = _formsPin.DefaultPinColor.ToUWPColor();
        }

        /// <summary>
        /// Ends the dragging
        /// </summary>
        /// <param name="e">The <see cref="PointerRoutedEventArgs"/></param>
        private void EndDragging(PointerRoutedEventArgs e)
        {

            _map.PanInteractionMode = MapPanInteractionMode.Auto;

            if (_map != null)
            {
                _map.PointerReleased -= MapPointerReleased;
                _map.PointerMoved -= MapPointerMoved;
                _map.PointerCanceled -= MapPointerCanceled;
                _map.PointerCaptureLost -= _map_PointerCaptureLost;
            }

            var pointerPosition = e.GetCurrentPoint(_map);

            Geopoint location;

            _map.GetLocationFromOffset(pointerPosition.Position, out location);

            MapControl.SetLocation(this, location);

            _isDragging = false;

            OnDragEnd();

        }
    }
}
