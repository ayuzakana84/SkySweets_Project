using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddlePinIndicator : MonoBehaviour
{
    private RectTransform rectTransform;

    [SerializeField] private GameObject pinVisual; //表示を切り替える画像(子オブジェクト)

    [SerializeField] private float showThreshold = -100f; //ピンを表示するためのしきい値

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (pinVisual != null )
            pinVisual.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance == null)
            return;

        Vector3 paddleWorldPos = GameManager.Instance.GetPaddlePosition();

        //スクリーン座標に変換
        Vector3 screenPos = Camera.main.WorldToScreenPoint(paddleWorldPos);

        //パドルが画面より下にいるかチェック
        bool isHiddenBelow = screenPos.y < showThreshold;

        //ピンの表示、非表示を切り替え
        if (pinVisual != null && pinVisual.activeSelf != isHiddenBelow)
            pinVisual.SetActive(isHiddenBelow);

        //ピンを表示してる間、パドルのX軸に合わせて動かす
        if (isHiddenBelow)
            rectTransform.position = new Vector3(screenPos.x, rectTransform.position.y, 0);
    }
}
