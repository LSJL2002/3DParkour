using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class BoostTimerUI : MonoBehaviour
{
    public Image fillImage;

    private Coroutine timerCoroutine;

    public void StartTimer(float duration, Action onComplete)
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(TimerCoroutine(duration, onComplete));
    }

    private IEnumerator TimerCoroutine(float duration, Action onComplete)
    {
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            fillImage.fillAmount = timeLeft / duration;
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}
