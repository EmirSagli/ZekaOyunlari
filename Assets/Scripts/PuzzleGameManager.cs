using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleGameManager : MonoBehaviour
{
    public static PuzzleGameManager Instance { get; private set; }

    [Header("Seviye Veritabanı")]
    public PuzzleLevelData[] allLevels;
    public GameObject puzzleTilePrefab;

    [Header("UI & Grid Alanı")]
    public RectTransform puzzleGridArea;

    [Header("Tasarım Ayarları")]
    public float spacing = 12f; // Parçalar arası boşluk

    private int gridSize = 3;
    private int totalSlots;
    private int emptyIndex;
    private float tileSize;

    private List<PuzzleTile> tiles = new List<PuzzleTile>();
    private Vector3[] slotPositions;
    private PuzzleLevelData currentData;
    private bool isWon = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        isWon = false;
        int levelNum = PlayerPrefs.GetInt("CurrentLevelNumber", 1);
        StartLevel(levelNum);
    }

    public void StartLevel(int levelNumber)
    {
        isWon = false;

        foreach (Transform child in puzzleGridArea)
        {
            Destroy(child.gameObject);
        }
        tiles.Clear();

        int dataIndex = Mathf.Clamp(levelNumber - 1, 0, (allLevels != null && allLevels.Length > 0) ? allLevels.Length - 1 : 0);
        currentData = (allLevels != null && allLevels.Length > 0) ? allLevels[dataIndex] : null;

        gridSize = (currentData != null) ? currentData.gridSize : 3;
        totalSlots = gridSize * gridSize;
        emptyIndex = totalSlots - 1;

        if (GameplayManager.Instance != null && currentData != null)
        {
            GameplayManager.Instance.optimalMoves3Stars = currentData.targetMoves3Stars;
            GameplayManager.Instance.targetMoves2Stars = currentData.targetMoves2Stars;
        }

        CalculateDynamicPositions();
        CreateTiles();
        ShuffleSolvable();
    }

    void CalculateDynamicPositions()
    {
        slotPositions = new Vector3[totalSlots];

        float boardSize = puzzleGridArea.rect.width;
        float totalSpacing = spacing * (gridSize - 1);
        tileSize = (boardSize - totalSpacing) / gridSize;

        float startX = -boardSize / 2f + tileSize / 2f;
        float startY = boardSize / 2f - tileSize / 2f;

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                int index = r * gridSize + c;
                float x = startX + c * (tileSize + spacing);
                float y = startY - r * (tileSize + spacing);
                slotPositions[index] = new Vector3(x, y, 0f);
            }
        }
    }

    void CreateTiles()
    {
        int totalTiles = totalSlots - 1;

        for (int i = 0; i < totalTiles; i++)
        {
            GameObject obj = Instantiate(puzzleTilePrefab, puzzleGridArea);

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(tileSize, tileSize);
            }

            PuzzleTile tile = obj.GetComponent<PuzzleTile>();
            Sprite sprite = (currentData != null && currentData.puzzleSprites != null && i < currentData.puzzleSprites.Length)
                ? currentData.puzzleSprites[i]
                : null;

            tile.SetupTile(i, i, sprite);
            tile.transform.localPosition = slotPositions[i];
            tiles.Add(tile);
        }
    }

    public bool CanMove(int tileIndex)
    {
        int tileRow = tileIndex / gridSize;
        int tileCol = tileIndex % gridSize;

        int emptyRow = emptyIndex / gridSize;
        int emptyCol = emptyIndex % gridSize;

        bool isHorizontal = (tileRow == emptyRow) && Mathf.Abs(tileCol - emptyCol) == 1;
        bool isVertical = (tileCol == emptyCol) && Mathf.Abs(tileRow - emptyRow) == 1;

        return isHorizontal || isVertical;
    }

    public void MoveTileToEmpty(PuzzleTile tile)
    {
        if (isWon) return;

        int oldTileIndex = tile.currentGridIndex;

        tile.currentGridIndex = emptyIndex;
        tile.transform.localPosition = slotPositions[emptyIndex];
        tile.UpdateVisualState();

        emptyIndex = oldTileIndex;

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.RegisterMove();
        }

        CheckSolved();
    }

    public void ResetTilePosition(PuzzleTile tile)
    {
        tile.transform.localPosition = slotPositions[tile.currentGridIndex];
    }

    public Vector3 GetEmptySlotWorldPosition()
    {
        return puzzleGridArea.TransformPoint(slotPositions[emptyIndex]);
    }

    void ShuffleSolvable(int steps = 15)
    {
        // 2x2 için minimum 6-8 hamle, diğerleri için belirlenen adımı alalım
        int effectiveSteps = Mathf.Max(steps, gridSize == 2 ? 8 : 15);
        int lastMovedIndex = -1;

        for (int m = 0; m < effectiveSteps; m++)
        {
            List<PuzzleTile> movableTiles = new List<PuzzleTile>();
            foreach (var tile in tiles)
            {
                // Bir önceki hamlede hareket eden parçayı hemen geri almayı engelle
                if (CanMove(tile.currentGridIndex) && tile.currentGridIndex != lastMovedIndex)
                {
                    movableTiles.Add(tile);
                }
            }

            // Eğer tek seçenek eski parça kaldıysa zorunlu olarak onu al
            if (movableTiles.Count == 0)
            {
                foreach (var tile in tiles)
                {
                    if (CanMove(tile.currentGridIndex)) movableTiles.Add(tile);
                }
            }

            if (movableTiles.Count > 0)
            {
                PuzzleTile randomTile = movableTiles[Random.Range(0, movableTiles.Count)];
                int oldIdx = randomTile.currentGridIndex;

                randomTile.currentGridIndex = emptyIndex;
                randomTile.transform.localPosition = slotPositions[emptyIndex];

                lastMovedIndex = emptyIndex;
                emptyIndex = oldIdx;
            }
        }

        // GÜVENCE: Eğer karıştırma bittiğinde tahta hâlâ çözülmüşse, bozulana kadar hamle yap
        int safetyLoop = 0;
        while (IsBoardSolved() && safetyLoop < 20)
        {
            safetyLoop++;
            List<PuzzleTile> movableTiles = new List<PuzzleTile>();
            foreach (var tile in tiles)
            {
                if (CanMove(tile.currentGridIndex)) movableTiles.Add(tile);
            }

            if (movableTiles.Count > 0)
            {
                PuzzleTile randomTile = movableTiles[Random.Range(0, movableTiles.Count)];
                int oldIdx = randomTile.currentGridIndex;

                randomTile.currentGridIndex = emptyIndex;
                randomTile.transform.localPosition = slotPositions[emptyIndex];

                emptyIndex = oldIdx;
            }
        }

        // Görsel renk durumlarını güncelle
        foreach (var tile in tiles)
        {
            tile.UpdateVisualState();
        }
    }

    // Tahtanın tamamen çözülmüş olup olmadığını kontrol eden yardımcı fonksiyon
    bool IsBoardSolved()
    {
        foreach (var tile in tiles)
        {
            if (tile.currentGridIndex != tile.correctIndex)
                return false;
        }
        return true;
    }

    void CheckSolved()
    {
        foreach (var tile in tiles)
        {
            if (tile.currentGridIndex != tile.correctIndex)
                return;
        }

        isWon = true;
        Invoke(nameof(TriggerWin), 0.4f);
    }

    void TriggerWin()
    {
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnLevelCompleted();
        }
    }
}