using ConnectionLibrary;
using ConnectionLibrary.Entity;
using ConnectionLibrary.Tools;
using System;
using System.Net.Sockets;

namespace Client
{
    public class Program
    {
        private const int RowCount = 3;
        private const int ColumnCount = 3;

        static void Main(string[] args)
        {
            TcpClient client = ConnectionTools.Connect();

            if (client != null)
                Play(client);

            Console.ReadLine();
        }

        private static void Play(TcpClient client)
        {
            CellValue[,] field = new CellValue[RowCount, ColumnCount];
            ClearField(field);
            string team = ConnectionTools.GetResponce(client).Value;

            Console.WriteLine("Вы играете за " + team);

            if (team == ConstantData.PlayerChars.Cross)
                Console.WriteLine("Ожидаем второго игрока");

            string gameStates = ConnectionTools.GetResponce(client).Value;
            Console.WriteLine(gameStates);
            bool isPlaying = true;

            GameStatus gameStatus = GameStatus.Play;

            //if (gameStatus == ConstantData.GameStatus.Play)
            //    isPlaying = true;

            CellValue currentValue = team == ConstantData.PlayerChars.Cross ?
                CellValue.Cross : CellValue.Zero;
            Console.WriteLine(currentValue);

            while (isPlaying)
            {
                Console.WriteLine("===============================");
                //Console.Clear();
                Console.WriteLine("Вы играете за " + team);

                switch (gameStates)
                {
                    case ConstantData.GameStates.Go:
                        Console.WriteLine(GetField(field));
                        Step(field, currentValue);
                        Console.WriteLine($"user {team} make step");
                        gameStatus = GetGameStatus(field);
                        ConnectionTools.SendResponce(client, ConstantData.GameStates.Go);
                        ConnectionTools.SendRequest(client, new Request() { Command = GetGameStatusString(gameStatus), Parameters = new string[] { GetField(field) } });
                        gameStates = ConstantData.GameStates.Wait;
                        break;
                    case ConstantData.GameStates.Wait:
                        Console.WriteLine("Ждем ход противника");
                        break;
                }

                Console.WriteLine("try get Response!");
                gameStates = ConnectionTools.GetResponce(client).Value;
                Console.WriteLine(gameStates);
                field = GetFieldCellValue(ConnectionTools.GetRequest(client).Parameters[0]);
                Console.WriteLine(field);
                if (ConnectionTools.GetResponce(client).Value == ConstantData.GameStates.End)
                    isPlaying = false;
            }
        }

        private static string GetGameStatusString(GameStatus status)
        {
            switch (status)
            {
                case GameStatus.Play:
                    return ConstantData.GameStatus.Play;
                case GameStatus.Draw:
                    return ConstantData.GameStatus.Draw;
                case GameStatus.WinCross:
                    return ConstantData.GameStatus.WinCross;
                case GameStatus.WinZero:
                    return ConstantData.GameStatus.WinZero;
            }

            return string.Empty;
        }

        private static void ProcessCommandEndStep(CellValue[,] field, TcpClient client)
        {
            bool isEndGame = GetGameStatus(field) == GameStatus.Play;
            string endResult = isEndGame ? ConstantData.ResponceResults.Ok : ConstantData.GameStates.End;

            ConnectionTools.SendRequest(client, new Request() { Command = ConstantData.Commands.EndStep });
        }

        private static int GetClumpValue(string valueName, int start, int end)
        {
            Console.WriteLine($"Введите {valueName} от {start} до {end}");
            return int.TryParse(Console.ReadLine(), out int value) ? value : -1;
        }

        private static void Step(CellValue[,] field, CellValue value)
        {
            int i = 0;
            int j = 0;

            do
            {
                i = GetClumpValue("i", 0, RowCount);
                j = GetClumpValue("j", 0, ColumnCount);
            } while (TryMakeStep(field, i, j, value) == false);
        }

        private static bool TryMakeStep(CellValue[,] field, int i, int j, CellValue value)
        {
            if (i <= 0 || j <= 0 || i > RowCount || j > ColumnCount || field[i - 1, j - 1] != CellValue.Empty)
                return false;

            field[i - 1, j - 1] = value;
            return true;
        }

        private static string GetField(CellValue[,] field)
        {
            string textField = string.Empty;

            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    switch (field[i, j])
                    {
                        case CellValue.Empty:
                            textField += ".";
                            break;
                        case CellValue.Cross:
                            textField += ConstantData.PlayerChars.Cross;
                            break;
                        case CellValue.Zero:
                            textField += ConstantData.PlayerChars.Zero;
                            break;
                    }
                }
                textField += "\n";
            }

            return textField;
        }

        private static CellValue[,] GetFieldCellValue(string textField)
        {
            CellValue[,] field = null;

            for (int i = 0; i < textField.Length; i++)
            {
                for (int j = 0; j < RowCount; j++)
                {
                    for (int k = 0; k < ColumnCount; k++)
                    {
                        switch (textField[i])
                        {
                            case ' ':
                                field[j, k] = CellValue.Empty;
                                break;
                            case 'X':
                                field[j, k] = CellValue.Cross;
                                break;
                            case '0':
                                field[j, k] = CellValue.Zero;
                                break;
                        }
                    }
                }
            }

            return field;
        }

        private static GameStatus GetGameStatus(CellValue[,] field)
        {
            if (CheckWinCondition(field, CellValue.Cross))
                return GameStatus.WinCross;

            if (CheckWinCondition(field, CellValue.Zero))
                return GameStatus.WinZero;

            if (HasEmpty(field) == false)
                return GameStatus.Draw;

            return GameStatus.Play;
        }

        private static bool CheckWinCondition(CellValue[,] field, CellValue value)
        {
            return field[0, 0] == value && field[0, 1] == value && field[0, 2] == value ||
                   field[1, 0] == value && field[1, 1] == value && field[1, 2] == value ||
                   field[2, 0] == value && field[2, 1] == value && field[2, 2] == value ||

                   field[0, 0] == value && field[1, 0] == value && field[2, 0] == value ||
                   field[0, 1] == value && field[1, 1] == value && field[2, 1] == value ||
                   field[0, 2] == value && field[1, 2] == value && field[2, 2] == value ||

                   field[0, 0] == value && field[1, 1] == value && field[2, 2] == value ||
                   field[2, 0] == value && field[1, 1] == value && field[0, 2] == value;
        }

        private static bool HasEmpty(CellValue[,] field)
        {
            for (int i = 0; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                    if (field[i, j] == CellValue.Empty)
                        return true;

            return false;
        }


        private static void ClearField(CellValue[,] field)
        {
            for (int i = 0; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                    field[i, j] = CellValue.Empty;
        }

        public static void SendRequest(TcpClient client, Request request)
        {
            Thread.Sleep(100);
            StreamWriter stream = new StreamWriter(client.GetStream());
            stream.Write(ConverterData.SerializeRequest(request));
            stream.Flush();
        }
    }

    enum CellValue
    {
        Empty = '.',
        Cross = 'X',
        Zero = 'O'
    }

    enum GameStatus
    {
        Play = -1,
        Draw = 0,
        WinCross = 1,
        WinZero = 2
    }
}

