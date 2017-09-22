namespace Io.Huiming.Hangman {

using System.Collections.Generic;

public class HangmanGame {
  /**
   * A enum for the current state of the game
   */
  public enum Status { GAME_WON, GAME_LOST, KEEP_GUESSING }

  /**
   * A marker for the letters in the secret words that have not been guessed yet.
   */
  public static readonly char MYSTERY_LETTER = '-';

  /**
   * The word that needs to be guessed
   */
  private readonly string secretWord;

  /**
   * The maximum number of wrong letter/word guesses that are allowed (e.g. 6, and if you exceed 6 then you lose)
   */
  private readonly int maxWrongGuesses;

  /**
   * The letters guessed so far (unknown letters will be marked by the MYSTERY_LETTER constant). For example, 'AB--T'
   */
  private readonly char[] guessedSoFar;

  /**
   * Set of all correct letter guesses so far
   */
  private ISet<char> correctlyGuessedLetters = new HashSet<char>();

  /**
   * Set of all incorrect letter guesses so far
   */
  private ISet<char> incorrectlyGuessedLetters = new HashSet<char>();

  /**
   * Set of all incorrect word guesses so far
   */
  private ISet<string> incorrectlyGuessedWords = new HashSet<string>();

  /**
   * @param secretWord The word that needs to be guessed
   * @param maxWrongGuesses The maximum number of incorrect word/letter guesses that are allowed
   */
  public HangmanGame(string secretWord, int maxWrongGuesses) {
    this.secretWord = secretWord.ToUpper();
    this.guessedSoFar = new char[secretWord.Length];
    for (int i = 0; i < secretWord.Length; i++) {
      guessedSoFar[i] = MYSTERY_LETTER;
    }
    this.maxWrongGuesses = maxWrongGuesses;
  }

  /**
   * Guess the specified letter and update the game state accordingly
   * @return The string representation of the current game state
   * (which will contain MYSTERY_LETTER in place of unknown letters)
   */
  public string GuessLetter(char ch) {
    AssertCanKeepGuessing();
    ch = char.ToUpper(ch);

    // update the guessedSoFar buffer with the new character
    bool goodGuess = false;
    for (int i = 0; i < secretWord.Length; i++) {
      if (secretWord[i] == ch) {
        guessedSoFar[i] = ch;
        goodGuess = true;
      }
    }

    // update the proper set of guessed letters
    if (goodGuess) {
      correctlyGuessedLetters.Add(ch);
    }
    else {
      incorrectlyGuessedLetters.Add(ch);
    }

    return GuessedSoFar;
  }

  /**
   * Guess the specified word and update the game state accordingly
   * @return The string representation of the current game state
   * (which will contain MYSTERY_LETTER in place of unknown letters)
   */
  public string GuessWord(string guess) {
    AssertCanKeepGuessing();
    guess = guess.ToUpper();

    if (guess.Equals(secretWord)) {
      // if the guess is correct, then set guessedSoFar to the secret word
      for (int i = 0; i < secretWord.Length; i++) {
        guessedSoFar[i] = secretWord[i];
      }
    }
    else {
      incorrectlyGuessedWords.Add(guess);
    }

    return GuessedSoFar;
  }

  /**
   * @return The score for the current game state
   */
  public int CurrentScore =>
    GameStatus == Status.GAME_LOST ? 25 : NumWrongGuessesMade + correctlyGuessedLetters.Count;

  private void AssertCanKeepGuessing() {
    if (GameStatus != Status.KEEP_GUESSING) {
      throw new System.InvalidOperationException("Cannot keep guessing in current game state: " + GameStatus);
    }
  }

  /**
   * @return The current game status
   */
  public Status GameStatus {
    get {
      if (secretWord.Equals(GuessedSoFar)) {
        return Status.GAME_WON;
      }
      else if (NumWrongGuessesMade > maxWrongGuesses) {
        return Status.GAME_LOST;
      }
      else {
        return Status.KEEP_GUESSING;
      }
    }
  }

  /**
   * @return Number of wrong guesses made so far
   */
  public int NumWrongGuessesMade => incorrectlyGuessedLetters.Count + incorrectlyGuessedWords.Count;

  /**
   * @return Number of wrong guesses still allowed
   */
  public int NumWrongGuessesRemaining => MaxWrongGuesses - NumWrongGuessesMade;

  /**
   * @return Number of total wrong guesses allowed
   */
  public int MaxWrongGuesses => maxWrongGuesses;

  /**
   * @return The string representation of the current game state
   * (which will contain MYSTERY_LETTER in place of unknown letters)
   */
  public string GuessedSoFar => new string(guessedSoFar);

  /**
   * @return Set of all correctly guessed letters so far
   */
  public ISet<char> CorrectlyGuessedLetter => new HashSet<char>(correctlyGuessedLetters);

  /**
   * @return Set of all incorrectly guessed letters so far
   */
  public ISet<char> IncorrectlyGuessedLetters => new HashSet<char>(incorrectlyGuessedLetters);

  /**
   * @return Set of all guessed letters so far
   */
  public ISet<char> AllGuessedLetters {
    get {
      ISet<char> guessed = new HashSet<char>();
      guessed.AddAll(correctlyGuessedLetters);
      guessed.AddAll(incorrectlyGuessedLetters);
      return guessed;
    }
  }

  /**
   * @return Set of all incorrectly guessed words so far
   */
  public ISet<string> IncorrectlyGuessedWords => new HashSet<string>(incorrectlyGuessedWords);

  /**
   * @return The length of the secret word
   */
  public int SecretWordLength => secretWord.Length;

  public override string ToString() {
    return GuessedSoFar + "; score=" + CurrentScore + "; status=" + GameStatus;
  }
}

}
