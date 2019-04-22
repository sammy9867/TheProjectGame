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
        public JValue value;
    }

    public class JValue
    {
        public int manhattanDistance;
        public string contains;
        public long timestamp;
        public string userGuid;
    }

}
