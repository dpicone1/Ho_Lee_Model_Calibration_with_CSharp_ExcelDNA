using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverApp_Alglib_HoLee_Calibr_Veronesi_Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Alglib_HoLee_Calibration_Testing
    {
        class Program
        {
            static void Main(string[] args)
            {

                // Starting data
                double[] _S = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0 };
         
                double[] _DF_mktValue = new double[] { 0.996400420164052, 0.983797731412662, 0.961105779236138,
                0.934389069590261, 0.905918834548805, 0.876871401656719, 0.847836610062567, 0.818860113067018,
                0.790529032144903, 0.762590288508597 };

                Dictionary<double, double> DF_Curve = new Dictionary<double, double>(); // Maturity and DF
                for (int i = 0; i < _S.Length; i++)
                {
                    DF_Curve.Add(_S[i], _DF_mktValue[i]);
                }

                double spot  = 0.00154;      // the current short term spot rate
                double stdev = 0.00039;      // the volatility of current short term spot rate
                double dt    = 1.0 / 252.0;  // time steps in the pricing integral. Page xx fo Veronesi

                HoLeeAlglibTesting hla = new HoLeeAlglibTesting(DF_Curve, _S, spot, stdev, dt);
                hla.Go();
            }
        }

        public class HoLeeAlglibTesting
        {
            private double spot;        // the current short term spot rate
            private double stdev;       // the volatility of current short term spot rate
            private double dt;          // time steps in the pricing integral. Page xx fo Veronesi

            private double[] S;         // time 
            private double[] F_star;    // array of theta rates to find  F*

            private Dictionary<double, double> DF_Curve;

            // Condition to match
            private int NConstrains;    // no of thetas to find

            public HoLeeAlglibTesting(Dictionary<double, double> DF_Curve, double[] S, double spot, double stdev, double dt)
            {
                this.stdev = stdev; this.spot = spot; this.dt = dt;

                this.DF_Curve       = DF_Curve;
                this.S              = S;
                this.NConstrains    = S.Length;

                F_star = Enumerable.Repeat(0.01, this.DF_Curve.Count).ToArray<double>();
            }

            public void Go()
            {
                double[] testRates = Enumerable.Repeat(0.01, this.DF_Curve.Count).ToArray<double>();

                // setting up the optimiser
                double epsg   = 0.0000000001;
                double epsf   = 0;
                double epsx   = 0;
                int    maxits = 0;

                // Printing starting guess
                Console.WriteLine("Starting guess");
                int i1 = 0;
                foreach (var item in DF_Curve)
                {
                    double Mat        = item.Key;              // target maturity of df
                    double RecDf      = price(testRates, Mat); // recalculated DF
                    double DFmktValue = (double)DF_Curve[Mat]; // A dictionary: given a time point, give me the DF

                    Console.WriteLine(
                        "Maturity: {0}\t theta: {1:p3}\t DF: {2:f7}\t DF*: {3:f3}\t diff: {4:f2} ",
                        Mat,
                        F_star[i1],
                        RecDf,
                        DFmktValue,
                        (RecDf - DFmktValue) * 1000000
                        );
                    i1++;
                    //Console.WriteLine();
                }

                // setting up the optimiser
                alglib.minlmstate  state;
                alglib.minlmreport rep  ;

                alglib.minlmcreatev(this.NConstrains, F_star, 0.000001, out state);
                alglib.minlmsetcond(state, epsg, epsf, epsx, maxits);
                alglib.minlmoptimize(state, function_fvec, null, null);
                alglib.minlmresults(state, out F_star, out rep);

                // Print thetas
                Console.WriteLine();
                Console.WriteLine("Solution");

                for (int i = 0; i < F_star.Length; i++)
                {
                    //Console.WriteLine("Theta[{0},{1}] : {2:p4} \t", S[i] - 1, S[i], F_star[i]);
                    Console.WriteLine("Theta[{0},{1}] : {2:f10} \t", S[i] - 1, S[i], F_star[i]);
                }

                Console.WriteLine();
                Console.WriteLine("Check on DF"); // print check on df
                for (int i = 0; i < F_star.Length; i++)
                {
                    double Mat = S[i];                         // target maturity of df
                    double RecDf = price(F_star, Mat);         // recalculated DF
                    double DFmktValue = (double)DF_Curve[Mat]; // dictionary: given a time key, extract the value 

                    Console.WriteLine(
                        "Maturity: {0}\t Theta: {1:p3}\t DF: {2:f7}\t DF*: {3:f3}\t diff: {4:f2} ",
                        Mat,
                        F_star[i],
                        RecDf,
                        DFmktValue,
                        (RecDf - DFmktValue) * 1000000
                        );
                }
                Console.WriteLine();
                Console.WriteLine("The Original Inputs - our Dictionary");
                foreach (KeyValuePair<double, double> kvp in DF_Curve)
                {
                    Console.WriteLine("Maturity = {0}, DF = {1}", kvp.Key, kvp.Value);
                }
                Console.WriteLine();
            }

            public void function_fvec(double[] rates, double[] fi, object obj)
            {
                // minimise the difference
                for (int i = 0; i < fi.Length; i++)
                {
                    double value = (double)this.DF_Curve[this.S[i]];
                    fi[i] = 1000000 * (value - this.price(rates, this.S[i]));
                }
            }
            
            public double price(double[] parameter, double maturity)
            {
                int n = Array.IndexOf(this.S, maturity) + 1;
                var dest = parameter.Skip(0).Take(n).ToArray();

                // create term object for Ho-Lee bond price for a given eta and t
                double B = maturity;
                double c2 = 1.0 / 6.0;

                double A = -this.Integrale(dest, maturity) * this.dt + c2 * Math.Pow(this.stdev, 2) * Math.Pow(maturity, 3);

                return Math.Exp(A - this.spot * B);
            }

            private double Integrale(double[] parameter, double t)
            {
                double res = 0.0;
                double tempo = this.dt;
                double Mat = Convert.ToDouble(t);
                int counter = 0;
                while (tempo < (Mat - this.dt))
                {
                    if (this.S[counter] < tempo)
                    {
                        counter++;
                    }
                    res += (Mat - tempo) * parameter[counter];
                    tempo += this.dt;
                }
                return res;
            }
        }
    }
}
