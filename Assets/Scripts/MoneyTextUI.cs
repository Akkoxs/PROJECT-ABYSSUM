using TMPro;
using UnityEngine;

public class MoneyTextUI : MonoBehaviour
{
    [SerializeField] private GameObject gmObject;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI debtText;

    private GameManager gameManager;

    private void Awake()
    {
        if (gmObject != null)
            gameManager = gmObject.GetComponent<GameManager>();    
    }

    private void OnEnable()
    {
        gameManager.onMoneyChanged.AddListener(DisplayMoney);
    }

    private void OnDisable()
    {
        gameManager.onMoneyChanged.RemoveListener(DisplayMoney);
    }

    private void DisplayMoney(int money, int debt)
    {
        moneyText.text = "$" + money.ToString();
        debtText.text = "$" + debt.ToString();
    }

}
