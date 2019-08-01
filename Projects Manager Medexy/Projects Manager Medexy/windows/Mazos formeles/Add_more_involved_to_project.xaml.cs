using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projects_Manager_Medexy
{
    /// <summary>
    /// Interaction logic for Add_more_involved_to_project.xaml
    /// </summary>
    public partial class Add_more_involved_to_project : Window
    {
        public Add_more_involved_to_project()
        {
            InitializeComponent();

            int NumberOfColumns = 10;
            int NumberOfRows = 10;

            bool[,] Matrix = new bool[NumberOfRows, NumberOfColumns];

            Random rng  = new Random();

            for (int i = 0; i < NumberOfRows; i++)
            {
                for (int j = 0; j < NumberOfColumns; j++)
                {
                    if (rng.Next(0, 2) == 1)
                    {
                        Matrix[i,j] = true;
                    }
                    else
                    {
                        Matrix[i,j] = false;
                    }
                }
            }
        }
    }
}
