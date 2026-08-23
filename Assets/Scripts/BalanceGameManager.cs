using UnityEngine;
using TMPro;

public class BalanceGameManager : MonoBehaviour
{
    public static BalanceGameManager Instance; // [cite: 807]

    [Header("Seviye Veritabaný")]
    public BalanceLevelData[] allLevels; // [cite: 808]
    public GameObject weightPrefab; // [cite: 809]

    [Header("Kefeler ve Alt Panel")]
    public Transform leftScaleSlot; // [cite: 810]
    public Transform rightScaleSlot; // [cite: 810]
    public RectTransform weightsInventoryPanel; // [cite: 810]

    [Header("UI Metinleri")]
    public TextMeshProUGUI leftWeightText; // [cite: 811]
    public TextMeshProUGUI rightWeightText; // [cite: 811]

    private BalanceLevelData currentData; // [cite: 811]
    private bool levelWon = false; // [cite: 811]

    private void Awake()
    {
        if (Instance == null) Instance = this; // [cite: 812]
    }

    private void OnEnable()
    {
        levelWon = false;
        LoadCurrentLevel();
    }

    void LoadCurrentLevel()
    {
        // 1. ÖNCEKÝ TÜM AÐIRLIKLARI TEMÝZLE (Çift oluþumu engelleyen kýsým)
        ClearAllWeights();

        int levelNum = PlayerPrefs.GetInt("CurrentLevelNumber", 1);
        int dataIndex = Mathf.Clamp(levelNum - 1, 0, (allLevels != null && allLevels.Length > 0) ? allLevels.Length - 1 : 0);

        if (allLevels == null || allLevels.Length == 0) return;
        currentData = allLevels[dataIndex];

        if (GameplayManager.Instance != null && currentData != null)
        {
            GameplayManager.Instance.optimalMoves3Stars = currentData.optimalMoves3Stars;
            GameplayManager.Instance.targetMoves2Stars = currentData.targetMoves2Stars;
        }

        // Aðýrlýklarý alt panele üret
        if (currentData != null && currentData.inventoryWeights != null)
        {
            foreach (var w in currentData.inventoryWeights)
            {
                CreateWeightObjectRandom(w, weightsInventoryPanel);
            }
        }

        CheckBalance();
    }
    void ClearAllWeights()
    {
        // Alt paneldeki tüm parçalarý sil
        if (weightsInventoryPanel != null)
        {
            foreach (Transform child in weightsInventoryPanel)
            {
                Destroy(child.gameObject);
            }
        }

        // Sol kefedeki parçalarý sil
        if (leftScaleSlot != null)
        {
            foreach (Transform child in leftScaleSlot)
            {
                Destroy(child.gameObject);
            }
        }

        // Sað kefedeki parçalarý sil
        if (rightScaleSlot != null)
        {
            foreach (Transform child in rightScaleSlot)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void CreateWeightObjectRandom(WeightData data, RectTransform panelRect)
    {
        if (weightPrefab == null || panelRect == null) return;

        // UI elemanlarýnýn ekran boyutuna tam oturmasýný garantiye al
        Canvas.ForceUpdateCanvases();

        GameObject obj = Instantiate(weightPrefab, panelRect);
        WeightItem item = obj.GetComponent<WeightItem>();
        if (item != null)
        {
            item.SetupWeight(data.weightValue, data.itemSprite);
        }

        RectTransform itemRect = obj.GetComponent<RectTransform>();

        // Aðýrlýðýn kendi geniþlik ve yüksekliðini al (yoksa varsayýlan 100px)
        float itemWidth = (itemRect != null && itemRect.rect.width > 0) ? itemRect.rect.width : 100f;
        float itemHeight = (itemRect != null && itemRect.rect.height > 0) ? itemRect.rect.height : 100f;

        // Panelin gerçek sýnýrlarýný (Local Rect) hesapla ve kenarlardan parça boyu kadar pay býrak
        float marginX = (itemWidth / 2f) + 30f;  // Kenar güvenlik payý
        float marginY = (itemHeight / 2f) + 20f;

        float minX = panelRect.rect.xMin + marginX;
        float maxX = panelRect.rect.xMax - marginX;
        float minY = panelRect.rect.yMin + marginY;
        float maxY = panelRect.rect.yMax - marginY;

        // Eðer panel çok dar ise merkeze sabitle
        if (minX > maxX) { minX = 0; maxX = 0; }
        if (minY > maxY) { minY = 0; maxY = 0; }

        Vector3 randomLocalPos = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );

        obj.transform.localPosition = randomLocalPos;
    }

    public void CheckBalance()
    {
        if (levelWon) return;

        int totalLeft = CalculateTotalWeight(leftScaleSlot);
        int totalRight = CalculateTotalWeight(rightScaleSlot);

        if (leftWeightText != null) leftWeightText.text = totalLeft > 0 ? totalLeft + " kg" : "0 kg";
        if (rightWeightText != null) rightWeightText.text = totalRight > 0 ? totalRight + " kg" : "0 kg";

        // KAZANMA KONTROLÜ:
        // Her iki kefede de en az birer aðýrlýk olacak ve aðýrlýklarý eþit olacak!
        if (totalLeft > 0 && totalLeft == totalRight)
        {
            levelWon = true;
            Invoke(nameof(TriggerWin), 0.5f);
        }
    }

    int CalculateTotalWeight(Transform slot)
    {
        int sum = 0; // [cite: 826]
         foreach (Transform child in slot) // [cite: 826]
        {
            WeightItem item = child.GetComponent<WeightItem>(); // [cite: 827]
             if (item != null) // [cite: 827]
            {
                sum += item.weightValue; // [cite: 828]
            }
        }
        return sum; // [cite: 829]
    }

    void TriggerWin()
    {
         if (GameplayManager.Instance != null) // [cite: 830]
        {
            GameplayManager.Instance.OnLevelCompleted(); // [cite: 830]
        }
    }
}