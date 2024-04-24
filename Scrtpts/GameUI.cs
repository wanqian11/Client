using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;


public class GameUI : MonoBehaviour
{
    private static GameUI _instance;
    public static GameUI Instance
    {
        get
        {
            return _instance;
        }
    }
    public int score = 0;
    public int length = 0;
    public Text scoreText;
    public Text lengthText;

    private void Awake()
    {
        _instance = this;
    }
    public void UpdateUI(int s = 10, int l =1)
    {
        score += s;
        length += l;
        scoreText.text = "玩家1得分：" + score;
        lengthText.text = "玩家1长度：" + length;
    }
}

