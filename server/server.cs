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
                        string response = ProcessCommand(command);
                        client.Send(Encoding.UTF8.GetBytes(response));

                        if (command.Equals("stop", StringComparison.OrdinalIgnoreCase))
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

    private string ProcessCommand(string command)
    {
        var parts = command.Split(' ');
        switch (parts[0].ToLower())
        {
            case "uptime":
                return JsonConvert.SerializeObject(new { command = "uptime", uptime = (DateTime.Now - startTime).ToString() });
            case "info":
                return JsonConvert.SerializeObject(new { command = "info", version = "0.2.1", creationDate = startTime.ToString("yyyy-MM-dd") });
            case "help":
                return JsonConvert.SerializeObject(new { command = "help", commands = new string[] { "register - sign up new user", "uptime - shows the lifetime of the server", "info - shows the current version and server start date", "help - lists available commands", "stop - shuts down the server", "register <username> <password> <email> - registers a new user" } });
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
            case "stop":
                return JsonConvert.SerializeObject(new { command = "stop", message = "SERVER CLOSED..." });
            default:
                return JsonConvert.SerializeObject(new { error = "unknown command", command = parts[0], message = "Try 'help' for a list of available commands." });
        }
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