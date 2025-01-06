using UnityEngine;
using UnityEngine.UI;

public class PositionPulse : MonoBehaviour
{

    public float startX;
    public float startY;

    public float endX;
    public float endY;

    public float speed = .5f;
    public float delay = 0;

    private float startTime;
    private bool inflate = true;

    private Vector3 startPosition;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        startPosition = transform.GetComponent<RectTransform>().position;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
        float elapsedTime = Time.time - startTime;
        float lerp = elapsedTime / speed;
        if (inflate)
        {
            Vector3 newPosition = new Vector3(Screen.width / 2 + Mathf.Lerp(startX, endX, lerp), startPosition.y + Mathf.Lerp(startY, endY, lerp),1);
            transform.GetComponent<RectTransform>().position = newPosition;
            if (lerp >= 1)
            {
                inflate = false;
                startTime = Time.time;
            }
        }
        else
        {
            Vector3 newPosition = new Vector3(Screen.width / 2 + Mathf.Lerp(endX, startX, Mathf.Pow(lerp, 3)), startPosition.y + Mathf.Lerp(endY, startY, Mathf.Pow(lerp, 3)), 1);
            transform.GetComponent<RectTransform>().position = newPosition;
            if (lerp >= 1)
            {
                inflate = true;
                startTime = Time.time;
            }
        }
    }
}
