using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;

    private bool SetupVisuals()
    {
        if (itemData == null)
        {
            return false;
        }
        GetComponent<SpriteRenderer>().sprite = itemData.itemIcon;
        gameObject.name = itemData.name;
        return true;
    }

    public void SetupItem(ItemData _itemData, Vector2 _velocity)
    {
        itemData = _itemData;
        rb.velocity = _velocity;
        SetupVisuals();
    }

    public void PickupItem()
    {
        // 根据物品类型分别检查背包和材料栏是否已满
        if (itemData.itemType == ItemType.Equipment && !Inventory.instance.CanAddItem())
        {
            rb.velocity = new Vector2(0, 7);
            PlayerManager.instance.player.fX.CreatePopUpText("Equipment Inventory Full");
            return;
        }
        else if (itemData.itemType == ItemType.Material && !Inventory.instance.CanAddToStash())
        {
            rb.velocity = new Vector2(0, 7);
            PlayerManager.instance.player.fX.CreatePopUpText("Material Stash Full");
            return;
        }

        AudioManager.instance.PlaySFX(18, transform);
        Inventory.instance.AddItem(itemData);
        Destroy(gameObject);
    }
}