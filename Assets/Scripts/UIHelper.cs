using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class UIHelper : MonoBehaviour
{

 public IEnumerator BarFlash(float flashTime, Color flashColor, Color regColor, Image barFill)
 {
    barFill.color = flashColor;  
    yield return new WaitForSeconds(flashTime);
    barFill.color = regColor;
 }    

public IEnumerator Translate(RectTransform panel, RectTransform target, float moveDuration)
{
    float elapsed = 0f;
    Vector2 start = panel.anchoredPosition;
    Vector2 end = target.anchoredPosition;

    while (elapsed < moveDuration)
    {
        panel.anchoredPosition = Vector2.Lerp(start, end, elapsed / moveDuration);
        elapsed += Time.deltaTime;
        yield return null;
    }

    panel.anchoredPosition = end;
}

public Vector3 GetVectorFromAngle(float angle)
   {
      float angleRad = angle * (Mathf.PI/180f);
      return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
   }

}
