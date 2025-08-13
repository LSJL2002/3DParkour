using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Transform laserStartPoint;
    public Vector3 laserDirection = Vector3.forward;
    public float laserLength;
    public LayerMask playerLayer;

    public CanvasGroup warningUI;
    public float fadeDuration = 1f;
    public float visibleDuration = 3f;

    private bool playerDetectedLastFrame = false;
    private Coroutine fadeCoroutine;
    private Coroutine visibleCoroutine;

    void Start()
    {
        warningUI.alpha = 0f;
    }

    void Update()
    {
        Vector3 origin = laserStartPoint.position;
        Vector3 dir = laserStartPoint.TransformDirection(laserDirection);

        RaycastHit hit;
        bool hitplayer = Physics.Raycast(origin, dir, out hit, laserLength, playerLayer);

        if (hitplayer && !playerDetectedLastFrame)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if (visibleCoroutine != null) StopCoroutine(visibleCoroutine);

            fadeCoroutine = StartCoroutine(FadeUI(1f));
            visibleCoroutine = StartCoroutine(VisibleTimer());
        }

        playerDetectedLastFrame = hitplayer;
    }

    IEnumerator VisibleTimer()
    {
        yield return new WaitForSeconds(visibleDuration);
        fadeCoroutine = StartCoroutine(FadeUI(0f));
    }

    IEnumerator FadeUI(float targetAlpha)
    {
        float startAlpha = warningUI.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            warningUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        warningUI.alpha = targetAlpha;
    }
}
