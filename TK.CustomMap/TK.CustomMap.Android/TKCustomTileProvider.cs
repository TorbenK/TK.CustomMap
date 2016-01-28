using Android.Gms.Maps.Model;
using Java.Net;

namespace TK.CustomMap.Droid
{
    /// <summary>
    /// Provides the map with custom tiles via an url
    /// </summary>
    public class TKCustomTileProvider : UrlTileProvider
    {
        private readonly string _url;
        /// <summary>
        /// Creates a new instance of <see cref="TKCustomTileProvider" />
        /// </summary>
        /// <param name="url">The url to fetch tiles from</param>
        public TKCustomTileProvider(string url) 
            : base(256, 256)
        {
            this._url = url;
        }
        /// <inheritdoc />
        public override URL GetTileUrl(int x, int y, int zoom)
        {
            return new URL(string.Format(this._url, x, y, zoom));
        }
    }
}