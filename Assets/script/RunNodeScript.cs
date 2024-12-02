using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class RunNodeScript : MonoBehaviour
{
    public async Task<(string address, string privateKey)> RunJavaScript(string user)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "node";
        startInfo.Arguments = $"Assets/address.js {user}"; // 사용자 ID를 인자로 전달
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        Process process = new Process();
        process.StartInfo = startInfo;

        string address = string.Empty;
        string privateKey = string.Empty;

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains("Address:"))
                {
                    // 'Address:' 문자열을 제거하고 주소만 추출
                    address = e.Data.Replace("Address:", "").Trim();
                    UnityEngine.Debug.Log("Extracted Address: " + address); // 주소만 로그 출력
                }
                else if (e.Data.Contains("Private Key:"))
                {
                    // 'Private Key:' 문자열을 제거하고 프라이빗 키만 추출
                    privateKey = e.Data.Replace("Private Key:", "").Trim();
                    privateKey = privateKey.Replace("0x", ""); // 맨 앞의 "0x" 제거
                    UnityEngine.Debug.Log("Extracted Private Key: " + privateKey); // 프라이빗 키만 로그 출력
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.LogError(e.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Node.js 프로세스가 종료될 때까지 대기
            await Task.Run(() => process.WaitForExit());
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Exception occurred while starting Node.js process: " + ex.Message);
        }

        return (address, privateKey);
    }
}
