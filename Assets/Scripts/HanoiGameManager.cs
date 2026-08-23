using UnityEngine;

public class HanoiGameManager : MonoBehaviour
{
    public static HanoiGameManager Instance { get; private set; }

    [Header("Veritabaný")]
    public HanoiLevelData[] allLevels;
    public GameObject diskPrefab;

    [Header("Sütunlar (Sol: 0, Orta: 1, Sað: 2)")]
    public HanoiPeg[] pegs;

    [Header("Disk Renkleri (Küçükten Büyüðe)")]
    public Color[] diskColors = new Color[]
    {
        new Color(0.95f, 0.3f, 0.3f), // Kýrmýzý (1)
        new Color(1f, 0.6f, 0.2f),    // Turuncu (2)
        new Color(0.95f, 0.9f, 0.2f), // Sarý (3)
        new Color(0.3f, 0.85f, 0.4f), // Yeþil (4)
        new Color(0.2f, 0.6f, 0.95f), // Mavi (5)
        new Color(0.6f, 0.3f, 0.9f)   // Mor (6)
    };

    private HanoiLevelData currentData;
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

        int levelNum = PlayerPrefs.GetInt("CurrentLevelNumber", 1);
        int dataIndex = Mathf.Clamp(levelNum - 1, 0, (allLevels != null && allLevels.Length > 0) ? allLevels.Length - 1 : 0);
        currentData = (allLevels != null && allLevels.Length > 0) ? allLevels[dataIndex] : null;

        if (GameplayManager.Instance != null && currentData != null)
        {
            GameplayManager.Instance.optimalMoves3Stars = currentData.targetMoves3Stars;
            GameplayManager.Instance.targetMoves2Stars = currentData.targetMoves2Stars;
            GameplayManager.Instance.StartGameTracking();
        }

        SetupBoard();
    }

    void SetupBoard()
    {
        foreach (var peg in pegs)
        {
            peg.ClearPeg();
        }

        int count = (currentData != null) ? currentData.diskCount : 3;
        int startIdx = (currentData != null) ? currentData.startPegIndex : 0;

        float minWidth = 80f;
        float maxWidth = 190f;
        float step = (maxWidth - minWidth) / Mathf.Max(1, count - 1);

        // Büyükten (tabandan) küçüðe (tepeye) doðru üret
        for (int i = count; i >= 1; i--)
        {
            GameObject diskObj = Instantiate(diskPrefab, pegs[startIdx].diskHolder);
            HanoiDisk disk = diskObj.GetComponent<HanoiDisk>();

            float width = minWidth + (i - 1) * step;
            Color col = (i - 1 < diskColors.Length) ? diskColors[i - 1] : Color.cyan;

            disk.Setup(i, width, col, pegs[startIdx]);

            // Stack'e ekle ve en üste oturt
            pegs[startIdx].disksOnPeg.Push(disk);
            diskObj.transform.SetAsLastSibling();
        }
    }

    // Disk geçerli bir kuleye býrakýldýðýnda tetiklenir
    public void OnSuccessfulMove()
    {
        if (isWon) return;

        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.RegisterMove();
        }

        CheckWinCondition();
    }

    void CheckWinCondition()
    {
        int targetIdx = (currentData != null) ? currentData.targetPegIndex : 2;
        int requiredDisks = (currentData != null) ? currentData.diskCount : 3;

        if (pegs[targetIdx].disksOnPeg.Count == requiredDisks)
        {
            isWon = true;
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.OnLevelCompleted();
            }
        }
    }
}