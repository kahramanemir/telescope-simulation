using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TelescopeCameraCont : MonoBehaviour
{
    public Camera telescopeCam;
    public Transform ankara;
    private Transform currentTarget;
    private Vector3 targetCameraPos;

    public float moveSpeed = 2f;
    public TMP_Dropdown planetDropdown;

    UdpClient udpReceive;
    UdpClient udpSend;
    Thread receiveThread;
    volatile bool running = false;
    public int receivePort = 5008;
    public string pythonIP = "127.0.0.1";
    public int cameraSendPort = 5005;

    private int previousPlanetIndex = 0;
    private bool receivedValidPosition = false;

    void Start()
    {
        udpSend = new UdpClient();

        if (planetDropdown != null)
        {
            planetDropdown.onValueChanged.AddListener(OnPlanetChanged);
            planetDropdown.value = 0;
            previousPlanetIndex = 0;
        }

        udpReceive = new UdpClient(receivePort);
        running = true;
        receiveThread = new Thread(ReceiveData) { IsBackground = true };
        receiveThread.Start();
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

                if (message.StartsWith("ERROR"))
                {
                    receivedValidPosition = false;
                    Debug.LogWarning(message);
                }
                else if (message.StartsWith("AZ"))
                {
                    receivedValidPosition = true;
                }
            }
            catch { if (!running) break; }
        }
    }

    void Update()
    {
        if (currentTarget != null && receivedValidPosition)
        {
            telescopeCam.transform.position = Vector3.Lerp(
                telescopeCam.transform.position,
                targetCameraPos,
                Time.deltaTime * moveSpeed
            );
            telescopeCam.transform.LookAt(currentTarget.position);
        }
    }

    void OnPlanetChanged(int index)
    {
        if (planetDropdown == null) return;
        string planetName = planetDropdown.options[index].text;
        if (planetName == "Cisim Secin") return;

        receivedValidPosition = false;

        GameObject planetObj = GameObject.Find(planetName);
        if (planetObj != null)
        {
            currentTarget = planetObj.transform;
            UpdateTargetCameraPos();
            previousPlanetIndex = index;

            SendSelectionToPython(planetName);
        }
    }

    void UpdateTargetCameraPos()
    {
        if (currentTarget != null && ankara != null)
        {
            Vector3 ankaraToTarget = (currentTarget.position - ankara.position).normalized;
            float planetRadius = Mathf.Max(currentTarget.localScale.x,
                                           currentTarget.localScale.y,
                                           currentTarget.localScale.z);
            targetCameraPos = currentTarget.position - ankaraToTarget * planetRadius * 4f;
        }
    }

    public void TeleportToTarget()
    {
        if (currentTarget != null)
        {
            UpdateTargetCameraPos();
            telescopeCam.transform.position = targetCameraPos;
            telescopeCam.transform.LookAt(currentTarget.position);
        }
    }

    void SendSelectionToPython(string targetName)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(targetName);
            udpSend.Send(data, data.Length, pythonIP, cameraSendPort);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Python'a gönderme hatası: " + e.Message);
        }
    }

    void OnApplicationQuit() => StopUDP();
    void OnDestroy() => StopUDP();
    void StopUDP()
    {
        if (!running) return;
        running = false;
        try { udpReceive?.Close(); } catch { }
        try { udpSend?.Close(); } catch { }
        if (receiveThread != null && receiveThread.IsAlive) receiveThread.Join(200);
    }
}