using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{
    public Equip curEquip;
    public Transform equipParent;

    private PlayerController controller;
    private PlayerCondition condition;
    public Transform firstPersonEquip; 
    public Transform thirdPersonEquip;  

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
    }

    public void EquipNew(ItemData data)
    {
        UnEquip();
        Transform parent = controller.isThirdPerson ? thirdPersonEquip : firstPersonEquip;
        curEquip = Instantiate(data.equipPrefab, parent).GetComponent<Equip>();
    }

    public void UnEquip()
    {
        if (curEquip != null)
        {
            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }

    public void SwitchPerspective(bool toThirdPerson)
    {
        if (curEquip == null)
        {
            return;
        }

        Transform newParent = toThirdPerson ? thirdPersonEquip : firstPersonEquip;
        curEquip.transform.SetParent(newParent);
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && curEquip != null && controller.canLook)
        {
            curEquip.OnAttackInput();
        }
    }
}
