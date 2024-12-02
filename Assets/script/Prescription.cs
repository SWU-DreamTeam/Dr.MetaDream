using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;
using System.Text;



public class Prescription : MonoBehaviour
{
    public TextMeshProUGUI doctor_numText;
    public TextMeshProUGUI patient_numText;
    public TextMeshProUGUI doctor_nameText;
    public TextMeshProUGUI patient_nameText;
    public TextMeshProUGUI medical_certificate_num;
    public Toggle PrescriptionDateToggle;
    public TextMeshProUGUI PrescriptionOutputText;

    public GameObject medicineTextPrefab;
    public Transform scrollViewContent;

    public GameObject PopupPanelObj; // Popup Panel for password input
    public InputField AESGCMpassInputField; // Password input field
    public Button SendPasswordButton; // Button to confirm password input

    private string doctorPrivateKey;
    private string patientAddress;
    private string userPassword; // Password received from MedicalCertificate

    private List<MedicineInfo> selectedMedicines = new List<MedicineInfo>();
    public BlockchainTransaction blockchainTransaction;

    private string latestCertificateUrl = "http://pbl.dothome.co.kr/GetLatestCertificate.php";
    private string prescriptionUrl = "http://pbl.dothome.co.kr/Prescription_f6.php";
    private string getMedicineUrl = "http://pbl.dothome.co.kr/GetMedicine.php";
    private string fetchCertificateUrl = "http://pbl.dothome.co.kr/GetcertificateInfo.php";

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }

        // Try to retrieve the userPassword set by MedicalCertificate
        userPassword = PlayerPrefs.GetString("UserPassword", null);

        doctorPrivateKey = gameManager.GetDoctorPrivateKey();
        StartCoroutine(InitializeAfterWalletLoad());
    }

    IEnumerator InitializeAfterWalletLoad()
    {
        while (!gameManager.IsWalletInfoLoaded)
        {
            yield return null;
        }

        doctorPrivateKey = gameManager.GetDoctorPrivateKey();
        Debug.Log("Doctor's Private Key: " + doctorPrivateKey);

        // Fetch the latest certificate number
        yield return StartCoroutine(FetchLatestCertificateNum());

        if (string.IsNullOrEmpty(patientAddress))
        {
            Debug.LogError("Patient address not found.");
            yield break;
        }

        if (blockchainTransaction == null)
        {
            blockchainTransaction = FindObjectOfType<BlockchainTransaction>();
            if (blockchainTransaction == null)
            {
                Debug.LogError("BlockchainTransaction instance not found in the scene.");
            }
        }

        blockchainTransaction.SetTransactionAddresses(doctorPrivateKey, patientAddress);

        PrescriptionDateToggle.isOn = true;
        PrescriptionDateToggle.onValueChanged.AddListener(delegate { TogglePrescriptionDate(); });

        // Fetch medicine data
        yield return StartCoroutine(GetMedicineData());
    }

    private void TogglePrescriptionDate()
    {
        string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        if (PrescriptionOutputText != null)
        {
            PrescriptionOutputText.text = PrescriptionDateToggle.isOn ? currentDate : "Date not selected";
        }
    }

    private IEnumerator FetchLatestCertificateNum()
    {
        UnityWebRequest request = UnityWebRequest.Get(latestCertificateUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching latest certificate: " + request.error);
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;

        CertificateNumResponse response = JsonUtility.FromJson<CertificateNumResponse>(jsonResponse);
        if (response != null && !string.IsNullOrEmpty(response.medical_certificate_num))
        {
            yield return StartCoroutine(FetchCertificateInfo(response.medical_certificate_num));
        }
        else
        {
            Debug.LogError("Failed to fetch latest certificate number.");
        }
    }

    private IEnumerator FetchCertificateInfo(string certificateNum)
    {
        string url = fetchCertificateUrl + "?certificate_num=" + certificateNum;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching certificate info: " + request.error);
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;

        CertificateData data = JsonUtility.FromJson<CertificateData>(jsonResponse);
        if (data != null)
        {
            doctor_numText.text = data.doctor_num;
            patient_numText.text = data.patient_num;
            doctor_nameText.text = data.d_user;
            patient_nameText.text = data.p_user;
            medical_certificate_num.text = data.medical_certificate_num;

            yield return StartCoroutine(FetchPatientInfo(data.patient_num));
        }
        else
        {
            Debug.LogError("Failed to load certificate data.");
        }
    }

    private IEnumerator FetchPatientInfo(string patientNum)
    {
        string url = "http://pbl.dothome.co.kr/getPatientInfo2.php?patient_num=" + patientNum;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching patient info: " + request.error);
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        JObject parsedResponse = JObject.Parse(jsonResponse);

        if (parsedResponse["error"] != null)
        {
            Debug.LogError("Error in patient info response: " + parsedResponse["error"]);
            yield break;
        }

        patientAddress = parsedResponse["address"]?.ToString();
    }

    private IEnumerator GetMedicineData()
    {
        UnityWebRequest request = UnityWebRequest.Get(getMedicineUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error fetching medicine data: " + request.error);
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        MedicineListWrapper wrapper = JsonUtility.FromJson<MedicineListWrapper>(jsonResponse);

        if (wrapper != null && wrapper.medicines != null)
        {
            AssignMedicineToButtons(wrapper.medicines);
        }
        else
        {
            Debug.LogError("Medicine data parsing error or empty data.");
        }
    }

    private void AssignMedicineToButtons(List<MedicineInfo> medicines)
    {
        Button[] buttons = {
            GameObject.Find("Button").GetComponent<Button>(),
            GameObject.Find("Button2").GetComponent<Button>(),
            GameObject.Find("Button3").GetComponent<Button>(),
            GameObject.Find("Button4").GetComponent<Button>(),
            GameObject.Find("Button5").GetComponent<Button>()
        };

        for (int i = 0; i < buttons.Length && i < medicines.Count; i++)
        {
            int index = i;
            buttons[index].onClick.RemoveAllListeners();
            buttons[index].onClick.AddListener(() => OnMedicineSelected(medicines[index]));
        }
    }

    public void OnMedicineSelected(MedicineInfo medicine)
    {
        if (!selectedMedicines.Contains(medicine))
        {
            selectedMedicines.Add(medicine);
            AddMedicineToScrollView(medicine);
        }
    }

    private void AddMedicineToScrollView(MedicineInfo medicine)
    {
        GameObject newMedicineText = Instantiate(medicineTextPrefab, scrollViewContent);
        TextMeshProUGUI textComponent = newMedicineText.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = $"{medicine.medicine_name} - {medicine.medicine_info} ({medicine.dose})";
        }
    }

    public void OnEnterButtonPressed()
    {
        if (selectedMedicines.Count == 0)
        {
            Debug.LogError("선택된 약이 없습니다.");
            return;
        }

        // userPassword 값 확인
        if (string.IsNullOrEmpty(userPassword))
        {
            // userPassword가 null 또는 비어 있는 경우 PopupPanelObj 활성화
            PopupPanelObj.SetActive(true);

            // SendPasswordButton 클릭 리스너 추가
            SendPasswordButton.onClick.RemoveAllListeners(); // 중복 추가 방지
            SendPasswordButton.onClick.AddListener(() =>
            {
                userPassword = AESGCMpassInputField.text;
                if (!string.IsNullOrEmpty(userPassword) && userPassword.Length == 8)
                {
                    // 비밀번호 저장
                    PlayerPrefs.SetString("UserPassword", userPassword);
                    PopupPanelObj.SetActive(false); // 팝업 패널 비활성화
                    ProcessPrescription(); // 복호화 및 작업 진행
                }
                else
                {
                    Debug.LogError("비밀번호는 8자리여야 합니다.");
                }
            });
        }
        else
        {
            // userPassword 값이 이미 있는 경우 바로 복호화 및 작업 진행
            ProcessPrescription();
        }
    }

    public void ProcessPrescription()
    {
        try
        {
            string decryptedKey = AESGCMEncryption.Decrypt(doctorPrivateKey, userPassword);
            Debug.Log($"Decrypted Private Key: {decryptedKey}");

            blockchainTransaction.SetTransactionAddresses(decryptedKey, patientAddress);

            // 트랜잭션 생성 코루틴 호출
            StartCoroutine(CreateTransactionAndSendPrescription());
        }
        catch (Exception e)
        {
            Debug.LogError("개인키 복호화 실패: " + e.Message);
        }
    }

    // 서버로 데이터 전송하는 메소드, 서버용 데이터 구조를 사용
    IEnumerator SendPrescriptionDataToServer(ServerPrescriptionData data)
    {
        string jsonData = JsonUtility.ToJson(data);

        // 전송할 JSON 데이터 출력
        UnityEngine.Debug.Log("전송할 JSON 데이터: " + jsonData);

        UnityWebRequest request = new UnityWebRequest(prescriptionUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // 요청 결과 처리
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            UnityEngine.Debug.LogError("처방전 전송 실패: " + request.error);
            UnityEngine.Debug.LogError("HTTP 응답 코드: " + request.responseCode);
            UnityEngine.Debug.LogError("서버로부터 받은 원시 응답: " + request.downloadHandler.text);
        }
        else
        {
            UnityEngine.Debug.Log("처방전 전송 성공. 서버 응답: " + request.downloadHandler.text);

            // 서버 응답 JSON 파싱
            try
            {
                JObject serverResponse = JObject.Parse(request.downloadHandler.text);
                if (serverResponse.ContainsKey("error"))
                {
                    UnityEngine.Debug.LogError("서버 오류 메시지: " + serverResponse["error"]);
                }
                else if (serverResponse.ContainsKey("success"))
                {
                    UnityEngine.Debug.Log("서버 성공 메시지: " + serverResponse["success"]);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("서버 응답이 예상 형식이 아닙니다: " + request.downloadHandler.text);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("서버 응답 파싱 중 오류 발생: " + e.Message);
            }
        }
    }

    private IEnumerator CreateTransactionAndSendPrescription()
    {
        // 블록체인에 보낼 데이터 준비
        BlockchainPrescriptionData blockchainData = new BlockchainPrescriptionData
        {
            doctor_num = doctor_numText.text,
            patient_num = patient_numText.text,
            prescription_date = PrescriptionOutputText.text,
            selectedMedicines = selectedMedicines
        };

        UnityEngine.Debug.Log("블록체인 데이터 준비 완료: " + JsonUtility.ToJson(blockchainData));

        // 서버에 보낼 데이터 준비
        ServerPrescriptionData serverData = new ServerPrescriptionData
        {
            doctor_num = doctor_numText.text,
            patient_num = patient_numText.text,
            doctor_name = doctor_nameText.text,
            patient_name = patient_nameText.text,
            medical_certificate_num = medical_certificate_num.text,
            prescription_date = PrescriptionOutputText.text,
            selectedMedicines = selectedMedicines
        };

        UnityEngine.Debug.Log("서버 데이터 준비 완료: " + JsonUtility.ToJson(serverData));

        // 트랜잭션 보내기
        if (blockchainTransaction != null)
        {
            try
            {
                blockchainTransaction.SendTransaction(blockchainData);
                UnityEngine.Debug.Log("블록체인으로 트랜잭션 전송 성공.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("블록체인 트랜잭션 전송 실패: " + e.Message);
            }

            // 서버로 데이터 전송
            yield return StartCoroutine(SendPrescriptionDataToServer(serverData));
        }
        else
        {
            UnityEngine.Debug.LogError("BlockchainTransaction instance가 존재하지 않습니다.");
        }
    }


    [Serializable]
    public class CertificateNumResponse
    {
        public string medical_certificate_num;
    }

    [Serializable]
    public class CertificateData
    {
        public string doctor_num;
        public string patient_num;
        public string d_user;
        public string p_user;
        public string medical_certificate_num;
    }

    [Serializable]
    public class MedicineListWrapper
    {
        public List<MedicineInfo> medicines;
    }

    [Serializable]
    public class MedicineInfo
    {
        public string medicine_num;
        public string medicine_name;
        public string medicine_info;
        public string dose;
    }

    [Serializable]
    public class BlockchainPrescriptionData
    {
        public string doctor_num;
        public string patient_num;
        public string prescription_date;
        public List<MedicineInfo> selectedMedicines;
    }

    [Serializable]
    public class ServerPrescriptionData
    {
        public string doctor_num;
        public string patient_num;
        public string doctor_name;
        public string patient_name;
        public string medical_certificate_num;
        public string prescription_date;
        public List<MedicineInfo> selectedMedicines;
    }
}
