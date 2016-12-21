using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Streams;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.UWP
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert <see cref="Position" /> to <see cref="Geopoint"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>UWP coordinate</returns>
        public static Geopoint ToLocationCoordinate(this Position self)
        {
            return new Geopoint(new BasicGeoposition
            {
                Latitude = self.Latitude,
                Longitude = self.Longitude
            });
        }

        /// <summary>
        /// Convert <see cref="MapSpan" /> to <see cref="Geopoint"/>
        /// </summary>
        /// <param name="self">Self instance</param>
        /// <returns>UWP coordinate</returns>
        public static Geopoint ToLocationCoordinate(this MapSpan self)
        {
            return new Geopoint(new BasicGeoposition
            {
                Latitude = self.LatitudeDegrees,
                Longitude = self.LongitudeDegrees
            });
        }

        public static Position ToPosition(this Geopoint self)
        {
            return new Position(self.Position.Latitude, self.Position.Longitude);
        }

        public static Position ToPosition(this BasicGeoposition self)
        {
            return new Position(self.Latitude, self.Longitude);
        }

        public static Windows.UI.Color ToUWPColor(this Color self)
        {
            return Windows.UI.Color.FromArgb(Convert.ToByte(self.A * 255), Convert.ToByte(self.R * 255), Convert.ToByte(self.G * 255), Convert.ToByte(self.B * 255));
        }

        public static double ToRadians(this double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double ToDegrees(this double radians)
        {
            return radians * (180 / Math.PI);
        }

        /// <summary>
        /// Convert a <see cref="ImageSource"/> to the native Android <see cref="Windows.UI.Xaml.Media.ImageSource"/>
        /// </summary>
        /// <param name="source">Self instance</param>
        /// <returns>The Bitmap</returns>
        public static async Task<RandomAccessStreamReference> ToUWPImageSource(this ImageSource source)
        {
            if (source is FileImageSource)
            {
                var fileName = ((FileImageSource)source).File;

                IStorageFile file = null;

                if (File.Exists(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, fileName)))
                    file = await StorageFile.GetFileFromPathAsync(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, fileName));
                else if (
                    File.Exists(Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets",
                        fileName)))
                    file =
                        await
                            StorageFile.GetFileFromPathAsync(
                                Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", fileName));

                if (file == null)
                    throw new FileNotFoundException($"{fileName} not found for map icon");

                return RandomAccessStreamReference.CreateFromFile(file);
            }
            if (source is UriImageSource)
            {
                return RandomAccessStreamReference.CreateFromUri(((UriImageSource)source).Uri);
            }
            if (source is StreamImageSource)
            {
                return RandomAccessStreamReference.CreateFromStream((await ((StreamImageSource)source).GetStreamAsync()).AsRandomAccessStream());
            }
            return null;
        }

        public static async Task<Stream> GetStreamAsync(this StreamImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageSource.Stream != null)
            {
                return await imageSource.Stream(cancellationToken);
            }
            return null;
        }

        public static Windows.Foundation.Point ToUWPPoint(this Point point)
        {
            return new Windows.Foundation.Point((int)point.X, (int)point.Y);
        }
    }
}