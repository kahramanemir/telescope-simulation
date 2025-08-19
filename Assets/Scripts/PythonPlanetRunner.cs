using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class PlanetSocketUpdater : MonoBehaviour
{
    public Dictionary<string, GameObject> planets;
    public string host = "127.0.0.1";
    public int port = 65432;

    private Thread clientThread;
    private TcpClient client;
    private NetworkStream stream;
    private bool running = true;
    private string buffer = "";

    void Start()
    {
        planets = new Dictionary<string, GameObject>()
        {
            {"sun", GameObject.Find("Sun")},
            {"mercury", GameObject.Find("Mercury")},
            {"venus", GameObject.Find("Venus")},
            {"earth", GameObject.Find("Earth")},
            {"moon", GameObject.Find("Moon")},
            {"mars", GameObject.Find("Mars")},
            {"jupiter", GameObject.Find("Jupiter")},
            {"saturn", GameObject.Find("Saturn")},
            {"uranus", GameObject.Find("Uranus")},
            {"neptune", GameObject.Find("Neptune")},
            {"polaris", GameObject.Find("Polaris")},
            {"antares", GameObject.Find("Antares")},
            {"capella", GameObject.Find("Capella")},
            {"spica", GameObject.Find("Spica")},
            {"ankara", GameObject.Find("Ankara")}
        };

        clientThread = new Thread(SocketListener);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    void SocketListener()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            byte[] recvBuffer = new byte[4096];

            while (running)
            {
                int bytesRead = stream.Read(recvBuffer, 0, recvBuffer.Length);
                if (bytesRead > 0)
                {
                    buffer += Encoding.UTF8.GetString(recvBuffer, 0, bytesRead);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Socket error: " + e.Message);
        }
    }

    void Update()
    {
        while (buffer.Contains("\n"))
        {
            int idx = buffer.IndexOf("\n");
            string json = buffer.Substring(0, idx).Trim();
            buffer = buffer.Substring(idx + 1);

            if (string.IsNullOrEmpty(json)) continue;

            try
            {
                var data = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
                var positions = data["positions"] as Dictionary<string, object>;

                foreach (var kvp in positions)
                {
                    string name = kvp.Key.ToLower();
                    if (!planets.ContainsKey(name)) continue;

                    var posDict = kvp.Value as Dictionary<string, object>;
                    float x = Convert.ToSingle(posDict["x"]) * 10f;
                    float y = Convert.ToSingle(posDict["y"]) * 10f;
                    float z = Convert.ToSingle(posDict["z"]) * 10f;

                    planets[name].transform.position = new Vector3(x, y, z);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("JSON parse error: " + e.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        stream?.Close();
        client?.Close();
        clientThread?.Abort();
    }
}