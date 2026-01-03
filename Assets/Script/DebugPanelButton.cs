using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanelButton : MonoBehaviour
{
    public Button buttonRun;
    public Button buttonLeft;
    public Button buttonRight;

    public int kind = 0;

    public Action OnChange;

    public string[] nameKindList;

    [SerializeField] TextMeshProUGUI textKind;
    [SerializeField] TextMeshProUGUI nameKind;


    void Start()
    {
        buttonLeft.onClick.AddListener(() =>
        {
            kind--;
            if (kind < 0) kind = nameKindList.Length - 1;
            UpdateView();
        });

        buttonRight.onClick.AddListener(() =>
        {
            kind++;
            if (kind >= nameKindList.Length) kind = 0;
            UpdateView();
        });
    }
    internal void UpdateView()
    {
        textKind.text = (kind + 1) + "/" + nameKindList.Length;
        nameKind.text = nameKindList[kind];
        OnChange?.Invoke();
    }

    internal void SetView(int stageNum)
    {
        kind = stageNum;
        kind = Mathf.Clamp(kind, 0, nameKindList.Length - 1);
        UpdateView();
    }
}
