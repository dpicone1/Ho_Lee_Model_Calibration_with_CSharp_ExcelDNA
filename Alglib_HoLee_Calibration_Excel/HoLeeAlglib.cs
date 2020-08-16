using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alglib_HoLee_Calibration_Excel
{
    public class HoLeeAlglib
    {
        private double spot;        // the current short term spot rate
        private double stdev;       // the volatility of current short term spot rate
        private double dt;          // time steps in the pricing integral. Page xx fo Veronesi

        private double[] S;         // time 
        private double[] F_star;    // array of theta rates to find  F*

        private Dictionary<double, double> DF_Curve;

        // Condition to match
        private int NConstrains;    // no of thetas to find

        public double[] THETA { get { return this.F_star; } }

        public HoLeeAlglib(Dictionary<double, double> DF_Curve, double[] S, double spot, double stdev, double dt)
        {
            this.stdev = stdev; this.spot = spot; this.dt = dt;

            this.DF_Curve = DF_Curve;
            this.S = S;
            this.NConstrains = S.Length;

            this.F_star = Enumerable.Repeat(0.01, this.DF_Curve.Count).ToArray<double>();
        }

        public void Go()
        {
         
            // setting up the optimiser
            double epsg = 0.0000000001;
            double epsf = 0;
            double epsx = 0;
            int maxits = 0;

            // setting up the optimiser
            alglib.minlmstate state;
            alglib.minlmreport rep;

            alglib.minlmcreatev(this.NConstrains, F_star, 0.000001, out state);
            alglib.minlmsetcond(state, epsg, epsf, epsx, maxits);
            alglib.minlmoptimize(state, function_fvec, null, null);
            alglib.minlmresults(state, out F_star, out rep);
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
