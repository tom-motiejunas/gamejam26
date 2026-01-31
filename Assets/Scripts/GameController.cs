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

    [Header("Faction UI")]
    public Image symbolDisplay; // The main UI box on the left
    [Tooltip("Order: Infinity, Knot, Bee, Pentagram")]
    public Sprite[] factionGhostSprites; 
    [Tooltip("Order: Infinity, Knot, Bee, Pentagram")]
    public Sprite[] factionDisguiseDisplaySprites; 
    public Faction currentDisguise;

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
        SetDisguise(Faction.Infinity); // Default
        if (winPanel != null) winPanel.SetActive(false);
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SetDisguise(Faction.Infinity);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SetDisguise(Faction.Knot);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SetDisguise(Faction.Bee);
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SetDisguise(Faction.Pentagram);
    }

    public void SetDisguise(Faction faction)
    {
        currentDisguise = faction;
        UpdateSymbolDisplay();
    }

    void UpdateSymbolDisplay()
    {
        if (symbolDisplay != null && factionDisguiseDisplaySprites != null && factionDisguiseDisplaySprites.Length > (int)currentDisguise)
        {
            symbolDisplay.sprite = factionDisguiseDisplaySprites[(int)currentDisguise];
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
            coinText.text = (totalCoins - collectedCoins).ToString("000");
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

    public void CheckDrawing(Texture2D drawnTexture)
    {
        // 0: Infinity, 1: Knot, 2: Bee, 3: Pentagram
        // Faction enum: Infinity=0, Knot=1, Bee=2, Pentagram=3
        // So the index matches the enum directly.
        
        int matchIndex = SymbolRecognizer.GetBestMatch(drawnTexture, factionDisguiseDisplaySprites);

        if (matchIndex != -1)
        {
            SetDisguise((Faction)matchIndex);
            Debug.Log($"Disguise set to: {(Faction)matchIndex}");
        }
        else
        {
            Debug.Log("Drawing not recognized.");
            // Ideally we callback to DrawingManager to flash red
            // Since we don't have a direct reference here yet, let's find it or use a singleton approach or event
            // But better: Input the DrawingManager reference in inspector or FindObjects
            DrawingManager dm = FindObjectOfType<DrawingManager>();
            if (dm != null)
            {
                dm.FlashError();
            }
        }
    }
}
