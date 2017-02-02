using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SharpSolver
{
    public partial class Form1 : Form
    {
        // GLOBAL VARIABLES
        const int grid_size = 81;

        List<int> trialSq = new List<int>();
        List<int> trialRow = new List<int>();
        List<int> trialCol = new List<int>();

        const string DupGridMsg = "Entries Conflict!";
        const string EmptyGridMsg = "Fill Sudoku Please!";        
        const string GiveupMsg = "Timeout!";

        private bool isFull(List<int> grid)
        {
            return grid.Where(x => x.Equals(0)).Count() == 0;
        }

        // can be used more purposefully
        private int getTrialCelli(List<int> grid)
        {
            return grid.IndexOf(0);
        }

        private void ListFunc(int range, int value, List<int> trialList)
        {
            for (int x = 0; x < range; x++)
            {
                List<int> listRange = new List<int>();
                {
                    trialList.Add((trialList.Equals(trialCol) ? 9*x : x) + value);
                }
            }
        }

        private bool isLegal(int trialVal, int trialCelli, List<int> grid)
        {

            foreach (List<int> trialSq in sqs)// eachSq = 0; eachSq < 9; eachSq++)
            {

                isGridValid = IsGridValid(trialSq, grid);

                if (trialSq.Contains(trialCelli))
                {
                    foreach (int i in trialSq)
                        if (grid[i] != 0)
                            if (trialVal == grid[i])
                                return false;
                }                
            }

            //Array rows = row.Select(x => row.ConvertAll(y => y + x * 9)).ToArray();
            foreach (List<int> trialRow in rows)
            {
                isGridValid = IsGridValid(trialRow, grid);

                if (trialRow.Contains(trialCelli))
                {
                    foreach (int i in trialRow)
                        if (grid[i] != 0)
                            if (trialVal == grid[i])
                                return false;
                }
            }

            //Array columns = row.Select(x => row.ConvertAll(y => x + y * 9)).ToArray();
            foreach (List<int> trialCol in columns)
            {
                isGridValid = IsGridValid(trialCol, grid);

                if (trialCol.Contains(trialCelli))
                {
                    foreach (int i in trialCol)
                        if (grid[i] != 0)
                            if (trialVal == grid[i])
                                return false;
                }
            }

            return true;

        }

        private List<int> setCell(int trialVal, int trialCelli, List<int> grid)
        {
            grid[trialCelli] = trialVal;
            return grid;
        }
      
        private List<int> clearCell( int trialCelli, List<int> grid)
        {
            grid[trialCelli] = 0;
            return grid;
        }

        private bool hasSolution (List<int> grid)
        {
            if (isFull(grid))
                //print '\nSOLVED'
                return true;
            else
            {
                int trialCelli = getTrialCelli(grid);
                int trialVal = 1;
                bool solution_found = false;

                while (!dupEntryFound && isGridValid && !solution_found && (trialVal < 10) && !stopRequested )
                {
                    //print 'trial valu',trialVal,
                    if (isLegal(trialVal, trialCelli, grid))
                    {
                        grid = setCell(trialVal, trialCelli, grid);
                        if (hasSolution(grid))
                        {
                            solution_found = true;                            
                            return true;
                        }
                        else
                            clearCell(trialCelli, grid);
                    }
                    trialVal += 1;
                }                
                return solution_found;
            }
            
        }

        private bool IsGridValid(List<int> trialGrid, List<int> grid)
        {

            List<int> trialGrid_v = trialGrid.ConvertAll(x => grid[x]);

            var DupCounts =
                trialGrid_v.GroupBy(x => x).Where(x => x.Count() > 1).Count();

            if (DupCounts > 1)
            {
                timer.Stop();
                ProcessDuplicates(trialGrid_v, trialGrid);
                return false;
            }
            else
                return true;

        }

        private void ProcessDuplicates(List<int> grid_v, List<int> grid)
        {
            var duplicatesWithIndices = grid_v
                // Associate each name/value with an index
                .Select((Name, Index) => new { Name, Index })
                // Group according to name
                .GroupBy(x => x.Name)
                // Only care about Name -> {Index1, Index2, ..}
                .Select(xg => new
                {
                    Name = xg.Key,
                    Indices = xg.Select(x => x.Index)
                })
                // And groups with more than one index represent a duplicate key
                .Where(x => x.Indices.Count() > 1 && x.Name != 0 );

            List<int> list_v = null;
            List<int> list = null;


            foreach (var g in duplicatesWithIndices)
            {
                int n = g.Name;
                list_v = g.Indices.ToList();
                list = list_v.ConvertAll(x => grid[x]);

                foreach (int num in list)
                {
                    dgv.Rows[num / 9].Cells[num % 9].Style.BackColor = Color.Yellow;
                    dupEntryFound = true;
                }
            }
        }

        private void showErrorInvoke(Label lb, string msg, bool enableTimer = false)
        {
            warnTimer.Enabled = enableTimer;

            if (lb.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    lb.ForeColor = Color.Red;
                    lb.Text = msg;                    
                });
            }
            else
            {
                lb.ForeColor = Color.Red;
                lb.Text = msg;                
            }
        }

        private List<int> solveIt(List<int> rawGrid)
        {
            if (hasSolution(rawGrid))
            {
                solved = true;                
                return rawGrid;
            }
            else if (dupEntryFound)
            {
                showErrorInvoke(lbStatus, DupGridMsg);
                return null;
            }
            else return null;
        }

    }
}
