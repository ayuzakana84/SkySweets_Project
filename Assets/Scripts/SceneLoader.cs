using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private string retrySceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //リトライの処理用にシーン名を保存
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Stage")) //シーン名にStageが入っていたら
        {
            retrySceneName = scene.name; //シーンの名前を保存
        }
    }

    //タイトルシーンへの切り替え
    public void TitleScene()
    {
        SceneFader.Instance.FadeToScene("TitleScene");
    }

    //今までいたステージへの切り替え
    public void RetryStageScene()
    {
        //retrySceneNameがNULLや記入漏れじゃないかチェック
        if (!string.IsNullOrEmpty(retrySceneName))
            SceneFader.Instance.FadeToScene(retrySceneName);
    }

    //ゲームオーバーシーンへの切り替え
    public void GameOverScene()
    {
        SceneFader.Instance.FadeToScene("GameoverScene"); //後々専用演出に変えたい
    }

    //ゲームクリアシーンへの切り替え
    public void GameClearScene()
    {
        SceneFader.Instance.FadeToScene("GameclearScene"); //後々専用演出に変えたい
    }

    //ステージセレクトシーンへの切り替え
    public void StageSelectScene()
    {
        SceneFader.Instance.FadeToScene("StageSelectScene");
    }

    //各ステージへの切り替え
    public void LoadStage(string sceneName)
    {
        SceneFader.Instance.FadeToScene(sceneName);
    }
}
