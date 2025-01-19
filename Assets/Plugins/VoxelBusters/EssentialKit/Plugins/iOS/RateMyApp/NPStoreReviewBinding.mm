//
//  NPStoreReviewBinding.mm
//  Native Plugins
//
//  Created by Ashwin kumar on 22/01/19.
//  Updated 12/08/24
//  Copyright (c) 2019 Voxel Busters Interactive LLP. All rights reserved.


#import <StoreKit/StoreKit.h>
#import "NPDefines.h"
#import "NPUnityAppController.h"


NPBINDING DONTSTRIP void NPStoreReviewRequestReview()
{
    NPUnityAppController    *appController      = (NPUnityAppController*)GetAppController();
    UIWindowScene           *windowScene        = [[appController window] windowScene];
    
    [SKStoreReviewController requestReviewInScene: windowScene];
}
