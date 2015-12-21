using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Api
{
    /// <summary>
    /// Places search by using the recommended native API on each platform
    /// Android: <value>Google Maps Places API for Android</value>
    /// iOS: <value>MKLocalSearch</value>
    /// </summary>
    public interface INativePlacesApi
    {
        /// <summary>
        /// Connect to the API. Only required on <value>Android</value>
        /// </summary>
        void Connect();
        /// <summary>
        /// Disonnect from the API and release resources. Only required on <value>Android</value>
        /// </summary>
        void DisconnectAndRelease();
        /// <summary>
        /// Gets the place predictions by the search query
        /// </summary>
        /// <param name="query">The query to search for places</param>
        /// <returns>Collection of <see cref="IPlaceResult"/></returns>
        Task<IEnumerable<IPlaceResult>> GetPredictions(string query, MapSpan bounds);
        /// <summary>
        /// Gets place details by the place id. Only required on <value>Android</value> as <value>iOS</value> returns the details already 
        /// in the <see cref="GetPredictions"/>
        /// </summary>
        /// <param name="id">Place Id</param>
        /// <returns>Place Details</returns>
        Task<TKPlaceDetails> GetDetails(string id);
    }
}
