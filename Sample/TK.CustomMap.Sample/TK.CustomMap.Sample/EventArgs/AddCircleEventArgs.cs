using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TK.CustomMap.Sample
{
    public class AddCircleEventArgs : EventArgs
    {
        public AddCircleViewModel Model { get; set; }

        public AddCircleEventArgs(AddCircleViewModel model)
        {
            this.Model = model;
        }
    }
}
