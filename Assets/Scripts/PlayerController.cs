using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    public enum PlayerState
    {
        WaitingForStart, //カウントダウン中の待機状態
        Playing,         //通常のプレイ中（跳ねたりぶつかったりする状態）
        InPaddle,        //パドルの中に入った状態
        Launching        //パドルから発射される状態
    }

    //読み取り専用にして書き換えられないようにする
    public PlayerState CurrentState { get; private set; } = PlayerState.WaitingForStart;

    [SerializeField] GameObject normalBody;
    [SerializeField] GameObject chubbyBody;
    [SerializeField] GameObject maxfatBody;

    [SerializeField] int attackForce = 1;
    [SerializeField] float bounceForce = 13f;

    [SerializeField] float fallBoundary = -15f; //ミスになる高さ

    [SerializeField] float fatnessMultiplier = 5f; // 肥満度レベル1ごとに力を増やす倍率
    private int fatnessLevel = 0; // 肥満度レベル

    [SerializeField] ParticleSystem smokeEffect; //体型変化時に出すエフェクト

    [SerializeField] GameObject normalAura;
    [SerializeField] GameObject chubbyAura;
    [SerializeField] GameObject maxfatAura;

    private PaddleController paddle;

    public int FatnessLevel => fatnessLevel; //ゲームマネージャーから見れるようにプロパティ化

    //アニメーター
    private Animator animNormal;
    private Animator animChubby;
    private Animator animFat;

    private Animator currentAnim; //現在の体型に合わせたアニメーターを入れる

    public event System.Action<int> OnFatnessLevelChanged;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPlayer(this); //プレイヤーの情報を更新する
    }

    public void PrepareRespawn()
    {
        CurrentState = PlayerState.WaitingForStart;

        UpdateAuraState();

        if (rb != null)
        {
            rb.simulated = false;
            rb.velocity = Vector2.zero;
        }

        //BaseLayerの数値を取得する
        int layerIndex = currentAnim.GetLayerIndex("Base Layer");

        currentAnim.SetFloat("VelocityY", 0f);
        currentAnim.Play("Idle", layerIndex, 0f);
    }

    //プレイヤーの物理演算を開始する
    public void LaunchPlayer()
    {
        if (CurrentState != PlayerState.WaitingForStart) return;

        CurrentState = PlayerState.Playing;

        UpdateAuraState();

        rb.simulated = true;
        rb.velocity = Vector3.zero;
    }

    void Start()
    {
        //アニメーター
        animNormal = normalBody.GetComponent<Animator>();
        animChubby = chubbyBody.GetComponent<Animator>();
        animFat = maxfatBody.GetComponent<Animator>();

        CurrentState = PlayerState.WaitingForStart;
        UpdateBodyState();
    }

    public void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    void Update()
    {
        //ポーズ中は処理をしない
        if (GameManager.Instance.IsPaused) return;

        switch (CurrentState)
        {
            case PlayerState.WaitingForStart:
                UpdateWaitingState();
                break;

            case PlayerState.InPaddle:
                UpdateInPaddleState();
                break;

            //発射モーション中は移動処理などはさせない
            case PlayerState.Launching:
                break;

            case PlayerState.Playing:
                UpdatePlayingState();
                break;
        }
    }

    //スタート待機中の処理
    private void UpdateWaitingState()
    {
        float paddleX = GameManager.Instance.GetPaddlePosition().x;
        transform.position = new Vector3(paddleX, -3, 0);

        if (Input.GetMouseButtonDown(0))
        {
            LaunchPlayer();
        }
    }

    //パドルの中に入った時の処理
    private void UpdateInPaddleState()
    {
        //ダイエット処理
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReduceFatnessPoints(Time.deltaTime * 2.0f);
        }

        //左クリックが離されたらプレイヤーを発射
        if (paddle != null && !paddle.isCharging)
        {
            CurrentState = PlayerState.Launching; //状態を発射中に切り替える
            paddle.PlayShotAnimation(); //パドルに発射するアニメーションを再生してもらう
        }
    }

    private void UpdatePlayingState()
    {
        //肥満度レベルに応じて重力を調整
        rb.gravityScale = 1.0f + (fatnessLevel * 0.5f);

        //ライフを減らす処理
        if (transform.position.y < fallBoundary)
        {
            GameManager.Instance.LoseLife();
        }

        //アニメーター
        if (currentAnim != null) //念のためエラー防止
        {
            //自分の縦方向のスピード（velocity.y）を、アニメーターの「VelocityY」に渡す
            currentAnim.SetFloat("VelocityY", rb.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            GameManager.Instance.ResetCombo();

            paddle = collision.gameObject.GetComponent<PaddleController>();
            
            if(paddle != null && paddle.isCharging) //パドルがチャージ中の処理
            {
                CurrentState = PlayerState.InPaddle;
                rb.simulated = false; //物理演算を止める

                //パドルの見た目を変更
                paddle.SetPlayer(true);

                //見た目を非表示にする
                normalBody.SetActive(false);
                chubbyBody.SetActive(false);
                maxfatBody.SetActive(false);
            }
            else //通常の処理
            {
                // 肥満度レベルに応じて跳ね返る力を計算
                float currentBounceForce = bounceForce + (fatnessLevel * fatnessMultiplier);

                // プレイヤーとパドルの衝突位置を計算
                float paddleWidth = collision.collider.bounds.size.x; //パドルの大きさ
                float hitPoint = collision.contacts[0].point.x - collision.transform.position.x; //当たった位置がパドルの中心からどの程度離れているか
                float normalizedHitPoint = hitPoint / (paddleWidth / 2f); //パドルの中心を0、端を-1〜1の範囲に正規化

                // 跳ね返る方向を計算
                Vector2 bounceDirection = new Vector2(normalizedHitPoint, 1f).normalized;

                // プレイヤーを跳ね返らせる
                rb.velocity = bounceDirection * currentBounceForce;

                //体型に合わせた音を鳴らす
                SoundManager.Instance.PlayJumpSE(fatnessLevel);
            }
        }
        
        if (collision.gameObject.CompareTag("Block"))
        {
            AttackBlock(collision.gameObject); //ブロックにダメージを与える処理
        }

        if (collision.gameObject.CompareTag("Boss"))
        {
            AttackBoss(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Block"))
        {
            AttackBlock(other.gameObject);
        }
    }

    //パドルのアニメーションイベントに合わせて発射
    public void ActualLaunch()
    {
        if (CurrentState != PlayerState.Launching) return;

        CurrentState = PlayerState.Playing;

        //パドルの中心にキャラクターを移動
        if (paddle != null)
        {
            //パドルとの再衝突を防ぐため、パドルの少し上から発射
            transform.position = paddle.transform.position + Vector3.up * 1.5f;
        }

        rb.simulated = true;
        rb.velocity = Vector2.zero;

        //どの体型でも「肥満体型(Level 2)のジャンプ」と同じ高さにする計算
        // 基準となる肥満体型の重力は 2.0
        float targetGravity = 2.0f;

        // 今の体型の重力を取得
        float currentGravity = 1.0f + (fatnessLevel * 0.5f);

        // 肥満体型の時のジャンプ力（9）をベースに、重力差を補正する
        float maxFatPower = bounceForce + (2 * fatnessMultiplier); // 9.0f

        // 重力の比率のルートを掛けることで、物理的に同じ高さまで届くようになる
        float adjustedForce = maxFatPower * Mathf.Sqrt(currentGravity / targetGravity);
        rb.velocity = Vector2.up * adjustedForce;

        //見た目を表示
        UpdateBodyState();
    }

    private void AttackBlock(GameObject target)
    {
        // 衝突した相手がIDamageableインターフェースを持っているかチェック
        IDamageable damageable = target.GetComponent<IDamageable>();


        if (damageable != null)
        {
            damageable.AddDamage(attackForce); //持っていたらダメージを与える

            //食べるアニメーションを再生する
            if (currentAnim != null)
            {
                //FaceLayerの数値を取得する
                int layerIndex = currentAnim.GetLayerIndex("Face Layer");

                currentAnim.Play("Eat", layerIndex, 0f);
            }
        }
    }

    private void AttackBoss(GameObject target)
    {
        IDamageable damageable = target.GetComponentInParent<IDamageable>();

        if (damageable != null)
            damageable.AddDamage(attackForce);
    }

    //ボスから呼ばれるプレイヤーをはじき返す関数
    public void Knockback(Vector2 direction, float forceMultiplier)
    {
        //パドルの中にいる時や、リスポーン待機中は吹き飛ばないようにする
        if (CurrentState != PlayerState.Playing) return;

        rb.velocity = Vector2.zero;

        //指定された方向 × 基本の跳ね返り力 × 倍率 で吹き飛ばす！
        rb.velocity = direction * (bounceForce * forceMultiplier);
    }

    public void ChangeToNormal()
    {
        if (smokeEffect != null && CurrentState != PlayerState.InPaddle)
        {
            smokeEffect.Play();
        }

        fatnessLevel = 0;
        attackForce = 1;
        UpdateBodyState();

        OnFatnessLevelChanged?.Invoke(fatnessLevel);
    }

    public void ChangeToChubby()
    {
        if (smokeEffect != null && CurrentState != PlayerState.InPaddle)
        {
            smokeEffect.Play();
        }

        fatnessLevel = 1;
        attackForce = 2;
        UpdateBodyState();

        OnFatnessLevelChanged?.Invoke(fatnessLevel);
    }

    public void ChangeToMaxfat()
    {
        if (smokeEffect != null && CurrentState != PlayerState.InPaddle)
        {
            smokeEffect.Play();
        }

        fatnessLevel = 2;
        attackForce = 3;
        UpdateBodyState();

        OnFatnessLevelChanged?.Invoke(fatnessLevel);
    }

    private void UpdateAuraState()
    {
        //念のため全て非表示に
        if (normalAura != null) normalAura.SetActive(false);
        if (chubbyAura != null) chubbyAura.SetActive(false);
        if (maxfatAura != null) maxfatAura.SetActive(false);

        //待機中じゃなければここで処理を終わる
        if (CurrentState != PlayerState.WaitingForStart) return;

        // 待機中なら、現在の体型に合わせて1つだけ表示
        if (fatnessLevel == 0 && normalAura != null) normalAura.SetActive(true);
        else if (fatnessLevel == 1 && chubbyAura != null) chubbyAura.SetActive(true);
        else if (fatnessLevel == 2 && maxfatAura != null) maxfatAura.SetActive(true);
    }

    //体型の見た目とcurrentAnimの参照を一括更新する
    private void UpdateBodyState()
    {
        //パドルの中にいる間は、見た目は表示させない
        if (CurrentState == PlayerState.InPaddle) return;

        normalBody.SetActive(fatnessLevel == 0);
        chubbyBody.SetActive(fatnessLevel == 1);
        maxfatBody.SetActive(fatnessLevel == 2);

        UpdateAuraState();

        if (fatnessLevel == 0)
            currentAnim = animNormal;
        else if (fatnessLevel == 1)
            currentAnim = animChubby;
        else if (fatnessLevel == 2)
            currentAnim = animFat;
    }
}
