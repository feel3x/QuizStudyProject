using UnityEngine;

public class LivesHandler : MonoBehaviour
{

    private int amountOfVisibleHearts;

    public GameObject heartPrefab;

    private GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        amountOfVisibleHearts = transform.childCount;
        gameManager = GameObject.Find("Scripts").GetComponent<GameManager>();
    }


    private void createHearts()
    {
        foreach(Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        for (int i = 0; i < gameManager.currentLives; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, transform);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(amountOfVisibleHearts != gameManager.currentLives)
        {
            createHearts();
        }
    }
}
