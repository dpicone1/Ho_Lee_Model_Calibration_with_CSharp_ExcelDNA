import numpy as np
import scipy.optimize as opt

import matplotlib.pyplot as pl
import ZeroCouponBond as Zrcb


PVs = [0.996400420164052, 0.983797731412662, 0.961105779236138,
                0.934389069590261, 0.905918834548805, 0.876871401656719, 0.847836610062567, 0.818860113067018,
                0.790529032144903, 0.762590288508597]

zcb  = Zrcb.ZCB(PVs);
size = len(zcb.zeroPricesHL)
    
# *****************************************************************************************************************************
# Optimization: we find the Ho Lee theta(t)
# *****************************************************************************************************************************
print('Optimization: finding Ho Lee theta(t)' ) 
zcb.set_Spot_SD(0.00154,0.00039)
initialGuess = np.full(size, 0.005)
model        = opt.minimize(zcb.ObjectiveFunctionHoLee, initialGuess, 
                            args = ([1000000.0]), 
                            method = 'SLSQP', 
                            constraints = zcb.zeroPricesHL)

# print selected model results
print('Success: ' + str(model.success))
print('Message: ' + str(model.message))
print('Number of iterations: ' + str(model.nit))
print('Objective function (sum of squared errors): ' + str(model.fun))
print('Changing variables (theta(t) rates): '        + str(model.x))
print()

# Print results vs Zero Coupon Price inputs
print('Maturities  Original Prices   Calculated Prices ')
for i in range(size):
    print(zcb.zeroPricesHL[i]['args'][0][2],"           ", \
        np.round(zcb.HoLeeZCB(model.x, zcb.zeroPricesHL[i]['args'][0][2]),6), \
        "         ", np.round(zcb.zeroPricesHL[i]['args'][0][0],6))
print()

# Plot theta(t)
pl.plot(model.x)
pl.show()