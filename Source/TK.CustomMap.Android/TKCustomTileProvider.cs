using Android.Gms.Maps.Model;
using Java.Net;
using TK.CustomMap.Overlays;

namespace TK.CustomMap.Droid
{
    /// <summary>
    /// Provides the map with custom tiles via an url
    /// </summary>
    public class TKCustomTileProvider : UrlTileProvider
    {
         readonly TKTileUrlOptions _options;
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomTileProvider" />
        /// </summary>
        /// <param name="options">Options of the tiles</param>
        public TKCustomTileProvider(TKTileUrlOptions options) 
            : base(options.TileWidth, options.TileHeight)
        {
            _options = options;
        }
        /// <inheritdoc />
        public override URL GetTileUrl(int x, int y, int zoom)
        {
            if (CheckTileExists(zoom))
            {
                return new URL(string.Format(_options.TilesUrl, x, y, zoom));
            }
            return null;
        }
        /// <summary>
        /// Check if the tile is available in the specified zoom
        /// </summary>
        /// <param name="zoom">The zoom to request the tile</param>
        /// <returns><value>False</value> if tile in the specified zoom is not available</returns>
         bool CheckTileExists(int zoom)
        {
            return !(zoom > _options.MaximumZoomLevel || zoom < _options.MinimumZoomLevel);
        }
    }
}