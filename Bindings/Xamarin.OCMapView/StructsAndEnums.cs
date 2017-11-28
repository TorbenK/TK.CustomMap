using System.Runtime.InteropServices;
using CoreLocation;

namespace Xamarin.OCMapView
{
    public enum OCClusteringMethod : uint
    {
        Bubble,
        Grid
    }

    static class CFunctions
    {
        // extern double CLLocationCoordinateDistance (CLLocationCoordinate2D c1, CLLocationCoordinate2D c2);
        [DllImport("__Internal")]
        //[Verify (PlatformInvoke)]
        static extern double CLLocationCoordinateDistance(CLLocationCoordinate2D c1, CLLocationCoordinate2D c2);
    }
}