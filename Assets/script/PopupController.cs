using UnityEngine;
using Photon.Pun;
using TMPro;

public class PopupController : MonoBehaviourPun
{
    [SerializeField]
    private GameObject popupPanel; // PopupPanel 객체를 Unity Inspector에서 할당
    [SerializeField]
    private TextMeshProUGUI popupText;

    private void Awake()
    {
        Debug.Log("PopupController Awake 호출됨");
    }

    private void Start()
    {
        Debug.Log("PopupController Start 호출됨. LoggedInUsername: " + PlayerPrefs.GetString("LoggedInUsername", ""));
    }


    [PunRPC]
    public void ShowPopupForPatient(string targetPatientName)
    {
        // 디버깅 로그 추가
        Debug.Log("ShowPopupForPatient RPC 호출됨");

        // 현재 로그인된 유저의 닉네임이 목표 환자명과 일치하는지 확인
        string loggedInUsername = PlayerPrefs.GetString("LoggedInUsername", "");
        Debug.Log("로그인된 유저: " + loggedInUsername + ", 목표 유저: " + targetPatientName);

        if (loggedInUsername == targetPatientName)
        {
            if (popupPanel != null && !popupPanel.activeSelf)
            {
                popupPanel.SetActive(true); // 패널 활성화

                if (popupText != null)
                {
                    popupText.text = "진료실로 입장하세요.";
                }

                Debug.Log("환자에게 팝업 표시됨: " + targetPatientName);
            }
            else
            {
                Debug.LogError("PopupPanel이 할당되지 않았거나 이미 활성화 상태입니다.");
            }
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null && popupPanel.activeSelf)
        {
            popupPanel.SetActive(false); // 팝업 창 비활성화
            Debug.Log("팝업 창이 닫혔습니다.");
        }
        else
        {
            Debug.LogError("PopupPanel이 할당되지 않았거나 이미 비활성화 상태입니다.");
        }
    }

}
