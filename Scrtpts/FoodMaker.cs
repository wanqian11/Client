
using UnityEngine;
using UnityEngine.UI;



public class FoodMaker : MonoBehaviour
{
    private static FoodMaker _instance;
    public static FoodMaker Instance
    {
        get
        {
            return _instance;
        }
    }
    public int xlimit = 33;
    public int ylimit = 14;
    public GameObject foodPrefab;
    public Sprite[] foodSprites;
    private Transform foodHolder;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        foodHolder = GameObject.Find("FoodHolder").transform;
        MakeFood();
    }
    public void MakeFood()
    {
        int index = Random.Range(0, foodSprites.Length);
        GameObject food = Instantiate(foodPrefab);
        food.GetComponent<Image>().sprite = foodSprites[index];
        food.transform.SetParent(foodHolder, false);
        int x = Random.Range(-xlimit, xlimit);
        int y = Random.Range(-ylimit, ylimit);
        food.transform.localPosition = new Vector3(x * 30, y * 30, 0);
    }
}