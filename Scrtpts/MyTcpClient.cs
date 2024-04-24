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
    CmdGameUpdate = 1, // 游戏状态更新
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
        Debug.Log("连接服务器成功");

        Thread receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        try
        {
            byte[] headerBytes = new byte[6];  // 前6个字节包含了长度和命令码
            Debug.Log("开始接收数据...");
            while (ReadFull(stream, headerBytes, headerBytes.Length))
            {
                // 注意：BitConverter默认按照系统的字节序，大多数Windows系统为小端字节序
                int messageLength = BitConverter.ToInt32(headerBytes, 0) - 2;  // 总长度减去2字节命令码的长度
                if (messageLength > 1024 * 1024) // 设置合理的最大长度限制，比如1MB
                {
                    Debug.LogError("消息长度异常，可能是数据解析错误：" + messageLength);
                    continue; // 跳过这次循环
                }

                ushort cmdValue = BitConverter.ToUInt16(headerBytes, 4);
                Cmd cmd = (Cmd)cmdValue;  // 读取命令码

                Debug.Log($"接收到消息，命令码：{cmd}, 消息长度：{messageLength}");

                byte[] data = new byte[messageLength];
                if (ReadFull(stream, data, messageLength))
                {
                    Debug.Log("完整消息已接收，处理中...");
                    switch (cmd)
                    {
                        case Cmd.CmdGameUpdate:
                            HandleGameUpdate(data);
                            Debug.Log("游戏状态更新已处理。");
                            break;
                        default:
                            Debug.LogError("收到未知命令码: " + cmd);
                            break;
                    }
                }
                else
                {
                    Debug.LogError("消息接收不完整，数据可能已损坏。");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("接收数据过程中发生异常: " + ex.Message);
        }
        finally
        {
            Debug.Log("接收数据线程结束。");
        }
    }

    void HandleGameUpdate(byte[] data)
    {
        try
        {
            Snakegame.GameStateUpdate gameStateUpdate = Snakegame.GameStateUpdate.Parser.ParseFrom(data);
            gameStateUpdates.Enqueue(gameStateUpdate);
            Debug.Log("收到游戏更新状态");
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
                Debug.Log("发送游戏操作 " + action);
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