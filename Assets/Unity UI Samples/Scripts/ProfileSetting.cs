using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class ProfileSetting : MonoBehaviour
{
    public TextMeshProUGUI userinfoText;
    public TextMeshProUGUI usernameText;

    public TextMeshProUGUI scaddressText;
    public TextMeshProUGUI sexText;
    public TextMeshProUGUI residentregnumText;
    public TextMeshProUGUI phonenumText;
    public TextMeshProUGUI medicalhistoryText;
    public TextMeshProUGUI chronicdiseaseText;
    public TextMeshProUGUI medicationText;


    // PHP ��ũ��Ʈ URL (���� �ּҿ� �°� ����)
    private string getUserInfoUrl = "http://pbl.dothome.co.kr/Profile_info.php"; // ���� ���� URL�� ����

    // ����� �����͸� ������ Ŭ���� ����
    [System.Serializable]
    public class UserData
    {
        public string userinfo;
        public string username;
        public string sc_address;
        public string sex;
        public string resident_registration_num;
        public string phone_num;
        public string medical_history;
        public string chronic_disease;
        public string medication;
    }

    void Start()
    {
        // PlayerPrefs���� �α��ε� ����� �̸��� ID ��������
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "Guest");
        string loggedInUser = PlayerPrefs.GetString("LoggedInUser", "Guest");

        // Text�� ����� �̸� ���
        if (userinfoText != null)
        {
            userinfoText.text = loggedInUsername;
            usernameText.text = loggedInUsername;
        }
        else
        {
            Debug.LogError("UserInfoText is not assigned in the Inspector.");
        }

        // ����� �����͸� �����κ��� �������� ���� �ڷ�ƾ ����
        StartCoroutine(FetchUserData(loggedInUser));
    }

    IEnumerator FetchUserData(string user)
    {
        // GET ��û�� ���� URL ����
        string urlWithParams = getUserInfoUrl + "?user=" + UnityWebRequest.EscapeURL(user);
        Debug.Log("Request URL: " + urlWithParams); // URL ���

        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlWithParams))
        {
            yield return webRequest.SendWebRequest();

            // ��û ���� �� ���� ���
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("WebRequest Error: " + webRequest.error);
            }
            else
            {
                // ���� ���� ���
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response from server: " + jsonResponse);

                // JSON �Ľ� �� UI ������Ʈ
                try
                {
                    UserData userData = JsonUtility.FromJson<UserData>(jsonResponse);

                    // ���� �����͸� UI�� ���
                    if (scaddressText != null) scaddressText.text = userData.sc_address;
                    if (sexText != null) sexText.text = userData.sex;
                    if (residentregnumText != null) residentregnumText.text = userData.resident_registration_num;
                    if (phonenumText != null) phonenumText.text = userData.phone_num;
                    if (medicalhistoryText != null) medicalhistoryText.text = "����: " + userData.medical_history;
                    if (chronicdiseaseText != null) chronicdiseaseText.text = "������ȯ: " + userData.chronic_disease;
                    if (medicationText != null) medicationText.text = "����๰: " + userData.medication;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parsing Error: " + e.Message);
                }
            }
        }
    }

}
