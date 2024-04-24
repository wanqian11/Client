using Snakegame;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;



public class SnakeHead : MonoBehaviour
{

    public string playerID;
    public List<Transform> bodyList = new List<Transform>();
    public float velocity = 0.35f;
    public int step;
    private int x;
    private int y;
    private Vector3 headPos;
    private Transform canvas;
    private bool isDie = false;



    public GameObject bodyPrefab;
    public Sprite[] bodySprites = new Sprite[2];
    private object sanke;
    public void UpdatePosition(Vector3 newPosition)
    {
        // 更新蛇头的位置到新的坐标
        transform.position = newPosition;
    }
    void Awake()
    {
        canvas = GameObject.Find("Canvas").transform;

    }
    void Move()
    {
        headPos = gameObject.transform.localPosition;
        gameObject.transform.localPosition = new Vector3(headPos.x + x, headPos.y + y, headPos.z);
        if (bodyList.Count > 0)
        {
            //bodyList.Last().localPosition = headPos;
            // bodyList.Insert(0, bodyList.Last());
            // bodyList.RemoveAt(bodyList.Count - 1);

            for (int i = bodyList.Count - 2; i >= 0; i--)
            {
                bodyList[i + 1].localPosition = bodyList[i].localPosition;

            }
            bodyList[0].localPosition = headPos;
        }
    }
    void Grow()
    {
        int index = (bodyList.Count % 2 == 0) ? 0 : 1;
        GameObject body = Instantiate(bodyPrefab, new Vector3(2000, 2000, 0), Quaternion.identity);
        body.GetComponent<Image>().sprite = bodySprites[index];
        body.transform.SetParent(canvas, false);
        bodyList.Add(body.transform);
    }
    void Die()
    {
        CancelInvoke();
        isDie = true;
        
        StartCoroutine(GameOver(0.1f));
    }
    
    IEnumerator GameOver(float t)
    {
        yield return new WaitForSeconds(t);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Over");
    }
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Destroy(collision.gameObject);
            GameUI.Instance.UpdateUI();
            Grow();
            FoodMaker.Instance.MakeFood();

        }
        else if (collision.gameObject.CompareTag("Body"))
        {
            Die();
        }
        else
        {
            Die();
        }
    }

    public void UpdateSnake(Snake snakeProto)
    {
        // 更新蛇头位置
        transform.localPosition = new Vector3(snakeProto.Head.X, snakeProto.Head.Y, 0);

        // 更新蛇头方向
        switch (snakeProto.Direction)
        {
            case "up":
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case "down":
                transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case "left":
                transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
            case "right":
                transform.localRotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }
}


