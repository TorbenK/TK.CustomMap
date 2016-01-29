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
        private readonly TKTileUrlOptions _options;
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomTileProvider" />
        /// </summary>
        /// <param name="url">The url to fetch tiles from</param>
        public TKCustomTileProvider(TKTileUrlOptions options) 
            : base(options.TileWidth, options.TileHeight)
        {
            this._options = options;
        }
        /// <inheritdoc />
        public override URL GetTileUrl(int x, int y, int zoom)
        {
            if (this.CheckTileExists(zoom))
            {
                return new URL(string.Format(this._options.TilesUrl, x, y, zoom));
            }
            return null;
        }
        /// <summary>
        /// Check if the tile is available in the specified zoom
        /// </summary>
        /// <param name="zoom">The zoom to request the tile</param>
        /// <returns><value>False</value> if tile in the specified zoom is not available</returns>
        private bool CheckTileExists(int zoom)
        {
            return !(zoom > this._options.MaximumZoomLevel || zoom < this._options.MinimumZoomLevel);
        }
    }
}