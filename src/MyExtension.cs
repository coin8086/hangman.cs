namespace Io.Huiming.Hangman {

using System.Collections.Generic;

public static class MyExtension {
  public static void AddAll<T>(this ICollection<T> to, ICollection<T> from) {
    foreach (T e in from) {
      to.Add(e);
    }
  }
};

}
