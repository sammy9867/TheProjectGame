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
        public JFieldValue value;
    }

    public class JFieldValue
    {
        public string manhattanDistance;
        public string contains;
        public string timestamp;
        public string userGuid;
    }

}
