using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameoverPanel : MonoBehaviour
{
    [SerializeField] GameObject panel;
    float timer = 0;

    // Update is called once per frame
    void Update()
    {
        if (!Reference.Instance.isGameOverEnd)
        {
            timer = 0;
            panel.SetActiveIfChanged(false);
        }
        else
        {
            panel.SetActiveIfChanged(true);
            timer += Time.deltaTime;
            //if (timer > 0.5f)
            //{
            //    pauseText.SetActive(!pauseText.activeSelf);
            //    timer = 0;
            //}
            if (timer > 5)
            {
                SceneManager.LoadScene("LoadScene");
            }
        }
    }
}
