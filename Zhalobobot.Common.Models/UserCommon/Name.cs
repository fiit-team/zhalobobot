namespace Zhalobobot.Common.Models.UserCommon
{
    public record Name(string LastName, string FirstName, string? MiddleName)
    {
        public static Name UnknownPerson => new ("Анонимов", "Анон", "Анонимович");
        public static Name Empty => new("", "", "");

        public override string ToString() => $"{LastName} {FirstName} {MiddleName}";
    }
}