using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleAnimationEvent : MonoBehaviour
{
    // アニメーターと同じ階層に関数を置くことでアニメーションイベントが読み取れる
    public void OnCannonFire()
    {
        // 親（PaddleController）の関数を呼んであげる
        GetComponentInParent<PaddleController>().OnCannonFire();
    }
}
