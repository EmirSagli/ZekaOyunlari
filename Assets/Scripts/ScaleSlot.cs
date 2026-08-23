using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleSlot : MonoBehaviour, IDropHandler
{
    public enum ScaleSide { Left, Right }
    public ScaleSide side;

    [Header("Kapasite Sýnýrý")]
    public int maxCapacity = 3; // Bir kefeye en fazla 3 aðýrlýk konabilir

    [Header("Dinamik Hizalama & Piramit Ayarlarý")]
    public float itemSpacingX = 80f;
    public float itemSpacingY = 70f;
    public float baseOffsetY = 40f;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        DraggableItem draggable = droppedObject.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            // 1. KAPASÝTE KONTROLÜ: Kefede zaten 3 veya daha fazla parça varsa kabul etme!
            if (transform.childCount >= maxCapacity)
            {
                // Parça kabul edilmediði için parentAfterDrag deðiþmez ve alt panele döner
                return;
            }

            // 2. Parçayý kefeye kabul et
            draggable.parentAfterDrag = transform;
            draggable.transform.SetParent(transform);

            // 3. Kefedeki parçalarý yeniden piramit þeklinde diz
            RearrangeWeights();

            // 4. Hamle sayýsýný artýr
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.RegisterMove();
            }

            // 5. Teraziyi kontrol et
            if (BalanceGameManager.Instance != null)
            {
                BalanceGameManager.Instance.CheckBalance();
            }
        }
    }

    public void RearrangeWeights()
    {
        int count = transform.childCount;

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            child.localPosition = GetPositionForIndex(i, count);
        }
    }

    private Vector3 GetPositionForIndex(int index, int totalCount)
    {
        if (totalCount == 1)
        {
            return new Vector3(0, baseOffsetY, 0); // Tek parça tam ortada
        }

        switch (index)
        {
            case 0:
                return new Vector3(-itemSpacingX / 2f, baseOffsetY, 0); // 1. Parça: Sol alt

            case 1:
                return new Vector3(itemSpacingX / 2f, baseOffsetY, 0);  // 2. Parça: Sað alt

            case 2:
                return new Vector3(0, baseOffsetY + itemSpacingY, 0);   // 3. Parça: Üst orta

            default:
                return Vector3.zero;
        }
    }
}