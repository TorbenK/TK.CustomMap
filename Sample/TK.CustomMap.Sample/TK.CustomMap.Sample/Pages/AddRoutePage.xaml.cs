using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Overlays;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Sample
{
    public partial class AddRoutePage : ContentPage
    {
        

        public AddRoutePage(ObservableCollection<TKRoute> routes, ObservableCollection<TKCustomMapPin> pins, MapSpan bounds)
        {
            InitializeComponent();

            var googleImage = new Image 
            {
                Source = "powered_by_google_on_white.png"
            };

            var searchFrom = new PlacesAutoComplete(false) { ApiToUse = PlacesAutoComplete.PlacesApi.Native, Bounds = bounds, Placeholder = "From" };
            searchFrom.SetBinding(PlacesAutoComplete.PlaceSelectedCommandProperty, "FromSelectedCommand");
            var searchTo = new PlacesAutoComplete(false) { ApiToUse = PlacesAutoComplete.PlacesApi.Native, Bounds = bounds, Placeholder = "To" };
            searchTo.SetBinding(PlacesAutoComplete.PlaceSelectedCommandProperty, "ToSelectedCommand");
            var labelFrom = new Label { Text = "From", FontAttributes = Xamarin.Forms.FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };
            var labelTo = new Label { Text = "To", FontAttributes = Xamarin.Forms.FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };

            this._baseLayout.Children.Add(
                googleImage,
                Constraint.Constant(10),
                Constraint.RelativeToParent(l => l.Height - 30));

            //this._baseLayout.Children.Add(
            //    labelFrom,
            //    yConstraint: Constraint.Constant(30),
            //    widthConstraint: Constraint.RelativeToParent(p => p.Width));

            //this._baseLayout.Children.Add(
            //    labelTo,
            //    widthConstraint: Constraint.RelativeToParent(p => p.Width),
            //    yConstraint: Constraint.RelativeToView(searchFrom, (l, v) => searchFrom.Y + searchFrom.HeightOfSearchBar + 30));

            this._baseLayout.Children.Add(
                searchTo,
                yConstraint: Constraint.RelativeToView(searchFrom, (l, v) => searchFrom.HeightOfSearchBar + 10));

            this._baseLayout.Children.Add(
                searchFrom,
                Constraint.Constant(0),
                Constraint.Constant(10));

            this.BindingContext = new AddRouteViewModel(routes, pins, bounds);
        }
    }
}
