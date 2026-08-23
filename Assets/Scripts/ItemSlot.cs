using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public int slotID; // Yuvaya özel kimlik numarasý (Örn: 1. Yuva)

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem draggable = droppedObject.GetComponent<DraggableItem>();

        // Eðer yuva boþsa parçayý kabul et
        if (draggable != null && transform.childCount == 0)
        {
            draggable.parentAfterDrag = transform;

            // PARÇA YUVAYA BIRAKILDIÐI AN HAMLE SAYISINI 1 ARTIRIR!
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.RegisterMove();
            }

            // TODO: Seviyenin tamamlanýp tamamlanmadýðý kontrolü burada tetiklenecek
        }
    }
}