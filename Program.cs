
using System;
using System.Reflection;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Audio;
using System.IO;
using System.Text;


namespace SfmlTetris
{

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

    class Program
    {

        const string TITLE = "Tetris";

        static void Main(string[] args)
        {

            //Console.WriteLine("Hello, World!");
            var game = new MyGame();
            game.Run();

        }

        class MyGame
        {

            enum GameMode {
                STANDBY=0,
                PLAY,
                AUTO,
                HIGH_SCORES,
                GAME_OVER
            }

            private GameMode m_mode = GameMode.STANDBY;

            private int left, top, right, bottom;
            
            private int m_cell_size = 5;
            private int m_score = 0;
            private int m_idHightScore = -1;

            private int m_x_center = 0;

            private Tetromino? m_curTetromino;
            private Tetromino? m_nextTetromino;
            private Clock m_clock = new Clock();

            private VideoMode mode;            
            private RenderWindow? window;

             private SoundBuffer? succesSoundBuff;
            private Music? music;
            private Sound? succesSound = new Sound();
            private Font? myFont;
            private Text? textScore;

            private List<HighScore> m_highScores = new List<HighScore>();

            private Int32  m_idHighScore = -1;
            private String m_playerName="";
            private uint m_i_color = 0;

            private int VelH = 0;
            private bool fFastDown = false;
            private bool fDrop = false;


            public static bool m_fFastDown = false;
            public static bool m_fDrop = false;
            public static Int32 m_nbCompletedLines = 0;
            Int32 iColorHighScore = 0;

            public static int[] m_board = new int[Globals.NB_COLUMNS*Globals.NB_ROWS];

            public static Int32 m_horizontalMove = 0;
            public static Int32 m_horizontalStartColumn = 0;

            public delegate bool IsOutLimit_t();
            static IsOutLimit_t? IsOutLimit;


            static public Int32 idTetrominoBag = 14;
            static public Int32[] tetrominoBag = {1,2,3,4,5,6,7,1,2,3,4,5,6,7};  

            private delegate void ProcessKey(object? sender, SFML.Window.KeyEventArgs e);

            private ProcessKey? processKeyPressedProc;
            private ProcessKey? processKeyReleasedProc;

            public void SetStandbyMode()
            {
                m_mode = GameMode.STANDBY;
                processKeyPressedProc = processKeyPressedStandbyMode;
                processKeyReleasedProc = processKeyReleasedStandbyMode;                
            }

            public void SetPlayMode()
            {
                m_mode = GameMode.PLAY;
                processKeyPressedProc = processKeyPressedPlayMode;
                processKeyReleasedProc = processKeyReleasedPlayMode;                
            }

            public void SetHighScoresMode()
            {
                m_mode = GameMode.HIGH_SCORES;
                processKeyPressedProc = processKeyPressedHighScoresMode;
                processKeyReleasedProc = processKeyReleasedHighScoresMode;         

            }

            public void SetGameOverMode()
            {
                m_mode = GameMode.GAME_OVER;
                processKeyPressedProc = processKeyPressedGameOverMode;
                processKeyReleasedProc = processKeyReleasedGameOverMode;                
            }


            private void InitGame(){

                m_score = 0;

                for(int i=0;i<m_board.Length;i++){
                    m_board[i] = 0;
                }

                m_curTetromino = null;
                m_nextTetromino = new Tetromino(TetrisRandomizer(),(Globals.NB_COLUMNS+3)*Globals.cellSize,10*Globals.cellSize);

            }

            private void NewTetromino(){

                m_curTetromino = m_nextTetromino;
                m_curTetromino.x = 6 * Globals.cellSize;
                m_curTetromino.y = 0;
                m_curTetromino.y = -m_curTetromino.MaxY1() * Globals.cellSize;
                m_nextTetromino = new Tetromino(TetrisRandomizer(),(Globals.NB_COLUMNS+3)*Globals.cellSize,10*Globals.cellSize);

            }

            private void DrawBoard(RenderWindow window){

                var a = Globals.cellSize - 2;
                RectangleShape r1 = new RectangleShape(new Vector2f( a, a));

                for(int l=0;l<Globals.NB_ROWS;l++){
                    for(int c=0;c<Globals.NB_COLUMNS;c++){
                        var typ = m_board[l*Globals.NB_COLUMNS+c];
                        if (typ!=0){
                            r1.FillColor = Tetromino.Colors[typ];
                            r1.Position = new Vector2f(c*Globals.cellSize + Globals.LEFT + 1, l*Globals.cellSize + Globals.TOP + 1);
                            window.Draw(r1);
                        }
                    }
                }

            }

