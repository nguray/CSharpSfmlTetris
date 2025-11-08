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
                //----------------------------------------------
            if (game is not Game g) return;
            if (sender is not RenderWindow win) return;


            if (e.Code == SFML.Window.Keyboard.Key.Space)
            {
                g.InitGame();
                g.SetPlayMode();
                g.NewTetromino();

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Escape)
            {
                g.EndGame();

            }
            
        }

        public override void Draw()
        {

            if (game is not Game g) return;
            if (g.window is not RenderWindow win) return;

            int x_center = (int)(Globals.LEFT + Globals.cellSize * Globals.NB_COLUMNS / 2);

            int offSetY = Globals.TOP + 3 * Globals.cellSize;
            int yLin = offSetY;
            Text txt = new Text("TETRIS", g.myFont, 24);
            if ((g.i_color % 2) == 0)
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
            win.Draw(txt);

            yLin += 48;
            Text txt1 = new Text("powered by", g.myFont, 18);
            rect = txt1.GetLocalBounds();
            txt1.Style = Text.Styles.Bold | Text.Styles.Regular;
            txt1.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
            txt1.Position = new Vector2f(x_center, yLin);
            win.Draw(txt1);

            yLin += 42;
            Text txt2 = new Text("Sfml and C#", g.myFont, 22);
            rect = txt2.GetLocalBounds();
            txt2.Style = Text.Styles.Bold | Text.Styles.Regular;
            txt2.Origin = new Vector2f(rect.Width / 2.0f, rect.Height / 2.0f);
            txt2.Position = new Vector2f(x_center, yLin);
            win.Draw(txt2);

            yLin += 42;
            Text txt3 = new Text("Raymond NGUYEN THANH", g.myFont, 16);
            rect = txt3.GetLocalBounds();
            txt3.Style = Text.Styles.Bold | Text.Styles.Regular;
            txt3.Origin = new Vector2f(rect.Width / 2.0f, rect.Height / 2.0f);
            txt3.Position = new Vector2f(x_center - 9, yLin);
            win.Draw(txt3);

        }
    }

}