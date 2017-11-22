using System;
using Xamarin.Forms;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Base overlay class
    /// </summary>
    public abstract class TKOverlay : TKBase
    {
        public const string ColorPropertyName = "Color";

         Color _color;

        /// <summary>
        /// Gets/Sets the main color of the overlay.
        /// </summary>
        public Color Color 
        {
            get { return _color; }
            set { SetField(ref _color, value); }
        }
        /// <summary>
        /// Gets the id of the <see cref="TKOverlay"/>
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();
        /// <summary>
        /// Checks whether the <see cref="Id"/> of the overlays match
        /// </summary>
        /// <param name="obj">The <see cref="TKOverlay"/> to compare</param>
        /// <returns>true of the ids match</returns>
        public override bool Equals(object obj)
        {
            var overlay = obj as TKOverlay;

            if (overlay == null) return false;

            return Id.Equals(overlay.Id);
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
