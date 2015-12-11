using System.Collections.Generic;
using System.Collections.ObjectModel;
using TK.CustomMap.Overlays;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// An extensions of the <see cref="Xamarin.Forms.Maps.Map"/>
    /// </summary>
    public class TKCustomMap : Map
    {
        /// <summary>
        /// Bindable Property of <see cref="CustomPins" />
        /// </summary>
        public static readonly BindableProperty CustomPinsProperty = 
            BindableProperty.Create<TKCustomMap, ObservableCollection<TKCustomMapPin>>(
                p => p.CustomPins,
                null);
        /// <summary>
        /// Bindable Property of <see cref="SelectedPin" />
        /// </summary>
        public static readonly BindableProperty SelectedPinProperty = 
            BindableProperty.Create<TKCustomMap, TKCustomMapPin>(
                p => p.SelectedPin,
                null,
                BindingMode.TwoWay);
        /// <summary>
        /// Bindable Property of <see cref="PinSelectedCommand" />
        /// </summary>
        public static readonly BindableProperty PinSelectedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command>(
                p => p.PinSelectedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="MapClickedCommand"/>
        /// </summary>
        public static readonly BindableProperty MapClickedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<Position>>(
                p => p.MapClickedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="MapLongPressCommand"/>
        /// </summary>
        public static readonly BindableProperty MapLongPressCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<Position>>(
                p => p.MapLongPressCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="PinDragEndCommand"/>
        /// </summary>
        public static readonly BindableProperty PinDragEndCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<TKCustomMapPin>>(
                p => p.PinDragEndCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="PinsReadyCommand"/>
        /// </summary>
        public static readonly BindableProperty PinsReadyCommandProperty =
            BindableProperty.Create<TKCustomMap, Command>(
                p => p.PinsReadyCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="MapCenter"/>
        /// </summary>
        public static readonly BindableProperty MapCenterProperty =
            BindableProperty.Create<TKCustomMap, Position>(
                p => p.MapCenter,
                new Position(40.7142700, -74.0059700),
                BindingMode.TwoWay);
        /// <summary>
        /// Bindable Property of <see cref="AnimateMapCenterChange"/>
        /// </summary>
        public static readonly BindableProperty AnimateMapCenterChangeProperty =
            BindableProperty.Create<TKCustomMap, bool>(
                p => p.AnimateMapCenterChange,
                false);
        /// <summary>
        /// Bindable Property of <see cref="Routes"/>
        /// </summary>
        public static readonly BindableProperty RoutesProperty =
            BindableProperty.Create<TKCustomMap, IEnumerable<TKRoute>>(
                p => p.Routes,
                null);
        /// <summary>
        /// Bindable Property of <see cref="Circles"/>
        /// </summary>
        public static readonly BindableProperty CirclesProperty =
            BindableProperty.Create<TKCustomMap, IEnumerable<TKCircle>>(
                p => p.Circles,
                null);
        /// <summary>
        /// Bindable Property of <see cref="CustomInfoWindow"/>
        /// </summary>
        public static readonly BindableProperty CalloutClickedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command>(
                p => p.CalloutClickedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="Rectangles"/>
        /// </summary>
        public static readonly BindableProperty PolygonsProperty =
            BindableProperty.Create<TKCustomMap, IEnumerable<TKPolygon>>(
                p => p.Polygons,
                null);
        /// <summary>
        /// Bindable Property of <see cref="MapRegion"/>
        /// </summary>
        public static readonly BindableProperty MapRegionProperty =
            BindableProperty.Create<TKCustomMap, MapSpan>(
                p => p.MapRegion,
                default(MapSpan),
                propertyChanged: MapRegionChanged);
        /// <summary>
        /// Gets/Sets the custom pins of the Map
        /// </summary>
        public ObservableCollection<TKCustomMapPin> CustomPins
        {
            get { return (ObservableCollection<TKCustomMapPin>)this.GetValue(CustomPinsProperty); }
            set { this.SetValue(CustomPinsProperty, value); } 
        }
        /// <summary>
        /// Gets/Sets the currently selected pin on the map
        /// </summary>
        public TKCustomMapPin SelectedPin
        {
            get { return (TKCustomMapPin)this.GetValue(SelectedPinProperty); }
            set { this.SetValue(SelectedPinProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when the map was clicked/tapped
        /// </summary>
        public Command<Position> MapClickedCommand
        {
            get { return (Command<Position>)this.GetValue(MapClickedCommandProperty); }
            set { this.SetValue(MapClickedCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a long press was performed on the map
        /// </summary>
        public Command<Position> MapLongPressCommand
        {
            get { return (Command<Position>)this.GetValue(MapLongPressCommandProperty); }
            set { this.SetValue(MapLongPressCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a pin drag ended. The pin already has the updated position set
        /// </summary>
        public Command<TKCustomMapPin> PinDragEndCommand
        {
            get { return (Command<TKCustomMapPin>)this.GetValue(PinDragEndCommandProperty); }
            set { this.SetValue(PinDragEndCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a pin got selected
        /// </summary>
        public Command PinSelectedCommand
        {
            get { return (Command)this.GetValue(PinSelectedCommandProperty); }
            set { this.SetValue(PinSelectedCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when the pins are ready
        /// </summary>
        public Command PinsReadyCommand
        {
            get { return (Command)this.GetValue(PinsReadyCommandProperty); }
            set { this.SetValue(PinsReadyCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the current center of the map.
        /// </summary>
        public Position MapCenter
        {
            get { return (Position)this.GetValue(MapCenterProperty); }
            set { this.SetValue(MapCenterProperty, value); }
        }
        /// <summary>
        /// Gets/Sets if a change of <see cref="MapCenter"/> should be animated
        /// </summary>
        public bool AnimateMapCenterChange
        {
            get { return (bool)this.GetValue(AnimateMapCenterChangeProperty); }
            set { this.SetValue(AnimateMapCenterChangeProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the routes to display on the map
        /// </summary>
        public IEnumerable<TKRoute> Routes
        {
            get { return (IEnumerable<TKRoute>)this.GetValue(RoutesProperty); }
            set { this.SetValue(RoutesProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the circles to display on the map
        /// </summary>
        public IEnumerable<TKCircle> Circles
        {
            get { return (IEnumerable<TKCircle>)this.GetValue(CirclesProperty); }
            set { this.SetValue(CirclesProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a callout gets clicked. When this is set, there will be an accessory button visible inside the callout on iOS.
        /// Android will simply raise the command by clicking anywhere inside the callout, since Android simply renders a bitmap
        /// </summary>
        public Command CalloutClickedCommand
        {
            get { return (Command)this.GetValue(CalloutClickedCommandProperty); }
            set { this.SetValue(CalloutClickedCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the rectangles to display on the map
        /// </summary>
        public IEnumerable<TKPolygon> Polygons
        {
            get { return (IEnumerable<TKPolygon>)this.GetValue(PolygonsProperty); }
            set { this.SetValue(PolygonsProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the visible map region
        /// </summary>
        public MapSpan MapRegion
        {
            get { return (MapSpan)this.GetValue(MapRegionProperty); }
            set { this.SetValue(MapRegionProperty, value); }
        }
        /// <summary>
        /// When <see cref="MapRegion"/> changed
        /// </summary>
        /// <param name="obj">The custom map</param>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        private static void MapRegionChanged(BindableObject obj, MapSpan oldValue, MapSpan newValue)
        {
            var customMap = obj as TKCustomMap;
            if (customMap == null) return;

            if (!customMap.MapRegion.Equals(customMap.VisibleRegion))
            {
                customMap.MoveToRegion(customMap.MapRegion);
            }
        }
        /// <inheritdoc/>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "VisibleRegion")
            {
                this.MapRegion = this.VisibleRegion;
            }
        }
    }
}
