using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using IO.Swagger.Model;
using kcp2k;
using Snakegame;
using UnityEngine;

public enum Cmd : ushort
{
    CmdGameUpdate = 1, // ��Ϸ״̬����
}

public class MyTcpClient : MonoBehaviour
{
    private Thread clientThread;
    private TcpClient client;
    private NetworkStream stream;
    private ConcurrentQueue<GameStateUpdate> gameStateUpdates = new ConcurrentQueue<GameStateUpdate>();
    public SnakeHead snakeHead;
    public GameController controller;

    void Start()
    {
        clientThread = new Thread(ConnectToServer);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    void ConnectToServer()
    {
        client = new TcpClient("127.0.0.1", 8082);
        stream = client.GetStream();
        Debug.Log("���ӷ������ɹ�");

        Thread receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        try
        {
            byte[] headerBytes = new byte[6];  // ǰ6���ֽڰ����˳��Ⱥ�������
            Debug.Log("��ʼ��������...");
            while (ReadFull(stream, headerBytes, headerBytes.Length))
            {
                // ע�⣺BitConverterĬ�ϰ���ϵͳ���ֽ��򣬴����WindowsϵͳΪС���ֽ���
                int messageLength = BitConverter.ToInt32(headerBytes, 0) - 2;  // �ܳ��ȼ�ȥ2�ֽ�������ĳ���
                if (messageLength > 1024 * 1024) // ���ú������󳤶����ƣ�����1MB
                {
                    Debug.LogError("��Ϣ�����쳣�����������ݽ�������" + messageLength);
                    continue; // �������ѭ��
                }

                ushort cmdValue = BitConverter.ToUInt16(headerBytes, 4);
                Cmd cmd = (Cmd)cmdValue;  // ��ȡ������

                Debug.Log($"���յ���Ϣ�������룺{cmd}, ��Ϣ���ȣ�{messageLength}");

                byte[] data = new byte[messageLength];
                if (ReadFull(stream, data, messageLength))
                {
                    Debug.Log("������Ϣ�ѽ��գ�������...");
                    switch (cmd)
                    {
                        case Cmd.CmdGameUpdate:
                            HandleGameUpdate(data);
                            Debug.Log("��Ϸ״̬�����Ѵ���");
                            break;
                        default:
                            Debug.LogError("�յ�δ֪������: " + cmd);
                            break;
                    }
                }
                else
                {
                    Debug.LogError("��Ϣ���ղ����������ݿ������𻵡�");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("�������ݹ����з����쳣: " + ex.Message);
        }
        finally
        {
            Debug.Log("���������߳̽�����");
        }
    }

    void HandleGameUpdate(byte[] data)
    {
        try
        {
            Snakegame.GameStateUpdate gameStateUpdate = Snakegame.GameStateUpdate.Parser.ParseFrom(data);
            gameStateUpdates.Enqueue(gameStateUpdate);
            Debug.Log("�յ���Ϸ����״̬");
        }
        catch (Exception ex)
        {
            Debug.LogError("HandleGameUpdate failed: " + ex.Message);
        }
    }

    bool ReadFull(NetworkStream stream, byte[] buffer, int size)
    {
        int read = 0;
        while (read < size)
        {
            int remaining = size - read;
            int received = stream.Read(buffer, read, remaining);
            if (received == 0)
            {
                Debug.LogError("Socket closed");
                return false;
            }
            read += received;
        }
        return true;
    }


    void Update()
    {
        while (gameStateUpdates.TryDequeue(out GameStateUpdate gameStateUpdate))
        {
            if (controller != null)
            {
                controller.UpdateGameState(gameStateUpdate);
            }
            else
            {
                Debug.LogError("GameController instance is not set.");
            }
        }

        CheckUserInput();
    }

    void CheckUserInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
            SendMoveMessage(PlayerAction.MoveUp);
        if (Input.GetKeyDown(KeyCode.A))
            SendMoveMessage(PlayerAction.MoveLeft);
        if (Input.GetKeyDown(KeyCode.S))
            SendMoveMessage(PlayerAction.MoveDown);
        if (Input.GetKeyDown(KeyCode.D))
            SendMoveMessage(PlayerAction.MoveRight);
        if (Input.GetKeyDown(KeyCode.Space))
            SendMoveMessage(PlayerAction.SpeedUp);
    }

    void SendMoveMessage(PlayerAction action)
    {
        if (stream != null)
        {
            ClientAction message = new ClientAction { Action = action };
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms);
                byte[] buffer = ms.ToArray();
                stream.Write(buffer, 0, buffer.Length);
                Debug.Log("������Ϸ���� " + action);
            }
        }
    }

    void OnDestroy()
    {
        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
        if (clientThread != null)
        {
            clientThread.Abort();
        }
    }
}