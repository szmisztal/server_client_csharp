using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

class Server
{
    private TcpListener listener;
    private bool isRunning;
    private DateTime startTime;

    public Server(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        startTime = DateTime.Now;
        isRunning = true;
    }

    public void Start()
    {
        listener.Start();
        Console.WriteLine("SERVER`S UP...");

        while (isRunning)
        {
            Socket client = listener.AcceptSocket();
            byte[] buffer = new byte[1024];
            int size = client.Receive(buffer);
            string command = Encoding.UTF8.GetString(buffer, 0, size).Trim();

            string response = ProcessCommand(command);
            client.Send(Encoding.UTF8.GetBytes(response));

            if (command.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                isRunning = false;
            }

            client.Close();
        }

        listener.Stop();
    }

    private string ProcessCommand(string command)
    {
        switch (command.ToLower())
        {
            case "uptime":
                return JsonConvert.SerializeObject(new { command = "uptime", uptime = (DateTime.Now - startTime).ToString() });
            case "info":
                return JsonConvert.SerializeObject(new { command = "info", version = "1.0", creationDate = startTime.ToString("yyyy-MM-dd") });
            case "help":
                return JsonConvert.SerializeObject(new { command = "help", commands = new string[] { "uptime - shows the lifetime of the server", "info - shows the current version and server start date", "help - shuts down the server" } });
            case "stop":
                return JsonConvert.SerializeObject(new { command = "stop", message = "SERVER CLOSED..." });
            default:
                return JsonConvert.SerializeObject(new { error = "unknown command", command = command, message = "try again" });
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        new Server(12345).Start();
    }
}