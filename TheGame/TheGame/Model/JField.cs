using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Model
{
    public class JField
    {
        public string x;
        public string y;
        public JValue value;
    }

    public class JValue
    {
        public string manhattanDistance;
        public string contains;
        public string timestamp;
        public string userGuid;
    }

}
