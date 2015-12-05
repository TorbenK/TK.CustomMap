using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Api.Google;
using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public class GooglePlacesAutoComplete : RelativeLayout
    {
        // TODO: SUMMARIES

        private bool _textChangeItemSelected;

        private readonly SearchBar _searchBar;
        private readonly ListView _autoCompleteListView;

        private IEnumerable<GmsPlacePrediction> _predictions;

        public static readonly BindableProperty PlaceSelectedCommandProperty =
            BindableProperty.Create<GooglePlacesAutoComplete, Command<GmsPlacePrediction>>(
                p => p.PlaceSelectedCommand,
                null);

        public Command<GmsPlacePrediction> PlaceSelectedCommand
        {
            get { return (Command<GmsPlacePrediction>)this.GetValue(PlaceSelectedCommandProperty); }
            set { this.SetValue(PlaceSelectedCommandProperty, value); }
        }
        public double HeightOfSearchBar
        {
            get
            {
                return this._searchBar.Height;
            }
        }

        public GooglePlacesAutoComplete()
        {
            this._autoCompleteListView = new ListView
            {
                IsVisible = false,
                RowHeight = 40,
                HeightRequest = 0,
                BackgroundColor = Color.White
            };
            this._autoCompleteListView.ItemTemplate = new DataTemplate(() =>
            {
                var cell = new TextCell();
                cell.SetBinding(ImageCell.TextProperty, "Description");

                return cell;
            });

            this._searchBar = new SearchBar
            {
                Placeholder = "Search for address..."
            };
            this.Children.Add(this._searchBar,
                Constraint.Constant(0),
                Constraint.Constant(0),
                widthConstraint: Constraint.RelativeToParent(l => l.Width));
            this.Children.Add(
                this._autoCompleteListView,
                Constraint.Constant(0),
                Constraint.RelativeToView(this._searchBar, (r, v) => v.Y + v.Height));

            this._autoCompleteListView.ItemSelected += ItemSelected;
            this._searchBar.TextChanged += SearchTextChanged;
            this._searchBar.SearchButtonPressed += SearchButtonPressed;

            this._textChangeItemSelected = false;
        }

        private void SearchButtonPressed(object sender, EventArgs e)
        {
            if (this._predictions != null && this._predictions.Any())
                this.HandleItemSelected(this._predictions.First());
            else
                this.Reset();
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._textChangeItemSelected)
            {
                this._textChangeItemSelected = false;
                return;
            }

            this.SearchPlaces();
        }

        private async void SearchPlaces()
        {
            try
            {
                if (string.IsNullOrEmpty(this._searchBar.Text))
                {
                    this._autoCompleteListView.ItemsSource = null;
                    this._autoCompleteListView.IsVisible = false;
                    this._autoCompleteListView.HeightRequest = 0;
                    return;
                }

                var result = await GmsPlace.Instance.GetPredictions(this._searchBar.Text);

                if (result != null && result.Predictions != null && result.Predictions.Any())
                {
                    this._predictions = result.Predictions;

                    this._autoCompleteListView.HeightRequest = result.Predictions.Count() * 40;
                    this._autoCompleteListView.IsVisible = true;
                    this._autoCompleteListView.ItemsSource = this._predictions;
                }
                else
                {
                    this._autoCompleteListView.HeightRequest = 0;
                    this._autoCompleteListView.IsVisible = false;
                }
            }
            catch
            {
                // TODO
            }
        }
        private void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            var prediction = (GmsPlacePrediction)e.SelectedItem;

            this.HandleItemSelected(prediction);
        }

        private void HandleItemSelected(GmsPlacePrediction prediction)
        {
            if (this.PlaceSelectedCommand != null && this.PlaceSelectedCommand.CanExecute(this))
            {
                this.PlaceSelectedCommand.Execute(prediction);
            }

            this._textChangeItemSelected = true;

            this._searchBar.Text = prediction.Description;
            this._autoCompleteListView.SelectedItem = null;

            this.Reset();
        }
        private void Reset()
        {
            this._autoCompleteListView.ItemsSource = null;
            this._autoCompleteListView.IsVisible = false;
            this._autoCompleteListView.HeightRequest = 0;
            this._searchBar.Unfocus();
        }
    }
}
