using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Piece
    {
        public int pieceID { get; set; }
        public bool isSham { get; set; }
        public int row { get; set; }
        public int columns { get; set; }

    }
}
