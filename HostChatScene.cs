using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using SFML.System;
using SFML.Window;
using SFML.Graphics;

#pragma warning disable 8602
#pragma warning disable 8604

namespace Quicksilver {
    public class HostChatScene : WindowScene {
        public TcpListener Server {get; init;}
        public byte[] Password {get; init;}

        public int ClientCount {get; private set;} = 0;
        public List<long> ClientIDs {get; init;} = new List<long>();
        public List<Socket> ClientSockets {get; init;} = new List<Socket>();
        public List<Mutex> ClientMutexes {get; init;} = new List<Mutex>();
        public List<string> ClientNames {get; init;} = new List<string>();

        public List<UIElement> ActiveUIElements {get; private set;} = new List<UIElement>();

        public HostChatScene(int port, string password) {
            Server = new TcpListener(IPAddress.Any, port);
            Password = Cryptography.HashBytes(password);

            Server.Start();

            // Console.WriteLine($"Hosting Server.");
            // Console.WriteLine($"PORT: {port}");
            // Console.WriteLine($"PASSWORD: {password}");
            
            Thread tPoll = new Thread(PollForClients);
            tPoll.Start();
        }

        public void PollForClients() {
            while (true) {
                try {
                    Socket socket = Server.AcceptSocket();
                    // Console.WriteLine("Detected new Client.");

                    byte[] keyCheck = new byte[Program.Key.Length];
                    int keyLength = socket.Receive(keyCheck);
                    bool keyBroken = false;
                    if (keyLength != Program.Key.Length) {
                        // Console.WriteLine("Key failure.");
                        socket.Send([0]);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        keyBroken = true;
                        continue;
                    }
                    for (int i = 0; i < Program.Key.Length; ++i) {
                        if (keyCheck[i] != Program.Key[i]) {
                            // Console.WriteLine("Key failure.");
                            socket.Send([0]);
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            keyBroken = true;
                            break;
                        }
                    }
                    if (keyBroken) { continue; }
                    socket.Send([1]);

                    byte[] passwordBuffer = new byte[256];
                    int passwordLength = socket.Receive(passwordBuffer);
                    byte[] hash = Cryptography.HashBytes(passwordBuffer[..passwordLength]);
                    bool passwordBroken = false;
                    if (hash.Length != Password.Length) {
                        // Console.WriteLine("Password failure.");
                        socket.Send([0]);
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        continue;
                    }
                    for (int i = 0; i < Password.Length; ++i) {
                        if (hash[i] != Password[i]) {
                            // Console.WriteLine("Password failure.");
                            socket.Send([0]);
                            socket.Shutdown(SocketShutdown.Both);
                            socket.Close();
                            passwordBroken = true;
                            break;
                        }
                    }
                    if (passwordBroken) { continue; }
                    socket.Send([1]);
                    
                    byte[] idBuffer = new byte[8];
                    int idLength = socket.Receive(idBuffer);
                    long id = BitConverter.ToInt64(idBuffer);

                    byte[] nameBuffer = new byte[256];
                    int nameLength = socket.Receive(nameBuffer);
                    string name = Encoding.UTF8.GetString(nameBuffer[..nameLength]);

                    // Console.WriteLine($"{name} ({id}) has connected.");

                    socket.Send([(byte)OpCode.InitUserList]);
                    for (int i = 0; i < ClientCount; ++i) {
                        socket.Send(BitConverter.GetBytes(ClientIDs[i]));
                        socket.Send(Encoding.UTF8.GetBytes(ClientNames[i]));
                        socket.Send([1]);
                    }
                    socket.Send([0,0,0,0,0,0,0,0]);

                    Mutex mutex = new Mutex(false);
                    ClientCount += 1;
                    ClientIDs.Add(id);
                    ClientNames.Add(name);
                    ClientMutexes.Add(mutex);
                    ClientSockets.Add(socket);

                    for (int i = 0; i < ClientCount; ++i) {
                        int ci = i;
                        Thread tSend = new Thread(() => {
                            ClientMutexes[ci].WaitOne();
                            ClientSockets[ci].Send([(byte)OpCode.NoticeConnect]);
                            ClientSockets[ci].Send(BitConverter.GetBytes(id));
                            ClientSockets[ci].Send(Encoding.UTF8.GetBytes(name));
                            ClientMutexes[ci].ReleaseMutex();
                        });
                        tSend.Start();
                    }
                    
                    Thread tHandleClient = new Thread(() => HandleClient(id, name, mutex, socket));
                    tHandleClient.Start();
                } catch {
                    // Console.WriteLine("Connection error while attempting to connect client.");
                    // Console.WriteLine(err.Message);
                    // Console.WriteLine(err.StackTrace);
                }
            }
        }

