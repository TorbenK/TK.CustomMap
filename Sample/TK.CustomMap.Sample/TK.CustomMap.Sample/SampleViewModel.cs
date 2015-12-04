using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Api.Google;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TK.CustomMap.Sample
{
    public class SampleViewModel : INotifyPropertyChanged
    {
        private Position _mapCenter;
        private TKCustomMapPin _selectedPin;
        private ObservableCollection<TKCustomMapPin> _pins;

        public ObservableCollection<TKCustomMapPin> Pins
        {
            get { return this._pins; }
            set 
            {
                if (this._pins != value)
                {
                    this._pins = value;
                    this.OnPropertyChanged("Pins");
                }
            }
        }
        public Position MapCenter
        {
            get { return this._mapCenter; }
            set 
            {
                if (this._mapCenter != value)
                {
                    this._mapCenter = value;
                    this.OnPropertyChanged("MapCenter");
                }
            }
        }
        public TKCustomMapPin SelectedPin
        {
            get { return this._selectedPin; }
            set
            {
                if (this._selectedPin != value)
                {
                    this._selectedPin = value;
                    this.OnPropertyChanged("SelectedPin");
                }
            }
        }

        public Command<Position> MapLongPressCommand
        {
            get
            {
                return new Command<Position>((position) => 
                {
                    this._pins.Add(new TKCustomMapPin 
                    {
                        Position = position,
                        Title = string.Format("Pin {0}, {1}", position.Latitude, position.Longitude),
                        ShowCallout = true
                    });
                    this.MapCenter = position;
                });
            }
        }
        public Command<Position> MapClickedCommand
        {
            get
            {
                return new Command<Position>((positon) => 
                {
                    this.SelectedPin = null;
                });
            }
        }
        public Command<GmsPlacePrediction> PlaceSelectedCommand
        {
            get
            {
                return new Command<GmsPlacePrediction>(async p => 
                {
                    var position = await new Geocoder().GetPositionsForAddressAsync(p.Description);

                    if(position != null && position.Any())
                        this.MapCenter = position.First();
                });
            }
        }

        public SampleViewModel()
        {
            this._mapCenter = new Position(40.7142700, -74.0059700);
            this._pins = new ObservableCollection<TKCustomMapPin>(new TKCustomMapPin[] 
            {
                new TKCustomMapPin
                {
                    Position = new Position(40.7142700, -74.0059700),
                    ShowCallout = false,
                    Image = "https://maps.gstatic.com/mapfiles/ms2/micons/purple.png",
                    IsDraggable = true,
                    Title = "New York"
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var ev = this.PropertyChanged;

            if (ev != null)
                ev(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
