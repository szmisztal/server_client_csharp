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

    public bool ValidateUser(string username, string password, out User validatedUser)
    {
        using (var connection = _dbManager.GetConnection())
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM Users WHERE Username = @Username", connection);
            command.Parameters.AddWithValue("@Username", username);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string storedHash = reader["PasswordHash"].ToString();
                    if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                    {
                        validatedUser = new User
                        {
                            UserId = (int)reader["UserId"],
                            Username = (string)reader["Username"],
                            PasswordHash = storedHash,
                            Email = (string)reader["Email"],
                            CreateDate = (DateTime)reader["CreateDate"]
                        };
                        return true;
                    }
                }
            }
        }
        validatedUser = null;
        return false;
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}


