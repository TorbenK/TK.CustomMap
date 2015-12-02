using System;
using System.Collections.ObjectModel;
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
        /// Event is raised when all pins are created
        /// </summary>
        public event EventHandler PinsReady;
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
                null);
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
            get { return (TKCustomMapPin) this.GetValue(SelectedPinProperty); }
            set { this.SetValue(SelectedPinProperty, value); }
        }
    }
}
