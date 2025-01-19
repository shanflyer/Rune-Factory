//
//  NPWhatsAppShareComposer.h
//  Native Plugins
//
//  Created by Ashwin kumar on 22/01/19.
//  Copyright (c) 2019 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <Social/Social.h>
#import "NPSocialShareComposerProtocol.h"

@interface NPWhatsAppShareComposer : NSObject<UIDocumentInteractionControllerDelegate, NPSocialShareComposerProtocol>

// static methods
+ (bool)IsServiceAvailable;

@end
