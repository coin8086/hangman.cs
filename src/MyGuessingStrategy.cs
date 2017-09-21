using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class MyGuessingStrategy : IGuessingStrategy {
  /**
   * A WordSet is a set of words having the same pattern.
   * A pattern is a string returned by HangmanGame::getGuessedSoFar. It's like
   * "AB-", which matches words like "ABC", "ABD" and "ABX", but not "ABA" or "ABB".
   * A WordSet contains statistical info about letters that are NOT GUESSED yet.
   */
  private class WordSet : ICollection<string> {

    private class WordIterator : IEnumerator<string> {
      private readonly IEnumerator<string> it;

      public WordIterator(WordSet outer) {
        it = outer.words.GetEnumerator();
      }

      public string Current => it.Current;

      object IEnumerator.Current => it.Current;

      public bool MoveNext() {
        return it.MoveNext();
      }

      public void Reset() {
        it.Reset();
      }

      public void Dispose() {}
    }

    private class LetterStat : System.IComparable<LetterStat> {
      public readonly char ch;
      public int count = 0;  //How many times the letter appears in a WordSet
      public int wordCount = 0;  //How many words contains the letter in a WordSet

      public LetterStat(char ch) {
        this.ch = ch;
      }

      public int CompareTo(LetterStat rhs) {
        if (this.count > rhs.count)
          return -1;

        if (this.count == rhs.count)
          return this.wordCount - rhs.wordCount;

        return 1;
      }
    }

    /**
     * A map of letters to their statistical info.
     */
    private IDictionary<char, LetterStat> stat = new Dictionary<char, LetterStat>();

    /**
     * Letters in descendent order on frequency
     */
    private IList<LetterStat> order = null;

    /**
     * Words in the set.
     */
    private IList<string> words = new List<string>();

    public readonly string pattern;

    public readonly ISet<char> guessedLetters;

    public WordSet(string pattern, ISet<char> guessedLetters, ICollection<string> words) {
      //assert(pattern != null && guessedLetters != null);
      this.pattern = pattern;
      this.guessedLetters = guessedLetters;
      MyExtension.AddAll(this, words);
      MakeOrder();
      this.stat = null; //Make the WordSet unmodifiable
    }

    private void MakeOrder() {
      var list = new List<LetterStat>(this.stat.Values);
      list.Sort();
      this.order = list;
    }

    /**
     * Determine if a word matches the pattern and guessed letters.
     */
    private bool Match(string word) {
      int size = pattern.Length;
      //assert(size == word.Length);

      bool ret = true;
      for (int i = 0; i < size; i++) {
        if (pattern[i] != HangmanGame.MYSTERY_LETTER) {
          if (pattern[i] != word[i]) {
            ret = false;
            break;
          }
        }
        else {
          if (guessedLetters.Contains(word[i])) {
            ret = false;
            break;
          }
        }
      }
      return ret;
    }

    ///////////////////////////////////////////////////////////
    //
    // Properties and Methods of ICollection<T> and ICollection
    //

    public int Count => words.Count;

    public bool IsReadOnly => true;

    public void Add(string word) {
      if (stat == null) {
        throw new NotSupportedException();
      }

      if (!Match(word)) {
        return;
      }

      ISet<char> parsed = new HashSet<char>();
      foreach (char ch in word) {
        if (!guessedLetters.Contains(ch)) {
          LetterStat stat = this.stat[ch];
          if (stat != null) {
            stat.count++;
            if (parsed.Add(ch)) {
              stat.wordCount++;
            }
          }
          else {
            stat = new LetterStat(ch);
            stat.count++;
            stat.wordCount++;
            this.stat[ch] = stat;
            parsed.Add(ch);
          }
        }
      }
      this.words.Add(word);
    }

    public void Clear() {
      throw new NotSupportedException();
    }

    public bool Contains(string word) {
      throw new NotImplementedException();
    }

    public void CopyTo(string[] arr, int idx) {
      throw new NotImplementedException();
    }

    public bool Remove(string word) {
      throw new NotSupportedException();
    }

    public IEnumerator<string> GetEnumerator() {
      return new WordIterator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    //
    //
    //////////////////////////////////////////////

    /**
     * Give a suggest of the most probable letter not in the excluded letter set.
     */
    public char Suggest(ISet<char> excluded) {
      foreach (LetterStat l in order) {
        char ch = l.ch;
        if (!excluded.Contains(ch)) {
          return ch;
        }
      }
      //assert(false);
      return '\0';
    }

    public override string ToString() {
      return "WordSet[" + Count + "]";
    }
  };

  private WordSet wordset;

  public MyGuessingStrategy(HangmanGame game, ISet<string> dict) {
    string pattern = game.GuessedSoFar;
    int len = pattern.Length;
    IList<string> words = new List<string>();
    foreach (string word in dict) {
      if (word.Length == len) {
        words.Add(word);
      }
    }
    this.wordset = new WordSet(pattern, game.AllGuessedLetters, words);
  }

  public IGuess NextGuess(HangmanGame game) {
    string pattern = game.GuessedSoFar;
    ISet<char> guessedLetters = game.AllGuessedLetters;
    ISet<char> guessed = new HashSet<char>(guessedLetters);
    ISet<string> wrongWords = game.IncorrectlyGuessedWords;
    //NOTE: The strategy will make a word guess either when there's only one
    //word in the wordset, or when there's only one blank left.
    //When a word guess failed, the incorrectly guessed letter in the word should be
    //counted.
    if (wrongWords.Count > 0) {
      int idx = pattern.IndexOf(HangmanGame.MYSTERY_LETTER);
      guessed = new HashSet<char>(guessedLetters);
      foreach (string wd in wrongWords) {
        guessed.Add(wd[idx]);
      }
    }

    //Update the wordset when the game status(pattern and guessed) doesn't match.
    if (!(pattern.Equals(this.wordset.pattern) && guessed.Equals(this.wordset.guessedLetters))) {
      //Update wordset on a previous guess result, be it successful or not.
      this.wordset = new WordSet(pattern, guessed, this.wordset);
    }

    if (this.wordset.Count == 1) {
      return new GuessWord(this.wordset.First());
    }

    int patternBlanks = NumOfBlanks(pattern);
    if (patternBlanks > 1) {
      if (game.NumWrongGuessesRemaining == 0) {
        //Simply return the first word in the word set for the last chance.
        return new GuessWord(this.wordset.First());
      }
      else {
        return new GuessLetter((this.wordset.Suggest(guessedLetters)));
      }
    }
    else {
      //When there's only one blank letter, try to guess the word to save one
      //score on a successfull guess.
      char ch = this.wordset.Suggest(guessed);
      return new GuessWord(pattern.Replace(HangmanGame.MYSTERY_LETTER, ch));
    }
  }

  private static int NumOfBlanks(string pattern) {
    int count = 0;
    foreach (char ch in pattern) {
      if (ch == HangmanGame.MYSTERY_LETTER)
        count++;
    }
    return count;
  }

  public override string ToString() {
    return "MyGuessingStrategy[" + this.wordset + "]";
  }
}
