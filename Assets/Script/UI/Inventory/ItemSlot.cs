using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public ItemData item;
    public UIInventory inventory;
    public Button button;
    public Image icon;
    public TextMeshProUGUI quantityText;
    private Outline outline;

    public int index;
    public bool equipped;
    public int quantity;

    private Image draggedIcon;
    private Canvas parentCanvas;

    void Awake()
    {
        outline = GetComponent<Outline>();
    }

    void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        outline.enabled = equipped;
    }

    public void Set()
    {
        icon.gameObject.SetActive(true);
        icon.sprite = item.icon;
        quantityText.text = quantity > 1 ? quantity.ToString() : string.Empty;
        if (outline != null)
            outline.enabled = equipped;
    }

    public void Clear()
    {
        item = null;
        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;
    }

    public void onClickButton()
    {
        inventory.SelectItem(index);
    }

    // === Drag & Drop ===
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;

        // Create temporary dragged icon
        draggedIcon = new GameObject("DraggedIcon", typeof(Image)).GetComponent<Image>();
        draggedIcon.transform.SetParent(parentCanvas.transform, false);
        draggedIcon.sprite = icon.sprite;
        draggedIcon.rectTransform.sizeDelta = icon.rectTransform.sizeDelta;
        draggedIcon.raycastTarget = false;

        icon.enabled = false; // hide original icon
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint
            );
            draggedIcon.rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon.gameObject);
            draggedIcon = null;
        }
        icon.enabled = true; // show original icon again
    }

    public void OnDrop(PointerEventData eventData)
    {
        var fromSlot = eventData.pointerDrag?.GetComponent<ItemSlot>();
        if (fromSlot != null && fromSlot != this)
        {
            var tempItem = item;
            var tempQuantity = quantity;
            var tempEquipped = equipped;

            item = fromSlot.item;
            quantity = fromSlot.quantity;
            equipped = fromSlot.equipped;

            fromSlot.item = tempItem;
            fromSlot.quantity = tempQuantity;
            fromSlot.equipped = tempEquipped;

            inventory.UpdateUI();
        }
    }
}
