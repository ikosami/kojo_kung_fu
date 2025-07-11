﻿using TMPro;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.5f;
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] GameObject pushSpacePanel;
    [SerializeField] GameObject stageSelectPanel;

    [SerializeField] TextMeshProUGUI stageText;


    int state = 0;
    int stage = 1;
    int maxStage;

    private void Start()
    {
        maxStage = SaveDataManager.MaxStage;
        stage = maxStage;
        stageText.text = stage.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case 0:

                spriteChangeTimer += Time.deltaTime;
                if (spriteChangeTimer > spriteChangeInterval)
                {
                    spriteChangeTimer = 0f;
                    text.enabled = !text.enabled;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    pushSpacePanel.gameObject.SetActive(false);
                    stageSelectPanel.gameObject.SetActive(true);
                    state++;
                }
                break;
            case 1:

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    stage--;
                    stage = Mathf.Clamp(stage, 1, maxStage);
                    stageText.text = stage.ToString();
                }
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    stage++;
                    stage = Mathf.Clamp(stage, 1, maxStage);
                    stageText.text = stage.ToString();
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SaveDataManager.NowStage = stage;
                    SaveDataManager.Dojo = 0;
                    UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");

                }
                break;

        }
    }
}
