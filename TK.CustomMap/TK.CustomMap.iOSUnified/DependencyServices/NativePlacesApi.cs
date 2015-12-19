using CoreLocation;
using MapKit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Api;
using Xamarin.Forms.Maps;
using Foundation;
using TK.CustomMap.iOSUnified;

[assembly: Xamarin.Forms.Dependency(typeof(NativePlacesApi))]

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// iOS implementation of the <see cref="INativePlacesApi"/>
    /// </summary>
    public class NativePlacesApi : INativePlacesApi
    {
        /// <summary>
        /// Just to avoid linking
        /// </summary>
        [Preserve]
        public static void Init() { }
        ///<inheritdoc/>
        public void Connect()
        {
            // Nothing to do on iOS
            return;
        }
        ///<inheritdoc/>
        public void DisconnectAndRelease()
        {
            // Nothing to do on iOS
            return;
        }
        ///<inheritdoc/>
        public async Task<IEnumerable<IPlaceResult>> GetPredictions(string query, MapSpan bounds)
        {
            List<IPlaceResult> result = new List<IPlaceResult>();

            var region = new MKCoordinateRegion(bounds.Center.ToLocationCoordinate(), new MKCoordinateSpan(0.25, 0.25));

            var request = new MKLocalSearchRequest 
            {
                NaturalLanguageQuery = query,
                Region = region
            };

            MKLocalSearch search = new MKLocalSearch(request);
            var nativeResult = await search.StartAsync();

            if (nativeResult != null && nativeResult.MapItems != null)
            {
                result.AddRange(nativeResult.MapItems.Select(i =>
                    new TKNativeiOSPlaceResult
                    {
                        Description =  i.Placemark.Title,
                        Details = new TKPlaceDetails 
                        {
                            Coordinate = i.Placemark.Coordinate.ToPosition()
                        }
                    }));
                return result;
            }
            return null;
        }
        ///<inheritdoc/>
        public Task<TKPlaceDetails> GetDetails(string id)
        {
            throw new NotImplementedException("Not neccessary on iOS. Details already returned inside the prediction result set");
        }
    }
}
