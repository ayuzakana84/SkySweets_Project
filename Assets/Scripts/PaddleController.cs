using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PaddleController : MonoBehaviour
{

    [System.Serializable]
    public struct PaddleSizeData
    {
        public string sizeLabel;    //"L", "M", "S"
        public float colliderWidth; //9.6f, 6.2f, 3.0f
    }

    public enum PaddleState
    {
        Normal,            //通常の移動状態
        WaitingForRelease, //カウントダウン中の状態
        Charging,          //チャージ状態
        Locked             //クリア・ゲームオーバー時の停止状態
    }

    [Header("パドル設定")]
    [SerializeField] private BoxCollider2D paddleCollider;
    [SerializeField] private SpriteResolver bodyResolver;
    [SerializeField] private float moveLimitX = 10f;
    [SerializeField] private PaddleSizeData[] sizeDatas;

    [Header("エフェクト設定")]
    [SerializeField] private GameObject sEndRoot;
    [SerializeField] private GameObject mEndRoot;
    [SerializeField] private ParticleSystem chargeEffect;

    public bool isCharging => currentState == PaddleState.Charging;

    private PaddleState currentState = PaddleState.Normal;
    private int lastFatnessLevel = -1; //体型が変化したかチェックするための変数

    //キャッシュ用変数（あらかじめ宣言しておいて、毎フレームの処理を少し軽くする）
    private Animator animator;
    private GameManager gm;
    private ParticleSystem[] sEndParticles;
    private ParticleSystem[] mEndParticles;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        gm = GameManager.Instance; //Startで一度だけキャッシュする

        if (gm != null)
            gm.RegisterPaddle(this);
    }

    public void LockPaddle()
    {
        ChangeState(PaddleState.Locked);
    }

    void Update()
    {
        //ポーズ画面、ゲームクリア、ゲームオーバー演出、チュートリアル中は動かないように
        if (gm.IsPaused || currentState == PaddleState.Locked || gm.CurrentState == GameManager.GameState.Tutorial)
        {
            //チャージ状態なら、強制的にオフにして音とエフェクトを止める
            if (currentState == PaddleState.Charging)
                ChangeState(PaddleState.Normal);
            return;
        }

        UpdateMovement();
        UpdateChargeLogic();

        //チャージ状態による表情の更新
        if (animator != null)
            animator.SetBool("isCharging", isCharging);
    }

    private void UpdateMovement()
    {
        //マウスのスクリーン座標を取得
        Vector2 mousePosition = Input.mousePosition;

        //スクリーン座標をワールド座標に変換
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //パドルの新しい位置を計算
        float targetX = worldPosition.x;

        //プレイヤーがパドルの中にいたり、パドルに追従しているなら壁の外に出ないよう移動幅に制限をかける
        if (gm.Player != null && gm.Player.CurrentState != PlayerController.PlayerState.Playing)
        {
            targetX = Mathf.Clamp(worldPosition.x, -moveLimitX, moveLimitX);
        }

        transform.position = new Vector2(targetX, transform.position.y);
    }

    private void UpdateChargeLogic()
    {
        //カウントダウン中の処理
        if (gm.Player != null && gm.Player.CurrentState == PlayerController.PlayerState.WaitingForStart)
        {
            if (currentState == PaddleState.Charging)
                ChangeState(PaddleState.Normal);

            //カウントダウンをスキップするための左クリックでチャージ状態にならないようにする
            if (Input.GetMouseButton(0))
                ChangeState(PaddleState.WaitingForRelease);

            return;
        }

        //クリックを離したら待機状態を解除
        if (currentState == PaddleState.WaitingForRelease && !Input.GetMouseButton(0))
            ChangeState(PaddleState.Normal);

        //チャージ状態の切り替え
        if (Input.GetMouseButton(0) && currentState != PaddleState.WaitingForRelease)
        {
            if (currentState != PaddleState.Charging)
                ChangeState(PaddleState.Charging);
        }
        else
        {
            if (currentState == PaddleState.Charging)
                ChangeState(PaddleState.Normal);
        }
    }

    private void ChangeState(PaddleState newState)
    {
        //同じステートなら返す
        if (currentState == newState) return;

        //古い状態がチャージ状態なら演出を止める
        if (currentState == PaddleState.Charging)
        {
            SoundManager.Instance.StopChargeSE();
            if (chargeEffect != null) chargeEffect.Stop();
        }

        currentState = newState;

        //新しい状態がチャージ状態なら演出を始める
        if (currentState == PaddleState.Charging)
        {
            SoundManager.Instance.StartChargeSE();
            if (chargeEffect != null) chargeEffect.Play();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!isCharging)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();

                if (hitPlayer != null && animator != null)
                {
                    animator.Play("Squash", 0, 0f);

                    int layerIndex = animator.GetLayerIndex("Eyes Layer");

                    if (hitPlayer.FatnessLevel == 2)
                        animator.Play("Cry", layerIndex, 0f);
                    else
                        animator.Play("Pain", layerIndex, 0f);

                    if (hitPlayer.FatnessLevel != lastFatnessLevel) //プレイヤーの体型が変わっていたら処理
                    {
                        UpdatePaddleVisual(hitPlayer.FatnessLevel); //見た目を更新
                        lastFatnessLevel = hitPlayer.FatnessLevel; //記憶する体型を更新
                    }
                }
            }
        }
    }

    public void SetPlayer(bool hasPlayer)
    {
        if (animator != null)
        {
            animator.SetBool("hasPlayer", hasPlayer);
        }
    }

    //プレイヤーを発射する処理
    public void OnCannonFire()
    {
        SetPlayer(false);

        PlayerController player = gm.Player;

        if (player != null)
        {
            player.ActualLaunch();
        }

        SoundManager.Instance.PlayShotSE();
    }

    //プレイヤーを発射するアニメーションの再生
    public void PlayShotAnimation()
    {
        int layerIndex = animator.GetLayerIndex("Eyes Layer");

        if (animator != null)
        {
            animator.Play("Shot", layerIndex, 0f);
        }
    }

    //パドルの大きさを更新
    public void UpdatePaddleVisual(int fatnessLevel)
    {
        int oldLevel = lastFatnessLevel;
        int newLevel = fatnessLevel;

        if (sizeDatas != null && fatnessLevel >= 0 && fatnessLevel < sizeDatas.Length)
        {
            bodyResolver.SetCategoryAndLabel("Body", sizeDatas[fatnessLevel].sizeLabel);
            paddleCollider.size = new Vector2(sizeDatas[fatnessLevel].colliderWidth, paddleCollider.size.y);
        }

        if (oldLevel != -1 && oldLevel != newLevel) //初回起動時(-1)はエフェクトを出さない
        {
            //S <-> M の変化
            if ((oldLevel == 2 && newLevel == 1) || (oldLevel == 1 && newLevel == 2))
            {
                if (sEndRoot != null)
                {
                    //親の中にあるパーティクルを全て探して再生
                    var particleList = sEndRoot.GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particleList) ps.Play();
                }
            }
            //M <-> L の変化
            else if ((oldLevel == 1 && newLevel == 0) || (oldLevel == 0 && newLevel == 1))
            {
                if (mEndRoot != null)
                {
                    //親の中にあるパーティクルを全て探して再生
                    var particleList = mEndRoot.GetComponentsInChildren<ParticleSystem>();
                    foreach (var ps in particleList) ps.Play();
                }
            }
        }

        //GameManagerから呼ばれた時も、着地時の二重処理を防ぐために更新しておく
        lastFatnessLevel = fatnessLevel;
    }
}
