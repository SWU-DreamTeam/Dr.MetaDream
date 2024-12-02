using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Photon.Pun; // Photon ��Ʈ��ũ�� ����ϱ� ���� �߰�
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon; // Hashtable ����� ���� �߰�
using UnityEngine.SceneManagement;




public class SendSymptoms : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField symptomsInput; // Symptoms �Է� �ʵ�
    [SerializeField]
    private Button sendButton; // Send ��ư
    [SerializeField]
    private GameObject patientNamePrefab; // ȯ�ڸ� ������
    [SerializeField]
    private Transform contentParent; // ��ũ�� ���� Content ��ü

    private string uploadUrl = "http://pbl.dothome.co.kr/SaveSymptoms.php"; // ������ PHP ��ũ��Ʈ URL
    private bool isUploading = false; // ���ε� �� ���θ� Ȯ���ϱ� ���� �÷���

    private void Start()
    {
        sendButton.onClick.RemoveAllListeners();
        sendButton.onClick.AddListener(OnSendButtonClick);

        // �濡 ������ �� Room Properties���� ��� ����Ʈ ��������
        UpdateWaitingList();
    }

    private void UpdateWaitingList()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)PhotonNetwork.CurrentRoom.CustomProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            // �� ȯ�ڸ��� ��ũ�Ѻ信 �߰�
            foreach (string patientName in patientNames)
            {
                if (!string.IsNullOrEmpty(patientName))
                {
                    AddPatientNameToScrollView(patientName);
                }
            }
        }
    }

    public void OnSendButtonClick()
    {
        if (isUploading)
        {
            Debug.Log("���ε� ���Դϴ�. ��ø� ��ٷ��ּ���.");
            return;
        }

        string symptomsText = symptomsInput.text.Trim();
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername"); // �α��ε� ����� ID

        if (string.IsNullOrEmpty(symptomsText))
        {
            Debug.Log("���� ������ �Է����ּ���.");
            return;
        }

        if (string.IsNullOrEmpty(loggedInUsername))
        {
            Debug.Log("�α��ε� ����� ������ Ȯ���� �� �����ϴ�.");
            return;
        }

        // �̹� ��� ����Ʈ�� �����ϴ��� Ȯ��
        if (IsPatientAlreadyInList(loggedInUsername))
        {
            Debug.Log("�ش� ȯ�ڸ��� �̹� ��� ����Ʈ�� �ֽ��ϴ�.");
            return; // �̹� �����ϸ� ���ε����� ����
        }

        isUploading = true;

        // �ڷ�ƾ�� ���� ������ ������ ���ε� �õ�
        StartCoroutine(UploadSymptoms(symptomsText, loggedInUsername));
    }

    private bool IsPatientAlreadyInList(string patientName)
    {
        foreach (Transform child in contentParent)
        {
            TextMeshProUGUI childText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null && childText.text == MaskPatientName(patientName))
            {
                return true; // �̹� ����
            }
        }
        return false; // �������� ����
    }

    private IEnumerator UploadSymptoms(string symptomsText, string loggedInUsername)
    {
        WWWForm form = new WWWForm();
        form.AddField("reception_status", symptomsText);
        form.AddField("user", loggedInUsername);

        using (UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("���� �����Ͱ� ���������� ���ε�Ǿ����ϴ�.");
                symptomsInput.text = ""; // ���� �� �Է� �ʵ� �ʱ�ȭ

                // ��� Ŭ���̾�Ʈ�� ��� ����Ʈ�� ȯ�� �߰��� ����ȭ
                photonView.RPC("AddPatientNameToScrollViewRPC", RpcTarget.All, loggedInUsername);

                // Room Custom Properties�� ��� ����Ʈ ������Ʈ
                AddPatientToWaitingList(loggedInUsername);
            }
            else
            {
                Debug.LogError("������ ���ε� ����: " + www.error);
            }
        }

        isUploading = false;
    }

    private string MaskPatientName(string patientName)
    {
        if (string.IsNullOrEmpty(patientName) || patientName.Length < 2)
            return patientName; // �̸��� �ʹ� ª�� ��� ó������ ����

        string masked = patientName[0] + "*" + patientName[patientName.Length - 1];
        return masked;
    }

    private void AddPatientNameToScrollView(string patientName)
    {
        string maskedName = MaskPatientName(patientName); // �̸� ����ŷ

        GameObject newPatientEntry = Instantiate(patientNamePrefab, contentParent);
        TextMeshProUGUI textComponent = newPatientEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = maskedName; // ����ŷ�� �̸� ����
        }

        Debug.Log("ȯ�ڸ� �߰���: " + maskedName);
    }

    [PunRPC]
    private void AddPatientNameToScrollViewRPC(string patientName)
    {
        Debug.Log("RPC ȣ���: " + patientName);

        foreach (Transform child in contentParent)
        {
            TextMeshProUGUI childText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null && childText.text == MaskPatientName(patientName))
            {
                Debug.Log("�ش� ȯ�ڸ��� �̹� ��� ����Ʈ�� �ֽ��ϴ�.");
                return;
            }
        }

        AddPatientNameToScrollView(patientName); // ���� �Լ� ����
    }

    private void AddPatientToWaitingList(string patientName)
    {
        ExitGames.Client.Photon.Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (currentProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)currentProperties["WaitingList"];
            waitingList += "," + patientName;
            currentProperties["WaitingList"] = waitingList;
        }
        else
        {
            currentProperties["WaitingList"] = patientName;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);
    }

    private void RemovePatientFromWaitingList(string patientName)
    {
        ExitGames.Client.Photon.Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (currentProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)currentProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            List<string> updatedList = new List<string>(patientNames);
            updatedList.Remove(patientName);

            currentProperties["WaitingList"] = string.Join(",", updatedList.ToArray());
            PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);
        }
    }

    private void RemovePatientFromScrollView(string patientName)
    {
        foreach (Transform child in contentParent)
        {
            TextMeshProUGUI childText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null && childText.text == MaskPatientName(patientName))
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("�÷��̾ ���� �������ϴ�: " + otherPlayer.NickName);

        RemovePatientFromWaitingList(otherPlayer.NickName);
        RemovePatientFromScrollView(otherPlayer.NickName);
    }

    public void ChangeToAgoraScene()
    {
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername");

        Debug.Log("Button Clicked: Changing scene to Hospital_Patient and removing from waiting list");

        RemovePatientFromWaitingList(loggedInUsername);
        RemovePatientFromScrollView(loggedInUsername);

        SceneManager.LoadScene("Hospital_Patient");
    }
}
