using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScrollMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f; //流れるスピード
    [SerializeField] private float destroyX = -15f; //どのくらい左に行ったら消滅するか

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (gm == null || gm.IsPaused || gm.isEnding || gm.Player == null)
            return;

        transform.Translate(Vector3.left *  speed * Time.deltaTime);

        if (transform.position.x < destroyX)
            Destroy(gameObject);
    }
}
