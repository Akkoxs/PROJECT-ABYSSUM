using UnityEngine;
using UnityEngine.UI;

public class SonarPing : MonoBehaviour
{

    private Image image;
    private Color color;

    [SerializeField] private float dissapearTimer = 0f;
    [SerializeField] private float dissapearTimerMax = 1f;

    private void Start()
    {
        image = GetComponent<Image>();
        color = new Color (1, 1, 1, 1f);
    }

    private void Update()
    {
        dissapearTimer += Time.deltaTime;

        color.a = Mathf.Lerp(dissapearTimerMax, 0f, dissapearTimer / dissapearTimerMax);
        image.color = color;

        if (dissapearTimer >= dissapearTimerMax)
        {
            Destroy(gameObject);
        }
    }

    public void SetColor(Color color)
    {
        this.color = color; 
    }

    public void SetDissapearTimer(float dissapearTimerMax)
    {
        this.dissapearTimerMax = dissapearTimerMax;
        dissapearTimer = 0f;
    }


}
