//
//  AgoraSpatialAudioKit.h
//  AgoraRtcKit
//
//  Copyright (c) 2018 Agora. All rights reserved.
//

#ifndef AgoraSpatialAudioKit_h
#define AgoraSpatialAudioKit_h

#import <Foundation/Foundation.h>
#import "AgoraEnumerates.h"
#import "AgoraObjects.h"

__attribute__((visibility("default"))) @interface AgoraRemoteVoicePositionInfo : NSObject
@property(strong, nonatomic) NSArray<NSNumber*>* _Nonnull position;
@property(strong, nonatomic) NSArray<NSNumber*>* _Nullable forward;
@end

@class AgoraRtcEngineKit, AgoraBaseSpatialAudioKit, AgoraLocalSpatialAudioKit;

__attribute__((visibility("default"))) @interface AgoraLocalSpatialAudioConfig : NSObject
@property(assign, nonatomic) AgoraRtcEngineKit* _Nullable rtcEngine;
@end

__attribute__((visibility("default"))) @interface AgoraBaseSpatialAudioKit : NSObject

- (int)setMaxAudioRecvCount:(NSUInteger)maxCount;

- (int)setAudioRecvRange:(float)range;

- (int)setDistanceUnit:(float)unit;

- (int)updateSelfPosition:(NSArray<NSNumber*>* _Nonnull)position axisForward:(NSArray<NSNumber*>* _Nonnull)axisForward axisRight:(NSArray<NSNumber*>* _Nonnull)axisRight axisUp:(NSArray<NSNumber*>* _Nonnull)axisUp;

- (int)muteLocalAudioStream:(BOOL)mute;

- (int)muteAllRemoteAudioStreams:(BOOL)mute;

@end

__attribute__((visibility("default"))) @interface AgoraLocalSpatialAudioKit : AgoraBaseSpatialAudioKit

+ (instancetype _Nonnull)sharedLocalSpatialAudioWithConfig:(AgoraLocalSpatialAudioConfig* _Nonnull)config;

+ (void)destroy;

- (int)updateRemotePosition:(NSUInteger)uid positionInfo:(AgoraRemoteVoicePositionInfo* _Nonnull)posInfo;

- (int)removeRemotePosition:(NSUInteger)uid;

- (int)clearRemotePositions;

@end

#endif /* AgoraGmeKit_h */
