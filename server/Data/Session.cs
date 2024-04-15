public class Session
{
    public bool IsAuthenticated { get; set; } = false;
    public string Username { get; set; } = null;

    public void Authenticate(string username)
    {
        IsAuthenticated = true;
        Username = username;
    }

    public void Logout()
    {
        IsAuthenticated = false;
        Username = null;
    }
}
