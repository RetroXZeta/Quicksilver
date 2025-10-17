using System;
using System.Text;
using SFML.Graphics;
using SFML.System;

namespace Quicksilver {
    public static class RichTextRenderer {
        public static float Render(RenderWindow window, string text, uint charsize, Vector2f position, float width, float linespace) {
            Vector2f origPosition = position;

            Texture texture = Program.Font.GetTexture(charsize);
            texture.Smooth = false;
            
            byte[] utf32bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, Encoding.UTF8.GetBytes(text));

            Color color = Color.White;
            bool bold = false;
            float waveAmplitude = 0.0f;
            float waveFrequency = 1.0f;
            float wavePhase = 0.0f;
            float wavePhaseIncrement = 0.0f;

            TimeSpan time = DateTime.UtcNow - Program.BootTime;
            double sec = time.TotalSeconds;

            position.Y += linespace;
            for (int i = 0; i < utf32bytes.Length; i += 4) {
                uint unicode = BitConverter.ToUInt32(utf32bytes[i..(i+4)]);
                string c = Encoding.UTF32.GetString(utf32bytes[i..(i+4)]);
                switch (c) {
                    case "\n": {
                        position.X = origPosition.X;
                        position.Y += linespace;
                        break;
                    }
                    case "{": {
                        string cj = "{";
                        int j = i+4;
                        for (; j < utf32bytes.Length && cj != "}"; j += 4) {
                            cj = Encoding.UTF32.GetString(utf32bytes[j..(j+4)]);
                        }
                        if (cj == "}") {
                            // Bracket is closed! Parse and run the command inside.
                            string fullcmd = Encoding.UTF32.GetString(utf32bytes[(i+4)..(j-4)]);
                            i = j-4;
                            int split = fullcmd.IndexOf(':');
                            if (split == -1 || split >= fullcmd.Length-1) { break; }
                            string cmd = fullcmd[0..split];
                            string[] args = fullcmd[(split+1)..].Split(',');
                            switch (cmd) {
                                case "color": {
                                    if (args.Length != 1) { break; }
                                    switch (args[0]) {
                                        case "white": color = Color.White; break;
                                        case "red": color = Color.Red; break;
                                        case "green": color = Color.Green; break;
                                        case "blue": color = Color.Blue; break;
                                        case "yellow": color = Color.Yellow; break;
                                        case "cyan": color = Color.Cyan; break;
                                        case "magenta": color = Color.Magenta; break;
                                        case "black": color = Color.Black; break;
                                        default: break;
                                    }
                                    break;
                                }
                                case "bold": {
                                    if (args.Length >= 2) { break; }
                                    if (args.Length == 0) { bold = !bold; break; }
                                    switch (args[0]) {
                                        case "true": bold = true; break;
                                        case "false": bold = false; break;
                                        default: break;
                                    }
                                    break;
                                }
                                case "wave": {
                                    if (args.Length != 3) { break; }
                                    bool valid = true;
                                    float wamp, wfreq, wphase;
                                    if (!float.TryParse(args[0], out wamp)) { valid = false; }
                                    if (!float.TryParse(args[1], out wfreq)) { valid = false; }
                                    if (!float.TryParse(args[2], out wphase)) { valid = false; }
                                    if (valid) {
                                        waveAmplitude = wamp;
                                        waveFrequency = wfreq == 0 ? 1 : wfreq;
                                        wavePhaseIncrement = wphase;
                                    }
                                    break;
                                }
                                case "glyph": {
                                    if (args.Length != 1) { break; }
                                    float waveY = waveAmplitude * (float)Math.Sin(Math.PI * waveFrequency * sec + Math.PI * wavePhase);
                                    Texture? t;
                                    bool valid = Program.Glyphs.TryGetValue(args[0], out t);
                                    if (!valid) { t = Program.DefaultGlyph; }
                                    Sprite sprite = new Sprite(t);
                                    sprite.Position = position + new Vector2f(2, 4) - new Vector2f(0, t!.Size.Y) + new Vector2f(0, waveY);
                                    sprite.Color = color;
                                    window.Draw(sprite);
                                    position.X += t!.Size.X + 4;
                                    wavePhase -= wavePhaseIncrement;
                                    break;
                                }
                            }
                        } else {
                            // If it's unclosed, just draw it as a normal character.
                            float waveY = waveAmplitude * (float)Math.Sin(Math.PI * waveFrequency * sec + Math.PI * wavePhase);
                            Glyph g = Program.Font.GetGlyph(unicode, charsize, bold, 0);
                            Sprite sprite = new Sprite(texture);
                            sprite.TextureRect = g.TextureRect;
                            sprite.Position = position + g.Bounds.Position + new Vector2f(0, waveY);
                            sprite.Color = color;
                            window.Draw(sprite);
                            position.X += g.Advance;
                            wavePhase -= wavePhaseIncrement;
                        }
                        break;
                    }
                    default: {
                        float waveY = waveAmplitude * (float)Math.Sin(Math.PI * waveFrequency * sec + Math.PI * wavePhase);
                        Glyph g = Program.Font.GetGlyph(unicode, charsize, bold, 0);
                        Sprite sprite = new Sprite(texture);
                        sprite.TextureRect = g.TextureRect;
                        sprite.Position = position + g.Bounds.Position + new Vector2f(0, waveY);
                        sprite.Color = color;
                        window.Draw(sprite);
                        position.X += g.Advance;
                        wavePhase -= wavePhaseIncrement;
                        break;
                    }
                }
            }

