using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UIMatchTimer : MonoBehaviour
{
    [SerializeField] private MatchTimer timer;
    [SerializeField] private TextMeshProUGUI text;

    private Coroutine timerCoroutine;

    private void Start()
    {
        if (NetworkSessionManager.Instance != null)
        {
            if (NetworkSessionManager.Match != null)
                NetworkSessionManager.Match.MatchStart += OnMatchStart;
        }

        text.enabled = false;
    }

    private void OnMatchStart()
    {
        text.enabled = true;

        // Запускаем корутину для обновления таймера
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            int minutes = Mathf.FloorToInt(timer.TimeLeft / 60);
            int seconds = Mathf.FloorToInt(timer.TimeLeft % 60);

            text.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (timer.TimeLeft <= 10 && NetworkSessionManager.Match.MatchActive)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = Color.white;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDestroy()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        if (NetworkSessionManager.Instance != null)
        {
            if (NetworkSessionManager.Match != null)
                NetworkSessionManager.Match.MatchStart -= OnMatchStart;
        }
    }
}
