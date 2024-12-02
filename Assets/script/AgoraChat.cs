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
    private VideoSurface MyView; // �� ���� ȭ��
    [SerializeField]
    private VideoSurface RemoteView; // �ٸ� ����� ���� ȭ��
    [SerializeField]
    private Button JoinButton; // Join ��ư
    [SerializeField]
    private Button LeaveButton; // Leave ��ư

    private IRtcEngine mRtcEngine;
    private bool isJoined = false; // ä�� ���� ���� Ȯ��

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
            if (!isJoined) // �̹� ���ӵ� ���°� �ƴϸ�
            {
                mRtcEngine.EnableVideo();
                mRtcEngine.EnableVideoObserver();

                mRtcEngine.JoinChannel(ChannelName, "", 0); // ä�� ����
                isJoined = true; // ���� ���·� ǥ��

                // RemoteView�� �ٸ� ����ڰ� ������ ������ ��Ȱ��ȭ
                if (RemoteView != null)
                {
                    RemoteView.SetEnable(false);
                }
            }
        });

        LeaveButton.onClick.AddListener(() =>
        {
            if (isJoined) // ä�ο� ���ӵ� ������ ����
            {
                mRtcEngine.LeaveChannel(); // ä�� ������
                ResetVideoViews(); // ���� ȭ�� �ʱ�ȭ
                isJoined = false; // ���� ���� ����
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
            MyView.SetForUser(0); // �� uid�� 0���� ����
            MyView.SetEnable(true); // �� ���� ȭ�� Ȱ��ȭ
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
            RemoteView.SetForUser(uid); // �ٸ� ������� ������ RemoteView�� ǥ��
            RemoteView.SetEnable(true); // RemoteView Ȱ��ȭ
        }
    }

    void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        if (RemoteView != null)
        {
            RemoteView.SetEnable(false); // �ٸ� ������� ���� ȭ�� ��Ȱ��ȭ
        }
    }

    void ResetVideoViews()
    {
        if (MyView != null)
        {
            MyView.SetEnable(false); // �� ���� ȭ�� ��Ȱ��ȭ
        }
        if (RemoteView != null)
        {
            RemoteView.SetEnable(false); // �ٸ� ����� ���� ȭ�� ��Ȱ��ȭ
        }
        mRtcEngine.DisableVideo(); // ���� ��Ȱ��ȭ
        mRtcEngine.DisableVideoObserver(); // ���� ������ ��Ȱ��ȭ
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
