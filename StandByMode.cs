using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;

namespace SfmlTetris
{
    class StandByMode : IGameMode
    {
        public StandByMode(Game? g)
        {
            game = g;
        }
        
        public override void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            Console.WriteLine("StandByMode KeyPressed{0}", arg0: game.startTimeV);
                //----------------------------------------------
            if (sender == null) return;
            var window = (SFML.Window.Window)sender;
            if (e.Code == SFML.Window.Keyboard.Key.Space)
            {
                game.m_clock.Restart();
                game.SetPlayMode();
                game.NewTetromino();

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Escape)
            {
                game.endGame();

            }
            
        }

        public override void ProcessKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
        {
            //--

        }

        public override void Draw()
        {
            if (game.window != null)
            {
                int x_center = (int)(Globals.LEFT + Globals.cellSize * Globals.NB_COLUMNS / 2);

                int offSetY = Globals.TOP + 3 * Globals.cellSize;
                int yLin = offSetY;
                Text txt = new Text("TETRIS", game.myFont, 24);
                if ((game.m_i_color % 2) == 0)
                {
                    txt.FillColor = new Color(254, 238, 72, 255);
                }
                else
                {
                    txt.FillColor = Color.Blue;
                }
                var rect = txt.GetLocalBounds();
                txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                txt.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                txt.Position = new Vector2f(x_center, yLin);
                game.window.Draw(txt);

                yLin += 48;
                Text txt1 = new Text("powered by", game.myFont, 18);
                rect = txt1.GetLocalBounds();
                txt1.Style = Text.Styles.Bold | Text.Styles.Regular;
                txt1.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                txt1.Position = new Vector2f(x_center, yLin);
                game.window.Draw(txt1);

                yLin += 42;
                Text txt2 = new Text("Sfml and C#", game.myFont, 22);
                rect = txt2.GetLocalBounds();
                txt2.Style = Text.Styles.Bold | Text.Styles.Regular;
                txt2.Origin = new Vector2f(rect.Width / 2.0f, rect.Height / 2.0f);
                txt2.Position = new Vector2f(x_center, yLin);
                game.window.Draw(txt2);

                yLin += 42;
                Text txt3 = new Text("Raymond NGUYEN THANH", game.myFont, 16);
                rect = txt3.GetLocalBounds();
                txt3.Style = Text.Styles.Bold | Text.Styles.Regular;
                txt3.Origin = new Vector2f(rect.Width / 2.0f, rect.Height / 2.0f);
                txt3.Position = new Vector2f(x_center - 9, yLin);
                game.window.Draw(txt3);

            }


        }
    }

}