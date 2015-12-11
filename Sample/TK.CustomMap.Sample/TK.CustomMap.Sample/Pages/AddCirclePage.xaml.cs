using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public partial class AddCirclePage : ContentPage
    {
        private readonly AddCircleViewModel _model;

        public event EventHandler<AddCircleEventArgs> AddCircle;

        public AddCirclePage()
        {
            InitializeComponent();

            this.BindingContext = this._model = new AddCircleViewModel();
        }

        private void AddCircleClicked(object sender, EventArgs e)
        {
            this.OnAddCircle();
        }

        protected virtual void OnAddCircle()
        {
            var ev = this.AddCircle;

            if (ev != null)
                ev(this, new AddCircleEventArgs(this._model));
        }
    }
}
