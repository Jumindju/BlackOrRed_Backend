namespace WebAPI.Model.Game;

/// <summary>
/// Represents an answer a user can give to a question
/// </summary>
public enum Answer
{
    /// <summary>
    /// black in round 1
    /// above in round 2
    /// between in round 3
    /// diamonds in round 4
    /// </summary>
    Option1 = 1,
    /// <summary>
    /// red in round 1
    /// below in round 2
    /// not between in round 3
    /// hearts in round 4
    /// </summary>
    Option2,
    /// <summary>
    /// invalid in round 1-3
    /// spades in round 4
    /// </summary>
    Option3,
    /// <summary>
    /// invalid in round 1-3
    /// clubs in round 4
    /// </summary>
    Option4
}