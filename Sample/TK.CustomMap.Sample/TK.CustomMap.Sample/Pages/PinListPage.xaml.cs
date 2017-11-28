using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace TK.CustomMap.Sample.Pages
{
    public partial class PinListPage : ContentPage
    {
        public event EventHandler<PinSelectedEventArgs> PinSelected;

         readonly IEnumerable<TKCustomMapPin> _pins;


        public PinListPage(IEnumerable<TKCustomMapPin> pins)
        {
            InitializeComponent();

            _pins = pins;
            BindingContext = _pins;

            _lvPins.ItemSelected += (o, e) =>
            {
                if (_lvPins.SelectedItem == null) return;

                OnPinSelected((TKCustomMapPin)_lvPins.SelectedItem);
            };
        }
        protected virtual void OnPinSelected(TKCustomMapPin pin)
        {
            PinSelected?.Invoke(this, new PinSelectedEventArgs(pin));
        }
    }
    public class PinSelectedEventArgs : EventArgs
    {
        public TKCustomMapPin Pin { get;  set; }

        public PinSelectedEventArgs(TKCustomMapPin pin)
        {
            Pin = pin;
        }
        
    }
}
