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


    // PHP 스크립트 URL (서버 주소에 맞게 변경)
    private string getUserInfoUrl = "http://pbl.dothome.co.kr/Profile_info.php"; // 실제 서버 URL로 변경

    // 사용자 데이터를 저장할 클래스 정의
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
        // PlayerPrefs에서 로그인된 사용자 이름과 ID 가져오기
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "Guest");
        string loggedInUser = PlayerPrefs.GetString("LoggedInUser", "Guest");

        // Text에 사용자 이름 출력
        if (userinfoText != null)
        {
            userinfoText.text = loggedInUsername;
            usernameText.text = loggedInUsername;
        }
        else
        {
            Debug.LogError("UserInfoText is not assigned in the Inspector.");
        }

        // 사용자 데이터를 서버로부터 가져오기 위한 코루틴 실행
        StartCoroutine(FetchUserData(loggedInUser));
    }

    IEnumerator FetchUserData(string user)
    {
        // GET 요청을 위한 URL 설정
        string urlWithParams = getUserInfoUrl + "?user=" + UnityWebRequest.EscapeURL(user);
        Debug.Log("Request URL: " + urlWithParams); // URL 출력

        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlWithParams))
        {
            yield return webRequest.SendWebRequest();

            // 요청 실패 시 에러 출력
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("WebRequest Error: " + webRequest.error);
            }
            else
            {
                // 서버 응답 출력
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response from server: " + jsonResponse);

                // JSON 파싱 및 UI 업데이트
                try
                {
                    UserData userData = JsonUtility.FromJson<UserData>(jsonResponse);

                    // 받은 데이터를 UI에 출력
                    if (scaddressText != null) scaddressText.text = userData.sc_address;
                    if (sexText != null) sexText.text = userData.sex;
                    if (residentregnumText != null) residentregnumText.text = userData.resident_registration_num;
                    if (phonenumText != null) phonenumText.text = userData.phone_num;
                    if (medicalhistoryText != null) medicalhistoryText.text = "병력: " + userData.medical_history;
                    if (chronicdiseaseText != null) chronicdiseaseText.text = "만성질환: " + userData.chronic_disease;
                    if (medicationText != null) medicationText.text = "복용약물: " + userData.medication;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parsing Error: " + e.Message);
                }
            }
        }
    }

}
