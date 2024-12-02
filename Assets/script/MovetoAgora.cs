using UnityEngine;
using UnityEngine.SceneManagement;

public class MovetoAgora : MonoBehaviour // 클래스 이름이 파일 이름과 동일해야 함
{
    // 버튼의 OnClick 이벤트에 매개변수 없는 이 메서드를 연결해야 함
    public void ChangeToAgoraScene()
    {
        Debug.Log("Button Clicked: Changing scene to Hospital_Patient");
        // 'Agora'라는 이름의 씬으로 전환
        SceneManager.LoadScene("Hospital_Patient");
    }
}