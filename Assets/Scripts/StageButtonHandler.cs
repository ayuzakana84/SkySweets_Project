using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StageButtonHandler : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private StageData stageData;
    [SerializeField] private StageInfoDisplay display; //StageDataを表示するためのパネル

    //マウスが乗った時だけ実行
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (stageData != null && display != null)
        {
            display.SetStageInfo(stageData);
        }
    }
}
