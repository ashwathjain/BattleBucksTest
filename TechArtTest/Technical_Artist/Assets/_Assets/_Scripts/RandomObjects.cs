using UnityEngine;

public class RandomObjects : MonoBehaviour
{
    [SerializeField]
    Vector3 localRotationMin = Vector3.zero;
    [SerializeField]
    Vector3 localRotationMax = Vector3.zero;
    [SerializeField]
    bool applyRotation = true;
    [SerializeField]
    bool applyUniformRotation = false;
    [SerializeField]
    Vector3 localScaleMin = Vector3.zero;
    [SerializeField]
    Vector3 localScaleMax = Vector3.zero;
    [SerializeField]
    bool applyUniformScale = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        if (applyRotation)
        {
            if (applyUniformRotation)
            {
                float r = Random.Range(localRotationMin.x, localRotationMax.x);
                transform.localRotation = Quaternion.Euler(new Vector3(r, r, r));
            }
            else
            {
                transform.localRotation = Quaternion.Euler(new Vector3(
                    Random.Range(localRotationMin.x, localRotationMax.x),
                    Random.Range(localRotationMin.y, localRotationMax.y),
                    Random.Range(localRotationMin.z, localRotationMax.z)
                ));
            }
        }

        if (applyUniformScale)
        {
            float s = Random.Range(localScaleMin.x, localScaleMax.x);
            transform.localScale = new Vector3(s, s, s);
        }
        else
        {
            transform.localScale = new Vector3(
                Random.Range(localScaleMin.x, localScaleMax.x),
                Random.Range(localScaleMin.y, localScaleMax.y),
                Random.Range(localScaleMin.z, localScaleMax.z)
            );
        }
        
    }

}
