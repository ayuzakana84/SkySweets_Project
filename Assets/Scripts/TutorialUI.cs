using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class TutorialUI : MonoBehaviour
{
    public event Action OnTutorialFinished;

    private void Start()
    {
        //自分がいるシーンの名前を取得し、チュートリアルスキップが設定されてるか確認
        string currentScene = SceneManager.GetActiveScene().name;
        bool isSkipped = (PlayerPrefs.GetInt("SkipTutorial_" + currentScene, 0) == 1);

        //スキップ設定なら非表示にして、それ以上処理を行わない
        if (isSkipped)
        {
            gameObject.SetActive(false);
            return;
        }

        //チュートリアルがあることをゲームマネージャーに伝える
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterTutorial(this);
    }

    private void Update()
    {
        //ポーズ中ならチュートリアル画像を消さずに返す
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        if (Input.GetMouseButtonDown(0))
            CloseTutorial();
    }

    private void CloseTutorial()
    {
        gameObject.SetActive(false);
        OnTutorialFinished?.Invoke();
    }
}
