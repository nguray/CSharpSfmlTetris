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
            Console.WriteLine("StandByMode KeyPressed{0}", arg0: game.startTimeV);
            //----------------------------------------------
            if (sender == null) return;
            var window = (SFML.Window.Window)sender;
            if (e.Code == SFML.Window.Keyboard.Key.Space)
            {
                game.SetStandbyMode();

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Escape)
            {
                game.endGame();

            }

        }

        public override void Draw()
        {
            //---------------------------------------------------
            if (game.window != null)
            {
                var left = Globals.LEFT;
                var right = left + Globals.cellSize * Globals.NB_COLUMNS;
                var top = Globals.TOP;
                var bottom = top + Globals.cellSize * Globals.NB_ROWS;

                var textScore = new Text(String.Format("Game Over", game.m_score), game.myFont, 28);
                if (textScore != null)
                {
                    var rect = textScore.GetLocalBounds();
                    float xCenter = (left + right) / 2;
                    float yCenter = (bottom + top) / 2;
                    RectangleShape rShapeText = new RectangleShape(new Vector2f(rect.Width + 30, rect.Height + 14));
                    rShapeText.Position = new Vector2f(xCenter - (rect.Width + 24) / 2, yCenter - (rect.Height + 14) / 2);
                    rShapeText.FillColor = new Color(60, 60, 255, 255);
                    game.window.Draw(rShapeText);
                    textScore.FillColor = new Color(254, 238, 72, 255);
                    textScore.Style = Text.Styles.Bold | Text.Styles.Regular;
                    textScore.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                    textScore.Position = new Vector2f(xCenter, yCenter);
                    game.window.Draw(textScore);
                }
            }

        }
    }

}