using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;


public class UserLoginRegister : MonoBehaviour
{
    public InputField usernameInputField;
    public InputField passwordInputField;
    public Text feedbackText;

    private readonly string loginUrl = "http://127.0.0.1:8081/login";
    private readonly string registerUrl = "http://127.0.0.1:8081/register";

    private readonly string nextSceneName = "home";


    public void AttemptLogin()
    {
        StartCoroutine(Login());
    }

    public void AttemptRegister()
    {
        StartCoroutine(Register());
    }

    private IEnumerator Login()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usernameInputField.text);
        form.AddField("password", passwordInputField.text);

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                feedbackText.text = "µÇÂ¼³É¹¦£¡";

                SceneManager.LoadScene(nextSceneName);

                
            }
            else
            {
                feedbackText.text = "µÇÂ¼Ê§°Ü£º" + www.error;
            }
        }
    }

    private IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", usernameInputField.text);
        form.AddField("password", passwordInputField.text);

        using (UnityWebRequest www = UnityWebRequest.Post(registerUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                feedbackText.text = "×¢²á³É¹¦£¡";
            }
            else
            {
                feedbackText.text = "×¢²áÊ§°Ü£º" + www.error;
            }
        }
    }
}
