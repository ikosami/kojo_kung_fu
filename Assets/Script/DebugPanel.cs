using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    [SerializeField] StageManager stageManager;
    [Header("敵出現")]
    [SerializeField] DebugPanelButton debugPanelButton;
    [SerializeField] DebugPanelButton debugPanelButtonStage;
    [SerializeField] Button buttonBossRun;
    [SerializeField] Button buttonDojoRun;

    [SerializeField] Image image;


    [SerializeField] Button buttonGo;
    [SerializeField] Button buttonDestroy;
    [SerializeField] Button buttonRecovery;
    [SerializeField] Button buttonRecovery2;

    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Slider slider;

    [SerializeField] TextMeshProUGUI textPos;

    void Start()
    {
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        debugPanelButton.nameKindList = enemyDataList.Select(x => x.name).ToArray();
        debugPanelButton.OnChange = () => UpdateView();
        debugPanelButton.buttonRun.onClick.AddListener(OnClickPopStage);
        debugPanelButton.UpdateView();

        var list = new List<string>();
        for (int i = 0; i < Reference.Instance.stageDataList.List.Count; i++)
        {
            list.Add("STAGE-" + Reference.Instance.stageDataList.List[i].StageNum.ToString());
        }
        debugPanelButtonStage.nameKindList = list.ToArray();

        var stageNum = Reference.Instance.StageNum;

        //道場は次のステージの技を使うためステージ+1になっているため表示は下げる
        if (SceneManager.GetActiveScene().name == "BossScene" && Reference.Instance.isDojo)
        {
            stageNum--;
        }

        debugPanelButtonStage.SetView(stageNum - 1);

        debugPanelButtonStage.buttonRun.onClick.AddListener(() => { OnClickChangeStage(false, false); });
        debugPanelButtonStage.UpdateView();

        buttonBossRun.onClick.AddListener(() => { OnClickChangeStage(true, false); });
        buttonDojoRun.onClick.AddListener(() => { OnClickChangeStage(true, true); });

        buttonDestroy.onClick.AddListener(() =>
        {
            foreach (var enemy in Reference.Instance.enemyList)
            {
                enemy.TakeDamage(9999, true);

                // 2回目のダメージを与える前に、敵がまだ生きているか（GameObjectがアクティブ）をチェック
                if (enemy.gameObject != null && enemy.gameObject.activeSelf)
                {
                    enemy.TakeDamage(9999, true);
                }
            }
        });
        buttonRecovery.onClick.AddListener(() =>
        {
            SaveDataManager.Hp = SaveDataManager.HpMax;
            Reference.Instance.UpdateStateView();
        });
        buttonRecovery2.onClick.AddListener(() =>
        {
            SaveDataManager.Hp = 999999;
            Reference.Instance.UpdateStateView();
        });

        buttonGo.onClick.AddListener(() =>
        {
            // スクロールがロックされている場合は動かない
            if (Reference.Instance.isScroolStop)
                return;

            // 現在のステージ位置を取得
            float currentX = Reference.Instance.stageRect.anchoredPosition.x;

            // 次の144刻み（-144の倍数）の位置を計算
            float nextSpawnX = StageManager.timing * (Mathf.FloorToInt(currentX / StageManager.timing) + 1);

            // ステージ位置を設定
            var pos = Reference.Instance.stageRect.anchoredPosition;
            pos.x = nextSpawnX;
            Reference.Instance.stageRect.anchoredPosition = pos;
        });

        slider.onValueChanged.AddListener((value) =>
        {
            canvasGroup.alpha = value;
        });
    }
    void Update()
    {
        textPos.text = $"現在値:{-Reference.Instance.stageRect.anchoredPosition.x:0.0}";
    }

    private void OnClickChangeStage(bool isBoss, bool isDojo)
    {
        var kind = debugPanelButtonStage.kind;
        var stageNum = Reference.Instance.stageDataList.List[kind].StageNum;

        if (isDojo)
        {
            SaveDataManager.Dojo = stageNum;
            SaveDataManager.NowStage = stageNum + 1;
        }
        else
        {
            SaveDataManager.Dojo = 0;
            SaveDataManager.NowStage = stageNum;
        }


        SaveDataManager.Hp = SaveDataManager.HpMax;

        if (!isBoss)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            SceneManager.LoadScene("BossScene");
        }
    }

    void UpdateView()
    {
        var kind = debugPanelButton.kind;
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        var img = enemyDataList[kind].GetImage();

        image.sprite = img.sprite;
    }

    private void OnClickPopStage()
    {
        var kind = debugPanelButton.kind;
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        var enemyPopData = enemyDataList[kind];

        stageManager.SpawnEnemy(enemyPopData.prefab, 50);
    }
}
