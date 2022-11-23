using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MechaChat.API.Handlers.MySQLHandler;

namespace MechaChat.API.Commands
{
    public class GenerateKeyCommand
    {
        public void Execute(List<string> args)
        {
            try
            {
                string apiType = args[0];

                Generate(apiType);
            }
            catch
            {
                Console.WriteLine($"\nUnable to generate new key. Unknown arg.\n");
            }
        }

        private async void Generate(string type)
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string longstring = new string(Enumerable.Repeat(characters, 50).Select(s => s[Program.random.Next(s.Length)]).ToArray());

            byte[] longBase64 = Encoding.ASCII.GetBytes(longstring);
            string longBase64Str = Convert.ToBase64String(longBase64, 0, length: 50, options: Base64FormattingOptions.None);

            var GetAPIKeyData = await SelectSingleQuery<Models.UserAPIKeysModel>($"SELECT * FROM `user_apikeys` WHERE `api_key` = '{longBase64Str}' LIMIT 1");

            if (GetAPIKeyData != null)
            {
                // Regenerate key //
                Generate(type);
            }
            else
            {
                // Insert generated key //
                await ExecuteQuery($"INSERT INTO `user_apikeys` (`api_type`, `api_key`) VALUES ('{type}', '{longBase64Str}')");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nGenerated a new key: {longBase64Str} | For: {type}\n");
                Console.ResetColor();
            }
        }
    }
}
