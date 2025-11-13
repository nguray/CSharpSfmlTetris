
using System.Reflection;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using System.IO;
using System.Text;

namespace SfmlTetris
{

    class Game
    {
        private VideoMode mode;
        const string TITLE = "Tetris using SFML";
        const string SANSATION_TTF = "sansation.ttf";
        const string NUTCRACKER_OGG = "Nutcracker-song.ogg";
        const string SUCCES_WAV = "109662__grunz__success.wav";


        public int startTimeR = 0;

        public int[] board = new int[Globals.NB_COLUMNS * Globals.NB_ROWS];
        public int score = 0;

        public Int32 idTetrominoBag = 14;
        public Int32[] tetrominoBag = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };

        public Clock clock = new Clock();

        public SoundBuffer? succesSoundBuff;
        public Music? music;
        public Sound? succesSound = new Sound();
        public Font? myFont;

        private StandByMode? standbyMode;
        private PlayMode? playMode;
        private HighScoresMode? highScoresMode;
        private GameOverMode? gameOverMode;

        public IGameMode? curGameMode = null;

        public RenderWindow? window;

        public List<HighScore> highScores = new List<HighScore>();
        public Int32 idHighScore = -1;
        public String playerName = "";
        public uint i_color = 0;

        public Tetromino? curTetromino;
        public Tetromino? nextTetromino;

        private void getEmbeddedResource(string resourceName)
        {
            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"CSharpSfmlTetris.res.{resourceName}"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream(resourceName, FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }

        }

        public Game()
        {
            standbyMode = new StandByMode(this);
            playMode = new PlayMode(this);
            highScoresMode = new HighScoresMode(this);
            gameOverMode = new GameOverMode(this);
            curGameMode = standbyMode;

            LoadHighScores();
            highScores.Sort(delegate (HighScore h1, HighScore h2)
            {
                if (h1.Score > h2.Score)
                { //-- Tri Décroissant
                    return -1;
                }
                else if (h1.Score < h2.Score)
                {
                    return 1;
                }
                return 0;
            });


            getEmbeddedResource(SANSATION_TTF);
            getEmbeddedResource(SUCCES_WAV);
            getEmbeddedResource(NUTCRACKER_OGG);

            string filePath = SUCCES_WAV;
            succesSoundBuff = new SoundBuffer(filePath);
            filePath    = NUTCRACKER_OGG;
            music       = new Music(filePath);
            filePath    = SANSATION_TTF;
            myFont      = new Font(filePath);

            if (succesSoundBuff != null)
            {
                if (succesSound != null)
                {
                    succesSound.SoundBuffer = succesSoundBuff;
                    succesSound.Volume = 15.0f;
                }
            }

            if (music is not null)
            {
                music.Volume = 40.0f;
                music.Loop = true;
                music.Play();
            }


        }

        public void InitGame()
        {
            //--------------------------------------------------------
            score = 0;

            for (int i = 0; i < board.Length; i++)
            {
                board[i] = 0;
            }

        }

        public void EndGame()
        {
            //-------------------------
            if (music is not null)
            {
                music.Stop();
                music.Dispose();
            }
            if (succesSoundBuff is not null)
            {
                if (succesSound is not  null)
                {
                    succesSound.Dispose();
                }
                succesSoundBuff.Dispose();
            }
            SaveHighScores();

            if (myFont is not null)
            {
                myFont.Dispose();
            }


            //-- 
            if (File.Exists(SANSATION_TTF))
            {
                File.Delete(SANSATION_TTF);                
            }

            if (File.Exists(SUCCES_WAV))
            {
                File.Delete(SUCCES_WAV);                
            }

            if (File.Exists(NUTCRACKER_OGG))
            {
                File.Delete(NUTCRACKER_OGG);
            }


            window?.Close();

        }

        public (string Name, int score) ParseHighScore(string line)
        {
            //------------------------------------------------------                
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            string[] words = line.Split(delimiterChars);
            string n = words[0];
            int s = int.Parse(words[1]);
            return (n, s);

        }

        public void LoadHighScores()
        {
            int iLine = 0;
            string name;
            int score;
            string path = "HighScores.txt";
            //------------------------------------------------------
            if (File.Exists(path))
            {
                try
                {

                    highScores.Clear();

                    foreach (string line in System.IO.File.ReadLines(path))
                    {
                        //--
                        (name, score) = ParseHighScore(line);
                        highScores.Add(new HighScore(name, score));
                        //--
                        iLine++;
                        if (iLine > 9) break;

                    }
                }
                catch (FileNotFoundException uAEx)
                {
                    Console.WriteLine(uAEx.Message);
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    //--
                    highScores.Add(new HighScore("XXXXX", 0));
                }

            }
        }

