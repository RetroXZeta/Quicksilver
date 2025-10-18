using System;
using System.Collections.Generic;
using System.Net.Http;

using SFML.System;
using SFML.Window;
using SFML.Graphics;

#pragma warning disable 8602
#pragma warning disable 8604

namespace Quicksilver {
    public class StartMenuScene : WindowScene {
        public bool JoinPopUp {get; private set;} = false;
        public bool HostPopUp {get; private set;} = false;

        public UIButton JoinButton {get; private set;}
        public UIButton HostButton {get; private set;}
        public UIButton ExitPopUpButton {get; private set;}
        public UITextField IPTextField {get; private set;}
        public UITextField PortTextField {get; private set;}
        public UITextField PasswordTextField {get; private set;}
        public UITextField NameTextField {get; private set;}
        public UIButton ConfirmButton {get; private set;}

        public List<UIElement> ActiveUIElements {get; private set;} = new List<UIElement>();

        public StartMenuScene() {
            JoinButton = new UIButton(new FloatRect(0, -40, 320, 40), UIAlignment.Center, "Join Chatroom", 20, () => {
                JoinPopUp = true;
                ConfirmButton.Text = "Join";
                IPTextField.Text = "";
                IPTextField.Editable = true;
                NameTextField.Text = "";
                NameTextField.Editable = true;
                ActiveUIElements = [ExitPopUpButton, IPTextField, PortTextField, PasswordTextField, NameTextField, ConfirmButton];
            });
            HostButton = new UIButton(new FloatRect(0, +40, 320, 40), UIAlignment.Center, "Host Chatroom", 20, async () => {
                string ip = await new HttpClient().GetStringAsync("https://ipinfo.io/ip");
                HostPopUp = true;
                ConfirmButton.Text = "Host";
                IPTextField.Text = ip;
                IPTextField.Editable = false;
                NameTextField.Text = "HOST";
                NameTextField.Editable = false;
                ActiveUIElements = [ExitPopUpButton, IPTextField, PortTextField, PasswordTextField, NameTextField, ConfirmButton];
            });
            ExitPopUpButton = new UIButton(new FloatRect(-60, 60, 60, 60), UIAlignment.TopRight, "X", 40, () => {
                JoinPopUp = false;
                HostPopUp = false;
                ActiveUIElements = [JoinButton, HostButton];
            });
            IPTextField = new UITextField(new FloatRect(0, -150, 320, 40), UIAlignment.Center, "IP Address", 20, UIAlignment.Center, false, true, () => {});
            PortTextField = new UITextField(new FloatRect(0, -90, 320, 40), UIAlignment.Center, "Port", 20, UIAlignment.Center, false, false, () => {});
            PasswordTextField = new UITextField(new FloatRect(0, -30, 320, 40), UIAlignment.Center, "Password", 20, UIAlignment.Center, false, true, () => {});
            NameTextField = new UITextField(new FloatRect(0, +30, 320, 40), UIAlignment.Center, "Username", 20, UIAlignment.Center, false, true, () => {});
            ConfirmButton = new UIButton(new FloatRect(0, +150, 160, 40), UIAlignment.Center, "", 20, () => {
                if (JoinPopUp) {
                    Program.Scene = new ClientChatScene(IPTextField.Text, Convert.ToInt32(PortTextField.Text), PasswordTextField.Text, NameTextField.Text);
                }
                if (HostPopUp) {
                    Program.Scene = new HostChatScene(Convert.ToInt32(PortTextField.Text), PasswordTextField.Text);
                }
            });
            ActiveUIElements = [JoinButton, HostButton];
        }

        private Vector2f mousePos;
        private float width;
        private float height;

        public override void Run(RenderWindow window) {
            mousePos = window.MapPixelToCoords(Mouse.GetPosition(window));
            width = window.GetView().Size.X;
            height = window.GetView().Size.Y;

            if (JoinPopUp || HostPopUp) {
                using (RectangleShape rectangle = new RectangleShape()) {
                    rectangle.Position = new Vector2f(40, 40);
                    rectangle.Size = new Vector2f(width-80, height-80);
                    rectangle.FillColor = new Color(0, 0, 0);
                    rectangle.OutlineColor = new Color(255, 255, 255);
                    rectangle.OutlineThickness = 2;
                    window.Draw(rectangle);
                }
            }

            foreach (UIElement element in ActiveUIElements) {
                element.Run(window, mousePos);
            }
        }

        public override void OnClose() {
            foreach (UIElement element in ActiveUIElements) {
                element.OnClose();
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