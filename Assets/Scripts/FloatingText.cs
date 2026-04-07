using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingText : MonoBehaviour
{
    [Header("Settings")]
    public float floatSpeed = 1.5f;
    public float fadeDuration = 2.0f;
    public float floatDistance = 2.0f;

    private TextMeshPro tmp;

    public void Init(string message)
    {
        tmp = GetComponent<TextMeshPro>();
        tmp.text = message;
        StartCoroutine(FloatAndFade());
    }

    IEnumerator FloatAndFade()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * floatDistance;
        Color startColor = tmp.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            transform.position = Vector3.Lerp(startPos, endPos, t);
            tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
