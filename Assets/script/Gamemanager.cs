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
    public InputField AESGCMpassInputField; // ����� �Է� ��й�ȣ
    public TextMeshProUGUI sc_addressText; // ���� �ּ� ��� Text
    public TextMeshProUGUI private_keyText; // ���� Ű ��� Text
    public Button sendButton;
    public GameObject PopupPanelObj;

    private string sc_address; // ������ ���� �ּ�
    private string privateKey; // ������ ����Ű
    private string encryptedPrivateKey; // ��ȣȭ�� ����Ű
    private string decryptedPrivateKey; // ��ȣȭ�� ����Ű
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
                UnityEngine.Debug.LogError("�α��� ��û ����: " + webRequest.error);
            }
            else
            {
                string responseText = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                UnityEngine.Debug.Log("���� ����: " + responseText);

                if (responseText.Contains("Login-Success!!"))
                {
                    string[] responseParts = responseText.Split('_');
                    if (responseParts.Length > 3)
                    {
                        loggedInUsername = responseParts[2];
                        loggedInUser = responseParts[1];
                        userType = responseParts[3].Trim();

                        // �ΰ����� ���� �����ʹ� PlayerPrefs�� ����
                        PlayerPrefs.SetString("LoggedInUsername", loggedInUsername);
                        PlayerPrefs.SetString("LoggedInUser", loggedInUser);
                        PlayerPrefs.SetString("UserType", userType);
                        PlayerPrefs.Save();

                        UnityEngine.Debug.Log($"�α��� ����. �����: {loggedInUsername}, Ÿ��: {userType}");

                        photonTest.Connect();
                        yield return StartCoroutine(FetchWalletInfo(loggedInUser));

                        // userType�� ���� �߰� ����
                        if (userType == "doctor")
                        {
                            UnityEngine.Debug.Log("Doctor ������Դϴ�.");
                        }
                        else if (userType == "patient")
                        {
                            UnityEngine.Debug.Log("Patient ������Դϴ�.");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("���� ���信�� ��ȿ�� ����� ������ �����ϴ�.");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("�α��� ����: ��ȿ���� ���� �ڰ� �����Դϴ�.");
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
                UnityEngine.Debug.LogError("���� ���� �������� ����: " + webRequest.error);
            }
            else
            {
                string responseText = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                UnityEngine.Debug.Log("���� ���� ����: " + responseText);

                var jsonResponse = JsonUtility.FromJson<WalletResponse>(responseText);

                if (jsonResponse.status == "success")
                {
                    userAddress = jsonResponse.sc_address;
                    userPrivateKey = jsonResponse.sc_privatekey;
                    isWalletInfoLoaded = true;
                    UnityEngine.Debug.Log("���� ������ �޸𸮿� ����Ǿ����ϴ�.");
                }
                else
                {
                    UnityEngine.Debug.LogError("���� �ּ� �Ǵ� ���� Ű �ε� ����: " + jsonResponse.message);
                }
            }
        }
    }

    public (string, string) GetWalletInfo()
    {
        if (!isWalletInfoLoaded)
        {
            UnityEngine.Debug.LogWarning("���� ������ ���� �ε���� �ʾҽ��ϴ�.");
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
        // ���� ����
        Task<(string address, string privateKey)> walletTask = runNodeScript.RunJavaScript(id);
        yield return new WaitUntil(() => walletTask.IsCompleted);

        if (walletTask.IsFaulted)
        {
            UnityEngine.Debug.LogError("���� �ּ� ���� �� ���� �߻�: " + walletTask.Exception.Flatten().Message);
            yield break;
        }

        // ���� �ּ� �� ����Ű ��������
        (sc_address, privateKey) = walletTask.Result;

        if (!string.IsNullOrEmpty(sc_address) && !string.IsNullOrEmpty(privateKey))
        {
            // UI�� ���� ���� ǥ��
            sc_addressText.text = sc_address;
            private_keyText.text = privateKey;

            // PopupPanel Ȱ��ȭ (��й�ȣ �Է� ���)
            PopupPanelObj.SetActive(true);
            Debug.Log("���� �ּҿ� ����Ű�� �����Ǿ����ϴ�. ����� �Է��� ����մϴ�...");

            // Send ��ư Ŭ�� �� ����� ����
            sendButton.onClick.RemoveAllListeners(); // ���� �̺�Ʈ ����
            sendButton.onClick.AddListener(() =>
            {
                Debug.Log("Send ��ư Ŭ����: ����Ű ��ȣȭ �� ���� ����");

                // ��й�ȣ �Է°� ��������
                string userPassword = AESGCMpassInputField.text;

                // ��й�ȣ ��ȿ�� �˻�
                if (string.IsNullOrEmpty(userPassword) || userPassword.Length != 8)
                {
                    Debug.LogError("��й�ȣ�� 8�ڸ����� �մϴ�.");
                    return;
                }

                // ����Ű ��ȣȭ
                encryptedPrivateKey = AESGCMEncryption.Encrypt(privateKey, userPassword);
                Debug.Log("��ȣȭ�� ����Ű: " + encryptedPrivateKey);

                // ������ ����
                StartCoroutine(SendAccountToServer(userType, id, password, name, sex, residentNum, phoneNum, encryptedPrivateKey, certification, department, medicalHistory, chronicDisease, medication));
            });
        }
        else
        {
            Debug.LogError("���� �ּ� �Ǵ� ����Ű ���� ����");
        }
    }

    IEnumerator SendAccountToServer(string userType, string id, string password, string name, string sex, string residentNum, string phoneNum, string encryptedPrivateKey, string certification = null, string department = null, string medicalHistory = null, string chronicDisease = null, string medication = null)
    {
        // ��й�ȣ �ؽ�
        string hashedPassword = ComputeSha256Hash(password);

        // form ��ü ����
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

        // ����� ������ ���� �߰� ������
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

        // ������ ��û ������
        using (UnityWebRequest webRequest = UnityWebRequest.Post(CreateUrl, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("���� ���� ����: " + webRequest.error);
            }
            else
            {
                string responseText = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
                Debug.Log("���� ����: " + responseText);

                if (responseText.Contains("Create Doctor Success") || responseText.Contains("Create Patient Success"))
                {
                    Debug.Log("������ ���������� ���۵�!");
                    PopupPanelObj.SetActive(false);

                    Debug.Log("ȸ������ �Ϸ�! �α��� �гη� ��ȯ�մϴ�.");
                    BackBtn(); // �α��� �гη� ���ư��� �޼��� ȣ��
                }
                else
                {
                    Debug.LogError("���� ���� ����: ��ȿ���� ���� ����");
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
