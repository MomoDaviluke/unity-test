using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField] private string newGameSceneName = "Level1Scene";
    [SerializeField] private GameObject continueButton;
    [SerializeField] private UI_FadeScreen fadeScreen;

    private void Start()
    {
        if (SaveManager.instance.HasSavedData() == false)
            continueButton.SetActive(false);
    }

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

    IEnumerator LoadExistingGame()
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainScene");
    }

    IEnumerator LoadNewGameScene()
    {
        fadeScreen.FadeOut();
        yield return new WaitForSeconds(1.5f);

        // 切换到 MainScene（覆盖当前场景）
        SceneManager.LoadScene("MainScene");

        // 等待一帧
        yield return null;

        // 叠加加载 Level1Scene
        if (!SceneManager.GetSceneByName(newGameSceneName).isLoaded)
        {
            yield return SceneManager.LoadSceneAsync(newGameSceneName, LoadSceneMode.Additive);
        }

        // 初始化玩家位置
        if (LevelManager.instance != null)
        {
            LevelManager.instance.InitializeNewGame();
        }
    }
}
