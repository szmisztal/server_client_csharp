using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration; 

class Server
{
    private TcpListener listener;
    private bool isRunning;
    private DateTime startTime;
    private IConfiguration Configuration;
    private DatabaseManager dbManager;
    private UserRepository userRepository;
    private MessageRepository messageRepository;

    public Server(int port)
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        dbManager = new DatabaseManager(Configuration.GetConnectionString("DefaultConnection"));
        userRepository = new UserRepository(dbManager);

        listener = new TcpListener(IPAddress.Any, port);
        isRunning = true;
        startTime = DateTime.Now;
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine("SERVER IS UP...");

        while (isRunning)
        {
            using (Socket client = listener.AcceptSocket())
            {
                Console.WriteLine("CLIENT CONNECTED");
                Session session = new Session(); 

                while (isRunning && client.Connected)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        int size = client.Receive(buffer);
                        if (size == 0)
                        {
                            break;
                        }

                        string command = Encoding.UTF8.GetString(buffer, 0, size).Trim();
                        string response = ProcessCommand(command, session);
                        client.Send(Encoding.UTF8.GetBytes(response));

                        if (command.Equals("stop", StringComparison.OrdinalIgnoreCase) && session.IsAuthenticated)
                        {
                            isRunning = false;
                        }
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Socket exception: {ex.Message}");
                        break;
                    }
                }

                Console.WriteLine("CLIENT DISCONNECTED");
            }
        }

        listener.Stop();
    }

    private string ProcessCommand(string command, Session session)
    {
        var parts = command.Split(' ');
        if (parts.Length < 1) return "Invalid command format.";

        switch (parts[0].ToLower())
        {
            case "uptime":
            case "info":
                if (!session.IsAuthenticated)
                {
                    return "You must be logged in to use this command.";
                }
                if (parts[0].ToLower() == "uptime")
                {
                    return JsonConvert.SerializeObject(new { command = "uptime", uptime = (DateTime.Now - startTime).ToString() });
                }
                if (parts[0].ToLower() == "info")
                {
                    return JsonConvert.SerializeObject(new { command = "info", version = "0.9.0", creationDate = startTime.ToString("yyyy-MM-dd") });
                }
                break;

            case "help":
                return JsonConvert.SerializeObject(new { command = "help", commands = new string[] { 
                    "uptime - shows the lifetime of the server", 
                    "info - shows the current version and server start date", 
                    "help - lists available commands", "stop - shuts down the server",
                    "send - send message to other user",
                    "read - read your messages",
                    "register <username> <password> <email> - registers a new user", 
                    "login <username> <password> - sign in the user", 
                    "logout - sign out the user" } 
                });

            case "send":
                if (!session.IsAuthenticated)
                    return "You must be logged in to send messages.";
                if (parts.Length < 3)
                    return "Usage: send <receiver> <message>";
                messageRepository.AddMessage(session.Username, parts[1], string.Join(" ", parts.Skip(2)));
                return "Message sent.";

            case "read":
                if (!session.IsAuthenticated)
                    return "You must be logged in to read messages.";
                var messages = messageRepository.GetMessagesForUser(session.Username);
                var response = JsonConvert.SerializeObject(messages);
                return response;

            case "register":
                if (parts.Length < 4)
                {
                    return "Insufficient data to register. Usage: register <username> <password> <email>";
                }
                try
                {
                    userRepository.AddUser(parts[1], parts[2], parts[3]);
                    return "User registered successfully.";
                }
                catch (Exception ex)
                {
                    return $"Failed to register user: {ex.Message}";
                }

            case "login":
                if (session.IsAuthenticated)
                {
                    return "Already logged in.";
                }
                if (parts.Length < 3)
                {
                    return "Insufficient data to login. Usage: login <username> <password>";
                }
                bool isValidUser = userRepository.ValidateUser(parts[1], parts[2]);
                if (isValidUser)
                {
                    session.Authenticate(parts[1]);
                    return "Login successful.";
                }
                else
                {
                    return "Login failed. Incorrect username or password.";
                }

            case "logout":
                if (!session.IsAuthenticated)
                {
                    return "You are not logged in.";
                }
                session.Logout();
                return "Logout successful.";

            case "stop":
                if (session.IsAuthenticated)
                {
                    return JsonConvert.SerializeObject(new { command = "stop", message = "SERVER CLOSED..." });
                }
                return "You must be logged in to use this command.";

            default:
                return JsonConvert.SerializeObject(new { error = "unknown command", command = parts[0], message = "Try 'help' for a list of available commands." });
        }
        return "Command processing error.";
    }

    static void Main(string[] args)
    {
        int port = 12345;
        if (args.Length > 0)
        {
            port = int.Parse(args[0]); 
        }
        Server server = new(port);
        server.Start();
    }
}