namespace TK.CustomMap.Overlays
{
    /// <summary>
    /// A Step of a route
    /// </summary>
    public sealed class TKRouteStep : TKBase, IRouteStepFunctions
    {
         double _distance;
         string _instructions;

        /// <summary>
        /// Gets the distance of the step
        /// </summary>
        public double Distance
        {
            get { return _distance; }
             set { SetField(ref _distance, value); }
        }
        /// <summary>
        /// Gets the instructions of the step
        /// </summary>
        public string Instructions
        {
            get { return _instructions; }
             set { SetField(ref _instructions, value); }
        }
        ///<inheritdoc/>
        void IRouteStepFunctions.SetDistance(double distance)
        {
            Distance = distance;
        }
        ///<inheritdoc/>
        void IRouteStepFunctions.SetInstructions(string instructions)
        {
            Instructions = instructions;
        }
    }
}
