using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // UI Timer variables.
    public TextMeshProUGUI timeDisplay;
    public static Timer instance;
    private TimeSpan timeSpent;
    private bool timerEnabled;
    private float elapsedTime;

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    private void Awake()
    {
        // If instance is unavailable then destroy it.
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Connect instance if available.
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start() is called before the first frame update.
    private void Start()
    {
        // Set UI Timer.
        if (timeDisplay != null)
        {
            timeDisplay.text = "Run Time: 00:00.00";
        }
        timerEnabled = false;
        Timer.instance.BeginTimer();
    }

    // Begin the timer.
    public void BeginTimer()
    {
        timerEnabled = true;
        elapsedTime = 0f;

        StartCoroutine(UpdateTimer());
    }

    // End the timer.
    public void EndTimer()
    {
        timerEnabled = false;
    }

    // Update the timer.
    private IEnumerator UpdateTimer()
    {
        while (timerEnabled)
        {
            if (timeDisplay == null)
            {
                timerEnabled = false;
                yield break;
            }

            elapsedTime += Time.deltaTime;
            timeSpent = TimeSpan.FromSeconds(elapsedTime);
            string timePlayingString = "Run Time: " + timeSpent.ToString("mm':'ss'.'ff");
            timeDisplay.text = timePlayingString;

            yield return null;
        }
    }
}