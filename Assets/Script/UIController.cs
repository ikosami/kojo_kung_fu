using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public RectTransform StageRect;
    public Transform EnemyParent;
    public Image StageBackFrontImage;
    public Image StageBackImage;

    public TextMeshProUGUI stageText;
    public TextMeshProUGUI scoreText;

    public TextMeshProUGUI lifeText;
    public Image[] hpBars;

    StageData stageData;

    public void SetStage(int stage)
    {
        stageText.text = $"STAGE-{stage}";

        stageData = Reference.Instance.stageDataList.List.Find(x => x.StageNum == stage);
        StageBackFrontImage.sprite = stageData.stageFrontSprites[0];
        StageBackImage.sprite = stageData.stageBackSprites[0];
    }

    float backTimer = 0f;
    int backIndex = 0;
    float backFontTimer = 0f;
    int backFontIndex = 0;
    private void Update()
    {

        if (stageData != null)
        {
            if (stageData.stageBackSprites.Length > 1)
            {
                backTimer += Time.deltaTime;
                if (backTimer > 0.5f)
                {
                    backTimer = 0f;
                    backIndex++;
                    if (backIndex >= stageData.stageBackSprites.Length)
                    {
                        backIndex = 0;
                    }
                    StageBackImage.sprite = stageData.stageBackSprites[backIndex];
                }
            }

            if (stageData.stageFrontSprites.Length <= 1)
            {
                backFontTimer += Time.deltaTime;
                if (backFontTimer > 0.5f)
                {
                    backFontTimer = 0f;
                    backFontIndex++;
                    if (backFontIndex >= stageData.stageFrontSprites.Length)
                    {
                        backFontIndex = 0;
                    }
                    StageBackFrontImage.sprite = stageData.stageFrontSprites[backFontIndex];
                }
            }
        }

    }

    public void SetHp(int hp)
    {
        for (int i = 0; i < hpBars.Length; i++)
        {
            if (i < hp)
            {
                hpBars[i].gameObject.SetActive(true);
            }
            else
            {
                hpBars[i].gameObject.SetActive(false);
            }
        }

    }

    public void SetLife()
    {
        int life = SaveDataManager.Life;
        lifeText.text = $"-{life}";
    }


    [SerializeField] GameObject dojoObj;
    [SerializeField] GameObject[] manuals;

    internal void SetDojo(int dojo)
    {
        stageText.text = $"Dojo-" + dojo;

        dojoObj.gameObject.SetActive(true);
        for (int i = 0; i < manuals.Length; i++)
        {
            manuals[i].SetActive(i == dojo); ;
        }

        StageBackFrontImage.gameObject.SetActive(false);
        StageBackImage.gameObject.SetActive(false);
    }

}
