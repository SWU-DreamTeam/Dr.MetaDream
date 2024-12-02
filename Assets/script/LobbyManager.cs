using Photon.Pun;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject[] characterPrefabs; // 여러 캐릭터 프리팹을 Inspector에 연결

    void Start()
    {
        // 플레이어가 방에 입장하면 캐릭터를 생성합니다.
        CreateCharacter();
    }

    void CreateCharacter()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // 캐릭터가 생성될 위치 지정
            Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));

            // 랜덤으로 캐릭터 프리팹 선택
            int randomIndex = Random.Range(0, characterPrefabs.Length);
            GameObject selectedPrefab = characterPrefabs[randomIndex];

            // PhotonNetwork를 사용하여 랜덤으로 선택된 캐릭터 프리팹 인스턴스화
            PhotonNetwork.Instantiate(selectedPrefab.name, spawnPosition, Quaternion.identity);
        }
    }
}
