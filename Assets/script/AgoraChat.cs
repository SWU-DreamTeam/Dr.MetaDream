using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;



#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif
using System.Collections;

public class AgoraChat : MonoBehaviour
{
    [SerializeField]
    private string AppID;
    [SerializeField]
    private string ChannelName;

    [SerializeField]
    private VideoSurface MyView; // 내 비디오 화면
    [SerializeField]
    private VideoSurface RemoteView; // 다른 사용자 비디오 화면
    [SerializeField]
    private Button JoinButton; // Join 버튼
    [SerializeField]
    private Button LeaveButton; // Leave 버튼

    private IRtcEngine mRtcEngine;
    private bool isJoined = false; // 채널 접속 여부 확인

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
#endif

    private void Awake()
    {
        SetupUI();
    }

    private void Start()
    {
        SetupAgora();
    }

    void Update()
    {
        CheckPermissions();
    }

    private void CheckPermissions()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    void SetupUI()
    {
        JoinButton.onClick.AddListener(() =>
        {
            if (!isJoined) // 이미 접속된 상태가 아니면
            {
                mRtcEngine.EnableVideo();
                mRtcEngine.EnableVideoObserver();

                mRtcEngine.JoinChannel(ChannelName, "", 0); // 채널 접속
                isJoined = true; // 접속 상태로 표시

                // RemoteView는 다른 사용자가 접속할 때까지 비활성화
                if (RemoteView != null)
                {
                    RemoteView.SetEnable(false);
                }
            }
        });

        LeaveButton.onClick.AddListener(() =>
        {
            if (isJoined) // 채널에 접속된 상태일 때만
            {
                mRtcEngine.LeaveChannel(); // 채널 나가기
                ResetVideoViews(); // 비디오 화면 초기화
                isJoined = false; // 접속 상태 해제
            }
        });
    }

    void SetupAgora()
    {
        if (mRtcEngine == null)
        {
            mRtcEngine = IRtcEngine.GetEngine(AppID);

            mRtcEngine.OnUserJoined += OnUserJoined;
            mRtcEngine.OnUserOffline += OnUserOffline;
            mRtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccessHandler;
            mRtcEngine.OnLeaveChannel += OnLeaveChannelHandler;
        }
    }

    void OnJoinChannelSuccessHandler(string channelName, uint uid, int elapsed)
    {
        if (MyView != null)
        {
            MyView.SetForUser(0); // 내 uid는 0으로 설정
            MyView.SetEnable(true); // 내 비디오 화면 활성화
        }
        Debug.Log("Joined channel: " + channelName);
    }

    void OnLeaveChannelHandler(RtcStats stats)
    {
        ResetVideoViews();
        Debug.Log("Left the channel");
    }

    void OnUserJoined(uint uid, int elapsed)
    {
        if (RemoteView != null)
        {
            RemoteView.SetForUser(uid); // 다른 사용자의 비디오를 RemoteView에 표시
            RemoteView.SetEnable(true); // RemoteView 활성화
        }
    }

    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        if (RemoteView != null)
        {
            RemoteView.SetEnable(false); // 다른 사용자의 비디오 화면 비활성화
        }
    }

    void ResetVideoViews()
    {
        if (MyView != null)
        {
            MyView.SetEnable(false); // 내 비디오 화면 비활성화
        }
        if (RemoteView != null)
        {
            RemoteView.SetEnable(false); // 다른 사용자 비디오 화면 비활성화
        }
        mRtcEngine.DisableVideo(); // 비디오 비활성화
        mRtcEngine.DisableVideoObserver(); // 비디오 관찰자 비활성화
    }

    void UnloadEngine()
    {
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }

    void OnApplicationQuit()
    {
        UnloadEngine();
    }
}
