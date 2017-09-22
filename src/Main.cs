namespace Io.Huiming.Hangman {

using System;
using System.IO;
using System.Collections.Generic;

class MainProgram {
  public static int Run(HangmanGame game, IGuessingStrategy strategy, bool debug) {
    while(game.GameStatus == HangmanGame.Status.KEEP_GUESSING) {
      if (debug) {
        Console.Error.WriteLine(game.ToString());
      }
      IGuess guess = strategy.NextGuess(game);
      if (debug) {
        Console.Error.WriteLine(guess.ToString());
        Console.Error.WriteLine(strategy.ToString());
      }
      guess.MakeGuess(game);
    }
    if (debug) {
      Console.Error.WriteLine(game.ToString());
    }
    return game.CurrentScore;
  }

  public static void Main(string[] args) {
    string file = Environment.GetEnvironmentVariable("hangman_dict");
    if (file == null)
      file = "words.txt";

    int guesses = 5;
    string sguesses = Environment.GetEnvironmentVariable("hangman_guesses");
    if (sguesses != null) {
      guesses = Int32.Parse(sguesses);
      if (guesses < 1)
        guesses = 5;
    }

    bool debug = Environment.GetEnvironmentVariable("hangman_debug") != null;

    // Read in dictionary file
    ISet<string> dict = new HashSet<string>();
    try {
      using (var sr = new StreamReader(file)) {
        string line;
        while ((line = sr.ReadLine()) != null) {
          dict.Add(line.ToUpper());
        }
      }
    }
    catch (Exception) {
      Console.Error.WriteLine($"Error when reading dictionary file '{file}'!");
      Environment.Exit(-1);
    }

    // Run game
    int totalScore = 0;
    int total = 0;
    string word;

    Console.Error.WriteLine("Enter a word:");
    while ((word = Console.ReadLine()) != null) {
      word = word.ToUpper();
      if (!dict.Contains(word)) {
        Console.Error.WriteLine($"Word '{word}' is not in dictionary!");
        continue;
      }

      if (debug) {
        Console.Error.WriteLine($"New Game [{word}]");
      }

      HangmanGame game = new HangmanGame(word, guesses);
      MyGuessingStrategy strategy = new MyGuessingStrategy(game, dict);
      int score = Run(game, strategy, debug);
      totalScore += score;
      total++;
      Console.WriteLine($"{word} = {score}");
      Console.Error.WriteLine("Enter a word:");
    }

    if (total > 0)
      Console.WriteLine(
$@"-----------------------------
AVG: {totalScore * 1.0 / total :g}
NUM: {total}
TOTAL: {totalScore}
"
      );
  }
}

}
