using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] int followStartY = 0;
    [SerializeField] float cameraOffsetY = 0.0f; //プレイヤーからどのくらい上を写すか

    [SerializeField] Material backgroundMaterial; //Inspectorで Mat_ScrollingSky をアタッチ
    [SerializeField] float scrollRatio = 0.1f;  //背景のスクロール速度（カメラの0.1倍）

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void LateUpdate()
    {
        float offsetY = 0;

        if (player.position.y > followStartY)
        {
            transform.position = new Vector3(startPosition.x, player.position.y + cameraOffsetY, startPosition.z);

            //scrollAmount:カメラが動く位置をどれだけ超えたか
            float scrollAmount = player.position.y - followStartY;
            offsetY = scrollAmount * scrollRatio;
        }

        if(backgroundMaterial != null)
        {
            backgroundMaterial.SetFloat("_OffsetY", offsetY);
        }
    }

    private void OnValidate()
    {
        //ゲーム再生中ではなく、エディタで数値をいじった時だけ実行される
        if (!Application.isPlaying)
        {
            //インスペクターで OffestY を変えると、Sceneビューのカメラも自動で動く
            transform.position = new Vector3(transform.position.x, followStartY + cameraOffsetY, transform.position.z);
        }
    }
}
