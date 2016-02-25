namespace TK.CustomMap.Interfaces
{
    /// <summary>
    /// Containing functions which are called from the renderer to the PCL
    /// </summary>
    public interface IMapFunctions
    {
        /// <summary>
        /// Sets the renderer functions
        /// </summary>
        /// <param name="renderer">The renderer</param>
        void SetRenderer(IRendererFunctions renderer);
    }
}
