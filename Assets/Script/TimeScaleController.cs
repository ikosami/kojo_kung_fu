using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Time.timeScale *= 2f;
            Log();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Time.timeScale *= 0.5f;
            Log();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Time.timeScale = 1f;
            Log();
        }
    }

    public void Log()
    {
        Debug.Log("Time scale: " + Time.timeScale);
    }
}
