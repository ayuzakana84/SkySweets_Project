using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPGauge : MonoBehaviour
{
    [SerializeField] Slider hpSlider;
    [SerializeField] Transform notchContainer;
    [SerializeField] GameObject notchPrefab;

    [SerializeField] Image fillImage;

    [SerializeField] Color highHpColor = Color.green;
    [SerializeField] Color midHpColor = Color.yellow;
    [SerializeField] Color lowHpColor = Color.red;

    private IBoss targetBoss;

    // Start is called before the first frame update
    void Start()
    {
        var bossObj = GameObject.FindWithTag("Boss");

        if (bossObj != null)
        {
            targetBoss = bossObj.GetComponent<IBoss>();

            if (targetBoss != null)
            {
                SetupGauge(targetBoss.MaxHP);

                UpdateGauge(targetBoss.MaxHP);

                targetBoss.OnBossDamageTaken += UpdateGauge;
            }
        }
    }

    private void OnDestroy()
    {
        if (targetBoss != null)
        {
            targetBoss.OnBossDamageTaken -= UpdateGauge;
        }
    }

    private void SetupGauge(int maxHP)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = maxHP;
        }
        GenerateNotches(maxHP);
    }

    private void UpdateGauge(int currentHP)
    {
        if (hpSlider != null) hpSlider.value = currentHP;
        if (fillImage == null) return;

        //現在のHPが最大の何パーセントか計算
        float hpRatio = (float)currentHP / hpSlider.maxValue;

        if (hpRatio <= 0.34f)
            fillImage.color = lowHpColor;
        else if (hpRatio <= 0.67f)
            fillImage.color = midHpColor;
        else
            fillImage.color = highHpColor;
    }

    private void GenerateNotches(int maxHP)
    {
        if (notchContainer == null || notchPrefab == null) return;

        foreach (Transform child in notchContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxHP; i++)
        {
            GameObject notch = Instantiate(notchPrefab, notchContainer);

            if (i == maxHP - 1)
            {
                //もし最後の（右端の）ブロックなら非表示にする
                Transform lineObj = notch.transform.Find("Line");
                if (lineObj != null)
                {
                    lineObj.gameObject.SetActive(false);
                }
            }
        }
    }
}
