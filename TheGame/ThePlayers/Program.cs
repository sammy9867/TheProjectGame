using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThePlayers
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ThePlayers has started");
            start();
        }

        private static void start()
        {
            bool flag = true;

            while (flag)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "add":
                        Console.WriteLine("new player shall be added");
                        break;
                    case "exit":
                        Console.WriteLine("Thank you for using our app !");
                        flag = false;
                        break;
                    default:
                        Console.WriteLine("<"+command + "> command does not exist");
                        break;
                }

            }
        }
    }
}
