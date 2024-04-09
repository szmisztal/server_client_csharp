using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    private bool isRunning = true;

    public void Start(string server, int port)
    {
        using (TcpClient client = new TcpClient(server, port))
        using (NetworkStream stream = client.GetStream())
        {
            while (isRunning)
            {
                Console.WriteLine("COMMAND:");
                string command = Console.ReadLine();

                if (string.IsNullOrEmpty(command))
                {
                    continue; 
                }

                byte[] data = Encoding.UTF8.GetBytes(command);
                stream.Write(data, 0, data.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine("Response: " + response);

                if (command.Equals("stop", StringComparison.OrdinalIgnoreCase))
                {
                    isRunning = false;
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