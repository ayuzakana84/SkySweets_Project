using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    //シングルトンパターンでGameManagerインスタンスを一つだけ存在させる
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Loading,
        Tutorial,
        Countdown,
        Playing,
        Ending
    }
    public GameState CurrentState { get; private set; } = GameState.Loading;

    public bool IsPaused { get; private set; } = false;
    public bool isBossStage = false;

    [Header("スコア・システム関連")]
    [SerializeField] float jingleTime = 4f;
    [SerializeField] int changeChubbyPoints = 2;
    [SerializeField] int changeMaxFatPoints = 5; //体型を変化させるブロック数
    [SerializeField] int maxLife = 3; //ライフ数

    public PlayerController Player { get; private set; }
    private PaddleController paddle;
    private UIManager currentUIManager;
    private TutorialUI currentTutorialUI;

    //スコア部分
    private int scoreCount = 0;
    private int combocount = 0;
    private int totalBlocksInStage = 0;
    private float fatnessPoints = 0;
    private int currentLife;

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

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
    }

    public void TogglePause(bool isPause)
    {
        if (CurrentState == GameState.Loading || CurrentState == GameState.Ending)
            return;

        IsPaused = isPause;

        if (IsPaused) Time.timeScale = 0f;
        else Time.timeScale = 1f;

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetPauseBGM(IsPaused);
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

    //新しく生成されたプレイヤーの情報を受け取る
    public void RegisterPlayer(PlayerController newPlayer)
    {
        Player = newPlayer;
    }

    public void RegisterPaddle(PaddleController newPaddle)
    {
        paddle = newPaddle;
    }

    public void RegisterTutorial(TutorialUI tutorialUI)
    {
        ChangeState(GameState.Tutorial);
        currentTutorialUI = tutorialUI;
        tutorialUI.OnTutorialFinished += OnTutorialFinishedHandler;
    }
    private void OnTutorialFinishedHandler()
    {
        if (currentTutorialUI != null)
        {
            currentTutorialUI.OnTutorialFinished -= OnTutorialFinishedHandler;
            currentTutorialUI = null;
        }

        StartCountdownSequence();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //ポーズから遷移した時用に正常化
        Time.timeScale = 1f;
        IsPaused = false;
        ChangeState(GameState.Loading);

        if (SoundManager.Instance != null)
            SoundManager.Instance.SetPauseBGM(false);

        //新しいゲームが始まるたびにリセットするデータ
        if (scene.name.Contains("Stage") && !scene.name.Contains("Select"))
        {
            isBossStage = (GameObject.FindWithTag("Boss")  != null);

            currentLife = maxLife;
            fatnessPoints = 0;
            scoreCount = 0;
            combocount = 0;

            //Tag("Block")を持つゲームオブジェクトの総数を設定
            GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
            totalBlocksInStage = blocks.Length;

            SoundManager.Instance.PlayBGM(BGMType.MainStage);

            StartGameFlow();
        }
        else if (scene.name == "TitleScene")
            SoundManager.Instance.PlayBGM(BGMType.Title);
        else if (scene.name == "GameclearScene")
            SoundManager.Instance.PlayBGM(BGMType.Clear);
        else if (scene.name == "GameoverScene")
            SoundManager.Instance.PlayBGM(BGMType.GameOver);
        else if (scene.name == "StageSelectScene")
            SoundManager.Instance.PlayBGM(BGMType.StageSelect);
    }

    private IEnumerator StartGameFlow()
    {
        //RegisterTutorialを待つために１フレーム待機
        yield return null;

        //チュートリアルが無かったらそのままゲーム開始
        if (CurrentState == GameState.Loading)
        {
            StartCountdownSequence();
        }
    }

    public void StartCountdownSequence()
    {
        ChangeState(GameState.Countdown);
        Player.PrepareRespawn();
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
                //カウントダウンがスキップされた場合
                if (Player != null && Player.CurrentState != PlayerController.PlayerState.WaitingForStart)
                {
                    if (currentUIManager != null)
                        currentUIManager.SetCountdownActive(false);
                    ChangeState(GameState.Playing);

                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
            count--;
        }

        if (currentUIManager != null)
            currentUIManager.UpdateCountdownText("GO!");

        if (Player != null)
            Player.LaunchPlayer();

        yield return new WaitForSeconds(0.5f);

        if (currentUIManager != null)
            currentUIManager.SetCountdownActive(false);
        ChangeState(GameState.Playing);
    }

    //ゲームオーバーなどでプレイヤーやパドルを止める処理
    private void StopGameEntities()
    {
        if (Player != null) Player.StopMovement();
        if (paddle != null) paddle.LockPaddle();
        SoundManager.Instance.StopBGM();
    }

    private IEnumerator ClearSequence()
    {
        if (CurrentState == GameState.Ending) yield break; //既に開始していたら何もしない
        ChangeState(GameState.Ending);

        StopGameEntities();
        SaveHighScore(); //ハイスコアの更新

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlayGameClearJingle();
        yield return new WaitForSeconds(jingleTime);

        SceneLoader.Instance.GameClearScene();
    }

    private IEnumerator GameOverSequence()
    {
        if (CurrentState == GameState.Ending) yield break; //既に開始していたら何もしない
        ChangeState(GameState.Ending);

        StopGameEntities();

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
                    currentUIManager.ShowCombo(combocount);
            }

            //ステージクリアしたかどうか
            if (totalBlocksInStage <= 0)
            {
                StartCoroutine(ClearSequence());
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
        int oldLevel = Player.FatnessLevel;

        //ポイントを減らす（0未満にはならないようにClampする）
        fatnessPoints = Mathf.Max(0, fatnessPoints - amount);

        //ゲージの見た目と体型判定を更新
        UpdateFatnessGauge();

        //体型が変化したらパドルの大きさも変更する
        if (Player.FatnessLevel < oldLevel)
        {
            if (paddle != null)
            {
                paddle.UpdatePaddleVisual(Player.FatnessLevel);
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
            if (Player.FatnessLevel < 2)
                Player.ChangeFatnessLevel(2);
        }
        else if (fatnessPoints >= changeChubbyPoints)
        {
            if (Player.FatnessLevel != 1)
                Player.ChangeFatnessLevel(1);
        }
        else
        {
            if (Player.FatnessLevel > 0)
                Player.ChangeFatnessLevel(0);
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
        if (CurrentState == GameState.Ending) return; //既に開始していたら何もしない
        ChangeState(GameState.Ending);

        StopGameEntities();
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
