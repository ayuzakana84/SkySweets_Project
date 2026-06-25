using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class ResultScoreViewer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resultScoreText;

    void Start()
    {
        //GameManagerからスコアの値を取ってくる
        int score = GameManager.Instance.GetScore();

        //テキストに表示
        resultScoreText.text = "Result: " + score.ToString();
    }
}
