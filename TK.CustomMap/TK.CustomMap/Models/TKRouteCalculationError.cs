using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Overlays;

namespace TK.CustomMap.Models
{
    /// <summary>
    /// Holds the <see cref="TKRoute"/> and the error message of the calculation failure
    /// </summary>
    public class TKRouteCalculationError
    {
        /// <summary>
        /// Gets the route
        /// </summary>
        public TKRoute Route { get; private set; }
        /// <summary>
        /// Gets the error message
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// Creates a new instance of <see cref="TKRouteCalculationError"/>
        /// </summary>
        /// <param name="route">The route</param>
        /// <param name="errorMessage">The error message</param>
        public TKRouteCalculationError(TKRoute route, string errorMessage)
        {
            this.Route = route;
            this.ErrorMessage = errorMessage;
        }
    }
}
