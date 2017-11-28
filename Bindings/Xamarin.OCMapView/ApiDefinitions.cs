using System;
using CoreLocation;
using Foundation;
using MapKit;
//using OCMapView;
using ObjCRuntime;

namespace Xamarin.OCMapView
{

    // @protocol OCAlgorithmDelegate <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface OCAlgorithmDelegate
    {
        // @required -(NSArray *)algorithmClusteredPartially;
        [Abstract]
        [Export("algorithmClusteredPartially")]
        //[Verify (MethodToProperty), Verify (StronglyTypedNSArray)]
        NSObject[] AlgorithmClusteredPartially { get; }

        // @optional -(void)algorithmDidBeganClustering;
        [Export("algorithmDidBeganClustering")]
        void AlgorithmDidBeganClustering();

        // @optional -(void)algorithmDidFinishClustering;
        [Export("algorithmDidFinishClustering")]
        void AlgorithmDidFinishClustering();
    }

    // @interface OCAlgorithms : NSObject
    [BaseType(typeof(NSObject))]
    interface OCAlgorithms
    {
        // +(NSArray *)bubbleClusteringWithAnnotations:(NSArray *)annotationsToCluster clusterRadius:(CLLocationDistance)radius grouped:(BOOL)grouped;
        [Static]
        [Export("bubbleClusteringWithAnnotations:clusterRadius:grouped:")]
        //[Verify (StronglyTypedNSArray), Verify (StronglyTypedNSArray)]
        IMKAnnotation[] BubbleClusteringWithAnnotations(IMKAnnotation[] annotationsToCluster, double radius, bool grouped);

        // +(NSArray *)gridClusteringWithAnnotations:(NSArray *)annotationsToCluster clusterRect:(MKCoordinateSpan)tileRect grouped:(BOOL)grouped;
        [Static]
        [Export("gridClusteringWithAnnotations:clusterRect:grouped:")]
        //[Verify (StronglyTypedNSArray), Verify (StronglyTypedNSArray)]
        IMKAnnotation[] GridClusteringWithAnnotations(IMKAnnotation[] annotationsToCluster, MKCoordinateSpan tileRect, bool grouped);
    }

    // @protocol OCGrouping <MKAnnotation>
    [Protocol, Model]
    interface OCGrouping : IMKAnnotation
    {
        // @required @property (readonly, copy, nonatomic) NSString * groupTag;
        [Abstract]
        [Export("groupTag")]
        string GroupTag { get; }
    }

    // @interface OCAnnotation : NSObject <OCGrouping>
    [BaseType(typeof(NSObject))]
    interface OCAnnotation : OCGrouping
    {
        // @property (copy, nonatomic) NSString * title;
        [Export("title")]
        string Title { get; set; }

        // @property (copy, nonatomic) NSString * subtitle;
        [Export("subtitle")]
        string Subtitle { get; set; }

        // @property (copy, nonatomic) NSString * groupTag;
        [Export("groupTag")]
        string GroupTag { get; set; }

        // @property (assign, nonatomic) CLLocationCoordinate2D coordinate;
        [Export("coordinate", ArgumentSemantic.Assign)]
        CLLocationCoordinate2D Coordinate { get; set; }

        // -(id)initWithAnnotation:(id<MKAnnotation>)annotation;
        [Export("initWithAnnotation:")]
        IntPtr Constructor(IMKAnnotation annotation);

        // -(NSArray *)annotationsInCluster;
        [Export("annotationsInCluster")]
        //[Verify (MethodToProperty), Verify (StronglyTypedNSArray)]
        IMKAnnotation[] AnnotationsInCluster { get; }

        // -(void)addAnnotation:(id<MKAnnotation>)annotation;
        [Export("addAnnotation:")]
        void AddAnnotation(IMKAnnotation annotation);

        // -(void)addAnnotations:(NSArray *)annotations;
        [Export("addAnnotations:")]
        //[Verify (StronglyTypedNSArray)]
        void AddAnnotations(params IMKAnnotation[] annotations);

        // -(void)removeAnnotation:(id<MKAnnotation>)annotation;
        [Export("removeAnnotation:")]
        void RemoveAnnotation(IMKAnnotation annotation);

        // -(void)removeAnnotations:(NSArray *)annotations;
        [Export("removeAnnotations:")]
        //[Verify (StronglyTypedNSArray)]
        void RemoveAnnotations(params IMKAnnotation[] annotations);
    }

    // @interface OCMapView : MKMapView
    [BaseType(typeof(MKMapView))]
    interface OCMapView
    {
        // @property (nonatomic, strong) NSMutableSet * annotationsToIgnore;
        [Export("annotationsToIgnore", ArgumentSemantic.Strong)]
        NSMutableSet AnnotationsToIgnore { get; set; }

        // @property (readonly, nonatomic) NSArray * displayedAnnotations;
        [Export("displayedAnnotations")]
        //[Verify (StronglyTypedNSArray)]
        IMKAnnotation[] DisplayedAnnotations { get; }

        // @property (assign, nonatomic) BOOL clusteringEnabled;
        [Export("clusteringEnabled")]
        bool ClusteringEnabled { get; set; }

        // @property (assign, nonatomic) OCClusteringMethod clusteringMethod;
        [Export("clusteringMethod", ArgumentSemantic.Assign)]
        OCClusteringMethod ClusteringMethod { get; set; }

        // @property (assign, nonatomic) CLLocationDistance clusterSize;
        [Export("clusterSize")]
        double ClusterSize { get; set; }

        // @property (assign, nonatomic) BOOL clusterByGroupTag;
        [Export("clusterByGroupTag")]
        bool ClusterByGroupTag { get; set; }

        // @property (assign, nonatomic) CLLocationDegrees minLongitudeDeltaToCluster;
        [Export("minLongitudeDeltaToCluster")]
        double MinLongitudeDeltaToCluster { get; set; }

        // @property (assign, nonatomic) NSUInteger minimumAnnotationCountPerCluster;
        [Export("minimumAnnotationCountPerCluster")]
        nuint MinimumAnnotationCountPerCluster { get; set; }

        // @property (assign, nonatomic) BOOL clusterInvisibleViews;
        [Export("clusterInvisibleViews")]
        bool ClusterInvisibleViews { get; set; }

        // -(void)doClustering;
        [Export("doClustering")]
        void DoClustering();
    }

    [Static]
    //[Verify (ConstantsInterfaceAssociation)]
    partial interface Constants
    {
        // extern double OCMapViewVersionNumber;
        [Field("OCMapViewVersionNumber", "__Internal")]
        double OCMapViewVersionNumber { get; }

        // extern const unsigned char [] OCMapViewVersionString;
        [Field("OCMapViewVersionString", "__Internal")]
        NSString OCMapViewVersionString { get; }
    }
}
