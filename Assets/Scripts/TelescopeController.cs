using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;

public class TelescopeController : MonoBehaviour
{
    UdpClient udpReceive;
    Thread receiveThread;
    volatile bool running = false;

    public int receivePort = 5006;
    public TMP_Text statusText;

    float currentAz = 0f;
    float currentAlt = 0f;

    public Transform telescopeBody;
    public Transform azimuthCylinder;
    public Transform altitudeCylinder;

    public float rotationSpeed = 30f;
    public float cylinderSpinSpeed = 500f;

    const float STEPS_PER_REVOLUTION = 200.0f;
    const float GEAR_RATIO = 10.0f;
    const float MICROSTEPPING = 16.0f;

    float prevAzAngle;
    float prevAltAngle;

    private bool isTargetObservable = true;
    string errorMessage = "";

    void Start()
    {
        udpReceive = new UdpClient(receivePort);
        running = true;
        receiveThread = new Thread(ReceiveData) { IsBackground = true };
        receiveThread.Start();

        if (telescopeBody)
        {
            prevAzAngle = telescopeBody.localEulerAngles.y;
            prevAltAngle = telescopeBody.localEulerAngles.x;
        }
    }

    void ReceiveData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, receivePort);
        while (running)
        {
            try
            {
                byte[] data = udpReceive.Receive(ref remoteEP);
                string message = Encoding.UTF8.GetString(data);

                if (message.StartsWith("AZ"))
                {
                    string[] parts = message.Split(' ');
                    currentAz = float.Parse(parts[0].Split(':')[1], CultureInfo.InvariantCulture);
                    currentAlt = float.Parse(parts[1].Split(':')[1], CultureInfo.InvariantCulture);
                    isTargetObservable = true;
                    errorMessage = "";
                }
                else if (message.StartsWith("ERROR"))
                {
                    errorMessage = "<color=red>Hedef ufkun altında veya gözlemlenemiyor.</color>";
                    isTargetObservable = false;
                }
            }
            catch { if (!running) break; }
        }
    }

    void Update()
    {
        if (!telescopeBody) return;

        float curAz = telescopeBody.localEulerAngles.y;
        float curAlt = telescopeBody.localEulerAngles.x;

        float targetAz = currentAz;
        float targetAlt = currentAlt;

        float azimuthStepsToTarget = Mathf.Abs(Mathf.DeltaAngle(curAz, targetAz)) * STEPS_PER_REVOLUTION * GEAR_RATIO * MICROSTEPPING / 360f;
        float altitudeStepsToTarget = Mathf.Abs(Mathf.DeltaAngle(curAlt, targetAlt)) * STEPS_PER_REVOLUTION * GEAR_RATIO * MICROSTEPPING / 360f;

        float azimuthTurnsToTarget = azimuthStepsToTarget / (STEPS_PER_REVOLUTION * MICROSTEPPING);
        float altitudeTurnsToTarget = altitudeStepsToTarget / (STEPS_PER_REVOLUTION * MICROSTEPPING);

        float stepAz = Mathf.MoveTowardsAngle(curAz, targetAz, rotationSpeed * Time.deltaTime);
        float stepAlt = Mathf.MoveTowardsAngle(curAlt, targetAlt, rotationSpeed * Time.deltaTime);

        telescopeBody.localEulerAngles = new Vector3(stepAlt, stepAz, 0);

        float azDelta = Mathf.DeltaAngle(prevAzAngle, stepAz);
        float altDelta = Mathf.DeltaAngle(prevAltAngle, stepAlt);

        if (Mathf.Abs(azDelta) > 0.01f && azimuthCylinder)
            azimuthCylinder.Rotate(Vector3.up * Mathf.Sign(azDelta) * cylinderSpinSpeed * Time.deltaTime, Space.Self);

        if (Mathf.Abs(altDelta) > 0.01f && altitudeCylinder)
            altitudeCylinder.Rotate(Vector3.up * Mathf.Sign(altDelta) * cylinderSpinSpeed * Time.deltaTime, Space.Self);

        prevAzAngle = stepAz;
        prevAltAngle = stepAlt;

        if (statusText)
        {
            if (!isTargetObservable && !string.IsNullOrEmpty(errorMessage))
            {
                statusText.text = errorMessage;
            }
            else
            {
                statusText.text =
                    $"Azimuth: {stepAz:F1}° → {targetAz:F1}° | Gerekli: {azimuthStepsToTarget:F0} steps ({azimuthTurnsToTarget:F2} turns)\n" +
                    $"Altitude: {stepAlt:F1}° → {targetAlt:F1}° | Gerekli: {altitudeStepsToTarget:F0} steps ({altitudeTurnsToTarget:F2} turns)";
            }
        }
    }

    void OnApplicationQuit() => StopNetworking();
    void OnDestroy() => StopNetworking();
    void StopNetworking()
    {
        if (!running) return;
        running = false;
        try { udpReceive?.Close(); } catch { }
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Join(200);
    }
}