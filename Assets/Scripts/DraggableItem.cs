using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;

    private Transform originalParent;
    private RectTransform inventoryRect;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
             canvasGroup = gameObject.AddComponent<CanvasGroup>(); 
    }

    private void Start()
    {
         originalParent = transform.parent; 
        if (originalParent != null)
        {
            inventoryRect = originalParent.GetComponent<RectTransform>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Transform oldParent = transform.parent;

        parentAfterDrag = originalParent; // Varsayýlan hedef alt panel [cite: 883]
        canvasGroup.blocksRaycasts = false; // Yuvalarý algýlayabilmesi için [cite: 884]
        transform.SetParent(transform.root); // Sürüklerken en öne al [cite: 885]

        // Eðer kefeden alýnýyorsa kefeyi yeniden diz ve aðýrlýklarý düþür
         ScaleSlot oldSlot = oldParent.GetComponent<ScaleSlot>();
         if (oldSlot != null)
        {
             oldSlot.RearrangeWeights(); 
             if (BalanceGameManager.Instance != null) 
            {
                BalanceGameManager.Instance.CheckBalance(); 
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // Parmaðý/Fareyi takip et [cite: 889]
    }

    public void OnEndDrag(PointerEventData eventData)
    {
         canvasGroup.blocksRaycasts = true; 
        transform.SetParent(parentAfterDrag); 

        // EÐER KEFEYE OTURMADIYSA (Yani alt paneldeyse veya boþluða býrakýldýysa):
        ScaleSlot currentSlot = parentAfterDrag.GetComponent<ScaleSlot>();
        if (currentSlot == null && inventoryRect != null)
        {
            // Býrakýlan dünya pozisyonunu panelin yerel (local) koordinatýna çevir
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                inventoryRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            // Panelin sýnýrlarý dýþýna çýkmasýný engelle (En yakýn güvenli sýnýra sýnýrla / Clamp)
            float padding = 60f; // Aðýrlýðýn panelin dýþýna taþmamasý için kenar payý
            float minX = -inventoryRect.rect.width / 2f + padding;
            float maxX = inventoryRect.rect.width / 2f - padding;
            float minY = -inventoryRect.rect.height / 2f + padding;
            float maxY = inventoryRect.rect.height / 2f - padding;

            float clampedX = Mathf.Clamp(localPoint.x, minX, maxX);
            float clampedY = Mathf.Clamp(localPoint.y, minY, maxY);

            transform.localPosition = new Vector3(clampedX, clampedY, 0f);
        }

        // Teraziyi güncelle
        if (BalanceGameManager.Instance != null) 
        {
            BalanceGameManager.Instance.CheckBalance(); 
        }
    }
}