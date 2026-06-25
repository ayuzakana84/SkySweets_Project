using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D paddleCollider;
    [SerializeField] private SpriteResolver bodyResolver;
    [SerializeField] private float moveLimitX = 10f;

    [SerializeField] private GameObject sEndRoot;
    [SerializeField] private GameObject mEndRoot;
    [SerializeField] private ParticleSystem chargeEffect;

    [HideInInspector] public bool isCharging = false;

    private bool isLocked = false; //ゲームクリア、ゲームオーバー演出中かを見る変数
    private bool waitForRelease = false;
    private Animator animator;
    private int lastFatnessLevel = -1; //体型が変化したかチェックするための変数

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterPaddle(this);
    }

    public void LockPaddle()
    {
        isLocked = true;
    }

    void Update()
    {
        //ポーズ画面、ゲームクリア、ゲームオーバー演出、チュートリアル中は動かないように
        if (GameManager.Instance.IsPaused || isLocked || GameManager.Instance.isTutorialActive)
        {
            //チャージ状態なら、強制的にオフにして音とエフェクトを止める
            if (isCharging)
                StopCharge();

            return;
        }

        //マウスのスクリーン座標を取得
        Vector2 mousePosition = Input.mousePosition;

        //スクリーン座標をワールド座標に変換
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        //パドルの新しい位置を計算
        float targetX = worldPosition.x;

        //プレイヤーがパドルの中にいたり、パドルに追従しているなら壁の外に出ないよう移動幅に制限をかける
        if (GameManager.Instance.Player != null &&
            GameManager.Instance.Player.CurrentState != PlayerController.PlayerState.Playing)
        {
            targetX = Mathf.Clamp(worldPosition.x, -moveLimitX, moveLimitX);
        }

        transform.position = new Vector2(targetX, transform.position.y);

        //カウントダウン中はパドルの移動だけ処理
        if (GameManager.Instance.Player != null && 
            GameManager.Instance.Player.CurrentState == PlayerController.PlayerState.WaitingForStart)
        {
            if (isCharging)
                StopCharge();

            //カウントダウンをスキップするための左クリックでチャージ状態にならないようにする
            if (Input.GetMouseButton(0))
                waitForRelease = true;

            return;
        }

        if (waitForRelease && !Input.GetMouseButton(0))
            waitForRelease = false;

        //チャージ状態の切り替え
        if (Input.GetMouseButton(0) && !waitForRelease)
        {
            if (!isCharging)
                StartCharge();
        }
        else
        {
            if (isCharging)
                StopCharge();
        }

        //チャージ状態による表情の更新
        if (animator != null)
            animator.SetBool("isCharging", isCharging);
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

    private void StartCharge()
    {
        isCharging = true;

        SoundManager.Instance.StartChargeSE();

        if (chargeEffect != null)
            chargeEffect.Play();
    }

    private void StopCharge()
    {
        isCharging = false;

        SoundManager.Instance.StopChargeSE();

        if (chargeEffect != null)
            chargeEffect.Stop();
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

        PlayerController player = GameManager.Instance.Player;

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

        string sizeLabel = "L";
        float colliderWidth = 9.6f;

        if (fatnessLevel == 2)
        {
            sizeLabel = "S";
            colliderWidth = 3f;
        }
        else if (fatnessLevel == 1)
        {
            sizeLabel = "M";
            colliderWidth = 6.2f;
        }
        else
        {
            sizeLabel = "L";
            colliderWidth = 9.6f;
        }

        bodyResolver.SetCategoryAndLabel("Body", sizeLabel);
        paddleCollider.size = new Vector2(colliderWidth, paddleCollider.size.y);

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
