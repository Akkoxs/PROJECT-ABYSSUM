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
 
}
