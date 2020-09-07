import xlwings as xw
import numpy as np
import scipy.optimize as opt

import ZeroCouponBond as Zrcb

def Calibration_in_Python():

    # Create a reference to the calling Excel Workbook
    sht = xw.Book.caller().sheets[0]

    # Create an instance of class ZCB   
    zcb = Zrcb.ZCB(sht.range('_PV').value);
    size         = len(zcb.zeroPricesHL)
    zcb.set_Spot_SD(sht.range('_r').value,sht.range('_stdev').value) # Set the spot Rate and the SD

    # Optimization: we find the Ho Lee theta(t)     
    initialGuess = np.full(size, 0.005)
    model        = opt.minimize(zcb.ObjectiveFunctionHoLee, initialGuess, 
                                args = ([1000000.0]), 
                                method = 'SLSQP', 
                                constraints = zcb.zeroPricesHL)
 
    # read the calibration results
    seq =     model.x

    # Clear output
    sht.range('Theta01').expand('vertical').clear_contents()

    # Return the output to Excel in column orientation
    sht.range('Theta01').options(transpose=True).value = seq
