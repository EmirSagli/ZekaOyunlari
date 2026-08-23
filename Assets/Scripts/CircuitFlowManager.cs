using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircuitFlowManager : MonoBehaviour
{
    public static CircuitFlowManager Instance { get; private set; }

    [Header("Veritabaný")]
    public CircuitFlowLevelData[] allLevels;
    public GameObject cellPrefab;
    public RectTransform gridContainer;
    public GridLayoutGroup gridLayoutGroup;

    [Header("Renk Paleti")]
    public Color[] flowColors = new Color[]
    {
        new Color(0.95f, 0.25f, 0.20f), // Kýrmýzý (0)
        new Color(0.20f, 0.55f, 0.95f), // Mavi (1)
        new Color(0.25f, 0.85f, 0.35f), // Yeþil (2)
        new Color(0.98f, 0.80f, 0.10f), // Sarý (3)
        new Color(0.70f, 0.30f, 0.90f), // Mor (4)
        new Color(1.00f, 0.50f, 0.10f), // Turuncu (5)
        new Color(0.15f, 0.85f, 0.85f)  // Turkuaz (6)
    };

    private CircuitFlowLevelData currentData;
    private FlowCell[,] grid;
    private int gridSize;
    private bool isDragging = false;
    private int activeColorId = -1;
    private List<FlowCell> currentPath = new List<FlowCell>();
    private Dictionary<int, List<FlowCell>> completedPaths = new Dictionary<int, List<FlowCell>>();
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

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    public void StartLevel()
    {
        isWon = false;
        isDragging = false;
        currentPath.Clear();
        completedPaths.Clear();

        int levelNum = PlayerPrefs.GetInt("CurrentLevelNumber", 1);
        int dataIndex = Mathf.Clamp(levelNum - 1, 0, (allLevels != null && allLevels.Length > 0) ? allLevels.Length - 1 : 0);
        currentData = (allLevels != null && allLevels.Length > 0) ? allLevels[dataIndex] : null;

        if (GameplayManager.Instance != null && currentData != null)
        {
            GameplayManager.Instance.optimalMoves3Stars = currentData.targetMoves3Stars;
            GameplayManager.Instance.targetMoves2Stars = currentData.targetMoves2Stars;
            GameplayManager.Instance.StartGameTracking();
        }

        BuildGrid();
    }

    void BuildGrid()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        gridSize = (currentData != null) ? currentData.gridSize : 5;
        grid = new FlowCell[gridSize, gridSize];

        float areaSize = Mathf.Min(gridContainer.rect.width, gridContainer.rect.height);
        float spacing = 8f;
        float cellSize = (areaSize - (spacing * (gridSize + 1))) / gridSize;

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = gridSize;
            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.spacing = new Vector2(spacing, spacing);
        }

        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                GameObject obj = Instantiate(cellPrefab, gridContainer);
                FlowCell cell = obj.GetComponent<FlowCell>();
                cell.Setup(r, c);
                grid[r, c] = cell;
            }
        }

        if (currentData != null && currentData.pairs != null)
        {
            foreach (var pair in currentData.pairs)
            {
                Color col = flowColors[pair.colorId % flowColors.Length];
                grid[pair.startPos.x, pair.startPos.y].SetDot(pair.colorId, col);
                grid[pair.endPos.x, pair.endPos.y].SetDot(pair.colorId, col);
            }
        }
    }

    public void OnCellPointerDown(FlowCell cell)
    {
        if (isWon) return;

        // Týklanan yer bir nokta veya önceden çizilmiþ bir hat ise
        if (cell.isDot)
        {
            StartNewPath(cell.dotColorId, cell);
        }
        else if (cell.currentOccupiedColor != -1)
        {
            StartNewPath(cell.currentOccupiedColor, cell);
        }
    }

    void StartNewPath(int colorId, FlowCell startCell)
    {
        // Varsa o rengin önceki yolunu temizle
        ClearPathForColor(colorId);

        isDragging = true;
        activeColorId = colorId;
        currentPath.Add(startCell);
    }

    public void OnCellPointerEnter(FlowCell cell)
    {
        if (!isDragging || isWon || currentPath.Count == 0) return;

        FlowCell lastCell = currentPath[currentPath.Count - 1];

        // Sadece komþu hücreye (sað, sol, üst, alt) geçiþ yapýlabilir
        if (Mathf.Abs(lastCell.row - cell.row) + Mathf.Abs(lastCell.col - cell.col) != 1)
            return;

        // Geri adým atma kontrolü
        if (currentPath.Count > 1 && currentPath[currentPath.Count - 2] == cell)
        {
            lastCell.ClearPath();
            currentPath.RemoveAt(currentPath.Count - 1);
            return;
        }

        // Kural: Baþka bir renge ait noktaya giremez
        if (cell.isDot && cell.dotColorId != activeColorId)
            return;

        // Kural: Çakýþma kontrolü (Baþka bir rengin borusunun üstünden geçilemez)
        if (cell.currentOccupiedColor != -1 && cell.currentOccupiedColor != activeColorId)
        {
            ClearPathForColor(cell.currentOccupiedColor);
        }

        // Kendi ayný rengindeki hedef noktaya ulaþtý mý?
        if (cell.isDot && cell.dotColorId == activeColorId && cell != currentPath[0])
        {
            currentPath.Add(cell);
            CompletePath();
            return;
        }

        // Boþ hücreyi boyayarak ilerle
        if (!cell.isDot)
        {
            cell.SetPath(activeColorId, flowColors[activeColorId % flowColors.Length]);
            currentPath.Add(cell);
        }
    }

    void CompletePath()
    {
        completedPaths[activeColorId] = new List<FlowCell>(currentPath);
        isDragging = false;
        currentPath.Clear();

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.RegisterMove();
        }

        CheckWinCondition();
    }

    void EndDrag()
    {
        if (!isDragging) return;

        // Hedef noktaya ulaþmadan býrakýldýysa yolu temizle
        ClearPathForColor(activeColorId);
        isDragging = false;
        currentPath.Clear();
    }

    void ClearPathForColor(int colorId)
    {
        if (completedPaths.ContainsKey(colorId))
        {
            foreach (var c in completedPaths[colorId])
            {
                c.ClearPath();
            }
            completedPaths.Remove(colorId);
        }

        foreach (var c in currentPath)
        {
            if (c.currentOccupiedColor == colorId)
            {
                c.ClearPath();
            }
        }
    }

    void CheckWinCondition()
    {
        if (currentData == null) return;

        // 1. KONTROL: Tüm renk çiftleri baðlandý mý?
        if (completedPaths.Count != currentData.pairs.Length)
        {
            return;
        }

        // 2. KONTROL: Izgaradaki TÜM kareler dolduruldu mu? (Boþ hücre kalmamalý)
        if (!IsGridFullyOccupied())
        {
            return;
        }

        // Ýki þart da saðlandýysa oyun kazanýldý!
        isWon = true;
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.OnLevelCompleted();
        }
    }

    bool IsGridFullyOccupied()
    {
        for (int r = 0; r < gridSize; r++)
        {
            for (int c = 0; c < gridSize; c++)
            {
                // Bir hücre ne nokta ne de bir renkle boyanmýþ boru ise alan henüz dolmamýþtýr
                if (grid[r, c].currentOccupiedColor == -1 && !grid[r, c].isDot)
                {
                    return false;
                }
            }
        }
        return true;
    }
}