using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScrollMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f; //流れるスピード
    [SerializeField] private float destroyX = -15f; //どのくらい左に行ったら消滅するか

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.IsPaused || GameManager.Instance.isEnding || GameManager.Instance.Player == null)
            return;

        transform.Translate(Vector3.left *  speed * Time.deltaTime);

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
