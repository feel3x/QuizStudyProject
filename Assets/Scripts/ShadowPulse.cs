using UnityEngine;
using UnityEngine.UI;

public class ShadowPulse : MonoBehaviour
{

    public float startX;
    public float startY;

    public float endX;
    public float endY;

    public float speed =.5f;
    public float delay = 0;

    private float startTime;
    private bool inflate = true;
    private Shadow shadow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         shadow = GetComponent<Shadow>();
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float elapsedTime = Time.time - startTime;
        float lerp = elapsedTime / speed;
        if (inflate)
        {
            Vector2 newEffectDistance = new Vector2(Mathf.Lerp(startX, endX, lerp), Mathf.Lerp(startY, endY, lerp));
            shadow.effectDistance = newEffectDistance;
            if(lerp >= 1)
            {
                inflate = false;
                startTime = Time.time;
            }
        }
        else
        {
            Vector2 newEffectDistance = new Vector2(Mathf.Lerp(endX, startX, Mathf.Pow(lerp, 3)), Mathf.Lerp(endY, startY, Mathf.Pow(lerp, 3)));
            shadow.effectDistance = newEffectDistance;
            if (lerp >= 1)
            {
                inflate = true;
                startTime = Time.time;
            }
        }
    }
}
