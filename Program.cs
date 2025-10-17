using System;
using System.Collections.Generic;
using System.IO;

using SFML.Window;
using SFML.Graphics;

// Initializing the window and scene before the Main method runs results in threading issues, so we just have to leave those fields null until Main runs.
#pragma warning disable CS8618

namespace Quicksilver {
    public static class Program {
        // Obviously this key bears extremely little weight in terms of security, it just serves as a way to filter out possible random noise connections from internet scraping bots.
        public static readonly byte[] Key = [19, 23, 247, 91, 68, 13, 77, 182];

        private static RenderWindow _window;
        public static WindowScene Scene {get; set;}

        public static Font Font {get; private set;}
        public static Dictionary<string, Texture> Glyphs {get; private set;}
        public static Texture DefaultGlyph {get; private set;}

        public static DateTime BootTime {get; private set;}

        public static void Main(string[] args) {
            Font = new Font(@"C:\Windows\Fonts\courbd.ttf");
            Glyphs = new Dictionary<string, Texture>();
            foreach (string file in Directory.GetFiles("Glyphs")) {
                Glyphs.Add(file[7..^4], new Texture(file));
            }
            DefaultGlyph = new Texture("Glyphs/default.png");

            BootTime = DateTime.UtcNow;

            _window = new RenderWindow(new VideoMode(1280, 720), "Quicksilver", Styles.Default, new ContextSettings() {
                AntialiasingLevel = 0
            });
            Scene = new StartMenuScene();

            _window.Closed += (object? sender, EventArgs e) => {
                Scene.OnClose();
                _window.Close();
                Environment.Exit(0);
            };
            _window.Resized += (object? sender, SizeEventArgs e) => {
                Scene.OnResize(e);
                _window.SetView(new View(new FloatRect(0, 0, e.Width, e.Height)));
            };
            _window.MouseButtonPressed += (object? sender, MouseButtonEventArgs e) => {
                Scene.OnMousePressed(e);
            };
            _window.MouseButtonReleased += (object? sender, MouseButtonEventArgs e) => {
                Scene.OnMouseReleased(e);
            };
            _window.KeyPressed += (object? sender, KeyEventArgs e) => {
                Scene.OnKeyPressed(e);
            };
            
            while (_window.IsOpen) {
                _window.DispatchEvents();
                _window.Clear(Color.Black);
                Scene.Run(_window);
                _window.Display();
            }
        }
    }
}