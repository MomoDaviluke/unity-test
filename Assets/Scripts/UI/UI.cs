using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// UI（UI管理器）- 管理游戏内所有UI面板的切换和显示
/// 功能：面板切换、快捷键、结束画面、存档/加载音量设置
/// </summary>
public class UI : MonoBehaviour, ISaveManager
{
    // ==================== 单例 ====================
    // 修复 B9：提供静态引用，使 PlayerDeadState 等调用方不再依赖 GameObject.Find("Canvas")。
    // 沿用项目既有弱单例惯例（同 AudioManager.instance）。UI 全项目仅挂 MainScene 一个 Canvas。
    public static UI instance;

    // ==================== 结束画面 ====================
    [Header("结束画面")]
    [SerializeField] private UI_FadeScreen fadeScreen;
    [SerializeField] private GameObject endText;
    [SerializeField] private GameObject restartButton;
    [Space]

    // ==================== UI面板 ====================
    [Header("UI面板")]
    [SerializeField] private GameObject charcaterUI;
    [SerializeField] private GameObject skillTreeUI;
    [SerializeField] private GameObject craftUI;
    [SerializeField] private GameObject optionsUI;
    [SerializeField] private GameObject inGameUI;

    // ==================== 工具提示窗口 ====================
    public UI_ItemTooltip itemTooltip;
    public UI_StatToolTip statToolTip;
    public UI_CraftWindow craftWindow;
    public UI_SkillToolTip skillToolTip;

    // ==================== 音量设置 ====================
    [SerializeField] private UI_VolumeSlider[] volumeSlider;

    // ==================== 初始化 ====================
    private void Awake()
    {
        // 修复 B9：注册单例（同 AudioManager 惯例：重复实例销毁旧的）
        if (instance != null && instance != this)
            Destroy(instance.gameObject);
        else
            instance = this;

        SwitchTo(skillTreeUI);
        fadeScreen.gameObject.SetActive(true);
    }

    void Start()
    {
        SwitchTo(inGameUI);
        itemTooltip.gameObject.SetActive(false);
        statToolTip.gameObject.SetActive(false);
    }

    // ==================== 快捷键输入 ====================
    // Z-角色面板 X-合成面板 C-技能树 V-设置
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            SwitchWithKeyTo(charcaterUI);
        if (Input.GetKeyDown(KeyCode.X))
            SwitchWithKeyTo(craftUI);
        if (Input.GetKeyDown(KeyCode.C))
            SwitchWithKeyTo(skillTreeUI);
        if (Input.GetKeyDown(KeyCode.V))
            SwitchWithKeyTo(optionsUI);
    }

    // ==================== 面板切换 ====================
    // 切换到指定面板，关闭其他所有面板，控制游戏暂停
    public void SwitchTo(GameObject _menu)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            bool fadeScreen = transform.GetChild(i).GetComponent<UI_FadeScreen>() != null;
            if (fadeScreen == false)
                transform.GetChild(i).gameObject.SetActive(false);
        }

        if (_menu != null)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySFX(7, null);
            _menu.SetActive(true);
        }

        if (GameManager.instance != null)
        {
            if (_menu == inGameUI)
                GameManager.instance.PauseGame(false);
            else
                GameManager.instance.PauseGame(true);
        }
    }

    // 按键切换面板（如果已打开则关闭）
    public void SwitchWithKeyTo(GameObject _menu)
    {
        if (_menu != null && _menu.activeSelf)
        {
            _menu.SetActive(false);
            checkForInGameUI();
            return;
        }
        SwitchTo(_menu);
    }

    // 检查是否有其他面板打开，没有则切回游戏UI
    private void checkForInGameUI()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf && transform.GetChild(i).GetComponent<UI_FadeScreen>() == null)
                return;
            SwitchTo(inGameUI);
        }
    }

    // ==================== 结束画面 ====================
    // 显示结束画面并播放淡入动画
    public void SwitchOnEndScreen()
    {
        fadeScreen.FadeOut();
        StartCoroutine(EndScreenCorutione());
    }

    IEnumerator EndScreenCorutione()
    {
        yield return new WaitForSeconds(1f);
        endText.SetActive(true);
        yield return new WaitForSeconds(1f);
        restartButton.SetActive(true);
    }

    public void RestartGameButton() => GameManager.instance.RestartScene();

    // ==================== 存档/加载 ====================
    public void LoadData(GameData _data)
    {
        foreach (KeyValuePair<string, float> pair in _data.volumeSettings)
        {
            foreach (UI_VolumeSlider item in volumeSlider)
            {
                if (item.parametr == pair.Key)
                    item.LoadSlider(pair.Value);
            }
        }
    }

    public void SaveData(ref GameData _data)
    {
        _data.volumeSettings.Clear();
        foreach (UI_VolumeSlider item in volumeSlider)
            _data.volumeSettings.Add(item.parametr, item.slider.value);
    }
}
