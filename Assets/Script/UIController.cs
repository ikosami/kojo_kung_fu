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

    public void SetStage(int stage)
    {
        stageText.text = $"STAGE-{stage}";

        var stageData = Reference.Instance.stageDataList.List.Find(x => x.StageNum == stage);
        StageBackFrontImage.sprite = stageData.stageFrontSprite;
        StageBackImage.sprite = stageData.stageBackSprite;
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
