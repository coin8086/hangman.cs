/**
 * A Guess that represents guessing a word for the current Hangman game
 */
public class GuessWord : IGuess {
  private readonly string guess;

  public GuessWord(string guess) {
    this.guess = guess;
  }

  public void MakeGuess(HangmanGame game) {
    game.GuessWord(guess);
  }

  public override string ToString() {
    return "GuessWord[" + guess + "]";
  }
}
