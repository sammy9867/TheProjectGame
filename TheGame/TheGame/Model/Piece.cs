using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class Piece
    {
        public int pieceID { get; set; }
        public bool isSham { get; set; }
        public int row { get; set; }
        public int column { get; set; }

        //internal bool
        public bool isTaken(int col, int ro)
        {
            if (column == col && row == ro)
                return true;

            return false;
        }

    }
}
