using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using OpenEngine.Core;

namespace OpenEngine.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Start();
            System.Console.ReadLine();
            bootstrapper.Stop();
        }
    }
}
