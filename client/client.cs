using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    private bool isRunning = true;

    private void SendCommand(NetworkStream stream, string command)
    {
        byte[] data = Encoding.UTF8.GetBytes(command);
        stream.Write(data, 0, data.Length);

        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Console.WriteLine("Response: " + response);
    }

    public void Start(string server, int port)
    {
        using (TcpClient client = new TcpClient(server, port))
        using (NetworkStream stream = client.GetStream())
        {
            while (isRunning)
            {
                Console.WriteLine("Enter command:");
                string command = Console.ReadLine();

                if (string.IsNullOrEmpty(command))
                {
                    continue;
                }

                if (command.ToLower().Equals("stop"))
                {
                    SendCommand(stream, command);
                    isRunning = false;
                    continue;
                }
                else if (command.ToLower().StartsWith("register"))
                {
                    Console.WriteLine("Enter username:");
                    string username = Console.ReadLine();
                    Console.WriteLine("Enter password:");
                    string password = Console.ReadLine();
                    Console.WriteLine("Enter email:");
                    string email = Console.ReadLine();

                    string fullCommand = $"register {username} {password} {email}";
                    SendCommand(stream, fullCommand);
                }
                else if (command.ToLower().StartsWith("login"))
                {
                    Console.WriteLine("Enter username:");
                    string username = Console.ReadLine();
                    Console.WriteLine("Enter password:");
                    string password = Console.ReadLine();

                    string loginCommand = $"login {username} {password}";
                    SendCommand(stream, loginCommand);
                }
                else if (command.ToLower().StartsWith("send"))
                {
                    Console.WriteLine("Enter receiver's username:");
                    string receiver = Console.ReadLine();
                    Console.WriteLine("Enter your message:");
                    string message = Console.ReadLine();

                    string fullCommand = $"send {receiver} {message}";
                    SendCommand(stream, fullCommand);
                }
                else if (command.ToLower().StartsWith("read"))
                {
                    string command = "read";
                    SendCommand(stream, command);
                }
                else
                {
                    SendCommand(stream, command);
                }
            }
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("CLIENT'S UP...");
        Client client = new Client();

        string server = "localhost";
        int port = 12345;

        client.Start(server, port);
    }
}