using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("通常ステージ用UI")]
    [SerializeField] GameObject uiFrame_R;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject comboUIRoot;
    [SerializeField] TextMeshProUGUI comboText;
    [SerializeField] ComboUIAnimation comboAnim; // アニメーション用スクリプトも直接セット
    [SerializeField] TextMeshProUGUI blockCountText;

    [Header("ボス戦用UI")]
    [SerializeField] GameObject bossHPGaugeRoot;

    [Header("プレイヤー情報UI")]
    [SerializeField] GameObject[] lifeIcons; // 3つのアイコンをインスペクターで入れる
    [SerializeField] FatnessGauge fatnessGauge; // FatnessGaugeもここで管理

    [Header("カウントダウンUI")]
    [SerializeField] GameObject countdownRoot;
    [SerializeField] TextMeshProUGUI countdownText;

    // Start is called before the first frame update
    void Start()
    {
        //ゲームマネージャーのイベントを登録
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageSetup += SetupStageUI;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnComboChanged += ShowCombo;
            GameManager.Instance.OnComboReset += HideCombo;
            GameManager.Instance.OnBlockCountChanged += UpdateBlockCount;
            GameManager.Instance.OnFatnessChanged += UpdateFatnessGauge;
            GameManager.Instance.OnLifeLostEvent += HideLifeIcon;
            GameManager.Instance.OnCountdownActiveChanged += SetCountdownActive;
            GameManager.Instance.OnCountdownTextChanged += UpdateCountdownText;

            //ステージ開始時の状態をセット
            GameManager.Instance.RequestInitialUIUpdate();
        }

        // 初期状態は非表示にしておくもの
        if (comboUIRoot != null) comboUIRoot.SetActive(false);
        if (countdownRoot != null) countdownRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageSetup -= SetupStageUI;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnComboChanged -= ShowCombo;
            GameManager.Instance.OnComboReset -= HideCombo;
            GameManager.Instance.OnBlockCountChanged -= UpdateBlockCount;
            GameManager.Instance.OnFatnessChanged -= UpdateFatnessGauge;
            GameManager.Instance.OnLifeLostEvent -= HideLifeIcon;
            GameManager.Instance.OnCountdownActiveChanged -= SetCountdownActive;
            GameManager.Instance.OnCountdownTextChanged -= UpdateCountdownText;
        }
    }

    //ボスステージかどうかでUIを切り替える
    public void SetupStageUI(bool isBossStage)
    {
        if (isBossStage)
        {
            if (uiFrame_R != null) uiFrame_R.SetActive(false);
            if (bossHPGaugeRoot != null) bossHPGaugeRoot.SetActive(true);

            if (blockCountText != null) blockCountText.text = "x --";
        }
        else
        {
            if (uiFrame_R != null) uiFrame_R.SetActive(true);
            if (bossHPGaugeRoot != null) bossHPGaugeRoot.SetActive(false);
        }
    }

    //スコアの更新
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    //コンボの表示・更新
    public void ShowCombo(int comboCount)
    {
        if (comboUIRoot != null) comboUIRoot.SetActive(true);

        if (comboText != null)
        {
            comboText.text = (comboCount == 10) ? "MAX" : comboCount.ToString();
        }

        if (comboAnim != null) comboAnim.PlayJump();
    }

    //コンボを隠す
    public void HideCombo()
    {
        if (comboUIRoot != null) comboUIRoot.SetActive(false);
    }

    //ブロックの残り数を更新
    public void UpdateBlockCount(int count)
    {
        if (blockCountText != null)
            blockCountText.text = "x " + count.ToString();
    }

    //カウントダウンの表示切替と文字更新
    public void SetCountdownActive(bool isActive)
    {
        if (countdownRoot != null) countdownRoot.SetActive(isActive);
    }

    public void UpdateCountdownText(string text)
    {
        if (countdownText != null) countdownText.text = text;
    }

    //ライフアイコンを消す
    public void HideLifeIcon(int lifeIndex)
    {
        if (lifeIndex >= 0 && lifeIndex < lifeIcons.Length)
        {
            if (lifeIcons[lifeIndex] != null) lifeIcons[lifeIndex].SetActive(false);
        }
    }

    //肥満度ゲージの更新
    public void UpdateFatnessGauge(float current, int chubbyParams, int maxParams)
    {
        if (fatnessGauge != null)
        {
            fatnessGauge.RefreshGauge(current, chubbyParams, maxParams);
        }
    }
}
