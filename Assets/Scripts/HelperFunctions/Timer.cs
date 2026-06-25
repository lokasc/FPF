using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public UnityEvent OnTimerFinished;
    
    public float currentTime = 0f;
    public bool isRunning = false;
    
    void Update()
    {
        if (isRunning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                isRunning = false;
                OnTimerFinished.Invoke();
            }
        }
    }

    public void StartTimer(float desiredTime)
    {
        currentTime = desiredTime;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }
    
    
    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime / 3600);
        int minutes = Mathf.FloorToInt((currentTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        int milliseconds = Mathf.FloorToInt((currentTime * 1000f) % 1000f);

        // Format as 2-digit HH : MM : SS
        return $"{minutes:00} : {seconds:00} : {milliseconds:000}";
    }
}
