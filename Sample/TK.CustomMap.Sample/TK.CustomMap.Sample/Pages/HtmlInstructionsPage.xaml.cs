using TK.CustomMap.Overlays;
using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public partial class HtmlInstructionsPage : ContentPage
    {
        public HtmlInstructionsPage(TKRoute route)
        {
            InitializeComponent();

            this.BindingContext = new HtmlInstructionsViewModel(route);
        }
    }
}
