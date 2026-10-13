using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI_MainMenu（主菜单UI）- 处理主菜单的继续游戏、新游戏、退出
/// 功能：检测存档、场景加载、淡出效果
/// </summary>
public class UI_MainMenu : MonoBehaviour
{
    // ==================== 菜单配置 ====================
    [SerializeField] private string newGameSceneName = "Level1Scene";
    [SerializeField] private GameObject continueButton;
    [SerializeField] private UI_FadeScreen fadeScreen;

    // ==================== 初始化 ====================
    private void Start()
    {
        if (SaveManager.instance.HasSavedData() == false)
            continueButton.SetActive(false);
    }

    // ==================== 按钮功能 ====================
    public void ContinueGame()
    {
        StartCoroutine(LoadExistingGame());
    }

    public void NewGame()
    {
        SaveManager.instance.DeleteSaveData();
        StartCoroutine(LoadNewGameScene());
    }

    public void ExitGame()
    {
        Debug.Log("退出游戏");
    }

    // ==================== 场景加载 ====================
    // 加载已有存档的游戏
    IEnumerator LoadExistingGame()
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainScene");
    }

    // 新游戏：加载主场景并叠加加载关卡场景
    IEnumerator LoadNewGameScene()
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene("MainScene");
        yield return null;

        if (!SceneManager.GetSceneByName(newGameSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(newGameSceneName, LoadSceneMode.Additive);
        }

        if (LevelManager.instance != null)
        {
            LevelManager.instance.InitializeNewGame();
        }
    }
}
