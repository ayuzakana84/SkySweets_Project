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
        bool wasBossStage = GameManager.Instance.isBossStage;

        if (!wasBossStage)
        {
            int score = GameManager.Instance.ScoreCount;

            resultScoreText.text = "Result: " + score.ToString();
        }
        else
        {
            float clearTime = GameManager.Instance.StageTimer;

            resultScoreText.text = "Clear Time: " + GameManager.Instance.FormatTime(clearTime);
        }
    }
}
