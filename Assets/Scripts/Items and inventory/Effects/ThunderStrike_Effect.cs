using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThunderStrike", menuName = "Data/Item effect/ThunderStrike")]
public class ThunderStrike_Effect : ItemEffect
{
   [SerializeField] private GameObject thunderStrikePrefab;
   public override void ExecuteEffect(Transform _enemyPosition)
   {
      GameObject newThunderStrike = Instantiate(thunderStrikePrefab, _enemyPosition.position, Quaternion.identity);
      Destroy(newThunderStrike, 1f);
   }
}
