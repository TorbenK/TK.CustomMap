using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
    }
}
