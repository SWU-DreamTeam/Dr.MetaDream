using UnityEngine;
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� �߰�

public class SceneTransitionManager : MonoBehaviour
{
    // Hospital_rooms ������ ��ȯ�ϴ� �޼���
    public void GoToHospitalRooms()
    {
        SceneManager.LoadScene("Hospital_rooms"); // �� �̸��� ��Ȯ�� �Է�
    }
}