using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SharpSolver
{
    public partial class Form1 : Form
    {
        DateTime start_time;
        private BackgroundWorker bw;
        DataGridViewRow Arow;
        DataGridViewTextBoxColumn Acolumn;

        static bool isGridValid = true; // flag for grid valid check
        // Initialize the flag to false.
        static bool dupEntryFound = false; // flag to mark for duplicates
        bool stopRequested = false;     // true if Reset is pressed
        bool solved = false;        

        List<int> row = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        Array rows, columns, sqs;
        List<List<int>> evens;
        Color[] NineColors = {Color.AliceBlue, Color.LightSkyBlue, Color.BlanchedAlmond,
                             Color.MistyRose, Color.PaleTurquoise, Color.LightPink,
                             Color.LightSlateGray, Color.Thistle, Color.LemonChiffon};
        
        //List<int> redOn = new List<int>(); //contains cell index of invalid entries
        String[] sKeys = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        List<int> nums = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        // preset grid (most difficult sudoku ever?)
        List<string> listGen; /* = new List<string>()
        {"8", "", "", "", "", "", "", "", "",
         "", "", "3", "6", "", "", "", "", "",
         "", "7", "", "", "9", "", "2", "", "",
         "", "5", "", "", "", "7", "", "", "",
         "", "", "", "", "4", "5", "7", "", "",
         "", "", "", "1", "", "", "", "3", "",
         "", "", "1", "", "", "", "", "6", "8",f
         "", "", "8", "5", "", "", "", "1", "",
         "", "9", "", "", "", "", "4", "", "",};
        */
        List<string> list_from_UI;
        private int[] _setRowPosition = { 0, 0, 0, 3, 3, 3, 6, 6, 6 };
        private int[] _setColPosition = { 0, 3, 6, 0, 3, 6, 0, 3, 6 };
        ClassGen cg = new ClassGen();

        // Time limit to solve question
        const int time_limit = 30;

        List<string> answer = new List<string>(new string[81]);

        // To bring window to the front
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Enum defining Game Level
        /// </summary>
        public enum GameLevel
        {
            SIMPLE,
            MEDIUM,
            COMPLEX
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public Form1()
        {
            InitializeComponent();
            Form_init();            
        }

        private void Form_init()
        {
            InitializeDataGridView(9, 9); // draw the 9x9 grid

            //row.ForEach(x =>row.ForEach(y=> dgv.Rows[x].Cells[y].Value =list[ x * 9 + y]));

            CalRowAndColumnGrid(); // calculate to get rows, columns, sqs
            this.dgv.RowCount = 9;
            this.dgv.ColumnCount = 9;

            cbLevel.SelectedIndex = 0;
            Reset();

            
        }

        private void Color_Theme()
        {
            //List<int[]> blocks = new List<int[]>(){
            //    new int[3]{0,1,2}, new int[3]{3,4,5}, new int[3]{6,7,8}};

            //int colorIndex = 0;
            //foreach (int[] i in blocks)
            //{
            //    foreach (int[] j in blocks)
            //    {
            //        i.ToList().ForEach(x => j.ToList().ForEach(y =>
            //            dgv[x, y].Style.BackColor = NineColors[colorIndex]));
            //        colorIndex++;
            //    }
            //}
            for (int x = 0; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    dgv[y, x].Style.BackColor = 
                    NineColors[y / 3 + (x / 3) * 3];
                }
            }           
            
        }

        private void Reset()
        {
            if (lbStatus.Text == "")
                Color_Theme();
            lbTimer.Text = @"00:00:00";
            //lbStatus.Text = "";
            pBar.Value = pBar.Minimum;
            //redOn.Clear();
            stopRequested = false;
            dupEntryFound = false;
            solved = false;
            //dgv.BeginEdit(false);
            dgv.Cursor = Cursors.Default;  
            dgv.ClearSelection();
        }

        private void InitializeDataGridView(int rows, int columns)
        {
            dgv.DefaultCellStyle.Font = new Font("Tahoma", 22F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

            for (int i = 0; i < columns; i++)
            {
                Acolumn = new DataGridViewTextBoxColumn();
                //OK I know this only works normally for 26 chars(columns)
                // I leave the rest of the Excel columns up to you to figure out :o)
                Acolumn.Width = 35;
                dgv.Columns.Add(Acolumn);
            }

            //helps to get rid of the selection triangle?
            dgv.RowHeadersDefaultCellStyle.Padding = new Padding(3);
            for (int i = 0; i < rows; i++)
            {
                Arow = new DataGridViewRow();
                Arow.Height = 35;
                dgv.Rows.Add(Arow);
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            List<int> grid = (List<int>)e.Argument;

            List<int> result = solveIt(grid); // pass the answer back

            int index = 0;
            if (!stopRequested && result != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        dgv.Rows[i].Cells[j].Value = result[index];
                        index++;
                    }
                }               
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (solved )
            {
                timer.Stop(); //stop the timer
                pBar.Value = pBar.Minimum;
                SetLabel(lbStatus, "Bingo! :-)");               
                lbStatus.ForeColor = Color.Black;                
                dgv.Cursor = Cursors.Default;
                toggleBtnGroup(true); // enable buttons upon completion
                SetForegroundWindow(this.Handle); // bring to front up completion
            }
            else if (stopRequested || dupEntryFound)
            {
                Reset();
            }           
         
        }
        internal void doSolver(List<int> matrix)
        {
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork  += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync(matrix);
        }

        private List<int> GetGridFromUI()
        {
            // Get the grid matrix
            List<int> matrix = new List<int>(81);
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var value = dgv.Rows[i].Cells[j].Value;
                    matrix.Add((value == null || value.ToString() == "" || value.ToString() == " ") ? 0 : Int32.Parse(value.ToString()));
                }
            }
            return matrix;
        }

        private void toggleBtnGroup(bool isEnabled)
        {
            btnRan.Enabled = isEnabled;
            btnAnswer.Enabled = isEnabled;
            btnSave.Enabled = isEnabled;
            btnClear.Enabled = isEnabled;
            btnStart.Enabled = isEnabled;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // clear dgv selection
            dgv.ClearSelection();
            // check if already solved? If no, go ahead
            if (!solved)
            {
                lbStatus.Text = "";// clear 
                // no need the following               
                //Color_Theme();

                // Get the grid matrix
                List<int> matrix = GetGridFromUI();

                // check if the matrix is emtpy
                if (matrix.Any(x => x != 0))
                {                    
                    // Start the timer                    
                    timer.Start();
                    start_time = DateTime.Now;

                    // Disable the buttons
                    toggleBtnGroup( false);
                    dgv.Cursor = Cursors.WaitCursor;

                    isGridValid = true;
                    doSolver(matrix);
                }
                else
                {
                    showErrorInvoke(lbStatus, EmptyGridMsg, true);
                }
            }

        }

        private void CalRowAndColumnGrid()
        {            
            rows = row.Select(x => row.ConvertAll(y => y + x*9)).ToArray();
            columns = row.Select(x => row.ConvertAll(y => x + y*9)).ToArray();
            sqs = row.Select(x => row.ConvertAll(y => 9*(y/3)+ y%3 + 3*x +  (x/3) * 18)).ToArray();
            evens = row.Where((x, index) => index % 2 != 0).Select(x => row.ConvertAll(y => 9*(y/3)+ y%3 + 3*x +  (x/3) * 18)).ToList();  
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - start_time;
            lbTimer.Text = ts.Hours.ToString("D2") + ":" + ts.Minutes.ToString("D2") + ":" + ts.Seconds.ToString("D2");
           
            pBar.Value = (pBar.Value < pBar.Maximum) ? pBar.Value + 1 : 0;
            if (ts.Minutes >= time_limit)
            {
                stopRequested = true;
                pBar.Value = 0;
                timer.Stop();
                showErrorInvoke(lbStatus, GiveupMsg);                
            }            
        }


        private void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {            
            
            var entry = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (entry != null && !sKeys.Contains(entry.ToString()))
            {
                dgv.CurrentCell.Value = "";
            }
            else if (entry != null)
            {
                string err_str;
                string i = entry.ToString(); // new KeysConverter().ConvertToString(e.KeyCode);
                int _row = e.RowIndex; // dgv.CurrentCell.RowIndex;
                int _col = e.ColumnIndex; // dgv.CurrentCell.ColumnIndex;               

                bool b_row = nums.Any(a => a != _col && dgv[a, _row].Value != null && dgv[a, _row].Value.ToString() == i);
                bool b_col = nums.Any(a => a != _row && dgv[_col, a].Value != null && dgv[_col, a].Value.ToString() == i);

                int cur_region = cg.GetRegionFromNumber(_col * 9 + _row + 1);
                int[] region1_index = { 0, 1, 2, 9, 10, 11, 18, 19, 20 };
                int[] region_step = { 0, 3, 6, 27, 30, 33, 54, 57, 60 };

                int[] sq_index = region1_index.Zip(region_step, (x, y) => x + region_step[cur_region - 1]).ToArray();

                bool b_sq = sq_index.Any(a => a != _col * 9 + _row && dgv[a / 9, a % 9].Value != null
                    && dgv[a / 9, a % 9].Value.ToString() == i);

                if (b_row) err_str = "ROW";
                else if (b_col) err_str = "COL";
                else if (b_sq) err_str = "SQ";
                else err_str = "";

                if (b_row || b_col || b_sq)
                {                    
                    dgv.CurrentCell.Value = "";                    
                    string msg_conficts = "{" + i + "}" + " Conflicts in " + err_str;
                    showErrorInvoke(lbStatus, msg_conficts, true);               
                }
            }

        }

        //[DllImport("user32.dll")]
        //static extern bool HideCaret(IntPtr hWnd);


        private void btnReset_Click(object sender, EventArgs e)
        {
            SudokuClear();

            // stop working on sudoku                        
            stopRequested = timer.Enabled;

            if (stopRequested)
            {
                bw.CancelAsync();               
                timer.Stop();
            }

            LoadSudoku(list_from_UI == null? listGen : list_from_UI);

            Reset();
            toggleBtnGroup(true);
        }

        private void dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (!timer.Enabled)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    foreach (DataGridViewCell dg in dgv.SelectedCells)
                        dg.Value = null;
                }
                solved = false;
            }
        }

        private void SetLabel(Label label, string status)
        {
            if (label.InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    label.Text = status;
                });
            }
            else
                label.Text = status;
        }

        private void btnRan_Click(object sender, EventArgs e)
        {
            SudokuClear();
            // Generate a random full box
            List<Square> sqs = new List<Square>() { };
            //ClassGen cg = new ClassGen();
            sqs = cg.GenerateGrid();

            // form a list of values (81)
            sqs.ForEach(s => answer[s.Index] = s.Value.ToString());

            int index = cbLevel.SelectedIndex;
            GameLevel[] levels = {GameLevel.SIMPLE,GameLevel.MEDIUM,GameLevel.COMPLEX};
            Generate(levels[index]);
            LoadSudoku(listGen);

            // enable start button by reset
            Reset();

        }
        public void Generate(GameLevel level)
        {
            listGen  = new List<string>(new string[81]);
            // This first creates answer set by using Game combinations
            
            int minPos, maxPos, noOfSets;

            // Now unmask positions and create problem set.
            switch (level)
            {

                case GameLevel.SIMPLE:
                    minPos = 4;
                    maxPos = 6;
                    noOfSets = 8;
                    UnMask(minPos, maxPos, noOfSets);
                    break;
                case GameLevel.MEDIUM:
                    minPos = 3;
                    maxPos = 5;
                    noOfSets = 7;
                    UnMask(minPos, maxPos, noOfSets);
                    break;
                case GameLevel.COMPLEX:
                    minPos = 3;
                    maxPos = 5;
                    noOfSets = 6;
                    UnMask(minPos, maxPos, noOfSets);
                    break;
                default:
                    UnMask(3, 6, 7);
                    break;
            }
            // Make copy of Problem Set
            //
        }

        		/// <summary>
		/// Method:UnMask
		/// Purpose:UnMasks set positions randomly based on complexity.
		/// </summary>
		/// <param name="minPos"></param>
		/// <param name="maxPos"></param>
        private void UnMask(int minPos, int maxPos, int noOfSets)
        {
            int seed;
            int[] posX = { 0, 0, 0, 1, 1, 1, 2, 2, 2 };
            int[] posY = { 0, 1, 2, 0, 1, 2, 0, 1, 2 };
            int[] maskedSet = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Random number;
            int setCount = 0;
            do
            {

                seed = DateTime.Now.Millisecond;
                number = new Random(seed);
                int i = number.Next(0, 9);

                if (maskedSet[i] == 0)
                {
                    maskedSet[i] = 1;
                    setCount++;
                    // Mask each set

                    seed = DateTime.Now.Millisecond;
                    number = new Random(seed);
                    int maskPos = number.Next(minPos, maxPos);
                    int j = 0;
                    do
                    {
                        seed = DateTime.Now.Millisecond;
                        number = new Random(seed);
                        int newPos = number.Next(1, 9);
                        int x = _setRowPosition[i] + posX[newPos];
                        int y = _setColPosition[i] + posY[newPos];
                        if (listGen[x * 9 + y] == null)
                        {
                            listGen[x * 9 + y] = answer[x * 9 + y];
                            j++;
                        }

                    } while (j < maskPos);


                }
            } while (setCount < noOfSets);
        }

        private void LoadSudoku(List<string> matrixList)
        {
            if (matrixList != null)
            {
                SudokuClear();
                row.ForEach(x => row.ForEach(y => dgv.Rows[x].Cells[y].Value = matrixList[x * 9 + y]));
            }
            else
                showErrorInvoke(lbStatus, "Nothing to Load!", true);
        }


        private void btnAnswer_Click(object sender, EventArgs e)
        {
            if (answer != null)
            {
                LoadSudoku(answer);
            }
        }

        private void dgv_MouseDown(object sender, MouseEventArgs e)
        {
            dgv.CurrentCell.Selected = false;
        }

        private void dgv_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) // right click
            {
                if (Control.ModifierKeys == Keys.Control)
                    dgv[e.ColumnIndex, e.RowIndex].Value = answer[e.RowIndex * 9 + e.ColumnIndex];
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // save current question for recall later
            list_from_UI = new List<string>(new string[81]);
            
            for (int i = 0; i <= 80; i++)
            {
                if (dgv[i % 9, i / 9].Value != null)
                    list_from_UI[i] = dgv[i % 9, i / 9].Value.ToString();
            }
            if (list_from_UI.Any(x => x != null && sKeys.Contains(x)))
            {                
                showErrorInvoke(lbStatus, "Saved :-)", true);
            }

        }

        private void SudokuClear()
        {
            dgv.Rows.Cast<DataGridViewRow>().ToList().ForEach(row => row.Cells.Cast<DataGridViewCell>().ToList().ForEach(cell => cell.Value = null));
            dgv.ClearSelection();
            lbStatus.Text = ""; // clear status info
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            SudokuClear();
            Reset(); // reset status
        }

        private void warnTimer_Tick(object sender, EventArgs e)
        {
            showErrorInvoke(lbStatus, "");
            warnTimer.Enabled = false;
        }

    }
}
