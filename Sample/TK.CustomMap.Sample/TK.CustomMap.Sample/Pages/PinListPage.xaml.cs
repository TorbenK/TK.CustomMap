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

        private readonly IEnumerable<TKCustomMapPin> _pins;


        public PinListPage(IEnumerable<TKCustomMapPin> pins)
        {
            InitializeComponent();

            this._pins = pins;
            this.BindingContext = this._pins;

            this._lvPins.ItemSelected += (o, e) =>
            {
                if (this._lvPins.SelectedItem == null) return;

                this.OnPinSelected((TKCustomMapPin)this._lvPins.SelectedItem);
            };
        }
        protected virtual void OnPinSelected(TKCustomMapPin pin)
        {
            var ev = this.PinSelected;
            if (ev != null)
                ev(this, new PinSelectedEventArgs(pin));
        }
    }
    public class PinSelectedEventArgs : EventArgs
    {
        public TKCustomMapPin Pin { get; private set; }

        public PinSelectedEventArgs(TKCustomMapPin pin)
        {
            this.Pin = pin;
        }
        
    }
}
