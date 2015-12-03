using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public partial class SamplePage : ContentPage
    {
        public SamplePage()
        {
            InitializeComponent();

            this.BindingContext = new SampleViewModel();
        }
    }
}
