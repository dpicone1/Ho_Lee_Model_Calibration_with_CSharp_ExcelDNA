using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using ExcelDna.Integration;


namespace Alglib_HoLee_Calibration_Excel
{ 
    public static class Alglib_HoLee_Calibration_Excel //: IExcelAddIn
    {
        public static void calibrateAlglibVeronesi()
        {
            // create Excel application object
            dynamic Excel = ExcelDnaUtil.Application;
            try
            {
                //
                // read scalar parameters from named ranges
                double spot  = (double)Excel.Range["_r"].Value2;     // the current short term spot rate
                double stdev = (double)Excel.Range["_stdev"].Value2; // the volatility of current short term spot rate

                // read curve from named range
                dynamic Matrix = Excel.Range["_curve"].Value2;
                int rows = Matrix.GetUpperBound(0);

                double[] _S = new double[rows];


                // export curve data into dictionary
                Dictionary<double, double> DF_Curve = new Dictionary<double, double>();
                for (int i = 0; i < rows; i++)
                {
                    double key = (double)Matrix.GetValue(i + 1, 1);
                    double value = (double)Matrix.GetValue(i + 1, 2);
                    DF_Curve.Add(key, value);
                    _S[i] = key;
                }

                // Important for the integral
                double dt = 1.0 / 252.0;  // time steps in the pricing integral. Page xx fo Veronesi

                HoLeeAlglib hla = new HoLeeAlglib(DF_Curve, _S, spot, stdev, dt);
                hla.Go();
                double[] theta = hla.THETA;

                // export theta parameters into 2-dimensional array
                double[,] result = new double[rows, 1];
                for (int i = 0; i < rows; i++) result[i, 0] = theta[i];

                // Write into excel
                dynamic xlApp = ExcelDnaUtil.Application;
                // print array into named range
                xlApp.Range["_theta"] = result;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }
    }
}
