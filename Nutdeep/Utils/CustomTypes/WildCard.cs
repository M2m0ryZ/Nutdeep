namespace Nutdeep.Utils.CustomTypes
{
    //This is use only as a type, to get when the user wants to
    //scan for an AOB with wildcards instead an string...
    public class WildCard
    {
        internal string Pattern { get; set; }

        WildCard(string pattern)
        {
            Pattern = pattern;
        }

        public static implicit operator WildCard(string pattern)
        {
            return new WildCard(pattern);
        }
    }
}
