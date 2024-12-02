using UnityEngine;
using Photon.Pun;

public class CustomPrefabPool : IPunPrefabPool
{
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        // Resources 폴더에서 프리팹 로드
        GameObject prefab = Resources.Load<GameObject>(prefabId);
        if (prefab == null)
        {
            Debug.LogError($"Prefab '{prefabId}' could not be loaded. Make sure it's in a 'Resources' folder.");
            return null;
        }
        return GameObject.Instantiate(prefab, position, rotation);
    }

    public void Destroy(GameObject gameObject)
    {
        GameObject.Destroy(gameObject);
    }
}

