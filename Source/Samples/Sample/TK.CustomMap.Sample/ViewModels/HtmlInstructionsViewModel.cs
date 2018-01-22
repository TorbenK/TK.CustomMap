using TK.CustomMap.Overlays;
using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public class HtmlInstructionsViewModel : TKBase
    {
        public HtmlWebViewSource Instructions { get;  set; }
        

        public HtmlInstructionsViewModel(TKRoute route)
        {
            Instructions = new HtmlWebViewSource();
            Instructions.Html = @"<html><body>";
            foreach (var s in route.Steps)
            {
                Instructions.Html += string.Format("<b>{0}km:</b> {1}<br /><hr />", s.Distance / 1000, s.Instructions);
            }
            Instructions.Html += @"</body></html>";
        }
    }
}
