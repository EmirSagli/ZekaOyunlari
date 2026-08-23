using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuCarousel : MonoBehaviour
{
    // C# Enum ile 5 mini oyun türümüz
    public enum GameType { Hanoi, Memory, Puzzle, Circuit, Balance }

    [System.Serializable]
    public struct GameData
    {
        public string gameName;           // Örn: "HANOÝ KULELERÝ"
        public GameType gameType;         // Tür seçimi
        public Sprite previewSprite;      // Oyunun kapak resmi
        public int totalStarsInGame;      // Bu mini oyundaki toplam kazanýlabilir yýldýz (Örn: 30)
    }

    [Header("Mini Oyun Tanýmlamalarý")]
    public GameData[] games;

    [Header("UI Baðlantýlarý")]
    public TextMeshProUGUI gameTitleText;
    public TextMeshProUGUI categoryTitleText;
    public TextMeshProUGUI starInfoText;
    public Image previewImage;

    private int currentIndex = 0;

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        // Oyundan Ana Menüye her dönüldüðünde yýldýzlarý güncel tutar
        UpdateUI();
    }

    // Sað Oka Basýnca
    public void NextGame()
    {
        if (games == null || games.Length == 0) return;
        currentIndex = (currentIndex + 1) % games.Length;
        UpdateUI();
    }

    // Sol Oka Basýnca
    public void PreviousGame()
    {
        if (games == null || games.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = games.Length - 1;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (games == null || games.Length == 0) return;

        GameData current = games[currentIndex];

        if (gameTitleText != null) gameTitleText.text = "BOARD QUEST";
        if (categoryTitleText != null) categoryTitleText.text = current.gameName;
        if (previewImage != null) previewImage.sprite = current.previewSprite;

        // --- 1'DEN 10'A KADAR TÜM BÖLÜMLERÝN YILDIZLARINI TOPLA ---
        int earnedStars = 0;
        int totalLevels = 10; // Her oyunda 10 bölüm var

        for (int i = 1; i <= totalLevels; i++)
        {
            string starKey = $"{current.gameType}_Level_{i}_Stars";
            earnedStars += PlayerPrefs.GetInt(starKey, 0);
        }

        // Kart üzerindeki metni güncelle (Örn: "Toplanan Yýldýz: 18 / 30")
        if (starInfoText != null)
        {
            int maxStars = (current.totalStarsInGame > 0) ? current.totalStarsInGame : (totalLevels * 3);
            starInfoText.text = $"Toplanan Yýldýz: {earnedStars} / {maxStars}";
        }
    }

    // OYNA Butonuna Basýldýðýnda Haritaya Geç
    public void PlaySelectedGame()
    {
        if (games == null || games.Length == 0) return;

        // Seçilen oyunu kaydet
        PlayerPrefs.SetString("SelectedGameType", games[currentIndex].gameType.ToString());
        PlayerPrefs.Save();

        // Harita Sahnesine Git
        SceneManager.LoadScene("LevelMap");
    }
}