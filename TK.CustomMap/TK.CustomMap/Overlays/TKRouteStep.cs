using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A Step of a route
    /// </summary>
    public sealed class TKRouteStep : TKBase, IRouteStepFunctions
    {
        private double _distance;
        private string _instructions;

        /// <summary>
        /// Gets the distance of the step
        /// </summary>
        public double Distance
        {
            get { return this._distance; }
            private set { this.SetField(ref this._distance, value); }
        }
        /// <summary>
        /// Gets the instructions of the step
        /// </summary>
        public string Instructions
        {
            get { return this._instructions; }
            private set { this.SetField(ref this._instructions, value); }
        }
        ///<inheritdoc/>
        void IRouteStepFunctions.SetDistance(double distance)
        {
            this.Distance = distance;
        }
        ///<inheritdoc/>
        void IRouteStepFunctions.SetInstructions(string instructions)
        {
            this.Instructions = instructions;
        }
    }
}
