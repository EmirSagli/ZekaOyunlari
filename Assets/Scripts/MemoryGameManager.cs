using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryGameManager : MonoBehaviour
{
    public static MemoryGameManager Instance { get; private set; }

    [Header("Veritabaný")]
    public MemoryLevelData[] allLevels;
    public GameObject cardPrefab;
    public RectTransform cardGridArea;
    public GridLayoutGroup gridLayoutGroup;

    [Header("Görseller & Renkler")]
    public Sprite cardBackSprite;

    // 12 Çift için Canlý Renk Paleti
    public Color[] pairColors = new Color[]
    {
        new Color(0.92f, 0.25f, 0.20f), // Kýrmýzý (1)
        new Color(0.18f, 0.53f, 0.95f), // Mavi (2)
        new Color(0.20f, 0.78f, 0.35f), // Yeþil (3)
        new Color(0.98f, 0.75f, 0.10f), // Sarý (4)
        new Color(0.65f, 0.28f, 0.85f), // Mor (5)
        new Color(1.00f, 0.45f, 0.10f), // Turuncu (6)
        new Color(0.10f, 0.80f, 0.80f), // Turkuaz (7)
        new Color(0.95f, 0.30f, 0.65f), // Pembe (8)
        new Color(0.55f, 0.35f, 0.20f), // Kahverengi (9)
        new Color(0.40f, 0.90f, 0.15f), // Açýk Limon Yeþili (10)
        new Color(0.30f, 0.20f, 0.65f), // Çivit Mavisi (11)
        new Color(0.20f, 0.20f, 0.25f)  // Koyu Gri/Antrasit (12)
    };

    private MemoryLevelData currentData;
    private List<MemoryCard> spawnedCards = new List<MemoryCard>();
    private MemoryCard firstSelected = null;
    private MemoryCard secondSelected = null;
    private bool isInputBlocked = false;
    private int matchedPairsCount = 0;
    private int totalPairsInLevel = 0;
    private bool isWon = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        isWon = false;
        isInputBlocked = false;
        firstSelected = null;
        secondSelected = null;
        matchedPairsCount = 0;

        int levelNum = PlayerPrefs.GetInt("CurrentLevelNumber", 1);
        int dataIndex = Mathf.Clamp(levelNum - 1, 0, (allLevels != null && allLevels.Length > 0) ? allLevels.Length - 1 : 0);
        currentData = (allLevels != null && allLevels.Length > 0) ? allLevels[dataIndex] : null;

        if (GameplayManager.Instance != null && currentData != null)
        {
            GameplayManager.Instance.optimalMoves3Stars = currentData.targetMoves3Stars;
            GameplayManager.Instance.targetMoves2Stars = currentData.targetMoves2Stars;
            GameplayManager.Instance.StartGameTracking();
        }

        SetupGrid();
    }

    void SetupGrid()
    {
        foreach (Transform child in cardGridArea)
        {
            Destroy(child.gameObject);
        }
        spawnedCards.Clear();

        int rows = (currentData != null) ? currentData.rows : 2;
        int cols = (currentData != null) ? currentData.columns : 2;
        int totalCards = rows * cols;
        totalPairsInLevel = totalCards / 2;

        float areaWidth = cardGridArea.rect.width;
        float areaHeight = cardGridArea.rect.height;
        float spacing = 12f;

        // Dikdörtgen en-boy oraný (Geniþlik : 1, Yükseklik : 1.35)
        float aspectRatio = 1.35f;

        float cellWidth = (areaWidth - (spacing * (cols + 1))) / cols;
        float cellHeight = (areaHeight - (spacing * (rows + 1))) / rows;

        // Dikdörtgen oranýna uygun en büyük boyutu seç
        float finalWidth = cellWidth;
        float finalHeight = finalWidth * aspectRatio;

        if (finalHeight > cellHeight)
        {
            finalHeight = cellHeight;
            finalWidth = finalHeight / aspectRatio;
        }

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = cols;
            gridLayoutGroup.cellSize = new Vector2(finalWidth, finalHeight);
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
        }

        List<int> cardIds = new List<int>();
        for (int i = 0; i < totalPairsInLevel; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        // Karýþtýr
        for (int i = 0; i < cardIds.Count; i++)
        {
            int rnd = Random.Range(i, cardIds.Count);
            int temp = cardIds[i];
            cardIds[i] = cardIds[rnd];
            cardIds[rnd] = temp;
        }

        // Kartlarý Oluþtur
        for (int i = 0; i < totalCards; i++)
        {
            int id = cardIds[i];
            GameObject cardObj = Instantiate(cardPrefab, cardGridArea);
            MemoryCard card = cardObj.GetComponent<MemoryCard>();

            Color assignedColor = (id < pairColors.Length) ? pairColors[id] : Color.white;
            card.Setup(id, assignedColor, cardBackSprite);
            spawnedCards.Add(card);
        }
    }

    public void OnCardClicked(MemoryCard clickedCard)
    {
        if (isInputBlocked || isWon) return;

        if (firstSelected == null)
        {
            firstSelected = clickedCard;
            firstSelected.FlipToFront();
        }
        else if (secondSelected == null && clickedCard != firstSelected)
        {
            secondSelected = clickedCard;
            secondSelected.FlipToFront();

            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.RegisterMove();
            }

            StartCoroutine(CheckMatchRoutine());
        }
    }

    private IEnumerator CheckMatchRoutine()
    {
        isInputBlocked = true;
        yield return new WaitForSeconds(0.6f);

        if (firstSelected.cardId == secondSelected.cardId)
        {
            firstSelected.SetMatched();
            secondSelected.SetMatched();
            matchedPairsCount++;

            if (matchedPairsCount >= totalPairsInLevel)
            {
                isWon = true;
                if (GameplayManager.Instance != null)
                {
                    GameplayManager.Instance.OnLevelCompleted();
                }
            }
        }
        else
        {
            firstSelected.FlipToBack();
            secondSelected.FlipToBack();
        }

        firstSelected = null;
        secondSelected = null;
        isInputBlocked = false;
    }
}