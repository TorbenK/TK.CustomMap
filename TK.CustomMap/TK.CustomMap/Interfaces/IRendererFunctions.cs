using System.Threading.Tasks;
using Xamarin.Forms;

namespace TK.CustomMap.Interfaces
{
    /// <summary>
    /// Containing all functions which are called from the PCL to the renderer directly
    /// </summary>
    public interface IRendererFunctions
    {
        /// <summary>
        /// Returns the current map as an image
        /// </summary>
        /// <returns>Image of the current map</returns>
        Task<byte[]> GetSnapshot();
    }
}
