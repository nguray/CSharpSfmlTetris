using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;

namespace SfmlTetris
{
    class GameOverMode : IGameMode
    {
        public GameOverMode(Game? g)
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
                g.SetStandbyMode();

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Escape)
            {
                g.EndGame();

            }

        }

        public override void Draw()
        {
            //---------------------------------------------------
            if (game is not Game g) return;
            if (g.window is not RenderWindow win) return;

            g.DrawBoard();

            var left = Globals.LEFT;
            var right = left + Globals.cellSize * Globals.NB_COLUMNS;
            var top = Globals.TOP;
            var bottom = top + Globals.cellSize * Globals.NB_ROWS;

            var textScore = new Text(String.Format("Game Over", game.score), game.myFont, 28);
            if (textScore != null)
            {
                var rect = textScore.GetLocalBounds();
                float xCenter = (left + right) / 2;
                float yCenter = (bottom + top) / 2;
                RectangleShape rShapeText = new RectangleShape(new Vector2f(rect.Width + 30, rect.Height + 14));
                rShapeText.Position = new Vector2f(xCenter - (rect.Width + 24) / 2, yCenter - (rect.Height + 14) / 2);
                rShapeText.FillColor = new Color(60, 60, 255, 255);
                win.Draw(rShapeText);
                textScore.FillColor = new Color(254, 238, 72, 255);
                textScore.Style = Text.Styles.Bold | Text.Styles.Regular;
                textScore.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                textScore.Position = new Vector2f(xCenter, yCenter);
                win.Draw(textScore);
            }

        }
    }

}
