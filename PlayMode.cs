using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;

namespace SfmlTetris
{
    class PlayMode : IGameMode
    {
        bool fDrop = false;
        bool fFastDown = false;
        int VelH = 0;

        int startTimeV = 0;
        int startTimeH = 0;
        int startTimeE = 0;

        public delegate bool IsOutLimit_t();
        private IsOutLimit_t? IsOutLimit = null;

        int nbCompletedLines = 0;

        int horizontalMove = 0;
        int horizontalStartColumn = 0;

        public PlayMode(Game? g)
        {
            game = g;
        }

        public override void Init()
        {
            startTimeV = 0;
            startTimeH = 0;
            startTimeE = 0;

        } 

        public override void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            //----------------------------------------------

            if (game is not Game g) return;
            if (sender is not RenderWindow win) return;
            if (g.curTetromino is not Tetromino curTetro) return;

            switch (e.Code)
            {
                case SFML.Window.Keyboard.Key.Escape:
                    g.CheckHighScore();
                    break;
                case SFML.Window.Keyboard.Key.Left:
                    VelH = -1;
                    IsOutLimit = curTetro.IsOutLeft;
                    break;
                case SFML.Window.Keyboard.Key.Right:
                    VelH = 1;
                    IsOutLimit = curTetro.IsOutRight;
                    break;
                case SFML.Window.Keyboard.Key.Up:
                    curTetro.RotateLeft();
                    if (curTetro.HitGround(g.board))
                    {
                        //-- Undo Rotate
                        curTetro.RotateRight();
                    }
                    else if (curTetro.IsOutRight())
                    {
                        var backupX = curTetro.x;
                        //-- Move Inside board
                        while (curTetro.IsOutRight())
                        {
                            curTetro.x--;
                        }
                        if (curTetro.HitGround(g.board))
                        {
                            curTetro.x = backupX;
                            //-- Undo Rotate
                            curTetro.RotateRight();

                        }
                    }
                    else if (curTetro.IsOutLeft())
                    {
                        var backupX = curTetro.x;
                        //-- Move Inside Board
                        while (curTetro.IsOutLeft())
                        {
                            curTetro.x++;
                        }
                        if (curTetro.HitGround(g.board))
                        {
                            curTetro.x = backupX;
                            //-- Undo Rotate
                            curTetro.RotateRight();
                        }
                    }
                    break;
                case SFML.Window.Keyboard.Key.Down:
                    fFastDown = true;
                    break;
                case SFML.Window.Keyboard.Key.Space:
                    fDrop = true;
                    break;

            }


        }
        
        public override void ProcessKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
        {
            switch (e.Code)
            {
                case SFML.Window.Keyboard.Key.Left:
                case SFML.Window.Keyboard.Key.Right:
                    VelH = 0;
                    break;
                case SFML.Window.Keyboard.Key.Down:
                    fFastDown = false;
                    break;

            }
            
        }

        public override void Draw()
        {
            //--
            if (game is not Game g) return;
            if (g.window is not RenderWindow win) return;

            g.curTetromino?.Draw(win);

            g.nextTetromino?.Draw(win);

            //--
            g.DrawBoard();
                
        }

        public override void Update()
        {

            int curTime;

            //_ = game ?? throw new ArgumentNullException(nameof(game));

            if (game is not Game g) return;
            if (g.curTetromino is not Tetromino curTetro) return;

            if (nbCompletedLines > 0)
            {
                curTime = g.clock.ElapsedTime.AsMilliseconds();
                if ((curTime - startTimeE) > 250)
                {
                    startTimeE = curTime;
                    nbCompletedLines--;
                    g.EraseFirstCompletedLine();
                    g.playSuccesSound();
                }

            }
            
            if (horizontalMove != 0)
            {

                curTime = g.clock.ElapsedTime.AsMilliseconds();

                if ((curTime - startTimeH) > 20)
                {
                    for (int i = 0; i < 5; i++)
                    {

                        var backupX = curTetro.x;
                        curTetro.x += horizontalMove;
                        //Console.WriteLine(horizontalMove);
                        if ((IsOutLimit is not null)&&IsOutLimit())
                        {
                            curTetro.x = backupX;
                            horizontalMove = 0;
                            break;
                        }
                        else
                        {
                            if (curTetro.HitGround(g.board))
                            {
                                curTetro.x = backupX;
                                horizontalMove = 0;
                                break;
                            }
                        }

                        if (horizontalMove != 0)
                        {
                            startTimeH = curTime;
                            if (horizontalStartColumn != curTetro.Column())
                            {
                                curTetro.x = backupX;
                                horizontalMove = 0;
                                break;
                            }

                        }

                    }

                }

            }
            else if (fDrop)
            {

                curTime = g.clock.ElapsedTime.AsMilliseconds();
                if ((curTime - startTimeV) > 10)
                {
                    startTimeV = curTime;
                    for (int i = 0; i < 6; i++)
                    {
                        //-- Move down to Check
                        curTetro.y++;
                        if (curTetro.HitGround(g.board))
                        {
                            curTetro.y--;
                            nbCompletedLines = g.FreezeCurTetromino();
                            g.NewTetromino();
                            fDrop = false;
                            break;
                        }
                        else if (curTetro.IsOutBottom())
                        {
                            curTetro.y--;
                            nbCompletedLines = g.FreezeCurTetromino();
                            g.NewTetromino();
                            fDrop = false;
                            break;
                        }
                        if (fDrop && (VelH != 0))
                        {
                            if ((curTime - startTimeH) > 15)
                            {
                                var backupX = curTetro.x;
                                curTetro.x += VelH;
                                if ((IsOutLimit is not null)&&IsOutLimit())
                                {
                                    curTetro.x = backupX;
                                }
                                else
                                {
                                    if (curTetro.HitGround(game.board))
                                    {
                                        curTetro.x = backupX;
                                    }
                                    else
                                    {
                                        horizontalMove = VelH;
                                        horizontalStartColumn = curTetro.Column();
                                        break;
                                    }
                                }
                            }

                        }
                    }
                }

            }
            else
            {
                curTime = g.clock.ElapsedTime.AsMilliseconds();

                int limitElapse = fFastDown ? 10 : 30;

                if ((curTime - startTimeV) > limitElapse)
                {

                    startTimeV = curTime;

                    for (int i = 0; i < 3; i++)
                    {
                        //-- Move down to check
                        curTetro.y++;
                        if (curTetro.HitGround(g.board))
                        {
                            curTetro.y--;
                            nbCompletedLines = g.FreezeCurTetromino();
                            g.NewTetromino();
                            break;
                        }
                        else if (curTetro.IsOutBottom())
                        {
                            curTetro.y--;
                            nbCompletedLines = g.FreezeCurTetromino();
                            g.NewTetromino();
                            break;
                        }

                        if (VelH != 0)
                        {
                            if ((curTime - startTimeH) > 15)
                            {

                                var backupX = curTetro.x;
                                curTetro.x += VelH;

                                if ((IsOutLimit is not null)&&IsOutLimit())
                                {
                                    curTetro.x = backupX;
                                }
                                else
                                {
                                    if (curTetro.HitGround(game.board))
                                    {
                                        curTetro.x -= VelH;
                                    }
                                    else
                                    {
                                        startTimeH = curTime;
                                        horizontalMove = VelH;
                                        horizontalStartColumn = curTetro.Column();
                                        break;
                                    }
                                }

                            }

                        }

                    }
                }
            }


        }
    }

}