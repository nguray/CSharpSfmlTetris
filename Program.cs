
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

    abstract class IGameMode
    {
        public Game? game;
        public abstract void ProcessKeyPressed(object? sender, SFML.Window.KeyEventArgs e);
        public virtual void ProcessKeyReleased(object? sender, SFML.Window.KeyEventArgs e) {}
        public virtual void Update() {}
        public abstract void Draw();
        public virtual void Init() {}
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