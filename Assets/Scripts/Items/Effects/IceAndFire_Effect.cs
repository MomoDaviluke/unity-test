using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IceAndFire_Effect（冰火效果）- 第三次攻击时发射冰火投射物
/// 功能：检测连击数，第三击时生成投射物
/// </summary>
[CreateAssetMenu(fileName = "Ice and Fire effect", menuName = "Data/Item effect/Ice and Fire")]
public class IceAndFire_Effect : ItemEffect
{
    [SerializeField] private GameObject iceAndFirePrefab;
    [SerializeField] private float xVelocity;

    // ==================== 效果执行 ====================
    public override void ExecuteEffect(Transform _respondPosition)
    {
        Player player = PlayerManager.instance.player;
        bool thirdAttack = player.GetComponent<Player>().primaryAttackState.comboCounter == 2;

        if (thirdAttack)
        {
            GameObject newIceAndFire = Instantiate(iceAndFirePrefab, _respondPosition.position, player.transform.rotation);
            newIceAndFire.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(xVelocity * player.facingDir, 0);
            Destroy(newIceAndFire, 8f);
        }
    }
}
