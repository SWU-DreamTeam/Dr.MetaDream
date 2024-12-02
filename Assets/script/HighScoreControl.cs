using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class HighScore
{
    public string name;
    public int score;
}

public class HighScoreControl : MonoBehaviour
{
    private string secretKey = "mySecretKey";
    public string addScoreURL = "http://localhost/serverscripts/addscore.php?";
    public string highscoreURL = "http://localhost/serverscripts/display.php";
    public Text nameTextInput;
    public Text scoreTextInput;
    public Text nameResultText;
    public Text scoreResultText;

    public void GetScoreBtn()
    {
        nameResultText.text = "Player: \n\n";
        scoreResultText.text = "Score: \n\n";
        StartCoroutine(GetScores());
    }

    public void SendScoreBtn()
    {
        StartCoroutine(PostScores(nameTextInput.text, Convert.ToInt32(scoreTextInput.text)));
        nameTextInput.gameObject.transform.parent.GetComponent<InputField>().text = "";
        scoreTextInput.gameObject.transform.parent.GetComponent<InputField>().text = "";
    }

    IEnumerator GetScores()
    {
        UnityWebRequest hs_get = UnityWebRequest.Get(highscoreURL);
        yield return hs_get.SendWebRequest();

        if (hs_get.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("There was an error getting the high score: " + hs_get.error);
        }
        else
        {
            string jsonResult = hs_get.downloadHandler.text;
            HighScore[] highScores = JsonHelper.FromJson<HighScore>(jsonResult);

            foreach (HighScore score in highScores)
            {
                nameResultText.text += score.name + "\n";
                scoreResultText.text += score.score + "\n";
            }
        }
    }

    IEnumerator PostScores(string name, int score)
    {
        string hash = HashInput(name + score + secretKey);
        string post_url = addScoreURL + "name=" + UnityWebRequest.EscapeURL(name) + "&score=" + score + "&hash=" + hash;

        UnityWebRequest hs_post = UnityWebRequest.Get(post_url);
        yield return hs_post.SendWebRequest();

        if (hs_post.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("There was an error posting the high score: " + hs_post.error);
        }
        else
        {
            Debug.Log("High score posted successfully");
        }
    }

    public string HashInput(string input)
    {
        using (SHA256Managed hm = new SHA256Managed())
        {
            byte[] hashValue = hm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
