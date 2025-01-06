using System.Collections;
using UnityEngine;

public class ViewChanger : MonoBehaviour
{
    //The speed at which the view chagnes
    public float speed = 1.0f;

    //The Empty which contains all the view Objects
    public GameObject viewContainer;

    public GameObject currentlyVisibleView;

    //Roll in a GameObject view
    public void rollIn(GameObject go, float time = 2f, float delay = 0, bool axisY=false)
    {
        StartCoroutine(rollerCoroutine(go, time, true, delay, false,axisY));
    }

    //Roll out a GameObject view
    public void rollOut(GameObject go, float time = 2f, float delay = 0, bool axisY=false)
    {
        StartCoroutine(rollerCoroutine(go, time, false, delay, true, axisY));
    }

    private void Start()
    {
        currentlyVisibleView = viewContainer.transform.Find("Menu").gameObject;
    }

    //Change the view from one View to another
    public void changeViews(string views)
    {
        string[] viewArray = views.Split(':');
        GameObject viewGo1= viewContainer.transform.Find(viewArray[0]).gameObject;
        GameObject viewGo2 = viewContainer.transform.Find(viewArray[1]).gameObject;

        disablePositionBounce(viewGo1);
        disablePositionBounce(viewGo2);
        rollOut(viewGo1, speed, 0);
        rollIn(viewGo2, speed, 0f);

        currentlyVisibleView = viewGo2;

        enablePositionBounce(viewGo2);
    }

    public void switchToView(string view)
    {
        if (currentlyVisibleView != null)
        {
 disablePositionBounce(currentlyVisibleView);
            rollOut(currentlyVisibleView, speed, 0);
        }
        GameObject viewGo2 = viewContainer.transform.Find(view).gameObject;

       
        disablePositionBounce(viewGo2);
      
        rollIn(viewGo2, speed, 0f);

        currentlyVisibleView = viewGo2;

        enablePositionBounce(viewGo2);
    }

    //Blend in one single View while keeping the reset open in the background
    public void blendInView(string view)
    {
        GameObject viewGo = viewContainer.transform.Find(view).gameObject;

        disablePositionBounce(viewGo);
        rollIn(viewGo, speed, 0f, true);

        enablePositionBounce(viewGo);
    }

    //Blend out one single View while keeping the reset open in the background
    public void blendOutView(string view)
    {
        GameObject viewGo = viewContainer.transform.Find(view).gameObject;

        disablePositionBounce(viewGo);
        rollOut(viewGo, speed, 0f, true);

        enablePositionBounce(viewGo);
    }

    private void disablePositionBounce(GameObject go)
    {
        PositionPulse[] pulses = go.GetComponentsInChildren<PositionPulse>();
        foreach(PositionPulse p in pulses)
        {
            p.enabled = false;
        }
    }
    private void enablePositionBounce(GameObject go)
    {
        PositionPulse[] pulses = go.GetComponentsInChildren<PositionPulse>();
        foreach (PositionPulse p in pulses)
        {
            p.enabled = true;
        }
    }

    private IEnumerator rollerCoroutine(GameObject go, float time, bool rollingIn, float delay, bool rightSide, bool axisY)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(true);
        float startTime = Time.time;

        Vector3 startRotation = rollingIn ? new Vector3(0, axisY?90:0, axisY?0:rightSide?180:-180) : go.transform.eulerAngles;
        Vector3 endRotation = rollingIn ? Vector3.zero : new Vector3(0, axisY ? 90 : 0, axisY ? 0 :rightSide ? 180 : -180);

        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            elapsedTime = Time.time - startTime;
            go.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, elapsedTime / time);
            yield return null;
        }
        go.SetActive(rollingIn);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
