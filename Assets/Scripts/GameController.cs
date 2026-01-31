using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Added for UI Sprites

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public TextMeshProUGUI coinText;
    public Image coinIcon; // Reference to a UI Image (Sprite)
    public GameObject winPanel;

    [Header("Symbol UI")]
    public Image symbolDisplay; // The main UI box on the left
    public Sprite[] symbolSprites; // Drag your sliced ui-symbols here
    private int _currentSymbolIndex = 0;

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
        UpdateSymbolDisplay();
        if (winPanel != null) winPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleSymbol();
        }
    }

    void CycleSymbol()
    {
        if (symbolSprites == null || symbolSprites.Length == 0) return;

        _currentSymbolIndex = (_currentSymbolIndex + 1) % symbolSprites.Length;
        UpdateSymbolDisplay();
    }

    void UpdateSymbolDisplay()
    {
        if (symbolDisplay != null && symbolSprites != null && symbolSprites.Length > 0)
        {
            symbolDisplay.sprite = symbolSprites[_currentSymbolIndex];
        }
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
