using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{

      [SerializeField] private int PossilbleItemDrop;
      [SerializeField] private ItemData[] possibleDrop;
      private List<ItemData> dropList;
      [SerializeField] private GameObject dropPrefab;


      private bool hasDropped = false;
      public virtual void GenerateDrop()
      {
            // 初始化掉落列表
            dropList = new List<ItemData>();
            // 根据每个物品的掉落概率决定是否加入掉落列表
            for (int i = 0; i < possibleDrop.Length; i++)
            {
                  if (Random.Range(0, 100) <= possibleDrop[i].dropChance)
                        dropList.Add(possibleDrop[i]);
            }

            // 如果没有物品可掉落，直接返回
            if (dropList.Count == 0)
                  return;

            for (int i = 0; i < PossilbleItemDrop && dropList.Count > 0; i++)
            {
                  ItemData randomItem = dropList[Random.Range(0, dropList.Count)];
                  dropList.Remove(randomItem);
                  DropItem(randomItem);
            }
      }


      /// <summary>
      /// 生成指定物品的掉落实例
      /// </summary>
      /// <param name="_itemData">要掉落的物品数据</param>
      protected void DropItem(ItemData _itemData)
      {
            if (hasDropped)
                  return;
            if (dropPrefab == null)
            {
                  Debug.LogError($"[{gameObject.name}] 的 dropPrefab 未设置！无法生成掉落物品。");
                  return;
            }

            hasDropped = true;
            GameObject newDrop = Instantiate(dropPrefab, transform.position, Quaternion.identity);
            // 给掉落物品一个随机的初始速度，使其看起来像是被弹出
            Vector2 randomVelocity = new Vector2(Random.Range(-5f, 5f), Random.Range(15f, 20f));
            newDrop.GetComponent<ItemObject>().SetupItem(_itemData, randomVelocity);
      }
}
