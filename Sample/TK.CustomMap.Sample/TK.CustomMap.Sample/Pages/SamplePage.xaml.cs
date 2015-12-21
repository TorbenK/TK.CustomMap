using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public partial class SamplePage : ContentPage
    {
        public SamplePage()
        {
            InitializeComponent();

            this.CreateView();
            this.BindingContext = new SampleViewModel();
        }

        private void CreateView()
        {

            var autoComplete = new PlacesAutoComplete { ApiToUse = PlacesAutoComplete.PlacesApi.Native };
            autoComplete.SetBinding(PlacesAutoComplete.PlaceSelectedCommandProperty, "PlaceSelectedCommand");

            var mapView = new TKCustomMap();
            mapView.SetBinding(TKCustomMap.CustomPinsProperty, "Pins");
            mapView.SetBinding(TKCustomMap.MapClickedCommandProperty, "MapClickedCommand");
            mapView.SetBinding(TKCustomMap.MapLongPressCommandProperty, "MapLongPressCommand");
            mapView.SetBinding(TKCustomMap.MapCenterProperty, "MapCenter");
            mapView.SetBinding(TKCustomMap.PinSelectedCommandProperty, "PinSelectedCommand");
            mapView.SetBinding(TKCustomMap.SelectedPinProperty, "SelectedPin");
            mapView.SetBinding(TKCustomMap.RoutesProperty, "Routes");
            mapView.SetBinding(TKCustomMap.PinDragEndCommandProperty, "DragEndCommand");
            mapView.SetBinding(TKCustomMap.CirclesProperty, "Circles");
            mapView.SetBinding(TKCustomMap.CalloutClickedCommandProperty, "CalloutClickedCommand");
            mapView.SetBinding(TKCustomMap.PolygonsProperty, "Polygons");
            mapView.SetBinding(TKCustomMap.MapRegionProperty, "MapRegion");
            mapView.SetBinding(TKCustomMap.RouteClickedCommandProperty, "RouteClickedCommand");
            mapView.SetBinding(TKCustomMap.RouteCalculationFinishedCommandProperty, "RouteCalculationFinishedCommand");
            mapView.AnimateMapCenterChange = true;

            autoComplete.SetBinding(PlacesAutoComplete.BoundsProperty, "MapRegion");


            this._baseLayout.Children.Add(
                mapView,
                Constraint.RelativeToView(autoComplete, (r, v) => v.X),
                Constraint.RelativeToView(autoComplete, (r, v) => autoComplete.HeightOfSearchBar),
                heightConstraint: Constraint.RelativeToParent((r) => r.Height - autoComplete.HeightOfSearchBar),
                widthConstraint: Constraint.RelativeToView(autoComplete, (r, v) => v.Width));

            this._baseLayout.Children.Add(
                autoComplete,
                Constraint.Constant(0),
                Constraint.Constant(0));
        }
    }
}
