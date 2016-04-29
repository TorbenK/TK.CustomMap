using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace TK.CustomMap
{
    /// <summary>
    /// Base class handling property changed
    /// </summary>
    public class TKBase : BindableObject
    {

        /// <summary>
        /// Changes the field value if not equal
        /// </summary>
        /// <typeparam name="T">Type of the field</typeparam>
        /// <param name="field">The field as reference</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>True if value changed</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (field != null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value))
                {
                    return false;
                }
            }
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
