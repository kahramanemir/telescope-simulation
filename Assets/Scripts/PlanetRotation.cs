using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public float rotationPeriodHours = 24f;
    public bool retrograde = false;
    public float timeStepHours = 0.005f;

    void Update()
    {
        float stepPerSecond = timeStepHours / 0.1f;
        float simHoursPerSecond = stepPerSecond;
        float degreesPerSimHour = 360f / rotationPeriodHours;
        float direction = retrograde ? -1f : 1f;

        transform.Rotate(Vector3.up, direction * degreesPerSimHour * simHoursPerSecond * Time.deltaTime, Space.Self);
    }
}