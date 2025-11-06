
using System;
using System.Reflection;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;


namespace SfmlTetris
{
    enum GameMode
    {
        STANDBY = 0,
        PLAY,
        HIGH_SCORES,
        GAME_OVER
    }


   static class Globals{

        public const int WIN_WIDTH = 480;
        public const int WIN_HEIGHT = 560;
  
        public const int NB_ROWS = 20;
        public const int NB_COLUMNS = 12;

        public const int LEFT = 10;
        public const int TOP = 10;

        public static int cellSize = Globals.WIN_WIDTH / (Globals.NB_COLUMNS + 7);

        public static Random? rand;

    }

    class Game
    {
        private VideoMode mode;
        const string TITLE = "Tetris using SFML";

        public int startTimeV = 0;
        public int startTimeH = 0;
        public int startTimeR = 0;

        public int[] m_board = new int[Globals.NB_COLUMNS * Globals.NB_ROWS];
        public int m_score = 0;

        public Int32 idTetrominoBag = 14;
        public Int32[] tetrominoBag = { 1, 2, 3, 4, 5, 6, 7, 1, 2, 3, 4, 5, 6, 7 };

        public Clock m_clock = new Clock();

        public SoundBuffer? succesSoundBuff;
        public Music? music;
        public Sound? succesSound = new Sound();
        public Font? myFont;

        public GameMode m_mode = GameMode.STANDBY;

        private StandByMode? m_standbyMode;
        private PlayMode? m_playMode;
        private HighScoresMode? m_highScoresMode;
        private GameOverMode? m_gameOverMode;

        public IGameMode? curGameMode = null;

        public RenderWindow? window;

        public List<HighScore> m_highScores = new List<HighScore>();
        public Int32 m_idHighScore = -1;
        public String m_playerName = "";
        public uint m_i_color = 0;

        public Tetromino? m_curTetromino;
        public Tetromino? m_nextTetromino;

