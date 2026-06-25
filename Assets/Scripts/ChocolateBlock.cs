using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChocolateBlock : MonoBehaviour, IDamageable
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] int ScorePoint = 300;
    [SerializeField] int HitPoint = 3;
    [SerializeField] float FatnessPoint = 1.0f;

    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite damagedSprite01;
    [SerializeField] Sprite damagedSprite02;

    [SerializeField] private GameObject breakEffect;

    private Collider2D myCollider;
    private bool isDestroying = false;

    public event Action<int> OnDamageTaken;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normalSprite;

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

        //エフェクトを再生する
        if (breakEffect != null)
        {
            Instantiate(breakEffect, transform.position, Quaternion.identity);
        }

        HitPoint -= damage;
        OnDamageTaken?.Invoke(HitPoint);

        if (HitPoint == 2)
        {
            spriteRenderer.sprite = damagedSprite01;
        }
        else if (HitPoint == 1)
        {
            spriteRenderer.sprite = damagedSprite02;
        }
        else if (HitPoint <= 0)
        {
            isDestroying = true;

            GameManager.Instance.DestroyedBlock(ScorePoint, FatnessPoint); // GameManagerの関数を呼び出す
            Destroy(gameObject);
        }
    }

    private void UpdateTriggerState(int currentFatnessLevel)
    {
        if (isDestroying || myCollider == null)
            return;

        if (currentFatnessLevel >= 2)
            myCollider.isTrigger = true;
        else
            myCollider.isTrigger = false;
    }
}