        public void HandleClient(long id, string name, Mutex mutex, Socket socket) {
            bool connected = true;
            while (connected) {
                byte[] opBuffer = new byte[1];
                int opLength = socket.Receive(opBuffer);
                if (opLength != 1) { continue; }
                OpCode op = (OpCode)opBuffer[0];
                
                mutex.WaitOne();
                
                // Console.WriteLine($"OPCODE RECEIVED: {op}");

                switch (op) {
                    case OpCode.Close: {
                        socket.Close();
                        for (int i = 0; i < ClientCount; ++i) {
                            if (ClientIDs[i] == id) {
                                ClientCount -= 1;
                                ClientIDs.RemoveAt(i);
                                ClientSockets.RemoveAt(i);
                                ClientMutexes.RemoveAt(i);
                                ClientNames.RemoveAt(i);
                            }
                        }

                        mutex.ReleaseMutex();

                        // Console.WriteLine($"{name} disconnected.");
                        for (int i = 0; i < ClientCount; ++i) {
                            int ci = i;
                            Thread tSend = new Thread(() => {
                                ClientMutexes[ci].WaitOne();
                                ClientSockets[ci].Send([(byte)OpCode.NoticeDisconnect]);
                                ClientSockets[ci].Send(BitConverter.GetBytes(id));
                                ClientMutexes[ci].ReleaseMutex();
                            });
                            tSend.Start();
                        }

                        connected = false;
                        break;
                    }
                    case OpCode.MessageBegin: {
                        // Console.WriteLine("Beginning message...");
                        List<byte> bytes = new List<byte>(1024);
                        while (true) {
                            byte[] msgBuffer = new byte[1];
                            socket.Receive(msgBuffer);
                            if (msgBuffer[0] == 1) { break; }
                            bytes.Add(msgBuffer[0]);
                        }

                        mutex.ReleaseMutex();

                        string msg = Encoding.UTF8.GetString(bytes.ToArray());
                        // Console.WriteLine($"{name}: {msg}");
                        for (int i = 0; i < ClientCount; ++i) {
                            int ci = i;
                            Thread tSend = new Thread(() => {
                                ClientMutexes[ci].WaitOne();
                                ClientSockets[ci].Send([(byte)OpCode.MessageBegin]);
                                ClientSockets[ci].Send(BitConverter.GetBytes(id));
                                for (int j = 0; j < bytes.Count; j += 1024) {
                                    int k = j+1024;
                                    if (k > bytes.Count) { k = bytes.Count; }
                                    ClientSockets[ci].Send(bytes[j..k].ToArray());
                                }
                                ClientSockets[ci].Send([1]);
                                ClientMutexes[ci].ReleaseMutex();
                            });
                            tSend.Start();
                        }
                        break;
                    }
                    case OpCode.FileBegin: {
                        List<byte> bytes = new List<byte>(1024);
                        while (true) {
                            byte[] msgBuffer = new byte[1];
                            int msgLength = socket.Receive(msgBuffer);
                            if (msgBuffer[0] == 1) { break; }
                            bytes.Add(msgBuffer[0]);
                        }

                        mutex.ReleaseMutex();
                        
                        for (int i = 0; i < ClientCount; ++i) {
                            int ci = i;
                            Thread tSend = new Thread(() => {
                                ClientSockets[ci].Send([(byte)OpCode.FileBegin]);
                                ClientSockets[ci].Send(BitConverter.GetBytes(id));
                                for (int j = 0; j < bytes.Count; j += 1024) {
                                    int k = j+1024;
                                    if (k > bytes.Count) { k = bytes.Count; }
                                    ClientSockets[ci].Send(bytes[j..k].ToArray());
                                }
                                ClientSockets[ci].Send([1]);
                            });
                            tSend.Start();
                        }
                        break;
                    }
                    default: {
                        mutex.ReleaseMutex();
                        break;
                    }
                }
            }
        }

        private Vector2f mousePos;
        private float width;
        private float height;

        public override void Run(RenderWindow window) {
            mousePos = window.MapPixelToCoords(Mouse.GetPosition(window));
            width = window.GetView().Size.X;
            height = window.GetView().Size.Y;

            foreach (UIElement element in ActiveUIElements) {
                element.Run(window, mousePos);
            }
        }

        public override void OnClose() {
            foreach (UIElement element in ActiveUIElements) {
                element.OnClose();
            }
            Thread[] tCloses = new Thread[ClientCount];
            for (int i = 0; i < ClientCount; ++i) {
                int ci = i;
                tCloses[i] = new Thread(() => {
                    ClientMutexes[ci].WaitOne();
                    ClientSockets[ci].Send([(byte)OpCode.Close]);
                    ClientMutexes[ci].ReleaseMutex();
                });
                tCloses[i].Start();
            }
            for (int i = 0; i < ClientCount; ++i) {
                tCloses[i].Join();
            }
        }

        public override void OnResize(SizeEventArgs e) {
            foreach (UIElement element in ActiveUIElements) {
                element.OnResize(e);
            }
        }

        public override void OnMousePressed(MouseButtonEventArgs e) {
            foreach (UIElement element in ActiveUIElements) {
                element.OnMousePressed(mousePos, e);
            }
        }

        public override void OnMouseReleased(MouseButtonEventArgs e) {
            foreach (UIElement element in ActiveUIElements) {
                element.OnMouseReleased(mousePos, e);
            }
        }

        public override void OnKeyPressed(KeyEventArgs e) {
            foreach (UIElement element in ActiveUIElements) {
                element.OnKeyPressed(e);
            }
        }
    }
}