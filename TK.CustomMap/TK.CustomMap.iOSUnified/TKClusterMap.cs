using MapKit;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.ClusterKit;
using ObjCRuntime;
using System.Threading;
using System.Linq;
using UIKit;

namespace TK.CustomMap.iOSUnified
{
    public class TKClusterMap : CKMap
    {
        private MKMapView _internalMap;

        public TKClusterMap(MKMapView internalMap)
        {
            _internalMap = internalMap;
        }

        private CKClusterManager _ckClusterManager;
        public override CKClusterManager ClusterManager => LazyInitializer.EnsureInitialized(ref _ckClusterManager, () =>
        {
            _ckClusterManager = new CKClusterManager();
            _ckClusterManager.Map = this;
            _ckClusterManager.Algorithm = new CKNonHierarchicalDistanceBasedAlgorithm();
            
            return _ckClusterManager;
        });

        public override MKMapRect VisibleMapRect => _internalMap.VisibleMapRect;

        public override double Zoom => Math.Log(360 * ((_internalMap.Frame.Size.Width / 256) / _internalMap.Region.Span.LongitudeDelta), 2.0);

        public override void AddClusters(CKCluster[] clusters)
        {
            _internalMap.AddAnnotations(clusters);
        }

        public override void DeselectCluster(CKCluster cluster, bool animated)
        {
            if (!_internalMap.SelectedAnnotations.Contains(cluster)) return;
            _internalMap.DeselectAnnotation(cluster, animated);
        }

        public override void PerformAnimations(CKClusterAnimation[] animations, Action<bool> completion)
        {
            foreach (var animation in animations)
            {
                animation.Cluster.Coordinate = animation.From;
            }

            UIView.AnimateNotify(
                ClusterManager.AnimationDuration,
                0,
                ClusterManager.AnimationOptions,
                () =>
                {
                    foreach (var animation in animations)
                    {
                        animation.Cluster.Coordinate = animation.To;
                    }
                },
                completion != null ? new UICompletionHandler(completion) : null);
        }

        public override void RemoveClusters(CKCluster[] clusters)
        {
            _internalMap.RemoveAnnotations(clusters);
        }

        public override void SelectCluster(CKCluster cluster, bool animated)
        {
            if (_internalMap.SelectedAnnotations.Contains(cluster)) return;
            _internalMap.SelectAnnotation(cluster, animated);
        }

        public void ShowCluster(CKCluster cluster, bool animated)
        {
            MKMapRect mapRect = MKMapRect.Null;
            foreach(var annoation in cluster.Annotations)
            {
                mapRect = MKMapRectByAddingPoint(mapRect, MKMapPoint.FromCoordinate(annoation.Coordinate));
            }
            _internalMap.SetVisibleMapRect(mapRect, animated);
        }

        public MKMapRect MKMapRectByAddingPoint(MKMapRect rect, MKMapPoint point)
        {
            return MKMapRect.Union(rect, new MKMapRect() { Origin = point, Size = MKMapRect.Null.Size });
        }
    }
}
