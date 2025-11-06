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

        public delegate bool IsOutLimit_t();
        static IsOutLimit_t? IsOutLimit;

        int nbCompletedLines = 0;

        int horizontalMove = 0;
        int horizontalStartColumn = 0;

        public PlayMode(Game? g)
        {
            game = g;
        }

        public override void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            //----------------------------------------------
            if ((game==null)||(sender==null)) return;
            var window = (SFML.Window.Window)sender;
            if (game.curTetromino == null) return;
            if (e.Code == SFML.Window.Keyboard.Key.Escape)
            {
                game.CheckHighScore();

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Left)
            {
                VelH = -1;
                IsOutLimit = game.curTetromino.IsOutLeft;

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Right)
            {
                VelH = 1;
                IsOutLimit = game.curTetromino.IsOutRight;

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Up)
            {

                if (game.curTetromino != null)
                {
                    game.curTetromino.RotateLeft();
                    if (game.curTetromino.HitGround(game.board))
                    {
                        //-- Undo Rotate
                        game.curTetromino.RotateRight();
                    }
                    else if (game.curTetromino.IsOutRight())
                    {
                        var backupX = game.curTetromino.x;
                        //-- Move Inside board
                        while (game.curTetromino.IsOutRight())
                        {
                            game.curTetromino.x--;
                        }
                        if (game.curTetromino.HitGround(game.board))
                        {
                            game.curTetromino.x = backupX;
                            //-- Undo Rotate
                            game.curTetromino.RotateRight();

                        }
                    }
                    else if (game.curTetromino.IsOutLeft())
                    {
                        var backupX = game.curTetromino.x;
                        //-- Move Inside Board
                        while (game.curTetromino.IsOutLeft())
                        {
                            game.curTetromino.x++;
                        }
                        if (game.curTetromino.HitGround(game.board))
                        {
                            game.curTetromino.x = backupX;
                            //-- Undo Rotate
                            game.curTetromino.RotateRight();

                        }

                    }

                }

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Down)
            {
                fFastDown = true;

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Space)
            {
                fDrop = true;

            }

        }
        
        public override void ProcessKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
        {
            if ((game==null)||(sender==null)) return;
            var window = (SFML.Window.Window)sender;
            if ((e.Code == SFML.Window.Keyboard.Key.Left) || (e.Code == SFML.Window.Keyboard.Key.Right))
            {
                VelH = 0;

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Down)
            {
                fFastDown = false;

            }
            
        }

        public override void Draw()
        {
            //--
            if ((game!=null)&&(game.window!=null))
            {
                if (game.curTetromino != null)
                {
                    game.curTetromino.Draw(game.window);
                }

                if (game.nextTetromino != null)
                {
                    game.nextTetromino.Draw(game.window);
                }

                //--
                game.DrawBoard();
                
            }


        }

        public override void Update()
        {

            int curTime;

            _ = game ?? throw new ArgumentNullException(nameof(game));
            
            if (game.curTetromino is not null)
            {
                if (nbCompletedLines > 0)
                {
                    curTime = game.clock.ElapsedTime.AsMilliseconds();
                    if ((curTime - startTimeV) > 500)
                    {
                        startTimeV = curTime;
                        nbCompletedLines--;
                        game.EraseFirstCompletedLine();
                        game.playSuccesSound();
                    }

                }
                else if (horizontalMove != 0)
                {

                    curTime = game.clock.ElapsedTime.AsMilliseconds();

                    if ((curTime - startTimeH) > 20)
                    {
                        for (int i = 0; i < 5; i++)
                        {

                            var backupX = game.curTetromino.x;
                            game.curTetromino.x += horizontalMove;
                            //Console.WriteLine(horizontalMove);
                            if (IsOutLimit())
                            {
                                game.curTetromino.x = backupX;
                                horizontalMove = 0;
                                break;
                            }
                            else
                            {
                                if (game.curTetromino.HitGround(game.board))
                                {
                                    game.curTetromino.x = backupX;
                                    horizontalMove = 0;
                                    break;
                                }
                            }

                            if (horizontalMove != 0)
                            {
                                startTimeH = curTime;
                                if (horizontalStartColumn != game.curTetromino.Column())
                                {
                                    game.curTetromino.x = backupX;
                                    horizontalMove = 0;
                                    break;
                                }

                            }

                        }

                    }

                }
                else if (fDrop)
                {

                    curTime = game.clock.ElapsedTime.AsMilliseconds();
                    if ((curTime - startTimeV) > 10)
                    {
                        startTimeV = curTime;
                        for (int i = 0; i < 6; i++)
                        {
                            //-- Move down to Check
                            game.curTetromino.y++;
                            if (game.curTetromino.HitGround(game.board))
                            {
                                game.curTetromino.y--;
                                nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fDrop = false;
                            }
                            else if (game.curTetromino.IsOutBottom())
                            {
                                game.curTetromino.y--;
                                nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fDrop = false;
                            }
                            if (fDrop && (VelH != 0))
                            {
                                if ((curTime - startTimeH) > 15)
                                {
                                    var backupX = game.curTetromino.x;
                                    game.curTetromino.x += VelH;
                                    if (IsOutLimit())
                                    {
                                        game.curTetromino.x = backupX;
                                    }
                                    else
                                    {
                                        if (game.curTetromino.HitGround(game.board))
                                        {
                                            game.curTetromino.x = backupX;
                                        }
                                        else
                                        {
                                            horizontalMove = VelH;
                                            horizontalStartColumn = game.curTetromino.Column();
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
                    curTime = game.clock.ElapsedTime.AsMilliseconds();

                    int limitElapse = fFastDown ? 10 : 30;

                    if ((curTime - startTimeV) > limitElapse)
                    {

                        startTimeV = curTime;

                        for (int i = 0; i < 3; i++)
                        {
                            //-- Move down to check
                            game.curTetromino.y++;
                            var fMove = true;
                            if (game.curTetromino.HitGround(game.board))
                            {
                                game.curTetromino.y--;
                                nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fMove = false;
                            }
                            else if (game.curTetromino.IsOutBottom())
                            {
                                game.curTetromino.y--;
                                nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fMove = false;
                            }

                            if (fMove)
                            {
                                if (VelH != 0)
                                {
                                    if ((curTime - startTimeH) > 15)
                                    {

                                        var backupX = game.curTetromino.x;
                                        game.curTetromino.x += VelH;

                                        if (IsOutLimit())
                                        {
                                            game.curTetromino.x = backupX;
                                        }
                                        else
                                        {
                                            if (game.curTetromino.HitGround(game.board))
                                            {
                                                game.curTetromino.x -= VelH;
                                            }
                                            else
                                            {
                                                startTimeH = curTime;
                                                horizontalMove = VelH;
                                                horizontalStartColumn = game.curTetromino.Column();
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
    }

}