using System;
using System.Collections.Generic;
using System.Text;

namespace LoadWinIO
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new IOPortAccess();

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();
        }
    }
}