            static Int32 ComputeScore(Int32 nbLines){
                switch(nbLines){
                    case 0 :
                        return 0;
                    case 1 :
                        return 40;
                    case 2 :
                        return 100;
                    case 3 :
                        return 300;
                    case 4 :
                        return 1200;
                    default:
                        return 2000; 
                }
            }


            static Int32 ComputeCompledLines(){

                Int32 nbLines = 0;
                bool fCompleted = false;
                for(int r=0;r<Globals.NB_ROWS;r++){
                    fCompleted = true;
                    for(int c=0;c<Globals.NB_COLUMNS;c++){
                        if (m_board[r*Globals.NB_COLUMNS+c]==0){
                            fCompleted = false;
                            break;
                        }
                    }
                    if (fCompleted){
                        nbLines++;
                    }
                }
                return nbLines;
            }

            static void EraseFirstCompletedLine(){
                //---------------------------------------------------
                bool fCompleted = false;
                for(int r=0;r<Globals.NB_ROWS;r++){
                    fCompleted = true;
                    for(int c=0;c<Globals.NB_COLUMNS;c++){
                        if (m_board[r*Globals.NB_COLUMNS+c]==0){
                            fCompleted = false;
                            break;
                        }
                    }
                    if (fCompleted){
                        //-- Décaler d'une ligne le plateau
                        for(int r1=r;r1>0;r1--){
                            for(int c1=0;c1<Globals.NB_COLUMNS;c1++){
                                m_board[r1*Globals.NB_COLUMNS+c1] = m_board[(r1-1)*Globals.NB_COLUMNS+c1];
                            }
                        }
                        return;
                    }
                }
            } 


            private void FreezeCurTetromino()
            {
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
                    m_nbCompletedLines = ComputeCompledLines();
                    if (m_nbCompletedLines > 0)
                    {
                        m_score += ComputeScore(m_nbCompletedLines);

                    }
                }

            }

