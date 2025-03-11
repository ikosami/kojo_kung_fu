using UnityEngine;

public class PausePanel : MonoBehaviour
{
    [SerializeField] GameObject pauseText;
    float timer = 0;

    // Update is called once per frame
    void Update()
    {
        if (!Reference.Instance.isPause)
        {
            timer = 1;
            pauseText.SetActive(false);
        }
        else
        {
            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                pauseText.SetActive(!pauseText.activeSelf);
                timer = 0;
            }
        }
    }
}
