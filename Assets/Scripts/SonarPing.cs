using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SonarPing : MonoBehaviour
{

    private Image image;
    private Color color;

    [SerializeField] private float dissapearTimer = 0f;
    [SerializeField] private float dissapearTimerMax = 1f;
    [SerializeField] private GameObject pfPingLabel;
    private TextMeshProUGUI label;

    private void Awake()
    {
        image = GetComponent<Image>();
        color = new Color (1, 1, 1, 1f);

        if (pfPingLabel != null)
        {
            GameObject labelObj = Instantiate(pfPingLabel, transform);
            label = labelObj.GetComponent<TextMeshProUGUI>();
            label.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        dissapearTimer += Time.deltaTime;

        color.a = Mathf.Lerp(dissapearTimerMax, 0f, dissapearTimer / dissapearTimerMax);
        image.color = color;

        if (label != null)
        {
            Color lc = label.color;
            lc.a = color.a;
            label.color = lc;
        }

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

    public void SetText(string insertedText)
    {
        if (label != null)
        {
            label.gameObject.SetActive(!string.IsNullOrEmpty(insertedText));
            label.text = insertedText;
        }
    }


}
