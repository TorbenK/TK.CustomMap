using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;


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
        void FitMapRegionToPositions(IEnumerable<Position> positions, bool animate = false, int padding = 0);
        /// <summary>
        /// Moves the visible region to the specified <see cref="MapSpan"/>
        /// </summary>
        /// <param name="region">Region to move the map to</param>
        /// <param name="animate">If the region change should be animated or not</param>
        void MoveToMapRegion(MapSpan region, bool animate);
        /// <summary>
        /// Moves the visible region to the specified collection <see cref="MapSpan"/>
        /// </summary>
        /// <param name="regions">Regions to move the map to</param>
        /// <param name="animate">If the region change should be animated or not</param>
        void FitToMapRegions(IEnumerable<MapSpan> regions, bool animate = false, int padding = 0);
        /// <summary>
        /// Converts an array of <see cref="Point"/> into geocoordinates
        /// </summary>
        /// <param name="screenLocations">The screen locations(pixel)</param>
        /// <returns>A collection of <see cref="Position"/></returns>
        IEnumerable<Position> ScreenLocationsToGeocoordinates(params Point[] screenLocations);
    }
}
