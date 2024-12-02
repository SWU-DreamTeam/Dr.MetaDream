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
    public InputField AESGCMpassInputField; // ����� �Է� ��й�ȣ
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
        // GameManager �ν��Ͻ� ��������
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            UnityEngine.Debug.LogError("GameManager instance not found.");
            return;
        }

        // BlockchainTransaction �ν��Ͻ� ��������
        blockchainTransaction = FindObjectOfType<BlockchainTransaction>();
        if (blockchainTransaction == null)
        {
            UnityEngine.Debug.LogError("BlockchainTransaction instance not found in the scene.");
            return;
        }

        // �ǻ� ���� �ʱ�ȭ
        InitializeDoctorInfo();

        // Toggle ������ �߰�
        MedicalcertificateDateToggle.onValueChanged.AddListener(delegate { ToggleMedicalCertificateDate(); });

    }

    private void InitializeDoctorInfo()
    {
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "Guest");
        string loggedInUser = PlayerPrefs.GetString("LoggedInUser", "Guest");
        StartCoroutine(FetchDoctorNum(loggedInUser));

        if (doctornumText != null && doctornameText != null)
        {
            doctornameText.text = loggedInUsername; // �ǻ� �̸� ����
        }
        else
        {
            UnityEngine.Debug.LogError("Doctor Text UI elements are not assigned.");
        }

        // Private Key ��������
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
                    doctornumText.text = doctorData.doctor_num; // �ǻ� ��ȣ ����

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
            Debug.Log("FetchPatientDataByName �Լ� ȣ��");
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
                    // JSON ������ �Ľ� (Newtonsoft.Json ���)
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

                        // UI�� �� ǥ��
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

    // JSON �Ľ��� ���� ������ Ŭ����
    [Serializable]
    public class PatientData
    {
        public string patient_num;
        public string name;
        public string sc_address;
    }
    public void Btn_Medicalcertificate_createBtn()
    {
        // PopupPanel Ȱ��ȭ
        PopupPanelObj.SetActive(true);
    }

    public void OnSendButtonClicked()
    {
        Debug.Log("Send ��ư Ŭ����: ����Ű ��ȣȭ �� ���ü�� Ʈ����� ���� ����");

        // ����� �Է� ��й�ȣ ��������
        string userPassword = AESGCMpassInputField.text;

        // ��й�ȣ ��ȿ�� �˻�
        if (string.IsNullOrEmpty(userPassword) || userPassword.Length != 8)
        {
            Debug.LogError("��й�ȣ�� 8�ڸ����� �մϴ�.");
            return;
        }

        // ����Ű ��ȣȭ
        try
        {
            decryptedPrivateKey = AESGCMEncryption.Decrypt(doctorPrivateKey, userPassword);
            Debug.Log("��ȣȭ�� ����Ű: " + decryptedPrivateKey);

            // ��ȣȭ ���� �� ���ü�� Ʈ����� ���� ���� ����
            StartCoroutine(BlockchainTransaction_createAndSendToServer());
        }
        catch (Exception e)
        {
            Debug.LogError("����Ű ��ȣȭ ����: " + e.Message);
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

        // ���ü�� Ʈ����� ����
        if (blockchainTransaction != null)
        {
            blockchainTransaction.SetTransactionAddresses(decryptedPrivateKey, patientAddress);
            blockchainTransaction.SendTransaction(certificateData);
            Debug.Log("Transaction sent to blockchain.");

            // Ʈ����� ������ �Ϸ�� �� ������ ������ ����
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
            certificateDate = ""; // ����ڰ� ���� �����ϰų� ������ �� �ֵ��� �� ������ ����
        }

        // WWWForm�� ����Ͽ� ������ ������ ������ �߰�
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

                // ���� ���信 ���� �߰� �۾� ����
                if (www.downloadHandler.text.Contains("success"))
                {
                    UnityEngine.Debug.Log("Medical certificate record created successfully!");
                    PopupPanelObj.SetActive(false); // �۾� �Ϸ� �� PopupPanel ��Ȱ��ȭ
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
