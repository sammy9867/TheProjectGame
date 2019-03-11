using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    class Board
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Team blue { get; set; }
        public Team red { get; set; }
    }
}
