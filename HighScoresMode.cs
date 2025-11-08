using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;

namespace SfmlTetris
{
    class HighScoresMode : IGameMode
    {
        public HighScoresMode(Game g)
        {
            game = g;
        }
        
        public override void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            //----------------------------------------------

            if (game is not Game g) return;
            if (sender is not RenderWindow win) return;

            if ((e.Code == SFML.Window.Keyboard.Key.Enter) || (e.Code == SFML.Window.Keyboard.Key.Escape))
            {
                g.SetStandbyMode();
                //m_curTetromino = null;
                if (g.playerName.Length == 0)
                {
                    g.playerName = "XXXXXX";
                }
                g.highScores[g.idHighScore].Name = g.playerName;
                g.SaveHighScores();
                g.idHighScore = -1;
            }
            else if (e.Code == SFML.Window.Keyboard.Key.Space)
            {
                if ((g.playerName == null) || (g.playerName.Length < 8))
                {
                    g.playerName += "_";
                }
                g.highScores[g.idHighScore].Name = g.playerName;
            }
            else if (e.Code == SFML.Window.Keyboard.Key.Backspace)
            {
                if (g.playerName != null)
                {
                    if (g.playerName.Length == 1)
                    {
                        g.playerName = "";
                    }
                    else if (g.playerName.Length > 1)
                    {
                        g.playerName = g.playerName.Substring(0, g.playerName.Length - 1);
                    }
                    g.highScores[g.idHighScore].Name = g.playerName;
                }
            }
            else
            {
                if ((g.playerName != null) && (g.playerName.Length < 8))
                {

                    if ((e.Code >= SFML.Window.Keyboard.Key.Num0) && (e.Code <= SFML.Window.Keyboard.Key.Num9))
                    {
                        char c = (char)((int)'0' + e.Code - SFML.Window.Keyboard.Key.Num0);
                        if (g.playerName.Length < 8)
                        {
                            g.playerName += c;
                        }
                    }
                    else if ((e.Code >= SFML.Window.Keyboard.Key.Numpad0) && (e.Code <= SFML.Window.Keyboard.Key.Numpad9))
                    {
                        char c = (char)((int)'0' + e.Code - SFML.Window.Keyboard.Key.Numpad0);
                        if (g.playerName.Length < 8)
                        {
                            g.playerName += c;
                        }

                    }
                    else if ((e.Code >= SFML.Window.Keyboard.Key.A) && (e.Code <= SFML.Window.Keyboard.Key.Z))
                    {
                        char c = (char)((int)'A' + e.Code - SFML.Window.Keyboard.Key.A);
                        if (g.playerName.Length < 8)
                        {
                            g.playerName += c;
                        }
                    }
                    g.highScores[g.idHighScore].Name = g.playerName;

                }
            }

        }

        public override void Draw()
        {
            
            //---------------------------------------------------

            if ((game is Game g) && (game.window is RenderWindow win))
            {
                int x_center = (int)(Globals.LEFT + Globals.cellSize * Globals.NB_COLUMNS / 2);

                Text txt = new Text("HIGH SCORES", game.myFont, 24);
                var rect = txt.GetLocalBounds();
                txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                txt.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                txt.Position = new Vector2f(x_center, 44);
                win.Draw(txt);

                int offSet = (Globals.cellSize * Globals.NB_COLUMNS) / 8;
                int xCol0 = (int)(1.5f * offSet);
                int xCol1 = offSet * 5;
                int yLin = 80;
                Color color;
                for (int i = 0; i < game.highScores.Count; i++)
                {
                    var h = g.highScores[i];
                    if (((g.i_color % 2) == 0) && (i == g.idHighScore))
                    {
                        color = Color.Blue;
                    }
                    else
                    {
                        color = new Color(254, 238, 72, 255);
                    }
                    txt = new Text(h.Name, game.myFont, 20);
                    txt.FillColor = color;
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Position = new Vector2f(xCol0, yLin);
                    win.Draw(txt);
                    txt = new Text(String.Format("{0:00000}", h.Score), g.myFont, 22);
                    txt.FillColor = color;
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Position = new Vector2f(xCol1, yLin);
                    win.Draw(txt);
                    yLin += 36;

                }
            }

        }
    }

}