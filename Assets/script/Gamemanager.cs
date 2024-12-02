using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("LoginPanel")]
    public InputField IDInputField;
    public InputField PassInputField;
    public GameObject LoginPanelObj;
    public PhotonTest photonTest;

    [Header("CreateAccountPatientPanel")]
    public InputField New_IDInputField;
    public InputField New_PassInputField;
    public InputField New_NameInputField;
    public InputField New_SexInputField;
    public InputField New_ResiNumInputField;
    public InputField New_PhoneInputField;
    public GameObject CreateAccountPatientPanelObj;

    [Header("CreateAccountDoctorPanel")]
    public InputField DNew_IDInputField;
    public InputField DNew_PassInputField;
    public InputField DNew_NameInputField;
    public InputField DNew_SexInputField;
    public InputField DNew_ResiNumInputField;
    public InputField DNew_PhoneInputField;
    public GameObject CreateAccountDoctorPanelObj;

    [Header("ChoicePanel")]
    public GameObject ChoicePanelObj;

    [Header("PatientPanel")]
    public InputField New_MedicalHisInputField;
    public InputField New_ChronicDiseaseInputField;
    public InputField New_MedicationInputField;
    public GameObject PatientPanelObj;

    [Header("DoctorPanel")]
    public InputField DNew_CertificateInputField;
    public InputField DNew_MedicaldepartmentInputField;
    public GameObject DoctorPanelObj;

    [Header ("PopupPanel")] // PopupPanel GameObject
    public InputField AESGCMpassInputField; // 사용자 입력 비밀번호
    public TextMeshProUGUI sc_addressText; // 지갑 주소 출력 Text
    public TextMeshProUGUI private_keyText; // 개인 키 출력 Text
    public Button sendButton;
    public GameObject PopupPanelObj;

    private string sc_address; // 생성된 지갑 주소
    private string privateKey; // 생성된 개인키
    private string encryptedPrivateKey; // 암호화된 개인키
    private string decryptedPrivateKey; // 암호화된 개인키
    public string LoginUrl;
    public string CreateUrl;
    public RunNodeScript runNodeScript;

    private string userAddress;
    private string userPrivateKey;
    public string loggedInUsername;
    public string loggedInUser;
    public string userType;
    private bool isWalletInfoLoaded = false;

    public bool IsWalletInfoLoaded => isWalletInfoLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoginUrl = "http://pbl.dothome.co.kr/Login3.php?";
        CreateUrl = "http://pbl.dothome.co.kr/Create.php?";
    }

    public void OpenCreateAccountBtn()
    {
        ClearCreateAccountFields();
        ChoicePanelObj.SetActive(true);
    }

    public void DoctorCreateAccountBtn()
    {
        StartCoroutine(CreateAccount(
            "doctor",
            DNew_IDInputField.text,
            DNew_PassInputField.text,
            DNew_NameInputField.text,
            DNew_SexInputField.text,
            DNew_ResiNumInputField.text,
            DNew_PhoneInputField.text,
            DNew_CertificateInputField.text,
            DNew_MedicaldepartmentInputField.text
        ));

    }

    public void PatientCreateAccountBtn()
    {
        StartCoroutine(CreateAccount(
             "patient",
             New_IDInputField.text,
             New_PassInputField.text,
             New_NameInputField.text,
             New_SexInputField.text,
             New_ResiNumInputField.text,
             New_PhoneInputField.text,
             null,
             null,
             New_MedicalHisInputField.text,
             New_ChronicDiseaseInputField.text,
             New_MedicationInputField.text
         ));
    }

    public void BackBtn()
    {
        LoginPanelObj.SetActive(true);
        CreateAccountPatientPanelObj.SetActive(false);
        CreateAccountDoctorPanelObj.SetActive(false);
        ChoicePanelObj.SetActive(false);
        PatientPanelObj.SetActive(false);
        DoctorPanelObj.SetActive(false);
    }

    public void DBackBtn()
    {
        LoginPanelObj.SetActive(false);
        CreateAccountPatientPanelObj.SetActive(false);
        CreateAccountDoctorPanelObj.SetActive(true);
        ChoicePanelObj.SetActive(false);
        PatientPanelObj.SetActive(false);
        DoctorPanelObj.SetActive(false);
    }

    public void CreateAccountPatientBackBtn()
    {
        LoginPanelObj.SetActive(false);
        CreateAccountPatientPanelObj.SetActive(true);
        CreateAccountDoctorPanelObj.SetActive(false);
        ChoicePanelObj.SetActive(false);
        PatientPanelObj.SetActive(false);
    }

    public void CreateAccountDoctorBackBtn()
    {
        LoginPanelObj.SetActive(false);
        CreateAccountPatientPanelObj.SetActive(false);
        CreateAccountDoctorPanelObj.SetActive(true);
        ChoicePanelObj.SetActive(false);
        PatientPanelObj.SetActive(false);
    }

    public void PatientBtn()
    {
        CreateAccountPatientPanelObj.SetActive(true);
        ChoicePanelObj.SetActive(false);
    }

    public void DoctorBtn()
    {
        CreateAccountDoctorPanelObj.SetActive(true);
        CreateAccountPatientPanelObj.SetActive(false);
        ChoicePanelObj.SetActive(false);
    }

    public void NextBtn()
    {
        PatientPanelObj.SetActive(true);
        ChoicePanelObj.SetActive(false);
    }

    public void DNextBtn()
    {
        DoctorPanelObj.SetActive(true);
        ChoicePanelObj.SetActive(false);
    }


    public void LoginBtn()
    {
        StartCoroutine(LoginCo());
    }

    IEnumerator LoginCo()
    {
        string hashedPassword = ComputeSha256Hash(PassInputField.text);

        WWWForm form = new WWWForm();
        form.AddField("Input_user", IDInputField.text);
        form.AddField("Input_pass", hashedPassword);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(LoginUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError("로그인 요청 실패: " + webRequest.error);
            }
            else
            {
                string responseText = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                UnityEngine.Debug.Log("서버 응답: " + responseText);

                if (responseText.Contains("Login-Success!!"))
                {
                    string[] responseParts = responseText.Split('_');
                    if (responseParts.Length > 3)
                    {
                        loggedInUsername = responseParts[2];
                        loggedInUser = responseParts[1];
                        userType = responseParts[3].Trim();

                        // 민감하지 않은 데이터는 PlayerPrefs에 저장
                        PlayerPrefs.SetString("LoggedInUsername", loggedInUsername);
                        PlayerPrefs.SetString("LoggedInUser", loggedInUser);
                        PlayerPrefs.SetString("UserType", userType);
                        PlayerPrefs.Save();

                        UnityEngine.Debug.Log($"로그인 성공. 사용자: {loggedInUsername}, 타입: {userType}");

                        photonTest.Connect();
                        yield return StartCoroutine(FetchWalletInfo(loggedInUser));

                        // userType에 따라 추가 로직
                        if (userType == "doctor")
                        {
                            UnityEngine.Debug.Log("Doctor 사용자입니다.");
                        }
                        else if (userType == "patient")
                        {
                            UnityEngine.Debug.Log("Patient 사용자입니다.");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("서버 응답에서 유효한 사용자 정보가 없습니다.");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("로그인 실패: 유효하지 않은 자격 증명입니다.");
                }
            }
        }
    }


    IEnumerator FetchWalletInfo(string userId)
    {
        WWWForm form = new WWWForm();
        form.AddField("Input_user", userId);

        using (UnityWebRequest webRequest = UnityWebRequest.Post("http://pbl.dothome.co.kr/GetWalletInfo2.php", form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError("지갑 정보 가져오기 오류: " + webRequest.error);
            }
            else
            {
                string responseText = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                UnityEngine.Debug.Log("지갑 정보 응답: " + responseText);

                var jsonResponse = JsonUtility.FromJson<WalletResponse>(responseText);

                if (jsonResponse.status == "success")
                {
                    userAddress = jsonResponse.sc_address;
                    userPrivateKey = jsonResponse.sc_privatekey;
                    isWalletInfoLoaded = true;
                    UnityEngine.Debug.Log("지갑 정보가 메모리에 저장되었습니다.");
                }
                else
                {
                    UnityEngine.Debug.LogError("지갑 주소 또는 개인 키 로드 실패: " + jsonResponse.message);
                }
            }
        }
    }

    public (string, string) GetWalletInfo()
    {
        if (!isWalletInfoLoaded)
        {
            UnityEngine.Debug.LogWarning("지갑 정보가 아직 로드되지 않았습니다.");
            return (null, null);
        }
        return (userPrivateKey, userAddress);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    private void ResetSensitiveData()
    {
        userAddress = string.Empty;
        userPrivateKey = string.Empty;
        isWalletInfoLoaded = false;
    }


    [Serializable]
    private class WalletResponse
    {
        public string status;
        public string sc_address;
        public string sc_privatekey;
        public string message;
    }


    IEnumerator CreateAccount(string userType, string id, string password, string name, string sex, string residentNum, string phoneNum, string certification = null, string department = null, string medicalHistory = null, string chronicDisease = null, string medication = null)
    {
        // 지갑 생성
        Task<(string address, string privateKey)> walletTask = runNodeScript.RunJavaScript(id);
        yield return new WaitUntil(() => walletTask.IsCompleted);

        if (walletTask.IsFaulted)
        {
            UnityEngine.Debug.LogError("지갑 주소 생성 중 오류 발생: " + walletTask.Exception.Flatten().Message);
            yield break;
        }

        // 지갑 주소 및 개인키 가져오기
        (sc_address, privateKey) = walletTask.Result;

        if (!string.IsNullOrEmpty(sc_address) && !string.IsNullOrEmpty(privateKey))
        {
            // UI에 지갑 정보 표시
            sc_addressText.text = sc_address;
            private_keyText.text = privateKey;

            // PopupPanel 활성화 (비밀번호 입력 대기)
            PopupPanelObj.SetActive(true);
            Debug.Log("지갑 주소와 개인키가 생성되었습니다. 사용자 입력을 대기합니다...");

            // Send 버튼 클릭 시 실행될 로직
            sendButton.onClick.RemoveAllListeners(); // 이전 이벤트 제거
            sendButton.onClick.AddListener(() =>
            {
                Debug.Log("Send 버튼 클릭됨: 개인키 암호화 및 전송 시작");

                // 비밀번호 입력값 가져오기
                string userPassword = AESGCMpassInputField.text;

                // 비밀번호 유효성 검사
                if (string.IsNullOrEmpty(userPassword) || userPassword.Length != 8)
                {
                    Debug.LogError("비밀번호는 8자리여야 합니다.");
                    return;
                }

                // 개인키 암호화
                encryptedPrivateKey = AESGCMEncryption.Encrypt(privateKey, userPassword);
                Debug.Log("암호화된 개인키: " + encryptedPrivateKey);

                // 서버로 전송
                StartCoroutine(SendAccountToServer(userType, id, password, name, sex, residentNum, phoneNum, encryptedPrivateKey, certification, department, medicalHistory, chronicDisease, medication));
            });
        }
        else
        {
            Debug.LogError("지갑 주소 또는 개인키 생성 실패");
        }
    }

    IEnumerator SendAccountToServer(string userType, string id, string password, string name, string sex, string residentNum, string phoneNum, string encryptedPrivateKey, string certification = null, string department = null, string medicalHistory = null, string chronicDisease = null, string medication = null)
    {
        // 비밀번호 해싱
        string hashedPassword = ComputeSha256Hash(password);

        // form 객체 생성
        WWWForm form = new WWWForm();
        form.AddField("Input_user", id);
        form.AddField("Input_pass", hashedPassword);
        form.AddField("Input_name", name);
        form.AddField("Input_sex", sex);
        form.AddField("Input_resident_registration_num", residentNum);
        form.AddField("Input_phone_num", phoneNum);
        form.AddField("user_type", userType);
        form.AddField("sc_address", sc_address);
        form.AddField("sc_privatekey", encryptedPrivateKey);

        // 사용자 유형에 따른 추가 데이터
        if (userType == "doctor")
        {
            if (!string.IsNullOrEmpty(certification)) form.AddField("certification", certification);
            if (!string.IsNullOrEmpty(department)) form.AddField("medical_department", department);
        }
        else if (userType == "patient")
        {
            if (!string.IsNullOrEmpty(medicalHistory)) form.AddField("medical_history", medicalHistory);
            if (!string.IsNullOrEmpty(chronicDisease)) form.AddField("chronic_disease", chronicDisease);
            if (!string.IsNullOrEmpty(medication)) form.AddField("medication", medication);
        }

        // 서버로 요청 보내기
        using (UnityWebRequest webRequest = UnityWebRequest.Post(CreateUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("서버 전송 실패: " + webRequest.error);
            }
            else
            {
                string responseText = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                Debug.Log("서버 응답: " + responseText);

                if (responseText.Contains("Create Doctor Success") || responseText.Contains("Create Patient Success"))
                {
                    Debug.Log("서버로 성공적으로 전송됨!");
                    PopupPanelObj.SetActive(false);

                    Debug.Log("회원가입 완료! 로그인 패널로 전환합니다.");
                    BackBtn(); // 로그인 패널로 돌아가는 메서드 호출
                }
                else
                {
                    Debug.LogError("서버 전송 실패: 유효하지 않은 응답");
                }
            }
        }
    }


    private void ClearCreateAccountFields()
    {
        New_IDInputField.text = "";
        New_PassInputField.text = "";
        New_NameInputField.text = "";
        New_SexInputField.text = "";
        New_ResiNumInputField.text = "";
        New_PhoneInputField.text = "";

        New_MedicalHisInputField.text = "";
        New_ChronicDiseaseInputField.text = "";
        New_MedicationInputField.text = "";

        DNew_IDInputField.text = "";
        DNew_PassInputField.text = "";
        DNew_NameInputField.text = "";
        DNew_SexInputField.text = "";
        DNew_ResiNumInputField.text = "";
        DNew_PhoneInputField.text = "";

        DNew_CertificateInputField.text = "";
        DNew_MedicaldepartmentInputField.text = "";
    }

    public string GetDoctorPrivateKey()
    {
        return userPrivateKey;
    }
}