        public Game()
        {
            m_standbyMode = new StandByMode(this);
            m_playMode = new PlayMode(this);
            m_highScoresMode = new HighScoresMode(this);
            m_gameOverMode = new GameOverMode(this);
            curGameMode = m_standbyMode;

            loadHighScores();
            m_highScores.Sort(delegate (HighScore h1, HighScore h2)
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

            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(@"CSharpSfmlTetris.sansation.ttf"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream("sansation.ttf", FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }

            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(@"CSharpSfmlTetris.109662__grunz__success.wav"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream("109662__grunz__success.wav", FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }

            using (Stream? myStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream(@"CSharpSfmlTetris.Nutcracker-song.ogg"))
            {
                if (myStream is not null)
                {
                    using var fileStream = new FileStream("Nutcracker-song.ogg", FileMode.Create, FileAccess.Write);
                    {
                        myStream.CopyTo(fileStream);
                        fileStream.Close();
                    }
                    myStream.Dispose();
                }
            }

            string filePath = "109662__grunz__success.wav";
            succesSoundBuff = new SoundBuffer(filePath);
            filePath = "Nutcracker-song.ogg";
            music = new Music(filePath);
            filePath = "sansation.ttf";
            myFont = new Font(filePath);

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
            m_score = 0;

            for (int i = 0; i < m_board.Length; i++)
            {
                m_board[i] = 0;
            }
            startTimeV = 0;
            startTimeH = startTimeV;
            startTimeR = startTimeV;

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

        public void loadHighScores()
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

                    m_highScores.Clear();

                    foreach (string line in System.IO.File.ReadLines(path))
                    {
                        //--
                        (name, score) = ParseHighScore(line);
                        m_highScores.Add(new HighScore(name, score));
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
                    m_highScores.Add(new HighScore("XXXXX", 0));
                }

            }
        }

        public void writeScoreLine(FileStream fs, string value)
        {
            //------------------------------------------------------
            byte[] info = new UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        public void saveHighScores()
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
                foreach (var h in m_highScores)
                {
                    lin = String.Format("{0},{1}\n", h.Name, h.Score);
                    writeScoreLine(fs, lin);
                }

            }


        }

        public void insertHighScore(int id, String name, int score)
        {
            if ((id >= 0) && (id < 10))
            {
                m_highScores.Insert(id, new HighScore(name, score));
                if (m_highScores.Count > 10)
                {
                    m_highScores.RemoveAt(m_highScores.Count - 1);
                }
            }
        }

        public Int32 IsHighScore(int score)
        {
            //---------------------------------------------------
            for (int i = 0; i < 10; i++)
            {
                if (score > m_highScores[i].Score)
                {
                    return i;
                }
            }
            return -1;
        }


        public void endGame()
        {
            //-------------------------
            if (music != null)
            {
                music.Stop();
            }
            saveHighScores();

            if (window != null) window.Close();

        }

        public void SetStandbyMode()
        {
            m_mode = GameMode.STANDBY;
            curGameMode = m_standbyMode;
        }

        public void SetPlayMode()
        {
            m_mode = GameMode.PLAY;
            curGameMode = m_playMode;
        }

        public void SetHighScoresMode()
        {
            m_mode = GameMode.HIGH_SCORES;
            curGameMode = m_highScoresMode;

        }

        public void SetGameOverMode()
        {
            m_mode = GameMode.GAME_OVER;
            curGameMode = m_gameOverMode;
        }

        public bool isGameOver()
        {
            //----------------------------------------
            for (int c = 0; c < Globals.NB_COLUMNS; c++)
            {
                if (m_board[c] != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void DrawBoard()
        {

            var a = Globals.cellSize - 2;
            RectangleShape r1 = new RectangleShape(new Vector2f(a, a));

            for (int l = 0; l < Globals.NB_ROWS; l++)
            {
                for (int c = 0; c < Globals.NB_COLUMNS; c++)
                {
                    var typ = m_board[l * Globals.NB_COLUMNS + c];
                    if (typ != 0)
                    {
                        r1.FillColor = Tetromino.Colors[typ];
                        r1.Position = new Vector2f(c * Globals.cellSize + Globals.LEFT + 1, l * Globals.cellSize + Globals.TOP + 1);
                        window.Draw(r1);
                    }
                }
            }

        }

        public Int32 tetrisRandomizer()
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
                for (int i = 0; i < tetrominoBag.Length; i++)
                {
                    iSrc = Globals.rand.Next(0, 14);
                    iTyp = tetrominoBag[iSrc];
                    tetrominoBag[iSrc] = tetrominoBag[0];
                    tetrominoBag[0] = iTyp;
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

        public void drawCurrentScore()
        {
            //---------------------------------------------------
            if (window != null)
            {
                var textScore = new Text(String.Format("Score : {0:00000}", m_score), myFont, 20);
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
            if (m_nextTetromino == null)
            {
                m_nextTetromino = new Tetromino(tetrisRandomizer(), (Globals.NB_COLUMNS + 3) * Globals.cellSize, 10 * Globals.cellSize);
            }
            m_curTetromino = m_nextTetromino;
            m_curTetromino.x = 6 * Globals.cellSize;
            m_curTetromino.y = 0;
            m_curTetromino.y = -m_curTetromino.MaxY1() * Globals.cellSize;
            m_nextTetromino = new Tetromino(tetrisRandomizer(), (Globals.NB_COLUMNS + 3) * Globals.cellSize, 10 * Globals.cellSize);

        }

        public void Update()
        {
            //-----------------------------------------
            var curTime = m_clock.ElapsedTime.AsMilliseconds();
            if ((curTime - startTimeR) > 500)
            {
                startTimeR = curTime;
                if (m_nextTetromino != null)
                {
                    m_nextTetromino.RotateLeft();
                }
                m_i_color++;

            }

            //-- Check Game Over
            if (isGameOver())
            {
                m_idHighScore = IsHighScore(m_score);
                if (m_idHighScore >= 0)
                {
                    insertHighScore(m_idHighScore, m_playerName, m_score);
                    SetHighScoresMode();
                    InitGame();
                }
                else
                {
                    InitGame();
                    SetGameOverMode();
                }

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
                    if (m_board[r * Globals.NB_COLUMNS + c] == 0)
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
                    if (m_board[r * Globals.NB_COLUMNS + c] == 0)
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
                            m_board[r1 * Globals.NB_COLUMNS + c1] = m_board[(r1 - 1) * Globals.NB_COLUMNS + c1];
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
            if (m_curTetromino != null)
            {
                var ix = (m_curTetromino.x + 1) / Globals.cellSize;
                var iy = (m_curTetromino.y + 1) / Globals.cellSize;
                foreach (var v in m_curTetromino.vectors)
                {
                    var x = v.X + ix;
                    var y = v.Y + iy;
                    if ((x >= 0) && (x < Globals.NB_COLUMNS) && (y >= 0) && (y < Globals.NB_ROWS))
                    {
                        m_board[y * Globals.NB_COLUMNS + x] = m_curTetromino.type;
                    }
                }
                //--
                nbCompletedLines = ComputeCompledLines();
                if (nbCompletedLines > 0)
                {
                    m_score += ComputeScore(nbCompletedLines);

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


            window.Closed += (StringReader, args) => endGame();
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

                if (curGameMode!=null)
                {
                    curGameMode.Update();                
                    curGameMode.Draw();
                }

                Update();

                //--
                drawCurrentScore();

                //draw();
                window.Display();


            }

        }

        void OnKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
        {
            if (curGameMode != null)
            {
                curGameMode.ProcessKeyPressed(sender, e);
            }
        }
        void OnKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
        {
            if (curGameMode != null)
            {
                curGameMode.ProcessKeyReleased(sender, e);
            }
        }


    }

    abstract class IGameMode
    {
        public Game? game;
        public abstract void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e);
        public virtual void ProcessKeyReleased(object? sender, SFML.Window.KeyEventArgs e) {}
        public virtual void Update() {}
        public abstract void Draw();
    }

    class Program
    {


        static void Main(string[] args)
        {
            //--------------------------------------------
            var app = new Game();
            app.Run();

        }


    }

}