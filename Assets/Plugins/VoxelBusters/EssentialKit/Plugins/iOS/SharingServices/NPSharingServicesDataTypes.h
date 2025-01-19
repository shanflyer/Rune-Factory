//
//  NPSharingServicesDataTypes.h
//  Native Plugins
//
//  Created by Ashwin kumar on 22/01/19.
//  Copyright (c) 2019 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <MessageUI/MessageUI.h>
#import <Social/Social.h>
#import "NPSocialShareComposerProtocol.h"
#import "NPUnityDataTypes.h"


// enum for determining recipient type
typedef enum : NSInteger
{
    NPMailRecipientTypeTo,
    NPMailRecipientTypeCc,
    NPMailRecipientTypeBcc,
} NPMailRecipientType;

typedef enum : int
{
    NPSocialShareComposerTypeFacebook,
    NPSocialShareComposerTypeTwitter,
    NPSocialShareComposerTypeWhatsApp,
} NPSocialShareComposerType;

// callback signatures
typedef void (*MailComposerClosedNativeCallback)(void* nativePtr, MFMailComposeResult result, NPError error);

typedef void (*MessageComposerClosedNativeCallback)(void* nativePtr, MessageComposeResult result);

typedef void (*ShareSheetClosedNativeCallback)(void* nativePtr, bool completed, NPError error);

typedef void (*SocialShareComposerClosedNativeCallback)(void* nativePtr, SLComposeViewControllerResult result);
