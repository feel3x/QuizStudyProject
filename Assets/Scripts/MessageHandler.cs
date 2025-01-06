using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageHandler : MonoBehaviour
{


    public GameObject messageView;

    public TMP_Text messageObject;
    public Button okButton;

    public ViewChanger viewChanger;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void showMessage(string title, string message, UnityAction okButtonCallback)
    {
        TMP_Text titleObject = messageView.transform.Find("Header").Find("Title").GetComponent<TMP_Text>();
       
       titleObject.text = title;
        messageObject.text = message;
        okButton.onClick.RemoveAllListeners();
        if(okButtonCallback != null )
        {
            okButton.onClick.AddListener(okButtonCallback);
        }
        okButton.onClick.AddListener(()=>viewChanger.blendOutView("Message"));

        viewChanger.blendInView("Message");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
