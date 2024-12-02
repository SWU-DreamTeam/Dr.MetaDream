using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon; // Photon�� Hashtable�� ����ϱ� ���� �߰�
using System.Collections.Generic;
using Photon.Realtime;

public class DoctorWaitingList : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject patientNamePrefab; // ȯ�ڸ� ������
    [SerializeField]
    private GameObject buttonPrefab; // ��ư ������
    [SerializeField]
    private Transform contentParent; // ��ũ�� ���� Content ��ü
    [SerializeField]
    private float itemSpacing = 100f; // �׸� ���� ���� ����
    [SerializeField]
    private float yOffset = 150f;
    [SerializeField]
    private MedicalCertificate medicalCertificate; // MedicalCertificate ��ũ��Ʈ�� ���� ���� �߰�;
    [SerializeField]
    private GameObject medicalCertificateWindow;

    private void Start()
    {
        // �ǻ� ���� ���۵Ǹ� ��� ����Ʈ�� ������Ʈ
        UpdateWaitingList();
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername");
        if (!string.IsNullOrEmpty(loggedInUsername))
        {
            PhotonNetwork.NickName = loggedInUsername; // NickName�� �α��ε� ����� �̸����� ����
            Debug.Log("PhotonNetwork NickName ������: " + PhotonNetwork.NickName);
        }
    }

    private void UpdateWaitingList()
    {
        // Custom Properties���� ��� ����Ʈ�� ������
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)PhotonNetwork.CurrentRoom.CustomProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            // �� ȯ�ڸ��� ��ũ�Ѻ信 �߰�
            for (int i = 0; i < patientNames.Length; i++)
            {
                string patientName = patientNames[i];
                if (!string.IsNullOrEmpty(patientName))
                {
                    AddPatientNameAndButtonToScrollView(patientName, i);
                }
            }
        }
    }

    // ��ũ�� �信 ȯ�ڸ�� ��ư�� ���� �߰�
    private void AddPatientNameAndButtonToScrollView(string patientName, int index)
    {
        // ȯ�ڸ� ������ �ν��Ͻ�ȭ
        GameObject patientEntry = Instantiate(patientNamePrefab, contentParent, false);

        // ���� ��ġ ����
        RectTransform patientRect = patientEntry.GetComponent<RectTransform>();
        patientRect.anchoredPosition = new Vector2(-250, yOffset - index * itemSpacing); // ȯ�ڸ� ������ ��ġ ���� 

        // TextMeshProUGUI ������Ʈ ���� (ȯ�ڸ� ����)
        TextMeshProUGUI textComponent = patientEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = patientName;
        }

        // ��ư ������ �ν��Ͻ�ȭ
        GameObject buttonEntry = Instantiate(buttonPrefab, contentParent, false);

        // ��ư ���� ��ġ ����
        RectTransform buttonRect = buttonEntry.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(50, yOffset - index * itemSpacing); // send ��ư ������ ��ġ ���� 

        // Button ������Ʈ ã��
        Button sendMessageButton = buttonEntry.GetComponentInChildren<Button>();
        if (sendMessageButton != null)
        {
            // ��ư�� Ŭ���� �� ����� �Լ� ���� (ȯ�ڸ� ����)
            sendMessageButton.onClick.AddListener(() => OnSendMessageButtonClicked(patientName));
        }

        //Debug.Log("�ǻ� ��� ����Ʈ�� ȯ�ڸ�� ��ư �߰���: " + patientName);
    }

    public void OnSendMessageButtonClicked(string patientName)
    {
        bool patientFound = false;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == patientName)
            {
                patientFound = true;
                Debug.Log("RPC ���� �õ� ��: " + patientName);
                photonView.RPC("ShowPopupForPatient", player, patientName);
                Debug.Log("Ư�� ȯ�ڿ��Ը� RPC ���۵�: " + patientName);

                if (medicalCertificate != null)
                {
                    medicalCertificateWindow.SetActive(true); // Window GameObject Ȱ��ȭ
                    CoroutineManager.Instance.StartManagedCoroutine(medicalCertificate.LoadPatientInfo(patientName));
                }
                else
                {
                    Debug.LogWarning("medicalCertificate ��ũ��Ʈ�� �Ҵ���� �ʾҽ��ϴ�. Inspector���� �Ҵ����ּ���.");
                }
                break;
            }
        }

        if (!patientFound)
        {
            Debug.LogWarning("Ư�� ȯ�ڸ� ã�� �� �����ϴ�: " + patientName);
        }
    }


    // �÷��̾ ���� ������ �� ȣ��Ǵ� �޼���
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // ���� ���� �÷��̾��� �̸��� ��� ����Ʈ���� ����
        RemovePatientFromWaitingList(otherPlayer.NickName);
        // ��� ����Ʈ ������Ʈ
        UpdateWaitingList();
    }

    // ��� ����Ʈ���� ȯ�ڸ� ����
    private void RemovePatientFromWaitingList(string patientName)
    {
        // ���� ��� ����Ʈ ��������
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)PhotonNetwork.CurrentRoom.CustomProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            // ��� ����Ʈ���� �ش� ȯ�ڸ� ����
            List<string> updatedList = new List<string>(patientNames);
            updatedList.Remove(patientName);

            // ������Ʈ�� ����Ʈ�� �ٽ� ����
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
            newProperties["WaitingList"] = string.Join(",", updatedList.ToArray());
            PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties);

            Debug.Log("��� ����Ʈ���� ȯ�ڸ� ���ŵ�: " + patientName);
        }
    }

    // Room Custom Properties�� ������Ʈ�� �� ȣ��Ǵ� �޼���
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("WaitingList"))
        {
            // ��� ����Ʈ ���� �����ո� ����
            foreach (Transform child in contentParent)
            {
                if (child != null && child.name.Contains("Variant(Clone)")) // �̸��� ���� ������ ��� ���͸�
                {
                    Destroy(child.gameObject);
                }
            }

            // ��� ����Ʈ ���� ������Ʈ
            UpdateWaitingList();
        }
    }
}