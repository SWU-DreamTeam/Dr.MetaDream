using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class RunNodeScript : MonoBehaviour
{
    public async Task<(string address, string privateKey)> RunJavaScript(string user)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "node";
        startInfo.Arguments = $"Assets/address.js {user}"; // ����� ID�� ���ڷ� ����
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
                    // 'Address:' ���ڿ��� �����ϰ� �ּҸ� ����
                    address = e.Data.Replace("Address:", "").Trim();
                    UnityEngine.Debug.Log("Extracted Address: " + address); // �ּҸ� �α� ���
                }
                else if (e.Data.Contains("Private Key:"))
                {
                    // 'Private Key:' ���ڿ��� �����ϰ� �����̺� Ű�� ����
                    privateKey = e.Data.Replace("Private Key:", "").Trim();
                    privateKey = privateKey.Replace("0x", ""); // �� ���� "0x" ����
                    UnityEngine.Debug.Log("Extracted Private Key: " + privateKey); // �����̺� Ű�� �α� ���
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

            // Node.js ���μ����� ����� ������ ���
            await Task.Run(() => process.WaitForExit());
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Exception occurred while starting Node.js process: " + ex.Message);
        }

        return (address, privateKey);
    }
}
