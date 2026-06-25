using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CakeBlock : MonoBehaviour, IDamageable
{
    [SerializeField] int ScorePoint = 100;
    [SerializeField] int HitPoint = 1;
    [SerializeField] float FatnessPoint = 5.0f;

    [SerializeField] private GameObject breakEffect;

    private Collider2D myCollider;
    private bool isDestroying = false;

    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider2D>();
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
            if (breakEffect != null)
            {
                Instantiate(breakEffect, transform.position, Quaternion.identity);
            }

            GameManager.Instance.DestroyedBlock(ScorePoint, FatnessPoint); //GameManagerの関数を呼び出す
            Destroy(gameObject);
        }
    }
}
