using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// EntityFX（实体特效基类）- 管理所有实体的视觉特效
/// 功能：弹出文本、受击闪光、异常状态效果、攻击特效
/// 设计思路：将特效逻辑集中管理，便于统一控制和扩展
/// </summary>
public class EntityFX : MonoBehaviour
{
    // ==================== 核心组件 ====================
    protected Player player;
    protected SpriteRenderer sr;

    // ==================== 弹出文本 ====================
    [Header("弹出文本")]
    [SerializeField] private GameObject popUpTextPrefab;

    // ==================== 闪光特效 ====================
    [Header("闪光特效")]
    [SerializeField] private float flashDuration;
    [SerializeField] private Material hitMat;
    private Material originalMat;

    // ==================== 异常状态颜色 ====================
    [Header("异常状态颜色")]
    [SerializeField] private Color[] chillColor;
    [SerializeField] private Color[] igniteColor;
    [SerializeField] private Color[] shockColor;

    // ==================== 异常状态粒子 ====================
    [Header("异常状态粒子")]
    [SerializeField] private ParticleSystem igniteFX;
    [SerializeField] private ParticleSystem chillFX;
    [SerializeField] private ParticleSystem shockFX;

    // ==================== 攻击特效 ====================
    [Header("攻击特效")]
    [SerializeField] private GameObject hitFx;
    [SerializeField] private GameObject criticalHitFx;

    // ==================== 弹出文本颜色 ====================
    [Header("弹出文本颜色")]
    [SerializeField] private Color physicalDamageColor = Color.white;
    [SerializeField] private Color fireDamageColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color iceDamageColor = new Color(0.5f, 0.8f, 1f);
    [SerializeField] private Color lightningDamageColor = new Color(1f, 1f, 0f);
    [SerializeField] private Color criticalDamageColor = new Color(1f, 0f, 0f);
    [SerializeField] private Color healColor = new Color(0f, 1f, 0f);
    private GameObject myHealthBar;

    // ==================== 初始化 ====================
    protected virtual void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError($"SpriteRenderer not found on {gameObject.name}");
            enabled = false;
            return;
        }

        if (PlayerManager.instance != null)
            player = PlayerManager.instance.player;
        else
            Debug.LogError("PlayerManager instance is null");

        originalMat = sr.material;

        var healthBar = GetComponentInChildren<UI_HealthBar>();
        if (healthBar != null)
            myHealthBar = healthBar.gameObject;
        else
            Debug.LogWarning($"UI_HealthBar not found on {gameObject.name}");
    }

    /// <summary>
    // ==================== 弹出文本 ====================
    public void CreatePopUpText(string _text, DamageType _damageType = DamageType.Physical, bool _isCritical = false)
    {
        float randomX = Random.Range(-1, 1);
        float randomY = Random.Range(1.5f, 3f);
        Vector3 positionOffset = new Vector3(randomX, randomY, 0);

        GameObject newText = Instantiate(popUpTextPrefab, transform.position + positionOffset, Quaternion.identity);
        TextMeshPro textMesh = newText.GetComponent<TextMeshPro>();
        textMesh.text = _text;

        if (_isCritical)
        {
            textMesh.color = criticalDamageColor;
            textMesh.fontSize += 2;
        }
        else
        {
            switch (_damageType)
            {
                case DamageType.Physical:
                    textMesh.color = physicalDamageColor;
                    break;
                case DamageType.Fire:
                    textMesh.color = fireDamageColor;
                    break;
                case DamageType.Ice:
                    textMesh.color = iceDamageColor;
                    break;
                case DamageType.Lightning:
                    textMesh.color = lightningDamageColor;
                    break;
                case DamageType.Heal:
                    textMesh.color = healColor;
                    break;
            }
        }
    }

    // ==================== 透明度控制 ====================
    public void MakeTransprent(bool _transprent)
    {
        if (sr == null)
        {
            Debug.LogWarning("SpriteRenderer is null in MakeTransprent");
            return;
        }
        if (_transprent)
        {
            if (myHealthBar != null)
                myHealthBar.SetActive(false);
            sr.color = Color.clear;
        }
        else
        {
            if (myHealthBar != null)
                myHealthBar.SetActive(true);
            sr.color = Color.white;
        }
    }

    // ==================== 受击闪光 ====================
    private IEnumerator FlashFX()
    {
        sr.material = hitMat;
        Color currentColor = sr.color;
        sr.color = Color.white;

        yield return new WaitForSeconds(flashDuration);

        sr.color = currentColor;
        sr.material = originalMat;
    }

    /// <summary>
    /// 红色闪烁：低血量警告
    /// </summary>
    private void RedColorBlink()
    {
        if (sr.color != Color.white)
            sr.color = Color.white;
        else
            sr.color = Color.red;
    }

    // ==================== 取消异常状态 ====================
    private void CancelColorChange()
    {
        CancelInvoke();
        sr.color = Color.white;

        igniteFX.Stop();
        chillFX.Stop();
        shockFX.Stop();
    }

    // ==================== 异常状态效果 ====================
    public void IgniteFxFor(float _seconds)
    {
        igniteFX.Play();
        InvokeRepeating("igniteColorFx", 0, 0.3f);
        Invoke("CancelColorChange", _seconds);
    }

    public void ChilFxFor(float _seconds)
    {
        chillFX.Play();
        InvokeRepeating("ChillColorFx", 0, 0.3f);
        Invoke("CancelColorChange", _seconds);
    }

    public void ShockFxFor(float _seconds)
    {
        shockFX.Play();
        InvokeRepeating("shockColorFx", 0, 0.3f);
        Invoke("CancelColorChange", _seconds);
    }

    private void igniteColorFx()
    {
        sr.color = sr.color != igniteColor[0] ? igniteColor[0] : igniteColor[1];
    }

    private void ChillColorFx()
    {
        sr.color = sr.color != chillColor[0] ? chillColor[0] : chillColor[1];
    }

    private void shockColorFx()
    {
        sr.color = sr.color != shockColor[0] ? shockColor[0] : shockColor[1];
    }

    // ==================== 受击特效 ====================
    public void CreateFitFx(Transform _target, bool _critical)
    {
        if (_target == null)
            return;

        float zRotation = Random.Range(-90, 90);
        Vector3 hitFxRotion = new Vector3(0, 0, zRotation);
        Vector3 hitPosition = _target.position;

        Collider2D targetCollider = _target.GetComponent<Collider2D>();
        if (targetCollider == null)
            targetCollider = _target.GetComponentInChildren<Collider2D>();

        if (targetCollider != null)
            hitPosition = targetCollider.ClosestPoint(transform.position);

        GameObject hitPrefab = hitFx;

        // 暴击使用不同的特效和旋转
        if (_critical)
        {
            hitPrefab = criticalHitFx;
            float yRotation = 0;
            zRotation = Random.Range(-45, 45);

            if (GetComponent<Entity>().facingDir == -1)
                yRotation = 180;

            hitFxRotion = new Vector3(0, yRotation, zRotation);
        }

        if (hitPrefab == null)
            return;

        GameObject newHitFx = Instantiate(hitPrefab, hitPosition, Quaternion.identity, _target);
        newHitFx.transform.Rotate(hitFxRotion);
        Destroy(newHitFx, 0.5f);
    }
}
