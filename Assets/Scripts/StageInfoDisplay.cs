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
    [SerializeField] private Toggle tutorialSkipToggle;

    [SerializeField] private StageData defaultStageData;

    public StageData CurrentData { get; private set; }

    private void Start()
    {
        if (tutorialSkipToggle != null)
            tutorialSkipToggle.onValueChanged.AddListener(OnTutorialSkipToggleChanged);

        if (defaultStageData != null)
            SetStageInfo(defaultStageData);
    }

    public void SetStageInfo(StageData data)
    {
        if (data == null) return;

        //表示するデータの更新
        CurrentData = data;

        stageTitleText.text = data.displayStageName;
        previewImage.sprite = data.previewImage;

        if (GameManager.Instance != null)
            highScoreText.text = GameManager.Instance.GetBestRecordText(data);

        previewImage.gameObject.SetActive(true);

        if (tutorialSkipToggle != null)
        {
            //チュートリアルが無いならトグルを非表示に
            tutorialSkipToggle.gameObject.SetActive(data.hasTutorial);

            if (data.hasTutorial)
            {
                //保存されてるデータを読み取ってチュートリアルスキップのトグルを更新
                bool isSkipped = (PlayerPrefs.GetInt("SkipTutorial_" + data.sceneName, 0) == 1);

                //クリックされてるわけでは無いので、情報は更新せず見た目だけ変更
                tutorialSkipToggle.SetIsOnWithoutNotify(isSkipped);
            }
        }
    }

    public void RefreshDisplay()
    {
        if (CurrentData != null && GameManager.Instance != null)
            highScoreText.text = GameManager.Instance.GetBestRecordText(CurrentData);
    }

    //チュートリアルスキップ用のトグルをクリックしたときに呼ばれる処理
    private void OnTutorialSkipToggleChanged(bool isSkip)
    {
        if (CurrentData != null)
        {
            //チェックが入ったら1、外れたら0を現在のステージ名で保存
            PlayerPrefs.SetInt("SkipTutorial_" + CurrentData.sceneName, isSkip ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
