namespace ShirokuStudio.Core.Models
{
    public enum TriState
    {
        Null = 0,
        True = 1,
        False = 2
    }

    public static class TriStateExtensions
    {
        public static bool? ToBoolean(this TriState state)
        {
            return state switch
            {
                TriState.Null => null,
                TriState.True => true,
                TriState.False => false,
                _ => null,
            };
        }
    }
}