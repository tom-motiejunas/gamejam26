using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public TextMeshProUGUI coinText;
    public GameObject winPanel;

    private int totalCoins = 0;
    private int collectedCoins = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void RegisterCoin()
    {
        totalCoins++;
        UpdateUI();
    }

    public void CoinCollected()
    {
        collectedCoins++;
        UpdateUI();

        if (collectedCoins >= totalCoins)
        {
            WinGame();
        }
    }

    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins Left: " + (totalCoins - collectedCoins);
        }
    }

    void WinGame()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        Debug.Log("You Win!");
    }
}
