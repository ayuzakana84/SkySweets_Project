using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CakeBlock : MonoBehaviour, IDamageable
{
    [SerializeField] int scorePoint = 100;
    [SerializeField] int hitPoint = 1;
    [SerializeField] float fatnessPoint = 5.0f;

    [SerializeField] private GameObject breakEffect;

    private bool isDestroying = false;

    public void AddDamage(int damage)
    {
        if (isDestroying) return; //二重処理防止

        //サウンドマネージャーで効果音を鳴らす(ランダム)
        SoundManager.Instance.PlayBlockHitSE();

        hitPoint -= damage;

        if (hitPoint <= 0)
        {
            isDestroying = true;

            //エフェクトを再生する
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }

            GameManager.Instance.DestroyedBlock(scorePoint, fatnessPoint); //GameManagerの関数を呼び出す
            Destroy(gameObject);
        }
    }
}
