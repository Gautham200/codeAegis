namespace CodeAegis;

public class BadCode
{
    public void GetUser(string username)
    {
        // VULNERABILITY: Raw SQL string concatenation (SQL Injection)
        string query = "SELECT * FROM Users WHERE Username = '" + username + "'";
        
        // Imagine database execution happens here
        Console.WriteLine(query);
    }
}