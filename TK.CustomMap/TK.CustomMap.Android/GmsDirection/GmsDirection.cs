using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Droid.Api
{
    /// <summary>
    /// Calls the Google Maps Directions API to get a route
    /// </summary>
    public class GmsDirection
    {
        private static string _apiKey;

        private const string BaseUrl = "https://maps.googleapis.com/maps/api/directions/";
        /// <summary>
        /// Set the API key 
        /// </summary>
        /// <param name="apiKey">Google Maps API key</param>
        public static void Init(string apiKey)
        {
            _apiKey = apiKey;
        }
        /// <summary>
        /// Calculates a route
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="mode">The travelling mode</param>
        /// <param name="language">The language</param>
        /// <returns>A <see cref="GmsDirectionResult"/></returns>
        public static async Task<GmsDirectionResult> CalculateRoute(Position origin, Position destination, GmsDirectionTravelMode mode, string language = null)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(BuildQueryString(origin, destination, mode, language));
            webRequest.ContentType = "application/json";
            webRequest.Method = "GET";

            try
            {
                using (var response = await webRequest.GetResponseAsync())
                {
                    using (var strm = response.GetResponseStream())
                    {
                        return await Task.Run(() =>
                        {
                            using (var reader = new StreamReader(strm))
                            {
                                return JsonConvert.DeserializeObject<GmsDirectionResult>(reader.ReadToEnd());
                            }
                        });
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Builds the query string for the Google Maps Directions API call
        /// </summary>
        /// <param name="origin">The origin</param>
        /// <param name="destination">The destination</param>
        /// <param name="mode">The travelling mode</param>
        /// <param name="language">The language</param>
        /// <returns>The query string</returns>
        private static string BuildQueryString(Position origin, Position destination, GmsDirectionTravelMode mode, string language)
        {
            StringBuilder strBuilder = new StringBuilder(
                string.Format(
                    "{0}json?origin={1}&destination={2}&mode={3}", 
                    BaseUrl,
                    origin.AsString(),
                    destination.AsString(),
                    mode.ToString().ToLower()));

            if (!string.IsNullOrWhiteSpace(language))
            {
                strBuilder.AppendFormat("&language={0}", language);
            }
            strBuilder.AppendFormat("&key={0}", _apiKey);
            return strBuilder.ToString();
        }
    }
}
