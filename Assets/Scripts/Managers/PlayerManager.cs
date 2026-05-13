using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//P63，制作玩家管理器和技能管理器
//可以在SkeletonBattleState中通过PlayerManager.instance.player.transform获取到玩家的位置
public class PlayerManager : MonoBehaviour, ISaveManager
{
    //全局访问
    public static PlayerManager instance;//单例模式
    public Player player;

    public int currency;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }


    public bool HaveEnoughMoney(int _price)//是否有钱去买技能
    {
        if (_price > currency)
        {
            Debug.Log("没有足够的钱");
            return false;
        }
        else

            currency -= _price;
        return true;
    }


    public int GetCurrency() => currency;//返回当前的货币数量


    public void LoadData(GameData _data)
    {
        currency = _data.currency;
    }

    public void SaveData(ref GameData _data)
    {
        _data.currency = this.currency;
    }
}