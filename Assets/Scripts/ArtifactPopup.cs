using UnityEngine;
using TMPro;

public class ArtifactPopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro artifactNameText;
    [SerializeField] private TextMeshPro moneyText;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float fadeDuration = 1.5f;

    private float timer = 0f;

    public void Setup(string artifactName, int sellValue)
    {
        artifactNameText.text = artifactName;
        artifactNameText.color = Color.purple;

        moneyText.text = $"+${sellValue}";
        moneyText.color = Color.green;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / fadeDuration;

        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out both texts
        float alpha = Mathf.Lerp(1f, 0f, t);
        artifactNameText.alpha = alpha;
        moneyText.alpha = alpha;

        if (timer >= fadeDuration)
            Destroy(gameObject);
    }
}