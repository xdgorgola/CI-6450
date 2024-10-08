using UnityEngine;

public class RotationLocker : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
