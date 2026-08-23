using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FlowCell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    public int row;
    public int col;

    [Header("UI Referanslarý")]
    public Image backgroundImage;
    public Image dotImage;      // Nokta ise gösterilecek yuvarlak
    public Image pathFillImage;  // Boru geçtiðinde renklenecek dolgu

    [HideInInspector] public bool isDot = false;
    [HideInInspector] public int dotColorId = -1;
    [HideInInspector] public int currentOccupiedColor = -1;

    public void Setup(int r, int c)
    {
        row = r;
        col = c;
        isDot = false;
        dotColorId = -1;
        currentOccupiedColor = -1;

        if (dotImage != null) dotImage.gameObject.SetActive(false);
        if (pathFillImage != null) pathFillImage.gameObject.SetActive(false);
    }

    public void SetDot(int colorId, Color color)
    {
        isDot = true;
        dotColorId = colorId;
        currentOccupiedColor = colorId;

        if (dotImage != null)
        {
            dotImage.gameObject.SetActive(true);
            dotImage.color = color;
        }
    }

    public void SetPath(int colorId, Color color)
    {
        currentOccupiedColor = colorId;
        if (pathFillImage != null)
        {
            pathFillImage.gameObject.SetActive(true);
            pathFillImage.color = color;
        }
    }

    public void ClearPath()
    {
        if (!isDot)
        {
            currentOccupiedColor = -1;
            if (pathFillImage != null)
            {
                pathFillImage.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CircuitFlowManager.Instance.OnCellPointerDown(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CircuitFlowManager.Instance.OnCellPointerEnter(this);
    }
}