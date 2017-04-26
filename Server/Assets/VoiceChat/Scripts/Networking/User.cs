public class User
{
    private string username;
    private string role;

    public User(string username, string role)
    {
        this.username = username;
        this.role = role;
    }

    public string Username
    {
        get
        {
            return username;
        }
    }

    public string Role
    {
        get
        {
            return role;
        }
    }

}

public class ProxyIdUser
{
    private int proxyId;
    private User user;

    public ProxyIdUser(int proxyId, User user)
    {
        this.user = user;
        this.proxyId = proxyId;
    }

    public User User
    {
        get
        {
            return user;
        }
    }

    public int ProxyId
    {
        get
        {
            return proxyId;
        }
    }
}