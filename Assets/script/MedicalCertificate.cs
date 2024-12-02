using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;



public class MedicalCertificate : MonoBehaviour
{
    public TextMeshProUGUI doctornumText;
    public TextMeshProUGUI doctornameText;
    public TextMeshProUGUI patientnumText;
    public TextMeshProUGUI patientnameText;

    public InputField DiagnosisInputField;
    public InputField SymptomInputField;
    public Toggle MedicalcertificateDateToggle;
    public TextMeshProUGUI MedicalCertificateDateText;
    public Button Btn_Medicalcertificate_create;


    [Header("PopupPanel")] // PopupPanel GameObject
    public InputField AESGCMpassInputField; // 사용자 입력 비밀번호
    public Button sendButton;
    public GameObject PopupPanelObj;

    private string fetchDoctorNumUrl = "http://pbl.dothome.co.kr/fetch_doctor_num.php";
    private string fetchPatientDataUrl = "http://pbl.dothome.co.kr/fetch_patient_info3.php";

    private BlockchainTransaction blockchainTransaction;
    private GameManager gameManager;

    private string decryptedPrivateKey;
    private string doctorPrivateKey;
    private string patientAddress;

    [System.Serializable]
    public class MedicalcertificateData
    {
        public string doctor_num;
        public string doctorname;
        public string patient_num;
        public string patientname;
        public string symptom;
        public string diagnosis;
        public string medical_certificate_date;
    }

    void Start()
    {
        // GameManager 인스턴스 가져오기
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            UnityEngine.Debug.LogError("GameManager instance not found.");
            return;
        }

        // BlockchainTransaction 인스턴스 가져오기
        blockchainTransaction = FindObjectOfType<BlockchainTransaction>();
        if (blockchainTransaction == null)
        {
            UnityEngine.Debug.LogError("BlockchainTransaction instance not found in the scene.");
            return;
        }

        // 의사 정보 초기화
        InitializeDoctorInfo();

