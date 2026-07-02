using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TutorialUI : MonoBehaviour
{
    public event Action OnTutorialFinished;

    private void Start()
    {
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
