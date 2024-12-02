using UnityEngine;
using System.Collections; // IEnumerator�� ����ϱ� ���� �ʿ��� ���ӽ����̽�

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager instance;

    public static CoroutineManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CoroutineManager");
                instance = obj.AddComponent<CoroutineManager>();
                DontDestroyOnLoad(obj); // �� ��ȯ �ÿ��� �������� �ʵ��� ����
            }
            return instance;
        }
    }

    public Coroutine StartManagedCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
}