            return position.Y - origPosition.Y;
        }

        public static float Height(string text, uint charsize, float width, float linespace) {
            Vector2f position = new Vector2f(0, 0);

            Texture texture = Program.Font.GetTexture(charsize);
            texture.Smooth = false;
            
            byte[] utf32bytes = Encoding.Convert(Encoding.UTF8, Encoding.UTF32, Encoding.UTF8.GetBytes(text));

            bool bold = false;

            position.Y += linespace;
            for (int i = 0; i < utf32bytes.Length; i += 4) {
                uint unicode = BitConverter.ToUInt32(utf32bytes[i..(i+4)]);
                string c = Encoding.UTF32.GetString(utf32bytes[i..(i+4)]);
                switch (c) {
                    case "\n": {
                        position.X = 0;
                        position.Y += linespace;
                        break;
                    }
                    case "{": {
                        string cj = "{";
                        int j = i+4;
                        for (; j < utf32bytes.Length && cj != "}"; j += 4) {
                            cj = Encoding.UTF32.GetString(utf32bytes[j..(j+4)]);
                        }
                        if (cj == "}") {
                            // Bracket is closed! Parse and run the command inside.
                            string fullcmd = Encoding.UTF32.GetString(utf32bytes[(i+4)..(j-4)]);
                            i = j-4;
                            int split = fullcmd.IndexOf(':');
                            if (split == -1 || split >= fullcmd.Length-1) { break; }
                            string cmd = fullcmd[0..split];
                            string[] args = fullcmd[(split+1)..].Split(',');
                            switch (cmd) {
                                case "bold": {
                                    if (args.Length >= 2) { break; }
                                    if (args.Length == 0) { bold = !bold; break; }
                                    switch (args[0]) {
                                        case "true": bold = true; break;
                                        case "false": bold = false; break;
                                        default: break;
                                    }
                                    break;
                                }
                                case "glyph": {
                                    if (args.Length != 1) { break; }
                                    Texture? t;
                                    bool valid = Program.Glyphs.TryGetValue(args[0], out t);
                                    if (!valid) { t = Program.DefaultGlyph; }
                                    position.X += t!.Size.X + 4;
                                    break;
                                }
                            }
                        } else {
                            // If it's unclosed, just draw it as a normal character.
                            Glyph g = Program.Font.GetGlyph(unicode, charsize, bold, 0);
                            position.X += g.Advance;
                        }
                        break;
                    }
                    default: {
                        Glyph g = Program.Font.GetGlyph(unicode, charsize, bold, 0);
                        position.X += g.Advance;
                        break;
                    }
                }
            }

            return position.Y;
        }
    }
}