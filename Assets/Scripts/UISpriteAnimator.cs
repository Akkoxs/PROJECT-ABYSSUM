using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] frames;
    public float framesPerSecond = 12f;
    public bool isLooping = true;

    private Image uiImage;
    private float timer;
    private int currentFrame;

    void Awake()
    {
        uiImage = GetComponent<Image>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float frameInterval = 1f / framesPerSecond;

        if (timer >= frameInterval)
        {
            timer -= frameInterval;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (isLooping)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = frames.Length - 1;
                    return; // Stop animating
                }
            }

            uiImage.sprite = frames[currentFrame];
        }
    }
}