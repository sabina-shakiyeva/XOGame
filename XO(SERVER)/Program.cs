using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Numerics;

class Program
{
    static bool Turn1 = true;
    static char[,] board = new char[3, 3] { { '1', '2', '3' }, { '4', '5', '6' }, { '7', '8', '9' } };

    static void Main(string[] args)
    {
        var port = 5000;
        var ipAddress = IPAddress.Parse("192.168.100.16");
        var ep = new IPEndPoint(ipAddress, port);
        using var listener = new TcpListener(ep);

        try
        {
            listener.Start();
            Console.WriteLine("Server is running...");

            while (true)
            {
                Console.WriteLine("Waiting for Player 1 to connect...");
                var client1 = listener.AcceptTcpClient();
                Console.WriteLine($"Player 1 connected: {client1.Client.RemoteEndPoint}");

                Console.WriteLine("Waiting for Player 2 to connect...");
                var client2 = listener.AcceptTcpClient();
                Console.WriteLine($"Player 2 connected: {client2.Client.RemoteEndPoint}");

                var stream1 = client1.GetStream();
                var stream2 = client2.GetStream();
                var reader1 = new StreamReader(stream1);
                var reader2 = new StreamReader(stream2);
                var writer1 = new StreamWriter(stream1) { AutoFlush = true };
                var writer2 = new StreamWriter(stream2) { AutoFlush = true };

                _ = Task.Run(() => Game(reader1, reader2, writer1, writer2));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static async Task Game(StreamReader reader1, StreamReader reader2, StreamWriter writer1, StreamWriter writer2)
    {
        while (true)
        {
            Print(writer1, writer2);

            var currentWriter = Turn1 ? writer1 : writer2;
            var currentReader = Turn1 ? reader1 : reader2;
            var player = Turn1 ? "First player (X)" : "Second player (O)";

            currentWriter.WriteLine($"{player} your turn(Write position(1-9)");

            var current = await currentReader.ReadLineAsync();
            Console.WriteLine($"{player} chose position: {current}");

            if (int.TryParse(current, out int position) && position >= 1 && position <= 9)
            {
                if (GameMove(position, Turn1 ? 'X' : 'O'))
                {
                    if (checkWin())
                    {
                        Print(writer1, writer2);
                        writer1.WriteLine($"{player} wins!");
                        writer2.WriteLine($"{player} wins!");
                        Console.WriteLine($"{player} wins!");
                        break;
                    }
                    else if (checkDraw())
                    {
                        Print(writer1, writer2);
                        writer1.WriteLine("It's a draw!");
                        writer2.WriteLine("It's a draw!");
                        Console.WriteLine("It's a draw!");
                        break;
                    }
                    Turn1 = !Turn1;
                }
                else
                {
                    currentWriter.WriteLine("Wrong move!");
                }
            }
            else
            {
                currentWriter.WriteLine("Wrong input!");
            }
        }
    }

    static bool GameMove(int position, char mark)
    {
        int row = (position - 1) / 3;
        int col = (position - 1) % 3;
        if (board[row, col] != 'X' && board[row, col] != 'O')
        {
            board[row, col] = mark;
            return true;
        }
        return false;
    }

    static void Print(StreamWriter writer1, StreamWriter writer2)
    {

        string boardState = $@"
{board[0, 0]} | {board[0, 1]} | {board[0, 2]}
---------
{board[1, 0]} | {board[1, 1]} | {board[1, 2]}
---------
{board[2, 0]} | {board[2, 1]} | {board[2, 2]}
";
        writer1.WriteLine(boardState);
        writer2.WriteLine(boardState);
        Console.WriteLine("Board state sent to players:\n" + boardState);
    }

    static bool checkWin()
    {
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2]) return true;
            if (board[0, i] == board[1, i] && board[1, i] == board[2, i]) return true;
        }
        if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2]) return true;
        if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0]) return true;
        return false;
    }

    static bool checkDraw()
    {
        foreach (var cell in board)
            if (cell != 'X' && cell != 'O') return false;
        return true;
    }
}
