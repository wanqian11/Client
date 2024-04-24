using System.IO;
using System.Net.Sockets;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnHome : MonoBehaviour
{
    public void LoadHomeScene()
    {
        SceneManager.LoadScene("home");
    }
}
