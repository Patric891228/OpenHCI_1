using UnityEngine;

public class RaiseHandManager : MonoBehaviour
{
    public Transform leftShoulder;
    // public Transform rightShoulder;
    public float rotationAngle = 90f;  
    private float rotationTime = 1f;  
    private float timer = 0f;         
    private bool raising = true;      

    public float controlVariable = 0f;

    void Start()
    {
        if (leftShoulder == null)
        {
            Debug.LogError("Shoulder transforms not assigned.");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        float angle;
        if (raising) {
            angle = Mathf.Lerp(0, rotationAngle, timer / rotationTime);
        }
        else {
            angle = Mathf.Lerp(rotationAngle, 0, timer / rotationTime);
        }

        if (leftShoulder != null)
        {
            leftShoulder.localRotation = Quaternion.Euler(new Vector3(-angle, 0, 0));
        }

        if (timer >= rotationTime) {
            timer = 0f;
            raising = !raising;
        }

        if (controlVariable > 0) {
            raising = true;
        }
        else if (controlVariable < 0) {
            raising = false;
        }
    }

    public void SetControlVariable(float value) {
        Debug.Log("Raise hand");
        controlVariable = value;
        Debug.Log("Put Down hand");
    }
}
