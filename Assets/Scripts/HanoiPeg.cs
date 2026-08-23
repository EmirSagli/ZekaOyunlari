using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HanoiPeg : MonoBehaviour, IDropHandler
{
    public int pegIndex; // 0: Sol, 1: Orta, 2: Sað
    public Transform diskHolder; // Disklerin dizileceði alan
    public Stack<HanoiDisk> disksOnPeg = new Stack<HanoiDisk>();

    // 2. KURAL & 3. KURAL: Diskin kule üzerine býrakýlmasý ve boyut kontrolü
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        HanoiDisk incomingDisk = eventData.pointerDrag.GetComponent<HanoiDisk>();
        if (incomingDisk == null) return;

        // Eðer zaten bu kuledeyse eski yerine dönsün
        if (incomingDisk.currentPeg == this)
        {
            incomingDisk.ReturnToOrigin();
            return;
        }

        // 2. KURAL: Kendinden daha küçük bir diskin üstüne konulamaz!
        if (CanPush(incomingDisk))
        {
            // Eski kuleden çýkart
            if (incomingDisk.currentPeg != null)
            {
                incomingDisk.currentPeg.PopDisk();
            }

            // Yeni kuleye ekle
            PushDisk(incomingDisk);

            // Baþarýlý hamle bildirimi
            HanoiGameManager.Instance.OnSuccessfulMove();
        }
        else
        {
            // 4. KURAL: Kurala uyulmadýysa eski yerine geri dönsün
            incomingDisk.ReturnToOrigin();
        }
    }

    public bool CanPush(HanoiDisk incomingDisk)
    {
        if (disksOnPeg.Count == 0) return true; // Boþ kuleye her disk konabilir
        return incomingDisk.diskSize < disksOnPeg.Peek().diskSize; // Gelen disk tepedekinden küçük olmalý
    }

    public void PushDisk(HanoiDisk disk)
    {
        disksOnPeg.Push(disk);
        disk.currentPeg = this;
        disk.transform.SetParent(diskHolder, false);
        disk.transform.SetAsLastSibling();
    }

    public HanoiDisk PopDisk()
    {
        if (disksOnPeg.Count == 0) return null;
        return disksOnPeg.Pop();
    }

    public HanoiDisk PeekDisk()
    {
        if (disksOnPeg.Count == 0) return null;
        return disksOnPeg.Peek();
    }

    public void ClearPeg()
    {
        disksOnPeg.Clear();
        if (diskHolder != null)
        {
            foreach (Transform child in diskHolder)
            {
                Destroy(child.gameObject);
            }
        }
    }
}