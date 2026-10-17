using UnityEngine;

public class RobotController : MonoBehaviour
{
    public float linearSpeed = 5.0f;
    public float angularSpeed = 100.0f;

    void Update()
    {
        float move = Input.GetAxis("Vertical") * linearSpeed * Time.deltaTime;
        float turn = Input.GetAxis("Horizontal") * angularSpeed * Time.deltaTime;

        transform.Translate(0, 0, move);
        transform.Rotate(0, turn, 0);
    }
}