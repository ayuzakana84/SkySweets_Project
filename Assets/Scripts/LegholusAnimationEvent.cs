using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegholusAnimationEvent : MonoBehaviour
{
    // ダメージ硬直を解除するアニメーションイベント
    public void EndStun()
    {
        GetComponentInParent<LegholusController>().EndStun();
    }

    // 巨大化（お菓子を食べる）アニメーションが終わった時のイベント
    public void OnChangeMedium()
    {
        GetComponentInParent<LegholusController>().ChangeToMediumSize();
    }

    public void OnChangeLarge()
    {
        GetComponentInParent<LegholusController>().ChangeToLargeSize();
    }

    //倒したアニメーション中に効果音を鳴らすイベント
    public void OnPlayFlappingSE()
    {
        SoundManager.Instance.PlayFlappingSE();
    }

    public void OnPlayFallDownSE()
    {
        SoundManager.Instance.PlayFallDownSE();
    }

    public void OnPlayDefeatedSE()
    {
        SoundManager.Instance.PlayDefeatedSE();
    }
}
