using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechaChat.API.Handlers
{
    public static class CommandHandler
    {
        public static void Sync()
        {
            var commandString = Console.ReadLine();

            var commandParts = commandString.Split(' ').ToList();
            var commandName = commandParts[0];
            var args = commandParts.Skip(1).ToList();

            if (commandName.Length == 0)
            {
                Sync();
            }

            switch (commandName)
            {
                case "generate_key": 
                    new Commands.GenerateKeyCommand().Execute(args);
                    Sync();
                    break;

                default:
                    new Commands.InvalidCommand().Execute(args);
                    Sync();
                    break;
            }
        }
    }
}
