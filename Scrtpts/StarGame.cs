using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    private readonly string roomUrl = "http://127.0.0.1:8081/room";

    private readonly string nextSceneName = "game";

    public void StartGame()
    {
        StartCoroutine(AllocateRoom());
    }

    private IEnumerator AllocateRoom()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(roomUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {

                Debug.Log("房间分配成功：" + www.downloadHandler.text);
        
            
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {

                Debug.LogError("房间分配失败：" + www.error);
            }
        }
    }
}