        // Toggle 리스너 추가
        MedicalcertificateDateToggle.onValueChanged.AddListener(delegate { ToggleMedicalCertificateDate(); });

    }

    private void InitializeDoctorInfo()
    {
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "Guest");
        string loggedInUser = PlayerPrefs.GetString("LoggedInUser", "Guest");
        StartCoroutine(FetchDoctorNum(loggedInUser));

        if (doctornumText != null && doctornameText != null)
        {
            doctornameText.text = loggedInUsername; // 의사 이름 설정
        }
        else
        {
            UnityEngine.Debug.LogError("Doctor Text UI elements are not assigned.");
        }

        // Private Key 가져오기
        doctorPrivateKey = gameManager.GetDoctorPrivateKey();
        if (string.IsNullOrEmpty(doctorPrivateKey))
        {
            UnityEngine.Debug.LogError("Doctor Private Key not found.");
        }
    }

    IEnumerator FetchDoctorNum(string loggedInUsername)
    {
        //string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "Guest");
        string urlWithParams = fetchDoctorNumUrl + "?user=" + UnityWebRequest.EscapeURL(loggedInUsername);

        UnityEngine.Debug.Log("Request URL for doctor number: " + urlWithParams);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlWithParams))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError("Error fetching doctor number: " + webRequest.error);
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            UnityEngine.Debug.Log("Doctor number response: " + jsonResponse);

            try
            {
                MedicalcertificateData doctorData = JsonUtility.FromJson<MedicalcertificateData>(jsonResponse);

                if (doctorData != null && !string.IsNullOrEmpty(doctorData.doctor_num))
                {
                    doctornumText.text = doctorData.doctor_num; // 의사 번호 설정

                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to parse doctor number response.");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("JSON Parsing Error: " + e.Message);
            }
        }
    }

    private string patientNameToLoad;
    private bool shouldLoadPatientInfo = false;

    void OnEnable()
    {
        if (shouldLoadPatientInfo)
        {
            LoadPatientInfo(patientNameToLoad);
            shouldLoadPatientInfo = false;
        }
    }

    public void SetPatientInfo(string patientName)
    {
        patientNameToLoad = patientName;
        CoroutineManager.Instance.StartManagedCoroutine(LoadPatientInfo(patientName));
    }


    public IEnumerator LoadPatientInfo(string patientName)
    {
        if (gameObject.activeSelf)
        {
            yield return CoroutineManager.Instance.StartManagedCoroutine(FetchPatientDataByName(patientName));
            Debug.Log("FetchPatientDataByName 함수 호출");
        }
        else
        {
            Debug.LogWarning("GameObject is inactive. Patient info will be loaded when it becomes active.");
            shouldLoadPatientInfo = true;
        }
    }

    IEnumerator FetchPatientDataByName(string patientName)
    {
        string urlWithParams = fetchPatientDataUrl + "?patient_name=" + UnityWebRequest.EscapeURL(patientName);
        Debug.Log("Request URL for patient data: " + urlWithParams);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlWithParams))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("WebRequest Error: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                Debug.Log("Response from server for patient data: " + jsonResponse);

                try
                {
                    // JSON 데이터 파싱 (Newtonsoft.Json 사용)
                    JObject json = JObject.Parse(jsonResponse);

                    if (json["error"] != null)
                    {
                        Debug.LogError("Error from server: " + json["error"].ToString());
                    }
                    else
                    {
                        string patientNum = json["patient_num"]?.ToString();
                        string name = json["name"]?.ToString();
                        string scAddress = json["sc_address"]?.ToString();

                        // UI에 값 표시
                        patientnumText.text = patientNum;
                        patientnameText.text = name;
                        patientAddress = scAddress;

                        Debug.Log($"Fetched patient info:");
                        Debug.Log($"Patient Num: {patientNum}");
                        Debug.Log($"Name: {name}");
                        Debug.Log($"Blockchain Address: {scAddress}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON Parsing Error: " + e.Message);
                }
            }
        }
    }

    // JSON 파싱을 위한 데이터 클래스
    [Serializable]
    public class PatientData
    {
        public string patient_num;
        public string name;
        public string sc_address;
    }
    public void Btn_Medicalcertificate_createBtn()
    {
        // PopupPanel 활성화
        PopupPanelObj.SetActive(true);
    }

    public void OnSendButtonClicked()
    {
        Debug.Log("Send 버튼 클릭됨: 개인키 복호화 및 블록체인 트랜잭션 생성 시작");

        // 사용자 입력 비밀번호 가져오기
        string userPassword = AESGCMpassInputField.text;

        // 비밀번호 유효성 검사
        if (string.IsNullOrEmpty(userPassword) || userPassword.Length != 8)
        {
            Debug.LogError("비밀번호는 8자리여야 합니다.");
            return;
        }

        // 개인키 복호화
        try
        {
            decryptedPrivateKey = AESGCMEncryption.Decrypt(doctorPrivateKey, userPassword);
            Debug.Log("복호화된 개인키: " + decryptedPrivateKey);

            // 복호화 성공 후 블록체인 트랜잭션 생성 로직 실행
            StartCoroutine(BlockchainTransaction_createAndSendToServer());
        }
        catch (Exception e)
        {
            Debug.LogError("개인키 복호화 실패: " + e.Message);
        }
    }

    private IEnumerator BlockchainTransaction_createAndSendToServer()
    {
        string certificateDate = MedicalcertificateDateToggle.isOn
            ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            : "";

        MedicalcertificateData certificateData = new MedicalcertificateData
        {
            doctor_num = doctornumText.text,
            doctorname = doctornameText.text,
            patient_num = patientnumText.text,
            patientname = patientnameText.text,
            symptom = SymptomInputField.text,
            diagnosis = DiagnosisInputField.text,
            medical_certificate_date = certificateDate
        };

        // 블록체인 트랜잭션 생성
        if (blockchainTransaction != null)
        {
            blockchainTransaction.SetTransactionAddresses(decryptedPrivateKey, patientAddress);
            blockchainTransaction.SendTransaction(certificateData);
            Debug.Log("Transaction sent to blockchain.");

            // 트랜잭션 생성이 완료된 후 서버로 데이터 전송
            yield return StartCoroutine(Medicalcertificate_createCo());
        }
        else
        {
            Debug.LogError("BlockchainTransaction instance not found.");
        }
    }

    IEnumerator Medicalcertificate_createCo()
    {
        string certificateDate;
        if (MedicalcertificateDateToggle.isOn)
        {
            certificateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            certificateDate = ""; // 사용자가 직접 설정하거나 선택할 수 있도록 빈 값으로 설정
        }

        // WWWForm을 사용하여 서버로 전송할 데이터 추가
        WWWForm form = new WWWForm();
        form.AddField("doctor_num", doctornumText.text);
        form.AddField("patient_num", patientnumText.text);
        form.AddField("symptom", SymptomInputField.text);
        form.AddField("diagnosis", DiagnosisInputField.text);
        form.AddField("medical_certificate_date", certificateDate);
        form.AddField("p_user", patientnameText.text);
        form.AddField("d_user", doctornameText.text);

        using (UnityWebRequest www = UnityWebRequest.Post("http://pbl.dothome.co.kr/Medical_certificate.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                UnityEngine.Debug.LogError("Error while sending: " + www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Form upload complete! Server response: " + www.downloadHandler.text);

                // 서버 응답에 따라 추가 작업 수행
                if (www.downloadHandler.text.Contains("success"))
                {
                    UnityEngine.Debug.Log("Medical certificate record created successfully!");
                    PopupPanelObj.SetActive(false); // 작업 완료 후 PopupPanel 비활성화
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to create medical certificate record. Server response: " + www.downloadHandler.text);
                }
            }
        }
    }



    public void ToggleMedicalCertificateDate()
    {
        if (MedicalcertificateDateToggle.isOn)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (MedicalCertificateDateText != null)
            {
                MedicalCertificateDateText.text = currentDate;
            }
        }
        else
        {
            if (MedicalCertificateDateText != null)
            {
                MedicalCertificateDateText.text = "Date not selected";
            }
        }
    }
}
