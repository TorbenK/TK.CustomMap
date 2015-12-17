using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TK.CustomMap.Api
{
    /// <summary>
    /// Manages instance of <see cref="INativePlacesApi"/>
    /// </summary>
    public static class TKNativePlacesApi
    {
        private static INativePlacesApi _instance;

        /// <summary>
        /// Gets an instance of <see cref="INativePlacesApi"/>
        /// </summary>
        public static INativePlacesApi Instance
        {
            get
            {
                return _instance ?? (_instance = DependencyService.Get<INativePlacesApi>());
            }
        }
    }
}
