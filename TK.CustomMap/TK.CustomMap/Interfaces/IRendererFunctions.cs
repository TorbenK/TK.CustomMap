using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Interfaces
{
    /// <summary>
    /// Containing all functions which are called from the PCL to the renderer directly
    /// </summary>
    public interface IRendererFunctions
    {
        /// <summary>
        /// Returns the current map as an image
        /// </summary>
        /// <returns>Image of the current map</returns>
        Task<byte[]> GetSnapshot();
        /// <summary>
        /// Moves the visible region of the map to cover all positions
        /// </summary>
        /// <param name="positions">The positions to fit the visible region</param>
        void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false);
    }
}