            public static bool isGameOver()
            {
                //----------------------------------------
                for(int c=0;c<Globals.NB_COLUMNS;c++){
                    if (m_board[c]!=0){
                        return true;
                    }
                }
                return false;
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

                mode = new VideoMode(Globals.WIN_WIDTH, Globals.WIN_HEIGHT);

                window = new RenderWindow(mode, TITLE);

                window.SetVerticalSyncEnabled(true);
                window.SetFramerateLimit(60);

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


                //-- Get board size
                Vector2u s = window.Size;

                left = Globals.LEFT;
                right = left + Globals.cellSize * Globals.NB_COLUMNS;
                m_x_center = (int)(left + Globals.cellSize * Globals.NB_COLUMNS / 2);
                top = Globals.TOP;
                bottom = top + m_cell_size * Globals.NB_ROWS;

                DateTime randSeed = DateTime.Now;
                Globals.rand = new Random(randSeed.Millisecond);

                //-- Init Game

                InitGame();

                loadHighScores();


                window.Closed += (StringReader, args) => endGame();
                window.KeyReleased += OnKeyReleased;

                window.KeyPressed += OnKeyPressed;

                SetStandbyMode();

                int startTimeV = m_clock.ElapsedTime.AsMilliseconds();
                int startTimeH = startTimeV;
                int startTimeR = startTimeV;

                int curTime = 0;


                while (window.IsOpen)
                {


                    window.Clear(new Color(64, 64, 255));

                    RectangleShape r0 = new RectangleShape(new Vector2f(Globals.NB_COLUMNS * Globals.cellSize, Globals.NB_ROWS * Globals.cellSize));
                    r0.Position = new Vector2f(Globals.LEFT, Globals.TOP);
                    r0.FillColor = new Color(10, 10, 100);
                    window.Draw(r0);

                    //--
                    window.DispatchEvents();

                    if (m_mode == GameMode.PLAY)
                    {

                        if (m_curTetromino != null)
                        {

                            if (m_nbCompletedLines > 0)
                            {
                                curTime = m_clock.ElapsedTime.AsMilliseconds();
                                if ((curTime - startTimeV) > 500)
                                {
                                    startTimeV = curTime;
                                    m_nbCompletedLines--;
                                    EraseFirstCompletedLine();
                                    if (succesSound != null)
                                    {
                                        succesSound.Play();
                                    }
                                }

                            }
                            else if (m_horizontalMove != 0)
                            {

                                curTime = m_clock.ElapsedTime.AsMilliseconds();

                                if ((curTime - startTimeH) > 20)
                                {
                                    for (int i = 0; i < 5; i++)
                                    {

                                        var backupX = m_curTetromino.x;
                                        m_curTetromino.x += m_horizontalMove;
                                        //Console.WriteLine(horizontalMove);
                                        if (IsOutLimit())
                                        {
                                            m_curTetromino.x = backupX;
                                            m_horizontalMove = 0;
                                            break;
                                        }
                                        else
                                        {
                                            if (m_curTetromino.HitGround(m_board))
                                            {
                                                m_curTetromino.x = backupX;
                                                m_horizontalMove = 0;
                                                break;
                                            }
                                        }

                                        if (m_horizontalMove != 0)
                                        {
                                            startTimeH = curTime;
                                            if (m_horizontalStartColumn != m_curTetromino.Column())
                                            {
                                                m_curTetromino.x = backupX;
                                                m_horizontalMove = 0;
                                                break;
                                            }

                                        }

                                    }

                                }

                            }
                            else if (fDrop)
                            {

                                curTime = m_clock.ElapsedTime.AsMilliseconds();
                                if ((curTime - startTimeV) > 10)
                                {
                                    startTimeV = curTime;
                                    for (int i = 0; i < 6; i++)
                                    {
                                        //-- Move down to Check
                                        m_curTetromino.y++;
                                        if (m_curTetromino.HitGround(m_board))
                                        {
                                            m_curTetromino.y--;
                                            FreezeCurTetromino();
                                            NewTetromino();
                                            fDrop = false;
                                        }
                                        else if (m_curTetromino.IsOutBottom())
                                        {
                                            m_curTetromino.y--;
                                            FreezeCurTetromino();
                                            NewTetromino();
                                            fDrop = false;
                                        }
                                        if ((fDrop) && (VelH != 0))
                                        {
                                            if ((curTime - startTimeH) > 15)
                                            {
                                                var backupX = m_curTetromino.x;
                                                m_curTetromino.x += VelH;
                                                if (IsOutLimit())
                                                {
                                                    m_curTetromino.x = backupX;
                                                }
                                                else
                                                {
                                                    if (m_curTetromino.HitGround(m_board))
                                                    {
                                                        m_curTetromino.x = backupX;
                                                    }
                                                    else
                                                    {
                                                        m_horizontalMove = VelH;
                                                        m_horizontalStartColumn = m_curTetromino.Column();
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
                                curTime = m_clock.ElapsedTime.AsMilliseconds();

                                int limitElapse;
                                if (m_fFastDown)
                                {
                                    limitElapse = 20;
                                }
                                else
                                {
                                    limitElapse = 50;
                                }

                                if ((curTime - startTimeV) > limitElapse)
                                {
                                    startTimeV = curTime;

                                    for (int i = 0; i < 4; i++)
                                    {
                                        //-- Move down to check
                                        m_curTetromino.y++;
                                        var fMove = true;
                                        if (m_curTetromino.HitGround(m_board))
                                        {
                                            m_curTetromino.y--;
                                            FreezeCurTetromino();
                                            NewTetromino();
                                            fMove = false;
                                        }
                                        else if (m_curTetromino.IsOutBottom())
                                        {
                                            m_curTetromino.y--;
                                            FreezeCurTetromino();
                                            NewTetromino();
                                            fMove = false;
                                        }

                                        if (fMove)
                                        {
                                            if (VelH != 0)
                                            {
                                                if ((curTime - startTimeH) > 15)
                                                {

                                                    var backupX = m_curTetromino.x;
                                                    m_curTetromino.x += VelH;

                                                    if (IsOutLimit())
                                                    {
                                                        m_curTetromino.x = backupX;
                                                    }
                                                    else
                                                    {
                                                        if (m_curTetromino.HitGround(m_board))
                                                        {
                                                            m_curTetromino.x -= VelH;
                                                        }
                                                        else
                                                        {
                                                            startTimeH = curTime;
                                                            m_horizontalMove = VelH;
                                                            m_horizontalStartColumn = m_curTetromino.Column();
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

                        //--
                        if (m_curTetromino != null)
                        {
                            m_curTetromino.Draw(window);
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

                        //--
                        DrawBoard(window);

                    }
                    else if (m_mode == GameMode.STANDBY)
                    {

                        drawStandbyScreen();

                    }
                    else if (m_mode == GameMode.HIGH_SCORES)
                    {

                        curTime = m_clock.ElapsedTime.AsMilliseconds();
                        if ((curTime - startTimeH) > 200)
                        {
                            startTimeH = curTime;
                            iColorHighScore++;
                        }
                        drawHighScoresScreen();

                    }
                    else if (m_mode == GameMode.GAME_OVER)
                    {

                        drawGameOverScreen();

                    }

                    if (m_nextTetromino != null)
                    {
                        curTime = m_clock.ElapsedTime.AsMilliseconds();
                        if ((curTime - startTimeR) > 500)
                        {
                            startTimeR = curTime;
                            m_nextTetromino.RotateLeft();
                        }
                        m_nextTetromino.Draw(window);
                    }


                    //--
                    drawCurrentScore();

                    //draw();
                    window.Display();

                }

            }

            void endGame()
            {
                //-------------------------
                if (music!=null){
                    music.Stop();
                }     
                saveHighScores();
                if (window!=null) window.Close();
            }

            void OnKeyPressed(object? sender, SFML.Window.KeyEventArgs e)
            {
                if (processKeyPressedProc != null)
                {
                    processKeyPressedProc(sender, e);                
                }
            }
            void OnKeyReleased(object? sender, SFML.Window.KeyEventArgs e)
            {
                if (processKeyReleasedProc != null)
                {
                    processKeyReleasedProc(sender, e);
                }
            }
            
            //---------------------------------------------------------------------
            //-- Play Mode
            //---------------------------------------------------------------------
            void processKeyPressedPlayMode(object? sender, SFML.Window.KeyEventArgs e)
            {
                //----------------------------------------------
                if (sender==null) return;
                var window = (SFML.Window.Window) sender;
                if (e.Code == SFML.Window.Keyboard.Key.Escape)
                {
                    m_idHightScore = IsHighScore(m_score);
                    //window.KeyPressed -= OnKeyPressedPlayerMode;
                    if (m_idHightScore == -1)
                    {
                        SetStandbyMode();
                    }
                    else
                    {
                        SetHighScoresMode();
                    }

                }
                else if (e.Code == SFML.Window.Keyboard.Key.Left)
                {
                    VelH = -1;
                    IsOutLimit = m_curTetromino.IsOutLeft;

                }
                else if (e.Code == SFML.Window.Keyboard.Key.Right)
                {
                    VelH = 1;
                    IsOutLimit = m_curTetromino.IsOutRight;

                }
                else if (e.Code == SFML.Window.Keyboard.Key.Up)
                {

                    if (m_curTetromino != null)
                    {
                        m_curTetromino.RotateLeft();
                        if (m_curTetromino.HitGround(m_board))
                        {
                            //-- Undo Rotate
                            m_curTetromino.RotateRight();
                        }
                        else if (m_curTetromino.IsOutRight())
                        {
                            var backupX = m_curTetromino.x;
                            //-- Move Inside board
                            while (m_curTetromino.IsOutRight())
                            {
                                m_curTetromino.x--;
                            }
                            if (m_curTetromino.HitGround(m_board))
                            {
                                m_curTetromino.x = backupX;
                                //-- Undo Rotate
                                m_curTetromino.RotateRight();

                            }
                        }
                        else if (m_curTetromino.IsOutLeft())
                        {
                            var backupX = m_curTetromino.x;
                            //-- Move Inside Board
                            while (m_curTetromino.IsOutLeft())
                            {
                                m_curTetromino.x++;
                            }
                            if (m_curTetromino.HitGround(m_board))
                            {
                                m_curTetromino.x = backupX;
                                //-- Undo Rotate
                                m_curTetromino.RotateRight();

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

            void processKeyReleasedPlayMode(object? sender, SFML.Window.KeyEventArgs e)
            {
                if (sender == null) return;
                var window = (SFML.Window.Window)sender;
                switch (m_mode)
                {
                    case GameMode.PLAY:
                        if ((e.Code == SFML.Window.Keyboard.Key.Left) || (e.Code == SFML.Window.Keyboard.Key.Right))
                        {
                            VelH = 0;

                        }
                        break;
                }

            }

            //---------------------------------------------------------------------
            //--Standby Mode
            //---------------------------------------------------------------------
            void processKeyPressedStandbyMode(object? sender, SFML.Window.KeyEventArgs e) 
            {
                //----------------------------------------------
                if (sender==null) return;
                var window = (SFML.Window.Window) sender;
                if (e.Code == SFML.Window.Keyboard.Key.Space){
                    m_clock.Restart();
                    SetPlayMode();
                    NewTetromino();

                }else if (e.Code == SFML.Window.Keyboard.Key.Escape){
                    endGame();

                }

            }
            void processKeyReleasedStandbyMode(object? sender, SFML.Window.KeyEventArgs e)
            {

            }
            
            //------------------------------------------------------------------
            //--GameOver Mode
            //------------------------------------------------------------------
            void processKeyPressedGameOverMode(object? sender, SFML.Window.KeyEventArgs e) 
            {
                //----------------------------------------------
                if (sender==null) return;
                var window = (SFML.Window.Window) sender;
                if (e.Code == SFML.Window.Keyboard.Key.Space){
                    //window.KeyPressed -= OnKeyPressedGameOverMode;
                    m_mode = GameMode.STANDBY;
                    //window.KeyPressed += OnKeyPressedStandby;

                    //init();
                    updateHighScores("-----",m_score);

                }else if (e.Code == SFML.Window.Keyboard.Key.Escape){
                    endGame();

                }

            }

            void processKeyReleasedGameOverMode(object? sender, SFML.Window.KeyEventArgs e)
            {

            }
            
            //------------------------------------------------------------------
            //--HighScores Mode
            //------------------------------------------------------------------
            void processKeyPressedHighScoresMode(object? sender, SFML.Window.KeyEventArgs e) 
            {
                //----------------------------------------------
                if (sender==null) return;
                var window = (SFML.Window.Window) sender;

                if (e.Code == SFML.Window.Keyboard.Key.Enter){
                    insertHighScore(m_idHightScore, m_playerName, m_score);
                    SetStandbyMode();
                    m_curTetromino = null;
                    m_playerName = "";
                }else if (e.Code == SFML.Window.Keyboard.Key.Space){
                    if ((m_playerName==null)||(m_playerName.Length<8)){
                        m_playerName += "_";
                    }
                }else if (e.Code==SFML.Window.Keyboard.Key.Backspace){
                    if (m_playerName!=null){
                        if (m_playerName.Length==1){
                            m_playerName = "";
                        }else if (m_playerName.Length>1){
                            m_playerName = m_playerName.Substring(0,m_playerName.Length-1);
                        }
                    }                        
                }else{
                    if ((m_playerName==null)||(m_playerName.Length<8)){

                        if ((e.Code>=SFML.Window.Keyboard.Key.Num0)&&(e.Code<=SFML.Window.Keyboard.Key.Num9)){
                            char c = (char)  ((int)'0' + e.Code - SFML.Window.Keyboard.Key.Num0);
                            m_playerName += c;
                        }else if ((e.Code>=SFML.Window.Keyboard.Key.Numpad0)&&(e.Code<=SFML.Window.Keyboard.Key.Numpad9)){
                            char c = (char)  ((int)'0' + e.Code - SFML.Window.Keyboard.Key.Numpad0);
                            m_playerName += c;

                        }else  if ((e.Code>=SFML.Window.Keyboard.Key.A)&&(e.Code<=SFML.Window.Keyboard.Key.Z)){
                            char c = (char)  ((int)'A' + e.Code - SFML.Window.Keyboard.Key.A);
                            m_playerName += c;
                        }
                    }
                }
               
            }
            void processKeyReleasedHighScoresMode(object? sender, SFML.Window.KeyEventArgs e)
            {

            }

            //--------------------------------------------------------------------
            //--


            // private void draw()
            // {
            //     //----------------------------------------
            //     if (window!=null){

            //         window.Clear(new Color(64,64,255));

            //         RectangleShape  r0 = new RectangleShape(new Vector2f(Globals.NB_COLUMNS*Globals.cellSize, Globals.NB_ROWS*Globals.cellSize));
            //         r0.Position  = new Vector2f(Globals.LEFT,Globals.TOP);
            //         r0.FillColor = new Color(10,10,100);
            //         window.Draw(r0);

            //         switch(m_mode){
            //             case GameMode.STANDBY :
            //                 drawStandbyScreen();
            //                 break;
            //             case GameMode.AUTO:
            //             case GameMode.PLAY:
            //                 drawCurrentBrick();
            //                 int     x,y,typ;
            //                 RectangleShape  r = new RectangleShape(new Vector2f(Globals.cellSize-2, Globals.cellSize-2));

            //                 for (int l=0;l<Globals.NB_ROWS;l++){
            //                     for(int c=0;c<Globals.NB_COLUMNS;c++){
            //                         x = c*Globals.cellSize + Globals.LEFT;
            //                         y = l*Globals.cellSize + Globals.TOP;
            //                         r.Position =  new Vector2f(x+1, y+1);
            //                         typ = m_table[c+l*Globals.NB_COLUMNS];
            //                         if (typ!=0){
            //                             r.FillColor = Tetromino.Colors[typ];
            //                             window.Draw(r);
            //                         }                              
                                    
            //                     }
            //                 }
            //                 drawCurrentScore();
            //                 drawNextBrick();
            //                 break;

            //             case GameMode.HIGH_SCORES:
            //                 drawHighScoresScreen();
            //                 break;
            //             case GameMode.GAME_OVER:
            //                 drawGameOverScreen();
            //                 //drawCurrentScore();
            //                 break;
            //         }

            //         window.Display();

            //     }
            // }

            // bool isGameOver()
            // {
            //     //----------------------------------------
            //     for(int c=0;c<Globals.NB_COLUMNS;c++){
            //         if (m_table[c]!=0){
            //             return true;
            //         }
            //     }
            //     return false;

            // }

            // bool checkHit()
            // {
            //     int     x,y;
            //     //-------------------------------------------
            //     if (m_curTetromino is not null){
            //         foreach (var v in m_curTetromino.vectors){
            //             x = v.X + m_curTetromino.x;
            //             y = v.Y + m_curTetromino.y;
            //             if ((x<0)||(x>=Globals.NB_COLUMNS)||(y<0)||(y>=Globals.NB_ROWS)){
            //                 return true;
            //             }else if (m_table[x+y*Globals.NB_COLUMNS]!=0){
            //                 return true;
            //             }
            //         }
            //     }
            //     return false;
            // }

            // void setBrick()
            // {
            //     int     x,y;
            //     //-------------------------------------------
            //     if (m_curTetromino!=null){
            //         foreach (var v in m_curTetromino.vectors){
            //             x = v.X + m_curTetromino.x;
            //             y = v.Y + m_curTetromino.y;
            //             m_table[x+y*Globals.NB_COLUMNS] = m_curTetromino.type;
            //         }
            //     }
            // }

            // int clearCompletedLines()
            // {
            //     int     nbL=0;
            //     bool    fCompleted=false;
            //     //-------------------------------------------
            //     for (int l=0;l<Globals.NB_ROWS;l++){
            //         fCompleted = true;
            //         for(int c=0;c<Globals.NB_COLUMNS;c++){
            //             if (m_table[c+l*Globals.NB_COLUMNS]==0){
            //                 fCompleted = false;
            //                 break;
            //             }
            //         }
            //         if (fCompleted){
            //             nbL++;
            //             if (l==0){
            //                 for(int c=0;c<Globals.NB_COLUMNS;c++){
            //                     m_table[c+l*Globals.NB_COLUMNS] = 0;
            //                 }
            //             }else{
            //                 //-- Décaler les lignes au dessus sur la ligne courante
            //                 for(int l1=l;l1>0;l1--){
            //                     for(int c=0;c<Globals.NB_COLUMNS;c++){
            //                         m_table[c+l1*Globals.NB_COLUMNS] = m_table[c+(l1-1)*Globals.NB_COLUMNS];
            //                     }
            //                 }
            //             }
            //         }
            //     }
            //     return nbL;
            // }

            static Int32 TetrisRandomizer(){
                Int32 iSrc;
                Int32 iTyp=0;
                if (idTetrominoBag<14){
                    iTyp = tetrominoBag[idTetrominoBag];
                    idTetrominoBag++;
                }else{
                    //-- Shuttle bag
                    for(int i=0;i<tetrominoBag.Length;i++){
                        iSrc = Globals.rand.Next(0,14);
                        iTyp = tetrominoBag[iSrc];
                        tetrominoBag[iSrc] = tetrominoBag[0];
                        tetrominoBag[0] = iTyp;
                    }
                    iTyp =tetrominoBag[0];
                    idTetrominoBag = 1;

                }
                return iTyp;
            }

            private (string Name, int score) ParseHighScore(string line)
            {
                //------------------------------------------------------                
                char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
                string[] words = line.Split(delimiterChars);
                string n = words[0];
                int s = int.Parse(words[1]);
                return (n, s);

            } 

            void loadHighScores()
            {
                int     iLine = 0;
                string  name;
                int     score;
                //------------------------------------------------------
                string filePath = "HighScores.txt";
                try
                {
                    
                    m_highScores.Clear();

                    foreach (string line in System.IO.File.ReadLines(filePath))
                    {
                        //--
                        (name, score) = ParseHighScore( line);
                        m_highScores.Add(new HighScore(name,score));
                        //--
                        iLine++;
                        if (iLine>9) break;

                    }
                }
                catch (FileNotFoundException uAEx)
                {
                    Console.WriteLine(uAEx.Message);
                    m_highScores.Clear();
                    for (int i=0; i<10; i++)
                    {
                        m_highScores.Add(new HighScore("XXXXXX",0));                        
                    }
                }
            
            }

            void WriteLine(FileStream fs, string value)
            {
                //------------------------------------------------------
                byte[] info = new UTF8Encoding(true).GetBytes(value);
                fs.Write(info, 0, info.Length);
            }

            void saveHighScores()
            {
                //------------------------------------------------------
                // Delete the file if it exists.
                string filePath = "HighScores.txt";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using (FileStream fs = File.Create(filePath))
                {
                    String lin;
                    foreach ( var h in m_highScores ){
                        lin = String.Format("{0},{1}\n", h.Name, h.Score);
                        WriteLine(fs, lin);
                    }

                }

                
            }

            void updateHighScores(String name,int score)
            {
                int idHighScore = -1;
                //---------------------------------------------------
                for (int i=0;i<10;i++){
                    if (score>m_highScores[i].Score){
                        idHighScore = i;
                        break;
                    }
                }
                if (idHighScore>=0){
                    m_highScores.Insert(idHighScore, new HighScore(name, score));
                    if (m_highScores.Count>10){
                        m_highScores.RemoveAt(m_highScores.Count-1);
                    }
                    saveHighScores();
                }
            }
            void insertHighScore(int id,String name,int score){
                if ((id>=0)&&(id<10)){
                    m_highScores.Insert(id, new HighScore(name, score));
                    if (m_highScores.Count>10){
                        m_highScores.RemoveAt(m_highScores.Count-1);
                    }
                }
            }

            int IsHighScore(int score)
            {
                //---------------------------------------------------
                for (int i=0;i<10;i++){
                    if (score>m_highScores[i].Score){
                        return i;
                    }
                }
                return -1;
            }

            void drawHighScoresScreen()
            {
                //---------------------------------------------------
                if (window != null)
                {

                    Text txt = new Text("HIGH SCORES", myFont, 28);
                    if ((m_i_color % 2)==0){
                        txt.FillColor = new Color(254, 238, 72, 255);
                    }else{
                        txt.FillColor = Color.Blue;
                    }
                    var rect = txt.GetLocalBounds();
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                    txt.Position = new Vector2f(m_x_center, 44);
                    window.Draw(txt);

                    int xCol0 = m_cell_size*(Globals.NB_COLUMNS/4);
                    int xCol1 = m_cell_size*(3*Globals.NB_COLUMNS/4);
                    int yLin = 80;
                    foreach (var h in m_highScores)
                    {
                        txt = new Text(h.Name, myFont, 22);
                        txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                        txt.Position = new Vector2f(xCol0, yLin);
                        window.Draw(txt);
                        txt = new Text(String.Format("{0:00000}", h.Score), myFont, 22);
                        txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                        txt.Position = new Vector2f(xCol1, yLin);
                        window.Draw(txt);
                        yLin += 36;

                    }
                }
            }

            void drawCurrentScore()
            {
                //---------------------------------------------------
                 if (window!=null){
                    textScore = new Text(String.Format("Score : {0:00000}",m_score),myFont,20);
                    if (textScore!=null){
                        textScore.FillColor = new Color(255,223,0);
                        textScore.Style = Text.Styles.Bold | Text.Styles.Italic;
                        textScore.Position = new Vector2f(left+(Globals.cellSize*Globals.NB_COLUMNS+1),top);
                        window.Draw(textScore);
                    }
                 }

            }

            void drawGameOverScreen(){
                //---------------------------------------------------
                if (window!=null){
                    textScore = new Text(String.Format("Game Over",m_score),myFont,28);
                    if (textScore!=null){
                        var rect = textScore.GetLocalBounds();
                        float xCenter = (left+right)/2;
                        float yCenter = (bottom+top)/2;
                        RectangleShape  rShapeText = new RectangleShape(new Vector2f(rect.Width+30, rect.Height+14));
                        rShapeText.Position = new Vector2f(xCenter-(rect.Width+24)/2, yCenter-(rect.Height+14)/2);
                        rShapeText.FillColor = Color.Green;
                        window.Draw(rShapeText);
                        textScore.FillColor = Color.Red;
                        textScore.Style = Text.Styles.Bold | Text.Styles.Regular;
                        textScore.Origin = new Vector2f(rect.Left + rect.Width/2.0f,rect.Top + rect.Height/2.0f);
                        textScore.Position = new Vector2f(xCenter,yCenter);
                        window.Draw(textScore);
                    }
                }
            }

            void drawHightScoreInputScreen()
            {
                //-------------------------------------------
                if (window != null)
                {
                    Text txt = new Text("NEW HIGH SCORE", myFont, 28);
                    if ((m_i_color % 2)==0){
                        txt.FillColor = new Color(254, 238, 72, 255);
                    }else{
                        txt.FillColor = Color.Blue;
                    }
                    var rect = txt.GetLocalBounds();
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                    txt.Position = new Vector2f(m_x_center, 44);
                    window.Draw(txt);

                    int xCol0 = Globals.cellSize*(Globals.NB_COLUMNS/4);
                    int xCol1 = Globals.cellSize*(3*Globals.NB_COLUMNS/4);
                    int yLin = 2 * 36;
                    if (m_playerName == null)
                    {
                        txt = new Text("________", myFont, 22);
                    }
                    else
                    {
                        txt = new Text(m_playerName, myFont, 22);
                    }
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Position = new Vector2f(xCol0, yLin);
                    window.Draw(txt);
                    txt = new Text(String.Format("{0:00000}", m_score), myFont, 22);
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Position = new Vector2f(xCol1, yLin);
                    window.Draw(txt);
                }
            }

            void drawStandbyScreen()
            {
                //-------------------------------------------
                if (window != null){
                    int offSetY = Globals.TOP + 3*Globals.cellSize;
                    int yLin = offSetY;
                    Text txt = new Text("TETRIS", myFont, 24);
                    if ((m_i_color % 2)==0){
                        txt.FillColor = new Color(254, 238, 72, 255);
                    }else{
                        txt.FillColor = Color.Blue;
                    }
                    var rect = txt.GetLocalBounds();
                    txt.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                    txt.Position = new Vector2f(m_x_center, yLin);
                    window.Draw(txt);

                    yLin += 48;
                    Text txt1 = new Text("powered by", myFont, 18);
                    rect = txt1.GetLocalBounds();
                    txt1.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt1.Origin = new Vector2f(rect.Left + rect.Width / 2.0f, rect.Top + rect.Height / 2.0f);
                    txt1.Position = new Vector2f(m_x_center, yLin);
                    window.Draw(txt1);

                    yLin += 42;
                    Text txt2 = new Text("Sfml and C#", myFont, 22);
                    rect = txt2.GetLocalBounds();
                    txt2.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt2.Origin = new Vector2f(rect.Width / 2.0f, rect.Height / 2.0f);
                    txt2.Position = new Vector2f(m_x_center, yLin);
                    window.Draw(txt2);

                    yLin += 42;
                    Text txt3 = new Text("Raymond NGUYEN THANH", myFont, 18);
                    rect = txt3.GetLocalBounds();
                    txt3.Style = Text.Styles.Bold | Text.Styles.Regular;
                    txt3.Origin = new Vector2f(rect.Width / 2.0f, rect.Height / 2.0f);
                    txt3.Position = new Vector2f(m_x_center-9, yLin);
                    window.Draw(txt3);

                }

            }

        }


    }

}