//
//  AgoraSampleHander.h
//  BroadCastUI
//
//  Created by LEB on 2021/7/8.
//

#import <Foundation/Foundation.h>
#import <ReplayKit/ReplayKit.h>

typedef NS_ENUM(NSUInteger, AgoraReplayKitExtReason) {
  AgoraReplayKitExtReasonConnectFail = 1,  //连接主进程失败 需要主进程先开启startScreenCapture
  AgoraReplayKitExtReasonDisconnect = 2,      //连接断开，检查主进程是否退出
  AgoraReplayKitExtReasonInitiativeStop = 3,  //主进程主动要求停止
};

@class AgoraReplayKitExt;

@protocol AgoraReplayKitExtDelegate <NSObject>

- (void)broadcastFinished:(AgoraReplayKitExt* _Nonnull)broadcast
                   reason:(AgoraReplayKitExtReason)reason;

@end

NS_ASSUME_NONNULL_BEGIN

NS_SWIFT_NAME(AgoraReplayKitExt)
__attribute__((visibility("default"))) @interface AgoraReplayKitExt : NSObject

+ (instancetype)shareInstance;

- (void)start:(id<AgoraReplayKitExtDelegate>)delegate;
- (void)stop;
- (void)resume;
- (void)pause;
- (void)pushSampleBuffer:(CMSampleBufferRef)sampleBuffer
                withType:(RPSampleBufferType)sampleBufferType;

@end

NS_ASSUME_NONNULL_END
