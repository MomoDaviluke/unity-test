using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 教程管理器，单例模式，管理所有教程的显示和隐藏
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("教程UI")]
    [SerializeField] private GameObject tutorialPanel; // 教程面板
    [SerializeField] private TMPro.TextMeshProUGUI tutorialText; // 教程文本组件

    [Header("动画设置")]
    [SerializeField] private float fadeInDuration = 0.5f; // 淡入时间
    [SerializeField] private float fadeOutDuration = 0.3f; // 淡出时间

    [Header("存档设置")]
    [SerializeField] private string saveKey = "ShownTutorials"; // 存档键名

    private CanvasGroup canvasGroup; // CanvasGroup用于控制透明度
    private Coroutine currentCoroutine; // 当前运行的协程
    private HashSet<string> shownTutorials; // 已显示的教程ID集合

    private void Awake()
    {
        // 单例模式
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始化CanvasGroup
        if (tutorialPanel != null)
        {
            canvasGroup = tutorialPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = tutorialPanel.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            tutorialPanel.SetActive(false);
        }

        // 加载已显示的教程
        LoadShownTutorials();
    }

    private void Start()
    {
        // 如果没有设置教程UI，尝试自动查找
        if (tutorialPanel == null || tutorialText == null)
        {
            FindTutorialUI();
        }
    }

    // 自动查找教程UI
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

    // 显示教程
    public void ShowTutorial(string text, float duration)
    {
        if (tutorialText == null)
        {
            Debug.LogWarning("教程文本组件未设置！");
            return;
        }

        // 设置教程文本
        tutorialText.text = text;

        // 如果有正在运行的协程，先停止它
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // 激活面板并开始淡入动画
        tutorialPanel.SetActive(true);
        currentCoroutine = StartCoroutine(ShowTutorialCoroutine(duration));
    }

    // 显示教程协程
    private IEnumerator ShowTutorialCoroutine(float duration)
    {
        // 淡入
        yield return StartCoroutine(FadeIn());

        // 等待显示时间
        yield return new WaitForSeconds(duration);

        // 淡出
        yield return StartCoroutine(FadeOut());

        // 隐藏面板
        tutorialPanel.SetActive(false);
    }

    // 立即隐藏教程
    public void HideTutorial()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (tutorialPanel != null)
        {
            StartCoroutine(ImmediateHide());
        }
    }

    // 立即隐藏协程
    private IEnumerator ImmediateHide()
    {
        yield return StartCoroutine(FadeOut());
        tutorialPanel.SetActive(false);
    }

    // 淡入动画
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

    // 淡出动画
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

    // 标记教程为已显示
    public void MarkTutorialAsShown(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return;

        shownTutorials.Add(tutorialId);
        SaveShownTutorials();
    }

    // 检查教程是否已显示
    public bool HasShownTutorial(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return false;

        return shownTutorials.Contains(tutorialId);
    }

    // 重置所有教程（用于测试）
    public void ResetAllTutorials()
    {
        shownTutorials.Clear();
        SaveShownTutorials();
        Debug.Log("所有教程已重置");
    }

    // 保存已显示的教程
    private void SaveShownTutorials()
    {
        string json = JsonUtility.ToJson(new TutorialSaveData { shownTutorials = new List<string>(shownTutorials) });
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    // 加载已显示的教程
    private void LoadShownTutorials()
    {
        shownTutorials = new HashSet<string>();

        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            TutorialSaveData data = JsonUtility.FromJson<TutorialSaveData>(json);

            if (data != null && data.shownTutorials != null)
            {
                shownTutorials = new HashSet<string>(data.shownTutorials);
            }
        }
    }

    // 教程存档数据结构
    [System.Serializable]
    private class TutorialSaveData
    {
        public List<string> shownTutorials;
    }
}
