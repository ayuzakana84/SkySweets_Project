using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResetRecordPopup : MonoBehaviour
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private StageInfoDisplay infoDisplay;
    [SerializeField] private TextMeshProUGUI popupText;

    private void Awake()
    {
        yesButton.onClick.AddListener(OnYesPressed);
        noButton.onClick.AddListener(OnNoPressed);

        gameObject.SetActive(false);
    }

    //スコアリセットのButtonから紐づける関数
    public void OpenPopup()
    {
        if (infoDisplay == null || infoDisplay.CurrentData == null)
            return;

        if (popupText != null)
        {
            string stageName = infoDisplay.CurrentData.displayStageName;
            popupText.text = "<color=#FF0000>Reset Record</color> for <color=#FF0000>" + stageName + "</color> ?"; 
        }

        gameObject.SetActive(true);
    }

    private void OnYesPressed()
    {
        if (GameManager.Instance != null && infoDisplay != null)
        {
            GameManager.Instance.ResetStageRecord(infoDisplay.CurrentData);
            infoDisplay.RefreshDisplay();
        }

        gameObject.SetActive(false);
    }

    private void OnNoPressed()
    {
        gameObject.SetActive(false);
    }
}
