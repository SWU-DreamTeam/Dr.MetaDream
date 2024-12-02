using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class MedicalCertificateLoader : MonoBehaviour
{
    public TMP_Text patientNameText;
    public TMP_Text doctorNameText;
    public TMP_Text certificateDateText;
    public TMP_Text symptomText;
    public TMP_Text diagnosisText;

    private string baseUrl = "http://pbl.dothome.co.kr/get_medical_certificate.php";

    void Start()
    {
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "default_user");
        StartCoroutine(GetMedicalCertificateData(loggedInUsername));
    }

    IEnumerator GetMedicalCertificateData(string username)
    {
        string url = $"{baseUrl}?username={UnityWebRequest.EscapeURL(username)}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                string jsonResult = request.downloadHandler.text;
                Debug.Log($"JSON Data: {jsonResult}");

                MedicalCertificateData certificate = JsonUtility.FromJson<MedicalCertificateData>(jsonResult);

                if (certificate != null)
                {
                    UpdateUI(certificate);
                }
                else
                {
                    Debug.LogError("Failed to parse JSON data.");
                }
            }
        }
    }

    void UpdateUI(MedicalCertificateData certificate)
    {
        if (patientNameText != null)
            patientNameText.text = certificate.p_user;
        if (doctorNameText != null)
            doctorNameText.text = certificate.d_user;
        if (certificateDateText != null)
            certificateDateText.text = certificate.medical_certificate_date;
        if (symptomText != null)
            symptomText.text = certificate.symptom;
        if (diagnosisText != null)
            diagnosisText.text = certificate.diagnosis;

        Debug.Log($"Updated UI - Patient: {certificate.p_user}, Doctor: {certificate.d_user}");
        Debug.Log($"Updated UI - Date: {certificate.medical_certificate_date}");
        Debug.Log($"Updated UI - Symptom: {certificate.symptom}");
        Debug.Log($"Updated UI - Diagnosis: {certificate.diagnosis}");
    }
}

[System.Serializable]
public class MedicalCertificateData
{
    public string p_user;
    public string d_user;
    public string medical_certificate_date;
    public string symptom;
    public string diagnosis;
}