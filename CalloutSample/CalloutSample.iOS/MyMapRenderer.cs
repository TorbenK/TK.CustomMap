using CoreGraphics;
using MapKit;
using System;
using System.Collections.Generic;
using System.Text;
using CalloutSample;
using CalloutSample.iOS;
using TK.CustomMap.iOSUnified;
using UIKit;
using Xamarin.Forms;

[assembly:ExportRenderer(typeof(MyMap), typeof(MyMapRenderer))]

namespace CalloutSample.iOS
{
    public class MyMapRenderer : TKCustomMapRenderer
    {
        public override MapKit.MKAnnotationView GetViewForAnnotation(MapKit.MKMapView mapView, MapKit.IMKAnnotation annotation)
        {
            var annotationView = base.GetViewForAnnotation(mapView, annotation);
            if (annotationView == null) return null;

            var customAnnotation = GetPinByAnnotation(annotation);
            annotationView.LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("Icon-60.png"));

            return annotationView;
        }
        public override void OnDidSelectAnnotationView(object sender, MapKit.MKAnnotationViewEventArgs e)
        {
            base.OnDidSelectAnnotationView(sender, e);

            var customView = e.View as MKAnnotationView;
            var customPinView = new UIView();

            customPinView.Frame = new CGRect(0, 0, 200, 84);
            var image = new UIImageView(new CGRect(0, 0, 200, 84));
            image.Image = UIImage.FromFile("monkey.png");
            customPinView.AddSubview(image);
            customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));

            customView.AddSubview(customPinView);
        }
    }
}
