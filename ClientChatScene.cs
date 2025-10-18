using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;

using SFML.System;
using SFML.Window;
using SFML.Graphics;
using System;

#pragma warning disable 8602
#pragma warning disable 8604

namespace Quicksilver {
    public class ClientChatScene : WindowScene {
        public TcpClient Client {get; init;}
        public Socket Socket {get; private set;}

        public Dictionary<long, string> Usernames {get; init;} = new Dictionary<long, string>();
        
        public bool Connected {get; private set;}
        public bool Busy {get; private set;} // Implemented as a bool instead of the Mutex class because there's special behavior I need.

        public UIRichTextField MessageTextField {get; private set;}

        public List<UIElement> ActiveUIElements {get; private set;} = new List<UIElement>();

        public List<string> MessageLog {get; private set;} = new List<string>();

        public ClientChatScene(string ip, int port, string password, string username) {
            Client = new TcpClient();
            Socket = Client.Client;

            Connected = false;
            Busy = false;

            MessageTextField = new UIRichTextField(new FloatRect(0, -40, 800, 40), UIAlignment.BottomCenter, "Type a message here...", 20, () => {
                if (Connected) {
                    string message = MessageTextField.Text;
                    Thread tSend = new Thread(() => SendMessage(message));
                    tSend.Start();
                    MessageTextField.Text = "";
                }
            });

            ActiveUIElements = [
                MessageTextField
            ];

            Thread tConnect = new Thread(() => ConnectClient(ip, port, password, username));
            tConnect.Start();
        }

        public void ConnectClient(string ip, int port, string password, string username) {
            Client.Connect(ip, port);
            Socket = Client.Client;

            while (Busy) {}
            Busy = true;

            Socket.Send(Program.Key);

            byte[] keyResponseBuffer = new byte[1];
            int keyResponseLength = Socket.Receive(keyResponseBuffer);
            if (keyResponseLength != 1 || keyResponseBuffer[0] != 1) { return; }

            Socket.Send(Encoding.UTF8.GetBytes(password));

            byte[] passwordResponseBuffer = new byte[1];
            int passwordResponseLength = Socket.Receive(passwordResponseBuffer);
            if (passwordResponseLength != 1 || passwordResponseBuffer[0] != 1) { return; }

            long id = Random.Shared.NextInt64();
            Socket.Send(BitConverter.GetBytes(id));

            Socket.Send(Encoding.UTF8.GetBytes(username));

            Busy = false;

            Connected = true;

            Thread tListen = new Thread(Listen);
            tListen.Start();
        }

        public void Listen() {
            while (Connected) {
                byte[] opBuffer = new byte[1];
                OpCode op;
                
                do {
                    Socket.Receive(opBuffer);
                    op = (OpCode)opBuffer[0];
                } while (Busy);
                Busy = true;

                // Console.WriteLine($"OPCODE RECEIVED: {op}");

                switch (op) {
                    case OpCode.Close: {
                        Socket.Close();
                        Connected = false;

                        Busy = false;

                        MessageLog.Add("Server disconnected.");
                        break;
                    }
                    case OpCode.InitUserList: {
                        while (true) {
                            byte[] idBuffer = new byte[8];
                            Socket.Receive(idBuffer);
                            long id = BitConverter.ToInt64(idBuffer);

                            if (id == 0) { break; }

                            List<byte> nameBytes = new List<byte>();
                            byte[] tmpBuffer = new byte[1];
                            Socket.Receive(tmpBuffer);
                            while (tmpBuffer[0] != 1) {
                                nameBytes.Add(tmpBuffer[0]);
                                Socket.Receive(tmpBuffer);
                            }

                            Usernames.TryAdd(id, Encoding.UTF8.GetString(nameBytes.ToArray()));
                        }

                        Busy = false;
                        break;
                    }
                    case OpCode.NoticeDisconnect: {
                        byte[] idBuffer = new byte[8];
                        Socket.Receive(idBuffer);
                        long id = BitConverter.ToInt64(idBuffer);

                        string name = Usernames[id];

                        Usernames.Remove(id);

                        Busy = false;

                        MessageLog.Add($"{name} has disconnected.");
                        break;
                    }
                    case OpCode.NoticeConnect: {
                        byte[] idBuffer = new byte[8];
                        Socket.Receive(idBuffer);
                        long id = BitConverter.ToInt64(idBuffer);

                        byte[] nameBuffer = new byte[256];
                        int nameLength = Socket.Receive(nameBuffer);
                        string name = Encoding.UTF8.GetString(nameBuffer[..nameLength]);

                        Usernames.TryAdd(id, name);

                        Busy = false;

                        MessageLog.Add($"{name} has connected.");
                        break;
                    }
                    case OpCode.MessageBegin: {
                        byte[] idBuffer = new byte[8];
                        Socket.Receive(idBuffer);
                        long id = BitConverter.ToInt64(idBuffer);

                        List<byte> msgBytes = new List<byte>();
                        while (true) {
                            byte[] msgBuffer = new byte[1024];
                            int msgLength = Socket.Receive(msgBuffer);
                            if (msgBuffer[msgLength-1] == 1) {
                                if (msgLength > 1) { msgBytes.AddRange(msgBuffer[..(msgLength-1)]); }
                                break;
                            }
                            msgBytes.AddRange(msgBuffer[..msgLength]);
                        }

                        Busy = false;

                        string msg = Encoding.UTF8.GetString(msgBytes.ToArray());
                        MessageLog.Add($" {Usernames[id]}:\n{msg}");
                        break;
                    }
                    default: {
                        Busy = false;
                        break;
                    }
                }
            }
        }

        public void SendMessage(string message) {
            // Console.WriteLine($"ATTEMPTING TO SEND MESSAGE: {message}");
            
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            while (Busy) {}
            Busy = true;

            Socket.Send([(byte)OpCode.MessageBegin]);
            for (int i = 0; i < bytes.Length; i += 1024) {
                int j = i+1024;
                if (j > bytes.Length) { j = bytes.Length; }
                Socket.Send(bytes[i..j]);
            }
            Socket.Send([1]);

            Busy = false;
        }

        private Vector2f mousePos;
        private float width;
        private float height;

        public override void Run(RenderWindow window) {
            mousePos = window.MapPixelToCoords(Mouse.GetPosition(window));
            width = window.GetView().Size.X;
            height = window.GetView().Size.Y;

            float msgtextheight = RichTextRenderer.Height(MessageTextField.Text, 20, 640.0f, 20.0f) + 20;
            if (msgtextheight < 60) { msgtextheight = 60; }

            MessageTextField.Bounds = new FloatRect(0, -20, width-40, msgtextheight);

            foreach (UIElement element in ActiveUIElements) {
                element.Run(window, mousePos);
            }

            float y = height - msgtextheight - 30;
            for (int i = MessageLog.Count-1; i >= 0; --i) {
                if (y < 0) { break; } // Stop rendering the text if it's above the screen.
                y -= RichTextRenderer.Height(MessageLog[i], 20, 640.0f, 20.0f) + 10;
                RichTextRenderer.Render(window, MessageLog[i], 20, new Vector2f(20, y), 640.0f, 20.0f);
            }
        }

        public override void OnClose() {
            foreach (UIElement element in ActiveUIElements) {
                element.OnClose();
            }
            if (Connected) {
                Socket.Send([(byte)OpCode.Close]);
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