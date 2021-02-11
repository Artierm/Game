using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Game
{
    class Program
    {
        private static byte[] GetSecureRandom()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomNumber = new byte[16];
                rng.GetBytes(randomNumber);
                return randomNumber;
            }
        }
        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public static string Encode(string input, byte[] key)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            using (var myhmacsha1 = new HMACSHA256(key))
            {
                var hashArray = myhmacsha1.ComputeHash(byteArray);
                return hashArray.Aggregate("", (s, e) => s + String.Format("{0:x2}", e), s => s);
            }
        }

        private static void GetMenu(Dictionary<int,string> figures)
        {
            Console.WriteLine("Available moves:");
            foreach (KeyValuePair<int, string> figure in figures)
            {
                Console.WriteLine(figure.Key + " - " + figure.Value);
            }
            Console.WriteLine("0 - exit");
        }

        private static KeyValuePair<int,string> PlayerStroke(Dictionary<int, string> figures)
        {
            int key = Convert.ToInt32(Console.ReadLine());
            figures.TryGetValue(key, out string figure);
            return new KeyValuePair<int, string>(key, figure);
        }

        private static KeyValuePair<int, string> ComputerStroke(Dictionary<int, string> figures)
        {
            Random rnd = new Random();
            int key = rnd.Next(1, figures.Count + 1);
            figures.TryGetValue(key, out string figure);
            return new KeyValuePair<int, string>(key, figure);
        }

        private static Dictionary<int, string> CreateHelperFigures(Dictionary<int, string> figures, out int halfLength)
        {
            halfLength = figures.Count / 2;
            Dictionary<int, string> helperFigures = new Dictionary<int, string>(figures.Count + (halfLength * 2));

            int firstPosition = figures.Count - halfLength + 1;
            for (int i = firstPosition; i <= figures.Count; i++)
            {
                figures.TryGetValue(i, out string figure);
                helperFigures.Add(i - firstPosition, figure);
            }

            for (int i = 1; i <= figures.Count; i++)
            {
                figures.TryGetValue(i, out string figure);
                helperFigures.Add(i + halfLength, figure);
            }

            for (int i = 1; i <= halfLength; i++)
            {
                figures.TryGetValue(i, out string figure);
                helperFigures.Add(i + figures.Count + halfLength, figure);
            }

            return helperFigures;
        }

        private static bool ComputerIsWin(Dictionary<int, string> figures, KeyValuePair<int, string> computerStroke, KeyValuePair<int, string> playerStroke, out bool draw, out bool exit)
        {
            if(playerStroke.Key == 0)
            {
                exit = true;
            }
            else
            {
                exit = false;
            }

            if(playerStroke.Key == computerStroke.Key)
            {
                draw = true;
            }
            else
            {
                draw = false;
            }

            var helperFigures = CreateHelperFigures(figures, out int halfLength);
            int computerKeyHelper = computerStroke.Key + halfLength;

            for(int i = computerKeyHelper + 1; i <= computerKeyHelper + halfLength; i++)
            {
                helperFigures.TryGetValue(i, out string figure);
                if( figure == playerStroke.Value)
                {
                    return true;
                }            
            }

            return false;

        }

        static void Main(string[] args)
        {
            try
            {
                bool ValidAmountArguments = args.Length >= 3 && args.Length % 2 == 1;
                if (ValidAmountArguments)
                {
                    var NoRepeatArguments = args.Where(str => args.Count(s => s == str) > 1).Distinct().ToList();
                    bool RepeatArguments = 0 == NoRepeatArguments.Count;
                    if (RepeatArguments)
                    {
                     
                        Dictionary<int, string> figures = new Dictionary<int, string>(args.Length);
                        for(int i = 1; i <= args.Length; i++)
                        {
                            figures.Add(i, args[i - 1]);
                        }

                        var secretKey = GetSecureRandom();
                        var computerFigure = ComputerStroke(figures);
                        Console.WriteLine("HMAC: {0}", Encode(computerFigure.Value, secretKey));
                        GetMenu(figures);         
                        var playerFigure = PlayerStroke(figures);          
                        bool computerIsWinner  = ComputerIsWin(figures, computerFigure, playerFigure, out bool draw, out bool exit);
                        if (exit)
                        {
                            Console.WriteLine("Game is ended");
                            return;
                        }
                        if (draw)
                        {
                            Console.WriteLine("This is draw");
                        }
                        else
                        {
                            if (computerIsWinner)
                            {
                                Console.WriteLine("Computer is winner");
                            }
                            else
                            {
                                Console.WriteLine("You a winner");
                            }
                        }

                        Console.WriteLine("Your stroke was: {0}", playerFigure.Value);
                        Console.WriteLine("Stroke computer was: {0}", computerFigure.Value);
                        Console.WriteLine("Secret key:{0}", HashEncode(secretKey));
                       
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }


            }
            catch (ArgumentOutOfRangeException)
            {
                string ErrorMessage = FormattableString.Invariant($"The number of arguments is less than three or the number of arguments is even");
                string Example = FormattableString.Invariant($"Example of valid arguments:'rock', 'paper', 'scissors'.");
                Console.WriteLine(ErrorMessage);
                Console.WriteLine(Example);
            }
            catch (ArgumentException)
            {
                string ErrorMessage = FormattableString.Invariant($"The same arguments are present");
                string Example = FormattableString.Invariant($"Example of valid arguments:'rock', 'paper', 'scissors'.");
                Console.WriteLine(ErrorMessage);
                Console.WriteLine(Example);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}