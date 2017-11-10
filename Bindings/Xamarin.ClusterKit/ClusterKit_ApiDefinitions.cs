using System;
//using ClusterKit;
using CoreLocation;
using Foundation;
using MapKit;
using ObjCRuntime;
using UIKit;

namespace Xamarin.ClusterKit
{
    //[Static]
    //[Verify (ConstantsInterfaceAssociation)]
    //partial interface Constants
    //{
    //	// extern double ClusterKitVersionNumber;
    //	[Field ("ClusterKitVersionNumber", "__Internal")]
    //	double ClusterKitVersionNumber { get; }

    //	// extern const unsigned char [] ClusterKitVersionString;
    //	[Field ("ClusterKitVersionString", "__Internal")]
    //	byte[] ClusterKitVersionString { get; }
    //}

    // @protocol CKAnnotationTreeDelegate <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface CKAnnotationTreeDelegate
    {
        // @optional -(BOOL)annotationTree:(id<CKAnnotationTree> _Nonnull)annotationTree shouldExtractAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("annotationTree:shouldExtractAnnotation:")]
        bool ShouldExtractAnnotation(CKAnnotationTree annotationTree, MKAnnotation annotation);
    }

    // @protocol CKAnnotationTree <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface CKAnnotationTree
    {
        [Wrap("WeakDelegate")]
        [NullAllowed]
        CKAnnotationTreeDelegate Delegate { get; set; }

        // @required @property (nonatomic, weak) id<CKAnnotationTreeDelegate> _Nullable delegate;
        //[Abstract]
        [NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
        NSObject WeakDelegate { get; set; }

        // @required @property (readonly, nonatomic) NSArray<id<MKAnnotation>> * _Nonnull annotations;
        //[Abstract]
        [Export("annotations")]
        MKAnnotation[] Annotations { get; }

        // @required -(instancetype _Nonnull)initWithAnnotations:(NSArray<id<MKAnnotation>> * _Nonnull)annotations;
        //[Abstract]
        [Export("initWithAnnotations:")]
        IntPtr Constructor(MKAnnotation[] annotations);

        // @required -(NSArray<id<MKAnnotation>> * _Nonnull)annotationsInRect:(MKMapRect)rect;
        [Export("annotationsInRect:")]
        MKAnnotation[] AnnotationsInRect(MKMapRect rect);
    }

    // @protocol CKCluster <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface ICKCluster
    {
        // @required +(__kindof CKCluster * _Nonnull)clusterWithCoordinate:(CLLocationCoordinate2D)coordinate;
        [Static]
        [Export("clusterWithCoordinate:")]
        CKCluster ClusterWithCoordinate(CLLocationCoordinate2D coordinate);
    }

    // @interface CKCluster : NSObject <CKCluster, MKAnnotation, NSFastEnumeration>
    [BaseType(typeof(NSObject))]
    interface CKCluster : ICKCluster, IMKAnnotation//, INSFastEnumeration
    {
        // @property (nonatomic) CLLocationCoordinate2D coordinate;
        [Export("coordinate", ArgumentSemantic.Assign)]
        CLLocationCoordinate2D Coordinate { get; set; }

        // @property (readonly, copy, nonatomic) NSArray<id<MKAnnotation>> * _Nonnull annotations;
        [Export("annotations", ArgumentSemantic.Copy)]
        MKAnnotation[] Annotations { get; }

        // @property (readonly, nonatomic) NSUInteger count;
        [Export("count")]
        nuint Count { get; }

        // @property (readonly, nonatomic) id<MKAnnotation> _Nullable firstAnnotation;
        [NullAllowed, Export("firstAnnotation")]
        MKAnnotation FirstAnnotation { get; }

        // @property (readonly, nonatomic) id<MKAnnotation> _Nullable lastAnnotation;
        [NullAllowed, Export("lastAnnotation")]
        MKAnnotation LastAnnotation { get; }

        // @property (readonly, nonatomic) MKMapRect bounds;
        [Export("bounds")]
        MKMapRect Bounds { get; }

        // -(void)addAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("addAnnotation:")]
        void AddAnnotation(MKAnnotation annotation);

        // -(void)removeAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("removeAnnotation:")]
        void RemoveAnnotation(MKAnnotation annotation);

        // -(id<MKAnnotation> _Nonnull)annotationAtIndex:(NSUInteger)index;
        [Export("annotationAtIndex:")]
        MKAnnotation AnnotationAtIndex(nuint index);

        // -(BOOL)containsAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("containsAnnotation:")]
        bool ContainsAnnotation(MKAnnotation annotation);

        // -(id<MKAnnotation> _Nonnull)objectAtIndexedSubscript:(NSUInteger)index;
        [Export("objectAtIndexedSubscript:")]
        MKAnnotation ObjectAtIndexedSubscript(nuint index);

        // -(BOOL)isEqualToCluster:(CKCluster * _Nonnull)cluster;
        [Export("isEqualToCluster:")]
        bool IsEqualToCluster(CKCluster cluster);

        // -(BOOL)intersectsCluster:(CKCluster * _Nonnull)cluster;
        [Export("intersectsCluster:")]
        bool IntersectsCluster(CKCluster cluster);

        // -(BOOL)isSubsetOfCluster:(CKCluster * _Nonnull)cluster;
        [Export("isSubsetOfCluster:")]
        bool IsSubsetOfCluster(CKCluster cluster);
    }

    // @interface CKCentroidCluster : CKCluster
    [BaseType(typeof(CKCluster))]
    interface CKCentroidCluster
    {
    }

    // @interface CKNearestCentroidCluster : CKCentroidCluster
    [BaseType(typeof(CKCentroidCluster))]
    interface CKNearestCentroidCluster
    {
    }

    // @interface CKBottomCluster : CKCluster
    [BaseType(typeof(CKCluster))]
    interface CKBottomCluster
    {
    }

    // @interface CKClusterAlgorithm : NSObject
    [BaseType(typeof(NSObject))]
    interface CKClusterAlgorithm
    {
        // -(NSArray<CKCluster *> * _Nonnull)clustersInRect:(MKMapRect)rect zoom:(double)zoom tree:(id<CKAnnotationTree> _Nonnull)tree;
        [Export("clustersInRect:zoom:tree:")]
        CKCluster[] ClustersInRect(MKMapRect rect, double zoom, CKAnnotationTree tree);
    }

    // @interface CKCluster (CKClusterAlgorithm)
    [Category]
    [BaseType(typeof(CKClusterAlgorithm))]
    interface CKClusterAlgorithm_CKCluster
    {
        // -(void)registerClusterClass:(Class<CKCluster> _Nonnull)clusterClass;
        [Export("registerClusterClass:")]
        void RegisterClusterClass(CKCluster clusterClass);

        // -(__kindof CKCluster * _Nonnull)clusterWithCoordinate:(CLLocationCoordinate2D)coordinate;
        [Export("clusterWithCoordinate:")]
        CKCluster ClusterWithCoordinate(CLLocationCoordinate2D coordinate);
    }

    // @interface CKGridBasedAlgorithm : CKClusterAlgorithm
    [BaseType(typeof(CKClusterAlgorithm))]
    interface CKGridBasedAlgorithm
    {
        // @property (nonatomic) CGFloat cellSize;
        [Export("cellSize")]
        nfloat CellSize { get; set; }
    }

    // @interface CKNonHierarchicalDistanceBasedAlgorithm : CKClusterAlgorithm
    [BaseType(typeof(CKClusterAlgorithm))]
    interface CKNonHierarchicalDistanceBasedAlgorithm
    {
        // @property (nonatomic) CGFloat cellSize;
        [Export("cellSize")]
        nfloat CellSize { get; set; }
    }

    [Static]
    //[Verify(ConstantsInterfaceAssociation)]
    partial interface Constants
    {
        // extern const double kCKMarginFactorWorld;
        [Field("kCKMarginFactorWorld", "__Internal")]
        double kCKMarginFactorWorld { get; }
    }

    // @protocol CKClusterManagerDelegate <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface CKClusterManagerDelegate
    {
        // @optional -(BOOL)clusterManager:(CKClusterManager * _Nonnull)clusterManager shouldClusterAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("clusterManager:shouldClusterAnnotation:")]
        bool ClusterManager(CKClusterManager clusterManager, MKAnnotation annotation);

        // @optional -(void)clusterManager:(CKClusterManager * _Nonnull)clusterManager performAnimations:(void (^ _Nonnull)(void))animations completion:(void (^ _Nullable)(BOOL))completion;
        [Export("clusterManager:performAnimations:completion:")]
        void ClusterManager(CKClusterManager clusterManager, Action animations, Action<bool> completion);
    }

    // @interface CKClusterManager : NSObject
    [BaseType(typeof(NSObject))]
    interface CKClusterManager
    {
        // @property (assign, nonatomic) CGFloat animationDuration;
        [Export("animationDuration")]
        nfloat AnimationDuration { get; set; }

        // @property (assign, nonatomic) UIViewAnimationOptions animationOptions;
        [Export("animationOptions", ArgumentSemantic.Assign)]
        UIViewAnimationOptions AnimationOptions { get; set; }

        // @property (nonatomic, strong) __kindof CKClusterAlgorithm * _Nonnull algorithm;
        [Export("algorithm", ArgumentSemantic.Strong)]
        CKClusterAlgorithm Algorithm { get; set; }

        // @property (nonatomic, weak) id<CKMap> _Nullable map;
        [NullAllowed, Export("map", ArgumentSemantic.Weak)]
        CKMap Map { get; set; }

        [Wrap("WeakDelegate")]
        [NullAllowed]
        CKClusterManagerDelegate Delegate { get; set; }

        // @property (nonatomic, weak) id<CKClusterManagerDelegate> _Nullable delegate;
        [NullAllowed, Export("delegate", ArgumentSemantic.Weak)]
        NSObject WeakDelegate { get; set; }

        // @property (readonly, nonatomic) id<MKAnnotation> _Nonnull selectedAnnotation;
        [Export("selectedAnnotation")]
        MKAnnotation SelectedAnnotation { get; }

        // @property (readonly, copy, nonatomic) NSArray<CKCluster *> * _Nonnull clusters;
        [Export("clusters", ArgumentSemantic.Copy)]
        CKCluster[] Clusters { get; }

        // @property (nonatomic) CGFloat maxZoomLevel;
        [Export("maxZoomLevel")]
        nfloat MaxZoomLevel { get; set; }

        // @property (nonatomic) double marginFactor;
        [Export("marginFactor")]
        double MarginFactor { get; set; }

        // @property (copy, nonatomic) NSArray<id<MKAnnotation>> * _Nonnull annotations;
        [Export("annotations", ArgumentSemantic.Copy)]
        MKAnnotation[] Annotations { get; set; }

        // -(void)addAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("addAnnotation:")]
        void AddAnnotation(MKAnnotation annotation);

        // -(void)addAnnotations:(NSArray<id<MKAnnotation>> * _Nonnull)annotations;
        [Export("addAnnotations:")]
        void AddAnnotations(MKAnnotation[] annotations);

        // -(void)removeAnnotation:(id<MKAnnotation> _Nonnull)annotation;
        [Export("removeAnnotation:")]
        void RemoveAnnotation(MKAnnotation annotation);

        // -(void)removeAnnotations:(NSArray<id<MKAnnotation>> * _Nonnull)annotations;
        [Export("removeAnnotations:")]
        void RemoveAnnotations(MKAnnotation[] annotations);

        // -(void)selectAnnotation:(id<MKAnnotation> _Nonnull)annotation animated:(BOOL)animated;
        [Export("selectAnnotation:animated:")]
        void SelectAnnotation(MKAnnotation annotation, bool animated);

        // -(void)deselectAnnotation:(id<MKAnnotation> _Nullable)annotation animated:(BOOL)animated;
        [Export("deselectAnnotation:animated:")]
        void DeselectAnnotation([NullAllowed] MKAnnotation annotation, bool animated);

        // -(void)updateClusters;
        [Export("updateClusters")]
        void UpdateClusters();

        // -(void)updateClustersIfNeeded;
        [Export("updateClustersIfNeeded")]
        void UpdateClustersIfNeeded();
    }

    // @interface CKClusterAnimation : NSObject
    [BaseType(typeof(NSObject))]
    [DisableDefaultCtor]
    interface CKClusterAnimation
    {
        // @property (readonly, nonatomic) CKCluster * _Nonnull cluster;
        [Export("cluster")]
        CKCluster Cluster { get; }

        // @property (nonatomic) CLLocationCoordinate2D from;
        [Export("from", ArgumentSemantic.Assign)]
        CLLocationCoordinate2D From { get; set; }

        // @property (nonatomic) CLLocationCoordinate2D to;
        [Export("to", ArgumentSemantic.Assign)]
        CLLocationCoordinate2D To { get; set; }

        // -(instancetype _Nonnull)initWithCluster:(CKCluster * _Nonnull)cluster __attribute__((objc_designated_initializer));
        [Export("initWithCluster:")]
        [DesignatedInitializer]
        IntPtr Constructor(CKCluster cluster);
    }

    // @protocol CKMap <NSObject>
    [Protocol, Model]
    [BaseType(typeof(NSObject))]
    interface CKMap
    {
        // @required @property (readonly, nonatomic) CKClusterManager * _Nonnull clusterManager;
        [Abstract]
        [Export("clusterManager")]
        CKClusterManager ClusterManager { get; }

        // @required @property (readonly, nonatomic) MKMapRect visibleMapRect;
        [Abstract]
        [Export("visibleMapRect")]
        MKMapRect VisibleMapRect { get; }

        // @required @property (readonly, nonatomic) double zoom;
        [Abstract]
        [Export("zoom")]
        double Zoom { get; }

        // @required -(void)selectCluster:(CKCluster * _Nonnull)cluster animated:(BOOL)animated;
        [Abstract]
        [Export("selectCluster:animated:")]
        void SelectCluster(CKCluster cluster, bool animated);

        // @required -(void)deselectCluster:(CKCluster * _Nonnull)cluster animated:(BOOL)animated;
        [Abstract]
        [Export("deselectCluster:animated:")]
        void DeselectCluster(CKCluster cluster, bool animated);

        // @required -(void)removeClusters:(NSArray<CKCluster *> * _Nonnull)clusters;
        [Abstract]
        [Export("removeClusters:")]
        void RemoveClusters(CKCluster[] clusters);

        // @required -(void)addClusters:(NSArray<CKCluster *> * _Nonnull)clusters;
        [Abstract]
        [Export("addClusters:")]
        void AddClusters(CKCluster[] clusters);

        // @required -(void)performAnimations:(NSArray<CKClusterAnimation *> * _Nonnull)animations completion:(void (^ _Nullable)(BOOL))completion;
        [Abstract]
        [Export("performAnimations:completion:")]
        void PerformAnimations(CKClusterAnimation[] animations, Action<bool> completion);
    }

    // @interface CKQuadTree : NSObject <CKAnnotationTree>
    [BaseType(typeof(NSObject))]
    interface CKQuadTree : CKAnnotationTree
    {
        // -(instancetype _Nonnull)initWithAnnotations:(NSArray<id<MKAnnotation>> * _Nonnull)annotations __attribute__((objc_designated_initializer));
        [Export("initWithAnnotations:")]
        [DesignatedInitializer]
        IntPtr Constructor(MKAnnotation[] annotations);
    }

    //[Static]
    //[Verify (ConstantsInterfaceAssociation)]
    //partial interface Constants
    //{
    //	// extern double ClusterKitVersionNumber;
    //	[Field ("ClusterKitVersionNumber", "__Internal")]
    //	double ClusterKitVersionNumber { get; }

    //	// extern const unsigned char [] ClusterKitVersionString;
    //	[Field ("ClusterKitVersionString", "__Internal")]
    //	byte[] ClusterKitVersionString { get; }
    //}
}