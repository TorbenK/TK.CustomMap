using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// Tile Url Options
    /// </summary>
    public class TKTileUrlOptions 
    {
        /// <summary>
        /// Gets the width of a tile image
        /// </summary>
        public int TileWidth { get; private set; }
        /// <summary>
        /// Gets the height of a tile image
        /// </summary>
        public int TileHeight { get; private set; }
        /// <summary>
        /// Gets the url for custom map tiles.  
        /// <note type="note">Url must specify 3 placeholders({0}, {1}, {2}) which are used for providing x, y and zoom.</note>
        /// </summary>
        public string TilesUrl { get; private set; }
        /// <summary>
        /// Creates a new instance of <see cref="TKTileUrlOptions"/>
        /// </summary>
        /// <param name="tileWidth">Width of a tile image</param>
        /// <param name="tileHeight">Height of a tile image</param>
        /// <param name="tilesUrl">The url for custom map tiles. 
        /// <note type="note">Url must specify 3 placeholders({0}, {1}, {2}) which are used for providing x, y and zoom.</note>
        /// </param>
        public TKTileUrlOptions(int tileWidth, int tileHeight, string tilesUrl)
        {
            this.TileWidth = tileWidth;
            this.TileHeight = tileHeight;
            this.TilesUrl = tilesUrl;
        }
    }
}
