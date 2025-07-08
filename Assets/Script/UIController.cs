using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image StageBackFrontImage;
    public Image StageBackImage;

    public TextMeshProUGUI stageText;
    public TextMeshProUGUI scoreText;
    public Image[] hpBars;

    public void SetStage(int stage)
    {
        stageText.text = $"STAGE-{stage}";
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
