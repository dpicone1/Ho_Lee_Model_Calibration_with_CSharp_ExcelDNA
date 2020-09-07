# Ho_Lee_Model_Calibration_with_CSharp_ExcelDNA_and_Python_xlwings
Calibrating Ho and Lee short term interest rate model in C#, ExcelDNA, Alglib, Python and xlwings.

In this repository, we use both C# and Python to calibrate Ho Lee model's "theta" on market data. The calibration algo is explained in Veronesi, Fixed Income Securities, Chapter 19, at pages 653-659. The non linear solvers are from https://www.alglib.net/ when working with C#, and scipy.optimize when working with Python. 

We previously covered the implementation of the Ho Lee model together with three additional interest rate models, Vasicek, CIR and Hull and White, in our Repository "Vasicek_CIR_HoLee_Hull_White_Models_Python".

With both C# and Python, we use an excel spreadsheet to enter market data and see calibration results. The communication between C# and excel occurs via "ExcelDNA" whereas when working in Python we used "xlwings". 

For more on ExcelDNA and C# see also my previous Repository "MortgageLoanCsharpExcelDNA". We also found a similar work in https://mikejuniperhill.blogspot.com/2014/10 by Mikael Katajamäki with a different calibration algo from Veronesi. Mikael's work is in our view an excellent example on how to combine C# and ExcelDNA to solve a financial problem.

For the xlwings we refer to https://www.xlwings.org/.
