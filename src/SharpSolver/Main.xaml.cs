using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
//using DAnTE.ExtraControls;

//using System.Windows.Media;
//using System.Windows.Forms;

namespace SharpSolver
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        // The most difficult sudoku ever :))
        List<int> rawGrid = new List<int>()
        {8, 0, 0, 0, 0, 0, 0, 0, 0,
         0, 0, 3, 6, 0, 0, 0, 0, 0,
         0, 7, 0, 0, 9, 0, 2, 0, 0,
         0, 5, 0, 0, 0, 7, 0, 0, 0,
         0, 0, 0, 0, 4, 5, 7, 0, 0,
         0, 0, 0, 1, 0, 0, 0, 3, 0,
         0, 0, 1, 0, 0, 0, 0, 6, 8,
         0, 0, 8, 5, 0, 0, 0, 1, 0,
         0, 9, 0, 0, 0, 0, 4, 0, 0,};

        DateTime start_time;
        public DataTable dt { get; set; }
        //public DataView DataView { get; set; }
        int rowIndex, ColIndex;
        private DataColumn Acolumn;
        public DataRow Arow;
        //DataGridView dgv = null;
        private ContainerVisual _containerVisual;


        public Main()
        {
            InitializeComponent();

            dt = new DataTable();
            DataContext = dt;
            dGrid.DataContext = dt;
            InitializeDataGridView(9, 9);
            //dGrid.Items.Add(new DataGridRow());
            //dGrid.Columns.Add(new DataGridTextColumn());
            //solveIt(rawGrid);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //int index = 0;
            //TextBlock tb = null;


            //this.Controls.Add(dgv);

            //dgv.BeginEdit(false);

            //lbTimer.Text = @"00:00:00";

        }

        private void InitializeDataGridView(int rows, int columns)
        {
        
            // Add Rows
            for (int i = 0; i < rows; i++)
            {
                //dt.Rows.Add();//i.ToString());
            }

            //Add Columns

            DataGridTextColumn textColumn;
            for (int i = 0; i < columns; i++)
            {
                textColumn = new DataGridTextColumn();
                textColumn.Header = "6";
                
                dGrid.Columns.Add(textColumn);
            }

            // Preload matrix


            // Setting Data to Grid Cell
            if (GetCell(3,1).Content is TextBlock) // if grid cell is not editable
            {
                ((TextBlock)(GetCell(3,1).Content)).Text = "sometext";
            }
            else // TextBox  - if grid cell is editable
            {
                ((TextBox)(GetCell(3,1).Content)).Text = "sometext";
            }

            int index = 0;
            for (int i = 0; i < 9; i++)
            {
                //DataGridRow row = (DataGridRow)dGrid.ItemContainerGenerator.ContainerFromIndex(i);
                for (int j = 0; j < 9; j++)
                {
                    //DataGridCell cell = (DataGridCell)row.Item.
                    //cell = dt.Rows[i][j] as DataGridCell;
                    //cell.valurawGrid[index];
                    
                    //dt.Rows[i][j] = rawGrid[index];
                    index++;
                }
            }
        }

        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public DataGridCell GetCell(int columnIndex, int rowIndex)
        {
            DataGridRow rowContainer = GetRow(rowIndex);
            

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                DataGridColumn Column = (DataGridColumn)dGrid.ItemContainerGenerator.ContainerFromIndex(columnIndex);

                // Try to get the cell but it may possibly be virtualized.
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell == null)
                {
                    // Now try to bring into view and retreive the cell.
                    dGrid.ScrollIntoView(rowContainer, Column);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
                return cell;
            }
            return null;
        }

        public DataGridRow GetRow(int rowIndex)
        {
            DataGridRow row = (DataGridRow)dGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
            return row;
        }


    }
}
