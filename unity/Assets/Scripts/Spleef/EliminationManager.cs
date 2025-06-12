using System.Collections.Generic;

public static class EliminationManager
{
    // Holds the names (or IDs) of players in the order they were eliminated.
    // After the last player remains, we'll reverse this list so index 0 is the winner.
    public static List<string> eliminationOrder = new List<string>();
}
