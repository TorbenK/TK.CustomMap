using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TK.CustomMap.Api;
using TK.CustomMap.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;

[assembly: Dependency(typeof(NativePlacesApi))]

namespace TK.CustomMap.UWP
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
            // Nothing to do on UWP
        }

        ///<inheritdoc/>
        public void DisconnectAndRelease()
        {
            // Nothing to do on UWP
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<IPlaceResult>> GetPredictions(string query, MapSpan bounds)
        {
            var result = new List<IPlaceResult>();

            var region = bounds.Center.ToLocationCoordinate();
            // TODO
            //var request = new MKLocalSearchRequest
            //{
            //    NaturalLanguageQuery = query,
            //    Region = region
            //};

            //MKLocalSearch search = new MKLocalSearch(request);
            //var nativeResult = await search.StartAsync();

            //if (nativeResult != null && nativeResult.MapItems != null)
            //{
            //    result.AddRange(nativeResult.MapItems.Select(i =>
            //        new TKNativeiOSPlaceResult
            //        {
            //            Description = string.Format("{0}, {1} {2}", i.Placemark.Title, i.Placemark.AdministrativeArea, i.Placemark.SubAdministrativeArea),
            //            Details = new TKPlaceDetails
            //            {
            //                Coordinate = i.Placemark.Coordinate.ToPosition(),
            //                FormattedAddress = i.Placemark.Title,
            //                InternationalPhoneNumber = i.PhoneNumber.ToString(),
            //                Website = i.Url.ToString()

            //            }
            //        }));
            //    return result;
            //}
            return null;
        }

        ///<inheritdoc/>
        public Task<TKPlaceDetails> GetDetails(string id)
        {
            // TODO is this true?
            throw new NotImplementedException("Not neccessary on UWP. Details already returned inside the prediction result set");
        }
    }
}