using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;
    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;
    [Header("Select Item")]
    private ItemSlot selecetedItem;
    private int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedStatName;
    public TextMeshProUGUI selectedStatValue;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unequipButton;
    public GameObject dropButton;

    private int curEquipIndex;

    private PlayerController controller;
    private PlayerCondition condition;
    private BoostUI boostUI;

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;
        boostUI = FindObjectOfType<BoostUI>();

        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;

        inventoryWindow.SetActive(false);
        slots = new ItemSlot[slotPanel.childCount];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
            slots[i].Clear();
        }

        ClearSelectedItemWindow();
    }

    private void ClearSelectedItemWindow()
    {
        selecetedItem = null;

        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedStatName.text = string.Empty;
        selectedStatValue.text = string.Empty;

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unequipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void Toggle()
    {
        if (IsOpen())
        {
            inventoryWindow.SetActive(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData;

        //// If the item can be stacked, try adding to an existing stack
        if (data.canStack)
        {
            ItemSlot slot = GetItemStack(data);
            if (slot != null)
            {
                slot.quantity++;
                UpdateUI();
                CharacterManager.Instance.Player.itemData = null;
                return;
            }
        }

        ItemSlot emptySlot = getEmptySlot();
        //If no Stack is found just put it into an empty slot. 
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }
        //if the Inventory is full, drop the item
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        //Find a slot with the same item, and is not at max capacity. 
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }

    ItemSlot getEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    public void ThrowItem(ItemData data)
    {
        //Make the item drop on the transform postion of the player
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    public void SelectItem(int index)
    {
        if (slots[index].item == null)
        {
            return;
        }

        selecetedItem = slots[index];
        selectedItemIndex = index;

        selectedItemName.text = selecetedItem.item.displayName;
        selectedItemDescription.text = selecetedItem.item.description;

        selectedStatName.text = string.Empty;
        selectedStatValue.text = string.Empty;

        for (int i = 0; i < selecetedItem.item.consumables.Length; i++)
        {
            //Display all consumable stats
            selectedStatName.text += selecetedItem.item.consumables[i].type.ToString() + "\n";
            selectedStatValue.text += selecetedItem.item.consumables[i].value.ToString() + "\n";
        }
        // iF the item is a consubmable then set active
        // If the item is eqiupable then show the equip button
        // If 
        useButton.SetActive(selecetedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(selecetedItem.item.type == ItemType.Equipable && !slots[index].equipped);
        unequipButton.SetActive(selecetedItem.item.type == ItemType.Equipable && slots[index].equipped);
        dropButton.SetActive(true);
    }

    public void OnUseButton()
    {
        if (selecetedItem.item.type == ItemType.Consumable)
        {
            int boostDuration = selecetedItem.item.boostDuration;
            for (int i = 0; i < selecetedItem.item.consumables.Length; i++)
            {
                switch (selecetedItem.item.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selecetedItem.item.consumables[i].value);
                        break;
                    case ConsumableType.Stamina:
                        condition.Recover(selecetedItem.item.consumables[i].value);
                        break;
                    case ConsumableType.Boost:
                        var boost = selecetedItem.item.consumables[i];
                        switch (boost.boostType)
                        {
                            case BoostType.Speed:
                                condition.Boost(
                                    () => controller.moveSpeed,
                                    value => controller.moveSpeed = value,
                                    boost.value,
                                    boostDuration,
                                    BoostType.Speed
                                );
                                boostUI.ShowOrRefreshBoostTimer(boost.boostType, boostDuration);
                                break;

                            case BoostType.Jump:
                                condition.Boost(
                                    () => controller.jumpPower,
                                    value => controller.jumpPower = value,
                                    boost.value,
                                    boostDuration,
                                    BoostType.Jump
                                );
                                boostUI.ShowOrRefreshBoostTimer(boost.boostType, boostDuration);
                                break;
                            case BoostType.Stamina:
                                condition.Boost(
                                    () => 0f,
                                    value => { },
                                    0f,
                                    boostDuration,
                                    BoostType.Stamina
                                );
                                boostUI.ShowOrRefreshBoostTimer(boost.boostType, boostDuration);
                                break;
                        }
                        break;
                }
            }
            RemoveSelectedItem();
        }
    }

    public void OnDropButton()
    {
        ThrowItem(selecetedItem.item);
        RemoveSelectedItem();
    }

    private void RemoveSelectedItem()
    {
        selecetedItem.quantity--;

        if (selecetedItem.quantity <= 0)
        {
            if (slots[selectedItemIndex].equipped)
            {
                UnEquip(selectedItemIndex);
            }

            selecetedItem.item = null;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }

    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }
    public void OnEquipButton()
    {
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }

        slots[selectedItemIndex].equipped = true;
        curEquipIndex = selectedItemIndex;
        CharacterManager.Instance.Player.equip.EquipNew(selecetedItem.item);
        UpdateUI();

        SelectItem(selectedItemIndex);
    }

    void UnEquip(int index)
    {
        slots[index].equipped = false;
        CharacterManager.Instance.Player.equip.UnEquip();
        UpdateUI();

        if (selectedItemIndex == index)
        {
            SelectItem(selectedItemIndex);
        }
    }
    public void OnUnEquipButton()
    {
        UnEquip(selectedItemIndex);
    }    
}
