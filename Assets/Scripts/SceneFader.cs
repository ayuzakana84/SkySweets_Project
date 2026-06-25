using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }
    public bool IsFading { get; private set; } = false;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

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

    //SceneLoaderから呼ばれる関数
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        IsFading = true;

        //クリックをブロックする
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        //BGMのフェードアウトも開始
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.FadeOutBGM(fadeDuration);
        }

        //フェードアウト
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }

        //切り替えが早くて目が疲れないようにするための余韻
        yield return new WaitForSecondsRealtime(0.8f);

        //シーンの読み込み
        yield return SceneManager.LoadSceneAsync(sceneName);

        //切り替えが早くて目が疲れないようにするための余韻
        yield return new WaitForSecondsRealtime(0.2f);

        //フェードイン
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1 - (timer / fadeDuration);
            yield return null;
        }

        //クリックできるようにする
        canvasGroup.alpha = 0; // 念のため完全に0にする
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        IsFading = false;
    }
}
