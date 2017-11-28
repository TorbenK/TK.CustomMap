#ifdef __OBJC__
#import <UIKit/UIKit.h>
#else
#ifndef FOUNDATION_EXPORT
#if defined(__cplusplus)
#define FOUNDATION_EXPORT extern "C"
#else
#define FOUNDATION_EXPORT extern
#endif
#endif
#endif

#import "ClusterKit.h"
#import "CKClusterAlgorithm.h"
#import "CKGridBasedAlgorithm.h"
#import "CKNonHierarchicalDistanceBasedAlgorithm.h"
#import "CKCluster.h"
#import "CKClusterManager.h"
#import "CKMap.h"
#import "CKAnnotationTree.h"
#import "CKQuadTree.h"

FOUNDATION_EXPORT double ClusterKitVersionNumber;
FOUNDATION_EXPORT const unsigned char ClusterKitVersionString[];

