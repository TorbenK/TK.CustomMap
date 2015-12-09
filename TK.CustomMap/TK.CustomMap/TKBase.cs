using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap
{
    /// <summary>
    /// Base class handling property changed
    /// </summary>
    public class TKBase : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

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
        /// <summary>
        /// Raises <see cref="PropertyChanged"/>
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var ev = this.PropertyChanged;
            if (ev != null)
                ev(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
