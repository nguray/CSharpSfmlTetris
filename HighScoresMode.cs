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
            if ((game==null)||(sender==null)) return;
            var window = (SFML.Window.Window)sender;

            if ((e.Code == SFML.Window.Keyboard.Key.Enter) || (e.Code == SFML.Window.Keyboard.Key.Escape))
            {
                game.SetStandbyMode();
                //m_curTetromino = null;
                if (game.playerName.Length == 0)
                {
                    game.playerName = "XXXXXX";
                }
                game.highScores[game.idHighScore].Name = game.playerName;
                game.SaveHighScores();
                game.idHighScore = -1;
            }
            else if (e.Code == SFML.Window.Keyboard.Key.Space)
            {
                if ((game.playerName == null) || (game.playerName.Length < 8))
                {
                    game.playerName += "_";
                }
                game.highScores[game.idHighScore].Name = game.playerName;
            }
            else if (e.Code == SFML.Window.Keyboard.Key.Backspace)
            {
                if (game.playerName != null)
                {
                    if (game.playerName.Length == 1)
                    {
                        game.playerName = "";
                    }
                    else if (game.playerName.Length > 1)
                    {
                        game.playerName = game.playerName.Substring(0, game.playerName.Length - 1);
                    }
                    game.highScores[game.idHighScore].Name = game.playerName;
                }
            }
            else
            {
                if ((game.playerName != null) && (game.playerName.Length < 8))
                {

                    if ((e.Code >= SFML.Window.Keyboard.Key.Num0) && (e.Code <= SFML.Window.Keyboard.Key.Num9))
                    {
                        char c = (char)((int)'0' + e.Code - SFML.Window.Keyboard.Key.Num0);
                        if (game.playerName.Length < 8)
                        {
                            game.playerName += c;
                        }
                    }
                    else if ((e.Code >= SFML.Window.Keyboard.Key.Numpad0) && (e.Code <= SFML.Window.Keyboard.Key.Numpad9))
                    {
                        char c = (char)((int)'0' + e.Code - SFML.Window.Keyboard.Key.Numpad0);
                        if (game.playerName.Length < 8)
                        {
                            game.playerName += c;
                        }

                    }
                    else if ((e.Code >= SFML.Window.Keyboard.Key.A) && (e.Code <= SFML.Window.Keyboard.Key.Z))
                    {
                        char c = (char)((int)'A' + e.Code - SFML.Window.Keyboard.Key.A);
                        if (game.playerName.Length < 8)
                        {
                            game.playerName += c;
                        }
                    }
                    game.highScores[game.idHighScore].Name = game.playerName;

                }
            }

        }
        // public override void ProcessKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
        // {
            
        // }

        public override void Draw()
        {
            
            //---------------------------------------------------
            if ((game!=null)&&(game.window != null))
            {
                int x_center = (int)(Globals.LEFT + Globals.cellSize * Globals.NB_COLUMNS / 2);

                Text txt = new Text("HIGH SCORES", game.myFont, 24);
                var rect = txt.GetLocalBounds();
                txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                txt.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                txt.Position = new Vector2f(x_center, 44);
                game.window.Draw(txt);

                int offSet = (Globals.cellSize * Globals.NB_COLUMNS) / 8;
                int xCol0 = (int)(1.5f * offSet);
                int xCol1 = offSet * 5;
                int yLin = 80;
                Color color;
                for (int i = 0; i < game.highScores.Count; i++)
                {
                    var h = game.highScores[i];
                    if (((game.i_color % 2) == 0) && (i == game.idHighScore))
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
                    game.window.Draw(txt);
                    txt = new Text(String.Format("{0:00000}", h.Score), game.myFont, 22);
                    txt.FillColor = color;
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Position = new Vector2f(xCol1, yLin);
                    game.window.Draw(txt);
                    yLin += 36;

                }
            }

        }
    }

}