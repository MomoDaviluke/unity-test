// Cinemachine 3.x（Unity 6）：命名空间由 Cinemachine 改为 Unity.Cinemachine，
// CinemachineImpulseSource 类名不变；2.x 的 m_DefaultVelocity 字段改为 DefaultVelocity 属性
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// PlayerFX（玩家特效）- 继承自EntityFX，实现玩家特有的视觉效果
/// 功能：屏幕震动、残影特效、灰尘粒子
/// </summary>
public class PlayerFX : EntityFX
{
    // ==================== 屏幕震动 ====================
    private CinemachineImpulseSource screenShake;

    [Header("震动特效")]
    [SerializeField] private float shakeMultiplier;
    public Vector3 shakeSwordImpact;
    public Vector3 shakeHeightDamage;

    // ==================== 残影特效 ====================
    [Header("残影特效")]
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private float colorLooseRate;
    [SerializeField] private float afterImageCooldown;
    private float afterImageCooldownTimer;

    [Space]
    [SerializeField] private ParticleSystem dustFx;

    // ==================== 初始化 ====================
    protected override void Start()
    {
        base.Start();
        screenShake = GetComponent<CinemachineImpulseSource>();
    }

    // ==================== 冷却更新 ====================
    private void Update()
    {
        afterImageCooldownTimer -= Time.deltaTime;
    }

    // ==================== 创建残影 ====================
    // Phase 6 池化接入：残影高频生成（闪避/冲刺期间每 0.05 秒一个），淡出结束由 AfterImageFX 回池
    public void CreateAfterImage()
    {
        if (afterImageCooldownTimer < 0)
        {
            afterImageCooldownTimer = afterImageCooldown;
            GameObject newAfterImage = PoolManager.Instance.Spawn(afterImagePrefab, transform.position, transform.rotation);
            if (newAfterImage == null)
                return;
            AfterImageFX fx = newAfterImage.GetComponent<AfterImageFX>();
            if (fx != null)
                fx.SetupAfterImage(colorLooseRate, sr.sprite);
        }
    }

    // ==================== 屏幕震动 ====================
    public void ScreenShake(Vector3 _shakePower)
    {
        // Cinemachine 3.x：m_DefaultVelocity 字段迁移为 DefaultVelocity 属性
        screenShake.DefaultVelocity = new Vector3(_shakePower.x * player.facingDir, _shakePower.y) * shakeMultiplier;
        screenShake.GenerateImpulse();
    }

    // ==================== 灰尘粒子 ====================
    public void PlayDustFx()
    {
        if (dustFx != null)
            dustFx.Play();
    }
}
