using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboUIAnimation : MonoBehaviour
{
    private Vector3 initialPos;
    private Coroutine jumpCoroutine;

    private void Awake()
    {
        //èâä˙à íuÇãLò^
        initialPos = transform.localPosition;
    }

    public void PlayJump()
    {
        if (jumpCoroutine != null)
        {
            StopCoroutine(jumpCoroutine);
            transform.localPosition = initialPos;
        }

        jumpCoroutine = StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        float jumpHeight = 30f;
        float duration = 0.2f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / duration;

            float yOffset = Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;
            transform.localPosition = initialPos + new Vector3(0, yOffset, 0);

            yield return null;
        }

        transform.localPosition = initialPos;
        jumpCoroutine = null;
    }
}
