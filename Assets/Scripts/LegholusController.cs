using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegholusController : MonoBehaviour, IDamageable, IBoss
{
    [SerializeField] GameObject smallSizeBody;
    [SerializeField] GameObject mediumSizeBody;
    [SerializeField] GameObject largeSizeBody;

    [SerializeField] ParticleSystem smokeEffect;

    [SerializeField] int maxHitPoint = 12;
    private int hitPoint;

    private int bossLevel = 0;
    public int BossLevel => bossLevel;

    [SerializeField] float moveSpeed = 3.0f;
    [SerializeField] float moveDistance = 10.0f; //スタート地点から左右にどの程度動くか

    private Vector3 startPosition;
    private bool isMovingRight = false;

    private bool isStunned = false; //ダメージを受けたかどうか
    private bool isTransitioning = false; //体型変化中かどうか

    private Animator animSmall;
    private Animator animMedium;
    private Animator animLarge;

    private Animator currentAnim;

    public event System.Action<int> OnBossDamageTaken;
    public int MaxHP => maxHitPoint;

    private enum BossState { Patrol, Swoop ,Idle}
    private BossState currentState = BossState.Patrol;

    [SerializeField] float swoopDuration = 1.5f; //突進が終わるまでの時間
    [SerializeField] float swoopDipAmount = 4.0f; //突進でどこまで下がるか
    private int turnCount = 0; //方向転換した回数をカウント

    void Start()
    {
        hitPoint = maxHitPoint;

        smallSizeBody.SetActive(true);
        mediumSizeBody.SetActive(false);
        largeSizeBody.SetActive(false);

        animSmall = smallSizeBody.GetComponent<Animator>();
        animMedium = mediumSizeBody.GetComponent<Animator>();
        animLarge = largeSizeBody.GetComponent<Animator>();

        currentAnim = animSmall;
        startPosition = transform.position;
    }

    void Update()
    {
        //スタン中か変身中なら移動をさせない
        if (isTransitioning || isStunned)
            return;

        if (currentState == BossState.Patrol)
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (isMovingRight)
        {
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;

            if (transform.position.x >= startPosition.x + moveDistance)
            {
                isMovingRight = false;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                CheckTurnForSwoop();
            }
        }
        else
        {
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            if (transform.position.x <= startPosition.x - moveDistance)
            {
                isMovingRight = true;
                transform.rotation = Quaternion.Euler(0, 180, 0);
                CheckTurnForSwoop();
            }
        }
    }

    private void CheckTurnForSwoop()
    {
        turnCount++;

        //２回ターン（１往復）していて、ボスレベルが１以上なら突進攻撃開始
        if (turnCount >= 2 && bossLevel >= 1)
        {
            StartCoroutine(SwoopSequence());
        }
    }

    private IEnumerator SwoopSequence()
    {
        currentState = BossState.Swoop;
        turnCount = 0;

        yield return new WaitForSeconds(1.0f);

        float timer = 0f;
        Vector3 startPos = transform.position;

        float targetX = isMovingRight ? (startPosition.x + moveDistance) : (startPosition.x - moveDistance);
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);

        //U字に滑空
        while (timer < swoopDuration)
        {
            timer += Time.deltaTime;

            //進行度（0.0 から 1.0）
            float progress = timer / swoopDuration;

            //X軸は等速で横にスライド
            float currentX = Mathf.Lerp(startPos.x, targetPos.x, progress);

            //Y軸はサインカーブで下に弧を描く動くを作る
            float currentY = startPos.y - (Mathf.Sin(progress * Mathf.PI) * swoopDipAmount);

            transform.position = new Vector3(currentX, currentY, startPos.z);
            yield return null;
        }

        transform.position = targetPos;

        isMovingRight = !isMovingRight;
        if (isMovingRight)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        currentState = BossState.Idle;

        yield return new WaitForSeconds(2.0f); //大きな隙

        //パトロール再開
        currentState = BossState.Patrol;
    }

    public void AddDamage(int damage)
    {
        //変身中ならダメージを与えない
        if (isTransitioning) return;

        //突進中はダメージを無効化して、弾いた音を鳴らす
        if (currentState == BossState.Swoop)
        {
            currentAnim.SetTrigger("OnBounce");
            SoundManager.Instance.PlayBossBounceSE();
            return;
        }

        if (damage < (bossLevel + 1))
        {
            currentAnim.SetTrigger("OnBounce");
            SoundManager.Instance.PlayBossBounceSE();
            return;
        }

        int finalDamage = Mathf.Max(1, damage - bossLevel);
        hitPoint -= finalDamage;

        SoundManager.Instance.PlayAttackSE();
        currentAnim.SetTrigger("OnDamage");

        int displayHPLevel = bossLevel;
        if (bossLevel == 0 && hitPoint <= 8)
        {
            currentAnim.SetBool("OnNextLevel", true);
            isTransitioning = true;
            displayHPLevel = 1;
        }
        else if (bossLevel == 1 && hitPoint <= 4)
        {
            currentAnim.SetBool("OnNextLevel", true);
            isTransitioning = true;
            displayHPLevel = 2;
        }
        else if (bossLevel == 2 && hitPoint <= 0)
        {
            isTransitioning = true;
            StartCoroutine(DefeatSequence());
        }
        else
        {
            isStunned = true; //倒れたので動きを止める
        }

        OnBossDamageTaken?.Invoke(hitPoint);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();

            if (player != null && !isTransitioning)
            {
                //プレイヤーの肥満度レベルがボスのレベル以下ならはじき返す
                if (player.FatnessLevel < bossLevel || currentState == BossState.Swoop)
                {
                    //ボスの中心からプレイヤーに向かうベクトル（外側へ弾き返す方向）を計算
                    Vector2 repelDirection = (player.transform.position - transform.position).normalized;

                    //プレイヤーの Knockback 関数を呼んで吹き飛ばす！（1.3倍の力）
                    player.Knockback(repelDirection, 1.3f);
                }
            }
        }
    }

    private IEnumerator DefeatSequence()
    {
        GameManager.Instance.OnBossDefeated();

        currentAnim.SetBool("OnDefeat", true);

        yield return new WaitForSeconds(8.0f);

        smallSizeBody.SetActive(false);
        mediumSizeBody.SetActive(false);
        largeSizeBody.SetActive(false);

        GameManager.Instance.StageCleared();
    }

    //アニメーションイベントの 中継スクリプト から呼ばれる スタン終了用の関数
    public void EndStun()
    {
        isStunned = false;
    }

    //アニメーションイベントの 中継スクリプト から呼ばれる 変身用の関数
    public void ChangeToMediumSize()
    {
        currentAnim.SetBool("OnNextLevel", false);

        if (smokeEffect != null)
            smokeEffect.Play();

        smallSizeBody.SetActive(false);
        mediumSizeBody.SetActive(true);

        currentAnim = animMedium;

        bossLevel = 1;
        moveSpeed = 5f;
        isTransitioning = false;
    }

    //アニメーションイベントの 中継スクリプト から呼ばれる 変身用の関数
    public void ChangeToLargeSize()
    {
        currentAnim.SetBool("OnNextLevel", false);

        if (smokeEffect != null)
            smokeEffect.Play();

        mediumSizeBody.SetActive(false);
        largeSizeBody.SetActive(true);

        currentAnim = animLarge;

        bossLevel = 2;
        moveSpeed = 8f;
        isTransitioning = false;
    }
}