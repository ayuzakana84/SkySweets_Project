using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MenuManager : MonoBehaviour
{
    public void OnClickRetry()
    {
        SceneLoader.Instance.RetryStageScene();
    }

    public void OnClickToTitle()
    {
        SceneLoader.Instance.TitleScene();
    }

    public void OnClickToStageSelect()
    {
        SceneLoader.Instance.StageSelectScene();
    }

    public void OnClickToStage(StageData date)
    {
        SceneLoader.Instance.LoadStage(date.sceneName);
    }

    public void OnClickQuitGame()
    {
        //ボタン選択の効果音を鳴らすため、少し待ってから終了させる
        StartCoroutine(QuitProcess());
    }

    private IEnumerator QuitProcess()
    {
        //0.5秒待つ
        yield return new WaitForSecondsRealtime(0.5f);

        //アプリ終了
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
