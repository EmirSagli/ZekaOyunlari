using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapManager : MonoBehaviour
{
    [Header("UI Baþlýk")]
    public TextMeshProUGUI categoryTitleText;

    [Header("Bölüm Butonlarý")]
    public Button[] levelButtons; // Haritadaki 10 adet buton

    [Header("Bölümler Yakýnda Paneli")]
    public GameObject comingSoonPanel;
    public TextMeshProUGUI comingSoonMessageText;
    public Button comingSoonCloseButton;

    private string selectedGameType;

    private void Start()
    {
        // Ana menüden seçilen oyun türünü al (Varsayýlan: Balance)
        selectedGameType = PlayerPrefs.GetString("SelectedGameType", "Balance");

        if (categoryTitleText != null)
        {
            categoryTitleText.text = $"{selectedGameType.ToUpper()} HARÝTASI";
        }

        UpdateMapButtons();
        CheckComingSoonNotification();
    }

    void CheckComingSoonNotification()
    {
        if (PlayerPrefs.GetInt("ShowComingSoonPopup", 0) == 1)
        {
            PlayerPrefs.SetInt("ShowComingSoonPopup", 0);
            PlayerPrefs.Save();

            if (comingSoonPanel != null)
            {
                comingSoonPanel.SetActive(true);

                if (comingSoonMessageText != null)
                {
                    comingSoonMessageText.text = "Tebrikler! Bu oyunun mevcut 10 seviyesini tamamladýnýz.\n\nYeni bölümler çok yakýnda eklenecektir!";
                }

                if (comingSoonCloseButton != null)
                {
                    comingSoonCloseButton.onClick.RemoveAllListeners();
                    comingSoonCloseButton.onClick.AddListener(() => comingSoonPanel.SetActive(false));
                }
            }
        }
    }

    public void UpdateMapButtons()
    {
        // Açýk olan en yüksek seviye (Varsayýlan: 1)
        string unlockKey = $"{selectedGameType}_UnlockedLevel";
        int unlockedLevel = PlayerPrefs.GetInt(unlockKey, 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1;
            Button btn = levelButtons[i];

            if (btn == null) continue;

            // Kilit kontrolü
            bool isUnlocked = levelNum <= unlockedLevel;
            btn.interactable = isUnlocked;

            // Buton týklama olayýný kodla dinamik baðlama
            btn.onClick.RemoveAllListeners();
            if (isUnlocked)
            {
                btn.onClick.AddListener(() => OpenLevel(levelNum));
            }

            // Yýldýzlarý gösterme (Butonun altýnda Image'lar varsa)
            string starKey = $"{selectedGameType}_Level_{levelNum}_Stars";
            int earnedStars = PlayerPrefs.GetInt(starKey, 0);

            // Butonun içindeki yýldýz ikonlarýný aktif etme (opsiyonel)
            Transform starContainer = btn.transform.Find("StarsContainer");
            if (starContainer != null)
            {
                for (int s = 0; s < starContainer.childCount; s++)
                {
                    starContainer.GetChild(s).gameObject.SetActive(s < earnedStars);
                }
            }
        }
    }

    // Butona týklandýðýnda Gameplay sahnesine geçiþ
    public void OpenLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("CurrentLevelNumber", levelNumber);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Gameplay");
    }

    // Ana Menüye Geri Dön Butonu
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}