using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kütüphanesi eklendi

public class WeightItem : MonoBehaviour
{
    [Header("Aðýrlýk Deðeri")]
    public int weightValue = 1;

    [Header("UI Görsel ve Metin")]
    public Image itemImage;
    public TextMeshProUGUI weightText; // Aðýrlýðýn üzerindeki yazý (Örn: "3 kg")

    public void SetupWeight(int value, Sprite sprite = null)
    {
        weightValue = value;

        // Üzerindeki yazýyý güncelle
        if (weightText != null)
        {
            weightText.text = $"{value} kg";
        }

        // Eðer sprite verilmiþse görseli deðiþtir
        if (itemImage != null && sprite != null)
        {
            itemImage.sprite = sprite;
        }
    }
}