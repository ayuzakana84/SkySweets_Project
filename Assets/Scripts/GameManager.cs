using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System; //UIのテキスト用

public class GameManager : MonoBehaviour
{
    //シングルトンパターンでGameManagerインスタンスを一つだけ存在させる
    public static GameManager Instance { get; private set; }

    public bool IsPaused { get; private set; } = false;

    private PlayerController player;
    public PlayerController Player => player; //別のスクリプトからプレイヤーを読み取れるようにする

    private PaddleController paddle;

    private UIManager currentUIManager;

    //スコア部分
    private int scoreCount = 0;
    private int combocount = 0;
    private int totalBlocksInStage = 0;
    private float fatnessPoints = 0;

    [SerializeField] int changeChubbyPoints = 2;
    [SerializeField] int changeMaxFatPoints = 5; //体型を変化させるブロック数

    [SerializeField] int maxLife = 3; //ライフ数
    private int currentLife;

    public bool isTutorialActive = false;
    public bool isBossStage = false;
    [SerializeField] float jingleTime = 4f;
    public bool isEnding = false;

    private void OnEnable()
    {
        // ライフが残っているGameManagerはシーンをまたいで生き残る
        // シーンがロードされるたびに、参照更新関数を登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void RegisterUIManager(UIManager uiManager)
    {
        currentUIManager = uiManager;

        // 登録された瞬間に、ボス戦かどうかに合わせてUIを切り替えるよう命令
        currentUIManager.SetupStageUI(isBossStage);

        // 各種UIの初期化命令
        currentUIManager.UpdateScore(scoreCount);
        currentUIManager.UpdateFatnessGauge(0, changeChubbyPoints, changeMaxFatPoints);
        if (!isBossStage)
            currentUIManager.UpdateBlockCount(totalBlocksInStage);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //ポーズから遷移した時用に正常化
        Time.timeScale = 1f;
        SetPaused(false);
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetPauseBGM(false);

        //新しいゲームが始まるたびにリセットするデータ
        if (scene.name.Contains("Stage") && !scene.name.Contains("Select"))
        {
            isBossStage = (GameObject.FindWithTag("Boss")  != null);

            isEnding = false;
            currentLife = maxLife;
            fatnessPoints = 0;
            scoreCount = 0;

            //Tag("Block")を持つゲームオブジェクトの総数を設定
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            totalBlocksInStage = blocks.Length;

            SoundManager.Instance.PlayBGM(BGMType.MainStage);

            StartCoroutine(WaitAndStartGame());
        }
        else if (scene.name == "TitleScene")
        {
            //タイトルBGMを鳴らす
            SoundManager.Instance.PlayBGM(BGMType.Title);
        }
        else if (scene.name == "GameclearScene")
        {
            SoundManager.Instance.PlayBGM(BGMType.Clear);
        }
        else if (scene.name == "GameoverScene")
        {
            SoundManager.Instance.PlayBGM(BGMType.GameOver);
        }
        else if (scene.name == "StageSelectScene")
        {
            SoundManager.Instance.PlayBGM(BGMType.StageSelect);
        }
    }

    //ゲームマネージャーのセット
    private void Awake() //Awake()はStart()よりも早く実行される
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    //新しく生成されたプレイヤーの情報を受け取る
    public void RegisterPlayer(PlayerController newPlayer)
    {
        player = newPlayer;
    }

    public void RegisterPaddle(PaddleController newPaddle)
    {
        paddle = newPaddle;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
    }

    private IEnumerator WaitAndStartGame()
    {
        //チュートリアルUIの Start() が実行されるのを1フレームだけ待つ
        yield return null;

        //チュートリアルが終わるまで待機
        yield return new WaitUntil(() => isTutorialActive == false);

        //カウントダウン開始
        StartCountdownSequence();
    }

    public void StartCountdownSequence()
    {
        player.PrepareRespawn();
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        if (currentUIManager != null)
            currentUIManager.SetCountdownActive(true);

        int count = 3;
        while (count > 0)
        {
            if (currentUIManager != null)
                currentUIManager.UpdateCountdownText(count.ToString());

            float timer = 0;
            while (timer < 1.0f)
            {
                if (player != null && player.CurrentState != PlayerController.PlayerState.WaitingForStart)
                    goto EndSequence;

                timer += Time.deltaTime;
                yield return null;
            }
            count--;
        }

        if (currentUIManager != null)
            currentUIManager.UpdateCountdownText("GO!");

        if (player != null)
            player.LaunchPlayer();

        yield return new WaitForSeconds(0.5f);

    EndSequence:
        if (currentUIManager != null)
            currentUIManager.SetCountdownActive(false);
    }

    private IEnumerator ClearSequence()
    {
        if (isEnding) yield break; //既に開始していたら何もしない
        isEnding = true;

        if (player != null)
            player.StopMovement();
        if (paddle != null)
            paddle.LockPaddle();

        SoundManager.Instance.StopBGM();

        SaveHighScore(); //ハイスコアの更新

        yield return new WaitForSeconds(1f);

        SoundManager.Instance.PlayGameClearJingle();

        yield return new WaitForSeconds(jingleTime);

        SceneLoader.Instance.GameClearScene();
    }

    private IEnumerator GameOverSequence()
    {
        if (isEnding) yield break;
        isEnding = true;

        if (player != null)
            player.StopMovement();
        if (paddle != null)
            paddle.LockPaddle();

        SoundManager.Instance.StopBGM();

        yield return new WaitForSeconds(1f);

        SoundManager.Instance.PlayGameOverJingle();

        yield return new WaitForSeconds(jingleTime);

        SceneLoader.Instance.GameOverScene();
    }

    //壊したブロックのカウント,体型の変更
    public void DestroyedBlock(int scorePoint, float addFatnessPoint)
    {
        totalBlocksInStage--;

        //changeMaxFatPoints を数値の上限として、それ以上は増えない
        fatnessPoints = Mathf.Min(fatnessPoints + addFatnessPoint, changeMaxFatPoints);
        UpdateFatnessGauge();

        if(!isBossStage)
        {
            //残り枚数の表示
            if (currentUIManager != null)
                currentUIManager.UpdateBlockCount(totalBlocksInStage);

            //スコアの加算と表示
            if (combocount < 10)
                combocount++;

            scoreCount += scorePoint * combocount;

            if (currentUIManager != null)
            {
                currentUIManager.UpdateScore(scoreCount);
                if (combocount >= 2)
                {
                    currentUIManager.ShowCombo(combocount);
                }
            }

            //ステージクリアしたかどうか
            if (totalBlocksInStage <= 0)
            {
                StartCoroutine(ClearSequence());
                return;
            }
        }
    }

    //コンボを0にする関数
    public void ResetCombo()
    {
        combocount = 0;

        if (currentUIManager != null)
            currentUIManager.HideCombo();
    }

    public void ReduceFatnessPoints(float amount)
    {
        // 減量前の体型を覚えておく
        int oldLevel = player.FatnessLevel;

        //ポイントを減らす（0未満にはならないようにClampする）
        fatnessPoints = Mathf.Max(0, fatnessPoints - amount);

        //ゲージの見た目と体型判定を更新
        UpdateFatnessGauge();

        //体型が変化したらパドルの大きさも変更する
        if (player.FatnessLevel < oldLevel)
        {
            if (paddle != null)
            {
                paddle.UpdatePaddleVisual(player.FatnessLevel);
            }
        }
    }

    private void UpdateFatnessGauge()
    {
        if (currentUIManager != null)
        {
            currentUIManager.UpdateFatnessGauge(fatnessPoints, changeChubbyPoints, changeMaxFatPoints);
        }

        CheckBodyChange();
    }

    //体型変化が起きるかどうか
    private void CheckBodyChange()
    {
        if (fatnessPoints >= changeMaxFatPoints)
        {
            if (player.FatnessLevel < 2)
                player.ChangeToMaxfat();
        }
        else if (fatnessPoints >= changeChubbyPoints)
        {
            if (player.FatnessLevel != 1)
                player.ChangeToChubby();
        }
        else
        {
            if (player.FatnessLevel > 0)
                player.ChangeToNormal();
        }
    }

    public Vector3 GetPaddlePosition()
    {
        if (paddle != null)
        {
            return paddle.transform.position;
        }

        return Vector3.zero;
    }

    //ライフを減らす
    public void LoseLife()
    {
        if (currentLife <= 0) return;

        currentLife--;
        SoundManager.Instance.PlayMissSE();

        if(currentLife <= 0)
        {
            StartCoroutine(GameOverSequence());
        }
        else
        {
            StartCountdownSequence();
        }

        if (currentLife >= 0 && currentLife < 3)
        {
            if (currentUIManager != null)
                currentUIManager.HideLifeIcon(currentLife);
        }
    }

    //ボスなどからいつでもステージクリアを呼び出せる窓口
    public void StageCleared()
    {
        StartCoroutine(BossClearSequence());
    }

    private IEnumerator BossClearSequence()
    {
        SaveHighScore(); // ハイスコアの更新

        SoundManager.Instance.PlayGameClearJingle();

        yield return new WaitForSeconds(jingleTime);

        SceneLoader.Instance.GameClearScene();
    }

    public void OnBossDefeated()
    {
        if (isEnding) return; //二重クリア防止
        isEnding = true;

        if (player != null)
            player.StopMovement();
        if (paddle != null)
            paddle.LockPaddle();

        SoundManager.Instance.StopBGM();
    }

    //リザルト画面にスコアを送るための関数
    public int GetScore()
    {
        return scoreCount;
    }

    //ハイスコア機能
    public void SaveHighScore()
    {
        //現在のステージ名を取得
        string currentSceneName = SceneManager.GetActiveScene().name;

        //保存用のキーの名前を作る
        string key = "HighScore_" + currentSceneName;

        //過去のハイスコアをロードする
        int savedHighScore = PlayerPrefs.GetInt(key, 0);

        //ハイスコアを上回っていたら更新
        if (scoreCount > savedHighScore)
        {
            PlayerPrefs.SetInt(key, scoreCount);
            PlayerPrefs.Save();
        }
    }

    public int GetHighScore(string sceneName)
    {
        return PlayerPrefs.GetInt("HighScore_" + sceneName, 0);
    }
}
