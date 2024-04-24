using Snakegame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Sprite[] headSprites;
    private static GameController _instance;
    public static GameController Instance
    {
        get { return _instance; }
    }
    public GameObject snakePrefab;
    private Transform snakeHolder;
    private Dictionary<string, GameObject> snakes = new Dictionary<string, GameObject>();

    void Start()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        snakeHolder = GameObject.Find("SnakeHolder").transform;
        MakeHead();
    }

    public void MakeHead()
    {
        int index = UnityEngine.Random.Range(0, headSprites.Length);
        GameObject head = Instantiate(snakePrefab);
        head.GetComponent<Image>().sprite = headSprites[index];
        head.transform.SetParent(snakeHolder, false);
    }

    public void UpdateGameState(GameStateUpdate gameStateUpdate)
    {
        Debug.Log("Processing game state update");
        foreach (var snakeProto in gameStateUpdate.Snakes)
        {
            Debug.Log($"蛇 {snakeProto.Id} 移动到了 ({snakeProto.Head.X}, {snakeProto.Head.Y}) 朝向 {snakeProto.Direction}");
            if (!snakes.ContainsKey(snakeProto.Id))
            {
                GameObject snakeObj = Instantiate(snakePrefab, snakeHolder);
                SnakeHead snakeHeadComponent = snakeObj.GetComponent<SnakeHead>();
                snakeHeadComponent.playerID = snakeProto.Id;
                snakes.Add(snakeProto.Id, snakeObj);
                Debug.Log($"Snake {snakeProto.Id} added to game");
            }

            SnakeHead existingSnakeHead = snakes[snakeProto.Id].GetComponent<SnakeHead>();
            existingSnakeHead.UpdateSnake(snakeProto);
        }
    }
}