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

 public IEnumerator Translate(Transform startPos, Transform endPos, float moveDuration) //move an object down from point A to B
   {
      float elapsed = 0f; 
      Vector3 start = startPos.position;
      Vector3 end = endPos.position;

      while (elapsed < moveDuration)
      {
         startPos.position = Vector3.Lerp(start, end, elapsed/moveDuration);
         elapsed += Time.deltaTime;
         yield return null;
      }
      startPos.position = end; 
   }
}
