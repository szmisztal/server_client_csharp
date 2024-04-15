using System.Data.SqlClient;

public class MessageRepository
{
    private readonly DatabaseManager _dbManager;

    public MessageRepository(DatabaseManager dbManager)
    {
        _dbManager = dbManager;
    }

    public void AddMessage(string sender, string receiver, string content)
    {
        using (var connection = _dbManager.GetConnection())
        {
            connection.Open();
            var command = new SqlCommand("INSERT INTO Messages (SenderUsername, ReceiverUsername, Content) VALUES (@Sender, @Receiver, @Content)", connection);
            command.Parameters.AddWithValue("@Sender", sender);
            command.Parameters.AddWithValue("@Receiver", receiver);
            command.Parameters.AddWithValue("@Content", content);
            command.ExecuteNonQuery();
        }
    }

    public List<Message> GetMessagesForUser(string username)
    {
        var messages = new List<Message>();
        using (var connection = _dbManager.GetConnection())
        {
            connection.Open();
            var command = new SqlCommand("SELECT SenderUsername, Content, Timestamp FROM Messages WHERE ReceiverUsername = @Username ORDER BY Timestamp DESC", connection);
            command.Parameters.AddWithValue("@Username", username);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    messages.Add(new Message
                    {
                        Sender = reader["SenderUsername"].ToString(),
                        Content = reader["Content"].ToString(),
                        Timestamp = Convert.ToDateTime(reader["Timestamp"])
                    });
                }
            }
        }
        return messages;
    }
}