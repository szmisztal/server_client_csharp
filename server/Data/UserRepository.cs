using System.Data.SqlClient;

public class UserRepository
{
    private readonly DatabaseManager _dbManager;

    public UserRepository(DatabaseManager dbManager)
    {
        _dbManager = dbManager;
    }

    public void AddUser(string username, string password, string email)
    {
        var hashedPassword = HashPassword(password); 

        using (var connection = _dbManager.GetConnection())
        {
            connection.Open();
            var command = new SqlCommand("INSERT INTO Users (Username, PasswordHash, Email, CreateDate) VALUES (@Username, @PasswordHash, @Email, @CreateDate)", connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", hashedPassword);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@CreateDate", DateTime.Now);

            command.ExecuteNonQuery();
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}