using TMPro;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    private float spriteChangeTimer = 0f;
    [SerializeField] private float spriteChangeInterval = 0.5f;
    [SerializeField] TextMeshProUGUI text;


    // Update is called once per frame
    void Update()
    {
        spriteChangeTimer += Time.deltaTime;
        if (spriteChangeTimer > spriteChangeInterval)
        {
            spriteChangeTimer = 0f;
            text.enabled = !text.enabled;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
