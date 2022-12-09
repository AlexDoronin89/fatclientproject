using System;
using ConnectionLibrary;
using ConnectionLibrary.Tools;
using ConnectionLibrary.Entity;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        private static void Main(string[] args)
        {
            TcpListener server = ConnectionTools.GetListener();

            Logger.Log("SERVER STARTED");

            TcpClient playerCrossClient = AcceptClient(server, ConstantData.PlayerChars.Cross);
            TcpClient playerZeroClient = AcceptClient(server, ConstantData.PlayerChars.Zero);

            ConnectionTools.SendResponce(playerCrossClient, ConstantData.GameStates.Go);
            ConnectionTools.SendResponce(playerZeroClient, ConstantData.GameStates.Wait);
            TcpClient goingPlayer = playerZeroClient;

            while (playerCrossClient.Connected && playerZeroClient.Connected)
            {
                if (goingPlayer != playerCrossClient)
                {
                    string message = ConnectionTools.GetStringRequest(playerCrossClient);

                    Console.WriteLine(message);
                    ConnectionTools.SendMessage(playerZeroClient, message);
                    goingPlayer = playerCrossClient;
                }
                else
                {
                    string message = ConnectionTools.GetStringRequest(playerCrossClient);

                    Console.WriteLine(message);
                    ConnectionTools.SendMessage(playerCrossClient, message);
                    goingPlayer = playerZeroClient;
                }
            }

            playerCrossClient.Close();
            playerZeroClient.Close();

            server.Stop();

            Logger.Log("SERVER STOPED");
            Console.ReadLine();
        }


        private static TcpClient AcceptClient(TcpListener server, string teame)
        {
            TcpClient player = server.AcceptTcpClient();
            Logger.Log($"player {teame} connected from { player.Client.RemoteEndPoint}");
            ConnectionTools.SendResponce(player, teame);

            return player;
        }
    }
}