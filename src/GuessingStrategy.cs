/**
 * A strategy for generating guesses given the current state of a Hangman game.
 */
public interface IGuessingStrategy {
  IGuess NextGuess(HangmanGame game);
}
