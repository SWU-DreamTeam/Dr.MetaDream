using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    private string loginUrl = "http://pbl.dothome.co.kr/login3.php"; // 서버의 로그인 URL로 변경

    public IEnumerator Login(string user, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("Input_user", user);
        form.AddField("Input_pass", password);

        using (UnityWebRequest request = UnityWebRequest.Post(loginUrl, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Login error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);

                LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);
                if (response.status == "success")
                {
                    PlayerPrefs.SetString("user_id", response.user_id);
                    PlayerPrefs.SetString("username", response.username);
                    Debug.Log("Login successful!");
                }
                else
                {
                    Debug.Log("Login failed: " + response.message);
                }
            }
        }
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string status;
        public string user_id;
        public string username;
        public string message;
    }
}
