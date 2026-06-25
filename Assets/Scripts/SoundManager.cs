using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//BGMを名前で呼べるように
public enum BGMType
{
    Title,
    MainStage,
    Clear,
    GameOver,
    StageSelect
}

//BGMのイントロとループをセットで管理
[System.Serializable]
public class BGMSet
{
    public BGMType type;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("スピーカー設定")]
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource loopSESource;
    [SerializeField] private AudioSource jingleSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("BGMリスト設定")]
    [SerializeField] private BGMSet[] bgmSettings;

    [Header("ジャンプ音")]
    [SerializeField] private AudioClip jumpNormalSE;
    [SerializeField] private AudioClip jumpPlumpSE;
    [SerializeField] private AudioClip jumpHeavySE;

    [Header("ブロック破壊時の音（ランダム）")]
    [SerializeField] private AudioClip[] blockHitSEs;

    [Header("パドルのチャージ音")]
    [SerializeField] private AudioClip chargeSE;

    [Header("パドル大砲の射出音")]
    [SerializeField] private AudioClip shotSE;

    [Header("プレイヤーのミス音")]
    [SerializeField] private AudioClip missSE;

    [Header("プレイヤーの攻撃音")]
    [SerializeField] private AudioClip attackSE;

    [Header("ボス関連")]
    [SerializeField] private AudioClip bossBounceSE;
    [SerializeField] private AudioClip FallDownSE;
    [SerializeField] private AudioClip DefeatedSE;
    [SerializeField] private AudioClip FlappingnSE;

    [Header("UI音")]
    [SerializeField] private AudioClip cursorSE; //カーソル音
    [SerializeField] private AudioClip decisionSE; //決定音
    [SerializeField] private AudioClip buzzerSE;

    [Header("ジングル")]
    [SerializeField] private AudioClip GameClearJingle;
    [SerializeField] private AudioClip GameOverJingle;

    private Coroutine fadeCoroutine;
    private float defaultBGMVolume;

    private void Awake() //Awake()はStart()よりも早く実行される
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //BGMを流すために音量を記録しておく
            if (bgmSource != null)
                defaultBGMVolume = bgmSource.volume;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    //ジャンプ音を鳴らす
    public void PlayJumpSE(int bodyType)
    {
        AudioClip clip = null; //鳴らす効果音を入れる変数


        if (bodyType == 0) //通常体型
            clip = jumpNormalSE;
        else if (bodyType == 1) //ぽっちゃり体型
            clip = jumpPlumpSE;
        else if (bodyType == 2) //肥満体型
            clip = jumpHeavySE;

        if (clip != null)
            seSource.PlayOneShot(clip);
    }

    //ブロックにぶつかった時の音を鳴らす
    public void PlayBlockHitSE()
    {
        //配列が空じゃないかチェック
        if (blockHitSEs != null && blockHitSEs.Length > 0)
        {
            //ランダムに音を選ぶ
            int randomIndex = Random.Range(0, blockHitSEs.Length);

            //選ばれた音を鳴らす
            AudioClip clip = blockHitSEs[randomIndex];
            if (clip != null)
                seSource.PlayOneShot(clip);
        }
    }

    //カーソル音(ボタンホバー時)
    public void PlayCursorSE()
    {
        if (cursorSE != null)
            seSource.PlayOneShot(cursorSE);
    }

    //決定音(ボタンクリック時)
    public void PlayDecisionSE()
    {
        if (decisionSE != null)
            seSource.PlayOneShot(decisionSE);
    }

    //ブザー音
    public void PlayBuzzerSE()
    {
        if (buzzerSE != null)
            seSource.PlayOneShot(buzzerSE);
    }

    //チャージ音開始
    public void StartChargeSE()
    {
        if (chargeSE != null && !loopSESource.isPlaying)
        {
            loopSESource.clip = chargeSE;
            loopSESource.loop = true;
            loopSESource.Play();
        }
    }

    //チャージ音停止
    public void StopChargeSE()
    {
        loopSESource.Stop();
        loopSESource.loop = false;
    }

    //射出音
    public void PlayShotSE()
    {
        if (shotSE != null)
            seSource.PlayOneShot(shotSE);
    }

    //ミス音
    public void PlayMissSE()
    {
        if (missSE != null)
            seSource.PlayOneShot(missSE);
    }

    public void PlayAttackSE()
    {
        if (attackSE != null)
            seSource.PlayOneShot(attackSE);
    }

    public void PlayBossBounceSE()
    {
        if (bossBounceSE != null)
            seSource.PlayOneShot(bossBounceSE);
    }

    public void PlayFallDownSE()
    {
        if (FallDownSE != null)
            seSource.PlayOneShot(FallDownSE);
    }

    public void PlayDefeatedSE()
    {
        if (DefeatedSE != null)
            seSource.PlayOneShot(DefeatedSE);
    }

    public void PlayFlappingSE()
    {
        if (FlappingnSE != null)
            seSource.PlayOneShot(FlappingnSE);
    }

    //BGMを再生
    public void PlayBGM(BGMType targetType)
    {
        BGMSet target = System.Array.Find(bgmSettings, x => x.type == targetType);
        if (target == null || target.clip == null) return;

        // すでに同じ曲が流れているなら何もしない（リトライ時のブツ切り防止）
        if (bgmSource.isPlaying && bgmSource.clip == target.clip) return;

        bgmSource.Stop();
        bgmSource.clip = target.clip;
        bgmSource.loop = true; // ループをオンにする
        bgmSource.Play();
    }

    //ポーズに合わせてBGMの音量を調整する
    public void SetPauseBGM(bool isPaused)
    {
        if (bgmSource == null) return;

        if (isPaused)
        {
            //元の音量の20%に落とす
            bgmSource.volume = defaultBGMVolume * 0.2f;
        }
        else
        {
            //元の音量に戻す
            bgmSource.volume = defaultBGMVolume;
        }
    }

    //BGMを停止
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    //BGMのフェードアウト
    public void FadeOutBGM(float duration)
    {
        // すでにフェード中なら一旦止める（二重動作防止）
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume; //次のBGMを再生するためにボリュームを元に戻す
    }

    public void PlayGameClearJingle()
    {
        if (GameClearJingle != null)
            jingleSource.PlayOneShot(GameClearJingle);
    }

    public void PlayGameOverJingle()
    {
        if (GameOverJingle != null)
            jingleSource.PlayOneShot(GameOverJingle);
    }
}
