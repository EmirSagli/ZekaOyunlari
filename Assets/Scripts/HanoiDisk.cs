using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HanoiDisk : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int diskSize; // 1: En küçük, 6: En büyük
    public RectTransform rectTransform;
    public Image diskImage;

    [HideInInspector] public HanoiPeg currentPeg; // Þu an takýlý olduðu kule
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalLocalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (diskImage == null) diskImage = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
    }

    public void Setup(int size, float width, Color color, HanoiPeg peg)
    {
        diskSize = size;
        currentPeg = peg;

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(width, 35f);
        }
        if (diskImage != null)
        {
            diskImage.color = color;
        }
    }

    // 1. KURAL: Sadece tepedeki parça hareket ettirilebilir
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Eðer bu disk kendi kulesinin en tepesindeki disk deðilse sürüklemeye izin verme!
        if (currentPeg == null || currentPeg.PeekDisk() != this)
        {
            eventData.pointerDrag = null; // Sürüklemeyi tamamen iptal et
            return;
        }

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;

        // Diskin diðer sütunlarýn arkasýnda kalmamasý için en üst Canvas katmanýna taþý
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        // Sürüklerken altýndaki sütunun (Drop alanýnýn) algýlanabilmesi için raycast'i geçici kapat
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. Ekran Çözünürlüðünden baðýmsýz olarak fare pozisyonunu doðrudan takip et:
        transform.position = eventData.position;
    }

    // 4. KURAL: Geçerli bir kuleye býrakýlmadýysa orijinal yerine geri dönsün
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Eðer geçerli bir kuleye býrakýlmadýysa (currentPeg deðiþmediyse)
        if (transform.parent == canvas.transform)
        {
            ReturnToOrigin();
        }
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(originalParent, false);
        transform.localPosition = originalLocalPosition;
    }
}