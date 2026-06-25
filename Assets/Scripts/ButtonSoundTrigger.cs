using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    //マウスがボタンに乗った時
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlayCursorSE();
    }

    //クリックした時
    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlayDecisionSE();
    }
}
