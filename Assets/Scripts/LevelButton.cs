using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("Bölüm Bilgisi")]
    public int levelNumber = 1; // Hangi bölüm butonu? (1, 2, 3...)

    [Header("Yýldýz UI")]
    public Image[] starImages;         // Butondaki 3 yýldýz Image'ý
    public Sprite fullStarSprite;      // Parlak/Yanan Yýldýz PNG
    public Sprite emptyStarSprite;     // Sönük/Gri Yýldýz PNG

    void Start()
    {
        UpdateLevelStarsUI();
    }

    public void UpdateLevelStarsUI()
    {
        // Aktif oyun türünü al (Puzzle, Balance vs.)
        string currentGameType = PlayerPrefs.GetString("SelectedGameType", "Puzzle");

        // GameplayManager'ýn kaydettiði ayný anahtarla yýldýz sayýsýný çek
        string starKey = $"{currentGameType}_Level_{levelNumber}_Stars";
        int starsEarned = PlayerPrefs.GetInt(starKey, 0);

        // Yýldýzlarý güncelle
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].sprite = (i < starsEarned) ? fullStarSprite : emptyStarSprite;
            }
        }
    }
}