using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public partial class AddRoutePage : ContentPage
    {
        

        public AddRoutePage()
        {
            InitializeComponent();

            var searchFrom = new PlacesAutoComplete(false) { ApiToUse = PlacesAutoComplete.PlacesApi.Osm };
            var searchTo = new PlacesAutoComplete(false) { ApiToUse = PlacesAutoComplete.PlacesApi.Osm };
            var labelFrom = new Label { Text = "From", FontAttributes = Xamarin.Forms.FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };
            var labelTo = new Label { Text = "To", FontAttributes = Xamarin.Forms.FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center };

            this._baseLayout.Children.Add(
                labelFrom,
                yConstraint: Constraint.Constant(30),
                widthConstraint: Constraint.RelativeToParent(p => p.Width));

            this._baseLayout.Children.Add(
                searchFrom,
                Constraint.Constant(0),
                Constraint.RelativeToView(labelFrom, (l, v) => labelFrom.Bounds.Bottom + 30));

            this._baseLayout.Children.Add(
                labelTo,
                widthConstraint: Constraint.RelativeToParent(p => p.Width),
                yConstraint: Constraint.RelativeToView(searchFrom, (l, v) => searchFrom.Y + searchFrom.HeightOfSearchBar + 30));

            this._baseLayout.Children.Add(
                searchTo,
                yConstraint: Constraint.RelativeToView(labelTo, (l, v) => labelTo.Bounds.Bottom + 30));
        }
    }
}
