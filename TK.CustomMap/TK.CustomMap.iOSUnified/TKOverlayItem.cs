using MapKit;
using TK.CustomMap.Overlays;

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// Holds the forms instance, the native overlay and the corresponding renderer
    /// </summary>
    internal class TKOverlayItem<TOverlay, TRenderer> 
        where TOverlay: TKOverlay
        where TRenderer: MKOverlayPathRenderer
    {
        /// <summary>
        /// Gets/Sets the <see cref="TKOverlay"/>
        /// </summary>
        public TOverlay Overlay { get; set; }
        /// <summary>
        /// Gets/Sets the overlay renderer
        /// </summary>
        public TRenderer Renderer { get; set; }

        public TKOverlayItem(TOverlay overlay)
        {
            this.Overlay = overlay;
        }

    }
}
