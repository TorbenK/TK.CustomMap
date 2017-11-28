//using System;
//using System.Runtime.InteropServices;
//using ClusterKit;
//using CoreLocation;
//using MapKit;

//namespace Xamarin.ClusterKit
//{
//    /// Quadtree point
//    struct hb_qpoint_t
//    {
//        MKMapPoint point;
//        MKAnnotation annotation;
//        hb_qpoint_t next;
//}
    

///// Quadtree node
//struct hb_qnode_t
//    {
//        int cap;         ///< Capacity of the node
//        int cnt;         ///< Number of point in the node
//        MKMapRect bound;        ///< Area covered by the node
//        hb_qpoint_t points;    ///< Chained list of node's points
//        hb_qnode_t nw;    ///< NW quadrant of the node
//        hb_qnode_t ne;    ///< NE quadrant of the node
//        hb_qnode_t sw;    ///< SW quadrant of the node
//        hb_qnode_t se;    ///< SE quadrant of the node
//}

///// Quadtree container
//    struct hb_qtree_t
//    {
//        hb_qnode_t root;   ///< Root node
//    }
    

//    static class CFunctions
//    {
//        // extern double CKDistance (CLLocationCoordinate2D from, CLLocationCoordinate2D to) __attribute__((visibility("default")));
//        [DllImport("__Internal")]
//        static extern double CKDistance(CLLocationCoordinate2D from, CLLocationCoordinate2D to);

//        // extern MKMapRect MKMapRectByAddingPoint (MKMapRect rect, MKMapPoint point) __attribute__((visibility("default")));
//        [DllImport("__Internal")]
//        static extern MKMapRect MKMapRectByAddingPoint(MKMapRect rect, MKMapPoint point);

//        // extern hb_qtree_t * _Nonnull hb_qtree_new (MKMapRect rect, NSUInteger cap);
//        [DllImport("__Internal")]
//        static extern unsafe hb_qtree_t* hb_qtree_new(MKMapRect rect, nuint cap);

//        // extern void hb_qtree_free (hb_qtree_t * _Nonnull tree);
//        [DllImport("__Internal")]
//        static extern unsafe void hb_qtree_free(hb_qtree_t* tree);

//        // extern void hb_qtree_insert (hb_qtree_t * _Nonnull tree, id<MKAnnotation> _Nonnull annotation);
//        [DllImport("__Internal")]
//        static extern unsafe void hb_qtree_insert(hb_qtree_t* tree, MKAnnotation annotation);

//        // extern void hb_qtree_remove (hb_qtree_t * _Nonnull tree, id<MKAnnotation> _Nonnull annotation);
//        [DllImport("__Internal")]
//        static extern unsafe void hb_qtree_remove(hb_qtree_t* tree, MKAnnotation annotation);

//        // extern void hb_qtree_clear (hb_qtree_t * _Nonnull tree);
//        [DllImport("__Internal")]
//        static extern unsafe void hb_qtree_clear(hb_qtree_t* tree);

//        // extern void hb_qtree_find_in_range (hb_qtree_t * _Nonnull tree, MKMapRect range, void (^ _Nonnull)(id<MKAnnotation> _Nonnull) find);
//        [DllImport("__Internal")]
//        static extern unsafe void hb_qtree_find_in_range(hb_qtree_t* tree, MKMapRect range, Action<MKAnnotation> find);
//    }
//}
