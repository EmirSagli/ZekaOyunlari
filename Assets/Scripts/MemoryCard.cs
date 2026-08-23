using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MemoryCard : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referanslarý")]
    public Image cardBackImage;   // Kapalý arka yüz
    public Image cardFrontImage;  // Açýk ön yüz (Renkli zemin)

    [HideInInspector] public int cardId;
    [HideInInspector] public bool isRevealed = false;
    [HideInInspector] public bool isMatched = false;

    private bool isFlipping = false;

    public void Setup(int id, Color cardColor, Sprite backSprite)
    {
        cardId = id;
        isRevealed = false;
        isMatched = false;

        if (cardFrontImage != null)
        {
            cardFrontImage.color = cardColor;
        }

        if (cardBackImage != null && backSprite != null)
        {
            cardBackImage.sprite = backSprite;
        }

        cardFrontImage.gameObject.SetActive(false);
        cardBackImage.gameObject.SetActive(true);
        transform.localRotation = Quaternion.identity;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRevealed || isMatched || isFlipping) return;
        MemoryGameManager.Instance.OnCardClicked(this);
    }

    public void FlipToFront()
    {
        StartCoroutine(FlipRoutine(true));
    }

    public void FlipToBack()
    {
        StartCoroutine(FlipRoutine(false));
    }

    private IEnumerator FlipRoutine(bool showFront)
    {
        isFlipping = true;
        float duration = 0.12f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(0, 90, elapsed / duration);
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        cardFrontImage.gameObject.SetActive(showFront);
        cardBackImage.gameObject.SetActive(!showFront);
        isRevealed = showFront;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(90, 0, elapsed / duration);
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
        isFlipping = false;
    }

    public void SetMatched()
    {
        isMatched = true;
        StartCoroutine(MatchPulse());
    }

    private IEnumerator MatchPulse()
    {
        transform.localScale = Vector3.one * 1.15f;
        yield return new WaitForSeconds(0.12f);
        transform.localScale = Vector3.one;
    }
}