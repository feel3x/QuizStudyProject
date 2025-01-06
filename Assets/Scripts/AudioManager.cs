using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{

    public AudioSource audioSource;

    public AudioClip startSound;
    public AudioClip endSound;
    public AudioClip correctSound;

    public AudioClip incorrectSound;
    public AudioClip highScoreSound;

    public GameManager gameManager;

    public float scrollTime = 3f;


    public Button volumeButton;
    public Sprite muteTexture;
    public Sprite volumeTexture;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        gameManager = GameObject.Find("Scripts").GetComponent<GameManager>();

    }

    private void OnEnable()
    {
        gameManager.gameStarted += playStartSound;
        gameManager.gameEnded += playEndSound ;
        gameManager.correctlyAnswered += playCorrectSound;
        gameManager.incorrectlyAnswered += playIncorrectSound;
        gameManager.highScoreAchieved += ()=>{ audioSource.PlayOneShot(highScoreSound); };
    }
    private void OnDisable()
    {
        gameManager.gameStarted -= playStartSound;
        gameManager.gameEnded -= playEndSound;
        gameManager.correctlyAnswered -= playCorrectSound;
        gameManager.incorrectlyAnswered -= playIncorrectSound;
        gameManager.highScoreAchieved -= () => { audioSource.PlayOneShot(highScoreSound); };
    }



    private void playStartSound()
    {
        audioSource.PlayOneShot(startSound);
        StartCoroutine(scrollSound(79f)); 

    }

    public void switchMute()
    {
        audioSource.enabled = !audioSource.enabled;
        if (audioSource.enabled)
        {
            volumeButton.image.sprite = volumeTexture;
        }
        else
        {
            volumeButton.image.sprite = muteTexture;
        }
    }
    private void playEndSound()
    {
        audioSource.PlayOneShot(endSound);
        StartCoroutine(scrollSound(0.5f));

    }
    private void playCorrectSound(Question q, int questionNumber)
    {
        audioSource.PlayOneShot(correctSound);
    }
    private void playIncorrectSound(Question q, int questionNumber)
    {
        audioSource.PlayOneShot(incorrectSound);
    }

    private IEnumerator scrollSound(float time)
    {
        float startTime = Time.realtimeSinceStartup;
        float timeElapsed = 0;
        float startAudioTime = audioSource.time;
        while (timeElapsed < scrollTime)
        {

            timeElapsed = Time.realtimeSinceStartup - startTime;
            audioSource.volume = Mathf.Lerp(1, 0,timeElapsed / scrollTime);
            yield return null;
        }
        startTime = Time.realtimeSinceStartup;
        audioSource.time = time;
        timeElapsed = 0;
        while (timeElapsed < scrollTime)
        {

            timeElapsed = Time.realtimeSinceStartup - startTime;
            audioSource.volume = Mathf.Lerp(0, 1, timeElapsed / scrollTime);
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
