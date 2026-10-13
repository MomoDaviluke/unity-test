using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TutorialManager（教程管理器）- 管理教程面板的显示和存档
/// 设计思路：使用PlayerPrefs记录已显示的教程，避免重复显示
/// 使用场景：新手引导、操作提示、机制说明
/// </summary>
public class TutorialManager : MonoBehaviour
{
    // ==================== 单例 ====================
    public static TutorialManager instance;

    // ==================== 教程UI ====================
    [Header("教程UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TMPro.TextMeshProUGUI tutorialText;

    // ==================== 动画设置 ====================
    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    // ==================== 存档设置 ====================
    [Header("存档设置")]
    [SerializeField] private string saveKey = "ShownTutorials";

    // ==================== 内部状态 ====================
    private CanvasGroup canvasGroup;
    private Coroutine currentCoroutine;
    private HashSet<string> shownTutorials;

    // ==================== 单例初始化 ====================
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (tutorialPanel != null)
        {
            canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            tutorialPanel.SetActive(false);
        }

        LoadShownTutorials();
    }

    private void Start()
    {
        if (tutorialPanel == null || tutorialText == null)
            FindTutorialUI();
    }

    // ==================== UI查找 ====================
    private void FindTutorialUI()
    {
        GameObject[] uiObjects = GameObject.FindGameObjectsWithTag("UI");

        foreach (var obj in uiObjects)
        {
            TutorialUI tutorialUI = obj.GetComponentInChildren<TutorialUI>();
            if (tutorialUI != null)
            {
                tutorialPanel = tutorialUI.gameObject;
                tutorialText = tutorialUI.GetTutorialText();

                if (tutorialPanel != null)
                {
                    canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                        canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();

                    canvasGroup.alpha = 0f;
                    tutorialPanel.SetActive(false);
                }

                Debug.Log("自动找到教程UI");
                break;
            }
        }
    }

    // ==================== 教程显示 ====================
    public void ShowTutorial(string text, float duration)
    {
        if (tutorialText == null)
        {
            Debug.LogWarning("教程文本组件未设置！");
            return;
        }

        tutorialText.text = text;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        tutorialPanel.SetActive(true);
        currentCoroutine = StartCoroutine(ShowTutorialCoroutine(duration));
    }

    // ==================== 教程协程 ====================
    // 淡入 -> 等待 -> 淡出
    private IEnumerator ShowTutorialCoroutine(float duration)
    {
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(FadeOut());
        tutorialPanel.SetActive(false);
    }

    public void HideTutorial()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (tutorialPanel != null)
            StartCoroutine(ImmediateHide());
    }

    private IEnumerator ImmediateHide()
    {
        yield return StartCoroutine(FadeOut());
        tutorialPanel.SetActive(false);
    }

    // ==================== 淡入淡出动画 ====================
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 1f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    // ==================== 教程记录管理 ====================
    public void MarkTutorialAsShown(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return;

        shownTutorials.Add(tutorialId);
        SaveShownTutorials();
    }

    public bool HasShownTutorial(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return false;

        return shownTutorials.Contains(tutorialId);
    }

    public void ResetAllTutorials()
    {
        shownTutorials.Clear();
        SaveShownTutorials();
        Debug.Log("所有教程已重置");
    }

    // ==================== PlayerPrefs存档 ====================
    private void SaveShownTutorials()
    {
        string json = JsonUtility.ToJson(new TutorialSaveData { shownTutorials = new List<string>(shownTutorials) });
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadShownTutorials()
    {
        shownTutorials = new HashSet<string>();

        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            TutorialSaveData data = JsonUtility.FromJson<TutorialSaveData>(json);

            if (data != null && data.shownTutorials != null)
                shownTutorials = new HashSet<string>(data.shownTutorials);
        }
    }

    // ==================== 存档数据结构 ====================
    [System.Serializable]
    private class TutorialSaveData
    {
        public List<string> shownTutorials;
    }
}
