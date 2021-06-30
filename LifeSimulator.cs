using System;
using System.IO;
using System.Threading;

namespace GameOfLife
{
    public class LifeSimulator
    {
        private Thread simulatorThread;
        private bool[] lifeSimulator = new bool[Program.SIZEX * Program.SIZEY];
        private bool[] lifeSimulatorNewState = new bool[Program.SIZEX * Program.SIZEY];
        private byte[] drawData = new byte[Program.SIZEX * Program.SIZEY * 4];
        private AutoResetEvent simulatorSync;
        private long nextUpdateTime;
        private bool running = true;
        private Action<byte[]> UpdateFrame;

        public LifeSimulator(AutoResetEvent simulatorSync, Action<byte[]> UpdateFrame)
        {
            Load("initialstate.txt");
            for (int i = 3; i < drawData.Length; i = i + 4)
            {
                drawData[i] = 255;
            }
            UpdateFrame(drawData);
            this.simulatorSync = simulatorSync;
            this.simulatorThread = new Thread(new ThreadStart(UpdateState));
            this.UpdateFrame = UpdateFrame;
            simulatorThread.Start();
        }

        private void Load(string fileName)
        {
            int x = -1;
            int y = 0;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    if (x == -1)
                    {
                        x = currentLine.Length;
                    }
                    y++;
                }
            }
            int xinit = Program.SIZEX / 2 - x / 2;
            int yinit = Program.SIZEY / 2 - y / 2;
            int xDraw = xinit;
            int yDraw = yinit;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    foreach (char c in currentLine)
                    {
                        if (c == '1')
                        {
                            lifeSimulator[GetCellPosition(xDraw, yDraw)] = true;
                        }
                        xDraw++;
                    }
                    xDraw = xinit;
                    yDraw++;
                }
            }
        }

        public void Stop()
        {
            running = false;
            simulatorThread.Join();
        }

        public void UpdateState()
        {
            while (running)
            {
                if (simulatorSync.WaitOne(10))
                {
                    while (DateTime.UtcNow.Ticks < nextUpdateTime)
                    {
                        Thread.Sleep(1);
                    }
                    for (int y = 0; y < Program.SIZEY; y++)
                    {
                        for (int x = 0; x < Program.SIZEX; x++)
                        {
                            bool newState = GetNewStateForCell(x, y);
                            lifeSimulatorNewState[GetCellPosition(x, y)] = newState;
                            int pixelID = (Program.SIZEX * y * 4) + (x * 4);
                            if (newState)
                            {
                                drawData[pixelID] = 0;
                            }
                            else
                            {
                                drawData[pixelID] = 255;
                            }
                            drawData[pixelID + 1] = drawData[pixelID];
                            drawData[pixelID + 2] = drawData[pixelID];
                        }
                    }
                    var temp = lifeSimulator;
                    lifeSimulator = lifeSimulatorNewState;
                    lifeSimulatorNewState = temp;
                    nextUpdateTime = DateTime.UtcNow.Ticks + TimeSpan.TicksPerMillisecond * 100;
                    UpdateFrame(drawData);
                }
            }
        }

        public bool GetNewStateForCell(int x, int y)
        {
            int aliveNeighbours = 0;
            bool isLeftMargin = x == 0;
            bool isRightMargin = x == Program.SIZEX - 1;
            bool isTopMargin = y == 0;
            bool isBottomMargin = y == Program.SIZEY - 1;
            if (!isLeftMargin)
            {
                //Top Left
                if (!isTopMargin && lifeSimulator[GetCellPosition(x - 1, y - 1)])
                {
                    aliveNeighbours++;
                }
                //Bottom Left
                if (!isBottomMargin && lifeSimulator[GetCellPosition(x - 1, y + 1)])
                {
                    aliveNeighbours++;
                }
                //Left
                if (lifeSimulator[GetCellPosition(x - 1, y)])
                {
                    aliveNeighbours++;
                }
            }
            if (!isRightMargin)
            {
                //Top Right
                if (!isTopMargin && lifeSimulator[GetCellPosition(x + 1, y - 1)])
                {
                    aliveNeighbours++;
                }
                //Bottom Right
                if (!isBottomMargin && lifeSimulator[GetCellPosition(x + 1, y + 1)])
                {
                    aliveNeighbours++;
                }
                //Right
                if (lifeSimulator[GetCellPosition(x + 1, y)])
                {
                    aliveNeighbours++;
                }
            }
            //Top
            if (!isTopMargin && lifeSimulator[GetCellPosition(x, y - 1)])
            {
                aliveNeighbours++;
            }
            //Bottom
            if (!isBottomMargin && lifeSimulator[GetCellPosition(x, y + 1)])
            {
                aliveNeighbours++;
            }
            //0, 1 or 4 neighbours = dead
            //2 = stay on
            //3 = turn on
            return (aliveNeighbours == 2 && lifeSimulator[GetCellPosition(x, y)]) || aliveNeighbours == 3;
        }

        public int GetCellPosition(int x, int y)
        {
            return y * Program.SIZEX + x;
        }
    }
}