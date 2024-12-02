using UnityEngine;
using System.Collections; // IEnumerator를 사용하기 위해 필요한 네임스페이스

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
                DontDestroyOnLoad(obj); // 씬 전환 시에도 삭제되지 않도록 설정
            }
            return instance;
        }
    }

    public Coroutine StartManagedCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
}
