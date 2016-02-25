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
        /// Gets the minimum zoom level supported by the tiles
        /// </summary>
        public int MinimumZoomLevel { get; private set; }
        /// <summary>
        /// Gets the maximum zoom level supported by the tiles
        /// </summary>
        public int MaximumZoomLevel { get; private set; }
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
        /// <param name="minZoomLevel">The minimum zoom level supported by the tiles</param>
        /// <param name="maxZoomLevel">The maximum zoom level supported by the tiles</param>
        /// </param>
        public TKTileUrlOptions(string tilesUrl, int tileWidth, int tileHeight, int minZoomLevel, int maxZoomLevel)
        {
            this.TileWidth = tileWidth;
            this.TileHeight = tileHeight;
            this.TilesUrl = tilesUrl;
            this.MinimumZoomLevel = minZoomLevel;
            this.MaximumZoomLevel = maxZoomLevel;
        }
    }
}
