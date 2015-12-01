using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap
{
    /// <summary>
    /// An extensions of the Xamarin.Forms.Maps
    /// </summary>
    public class TKCustomMap : Map
    {
        /// <summary>
        /// Event is raised when all pins are created
        /// </summary>
        public event EventHandler PinsReady;

      //example of custom property
      /// <summary>
      /// Thickness property of border
      /// </summary>
      //public static readonly BindableProperty BorderThicknessProperty =
      //  BindableProperty.Create<TK.CustomMapImage, int>(
      //    p => p.BorderThickness, 0);

      /// <summary>
      /// Border thickness of circle image
      /// </summary>
      //public int BorderThickness
      //{
      //  get { return (int)GetValue(BorderThicknessProperty); }
      //  set { SetValue(BorderThicknessProperty, value); }
      //}
  }
}
