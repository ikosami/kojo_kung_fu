using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI scoreText;
    public Image[] hpBars;

    private void Start()
    {
        SetStage(2);
    }

    void SetStage(int stage)
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
