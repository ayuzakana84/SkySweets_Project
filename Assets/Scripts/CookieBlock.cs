using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieBlock : MonoBehaviour, IDamageable
{
    [SerializeField] int ScorePoint = 100;
    [SerializeField] int HitPoint = 1;
    [SerializeField] float FatnessPoint = 1.0f;

    [SerializeField] private GameObject breakEffect;

    private Collider2D myCollider;
    private bool isDestroying = false;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();

        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            UpdateTriggerState(GameManager.Instance.Player.FatnessLevel);

            GameManager.Instance.Player.OnFatnessLevelChanged += UpdateTriggerState;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Player != null)
        {
            GameManager.Instance.Player.OnFatnessLevelChanged -= UpdateTriggerState;
        }
    }

    public void AddDamage(int damage)
    {
        if (isDestroying) return; //二重処理防止

        //サウンドマネージャーで効果音を鳴らす(ランダム)
        SoundManager.Instance.PlayBlockHitSE();

        HitPoint -= damage;

        if (HitPoint <= 0)
        {
            isDestroying = true;

            //エフェクトを再生する
            if(breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }

            GameManager.Instance.DestroyedBlock(ScorePoint, FatnessPoint); //GameManagerの関数を呼び出す
            Destroy(gameObject);
        }
    }

    private void UpdateTriggerState(int currentFatnessLevel)
    {
        if (isDestroying || myCollider == null)
            return;

        if (currentFatnessLevel >= 1)
            myCollider.isTrigger = true;
        else
            myCollider.isTrigger = false;
    }
}
