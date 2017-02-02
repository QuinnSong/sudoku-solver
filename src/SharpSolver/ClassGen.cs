using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSolver
{
    public struct Square
    {
        public int Across;
        public int Down;
        public int Region;
        public int Value;
        public int Index;
    }

    class ClassGen
    {
        public List<Square> Sudoku = new List<Square>() { };
        Random r = new Random();

        public List<Square> GenerateGrid()
        {
            Clear();
            List<Square> Squares = new List<Square>( new Square[81] );
            List<List<int>> Available = new List<List<int>>(new List<int>[81]);
            int c = 0;
            for (int x = 0; x < Available.Count; x++)
            {
                Available[x] = new List<int>();
                for (int i = 1; i <= 9; i++)
                {
                    Available[x].Add(i);
                }
            }

            do {
                if (Available[c].Count != 0)
                {
                    int i = GetRan(0, Available[c].Count - 1);
                    int z = Available[c].ElementAt(i);
                    if (!Conflicts(Squares, Item(c, z)))
                    {
                        Squares[c] = Item(c, z);
                        Available[c].RemoveAt(i);
                        c += 1;
                    }
                    else
                        Available[c].RemoveAt(i);

                }
                else
                {
                    for (int y = 1; y <= 9; y++)
                    {
                        Available[c].Add(y);
                    }
                    Squares[c - 1] = new Square() ;
                    c -= 1;
                }

            }while (c != 81);

            int j;
            for (j = 0; j <= 80; j++)
            {
                Sudoku.Add(Squares[j]);
            }
            return Sudoku;

        }
        public void Clear()
        {
            Sudoku.Clear();
        }

        private Square Item(int n, int v)
        {
            n += 1;
            return new Square()
            {
                Across = GetAcrossFromNumber(n),
                Down = GetDownFromNumber(n),
                Region = GetRegionFromNumber(n),
                Value = v,
                Index = n - 1
            };
        }

        private int GetRan(int lower, int upper)
        {
            return r.Next(lower, upper + 1);
        }

        private bool Conflicts(List<Square> CurrentValues, Square test)
        {
            foreach (Square s in CurrentValues)
            {
                if ((s.Across != 0 && s.Across == test.Across) ||
                    (s.Down != 0 && s.Down == test.Down) ||
                    (s.Region != 0 && s.Region == test.Region))
                {
                    if (s.Value == test.Value) return true;
                }
            }
            return false;
        }

        public int GetAcrossFromNumber(int n)
        {
            int k;
            k = n % 9;
            return k == 0 ? 9 : k;
        }

        public int GetDownFromNumber(int n)
        {
            int k;
            k = (GetAcrossFromNumber(n) == 9)? n / 9 : n / 9 + 1;
            return k;
        }

        public int GetRegionFromNumber(int n)
        {
            int k = 0;
            int a = GetAcrossFromNumber(n);
            int d = GetDownFromNumber(n);

            if(1 <= a && a < 4 & 1 <= d && d < 4) {
            k = 1;}
            else if (4 <= a && a < 7 && 1 <= d && d < 4) {
            k = 2;}
            else if (7 <= a && a < 10 && 1 <= d && d < 4 ) {
            k = 3;}
            else if (1 <= a && a < 4 && 4 <= d && d < 7 ) {
            k = 4;}
            else if (4 <= a && a < 7 && 4 <= d && d < 7 ) {
            k = 5;}
            else if (7 <= a && a < 10 && 4 <= d && d < 7 ) {
            k = 6;}
            else if (1 <= a && a < 4 && 7 <= d && d < 10 ) {
            k = 7;}
            else if (4 <= a && a < 7 && 7 <= d && d < 10 ) {
            k = 8;}
            else if ( 7<= a && a < 10 && 7 <= d && d < 10 ) {
            k = 9;}            
        
        return k;
        }
    }
}
