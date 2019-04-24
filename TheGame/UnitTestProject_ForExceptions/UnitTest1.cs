using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThePlayers;
using System.Collections.Generic;

namespace UnitTestProject_ForExceptions
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Something went wrong :(")]
        public void TestMethod_PlayerSocket()
        {
            Action<string> st = null;
            ThePlayers.PlayerSocket.Receive(st);
            
        }
    }
}
