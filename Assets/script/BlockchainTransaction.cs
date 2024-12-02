using System;
using System.Collections;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using UnityEngine;
using UnityEngine.Networking;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json.Linq;


public class BlockchainTransaction : MonoBehaviour
{
    private string rpcUrl = "https://test-eth-rpc.blocksdk.com/T3r3O5OJfA4Av31rB2QGDObkq258GyNDxFRu9snL";
    private string privateKey;
    private string toAddress;
    private GameManager gameManager;
    private bool isTransactionInProgress = false; // 트랜잭션 완료 여부를 나타내는 상태 변수

    // 시간 측정을 위한 변수
    private DateTime transactionStartTime; // 트랜잭션 요청 시작 시간
    private DateTime blockCreationTime;   // 블록 생성 확인 시간

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found.");
            return;
        }

        StartCoroutine(InitializeAfterWalletLoad());
    }

    IEnumerator InitializeAfterWalletLoad()
    {
        while (!gameManager.IsWalletInfoLoaded)
        {
            yield return null;
        }

        InitializeTransaction();
    }

    public void SetTransactionAddresses(string decryptedKey, string patientAddress)
    {
        privateKey = decryptedKey;
        toAddress = patientAddress;

        Debug.Log($"Doctor's Private Key set: {privateKey}");
        Debug.Log($"Patient's Address set: {toAddress}");
    }

    public void InitializeTransaction()
    {

        Debug.Log("Initializing Blockchain Transaction...");
    }

    public async void SendTransaction(Prescription.BlockchainPrescriptionData blockchainData)
    {
        if (isTransactionInProgress)
        {
            Debug.LogError("트랜잭션이 이미 진행 중입니다.");
            return;
        }

        isTransactionInProgress = true;

        try
        {
            string jsonData = JsonUtility.ToJson(blockchainData);
            Debug.Log($"Serialized Blockchain Prescription Data: {jsonData}");
            string transactionHash = await SendTransactionAsync(jsonData);

            if (!string.IsNullOrEmpty(transactionHash))
            {
                Debug.Log($"Prescription transaction sent successfully: {transactionHash}");
                StartCoroutine(GetBlockTimestampWithTotalDuration(transactionHash));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"트랜잭션 오류: {e.Message}");
        }
        finally
        {
            isTransactionInProgress = false;
        }
    }


    public async void SendTransaction(MedicalCertificate.MedicalcertificateData certificateData)
    {
        transactionStartTime = DateTime.UtcNow; // 트랜잭션 요청 시작 시간 기록
        Debug.Log($"Transaction request started at (UTC): {transactionStartTime:yyyy-MM-dd HH:mm:ss}");

        DateTime createButtonClickedTime = DateTime.UtcNow.AddHours(9); // KST 시간
        Debug.Log($"CREATE 버튼이 클릭된 시간 (KST): {createButtonClickedTime:yyyy-MM-dd HH:mm:ss}");

        if (!string.IsNullOrEmpty(privateKey) && !string.IsNullOrEmpty(toAddress))
        {
            string jsonData = JsonUtility.ToJson(certificateData);
            string transactionHash = await SendTransactionAsync(jsonData);

            if (!string.IsNullOrEmpty(transactionHash))
            {
                Debug.Log($"Medical certificate transaction sent successfully: {transactionHash}");
                StartCoroutine(GetBlockTimestampWithTotalDuration(transactionHash));
            }
        }
        else
        {
            Debug.LogError("Cannot send transaction. Private key or destination address is missing.");
        }
    }

    private async Task<string> SendTransactionAsync(string data)
    {
        var account = new Account(privateKey);
        var web3 = new Web3(account, rpcUrl);

        try
        {
            var dataHex = data.ToHexUTF8();
            var nonce = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address);
            var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();
            var gasLimit = new HexBigInteger(50000);

            var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput
            {
                From = account.Address,
                To = toAddress,
                Data = dataHex,
                Gas = gasLimit,
                GasPrice = gasPrice,
                Nonce = nonce
            };

            var transactionHash = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(
                await web3.TransactionManager.SignTransactionAsync(transactionInput)
            );

            DateTime transactionTimestamp = DateTime.UtcNow.AddHours(9); // KST 시간 변환
            Debug.Log($"Transaction successful with hash: {transactionHash} at {transactionTimestamp:yyyy-MM-dd HH:mm:ss} (KST)");
            isTransactionInProgress = true;
            return transactionHash;
        }
        catch (Exception e)
        {
            Debug.LogError("Transaction failed: " + e.Message);
            isTransactionInProgress = false;
            return null;
        }
    }

    private IEnumerator GetBlockTimestampWithTotalDuration(string transactionHash, int retryCount = 10, float delayBetweenRetries = 10f)
    {
        string blockSdkUrl = $"https://testnet-api.blocksdk.com/v3/eth/transaction/{transactionHash}";

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(blockSdkUrl))
            {
                webRequest.SetRequestHeader("x-api-key", "VUrU3PDgpZmOB7U2vlM6zQa5UaY1bVPVRDNFwh3i");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log("BlockSDK Response: " + jsonResponse);

                    try
                    {
                        var json = JObject.Parse(jsonResponse);
                        var timestampString = json["payload"]?["timestamp"]?.ToString();

                        if (!string.IsNullOrEmpty(timestampString) && long.TryParse(timestampString, out long timestamp))
                        {
                            blockCreationTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
                            Debug.Log($"Block creation time from BlockSDK: {blockCreationTime:yyyy-MM-dd HH:mm:ss} UTC");

                            // 전체 소요 시간 계산
                            TimeSpan totalDuration = blockCreationTime - transactionStartTime;
                            Debug.Log($"Total time from transaction request to block creation: {totalDuration.TotalSeconds} seconds.");

                            yield break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("JSON Parsing Error: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError($"BlockSDK API Error: {webRequest.error}, Status Code: {webRequest.responseCode}");
                    if (attempt < retryCount)
                    {
                        Debug.Log($"Retrying... Attempt {attempt} of {retryCount}");
                        yield return new WaitForSeconds(delayBetweenRetries);
                    }
                    else
                    {
                        Debug.LogError("All retry attempts failed.");
                    }
                }
            }
        }
    }
}
