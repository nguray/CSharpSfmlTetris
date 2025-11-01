using System;
using SFML.System;
using SFML.Graphics;

namespace SfmlTetris
{

    class Brick{

        public int m_type = 0;
        public Color m_color = new Color(0,0,0);
        public Vector2i m_position;
        static Vector2i[] m_typeBricks = {
            new Vector2i(0,0),  new Vector2i(0,0),   new Vector2i(0,0),new Vector2i(0,0),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(-1,0),new Vector2i(-1,1),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(1,0),new Vector2i(1,1),
            new Vector2i(0,-1), new Vector2i(0,0), new Vector2i(0,1),new Vector2i(0,2),
            new Vector2i(-1,0), new Vector2i(0,0), new Vector2i(1,0),new Vector2i(0,1),
            new Vector2i(0,0),  new Vector2i(1,0), new Vector2i(0,1),new Vector2i(1,1),
            new Vector2i(-1,-1),new Vector2i(0,-1),new Vector2i(0,0),new Vector2i(0,1),
            new Vector2i(1,-1), new Vector2i(0,-1),new Vector2i(0,0),new Vector2i(0,1)        
        };

        public static Color[] m_colors = {
            new Color(0,0,0),
            new Color(0xFF,0x60,0x60),
            new Color(0x60,0xFF,0x60),
            new Color(0x60,0x60,0xFF),
            new Color(0xCC,0xCC,0x60),
            new Color(0xCC,0x60,0xCC),
            new Color(0x60,0xCC,0xCC),
            new Color(0xDA,0xAA,0)
        };


        public List<Vector2i> m_vectors = new List<Vector2i>();

        public Brick(int type){
            m_type = type;
            var id = type*4;
            for (int i=0;i<4;i++){
                var v = m_typeBricks[id+i];
                m_vectors.Add(new Vector2i(v.X,v.Y));
            }
            m_color = m_colors[type];
            m_position = new Vector2i(0,0);
        }

        public void AjustStartY()
        {
            int     y,yMin=1000;
            //-------------------------------------------
            foreach (var v in m_vectors){
                y = v.Y + m_position.Y;
                if (y<yMin){
                    yMin = y;
                }
            }
            m_position.Y -= yMin;

        }

        public void RotateRight()
        {
            int     x,y;
            //-------------------------------------------
            if (m_type==5) return;
            for (int i=0;i<m_vectors.Count;i++){
                var v = m_vectors[i];
                x = -v.Y;
                y = v.X;
                m_vectors[i] = new Vector2i(x,y);
            }

        }

        public void RotateLeft()
        {
            int     x,y;
            //-------------------------------------------
            if (m_type==5) return;
            for (int i=0;i<m_vectors.Count;i++){
                var v = m_vectors[i];
                x = v.Y;
                y = -v.X;
                m_vectors[i] = new Vector2i(x,y);
            }
            
        }



    }


}