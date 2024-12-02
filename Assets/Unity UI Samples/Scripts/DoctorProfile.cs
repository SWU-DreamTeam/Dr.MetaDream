using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class DoctorProfile: MonoBehaviour
{
    public TextMeshProUGUI userinfoText;
    public TextMeshProUGUI usernameText;

    public TextMeshProUGUI scaddressText;
    public TextMeshProUGUI sexText;
    public TextMeshProUGUI residentregnumText;
    public TextMeshProUGUI phonenumText;
    public TextMeshProUGUI certificationText;
    public TextMeshProUGUI medicaldepartmentText;


    // PHP ��ũ��Ʈ URL (���� �ּҿ� �°� ����)
    private string getUserInfoUrl = "http://pbl.dothome.co.kr/Doctor_profile.php"; // ���� ���� URL�� ����

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
        public string certification;
        public string medical_department;
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
                    if (certificationText != null) certificationText.text = userData.certification;
                    if (medicaldepartmentText != null) medicaldepartmentText.text =  userData.medical_department;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parsing Error: " + e.Message);
                }
            }
        }
    }

}
