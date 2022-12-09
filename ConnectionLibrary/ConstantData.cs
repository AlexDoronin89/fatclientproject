namespace ConnectionLibrary
{
    public static class ConstantData
    {
        public static class ConnectionData
        {
            public const string Ip = "127.0.0.1";
            public const int Port = 32345;
        }

        public static class Commands
        {
            public const string GetField = "getfield";
            public const string Step = "step";
            public const string EndStep = "endstep";
        }

        public static class ResponceResults
        {
            public const string Ok = "ok";
            public const string Error = "error";
        }

        public static class GameStates
        {
            public const string Go = "go";
            public const string Wait = "wait";
            public const string End = "end";
        }

        public static class PlayerChars
        {
            public const string Cross = "X";
            public const string Zero = "O";
        }

        public static class GameStatus
        {
            public const string Play = "Игра продолжается";
            public const string Draw = "Ничья";
            public const string WinCross = "Победили " + PlayerChars.Cross;
            public const string WinZero = "Победили " + PlayerChars.Zero;
        }
    }
}
