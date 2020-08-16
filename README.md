# Ho_Lee_Model_Calibration_with_CSharp_ExcelDNA
Calibrating Ho and Lee short term interest rate model in C#, ExcelDNA and Alglib.

We previously covered the implementation of four interest rate models in Python in our Repository "Vasicek_CIR_HoLee_Hull_White_Models_Python".
In this repository, we use C# together with ExcelDNA to calibrate the Ho Lee model's "theta" on market data. The calibration algo is explained in Veronesi, Fixed Income Securities, Chapter 19, at pages 653-659. The non linear solver is from https://www.alglib.net/.

We found a similar work in https://mikejuniperhill.blogspot.com/2014/10 by Mikael Katajamäki with a different calibration algo from Veronesi. Mikael's work is in our view an excellent example on how to combine C# and ExcelDNA to solve a financial problem.

See also my Repository "MortgageLoanCsharpExcelDNA" to see another example of ExcelDNA and C#.
