using TMPro; // TextMeshPro를 사용하기 위해 추가
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리 추가

public class PhotonTest : MonoBehaviourPunCallbacks
{
    public InputField IDInputField; // 플레이어 닉네임 입력 필드

    void Start()
    {
        Screen.SetResolution(960, 600, false);
        Debug.Log("접속 로그: ");
        Debug.Log("접속자 목록: ");
    }

    // Photon 연결 함수
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 20;

        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername");
        PhotonNetwork.LocalPlayer.NickName = loggedInUsername;
        PhotonNetwork.JoinOrCreateRoom("Room1", options, null);
    }

    public override void OnJoinedRoom()
    {
        UpdatePlayerList();

        // 접속 로그 출력
        Debug.Log(IDInputField.text + " 님이 방에 참가하였습니다.");

        // UserType에 따라 씬 전환
        string userType = PlayerPrefs.GetString("UserType", "");

        if (userType == "doctor")
        {
            PhotonNetwork.LoadLevel("Hospital_Doctor");
        }
        else if (userType == "patient")
        {
            PhotonNetwork.LoadLevel("Hospital_rooms");
        }
        else
        {
            Debug.LogError("Unknown or missing user type. Loading default scene.");
            PhotonNetwork.LoadLevel("Hospital_rooms"); // 기본 씬 전환
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log(newPlayer.NickName + " 님이 입장하였습니다.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
        Debug.Log(otherPlayer.NickName + " 님이 퇴장하였습니다.");
    }

    void UpdatePlayerList()
    {
        Debug.Log("접속자 목록:");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
        }
    }
}
