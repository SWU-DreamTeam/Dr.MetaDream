using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PrescriptionLoader : MonoBehaviour
{
    public TMP_Text prescriptionNumText; // 추가: prescription_num을 표시할 Text
    public TMP_Text prescriptionDateText;
    public TMP_Text medicineNameText;
    public TMP_Text medicineInfoText;
    public TMP_Text doseText;
    public UnityEngine.UI.Image medicineImage;
    public TMP_Text doctorNameText;
    public TMP_Text patientNameText;

    private string baseUrl = "http://pbl.dothome.co.kr/get_prescription.php";

    void Start()
    {
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "default_user");
        StartCoroutine(GetPrescriptionData(loggedInUsername));
    }

    IEnumerator GetPrescriptionData(string username)
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

                PrescriptionData prescription = JsonUtility.FromJson<PrescriptionData>(jsonResult);

                if (prescription != null)
                {
                    UpdateUI(prescription);
                    StartCoroutine(LoadImage(prescription.p_imgsrc));
                }
                else
                {
                    Debug.LogError("Failed to parse JSON data.");
                }
            }
        }
    }

    void UpdateUI(PrescriptionData prescription)
    {
        if (prescriptionNumText != null)
            prescriptionNumText.text = prescription.prescription_num.ToString();
        if (prescriptionDateText != null)
            prescriptionDateText.text = prescription.prescription_date;
        if (medicineNameText != null)
            medicineNameText.text = prescription.p_medicine_name;
        if (medicineInfoText != null)
            medicineInfoText.text = prescription.p_medicine_info;
        if (doseText != null)
            doseText.text = prescription.p_dose;
        if (doctorNameText != null)
            doctorNameText.text = prescription.d_user;
        if (patientNameText != null)
            patientNameText.text = prescription.p_user;

        Debug.Log($"Updated UI - Prescription Number: {prescription.prescription_num}");
        Debug.Log($"Updated UI - Medicine Name: {prescription.p_medicine_name}");
        Debug.Log($"Updated UI - Dose: {prescription.p_dose}");
    }

    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Image Load Error: {request.error}");
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                if (medicineImage != null)
                {
                    medicineImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogError("Medicine Image component is not assigned.");
                }
            }
        }
    }
}

[System.Serializable]
public class PrescriptionData
{
    public int prescription_num;
    public string prescription_date;
    public string p_medicine_name;
    public string p_medicine_info;
    public string p_dose;
    public string p_imgsrc;
    public string d_user;
    public string p_user;
}