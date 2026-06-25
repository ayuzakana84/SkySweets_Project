using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FatnessGauge : MonoBehaviour
{
    [Header("ゲージの画像")]
    [SerializeField] Image layerChubby;
    [SerializeField] Image layerMaxFat;
    [SerializeField] GameObject fullTextObj;

    //ゲージが動くスピード
    [SerializeField] float fillSpeed = 2f;

    //ゲージが動く目標の値を保存する変数
    private float targetChubby;
    private float targetMaxFat;

    //GameMangerから呼ばれたら目標の値を保存する
    public void RefreshGauge(float point, int chubbyThreshold, int maxFatThreshold)
    {
        //ゲージを上げるパーセントを計算 (Clamp01で100%を超えても切り捨て)
        targetChubby = Mathf.Clamp01(point / chubbyThreshold);

        if (point > chubbyThreshold)
        {
            targetMaxFat = Mathf.Clamp01((float)(point - chubbyThreshold) / (maxFatThreshold - chubbyThreshold));
        }
        else
        {
            targetMaxFat = 0f;
        }
    }

    private void Start()
    {
        // 実際の見た目を０にする
        if (layerChubby != null) layerChubby.fillAmount = 0f;
        if (layerMaxFat != null) layerMaxFat.fillAmount = 0f;
        if (fullTextObj != null) fullTextObj.SetActive(false);

        // 目標地点も０にしておく
        targetChubby = 0f;
        targetMaxFat = 0f;
    }

    private void Update()
    {
        //ゲージを目標の値（target）に向かって、少しずつ近づける
        layerChubby.fillAmount = Mathf.MoveTowards(layerChubby.fillAmount, targetChubby, fillSpeed * Time.deltaTime);
        layerMaxFat.fillAmount = Mathf.MoveTowards(layerMaxFat.fillAmount, targetMaxFat, fillSpeed * Time.deltaTime);

        //ゲージが満タンになったら FullTextObj を表示する
        if (fullTextObj != null)
        {
            if(layerMaxFat.fillAmount >= 1f)
                fullTextObj.SetActive(true);
            else
                fullTextObj.SetActive(false);
        }
    }
}
