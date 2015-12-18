using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.CustomMap.Overlays;
using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public class HtmlInstructionsViewModel : TKBase
    {
        public HtmlWebViewSource Instructions { get; private set; }
        

        public HtmlInstructionsViewModel(TKRoute route)
        {
            this.Instructions = new HtmlWebViewSource();
            this.Instructions.Html = @"<html><body>";
            foreach (var s in route.Steps)
            {
                this.Instructions.Html += string.Format("<b>{0}km:</b> {1}<br /><hr />", s.Distance / 1000, s.Instructions);
            }
            this.Instructions.Html += @"</body></html>";
        }
    }
}
