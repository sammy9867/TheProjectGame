using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class JField
    {
        public int x;
        public int y;
        public JFieldValue value;
    }

    public class JFieldValue
    {
        public Int32  manhattanDistance;
        public string contains;
//        public string timestamp;
        public string userGuid;
    }

}
