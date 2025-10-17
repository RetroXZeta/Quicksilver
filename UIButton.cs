using System;

using SFML.System;
using SFML.Window;
using SFML.Graphics;

namespace Quicksilver {
    public class UIButton : UIElement {
        public string Text {get; set;}
        public uint CharacterSize {get; set;}
        public Action Action {get; set;}
        public bool Held {get; private set;}

        public UIButton(FloatRect bounds, UIAlignment alignment, string text, uint charsize, Action action) {
            Bounds = bounds;
            Alignment = alignment;
            Text = text;
            CharacterSize = charsize;
            Action = action;
        }

        public override void Run(RenderWindow window, Vector2f mousePos) {
            base.Run(window, mousePos);

            using (RectangleShape rectangle = new RectangleShape()) {
                rectangle.Position = TrueBounds.Position;
                rectangle.Size = TrueBounds.Size;
                rectangle.FillColor = TrueBounds.Contains(mousePos) ? new Color(32, 32, 32) : new Color(0, 0, 0);
                rectangle.OutlineColor = new Color(255, 255, 255);
                rectangle.OutlineThickness = 2;
                window.Draw(rectangle);
            }
            using (Text text = new Text()) {
                text.Font = Program.Font;
                text.DisplayedString = Text;
                text.CharacterSize = CharacterSize;
                text.Position = TrueBounds.Position + TrueBounds.Size / 2;
                text.Origin = text.GetLocalBounds().Position + text.GetGlobalBounds().Size / 2;
                text.FillColor = new Color(255, 255, 255);
                window.Draw(text);
            }
        }

        public override void OnMousePressed(Vector2f mousePos, MouseButtonEventArgs e) {
            if (e.Button == Mouse.Button.Left && TrueBounds.Contains(mousePos)) {
                Held = true;
            }
        }
        
        public override void OnMouseReleased(Vector2f mousePos, MouseButtonEventArgs e) {
            if (e.Button == Mouse.Button.Left && TrueBounds.Contains(mousePos) && Held) {
                Action();
            }
            if (e.Button == Mouse.Button.Left) {
                Held = false;
            }
        }
    }
}