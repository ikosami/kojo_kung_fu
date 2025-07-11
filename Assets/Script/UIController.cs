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
}
