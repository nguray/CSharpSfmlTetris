
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
    class Program
    {

        const int WIDTH = 480;
        const int HEIGHT = 560;
        const string TITLE = "Tetris";

        const int NB_ROWS = 20;
        const int NB_COLUMNS = 12;

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

            private int left,top,right,bottom;
            private List<int> m_table = new List<int>();
            private int m_cell_size = 5;
            private int m_score = 0;
            private int m_idHightScore = -1;

            private int m_speed = 500;

            private int m_x_center = 0;

            private Brick? m_curBrick;
            private Brick? m_nextBrick;
            private Clock m_clock = new Clock();
            private Random m_random = new Random();

            private VideoMode mode;            
            private RenderWindow? window;

             private SoundBuffer? succesSoundBuff;
            private Music? music;
            private Sound? succesSound = new Sound();
            private Font? myFont;
            private Text? textScore;

            private List<HighScore> m_highScores = new List<HighScore>();

            private String m_playerName="";

            private uint m_i_color = 0;
            private int m_velocityH = 0;

            private int m_last_updateV = 0;
            private int m_last_updateH = 0;

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

                m_cell_size = (int)(WIDTH / (NB_COLUMNS + 7));
                mode = new VideoMode(WIDTH, HEIGHT);

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

                left = m_cell_size;
                right = left + m_cell_size * NB_COLUMNS;
                m_x_center = (int)(left + m_cell_size * NB_COLUMNS / 2);
                top = m_cell_size;
                bottom = top + m_cell_size * NB_ROWS;

                //-- Init Game
                init();

                window.Closed += (StringReader, args) => endGame();
                window.KeyReleased += OnKeyReleased;

                window.KeyPressed += OnKeyPressed;

                SetStandbyMode();

                while (window.IsOpen)
                {
                    //--
                    window.DispatchEvents();
                    update();
                    draw();

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

            void init()
            {
                m_table.Clear();
                for (int r=0;r<NB_ROWS;r++){
                    for(int c=0;c<NB_COLUMNS;c++){
                        m_table.Add(0);
                    }
                }
                m_cell_size = (right - left) / NB_COLUMNS;
                bottom = m_cell_size * NB_ROWS + top;
                m_score = 0;
                m_speed = 500;
                m_curBrick = null;
                m_nextBrick = null;
                m_last_updateH = 0;
                m_last_updateV = 0;
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
                    if (m_idHightScore==-1){
                        SetStandbyMode();
                    }else{
                        SetHighScoresMode();
                    }

                }else if (e.Code == SFML.Window.Keyboard.Key.Left)
                {
                    m_velocityH = -1;

                }else if (e.Code == SFML.Window.Keyboard.Key.Right)
                {
                     m_velocityH = 1;

                }else if (e.Code == SFML.Window.Keyboard.Key.Up)
                {
                    if (m_curBrick!=null)
                    {
                        m_curBrick.RotateLeft();
                        if (checkHit()){
                            m_curBrick.RotateRight();
                        }
                    }

                }else if (e.Code == SFML.Window.Keyboard.Key.Down)
                {
                    if (m_curBrick!=null)
                    {
                        m_curBrick.RotateRight();
                        if (checkHit())
                        {
                            m_curBrick.RotateLeft();
                        }
                    }
                }else if (e.Code == SFML.Window.Keyboard.Key.Space){
                    m_mode = GameMode.AUTO;
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
                            m_velocityH = 0;
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
                   init();
                    m_curBrick = new Brick(m_random.Next(1,8));
                    m_curBrick.m_position.X = NB_COLUMNS/2;
                    m_curBrick.m_position.Y = 0;
                    m_curBrick.AjustStartY();

                    m_nextBrick = new Brick(m_random.Next(1,8));
                    m_nextBrick.m_position.X = NB_COLUMNS/2;
                    m_nextBrick.m_position.Y = 0;
                    m_nextBrick.AjustStartY();

                    m_clock.Restart();
                    m_last_updateH = 0;
                    m_last_updateV = 0;
                    SetPlayMode();
 
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

                    init();
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
                    m_curBrick = null;
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
            private void update()
            {
                if ((m_curBrick != null) && (window != null))
                {

                    if (m_mode == GameMode.PLAY)
                    {
                        var r = m_clock.ElapsedTime.AsMilliseconds();

                        if ((r - m_last_updateV) > m_speed)
                        {
                            m_last_updateV = r;

                            m_curBrick.m_position.X += m_velocityH;
                            if (checkHit())
                            {
                                m_curBrick.m_position.X -= m_velocityH;
                            }
                            else
                            {
                                m_last_updateH = r;
                            }

                            m_curBrick.m_position.Y++;
                            if (checkHit())
                            {
                                m_curBrick.m_position.Y--;
                                setBrick();
                                m_curBrick = m_nextBrick;
                                m_nextBrick = new Brick(m_random.Next(1, 8));
                                m_nextBrick.m_position.X = NB_COLUMNS / 2;
                                m_nextBrick.m_position.Y = 0;
                                m_nextBrick.AjustStartY();
                                if (checkHit())
                                {
                                    if (isGameOver())
                                    {
                                        m_idHightScore = IsHighScore(m_score);
                                        if (m_idHightScore == -1)
                                        {
                                            SetGameOverMode();
                                        }
                                        else
                                        {
                                            SetHighScoresMode();
                                        }

                                    }

                                }

                            }
                        }
                        else
                        {
                            if ((r - m_last_updateH) > 80)
                            {
                                m_curBrick.m_position.X += m_velocityH;
                                if (checkHit())
                                {
                                    m_curBrick.m_position.X -= m_velocityH;
                                }
                                else
                                {
                                    m_last_updateH = r;
                                }

                            }

                        }

                        //--
                        var nbL = clearCompletedLines();
                        if (nbL != 0)
                        {
                            if (succesSound != null) succesSound.Play();
                            switch (nbL)
                            {
                                case 1:
                                    m_score += 40;
                                    break;
                                case 2:
                                    m_score += 100;
                                    break;
                                case 3:
                                    m_score += 300;
                                    break;
                                case 4:
                                    m_score += 1200;
                                    break;
                            }

                        }



                    }
                    else if (m_mode == GameMode.AUTO)
                    {

                        m_curBrick.m_position.Y++;
                        if (checkHit())
                        {
                            m_curBrick.m_position.Y--;
                            setBrick();
                            m_curBrick = m_nextBrick;
                            m_nextBrick = new Brick(m_random.Next(1, 8));
                            m_nextBrick.m_position.X = NB_COLUMNS / 2;
                            m_nextBrick.m_position.Y = 0;
                            m_nextBrick.AjustStartY();
                            SetPlayMode();
                        }
                        //--
                        var nbL = clearCompletedLines();
                        if (nbL != 0)
                        {
                            if (succesSound != null) succesSound.Play();
                            switch (nbL)
                            {
                                case 1:
                                    m_score += 40;
                                    break;
                                case 2:
                                    m_score += 100;
                                    break;
                                case 3:
                                    m_score += 300;
                                    break;
                                case 4:
                                    m_score += 1200;
                                    break;
                            }
                        }

                    }
                }
                else if ((m_mode == GameMode.STANDBY) || (m_mode == GameMode.HIGH_SCORES))
                {
                    var r = m_clock.ElapsedTime.AsMilliseconds();
                    if (r > m_speed)
                    {
                        m_clock.Restart();
                        m_i_color++;
                    }
                }

            }

            private void draw()
            {
                //----------------------------------------
                if (window!=null){

                    window.Clear(new Color(64,64,255));

                    RectangleShape  r0 = new RectangleShape(new Vector2f(right-left, bottom-top));
                    r0.Position  = new Vector2f(left,top);
                    r0.FillColor = new Color(10,10,100);
                    window.Draw(r0);

                    switch(m_mode){
                        case GameMode.STANDBY :
                            drawStandbyScreen();
                            break;
                        case GameMode.AUTO:
                        case GameMode.PLAY:
                            drawCurrentBrick();
                            int     x,y,typ;
                            RectangleShape  r = new RectangleShape(new Vector2f(m_cell_size-2, m_cell_size-2));

                            for (int l=0;l<NB_ROWS;l++){
                                for(int c=0;c<NB_COLUMNS;c++){
                                    x = c*m_cell_size + left;
                                    y = l*m_cell_size + top;
                                    r.Position =  new Vector2f(x+1, y+1);
                                    typ = m_table[c+l*NB_COLUMNS];
                                    if (typ!=0){
                                        r.FillColor = Brick.m_colors[typ];
                                        window.Draw(r);
                                    }                              
                                    
                                }
                            }
                            drawCurrentScore();
                            drawNextBrick();
                            break;

                        case GameMode.HIGH_SCORES:
                            drawHighScoresScreen();
                            break;
                        case GameMode.GAME_OVER:
                            drawGameOverScreen();
                            //drawCurrentScore();
                            break;
                    }

                    window.Display();

                }
            }

            bool isGameOver()
            {
                //----------------------------------------
                for(int c=0;c<NB_COLUMNS;c++){
                    if (m_table[c]!=0){
                        return true;
                    }
                }
                return false;

            }

            bool checkHit()
            {
                int     x,y;
                //-------------------------------------------
                if (m_curBrick is not null){
                    foreach (var v in m_curBrick.m_vectors){
                        x = v.X + m_curBrick.m_position.X;
                        y = v.Y + m_curBrick.m_position.Y;
                        if ((x<0)||(x>=NB_COLUMNS)||(y<0)||(y>=NB_ROWS)){
                            return true;
                        }else if (m_table[x+y*NB_COLUMNS]!=0){
                            return true;
                        }
                    }
                }
                return false;
            }

            void setBrick()
            {
                int     x,y;
                //-------------------------------------------
                if (m_curBrick!=null){
                    foreach (var v in m_curBrick.m_vectors){
                        x = v.X + m_curBrick.m_position.X;
                        y = v.Y + m_curBrick.m_position.Y;
                        m_table[x+y*NB_COLUMNS] = m_curBrick.m_type;
                    }
                }
            }

            int clearCompletedLines()
            {
                int     nbL=0;
                bool    fCompleted=false;
                //-------------------------------------------
                for (int l=0;l<NB_ROWS;l++){
                    fCompleted = true;
                    for(int c=0;c<NB_COLUMNS;c++){
                        if (m_table[c+l*NB_COLUMNS]==0){
                            fCompleted = false;
                            break;
                        }
                    }
                    if (fCompleted){
                        nbL++;
                        if (l==0){
                            for(int c=0;c<NB_COLUMNS;c++){
                                m_table[c+l*NB_COLUMNS] = 0;
                            }
                        }else{
                            //-- Décaler les lignes au dessus sur la ligne courante
                            for(int l1=l;l1>0;l1--){
                                for(int c=0;c<NB_COLUMNS;c++){
                                    m_table[c+l1*NB_COLUMNS] = m_table[c+(l1-1)*NB_COLUMNS];
                                }
                            }
                        }
                    }
                }
                return nbL;
            }

            private (string Name, int score) ParseHighScore(string line)
            {
                //------------------------------------------------------                
                char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
                string[] words = line.Split(delimiterChars);
                string n = words[0];
                int    s = int.Parse(words[1]);
                return (n,s);

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

                    int xCol0 = m_cell_size*(NB_COLUMNS/4);
                    int xCol1 = m_cell_size*(3*NB_COLUMNS/4);
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
                        textScore.Position = new Vector2f(left+(m_cell_size*NB_COLUMNS+1),top);
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

            void drawCurrentBrick()
            {
                int x,y;
                //-------------------------------------------
                if ((m_curBrick!=null)&&(window != null)){
                    RectangleShape r1 = new RectangleShape(new Vector2f(m_cell_size - 2, m_cell_size - 2));
                    r1.FillColor = m_curBrick.m_color;

                    foreach (var v in m_curBrick.m_vectors)
                    {
                        x = (v.X + m_curBrick.m_position.X) * m_cell_size + left;
                        y = (v.Y + m_curBrick.m_position.Y) * m_cell_size + top;
                        r1.Position = new Vector2f(x + 1, y + 1);
                        window.Draw(r1);
                    }
                }

            }

            void drawNextBrick()
            {
                int x,y;
                //-------------------------------------------
                if ((m_nextBrick!=null)&&(window != null)){
                    RectangleShape r1 = new RectangleShape(new Vector2f(m_cell_size - 2, m_cell_size - 2));
                    r1.FillColor = m_nextBrick.m_color;

                    int iposX = (NB_COLUMNS+3);
                    int iposY = 10;
                    foreach (var v in m_nextBrick.m_vectors)
                    {
                        x = (v.X + iposX) * m_cell_size + left;
                        y = (v.Y + iposY) * m_cell_size + top;
                        r1.Position = new Vector2f(x + 1, y + 1);
                        window.Draw(r1);
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

                    int xCol0 = m_cell_size*(NB_COLUMNS/4);
                    int xCol1 = m_cell_size*(3*NB_COLUMNS/4);
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
                    int offSetY = (bottom - top) / 6;
                    int yLin = offSetY;
                    Text txt = new Text("TETRIS", myFont, 32);
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