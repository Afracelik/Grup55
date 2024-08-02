using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText = default;
    private float timer = 0f;
    private bool isRunning = true;

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            timerText.text = FormatTime(timer);
        }
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}
