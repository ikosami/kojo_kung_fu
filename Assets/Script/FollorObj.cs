using UnityEngine;

public class FollorObj : MonoBehaviour
{
    [SerializeField] GameObject targetObj;

    void LateUpdate()
    {
        transform.position = targetObj.transform.position;
    }
}
