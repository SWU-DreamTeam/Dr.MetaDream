using UnityEngine;
using Photon.Pun;
using TMPro;

public class PopupController : MonoBehaviourPun
{
    [SerializeField]
    private GameObject popupPanel; // PopupPanel ��ü�� Unity Inspector���� �Ҵ�
    [SerializeField]
    private TextMeshProUGUI popupText;

    private void Awake()
    {
        Debug.Log("PopupController Awake ȣ���");
    }

    private void Start()
    {
        Debug.Log("PopupController Start ȣ���. LoggedInUsername: " + PlayerPrefs.GetString("LoggedInUsername", ""));
    }


    [PunRPC]
    public void ShowPopupForPatient(string targetPatientName)
    {
        // ����� �α� �߰�
        Debug.Log("ShowPopupForPatient RPC ȣ���");

        // ���� �α��ε� ������ �г����� ��ǥ ȯ�ڸ�� ��ġ�ϴ��� Ȯ��
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "");
        Debug.Log("�α��ε� ����: " + loggedInUsername + ", ��ǥ ����: " + targetPatientName);

        if (loggedInUsername == targetPatientName)
        {
            if (popupPanel != null && !popupPanel.activeSelf)
            {
                popupPanel.SetActive(true); // �г� Ȱ��ȭ

                if (popupText != null)
                {
                    popupText.text = "����Ƿ� �����ϼ���.";
                }

                Debug.Log("ȯ�ڿ��� �˾� ǥ�õ�: " + targetPatientName);
            }
            else
            {
                Debug.LogError("PopupPanel�� �Ҵ���� �ʾҰų� �̹� Ȱ��ȭ �����Դϴ�.");
            }
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null && popupPanel.activeSelf)
        {
            popupPanel.SetActive(false); // �˾� â ��Ȱ��ȭ
            Debug.Log("�˾� â�� �������ϴ�.");
        }
        else
        {
            Debug.LogError("PopupPanel�� �Ҵ���� �ʾҰų� �̹� ��Ȱ��ȭ �����Դϴ�.");
        }
    }

}
