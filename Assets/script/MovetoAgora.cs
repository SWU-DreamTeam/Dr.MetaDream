using UnityEngine;
using UnityEngine.SceneManagement;

public class MovetoAgora : MonoBehaviour // Ŭ���� �̸��� ���� �̸��� �����ؾ� ��
{
    // ��ư�� OnClick �̺�Ʈ�� �Ű����� ���� �� �޼��带 �����ؾ� ��
    public void ChangeToAgoraScene()
    {
        Debug.Log("Button Clicked: Changing scene to Hospital_Patient");
        // 'Agora'��� �̸��� ������ ��ȯ
        SceneManager.LoadScene("Hospital_Patient");
    }
}