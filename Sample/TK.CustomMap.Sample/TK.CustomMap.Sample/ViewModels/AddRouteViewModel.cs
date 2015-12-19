using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Api;
using TK.CustomMap.Overlays;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Sample
{
    public class AddRouteViewModel
    {
        private IPlaceResult _fromPlace, _toPlace;
        private Position _from, _to;

        public ObservableCollection<TKCustomMapPin> Pins { get; private set; }
        public ObservableCollection<TKRoute> Routes { get; private set; }
        public MapSpan Bounds { get; private set; }

        public Command<IPlaceResult> FromSelectedCommand
        {
            get
            {
                return new Command<IPlaceResult>(async (p) => 
                {
                    if(Device.OS == TargetPlatform.iOS)
                    {
                        TKNativeiOSPlaceResult placeResult = (TKNativeiOSPlaceResult)p;
                        this._fromPlace = placeResult;
                        this._from = placeResult.Details.Coordinate;
                    }
                    else
                    {
                        TKNativeAndroidPlaceResult placeResult = (TKNativeAndroidPlaceResult)p;
                        this._fromPlace = placeResult;
                        var details = await TKNativePlacesApi.Instance.GetDetails(placeResult.PlaceId);

                        this._from = details.Coordinate;
                    }
                });
            }
        }
        public Command<IPlaceResult> ToSelectedCommand
        {
            get
            {
                return new Command<IPlaceResult>(async (p) => 
                {
                    if(Device.OS == TargetPlatform.iOS)
                    {
                        TKNativeiOSPlaceResult placeResult = (TKNativeiOSPlaceResult)p;
                        this._toPlace = placeResult;
                        this._to = placeResult.Details.Coordinate;
                    }
                    else
                    {
                        TKNativeAndroidPlaceResult placeResult = (TKNativeAndroidPlaceResult)p;
                        this._toPlace = placeResult;
                        var details = await TKNativePlacesApi.Instance.GetDetails(placeResult.PlaceId);

                        this._to = details.Coordinate;
                    }
                });
            }
        }

        public Command AddRouteCommand
        {
            get
            {
                return new Command(() => 
                {
                    if (this._toPlace == null || this._fromPlace == null) return;


                    var route = new TKRoute
                    {
                        TravelMode = TKRouteTravelMode.Driving,
                        Source = this._from,
                        Destination = this._to,
                        Color = Color.Blue
                    };
                    this.Pins.Add(new RoutePin 
                    {
                        IsSource = true,
                        Route = route,
                        IsDraggable = true,
                        Position = this._from,
                        Title = this._fromPlace.Description,
                        ShowCallout = true,
                        DefaultPinColor = Color.Green
                    });
                    this.Pins.Add(new RoutePin
                    {
                        Route = route,
                        IsDraggable = true,
                        Position = this._to,
                        Title = this._toPlace.Description,
                        ShowCallout = true,
                        DefaultPinColor = Color.Red
                    });
                    this.Routes.Add(route);

                    Application.Current.MainPage.Navigation.PopAsync();
                });
            }
        }

        public AddRouteViewModel(ObservableCollection<TKRoute> routes, ObservableCollection<TKCustomMapPin> pins, MapSpan bounds)
        {
            this.Routes = routes;
            this.Pins = pins;
            this.Bounds = bounds;
        }
    }
}
