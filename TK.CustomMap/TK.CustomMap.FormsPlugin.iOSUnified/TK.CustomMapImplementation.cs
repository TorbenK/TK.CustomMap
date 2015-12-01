using TK.CustomMap.FormsPlugin.Abstractions;
using System;
using Xamarin.Forms;
using TK.CustomMap.FormsPlugin.iOSUnified;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TK.CustomMap.FormsPlugin.Abstractions.TK.CustomMapControl), typeof(TK.CustomMapRenderer))]
namespace TK.CustomMap.FormsPlugin.iOSUnified
{
  /// <summary>
  /// TK.CustomMap Renderer
  /// </summary>
  public class TK.CustomMapRenderer //: TRender (replace with renderer type
  {
    /// <summary>
    /// Used for registration with dependency service
    /// </summary>
    public static void Init(){}
  }
}
