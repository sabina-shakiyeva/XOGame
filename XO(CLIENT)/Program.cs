using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

var port = 5000;
var ipAddress = IPAddress.Parse("192.168.100.16");
var ep = new IPEndPoint(ipAddress, port);

Console.Write("Choose player (1 or 2): ");
int player = int.Parse(Console.ReadLine());
var client = new TcpClient();

try
{
    client.Connect(ep);
    using var stream = client.GetStream();
    using var reader = new StreamReader(stream);
    using var writer = new StreamWriter(stream) { AutoFlush = true };

    while (true)
    {
        var message = await reader.ReadLineAsync();


        if (message.Contains("your turn(Write position(1-9)"))
        {
            Console.Write("Enter position (1-9): ");
            var current = Console.ReadLine();
            writer.WriteLine(current);
            Console.WriteLine("Position sent to server: " + current);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}





