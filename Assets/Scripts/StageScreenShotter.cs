using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScreenShotter : MonoBehaviour
{
    /*
     *ステージセレクトに使うプレビュー画像撮影用
     *MainCamera, CloudPad, PaddlePinをオフにするとエラーが消える
     */

    [SerializeField] Material backgroundMaterial;
    [Range(-1f, 1f)]
    [SerializeField] float manualOffsetY = 0f;

    [SerializeField] string fileName = "StageCapture.png";

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if(backgroundMaterial != null)
        {
            backgroundMaterial.SetFloat("_OffsetY", manualOffsetY);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeShot();
        }
#endif
    }

    private void TakeShot()
    {
        ScreenCapture.CaptureScreenshot(fileName);
        Debug.Log("スクショを保存しました: " + fileName);
    }
}
