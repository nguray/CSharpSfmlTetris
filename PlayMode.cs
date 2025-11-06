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

        int m_nbCompletedLines = 0;

        int m_horizontalMove = 0;
        int m_horizontalStartColumn = 0;

        public PlayMode(Game? g)
        {
            game = g;
        }

        public override void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            //----------------------------------------------
            if ((game==null)||(sender==null)) return;
            var window = (SFML.Window.Window)sender;
            if (game.m_curTetromino == null) return;
            if (e.Code == SFML.Window.Keyboard.Key.Escape)
            {
                game.m_idHighScore = game.IsHighScore(game.m_score);
                if (game.m_idHighScore >= 0)
                {
                    game.insertHighScore(game.m_idHighScore, game.m_playerName, game.m_score);
                    game.SetHighScoresMode();
                    game.InitGame();
                }
                else
                {
                    //game.InitGame();
                    game.SetGameOverMode();
                }

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Left)
            {
                VelH = -1;
                IsOutLimit = game.m_curTetromino.IsOutLeft;

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Right)
            {
                VelH = 1;
                IsOutLimit = game.m_curTetromino.IsOutRight;

            }
            else if (e.Code == SFML.Window.Keyboard.Key.Up)
            {

                if (game.m_curTetromino != null)
                {
                    game.m_curTetromino.RotateLeft();
                    if (game.m_curTetromino.HitGround(game.m_board))
                    {
                        //-- Undo Rotate
                        game.m_curTetromino.RotateRight();
                    }
                    else if (game.m_curTetromino.IsOutRight())
                    {
                        var backupX = game.m_curTetromino.x;
                        //-- Move Inside board
                        while (game.m_curTetromino.IsOutRight())
                        {
                            game.m_curTetromino.x--;
                        }
                        if (game.m_curTetromino.HitGround(game.m_board))
                        {
                            game.m_curTetromino.x = backupX;
                            //-- Undo Rotate
                            game.m_curTetromino.RotateRight();

                        }
                    }
                    else if (game.m_curTetromino.IsOutLeft())
                    {
                        var backupX = game.m_curTetromino.x;
                        //-- Move Inside Board
                        while (game.m_curTetromino.IsOutLeft())
                        {
                            game.m_curTetromino.x++;
                        }
                        if (game.m_curTetromino.HitGround(game.m_board))
                        {
                            game.m_curTetromino.x = backupX;
                            //-- Undo Rotate
                            game.m_curTetromino.RotateRight();

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
                if (game.m_curTetromino != null)
                {
                    game.m_curTetromino.Draw(game.window);
                }

                if (game.m_nextTetromino != null)
                {
                    game.m_nextTetromino.Draw(game.window);
                }

                //--
                game.DrawBoard();
                
            }


        }

        public override void Update()
        {

            int curTime;

            if (game.m_curTetromino != null)
            {
                if (m_nbCompletedLines > 0)
                {
                    curTime = game.m_clock.ElapsedTime.AsMilliseconds();
                    if ((curTime - startTimeV) > 500)
                    {
                        startTimeV = curTime;
                        m_nbCompletedLines--;
                        game.EraseFirstCompletedLine();
                        if (game.succesSound != null)
                        {
                            game.succesSound.Play();
                        }
                    }

                }
                else if (m_horizontalMove != 0)
                {

                    curTime = game.m_clock.ElapsedTime.AsMilliseconds();

                    if ((curTime - startTimeH) > 20)
                    {
                        for (int i = 0; i < 5; i++)
                        {

                            var backupX = game.m_curTetromino.x;
                            game.m_curTetromino.x += m_horizontalMove;
                            //Console.WriteLine(horizontalMove);
                            if (IsOutLimit())
                            {
                                game.m_curTetromino.x = backupX;
                                m_horizontalMove = 0;
                                break;
                            }
                            else
                            {
                                if (game.m_curTetromino.HitGround(game.m_board))
                                {
                                    game.m_curTetromino.x = backupX;
                                    m_horizontalMove = 0;
                                    break;
                                }
                            }

                            if (m_horizontalMove != 0)
                            {
                                startTimeH = curTime;
                                if (m_horizontalStartColumn != game.m_curTetromino.Column())
                                {
                                    game.m_curTetromino.x = backupX;
                                    m_horizontalMove = 0;
                                    break;
                                }

                            }

                        }

                    }

                }
                else if (fDrop)
                {

                    curTime = game.m_clock.ElapsedTime.AsMilliseconds();
                    if ((curTime - startTimeV) > 10)
                    {
                        startTimeV = curTime;
                        for (int i = 0; i < 6; i++)
                        {
                            //-- Move down to Check
                            game.m_curTetromino.y++;
                            if (game.m_curTetromino.HitGround(game.m_board))
                            {
                                game.m_curTetromino.y--;
                                m_nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fDrop = false;
                            }
                            else if (game.m_curTetromino.IsOutBottom())
                            {
                                game.m_curTetromino.y--;
                                m_nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fDrop = false;
                            }
                            if (fDrop && (VelH != 0))
                            {
                                if ((curTime - startTimeH) > 15)
                                {
                                    var backupX = game.m_curTetromino.x;
                                    game.m_curTetromino.x += VelH;
                                    if (IsOutLimit())
                                    {
                                        game.m_curTetromino.x = backupX;
                                    }
                                    else
                                    {
                                        if (game.m_curTetromino.HitGround(game.m_board))
                                        {
                                            game.m_curTetromino.x = backupX;
                                        }
                                        else
                                        {
                                            m_horizontalMove = VelH;
                                            m_horizontalStartColumn = game.m_curTetromino.Column();
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
                    curTime = game.m_clock.ElapsedTime.AsMilliseconds();

                    int limitElapse = fFastDown ? 10 : 30;

                    if ((curTime - startTimeV) > limitElapse)
                    {

                        startTimeV = curTime;

                        for (int i = 0; i < 3; i++)
                        {
                            //-- Move down to check
                            game.m_curTetromino.y++;
                            var fMove = true;
                            if (game.m_curTetromino.HitGround(game.m_board))
                            {
                                game.m_curTetromino.y--;
                                m_nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fMove = false;
                            }
                            else if (game.m_curTetromino.IsOutBottom())
                            {
                                game.m_curTetromino.y--;
                                m_nbCompletedLines = game.FreezeCurTetromino();
                                game.NewTetromino();
                                fMove = false;
                            }

                            if (fMove)
                            {
                                if (VelH != 0)
                                {
                                    if ((curTime - startTimeH) > 15)
                                    {

                                        var backupX = game.m_curTetromino.x;
                                        game.m_curTetromino.x += VelH;

                                        if (IsOutLimit())
                                        {
                                            game.m_curTetromino.x = backupX;
                                        }
                                        else
                                        {
                                            if (game.m_curTetromino.HitGround(game.m_board))
                                            {
                                                game.m_curTetromino.x -= VelH;
                                            }
                                            else
                                            {
                                                startTimeH = curTime;
                                                m_horizontalMove = VelH;
                                                m_horizontalStartColumn = game.m_curTetromino.Column();
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