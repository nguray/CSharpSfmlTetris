using SFML.Graphics;
using SFML.System;

namespace SfmlTetris
{
 
    class Tetromino{
        public int type = 0;
        public int x = 0;
        public int y = 0;
        public Color color = new Color(0,0,0);
        public List<Vector2i> vectors = new List<Vector2i>();

        static Vector2i[] TypeTetrominos = {
            new Vector2i(0,0),  new Vector2i(0,0),   new Vector2i(0,0),new Vector2i(0,0),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(-1,0),new Vector2i(-1,1),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(1,0),new Vector2i(1,1),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(0,1),new Vector2i(0,2),
            new Vector2i(-1,0), new Vector2i(0,0), new Vector2i(1,0),new Vector2i(0,1),
            new Vector2i(0,0),  new Vector2i(1,0), new Vector2i(0,1),new Vector2i(1,1),
            new Vector2i(-1,-1),new Vector2i(0,-1),new Vector2i(0,0),new Vector2i(0,1),
            new Vector2i(1,-1), new Vector2i(0,-1),new Vector2i(0,0),new Vector2i(0,1)        
        };

        public static Color[] Colors = {
            new Color(0,0,0),
            new Color(0,255,0),
            new Color(255,0,0),
            new Color(255,0,255),
            new Color(0,255,255),
            new Color(0,0,255),
            new Color(255,128,0),
            new Color(255,255,0)
        };

        public Tetromino(int type,int x,int y){
            this.type = type;
            this.x = x;
            this.y = y;
            var id = type*4;
            for (int i=0;i<4;i++){
                var v = TypeTetrominos[id+i];
                vectors.Add(new Vector2i(v.X,v.Y));
            }
            color = Colors[type];
            
        }

        public void Draw(RenderWindow window){


            RectangleShape r1 = new RectangleShape(new Vector2f(Globals.cellSize - 2, Globals.cellSize - 2));
            r1.FillColor = color;

            foreach (var v in vectors)
            {
                var vx = v.X * Globals.cellSize + this.x + Globals.LEFT;
                var vy = v.Y * Globals.cellSize + this.y + Globals.TOP;
                r1.Position = new Vector2f(vx + 1, vy + 1);
                window.Draw(r1);
            }            
        }

        public void RotateRight()
        {
            int     x,y;
            //-------------------------------------------
            if (type!=5){
                for (int i=0;i<vectors.Count;i++){
                    var v = vectors[i];
                    x = -v.Y;
                    y = v.X;
                    vectors[i]= v;
                }
            }

        }

        public void RotateLeft()
        {
            int     x,y;
            //-------------------------------------------
           if (type!=5){
                for (int i=0;i<vectors.Count;i++){
                    var v = vectors[i];
                    x = v.Y;
                    y = -v.X;
                    v.X = x;
                    v.Y = y;
                    vectors[i]= v;
                }
           }            
        }

        public int MaxY1() {
            int y; 
            var maxY = vectors[0].Y;
            for (int i=1;i<vectors.Count;i++){
                y = vectors[i].Y;
                if (y > maxY) {
                    maxY = y;
                }
            }
            return maxY;
        }

        public int MinX1() {
            int x; 
            var minX = vectors[0].X;
            for (int i=1;i<vectors.Count;i++){
                x = vectors[i].X;
                if (x < minX) {
                    minX = x;
                }
            }
            return minX;
        }

        public int MaxX1() {
            int x; 
            var maxX = vectors[0].X;
            for (int i=1;i<vectors.Count;i++){
                x = vectors[i].X;
                if (x > maxX) {
                    maxX = x;
                }
            }
            return maxX;
        }

        public bool IsOutLeft(){
            var l = MinX1()*Globals.cellSize + x;
            return (l < 0);
        }

        public bool IsOutRight(){
            var r = MaxX1()*Globals.cellSize + Globals.cellSize + x;
            return (r > Globals.NB_COLUMNS*Globals.cellSize);
        }

        public bool IsOutLRLimit(Int32 veloH){
            if (veloH<0){
                return IsOutLeft();
            }else if (veloH>0){
                return IsOutRight();
            }
            return true;
        }

        public bool IsOutBottom(){
            var b = MaxY1()*Globals.cellSize + Globals.cellSize + y;
            return (b>Globals.NB_ROWS*Globals.cellSize);
        }

        public bool HitGround(int[] board){
            Int32 x,y;

            Func< int, int, bool> Hit = (x, y) => {
                Int32 ix = x / Globals.cellSize;
                Int32 iy = y / Globals.cellSize;
                if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                    if (board[iy*Globals.NB_COLUMNS + ix] != 0) {
                        return true;
                    }

                }
                return false;
            };

            foreach(var v in vectors){

                x = v.X*Globals.cellSize + this.x + 1;
                y = v.Y*Globals.cellSize + this.y + 1;
                if (Hit(x,y)){
                    return true;
                }

                // ix = x / Globals.cellSize;
                // iy = y / Globals.cellSize;
                // if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                //     iHit = iy*Globals.NB_COLUMNS + ix;
                //     if (board[iHit] != 0) {
                //         return true;
                //     }

                // }

                x = v.X*Globals.cellSize + Globals.cellSize - 1 + this.x;
                y = v.Y*Globals.cellSize + this.y + 1;
                if (Hit(x,y)){
                    return true;
                }

                // ix = x / Globals.cellSize;
                // iy = y / Globals.cellSize;
                // if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                //     iHit = iy*Globals.NB_COLUMNS + ix;
                //     if (board[iHit] != 0) {
                //         return true;
                //     }

                // }

                x = v.X*Globals.cellSize + Globals.cellSize - 1 + this.x;
                y = v.Y*Globals.cellSize + Globals.cellSize - 1 + this.y;
                if (Hit(x,y)){
                    return true;
                }

                // ix = x / Globals.cellSize;
                // iy = y / Globals.cellSize;
                // if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                //     iHit = iy*Globals.NB_COLUMNS + ix;
                //     if (board[iHit] != 0) {
                //         return true;
                //     }

                // }

                x = v.X*Globals.cellSize + this.x + 1;
                y = v.Y*Globals.cellSize + Globals.cellSize - 1 + this.y;
                if (Hit(x,y)){
                    return true;
                }

                // ix = x / Globals.cellSize;
                // iy = y / Globals.cellSize;
                // if ((ix >= 0) && ix < Globals.NB_COLUMNS && (iy >= 0) && (iy < Globals.NB_ROWS)){
                //     iHit = iy*Globals.NB_COLUMNS + ix;
                //     if (board[iHit] != 0) {
                //         return true;
                //     }

                // }

            }


            return false;
        }

        public Int32 Column(){
            return x / Globals.cellSize;
        }

    }

}