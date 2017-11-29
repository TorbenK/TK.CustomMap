using CoreGraphics;
using MapKit;
using System.Linq;
using UIKit;
using Xamarin.iOS.ClusterKit;

namespace TK.CustomMap.iOSUnified
{
    /// <summary>
    /// Default cluster annotation view. 
    /// </summary>
    public class TKDefaultClusterAnnotationView : MKAnnotationView
    {
        UILabel _label;

        /// <summary>
        /// Gets/Sets the color of the circle. Default is <see cref="UIColor.Blue"/>
        /// </summary>
        public UIColor Color { get; set; } = UIColor.Blue;
        /// <summary>
        /// Gets/Sets the text color. Default is <see cref="UIColor.White"/>
        /// </summary>
        public UIColor TextColor 
        {
            get => _label.TextColor;
            set => _label.TextColor = value;
        }
        /// <summary>
        /// Gets/Sets the font of the text. Default is <see cref="UIFont.BoldSystemFontOfSize(13)"/>
        /// </summary>
        public UIFont Font
        {
            get => _label.Font;
            set => _label.Font = value;
        }
        /// <summary>
        /// Gets/Sets the base radius. Default is 25
        /// </summary
        public int BaseRadius { get; set; } = 25;

        /// <summary>
        /// Creates a new instance of <see cref="TKDefaultClusterAnnotationView"/>
        /// </summary>
        /// <param name="annotation">The annotation</param>
        /// <param name="reuseIdentifier">A reuse identifier</param>
        public TKDefaultClusterAnnotationView(IMKAnnotation annotation, string reuseIdentifier) 
            : base(annotation, reuseIdentifier)
        {
            _label = new UILabel
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.BoldSystemFontOfSize(13),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                AdjustsFontSizeToFitWidth = true,
                MinimumScaleFactor = 2,
                BaselineAdjustment = UIBaselineAdjustment.AlignCenters
            };
            AddSubview(_label);

            Configure();
        }
        /// <summary>
        /// Configures the view
        /// </summary>
        public void Configure()
        {
            var cluster = Annotation as CKCluster;
            if (cluster == null) return;

            var count = cluster.Annotations.Count();
            BackgroundColor = Color;
            var diameter = (double)BaseRadius * 2;
            
            if(count < 8)
            {
                diameter *= 0.6;
            }
            else if(count < 16)
            {
                diameter *= 0.8;
            }

            Frame = new CGRect(Frame.Location, new CGSize(diameter, diameter));
            _label.Text = count.ToString();
        }
        /// <summary>
        /// Layout subviews
        /// </summary>
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            Layer.MasksToBounds = true;
            Layer.CornerRadius = Bounds.Width / 2;
            _label.Frame = Bounds;
        }
    }
}
