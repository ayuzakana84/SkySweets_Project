using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    void Start()
    {
        //チュートリアル中のフラグを立てる
        if (GameManager.Instance != null)
            GameManager.Instance.isTutorialActive = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            CloseTutorial();
    }

    private void CloseTutorial()
    {
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.isTutorialActive = false;
    }
}
