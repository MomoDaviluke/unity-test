using System;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// GameEvents 事件总线测试（Phase 2 基建）
/// 覆盖：订阅后收到事件、退订后不再收到、PlayerManager.HaveEnoughMoney 最小生产者接线。
/// 静态事件跨用例残留风险：每个用例内显式退订，TearDown 兜底清理。
/// </summary>
public class GameEventsTests
{
    private Action<int> _handler;
    private int _received;
    private GameObject _playerManagerGo;

    [SetUp]
    public void Setup()
    {
        _received = -1;
        _handler = v => _received = v;
    }

    [TearDown]
    public void TearDown()
    {
        GameEvents.OnCurrencyChanged -= _handler;
        if (_playerManagerGo != null)
        {
            // EditMode 下 DestroyImmediate 不触发 OnDestroy，需手动清空继承的静态单例引用防泄漏
            UnityEngine.Object.DestroyImmediate(_playerManagerGo);
            _playerManagerGo = null;
            PlayerManager.instance = null;
        }
    }

    [Test]
    public void Subscriber_ReceivesEvent_WhileSubscribed()
    {
        GameEvents.OnCurrencyChanged += _handler;
        GameEvents.CurrencyChanged(42);
        Assert.AreEqual(42, _received, "订阅者应收到触发值");
    }

    [Test]
    public void Subscriber_StopsReceiving_AfterUnsubscribe()
    {
        GameEvents.OnCurrencyChanged += _handler;
        GameEvents.CurrencyChanged(42);
        GameEvents.OnCurrencyChanged -= _handler;
        GameEvents.CurrencyChanged(99);
        Assert.AreEqual(42, _received, "退订后不应再收到事件（防悬空引用契约）");
    }

    [Test]
    public void PlayerManager_HaveEnoughMoney_TriggersCurrencyChanged()
    {
        _playerManagerGo = new GameObject("PlayerManager_Test");
        PlayerManager pm = _playerManagerGo.AddComponent<PlayerManager>();
        pm.currency = 100;

        GameEvents.OnCurrencyChanged += _handler;
        bool paid = pm.HaveEnoughMoney(30);

        Assert.IsTrue(paid);
        Assert.AreEqual(70, pm.currency, "扣款应生效");
        Assert.AreEqual(70, _received, "最小生产者示例：扣款后应广播新货币值");
    }
}
