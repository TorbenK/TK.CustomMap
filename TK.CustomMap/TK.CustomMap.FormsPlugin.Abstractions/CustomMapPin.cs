using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// A custom map pin
    /// </summary>
    [ImplementPropertyChanged]
    public class TKCustomMapPin : INotifyPropertyChanged
    {
        public const string TitlePropertyName = "Title";
        public const string AddressPropertyName = "Address";
        public const string PositionPropertyName = "Position";
        public const string IconPropertyName = "Icon";
        public const string PinColorPropertyName = "PinColor";
        public const string IsVisiblePropertyName = "IsVisible";

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets/Sets visibility of a pin
        /// </summary>
        public bool IsVisible { get; set; }
        /// <summary>
        /// Gets/Sets title of the pin displayed in the callout
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets/Sets the subtitle of the pin displayed in the callout
        /// </summary>
        public string Subtitle { get; set; }
        /// <summary>
        /// Gets/Sets if the callout should be displayed when a pin gets selected
        /// </summary>
        public bool ShowCallout { get; set; }
        /// <summary>
        /// Gets/Sets the position of the pin
        /// </summary>
        public Position Position { get; set; }
        /// <summary>
        /// Gets/Sets the image of the pin. If null the default is used
        /// </summary>
        public ImageSource Image { get; set; }
        /// <summary>
        /// Gets/Sets if the pin is draggable
        /// </summary>
        public bool IsDraggable { get; set; }
    }
}
