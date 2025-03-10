﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatSystem;

namespace ChatSystem
{
    class main
    {
        static ChatSystem chatSystem;
        static bool isBot = false;
        static Random rand = new Random();
        const Int32 portNo = 11000;
        const string EOF = "<EOF>";
        static readonly int maxLength = 200 + EOF.Length;
        static ChatSystem.ConnectMode connectMode;
        enum FunctionMode { chat, bot, janken, shiritori };
        static FunctionMode functionMode = FunctionMode.chat;

        static void Main(string[] args)
        {
            chatSystem = new ChatSystem(maxLength);
            Console.WriteLine($"this hostName is {chatSystem.hostName}.");
            functionMode = SelectFunction();
            connectMode = SelectMode();
            switch (functionMode)
            {
                case FunctionMode.chat:
                    InChat();
                    break;
                case FunctionMode.bot:
                    InChatBot();
                    break;
                case FunctionMode.janken:
                    InJanken();
                    break;
                default:
                    Console.WriteLine("not suported");
                    break;
            }
        }
        static FunctionMode SelectFunction()
        {
            Console.WriteLine("Select Function\n0= chat\n1=bot\n2=janken\n3=shiritori ");
            int select = int.Parse(Console.ReadLine());
            FunctionMode[] function = { FunctionMode.chat, FunctionMode.bot, FunctionMode.janken, FunctionMode.shiritori };
            return function[select];
        }
        static ChatSystem.ConnectMode SelectMode()
        {
            ChatSystem.ConnectMode connectMode = ChatSystem.ConnectMode.host;
            Console.Write("Select Mode: 0=Host,1=Client\n");
            int select = int.Parse(Console.ReadLine());
            switch (select)
            {
                case 0: //Host
                    Console.WriteLine("Running Host mode");
                    InitializeHost();
                    connectMode = ChatSystem.ConnectMode.host;
                    break;
                case 1: //Client
                    Console.WriteLine("Running Client mode");
                    InitializeClient();
                    connectMode = ChatSystem.ConnectMode.client;
                    break;
                default:
                    Console.WriteLine("ERROR undefind");
                    break;
            }
            return connectMode;
        }
        static void InitializeHost()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(chatSystem.hostName);
            foreach (var addresslist in ipHostInfo.AddressList)
            {
                Console.WriteLine($"found own address:{addresslist.ToString()}");
            }
            Console.Write($"Select address to listen(0 - {ipHostInfo.AddressList.Length - 1}):");
            IPAddress ipAddress = ipHostInfo.AddressList[int.Parse(Console.ReadLine())];
            ChatSystem.EResult re = chatSystem.InitializeHost(ipAddress, portNo);
            if (re != ChatSystem.EResult.success)
            {
                Console.WriteLine($"failed to initialize,ERROR={re.ToString()}");
            }
        }
        static void InitializeClient()
        {
            Console.Write("Input IP address to connect:");
            var ipAddress = IPAddress.Parse(Console.ReadLine());
            ChatSystem.EResult re = chatSystem.InitializeClient(ipAddress, portNo);
            if (re == ChatSystem.EResult.success)
            {
                Console.WriteLine($"Connected host {ipAddress.ToString()}");
            }
            else
            {
                Console.WriteLine($"failed to connect to host,ERROR={chatSystem.resultMessage}");
            }
        }
        static void InChatBot()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            string received = string.Empty;
            string replay = string.Empty;
            string[] randamST = { "わかる～", "それな？", "まあそんな日もあるっしょ", "やるやん", "お疲れ～" };
            while (true)
            {
                if (turn)
                {   // 受信
                    received = string.Empty;
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    string inputSt = string.Empty;
                    Console.Write("送るメッセージ：");
                    if (connectMode == ChatSystem.ConnectMode.host)
                    {
                        if (received.Contains("こんにちは") || received.Contains("やっほー") || received.Contains("Hello") == true)
                        {
                            inputSt = ("おはよ！");
                            Console.WriteLine(inputSt);
                        }
                        else if (received.Contains("おやすみ") || received.Contains("おやすみなさい") || received.Contains("Good Night") == true)
                        {
                            inputSt = ("おやすみ！良い夢見てね～");
                            Console.WriteLine(inputSt);
                        }
                        else if (received.Contains("今何してる？") || received.Contains("今何してた？") || received.Contains("What aer you doing now?") == true)
                        {
                            inputSt = ("ご飯食べてたよ");
                            Console.WriteLine(inputSt);
                        }
                        else if (received.Contains("課題終わった？") || received.Contains("課題終わってる？") || received.Contains("Have you finished the task?") == true)
                        {
                            inputSt = ("もちろん終わってないよ");
                            Console.WriteLine(inputSt);
                        }
                        else if(received.Contains("好き")||received.Contains("好きだよ")||received.Contains("I love you") == true)
                        {
                            inputSt = ("私はきら～い");
                            Console.WriteLine(inputSt);
                        }
                        else
                        {
                            Random RanObj = new Random();
                            int RanNum = RanObj.Next(0, 5);
                            inputSt = (randamST[RanNum]);
                            Console.WriteLine(inputSt);
                        }
                    }
                    else
                    {   // Client
                        inputSt = Console.ReadLine();    // 入力文字で送信
                        if (inputSt.Length > maxLength)
                        {
                            inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                        }
                    }

                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static void InChat()
        {
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                }
                else
                {   // 送信
                    Console.Write("送るメッセージ：");
                    string inputSt = Console.ReadLine();    // 入力文字で送信
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }

        static void InJanken()
        {
            string RHand = string.Empty;
            string SHand = string.Empty;
            ChatSystem.Buffer buffer = new ChatSystem.Buffer(maxLength);
            bool turn = (connectMode == ChatSystem.ConnectMode.host);
            while (true)
            {
                if (turn)
                {   // 受信
                    buffer = new ChatSystem.Buffer(maxLength);
                    ChatSystem.EResult re = chatSystem.Receive(buffer);
                    if (re == ChatSystem.EResult.success)
                    {
                        string received = Encoding.UTF8.GetString(buffer.content).Replace(EOF, "");
                        int l = received.Length;
                        if (received[0] != '\0')
                        {   // 正常にメッセージを受信
                            Console.WriteLine($"受信メッセージ：{received}");
                            RHand = received;
                        }
                        else
                        {   // 正常に終了を受信
                            Console.WriteLine("相手から終了を受信");
                            break;
                        }
                    }
                    else
                    {   //　受信エラー
                        Console.WriteLine($"受信エラー：{chatSystem.resultMessage} ");
                        break;
                    }
                    if (RHand.Contains("グー") && SHand.Contains("チョキ") == true)
                    {
                        Console.WriteLine("あなたの勝ち！");
                    }
                    else if (RHand.Contains("チョキ") && SHand.Contains("パー") == true)
                    {
                        Console.WriteLine("あなたの勝ち！");
                    }
                    else if (RHand.Contains("パー") && SHand.Contains("グー") == true)
                    {
                        Console.WriteLine("あなたの勝ち！");
                    }
                    else if (RHand.Contains("グー") && SHand.Contains("パー") == true)
                    {
                        Console.WriteLine("あなたの負け！");
                    }
                    else if (RHand.Contains("チョキ") && SHand.Contains("グー") == true)
                    {
                        Console.WriteLine("あなたの負け！");
                    }
                    else if (RHand.Contains("パー") && SHand.Contains("チョキ") == true)
                    {
                        Console.WriteLine("あなたの負け！");
                    }
                    else
                    {
                        Console.WriteLine("あいこ！");
                    }
                }
                else
                {   // 送信

                    Console.Write("送るメッセージ：");
                    string inputSt = Hands();    // 入力文字で送信
                    if (inputSt.Length > maxLength)
                    {
                        inputSt = inputSt.Substring(0, maxLength - EOF.Length);
                    }
                    inputSt += EOF;
                    buffer.content = Encoding.UTF8.GetBytes(inputSt);
                    buffer.length = buffer.content.Length;
                    ChatSystem.EResult re = chatSystem.Send(buffer);
                    if (re != ChatSystem.EResult.success)
                    {
                        Console.WriteLine($"送信エラー：{re.ToString()} Error code: {chatSystem.resultMessage}");
                        break;
                    }
                }
                turn = !turn;
            }
            chatSystem.ShutDownColse();
        }
        
        static string Hands()
        {
            string select = "";
            while (true)
            {
                if (isBot)
                {
                    select = rand.Next(0, 3).ToString();
                    Console.WriteLine(select);
                }
                else
                {
                    select = Console.ReadLine();
                }

                if (select == "0")
                {
                    return "グー";
                }
                else if (select == "1")
                {
                    return "チョキ";
                }
                else if (select == "2")
                {
                    return "パー";
                }
                else
                {
                    return "\0";
                }
            }
        }
    }
}
