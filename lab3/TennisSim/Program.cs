using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ScottPlot;

namespace TennisSimulation
{
    class Zone
    {
        public string Name { get; }
        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }

        public Zone(int length, int width, string name)
        {
            Name = name;
            if (name == "A")
            {
                MinX = 0;
                MaxX = (length / 4) - 1;
                MinY = 0;
                MaxY = width - 1;
            }
            else if (name == "B")
            {
                MinX = length / 4;
                MaxX = (length / 2) - 1;
                MinY = width / 2;
                MaxY = width - 1;
            }
            else if (name == "C")
            {
                MinX = length / 4;
                MaxX = (length / 2) - 1;
                MinY = 0;
                MaxY = (width / 2) - 1;
            }
            else if (name == "D")
            {
                MinX = (length / 4) * 3;
                MaxX = length - 1;
                MinY = 0;
                MaxY = width - 1;
            }
            else if (name == "E")
            {
                MinX = length / 2;
                MaxX = ((length / 4) * 3) - 1;
                MinY = 0;
                MaxY = (width / 2) - 1;
            }
            else if (name == "F")
            {
                MinX = length / 2;
                MaxX = ((length / 4) * 3) - 1;
                MinY = width / 2;
                MaxY = width - 1;
            }
            else
            {
                throw new ArgumentException($"Unknown zone name: {name}");
            }
        }
    }

    class Court
    {
        public Dictionary<string, Zone> AllZones { get; }
        public int MatrixWidth { get; }
        public int MatrixLength { get; }

        public Court(int numberOfSquares)
        {
            int width = (int)Math.Sqrt(Math.Max(1, numberOfSquares / 3));
            if (width < 1) width = 1;
            MatrixWidth = width;
            MatrixLength = width * 3;
            AllZones = DivisionIntoZones(MatrixLength, MatrixWidth);
        }

        private Dictionary<string, Zone> DivisionIntoZones(int length, int width)
        {
            var zones = new Dictionary<string, Zone>
            {
                { "A", new Zone(length, width, "A") },
                { "B", new Zone(length, width, "B") },
                { "C", new Zone(length, width, "C") },
                { "D", new Zone(length, width, "D") },
                { "E", new Zone(length, width, "E") },
                { "F", new Zone(length, width, "F") },
            };
            return zones;
        }

        public void StartPositions(Zone a, Zone d, Player player, Dummy dummy)
        {
            int playerX = (int)Math.Round((a.MinX + a.MaxX) / 2.0);
            int playerY = (int)Math.Round((a.MinY + a.MaxY) / 2.0);
            player.X = playerX;
            player.Y = playerY;

            int dummyX = d.MinX; 
            int dummyY = (int)Math.Round((d.MinY + d.MaxY) / 2.0);
            dummy.X = dummyX;
            dummy.Y = dummyY;
        }
    }

    class Ball
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Ball()
        {
            X = -1;
            Y = -1;
        }

        public void NewLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class Dummy
    {
        public int R { get; }
        public int L { get; }
        public int X { get; set; }
        public int Y { get; set; }

        private Random rnd;

        public Dummy(int r, int l, Random rnd)
        {
            R = r;
            L = l;
            this.rnd = rnd; X = 0; Y = 0;
        }

        public void Pitch(Dictionary<string, Zone> dictOfZones, Ball ball)
        {
            int randZone = rnd.Next(1, 4);
            Zone zone;
            if (randZone == 1) zone = dictOfZones["A"];
            else if (randZone == 2) zone = dictOfZones["B"];
            else zone = dictOfZones["C"];

            int squareX = rnd.Next(zone.MinX, zone.MaxX + 1);
            int squareY = rnd.Next(zone.MinY, zone.MaxY + 1);
            ball.NewLocation(squareX, squareY);
        }

        public bool Move(Ball ball)
        {
            int ballX = ball.X;
            int ballY = ball.Y;
            int movements = L;

            while ((Y + R) < ballY || (Y - R) > ballY)
            {
                if (movements > 0)
                {
                    if ((Y + R) < ballY) Y++;
                    else if ((Y - R) > ballY) Y--;
                    movements--;
                }
                else return false;
            }

            while ((X + R) < ballX || (X - R) > ballX)
            {
                if (movements > 0)
                {
                    if ((X + R) < ballX) X++;
                    else if ((X - R) > ballX) X--;
                    movements--;
                }
                else return false;
            }

            return true;
        }
    }

    class Player
    {
        public int R { get; } 
        public int L { get; }
        public int X { get; set; }
        public int Y { get; set; }

        private Random rnd;

        public Player(int r, int l, Random rnd)
        {
            R = 2 * r;
            L = l;
            X = 0; Y = 0;
            this.rnd = rnd;
        }

        public bool Pitch(Zone zone, Ball ball, string message)
        {
            int squareX = rnd.Next(zone.MinX, zone.MaxX + 1);
            int squareY = rnd.Next(zone.MinY, zone.MaxY + 1);

            if (message == "Не промах")
            {
                ball.NewLocation(squareX, squareY);
                return false;
            }
            else
            {
                int missX = rnd.Next(Math.Max(squareX - 1, zone.MinX - 2), Math.Min(squareX + 1, zone.MaxX + 2) + 1);
                int missY = rnd.Next(Math.Max(squareY - 1, zone.MinY - 2), Math.Min(squareY + 1, zone.MaxY + 2) + 1);
                int attempts = 0;
                while (missX == squareX && missY == squareY && attempts < 10)
                {
                    missX = rnd.Next(Math.Max(squareX - 1, zone.MinX - 2), Math.Min(squareX + 1, zone.MaxX + 2) + 1);
                    missY = rnd.Next(Math.Max(squareY - 1, zone.MinY - 2), Math.Min(squareY + 1, zone.MaxY + 2) + 1);
                    attempts++;
                }

                if (missX < zone.MinX || missX > zone.MaxX || missY < zone.MinY || missY > zone.MaxY)
                {
                    return true;
                }
                else
                {
                    ball.NewLocation(missX, missY);
                    return false;
                }
            }
        }

        public bool TacticPitch(int maxX, int maxY, Ball ball, string message, Dictionary<string, Zone> dictOfZones)
        {
            if (message == "Не промах")
            {
                ball.NewLocation(maxX, maxY);
                return false;
            }
            else
            {
                int missX = rnd.Next(maxX - 1, maxX + 2);
                int missY = rnd.Next(maxY - 1, maxY + 2);
                int attempts = 0; while (missX == maxX && missY == maxY && attempts < 10)
                {
                    missX = rnd.Next(maxX - 1, maxX + 2);
                    missY = rnd.Next(maxY - 1, maxY + 2);
                    attempts++;
                }

                Zone zoneD = dictOfZones["D"];
                Zone zoneE = dictOfZones["E"];
                if ((missY < zoneE.MinY) || (missX > zoneD.MaxX) || (missY > zoneD.MaxY))
                {
                    return true;
                }
                else
                {
                    ball.NewLocation(missX, missY);
                    return false;
                }
            }
        }

        public bool ZoneSelection(Dictionary<string, Zone> dictOfZones, Ball ball, string flag, string tactic, Dummy dummy)
        {
            if (tactic == "random")
            {
                string message = "Не промах";
                if (flag == "first")
                {
                    int rand = rnd.Next(1, 3);
                    var zone = (rand == 1) ? dictOfZones["E"] : dictOfZones["F"];
                    return Pitch(zone, ball, message);
                }
                else if (flag == "default")
                {
                    int rand = rnd.Next(1, 4);
                    Zone zone = rand == 1 ? dictOfZones["D"] : (rand == 2 ? dictOfZones["E"] : dictOfZones["F"]);
                    string msg = (rnd.Next(1, 101) <= 5) ? "Промах" : "Не промах";
                    return Pitch(zone, ball, msg);
                }
                else throw new ArgumentException($"Unknown flag: {flag}");
            }
            else if (tactic == "far square")
            {
                string message = "Не промах";
                Zone zoneD = dictOfZones["D"];
                Zone zoneE = dictOfZones["E"];
                int dummyX = dummy.X;
                int dummyY = dummy.Y;
                int maxX = 0;
                int maxY = 0;

                if (flag == "first")
                {
                    maxX = Math.Abs(dummyX - zoneE.MinX) >= Math.Abs(dummyX - zoneE.MaxX) ? zoneE.MinX : zoneE.MaxX;
                    maxY = Math.Abs(dummyY - zoneD.MinY) >= Math.Abs(dummyY - zoneD.MaxY) ? zoneD.MinY : zoneD.MaxY;
                    return TacticPitch(maxX, maxY, ball, message, dictOfZones);
                }
                else 
                {
                    maxX = Math.Abs(dummyX - zoneE.MinX) >= Math.Abs(dummyX - zoneD.MaxX) ? zoneE.MinX : zoneD.MaxX;
                    maxY = Math.Abs(dummyY - zoneD.MinY) >= Math.Abs(dummyY - zoneD.MaxY) ? zoneD.MinY : zoneD.MaxY;
                    string msg = (rnd.Next(1, 101) <= 5) ? "Промах" : "Не промах";
                    return TacticPitch(maxX, maxY, ball, msg, dictOfZones);
                }
            }
            else throw new ArgumentException($"Unknown tactic: {tactic}");
        }

        public bool Move(Ball ball)
        {
            int ballX = ball.X;
            int ballY = ball.Y;
            int movements = L;

            if (ballX < X)
            {
                while (X > ballX)
                {
                    if (movements > 0)
                    {
                        X--; movements--;
                    }
                    else return false;
                }
                while ((Y + R) < ballY || (Y - R) > ballY)
                {
                    if (movements > 0)
                    {
                        if ((Y + R) < ballY) Y++;
                        else if ((Y - R) > ballY) Y--;
                        movements--;
                    }
                    else return false;
                }
            }
            else if (ballX > X)
            {
                while ((Y + R) < ballY || (Y - R) > ballY)
                {
                    if (movements > 0)
                    {
                        if ((Y + R) < ballY) Y++;
                        else if ((Y - R) > ballY) Y--;
                        movements--;
                    }
                    else return false;
                }
                while ((X + R) < ballX || (X - R) > ballX)
                {
                    if (movements > 0)
                    {
                        if ((X + R) < ballX) X++;
                        else if ((X - R) > ballX) X--;
                        movements--;
                    }
                    else return false;
                }
            }
            return true;
        }
    }

    static class Simulation
    {

        public static bool Game(int n, int r, int l, string[] playerPitch, string playerTactic, Random rnd)
        {
            Court court = new Court(n);
            var zones = court.AllZones;
            Player player = new Player(r, l, rnd);
            Dummy dummy = new Dummy(r, l, rnd);
            Ball ball = new Ball();

            court.StartPositions(zones["A"], zones["D"], player, dummy);

            int playerScore = 0;
            int dummyScore = 0;

            while (true)
            {

                player.ZoneSelection(zones, ball, playerPitch[0], playerTactic, dummy);

                while (true)
                {

                    bool dummyCan = dummy.Move(ball);
                    if (!dummyCan)
                    {
                        playerScore++;
                        court.StartPositions(zones["A"], zones["D"], player, dummy);
                        break;
                    }


                    dummy.Pitch(zones, ball);

                    bool playerCan = player.Move(ball);
                    if (!playerCan)
                    {
                        dummyScore++;
                        court.StartPositions(zones["A"], zones["D"], player, dummy);
                        break;
                    }

                    bool miss = player.ZoneSelection(zones, ball, playerPitch[1], playerTactic, dummy);
                    if (miss)
                    {
                        dummyScore++;
                        court.StartPositions(zones["A"], zones["D"], player, dummy);
                        break;
                    }
                    else
                    {
                        //
                    }
                }

                if ((playerScore >= 10 || dummyScore >= 10) && Math.Abs(playerScore - dummyScore) >= 2)
                    break;
            }

            return playerScore > dummyScore;
        }


        public static bool RunTournament(int n, int r, int l, Random rnd)
        {
            string[] playerPitch = new[] { "first", "default" };
            string playerTactic = "random";

            int playerSetWin = 0, dummySetWin = 0;

            while (playerSetWin < 2 && dummySetWin < 2)
            {
                int playerGameWin = 0, dummyGameWin = 0;
                while (true)
                {
                    bool playerWonGame = Game(n, r, l, playerPitch, playerTactic, rnd);
                    if (playerWonGame) playerGameWin++;
                    else dummyGameWin++;

                    if ((playerGameWin >= 6 || dummyGameWin >= 6) && Math.Abs(playerGameWin - dummyGameWin) >= 2)
                        break;
                }

                if (playerGameWin > dummyGameWin) playerSetWin++;
                else dummySetWin++;
            }

            return playerSetWin > dummySetWin;
        }
    }

    class Program
    {
        static void Main()
        {
            int[] nValues = new[] { 48, 192, 432, 768 };  
            int rMin = 1, rMax = 5;
            int lMin = 1, lMax = 10;

            int rCount = rMax - rMin + 1;
            int lCount = lMax - lMin + 1;

            int trialsPerCell = 100;

            Random rnd = new Random(12345);

            Console.WriteLine("Начало симуляции. Это может занять некоторое время...");

            foreach (var n in nValues)
            {
                double[,] playerWinFraction = new double[rCount, lCount];

                for (int r = rMin; r <= rMax; r++)
                {
                    for (int l = lMin; l <= lMax; l++)
                    {
                        int playerTotalWins = 0;

                        for (int t = 0; t < trialsPerCell; t++)
                        {
                            bool playerWonTournament = Simulation.RunTournament(n, r, l, rnd);
                            if (playerWonTournament) playerTotalWins++;
                        }

                        playerWinFraction[r - rMin, l - lMin] = (double)playerTotalWins / trialsPerCell;
                        Console.WriteLine($"n={n} r={r} l={l} -> win fraction = {playerWinFraction[r - rMin, l - lMin]:P1}");
                    }
                }

                SaveHeatmap(playerWinFraction, rMin, rMax, lMin, lMax, n);
            }

            Console.WriteLine("Симуляция завершена. PNG-файлы с графиками сохранены в каталоге с исполняемым файлом.");
        }

        static void SaveHeatmap(double[,] matrix, int rMin, int rMax, int lMin, int lMax, int n)
        {

            int rCount = rMax - rMin + 1;
            int lCount = lMax - lMin + 1;

            double[,] heat = new double[lCount, rCount];
            for (int ri = 0; ri < rCount; ri++)
                for (int li = 0; li < lCount; li++)
                    heat[li, ri] = matrix[ri, li];

            var plt = new ScottPlot.Plot(800, 600);
            var hm = plt.AddHeatmap(heat);
            hm.Update(heat); 

            plt.Title($"Вероятность победы агента (heatmap) — n={n}");
            plt.XLabel("r (радиус обзора)"); 
            plt.YLabel("l (максимальное расстояние перемещения)");

            double[] xTicksPos = new double[rCount];
            string[] xTicksLabels = new string[rCount];
            for (int i = 0; i < rCount; i++)
            {
                xTicksPos[i] = i + 0.5;
                xTicksLabels[i] = (rMin + i).ToString();
            }
            plt.XTicks(xTicksPos, xTicksLabels);
            double[] yTicksPos = new double[lCount];
            string[] yTicksLabels = new string[lCount];
            for (int i = 0; i < lCount; i++)
            {
                yTicksPos[i] = i + 0.5;
                yTicksLabels[i] = (lMin + i).ToString();
            }
            plt.YTicks(yTicksPos, yTicksLabels);

            plt.AddColorbar(hm);

            string filename = $"heatmap_n_{n}.png";
            plt.SaveFig(filename);
            Console.WriteLine($"Saved heatmap: {filename}");
        }
    }
}