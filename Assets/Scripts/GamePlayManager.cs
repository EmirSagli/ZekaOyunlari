using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance { get; private set; }

    [Header("Mini Oyun Alanlarý / Panelleri")]
    public GameObject balanceGameArea;
    public GameObject puzzleGameArea;
    public GameObject hanoiGameArea;
    public GameObject memoryGameArea;
    public GameObject circuitGameArea;

    [Header("Üst Bar UI")]
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI scoreTrackerText;

    [Header("Tutorial / Öðretici Paneli")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialTitleText;
    public TextMeshProUGUI tutorialDescText;
    public Button tutorialCloseButton;
    public Button tutorialDontShowAgainButton;

    [Header("Canlý Yýldýz Göstergesi (Header)")]
    public Image[] liveStars; // Header'daki 3 adet küçük yýldýz (0, 1, 2)
    public Sprite headerActiveStarSprite;   // Header - Yanan Yýldýz PNG
    public Sprite headerInactiveStarSprite; // Header - Sönmüþ Yýldýz PNG

    [Header("Kazanma Paneli (WinPanel)")]
    public GameObject winPanel;
    public Image[] winPanelStars; // WinPanel içindeki 3 adet büyük yýldýz
    public Sprite winActiveStarSprite;   // WinPanel - Özel Büyük Parlak Yýldýz PNG
    public Sprite winInactiveStarSprite; // WinPanel - Özel Büyük Sönmüþ Yýldýz PNG

    [Header("Yýldýz Hedef Deðerleri")]
    [HideInInspector] public int optimalMoves3Stars = 10;
    [HideInInspector] public int targetMoves2Stars = 16;
    [HideInInspector] public float targetTime3Stars = 30f;
    [HideInInspector] public float targetTime2Stars = 60f;

    [Header("Oyun Durumu")]
    public bool isTimeBased = false;
    private string currentGameType;
    private int currentLevelNumber;
    private int currentMoves = 0;
    private float elapsedTime = 0f;
    private bool isGameActive = false;
    private int currentLiveStars = 3;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentGameType = PlayerPrefs.GetString("SelectedGameType", "Puzzle");
        currentLevelNumber = PlayerPrefs.GetInt("CurrentLevelNumber", 1);

        isTimeBased = (currentGameType == "Origami");

        if (levelTitleText != null)
        {
            levelTitleText.text = $"{currentGameType.ToUpper()} - SEVÝYE {currentLevelNumber}";
        }

        ActivateSelectedGameArea();
        SetupTutorialPanel();
        StartGameTracking();
    }

    #region Tutorial Panel Yönetimi

    void SetupTutorialPanel()
    {
        string prefKey = "DontShowTutorial_" + currentGameType;
        bool dontShow = PlayerPrefs.GetInt(prefKey, 0) == 1;

        if (currentLevelNumber == 1 && !dontShow)
        {
            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
                SetTutorialContent();

                if (tutorialCloseButton != null)
                {
                    tutorialCloseButton.onClick.RemoveAllListeners();
                    tutorialCloseButton.onClick.AddListener(CloseTutorialPanel);
                }

                if (tutorialDontShowAgainButton != null)
                {
                    tutorialDontShowAgainButton.onClick.RemoveAllListeners();
                    tutorialDontShowAgainButton.onClick.AddListener(OnDontShowAgainClicked);
                }
            }
        }
        else
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
        }
    }

    void SetTutorialContent()
    {
        switch (currentGameType)
        {
            case "Balance":
                if (tutorialTitleText != null) tutorialTitleText.text = "DENGE OYUNU";
                if (tutorialDescText != null) tutorialDescText.text = "Aðýrlýklarý terazinin kefelerine yerleþtirerek iki tarafý mükemmel dengede tut!";
                break;
            case "Puzzle":
                if (tutorialTitleText != null) tutorialTitleText.text = "YAPBOZ";
                if (tutorialDescText != null) tutorialDescText.text = "Parçalarý doðru yerlerine sürükleyip býrakarak görseli eksiksiz tamamla!";
                break;
            case "Hanoi":
                if (tutorialTitleText != null) tutorialTitleText.text = "HANOÝ KULELERÝ";
                if (tutorialDescText != null) tutorialDescText.text = "Diskleri en saðdaki çubuða taþý. Unutma, büyük disk asla küçük diskin üstüne gelemez!";
                break;
            case "Circuit":
                if (tutorialTitleText != null) tutorialTitleText.text = "DEVRE / AKIÞ";
                if (tutorialDescText != null) tutorialDescText.text = "Ayný renkteki noktalarý birbirine baðla. Yollarý kesiþtirme ve tüm kareleri boru hatlarýyla doldur!";
                break;
            case "Memory":
                if (tutorialTitleText != null) tutorialTitleText.text = "KART EÞLEÞTÝRME";
                if (tutorialDescText != null) tutorialDescText.text = "Kartlara týklayarak ön yüzlerini aç ve ayný renge sahip çiftleri hafýzanda tutarak eþleþtir!";
                break;
        }
    }

    public void CloseTutorialPanel()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    public void OnDontShowAgainClicked()
    {
        string prefKey = "DontShowTutorial_" + currentGameType;
        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    #endregion

    void ActivateSelectedGameArea()
    {
        if (balanceGameArea != null) balanceGameArea.SetActive(false);
        if (puzzleGameArea != null) puzzleGameArea.SetActive(false);
        if (hanoiGameArea != null) hanoiGameArea.SetActive(false);
        if (memoryGameArea != null) memoryGameArea.SetActive(false);
        if (circuitGameArea != null) circuitGameArea.SetActive(false);

        switch (currentGameType)
        {
            case "Balance":
                if (balanceGameArea != null) balanceGameArea.SetActive(true);
                break;
            case "Puzzle":
                if (puzzleGameArea != null) puzzleGameArea.SetActive(true);
                break;
            case "Hanoi":
                if (hanoiGameArea != null) hanoiGameArea.SetActive(true);
                break;
            case "Memory":
                if (memoryGameArea != null) memoryGameArea.SetActive(true);
                break;
            case "Circuit":
                if (circuitGameArea != null) circuitGameArea.SetActive(true);
                break;
        }
    }

    public void StartGameTracking()
    {
        currentMoves = 0;
        elapsedTime = 0f;
        isGameActive = true;
        currentLiveStars = 3;

        UpdateTrackerUI();
        ResetLiveStarsUI();
    }

    private void Update()
    {
        if (!isGameActive) return;

        if (isTimeBased)
        {
            elapsedTime += Time.deltaTime;
            UpdateTrackerUI();
            CheckLiveStars();
        }
    }

    public void RegisterMove()
    {
        if (!isGameActive) return;

        currentMoves++;
        UpdateTrackerUI();

        if (!isTimeBased)
        {
            CheckLiveStars();
        }
    }

    void UpdateTrackerUI()
    {
        if (scoreTrackerText == null) return;

        if (isTimeBased)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            scoreTrackerText.text = string.Format("Süre: {0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            scoreTrackerText.text = $"Hamle: {currentMoves}";
        }
    }

    // --- HEADER (CANLI YILDIZ) SÝSTEMÝ ---

    void ResetLiveStarsUI()
    {
        if (liveStars == null) return;

        for (int i = 0; i < liveStars.Length; i++)
        {
            if (liveStars[i] != null)
            {
                if (headerActiveStarSprite != null)
                    liveStars[i].sprite = headerActiveStarSprite;

                liveStars[i].transform.localScale = Vector3.one;
            }
        }
    }

    void CheckLiveStars()
    {
        int starsNow = CalculateCurrentStars();

        if (starsNow < currentLiveStars)
        {
            for (int i = currentLiveStars - 1; i >= starsNow; i--)
            {
                if (i >= 0 && i < liveStars.Length && liveStars[i] != null)
                {
                    StartCoroutine(DimHeaderStarAnimation(liveStars[i]));
                }
            }
            currentLiveStars = starsNow;
        }
    }

    public int CalculateCurrentStars()
    {
        if (isTimeBased)
        {
            if (elapsedTime <= targetTime3Stars) return 3;
            if (elapsedTime <= targetTime2Stars) return 2;
            return 1;
        }
        else
        {
            if (currentMoves <= optimalMoves3Stars) return 3;
            if (currentMoves <= targetMoves2Stars) return 2;
            return 1;
        }
    }

    IEnumerator DimHeaderStarAnimation(Image starImage)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 initialScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.3f;

        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2);
            starImage.transform.localScale = Vector3.Lerp(initialScale, punchScale, t);
            yield return null;
        }

        if (headerInactiveStarSprite != null)
        {
            starImage.sprite = headerInactiveStarSprite;
        }

        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2);
            starImage.transform.localScale = Vector3.Lerp(punchScale, initialScale, t);
            yield return null;
        }

        starImage.transform.localScale = initialScale;
    }

    // --- KAZANMA VE WIN PANEL ÖZEL YILDIZ ANÝMASYONU ---

    public void OnLevelCompleted()
    {
        isGameActive = false;

        int starsEarned = CalculateCurrentStars();

        string starKey = $"{currentGameType}_Level_{currentLevelNumber}_Stars";
        int previousStars = PlayerPrefs.GetInt(starKey, 0);
        if (starsEarned > previousStars) PlayerPrefs.SetInt(starKey, starsEarned);

        string unlockKey = $"{currentGameType}_UnlockedLevel";
        int currentUnlocked = PlayerPrefs.GetInt(unlockKey, 1);
        if (currentLevelNumber >= currentUnlocked) PlayerPrefs.SetInt(unlockKey, currentLevelNumber + 1);

        PlayerPrefs.Save();

        if (winPanel != null)
        {
            winPanel.transform.SetAsLastSibling();
            winPanel.SetActive(true);

            StartCoroutine(AnimateWinPanelStars(starsEarned));
        }
    }

    IEnumerator AnimateWinPanelStars(int starsEarned)
    {
        if (winPanelStars == null || winPanelStars.Length == 0) yield break;

        for (int i = 0; i < winPanelStars.Length; i++)
        {
            if (winPanelStars[i] != null)
            {
                if (winInactiveStarSprite != null)
                    winPanelStars[i].sprite = winInactiveStarSprite;

                winPanelStars[i].transform.localScale = Vector3.one;
            }
        }

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < starsEarned; i++)
        {
            if (i < winPanelStars.Length && winPanelStars[i] != null)
            {
                yield return StartCoroutine(PopWinStar(winPanelStars[i]));
                yield return new WaitForSeconds(0.12f);
            }
        }
    }

    IEnumerator PopWinStar(Image starImage)
    {
        float duration = 0.28f;
        float elapsed = 0f;

        if (winActiveStarSprite != null)
            starImage.sprite = winActiveStarSprite;

        Vector3 startScale = Vector3.zero;
        Vector3 peakScale = Vector3.one * 1.35f;
        Vector3 finalScale = Vector3.one;

        while (elapsed < duration * 0.6f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.6f);
            starImage.transform.localScale = Vector3.Lerp(startScale, peakScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration * 0.4f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration * 0.4f);
            starImage.transform.localScale = Vector3.Lerp(peakScale, finalScale, t);
            yield return null;
        }

        starImage.transform.localScale = finalScale;
    }

    // --- BUTON YÖNLENDÝRMELERÝ ---

    public void LoadNextLevel()
    {
        if (currentLevelNumber >= 10)
        {
            PlayerPrefs.SetInt("ShowComingSoonPopup", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("LevelMap");
        }
        else
        {
            PlayerPrefs.SetInt("CurrentLevelNumber", currentLevelNumber + 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Gameplay");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void BackToMap()
    {
        SceneManager.LoadScene("LevelMap");
    }
}