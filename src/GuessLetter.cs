/**
 * A Guess that represents guessing a letter for the current Hangman game
 */
public class GuessLetter : IGuess {
  private readonly char guess;

  public GuessLetter(char guess) {
    this.guess = guess;
  }

  public void MakeGuess(HangmanGame game) {
    game.GuessLetter(guess);
  }

  public override string ToString() {
    return "GuessLetter[" + guess + "]";
  }
}
