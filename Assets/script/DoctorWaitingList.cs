using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon; // Photon의 Hashtable을 사용하기 위해 추가
using System.Collections.Generic;
using Photon.Realtime;

public class DoctorWaitingList : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject patientNamePrefab; // 환자명 프리팹
    [SerializeField]
    private GameObject buttonPrefab; // 버튼 프리팹
    [SerializeField]
    private Transform contentParent; // 스크롤 뷰의 Content 객체
    [SerializeField]
    private float itemSpacing = 100f; // 항목 간의 수직 간격
    [SerializeField]
    private float yOffset = 150f;
    [SerializeField]
    private MedicalCertificate medicalCertificate; // MedicalCertificate 스크립트에 대한 참조 추가;
    [SerializeField]
    private GameObject medicalCertificateWindow;

    private void Start()
    {
        // 의사 씬이 시작되면 대기 리스트를 업데이트
        UpdateWaitingList();
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername");
        if (!string.IsNullOrEmpty(loggedInUsername))
        {
            PhotonNetwork.NickName = loggedInUsername; // NickName을 로그인된 사용자 이름으로 설정
            Debug.Log("PhotonNetwork NickName 설정됨: " + PhotonNetwork.NickName);
        }
    }

    private void UpdateWaitingList()
    {
        // Custom Properties에서 대기 리스트를 가져옴
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)PhotonNetwork.CurrentRoom.CustomProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            // 각 환자명을 스크롤뷰에 추가
            for (int i = 0; i < patientNames.Length; i++)
            {
                string patientName = patientNames[i];
                if (!string.IsNullOrEmpty(patientName))
                {
                    AddPatientNameAndButtonToScrollView(patientName, i);
                }
            }
        }
    }

    // 스크롤 뷰에 환자명과 버튼을 각각 추가
    private void AddPatientNameAndButtonToScrollView(string patientName, int index)
    {
        // 환자명 프리팹 인스턴스화
        GameObject patientEntry = Instantiate(patientNamePrefab, contentParent, false);

        // 수직 위치 설정
        RectTransform patientRect = patientEntry.GetComponent<RectTransform>();
        patientRect.anchoredPosition = new Vector2(-250, yOffset - index * itemSpacing); // 환자명 프리팹 위치 변경 

        // TextMeshProUGUI 컴포넌트 설정 (환자명 설정)
        TextMeshProUGUI textComponent = patientEntry.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = patientName;
        }

        // 버튼 프리팹 인스턴스화
        GameObject buttonEntry = Instantiate(buttonPrefab, contentParent, false);

        // 버튼 수직 위치 설정
        RectTransform buttonRect = buttonEntry.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(50, yOffset - index * itemSpacing); // send 버튼 프리팹 위치 변경 

        // Button 컴포넌트 찾기
        Button sendMessageButton = buttonEntry.GetComponentInChildren<Button>();
        if (sendMessageButton != null)
        {
            // 버튼이 클릭될 때 실행될 함수 연결 (환자명 전달)
            sendMessageButton.onClick.AddListener(() => OnSendMessageButtonClicked(patientName));
        }

        //Debug.Log("의사 대기 리스트에 환자명과 버튼 추가됨: " + patientName);
    }

    public void OnSendMessageButtonClicked(string patientName)
    {
        bool patientFound = false;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == patientName)
            {
                patientFound = true;
                Debug.Log("RPC 전송 시도 중: " + patientName);
                photonView.RPC("ShowPopupForPatient", player, patientName);
                Debug.Log("특정 환자에게만 RPC 전송됨: " + patientName);

                if (medicalCertificate != null)
                {
                    medicalCertificateWindow.SetActive(true); // Window GameObject 활성화
                    CoroutineManager.Instance.StartManagedCoroutine(medicalCertificate.LoadPatientInfo(patientName));
                }
                else
                {
                    Debug.LogWarning("medicalCertificate 스크립트가 할당되지 않았습니다. Inspector에서 할당해주세요.");
                }
                break;
            }
        }

        if (!patientFound)
        {
            Debug.LogWarning("특정 환자를 찾을 수 없습니다: " + patientName);
        }
    }


    // 플레이어가 방을 나갔을 때 호출되는 메서드
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 방을 떠난 플레이어의 이름을 대기 리스트에서 제거
        RemovePatientFromWaitingList(otherPlayer.NickName);
        // 대기 리스트 업데이트
        UpdateWaitingList();
    }

    // 대기 리스트에서 환자명 제거
    private void RemovePatientFromWaitingList(string patientName)
    {
        // 현재 대기 리스트 가져오기
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WaitingList"))
        {
            string waitingList = (string)PhotonNetwork.CurrentRoom.CustomProperties["WaitingList"];
            string[] patientNames = waitingList.Split(',');

            // 대기 리스트에서 해당 환자명 제거
            List<string> updatedList = new List<string>(patientNames);
            updatedList.Remove(patientName);

            // 업데이트된 리스트를 다시 저장
            ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
            newProperties["WaitingList"] = string.Join(",", updatedList.ToArray());
            PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties);

            Debug.Log("대기 리스트에서 환자명 제거됨: " + patientName);
        }
    }

    // Room Custom Properties가 업데이트될 때 호출되는 메서드
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("WaitingList"))
        {
            // 대기 리스트 관련 프리팹만 삭제
            foreach (Transform child in contentParent)
            {
                if (child != null && child.name.Contains("Variant(Clone)")) // 이름을 통해 삭제할 대상 필터링
                {
                    Destroy(child.gameObject);
                }
            }

            // 대기 리스트 새로 업데이트
            UpdateWaitingList();
        }
    }
}