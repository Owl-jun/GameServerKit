using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TextCore.Text;

public class NetworkManager : MonoBehaviour
{

    public static NetworkManager Instance;
    [SerializeField] public GameObject playerPrefab;

    public volatile bool isRunning = false;
    private volatile bool TCPisRunning = true;

    private TcpClient tcpClient;
    private SslStream sslStream;
    private Thread receiveThread;

    private TcpClient chatClient;
    private NetworkStream chatStream;
    private Thread chatReceiveThread;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ConnectToGameServer("127.0.0.1", 9000);
        ConnectToChatServer("127.0.0.1", 12345);
        string userId = GameManager.userId;
        string token = GameManager.token;
        SendTlpMessage(0x01, $"{userId} {token}");
        MainThreadDispatcher.Enqueue(() => SpawnMyPlayer(userId));
    }

    long pingSentTime;
    private float pingInterval = 2f;
    private float pingTimer = 0f;

    public void SendPing()
    {
        pingSentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        SendTlpMessage(0x09, pingSentTime.ToString());
    }

    void Update()
    {
        pingTimer += Time.deltaTime;
        if (pingTimer >= pingInterval)
        {
            SendPing();
            pingTimer = 0f;
        }
    }

    private void SpawnMyPlayer(string userId)
    {
        if (!playerPrefab)
        {
            Debug.LogError("playerPrefab not assigned.");
            return;
        }

        var player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.name = $"Player_{userId}";
        player.GetComponent<UnitMovement>().isLocalPlayer = true;
        var nameTag = player.transform.Find("UnitUI/HealthTracker/Name/namTag").GetComponent<TextMeshProUGUI>();
        nameTag.text = userId;
        nameTag.transform.rotation = Quaternion.LookRotation(nameTag.transform.position - Camera.main.transform.position);
        PlayerManager.Instance.Register(userId, player);
        PlayerManager.Instance.myPlayer = player;
        PlayerManager.Instance.myPlayerName = userId;
    }

    private void ConnectToGameServer(string ip, int port)
    {
        try
        {
            tcpClient = new TcpClient(ip, port);
            sslStream = new SslStream(tcpClient.GetStream(), false, (sender, cert, chain, errors) => true);
            sslStream.AuthenticateAsClient("localhost");

            receiveThread = new Thread(GameReceiveLoop) { IsBackground = true };
            receiveThread.Start();
            Debug.Log("[NetworkManager] 서버 연결 성공 및 수신 시작.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetworkManager] 서버 연결 실패: " + ex.Message);
        }
    }
    public void ConnectToChatServer(string ip, int port)
    {
        try
        {
            chatClient = new TcpClient(ip, port);
            chatStream = chatClient.GetStream();

            chatReceiveThread = new Thread(ChatReceiveLoop);
            chatReceiveThread.IsBackground = true;
            chatReceiveThread.Start();

            isRunning = true;

            Debug.Log("[Chat] 채팅 서버 연결 성공 및 수신 시작");
        }
        catch (Exception ex)
        {
            isRunning = false;
            Debug.LogError("[Chat] 채팅 서버 연결 실패: " + ex.Message);
        }
    }
    private void ChatReceiveLoop()
    {
        List<byte> recvBuffer = new List<byte>();
        byte[] temp = new byte[1024];

        try
        {
            while (isRunning && chatClient.Connected)
            {
                int bytesRead = chatStream.Read(temp, 0, temp.Length);
                if (bytesRead <= 0) break;

                recvBuffer.AddRange(temp[..bytesRead]);

                while (recvBuffer.Count >= 5)  // 최소 패킷 크기: 1 + 4
                {
                    byte opcode = recvBuffer[0];
                    int len = BitConverter.ToInt32(recvBuffer.GetRange(1, 4).ToArray(), 0);
                    len = System.Net.IPAddress.NetworkToHostOrder(len);

                    if (recvBuffer.Count < 5 + len)
                        break; // 아직 전체 payload 안 옴

                    byte[] payloadBytes = recvBuffer.GetRange(5, len).ToArray();
                    string message = Encoding.UTF8.GetString(payloadBytes);

                    // Remove processed packet
                    recvBuffer.RemoveRange(0, 5 + len);

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        ChatManager.Instance?.DisplayMessage(message);
                    });

                    Debug.Log($"[Chat 수신] opcode: {opcode}, message: {message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[Chat 수신 루프] 오류: " + ex.Message);
            isRunning = false;
        }
    }



    private void GameReceiveLoop()
    {
        while (TCPisRunning)
        {
            try
            {
                if (sslStream == null || !tcpClient.Connected)
                {
                    Debug.LogWarning("[ReceiveLoop] 연결 종료됨");
                    break;
                }
                byte[] lengthBuffer = new byte[4];
                if (sslStream.Read(lengthBuffer, 0, 4) != 4) continue;
                int packetSize = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                byte[] bodyBuffer = new byte[packetSize];
                int offset = 0;
                while (offset < packetSize)
                {
                    int read = sslStream.Read(bodyBuffer, offset, packetSize - offset);
                    if (read <= 0) throw new Exception("Connection closed");
                    offset += read;
                }

                byte opcode = bodyBuffer[0];
                string body = Encoding.UTF8.GetString(bodyBuffer, 1, packetSize - 1);
                Debug.Log($"opcode : {opcode}, body : {body}");
                MainThreadDispatcher.Enqueue(() => HandleMessage(opcode, body));
            }
            catch (Exception ex)
            {
                Debug.LogError("[ReceiveLoop] 예외 발생: " + ex);
                break;
            }
        }
    }

    private void HandleMessage(byte opcode, string body)
    {
        switch (opcode)
        {
            case 1:
                HandleLogin(body);
                break;
            case 2:
                HandleMove(body);
                break;
            case 3:
                HandleAttack(body);
                break;
            case 4:
                HandleLogout(body);
                break;
            case 9:
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long sent = long.Parse(body);
                long ping = now - sent;
                Debug.Log($"[PING] 왕복시간: {ping} ms");
                PingOverlay.Instance?.UpdatePing(ping);
                break;
            default:
                Debug.LogWarning($"[HandleMessage] 알 수 없는 opcode: {opcode}");
                break;
        }
    }

    private void HandleAttack(string body)
    {
        var parts = body.Split(' ');
        if (parts.Length != 3) 
        {
            Debug.LogWarning($"[Attack] 잘못된 패킷: {body}");
            return;
        }

        string cmd = parts[0];
        string attackerId = parts[1];
        string targetId = parts[2];

        var attacker = PlayerManager.Instance.GetPlayer(attackerId);
        var target = PlayerManager.Instance.GetPlayer(targetId);

        if (attacker == null || target == null)
        {
            Debug.LogWarning($"[Attack] 유효하지 않은 플레이어 ID: {attackerId}, {targetId}");
            return;
        }

        var attackController = attacker.GetComponent<AttackController>();
        if (attackController == null)
        {
            Debug.LogWarning("[Attack] 공격자가 AttackController를 가지지 않음");
            return;
        }

        if (cmd == "START")
        {
            attackController.targetToAttack = target.transform;
            attackController.SetAttackMaterial();
        }
        else if (cmd == "STOP")
        {
            attackController.targetToAttack = null;
            attackController.SetIdleMaterial();
        }
    }


    private void HandleLogout(string userId)
    {
        if (PlayerManager.Instance.GetPlayer(userId) == null)
            return;
        var player = PlayerManager.Instance.GetPlayer(userId);
        Destroy(player);
        PlayerManager.Instance.Deluser(userId);
    }

    private void HandleLogin(string body)
    {
        var parts = body.Split(' ');
        string userId = parts[0];
        var x = Convert.ToSingle(parts[1]);
        Debug.Log(x);
        var y = 0;
        var z = Convert.ToSingle(parts[2]);
        Debug.Log(z);

        if (userId == GameManager.userId)
            return;

        Vector3 targetPos = new Vector3(x, y, z);
        GameObject player = PlayerManager.Instance.GetPlayer(userId);
        if (player == null)
        {
            player = Instantiate(playerPrefab, targetPos, Quaternion.identity);
            player.name = $"Player_{userId}";
            player.GetComponent<UnitMovement>().isLocalPlayer = false;
            PlayerManager.Instance.Register(userId, player);
        }
        var nameTag = player.transform.Find("UnitUI/HealthTracker/Name/namTag").GetComponent<TextMeshProUGUI>();
        nameTag.text = userId;
        nameTag.transform.rotation = Quaternion.LookRotation(nameTag.transform.position - Camera.main.transform.position);

        PlayerManager.Instance.Register(userId, player);
    }

    private void HandleMove(string body)
    {
        var parts = body.Split(' ');
        if (parts.Length < 2) return;

        string userId = parts[0];
        if (userId == GameManager.userId) return;

        var coords = parts[1].Split(',');
        if (coords.Length != 3) return;

        if (!float.TryParse(coords[0], out float x) ||
            !float.TryParse(coords[1], out float y) ||
            !float.TryParse(coords[2], out float z)) return;

        Vector3 targetPos = new Vector3(x, y, z);

        GameObject player = PlayerManager.Instance.GetPlayer(userId);
        if (player == null)
        {
            player = Instantiate(playerPrefab, targetPos, Quaternion.identity);
            player.name = $"Player_{userId}";
            player.GetComponent<UnitMovement>().isLocalPlayer = false;
            PlayerManager.Instance.Register(userId, player);
        }

        var agent = player.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
            agent.SetDestination(targetPos);
    }

    public void SendTlpMessage(byte opcode, string payload)
    {
        if (sslStream == null) return;

        byte[] bodyBytes = Encoding.UTF8.GetBytes(payload);
        byte[] fullPacket = new byte[1 + bodyBytes.Length];
        fullPacket[0] = opcode;
        Buffer.BlockCopy(bodyBytes, 0, fullPacket, 1, bodyBytes.Length);

        byte[] lengthPrefix = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(fullPacket.Length));
        byte[] packet = new byte[4 + fullPacket.Length];
        Buffer.BlockCopy(lengthPrefix, 0, packet, 0, 4);
        Buffer.BlockCopy(fullPacket, 0, packet, 4, fullPacket.Length);

        sslStream.Write(packet);
        sslStream.Flush();
    }

    public void SendChatMessage(byte opcode, string payload)
    {
        if (sslStream == null) return;

        byte[] bodyBytes = Encoding.UTF8.GetBytes(payload);
        byte[] fullPacket = new byte[bodyBytes.Length];
        
        Buffer.BlockCopy(bodyBytes, 0, fullPacket, 0, bodyBytes.Length);

        byte[] lengthPrefix = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(fullPacket.Length));
        byte[] packet = new byte[5 + fullPacket.Length];
        packet[0] = opcode;
        Buffer.BlockCopy(lengthPrefix, 0, packet, 1, 4);
        Buffer.BlockCopy(fullPacket, 0, packet, 5, fullPacket.Length);
        Debug.Log($"채팅전송 - Opcode: {opcode}, Length: {fullPacket.Length}, Payload: {payload}");
        chatStream.Write(packet);
        chatStream.Flush();
    }

    private void OnApplicationQuit()
    {
        LoginManager.Instance?.SendLogoutBlocking();
        try { sslStream?.Close(); } catch { }
        try { tcpClient?.Close(); } catch { }

        try { chatStream?.Close(); } catch { }
        try { chatClient?.Close(); } catch { }

        chatReceiveThread?.Join();
        receiveThread?.Join(); 
    }
}