        public void WriteScoreLine(FileStream fs, string value)
        {
            //------------------------------------------------------
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        public void SaveHighScores()
        {
            string path = @"HighScores.txt";
            //------------------------------------------------------
            // Delete the file if it exists.
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (FileStream fs = File.Create(path))
            {
                String lin;
                foreach (var h in highScores)
                {
                    lin = String.Format("{0},{1}\n", h.Name, h.Score);
                    WriteScoreLine(fs, lin);
                }

            }


        }

        public void InsertHighScore(int id, String name, int score)
        {
            if ((id >= 0) && (id < 10))
            {
                highScores.Insert(id, new HighScore(name, score));
                if (highScores.Count > 10)
                {
                    highScores.RemoveAt(highScores.Count - 1);
                }
            }
        }

        public Int32 IsHighScore(int score)
        {
            //---------------------------------------------------
            for (int i = 0; i < 10; i++)
            {
                if (score > highScores[i].Score)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SetStandbyMode()
        {
            curGameMode = standbyMode;
            curGameMode?.Init();
        }

        public void SetPlayMode()
        {
            curGameMode = playMode;
            curGameMode?.Init();
        }

        public void SetHighScoresMode()
        {
            curGameMode = highScoresMode;
            curGameMode?.Init();
        }

        public void SetGameOverMode()
        {
            curGameMode = gameOverMode;
            curGameMode?.Init();
        }

        public bool IsGameOver()
        {
            //----------------------------------------
            for (int c = 0; c < Globals.NB_COLUMNS; c++)
            {
                if (board[c] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void DrawBoard()
        {

            if (window is not RenderWindow win) return;

            var a = Globals.cellSize - 2;
            RectangleShape r1 = new RectangleShape(new Vector2f(a, a));

            for (int l = 0; l < Globals.NB_ROWS; l++)
            {
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    var typ = board[l * Globals.NB_COLUMNS + c];
                    if (typ != 0)
                    {
                        r1.FillColor = Tetromino.Colors[typ];
                        r1.Position = new Vector2f(c * Globals.cellSize + Globals.LEFT + 1, l * Globals.cellSize + Globals.TOP + 1);
                        win.Draw(r1);
                    }
                }
            }

        }

        public Int32 TetrisRandomizer()
        {
            Int32 iSrc;
            Int32 iTyp = 0;
            if (idTetrominoBag < 14)
            {
                iTyp = tetrominoBag[idTetrominoBag];
                idTetrominoBag++;
            }
            else
            {
                //-- Shuttle bag
                if (Globals.rand!=null)
                {
                    for (int i = 0; i < tetrominoBag.Length; i++)
                    {
                        iSrc = Globals.rand.Next(0, 14);
                        iTyp = tetrominoBag[iSrc];
                        tetrominoBag[iSrc] = tetrominoBag[0];
                        tetrominoBag[0] = iTyp;
                    }                    
                }
                iTyp = tetrominoBag[0];
                idTetrominoBag = 1;

            }
            return iTyp;
        }

        public Int32 ComputeScore(Int32 nbLines)
        {
            switch (nbLines)
            {
                case 0:
                    return 0;
                case 1:
                    return 40;
                case 2:
                    return 100;
                case 3:
                    return 300;
                case 4:
                    return 1200;
                default:
                    return 2000;
            }
        }

        public void DrawCurrentScore()
        {
            //---------------------------------------------------
            if (window != null)
            {
                var textScore = new Text(String.Format("Score : {0:00000}", score), myFont, 20);
                if (textScore != null)
                {
                    textScore.FillColor = new Color(255, 223, 0);
                    textScore.Style = Text.Styles.Bold | Text.Styles.Italic;
                    textScore.Position = new Vector2f(Globals.LEFT + Globals.cellSize * Globals.NB_COLUMNS + 10, Globals.TOP);
                    window.Draw(textScore);
                }
            }

        }

        public void NewTetromino()
        {
            if (nextTetromino == null)
            {
                nextTetromino = new Tetromino(TetrisRandomizer(), (Globals.NB_COLUMNS + 3) * Globals.cellSize, 10 * Globals.cellSize);
            }
            curTetromino = nextTetromino;
            curTetromino.x = 6 * Globals.cellSize;
            curTetromino.y = 0;
            curTetromino.y = -curTetromino.MaxY1() * Globals.cellSize;
            nextTetromino = new Tetromino(TetrisRandomizer(), (Globals.NB_COLUMNS + 3) * Globals.cellSize, 10 * Globals.cellSize);
        }

        public void Update()
        {
            //-----------------------------------------
            var curTime = clock.ElapsedTime.AsMilliseconds();
            if ((curTime - startTimeR) > 500)
            {
                startTimeR = curTime;
                if (nextTetromino != null)
                {
                    nextTetromino.RotateRight();
                }
                i_color++;

            }

            //-- Check Game Over
            if (IsGameOver())
            {
                CheckHighScore();

            }


        }

        public void CheckHighScore()
        {
            idHighScore = IsHighScore(score);
            if (idHighScore >= 0)
            {
                InsertHighScore(idHighScore, playerName, score);
                SetHighScoresMode();
                InitGame();
            }
            else
            {
                //game.InitGame();
                SetGameOverMode();
            }
            
        }

        public Int32 ComputeCompledLines()
        {

            Int32 nbLines = 0;
            bool fCompleted;
            for (int r = 0; r < Globals.NB_ROWS; r++)
            {
                fCompleted = true;
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    if (board[r * Globals.NB_COLUMNS + c] == 0)
                    {
                        fCompleted = false;
                        break;
                    }
                }
                if (fCompleted)
                {
                    nbLines++;
                }
            }
            return nbLines;
        }

        public void EraseFirstCompletedLine()
        {
            //---------------------------------------------------
            bool fCompleted = false;
            for (int r = 0; r < Globals.NB_ROWS; r++)
            {
                fCompleted = true;
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    if (board[r * Globals.NB_COLUMNS + c] == 0)
                    {
                        fCompleted = false;
                        break;
                    }
                }
                if (fCompleted)
                {
                    //-- Décaler d'une ligne le plateau
                    for (int r1 = r; r1 > 0; r1--)
                    {
                        for (int c1 = 0; c1 < Globals.NB_COLUMNS; c1++)
                        {
                            board[r1 * Globals.NB_COLUMNS + c1] = board[(r1 - 1) * Globals.NB_COLUMNS + c1];
                        }
                    }
                    return;
                }
            }
        }

        public int FreezeCurTetromino()
        {
            int nbCompletedLines = 0;
            //----------------------------------------------------
            if (curTetromino != null)
            {
                var ix = (curTetromino.x + 1) / Globals.cellSize;
                var iy = (curTetromino.y + 1) / Globals.cellSize;
                foreach (var v in curTetromino.vectors)
                {
                    var x = v.X + ix;
                    var y = v.Y + iy;
                    if ((x >= 0) && (x < Globals.NB_COLUMNS) && (y >= 0) && (y < Globals.NB_ROWS))
                    {
                        board[y * Globals.NB_COLUMNS + x] = curTetromino.type;
                    }
                }
                //--
                nbCompletedLines = ComputeCompledLines();
                if (nbCompletedLines > 0)
                {
                    score += ComputeScore(nbCompletedLines);

                }
            }
            return nbCompletedLines;

        }

        public void Run()
        {

            var names =
                System
                .Reflection
                .Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames();

            foreach (var name in names)
            {
                Console.WriteLine(name);
            }


            mode = new VideoMode(Globals.WIN_WIDTH, Globals.WIN_HEIGHT);

            window = new RenderWindow(mode, TITLE);

            window.SetVerticalSyncEnabled(true);
            window.SetFramerateLimit(60);


            //-- Get board size
            Vector2u s = window.Size;

            DateTime randSeed = DateTime.Now;
            Globals.rand = new Random(randSeed.Millisecond);

            //-- Init Game
            InitGame();

            window.Closed += (StringReader, args) => EndGame();
            window.KeyReleased += OnKeyReleased;

            window.KeyPressed += OnKeyPressed;

            SetStandbyMode();


            while (window.IsOpen)
            {

                window.Clear(new Color(64, 64, 255));

                RectangleShape r0 = new RectangleShape(new Vector2f(Globals.NB_COLUMNS * Globals.cellSize, Globals.NB_ROWS * Globals.cellSize));
                r0.Position = new Vector2f(Globals.LEFT, Globals.TOP);
                r0.FillColor = new Color(10, 10, 100);
                window.Draw(r0);

                //--
                window.DispatchEvents();


                if (curGameMode != null)
                {
                    curGameMode.Update();

                    curGameMode.Draw();
                }

                Update();


                //--
                DrawCurrentScore();

                //draw();
                window.Display();


            }
            

        }

        void OnKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            curGameMode?.ProcessKeyPressed(sender, e);
        }
        void OnKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
        {
            curGameMode?.ProcessKeyReleased(sender, e);
        }

        public void playSuccesSound()
        {
            succesSound?.Play();
        }

    }
}