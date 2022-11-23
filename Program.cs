using Microsoft.Owin.Hosting;
using System;
using System.Text;

namespace MechaChat.API
{
    public interface ICommand
    {
        void Execute();
    }

    public static class Program
    {
        const string url = "http://api.mecha.chat";

        public static Random random = new Random();

        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------");

                Console.WriteLine(
                    " _______  _______  _______           _______  _______           _______ _________   _______  _______ _________\n" +
                    "(       )(  ____ \\(  ____ \\|\\     /|(  ___  )(  ____ \\|\\     /|(  ___  )\\__   __/  (  ___  )(  ____ )\\__   __/\n" +
                    "| () () || (    \\/| (    \\/| )   ( || (   ) || (    \\/| )   ( || (   ) |   ) (     | (   ) || (    )|   ) (   \n" +
                    "| || || || (__    | |      | (___) || (___) || |      | (___) || (___) |   | |     | (___) || (____)|   | |   \n" +
                    "| |(_)| ||  __)   | |      |  ___  ||  ___  || |      |  ___  ||  ___  |   | |     |  ___  ||  _____)   | |   \n" +
                    "| |   | || (      | |      | (   ) || (   ) || |      | (   ) || (   ) |   | |     | (   ) || (         | |   \n" +
                    "| )   ( || (____/\\| (____/\\| )   ( || )   ( || (____/\\| )   ( || )   ( |   | |     | )   ( || )      ___) (___\n" +
                    "|/     \\|(_______/(_______/|/     \\||/     \\|(_______/|/     \\||/     \\|   )_(     |/     \\||/       \\_______/\n"
                );

                Console.WriteLine($"[API] Server Started: {url}\n");

                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------\n");

                // Command Logic
                Handlers.CommandHandler.Sync();
            }
        }

        public static string DecodeURIParameters(this string encodedStr)
        {

            var decodedStr = Uri.UnescapeDataString(encodedStr);

            return decodedStr.ToString();
        }
    }
}
