using System;

using SFML.System;
using SFML.Window;
using SFML.Graphics;

namespace Quicksilver {
    public class UIRichTextField : UIElement {
        public string GhostText {get; set;}
        public uint CharacterSize {get; set;}
        public bool Editable {get; set;}
        public Action Action {get; set;}
        public bool Selected {get; private set;}
        public string Text {get; set;}

        public UIRichTextField(FloatRect bounds, UIAlignment alignment, string ghostText, uint charSize, Action action) {
            Bounds = bounds;
            Alignment = alignment;
            GhostText = ghostText;
            CharacterSize = charSize;
            Editable = true;
            Action = action;
            Selected = false;
            Text = "";
        }

        public override void Run(RenderWindow window, Vector2f mousePos) {
            base.Run(window, mousePos);

            using (RectangleShape rectangle = new RectangleShape()) {
                rectangle.Position = TrueBounds.Position;
                rectangle.Size = TrueBounds.Size;
                rectangle.FillColor = Selected ? new Color(32, 32, 32) : new Color(0, 0, 0);
                rectangle.OutlineColor = new Color(255, 255, 255);
                rectangle.OutlineThickness = 2;
                window.Draw(rectangle);
            }
            if (Text == "" && !Selected) {
                using (Text text = new Text()) {
                    text.Font = Program.Font;
                    text.DisplayedString = GhostText;
                    text.CharacterSize = CharacterSize;
                    Vector2f extraPos = new Vector2f(10, 10);
                    Vector2f extraOrig = new Vector2f(0, 0);
                    text.Position = TrueBounds.Position + extraPos;
                    text.Origin = text.GetLocalBounds().Position + extraOrig;
                    text.FillColor = new Color(128, 128, 128);
                    window.Draw(text);
                }
            } else {
                RichTextRenderer.Render(window, Text + (Selected ? "|" : ""), 20, TrueBounds.Position + new Vector2f(10, 5), 100, 20);
            }
        }

        public override void OnMousePressed(Vector2f mousePos, MouseButtonEventArgs e) {
            if (e.Button == Mouse.Button.Left && Editable) {
                Selected = TrueBounds.Contains(mousePos);
            }
        }

        public override void OnKeyPressed(KeyEventArgs e) {
            if (Selected && Editable) {
                if (e.Code >= Keyboard.Key.Num0 && e.Code <= Keyboard.Key.Num9) {
                    Text += e.Shift ? ")!@#$%^&*("[(int)e.Code-26] : (char)('0'+(int)e.Code-26);
                }
                if (e.Code >= Keyboard.Key.A && e.Code <= Keyboard.Key.Z) {
                    Text += (char)((e.Shift ? 'A' : 'a') + (int)e.Code);
                }
                if (e.Code == Keyboard.Key.LBracket) { Text += e.Shift ? '{' : '['; }
                if (e.Code == Keyboard.Key.RBracket) { Text += e.Shift ? '}' : ']'; }
                if (e.Code == Keyboard.Key.Semicolon) { Text += e.Shift ? ':' : ';'; }
                if (e.Code == Keyboard.Key.Comma) { Text += e.Shift ? '<' : ','; }
                if (e.Code == Keyboard.Key.Period) { Text += e.Shift ? '>' : '.'; }
                if (e.Code == Keyboard.Key.Apostrophe) { Text += e.Shift ? '"' : '\''; }
                if (e.Code == Keyboard.Key.Slash) { Text += e.Shift ? '?' : '/'; }
                if (e.Code == Keyboard.Key.Backslash) { Text += e.Shift ? '|' : '\\'; }
                if (e.Code == Keyboard.Key.Grave) { Text += e.Shift ? '~' : '`'; }
                if (e.Code == Keyboard.Key.Equal) { Text += e.Shift ? '+' : '='; }
                if (e.Code == Keyboard.Key.Hyphen) { Text += e.Shift ? '_' : '-'; }
                if (e.Code == Keyboard.Key.Space) { Text += " "; }
                if (e.Code == Keyboard.Key.Backspace && Text.Length > 0) {
                    Text = e.Control ? "" : Text[..^1];
                }
                if (e.Code == Keyboard.Key.Enter) {
                    if (e.Shift) {
                        Text += "\n";
                    }
                    if (!e.Shift) {
                        Action();
                    }
                }
            }
        }
    }
}