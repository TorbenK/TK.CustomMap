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
            return new URL(string.Format(this._options.TilesUrl, x, y, zoom));
        }
    }
}