using ModernHttpClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Api.OSM
{
    public class OsmNominatim
    {
        const string BaseUrl = "http://nominatim.openstreetmap.org/search/";

        private static OsmNominatim _instance;
        private readonly HttpClient _httpClient;

        public static OsmNominatim Instance
        {
            get
            {
                return _instance ?? (_instance = new OsmNominatim());
            }
        }

        public int Limit { get; set; }
        public Collection<string> CountryCodes { get; private set; }


        private OsmNominatim()
        {
            this._httpClient = new HttpClient(new NativeMessageHandler());
            this._httpClient.BaseAddress = new Uri(BaseUrl);

            this.Limit = 5;
            this.CountryCodes = new Collection<string>();
        }

        public async Task<IEnumerable<OsmNominatimResult>> GetPredictions(string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm)) return null;

            var result = await this._httpClient.GetAsync(this.BuildQueryString(searchTerm));

            if (result.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<IEnumerable<OsmNominatimResult>>(await result.Content.ReadAsStringAsync());
            }
            return null;
        }

        private string BuildQueryString(string searchTerm)
        {
            StringBuilder str = new StringBuilder();

            if (CountryCodes.Any())
            {
                str.AppendFormat("{0}/", string.Join(",", this.CountryCodes));
            }

            str.AppendFormat("{0}?", searchTerm);

            if (this.Limit > 0)
            {
                str.AppendFormat("{0}=1&", "limit", this.Limit);
            }
            str.AppendFormat("{0}={1}", "format", "json");

            return str.ToString();
        }
    }
}
