using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageTitleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Image previewImage;

    [SerializeField] private StageData defaultStageData;
    private void Start()
    {
        if (defaultStageData != null)
            SetStageInfo(defaultStageData);
    }

    public void SetStageInfo(StageData data)
    {
        if (data == null) return;

        stageTitleText.text = data.displayStageName;
        previewImage.sprite = data.previewImage;

        if (GameManager.Instance != null)
        {
            int highScore = GameManager.Instance.GetHighScore(data.sceneName);
            highScoreText.text = "High Score: " + highScore.ToString();
        }

        previewImage.gameObject.SetActive(true);
    }
}
