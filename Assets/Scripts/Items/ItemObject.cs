using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ItemObject（物品实体）- 场景中掉落的物品实体
/// 功能：设置物品外观、拾取检测、背包满时弹开
/// </summary>
public class ItemObject : MonoBehaviour
{
    // ==================== 核心组件 ====================
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private ItemData itemData;

    // ==================== 视觉设置 ====================
    private bool SetupVisuals()
    {
        if (itemData == null)
            return false;

        GetComponent<SpriteRenderer>().sprite = itemData.itemIcon;
        gameObject.name = itemData.name;
        return true;
    }

    // ==================== 物品初始化 ====================
    public void SetupItem(ItemData _itemData, Vector2 _velocity)
    {
        itemData = _itemData;
        rb.linearVelocity = _velocity;
        SetupVisuals();
    }

    // ==================== 拾取逻辑 ====================
    // 根据物品类型检查背包/仓库是否已满，满则弹开并提示
    public void PickupItem()
    {
        if (itemData.itemType == ItemType.Equipment && !Inventory.instance.CanAddItem())
        {
            rb.linearVelocity = new Vector2(0, 7);
            PlayerManager.instance.player.fX.CreatePopUpText("Equipment Inventory Full");
            return;
        }
        else if (itemData.itemType == ItemType.Material && !Inventory.instance.CanAddToStash())
        {
            rb.linearVelocity = new Vector2(0, 7);
            PlayerManager.instance.player.fX.CreatePopUpText("Material Stash Full");
            return;
        }

        AudioManager.instance.PlaySFX(SfxIds.ItemPickup, transform);
        Inventory.instance.AddItem(itemData);
        Destroy(gameObject);
    }
}