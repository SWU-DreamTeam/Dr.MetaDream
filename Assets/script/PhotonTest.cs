using TMPro; // TextMeshPro�� ����ϱ� ���� �߰�
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement; // �� ���� �߰�

public class PhotonTest : MonoBehaviourPunCallbacks
{
    public InputField IDInputField; // �÷��̾� �г��� �Է� �ʵ�

    void Start()
    {
        Screen.SetResolution(960, 600, false);
        Debug.Log("���� �α�: ");
        Debug.Log("������ ���: ");
    }

    // Photon ���� �Լ�
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

        // ���� �α� ���
        Debug.Log(IDInputField.text + " ���� �濡 �����Ͽ����ϴ�.");

        // UserType�� ���� �� ��ȯ
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
            PhotonNetwork.LoadLevel("Hospital_rooms"); // �⺻ �� ��ȯ
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
        Debug.Log(newPlayer.NickName + " ���� �����Ͽ����ϴ�.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
        Debug.Log(otherPlayer.NickName + " ���� �����Ͽ����ϴ�.");
    }

    void UpdatePlayerList()
    {
        Debug.Log("������ ���:");
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Debug.Log(PhotonNetwork.PlayerList[i].NickName);
        }
    }
}
