using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseCanvas;

    // Start is called before the first frame update
    void Start()
    {
        //ゲーム開始時は非表示にする
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //フェードが始まったらポーズ画面の開閉を禁止にする
        if (SceneFader.Instance != null && SceneFader.Instance.IsFading)
            return;

        //ステージ終了の処理が始まったらポーズ画面の開閉を禁止にする
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Ending)
            return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance.IsPaused)
            {
                ResumeGame();
            }
            else
            {
                //パドルの中にプレイヤーキャラが入っているならポーズは禁止
                if (GameManager.Instance.Player != null && 
                    (GameManager.Instance.Player.CurrentState == PlayerController.PlayerState.InPaddle ||
                    GameManager.Instance.Player.CurrentState == PlayerController.PlayerState.Launching))
                {
                    SoundManager.Instance.PlayBuzzerSE();

                    return;
                }

                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        GameManager.Instance.TogglePause(true);
        pauseCanvas.SetActive(true);
    }

    public void ResumeGame()
    {
        GameManager.Instance.TogglePause(false);
        pauseCanvas.SetActive(false);
    }
}
