using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Photon.Pun; // Photon 네트워크를 사용하기 위해 추가
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon; // Hashtable 사용을 위해 추가
using UnityEngine.SceneManagement;




public class SendSymptoms : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField symptomsInput; // Symptoms 입력 필드
    [SerializeField]
    private Button sendButton; // Send 버튼
    [SerializeField]
    private GameObject patientNamePrefab; // 환자명 프리팹
    [SerializeField]
    private Transform contentParent; // 스크롤 뷰의 Content 객체

    private string uploadUrl = "http://pbl.dothome.co.kr/SaveSymptoms.php"; // 서버의 PHP 스크립트 URL
    private bool isUploading = false; // 업로드 중 여부를 확인하기 위한 플래그

    private void Start()
    {
        sendButton.onClick.RemoveAllListeners();
        sendButton.onClick.AddListener(OnSendButtonClick);

        // 방에 입장한 후 Room Properties에서 대기 리스트 가져오기
        UpdateWaitingList();
    }

    private void UpdateWaitingList()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)PhotonNetwork.CurrentRoom.CustomProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            // 각 환자명을 스크롤뷰에 추가
            foreach (string patientName in patientNames)
            {
                if (!string.IsNullOrEmpty(patientName))
                {
                    AddPatientNameToScrollView(patientName);
                }
            }
        }
    }

    public void OnSendButtonClick()
    {
        if (isUploading)
        {
            Debug.Log("업로드 중입니다. 잠시만 기다려주세요.");
            return;
        }

        string symptomsText = symptomsInput.text.Trim();
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername"); // 로그인된 사용자 ID

        if (string.IsNullOrEmpty(symptomsText))
        {
            Debug.Log("증상 내용을 입력해주세요.");
            return;
        }

        if (string.IsNullOrEmpty(loggedInUsername))
        {
            Debug.Log("로그인된 사용자 정보를 확인할 수 없습니다.");
            return;
        }

        // 이미 대기 리스트에 존재하는지 확인
        if (IsPatientAlreadyInList(loggedInUsername))
        {
            Debug.Log("해당 환자명은 이미 대기 리스트에 있습니다.");
            return; // 이미 존재하면 업로드하지 않음
        }

        isUploading = true;

        // 코루틴을 통해 서버에 데이터 업로드 시도
        StartCoroutine(UploadSymptoms(symptomsText, loggedInUsername));
    }

    private bool IsPatientAlreadyInList(string patientName)
    {
        foreach (Transform child in contentParent)
        {
            TextMeshProUGUI childText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null && childText.text == MaskPatientName(patientName))
            {
                return true; // 이미 존재
            }
        }
        return false; // 존재하지 않음
    }

    private IEnumerator UploadSymptoms(string symptomsText, string loggedInUsername)
    {
        WWWForm form = new WWWForm();
        form.AddField("reception_status", symptomsText);
        form.AddField("user", loggedInUsername);

        using (UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("증상 데이터가 성공적으로 업로드되었습니다.");
                symptomsInput.text = ""; // 전송 후 입력 필드 초기화

                // 모든 클라이언트에 대기 리스트에 환자 추가를 동기화
                photonView.RPC("AddPatientNameToScrollViewRPC", RpcTarget.All, loggedInUsername);

                // Room Custom Properties에 대기 리스트 업데이트
                AddPatientToWaitingList(loggedInUsername);
            }
            else
            {
                Debug.LogError("데이터 업로드 실패: " + www.error);
            }
        }

        isUploading = false;
    }

    private string MaskPatientName(string patientName)
    {
        if (string.IsNullOrEmpty(patientName) || patientName.Length < 2)
            return patientName; // 이름이 너무 짧은 경우 처리하지 않음

        string masked = patientName[0] + "*" + patientName[patientName.Length - 1];
        return masked;
    }

    private void AddPatientNameToScrollView(string patientName)
    {
        string maskedName = MaskPatientName(patientName); // 이름 마스킹

        GameObject newPatientEntry = Instantiate(patientNamePrefab, contentParent);
        TextMeshProUGUI textComponent = newPatientEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = maskedName; // 마스킹된 이름 설정
        }

        Debug.Log("환자명 추가됨: " + maskedName);
    }

    [PunRPC]
    private void AddPatientNameToScrollViewRPC(string patientName)
    {
        Debug.Log("RPC 호출됨: " + patientName);

        foreach (Transform child in contentParent)
        {
            TextMeshProUGUI childText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null && childText.text == MaskPatientName(patientName))
            {
                Debug.Log("해당 환자명은 이미 대기 리스트에 있습니다.");
                return;
            }
        }

        AddPatientNameToScrollView(patientName); // 기존 함수 재사용
    }

    private void AddPatientToWaitingList(string patientName)
    {
        ExitGames.Client.Photon.Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (currentProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)currentProperties["WaitingList"];
            waitingList += "," + patientName;
            currentProperties["WaitingList"] = waitingList;
        }
        else
        {
            currentProperties["WaitingList"] = patientName;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);
    }

    private void RemovePatientFromWaitingList(string patientName)
    {
        ExitGames.Client.Photon.Hashtable currentProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        if (currentProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)currentProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            List<string> updatedList = new List<string>(patientNames);
            updatedList.Remove(patientName);

            currentProperties["WaitingList"] = string.Join(",", updatedList.ToArray());
            PhotonNetwork.CurrentRoom.SetCustomProperties(currentProperties);
        }
    }

    private void RemovePatientFromScrollView(string patientName)
    {
        foreach (Transform child in contentParent)
        {
            TextMeshProUGUI childText = child.GetComponentInChildren<TextMeshProUGUI>();
            if (childText != null && childText.text == MaskPatientName(patientName))
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("플레이어가 방을 떠났습니다: " + otherPlayer.NickName);

        RemovePatientFromWaitingList(otherPlayer.NickName);
        RemovePatientFromScrollView(otherPlayer.NickName);
    }

    public void ChangeToAgoraScene()
    {
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername");

        Debug.Log("Button Clicked: Changing scene to Hospital_Patient and removing from waiting list");

        RemovePatientFromWaitingList(loggedInUsername);
        RemovePatientFromScrollView(loggedInUsername);

        SceneManager.LoadScene("Hospital_Patient");
    }
}
