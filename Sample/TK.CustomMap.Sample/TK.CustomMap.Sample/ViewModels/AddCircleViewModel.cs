using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public class AddCircleViewModel : TKBase
    {
        private bool _isValidColor;
        private string _colorText;
        private double _radius;
        private Color _color;
        private double _argbColor;

        public string ColorText
        {
            get { return this._colorText; }
            set
            {
                this.SetField(ref this._colorText, value);
            }
        }
        public double Radius
        {
            get { return this._radius; }
            set
            {
                this.SetField(ref this._radius, value);
            }
        }
        public double ArgbColor
        {
            get { return this._argbColor; }
            set 
            {
                var uIntVal = Convert.ToUInt32(value);

                if(this.SetField(ref this._argbColor, uIntVal))
                {
                    this.Color = Color.FromUint(uIntVal);
                }
            }
        }
        
        public Color Color
        {
            get { return this._color; }
            set { this.SetField(ref this._color, value); }
        }
        public AddCircleViewModel()
        {
            this._argbColor = 0xFF0000;
            this._radius = 1000;
        }
    }
}
