using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가

public class SceneTransitionManager : MonoBehaviour
{
    // Hospital_rooms 씬으로 전환하는 메서드
    public void GoToHospitalRooms()
    {
        SceneManager.LoadScene("Hospital_rooms"); // 씬 이름을 정확히 입력
    }
}