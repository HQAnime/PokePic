using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace PokePic
{
    internal class Interface
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("This program will download All Pokemon images.\nAll images are from pokemondb.net.");
            Console.Write(@"Start downloading(y/n): ");

            // Get user input
            var userInput = Console.ReadLine();
            if (userInput != null)
            {
                userInput = userInput.ToLower();
                if (userInput == "n")
                {
                    Console.WriteLine(@"Console will be closed in 2 second.");
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
                else
                {
                    // Download All Pokemon images
                    var webParser = new Parser();
                    Console.WriteLine(@"There are " + webParser.totalPokemon + @" Pokémon discovered so far.");
                    Thread.Sleep(2000);

                    // Using Regex to get all Pokemon Name
                    var pokemonNameRegex =
                        new Regex(@"<span class=""infocard-tall ""><a class=""pkg "".*?\/pokedex\/(.*?)""");
                    var pokemonNameMatch = pokemonNameRegex.Match(webParser.getWebData());

                    for (var i = 1; i <= webParser.totalPokemon; i++)
                    {
                        Downloader.downloadPokemonImage(pokemonNameMatch.Groups[1].Value, i);
                        pokemonNameMatch = pokemonNameMatch.NextMatch();
                    }

                    Console.WriteLine("\nCompleted!");
                }
            }

            // Waiting for A key
            Console.ReadKey();
            // Open Downloaded Folder
            Process.Start(Directory.GetCurrentDirectory() + @"/Pokémon/");
        }
    }

    internal class Downloader
    {
        private static readonly string currentPath = Directory.GetCurrentDirectory();
        private static int exceptionCount;

        public static void downloadPokemonImage(string name, int i)
        {
            if (exceptionCount > 10)
            {
                Console.WriteLine("Unable to download Pokemon images.\nConsole will be closed in 2 second");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
            else
            {
                try
                {
                    using (var webclient = new WebClient())
                    {
                        webclient.Proxy = null;
                        webclient.Encoding = Encoding.UTF8;

                        // Creating new directory
                        var downloadPath = currentPath + @"/Pokémon/";
                        if (!Directory.Exists(downloadPath))
                            Directory.CreateDirectory(downloadPath);

                        // Download this image to download path
                        var downloadUrl = @"https://img.pokemondb.net/artwork/" + name + @".jpg";
                        name = firstLetterToUpper(name);
                        webclient.DownloadFile(downloadUrl, downloadPath + i.ToString("000 ") + name + @".jpg");

                        // Write to Console
                        Console.WriteLine(@"Downloading " + name + @"'s image.");
                    }
                }
                catch
                {
                    Console.WriteLine(@"Unable to download " + name + @"'s image.");
                    Thread.Sleep(100);
                    // To find link inside that page
                    try
                    {
                        using (var webclient = new WebClient())
                        {
                            webclient.Proxy = null;
                            webclient.Encoding = Encoding.UTF8;

                            // Find link to this image
                            var pokemonUrl = @"https://pokemondb.net/pokedex/" + name;
                            var webData = webclient.DownloadString(pokemonUrl);

                            var allImageRegex = new Regex(@"<img src=""(https:\/\/img.pokemondb.net\/artwork\/.*?)""");

                            var nameSuffix = 1;
                            var downloadPath = currentPath + @"/Pokémon/";
                            foreach (Match match in allImageRegex.Matches(webData))
                            {
                                if (nameSuffix == 1)
                                    webclient.DownloadFile(match.Groups[1].Value, downloadPath + i.ToString("000 ") + name + @".jpg");
                                else
                                    webclient.DownloadFile(match.Groups[1].Value, downloadPath + i.ToString("000 ") + name + nameSuffix + @".jpg");
                                nameSuffix++;
                            }
                        }
                        Console.WriteLine(@"Image Downloaded.");
                    }
                    catch
                    {
                        Console.WriteLine(@"Unable to download this image.");
                        Thread.Sleep(100);
                        exceptionCount++;
                    }
                }
            }
        }

        private static string firstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
    }

    internal class Parser
    {
        public const string DATA_WEBSITE = @"https://pokemondb.net/pokedex/national";
        private static string webData = "";

        public Parser()
        {
            setWebData();
            setTotalPokemon();
        }

        public int totalPokemon { get; private set; }

        private void setWebData()
        {
            using (var webclient = new WebClient())
            {
                webclient.Proxy = null;
                webclient.Encoding = Encoding.UTF8;

                webData = webclient.DownloadString(DATA_WEBSITE);
            }
        }

        private void setTotalPokemon()
        {
            var totalPokemonRegex = new Regex(@"(\d+) Pokémon");
            var totalPokemonMatch = totalPokemonRegex.Match(webData);

            totalPokemon = Convert.ToInt16(totalPokemonMatch.Groups[1].Value);
        }

        public string getWebData()
        {
            return webData;
        }
    }
}