using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TK.CustomMap.Interfaces;
using TK.CustomMap.Overlays;
using TK.CustomMap.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// An extensions of the <see cref="Xamarin.Forms.Maps.Map"/>
    /// </summary>
    public class TKCustomMap : Map, IMapFunctions
    {
        private IRendererFunctions _renderer;

        /// <summary>
        /// Bindable Property of <see cref="CustomPins" />
        /// </summary>
        public static readonly BindableProperty CustomPinsProperty = 
            BindableProperty.Create<TKCustomMap, IEnumerable<TKCustomMapPin>>(
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
                default(Position),
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
        public static readonly BindableProperty PolylinesProperty =
            BindableProperty.Create<TKCustomMap, IEnumerable<TKPolyline>>(
                p => p.Polylines,
                null);
        /// <summary>
        /// Bindable Property of <see cref="Circles"/>
        /// </summary>
        public static readonly BindableProperty CirclesProperty =
            BindableProperty.Create<TKCustomMap, IEnumerable<TKCircle>>(
                p => p.Circles,
                null);
        /// <summary>
        /// Bindable Property of <see cref="CalloutClickedCommand"/>
        /// </summary>
        public static readonly BindableProperty CalloutClickedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command>(
                p => p.CalloutClickedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="Polygons"/>
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
                BindingMode.TwoWay,
                propertyChanged: MapRegionChanged);
        /// <summary>
        /// Bindable Property of <see cref="Routes"/>
        /// </summary>
        public static readonly BindableProperty RoutesProperty =
            BindableProperty.Create<TKCustomMap, IEnumerable<TKRoute>>(
                p => p.Routes,
                null);
        /// <summary>
        /// Bindable Property of <see cref="RouteClickedCommand"/>
        /// </summary>
        public static readonly BindableProperty RouteClickedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<TKRoute>>(
                p => p.RouteClickedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="RouteCalculationFinishedCommand"/>
        /// </summary>
        public static readonly BindableProperty RouteCalculationFinishedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<TKRoute>>(
                p => p.RouteCalculationFinishedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="RouteCalculationFailedCommand"/>
        /// </summary>
        public static readonly BindableProperty RouteCalculationFailedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<TKRoute>>(
                p => p.RouteCalculationFailedCommand,
                null);
        /// <summary>
        /// Bindable Property of <see cref="TilesUrlOptions"/>
        /// </summary>
        public static readonly BindableProperty TilesUrlOptionsProperty =
            BindableProperty.Create<TKCustomMap, TKTileUrlOptions>(
                p => p.TilesUrlOptions,
                null);
        /// <summary>
        /// Bindable Property of <see cref="UserLocationChangedCommand"/>
        /// </summary>
        public static readonly BindableProperty UserLocationChangedCommandProperty =
            BindableProperty.Create<TKCustomMap, Command<Position>>(
                p => p.UserLocationChangedCommand,
                null);
        /// <summary>
        /// Gets/Sets the custom pins of the Map
        /// </summary>
        public IEnumerable<TKCustomMapPin> CustomPins
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
        /// Gets/Sets the lines to display on the map
        /// </summary>
        public IEnumerable<TKPolyline> Polylines
        {
            get { return (IEnumerable<TKPolyline>)this.GetValue(PolylinesProperty); }
            set { this.SetValue(PolylinesProperty, value); }
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
        /// Gets/Sets the routes to calculate and display on the map
        /// </summary>
        public IEnumerable<TKRoute> Routes
        {
            get { return (IEnumerable<TKRoute>)this.GetValue(RoutesProperty); }
            set { this.SetValue(RoutesProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a route gets tapped
        /// </summary>
        public Command<TKRoute> RouteClickedCommand
        {
            get { return (Command<TKRoute>)this.GetValue(RouteClickedCommandProperty); }
            set { this.SetValue(RouteClickedCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a route calculation finished successfully
        /// </summary>
        public Command<TKRoute> RouteCalculationFinishedCommand
        {
            get { return (Command<TKRoute>)this.GetValue(RouteCalculationFinishedCommandProperty); }
            set { this.SetValue(RouteCalculationFinishedCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when a route calculation failed
        /// </summary>
        public Command<TKRoute> RouteCalculationFailedCommand
        {
            get { return (Command<TKRoute>)this.GetValue(RouteCalculationFailedCommandProperty); }
            set { this.SetValue(RouteCalculationFailedCommandProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the options for displaying custom tiles via an url
        /// </summary>
        public TKTileUrlOptions TilesUrlOptions
        {
            get { return (TKTileUrlOptions)this.GetValue(TilesUrlOptionsProperty); }
            set { this.SetValue(TilesUrlOptionsProperty, value); }
        }
        /// <summary>
        /// Gets/Sets the command when the user location changed
        /// </summary>
        public Command<Position> UserLocationChangedCommand
        {
            get { return (Command<Position>)this.GetValue(UserLocationChangedCommandProperty); }
            set { this.SetValue(UserLocationChangedCommandProperty, value); }
        }
        /// <summary>
        /// Creates a new instance of <c>TKCustomMap</c>
        /// </summary>
        public TKCustomMap() 
            : base() 
        { }
        /// <summary>
        /// Creates a new instance of <c>TKCustomMap</c>
        /// </summary>
        /// <param name="region">The initial region of the map</param>
        public TKCustomMap(MapSpan region)
            : base(region)
        {
            this.MapCenter = region.Center;
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
        /// <summary>
        /// Returns the currently visible map as a PNG image
        /// </summary>
        /// <returns>Map as image</returns>
        public async Task<byte[]> GetSnapshot()
        {
            return await this._renderer.GetSnapshot();
        }
        /// <summary>
        /// Fits the map region to make all given positions visible
        /// </summary>
        /// <param name="positions">Positions to fit inside the MapRegion</param>
        /// <param name="animate">If the camera change should be animated</param>
        public void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false)
        {
            this._renderer.FitMapRegionToPositions(positions, animate);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IMapFunctions.SetRenderer(IRendererFunctions renderer)
        {
            this._renderer = renderer;
        }
    }
}
