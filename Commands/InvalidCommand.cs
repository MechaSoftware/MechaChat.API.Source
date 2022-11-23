using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechaChat.API.Commands
{
    public class InvalidCommand
    {
        public void Execute(List<string> args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nInvalid command!\n");
            Console.ResetColor();
        }
    }
}
