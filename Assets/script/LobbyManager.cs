using Photon.Pun;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public GameObject[] characterPrefabs; // ���� ĳ���� �������� Inspector�� ����

    void Start()
    {
        // �÷��̾ �濡 �����ϸ� ĳ���͸� �����մϴ�.
        CreateCharacter();
    }

    void CreateCharacter()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            // ĳ���Ͱ� ������ ��ġ ����
            Vector3 spawnPosition = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));

            // �������� ĳ���� ������ ����
            int randomIndex = Random.Range(0, characterPrefabs.Length);
            GameObject selectedPrefab = characterPrefabs[randomIndex];

            // PhotonNetwork�� ����Ͽ� �������� ���õ� ĳ���� ������ �ν��Ͻ�ȭ
            PhotonNetwork.Instantiate(selectedPrefab.name, spawnPosition, Quaternion.identity);
        }
    }
}
