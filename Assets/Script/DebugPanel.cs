using System;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] StageManager stageManager;
    [SerializeField] Button popStage;
    [SerializeField] Button buttonLeft;
    [SerializeField] Button buttonRight;

    [SerializeField] TextMeshProUGUI textKind;
    [SerializeField] Image image;

    int kind = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;

        popStage.onClick.AddListener(OnClickPopStage);

        buttonLeft.onClick.AddListener(() =>
        {
            kind--;
            if (kind < 0) kind = enemyDataList.Count - 1;
            UpdateView();
        });

        buttonRight.onClick.AddListener(() =>
        {
            kind++;
            if (kind >= enemyDataList.Count) kind = 0;
            UpdateView();
        });

        UpdateView();
    }

    void UpdateView()
    {
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        textKind.text = kind.ToString();
        var img = enemyDataList[kind].prefab.GetComponent<Image>();

        if (img == null)
        {
            img = enemyDataList[kind].prefab.transform.GetChild(0).GetComponent<Image>();
        }

        image.sprite = img.sprite;
    }

    private void OnClickPopStage()
    {
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        var enemyPopData = enemyDataList[kind];

        stageManager.SpawnEnemy(enemyPopData.prefab, 50);
    }
}
