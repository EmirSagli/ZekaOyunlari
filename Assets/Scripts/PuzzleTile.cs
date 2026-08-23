using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PuzzleTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int correctIndex;    // Parçanýn olmasý gereken hedef sýra (0, 1, 2...)
    [HideInInspector] public int currentGridIndex; // Tablodaki þu anki konumu

    public Image tileImage;
    public TextMeshProUGUI numberText;

    [Header("Görsel Renk Ayarlarý")]
    public Color normalColor = Color.white;
    public Color correctColor = new Color(0.6f, 1f, 0.6f, 1f); // Doðru yerdeyse yeþil

    private CanvasGroup canvasGroup;
    private Transform originalParent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetupTile(int correctIdx, int currentIdx, Sprite sprite)
    {
        correctIndex = correctIdx;
        currentGridIndex = currentIdx;

        if (numberText != null)
        {
            numberText.text = (correctIdx + 1).ToString();
        }

        if (tileImage != null && sprite != null)
        {
            tileImage.sprite = sprite;
        }

        UpdateVisualState();
    }

    public void UpdateVisualState()
    {
        if (tileImage != null)
        {
            tileImage.color = (currentGridIndex == correctIndex) ? correctColor : normalColor;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PuzzleGameManager.Instance == null || !PuzzleGameManager.Instance.CanMove(currentGridIndex))
        {
            eventData.pointerDrag = null;
            return;
        }

        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root); // Sürüklerken en öne al
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        transform.SetParent(originalParent);

        if (PuzzleGameManager.Instance == null) return;

        Vector3 emptySlotWorldPos = PuzzleGameManager.Instance.GetEmptySlotWorldPosition();
        float dropDistance = Vector3.Distance(transform.position, emptySlotWorldPos);

        // Boþluðun üzerine veya yakýnýna býrakýldýysa taþý
        if (dropDistance < 180f)
        {
            PuzzleGameManager.Instance.MoveTileToEmpty(this);
        }
        else
        {
            PuzzleGameManager.Instance.ResetTilePosition(this);
        }
    }
}