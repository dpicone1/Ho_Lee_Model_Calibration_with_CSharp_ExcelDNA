import numpy as np
import scipy.optimize as opt

# *****************************************************************************************************************************
# Class Zero Coupon Bond 
# *****************************************************************************************************************************

class ZCB(object):
    def __init__(self, _PVs):

        self.initialRates = np.array([0.006276])
        self.scalingFactor = 1000000.0


        self.PVs = [0.996400420164052, 0.983797731412662, 0.961105779236138,
                0.934389069590261, 0.905918834548805, 0.876871401656719, 0.847836610062567, 0.818860113067018,
                0.790529032144903, 0.762590288508597]

        self.PVs = _PVs

        self.zeroPricesHL = [{'type': 'eq', 'fun': self.ZeroCouponBondHL, 'args': [[self.PVs[0], self.scalingFactor, 1.0, 0]]}]        
        for i in range(len(self.PVs) - 1):
            self.zeroPricesHL.append({'type': 'eq', 'fun': self.ZeroCouponBondHL, 'args': [[self.PVs[i + 1], self.scalingFactor, i+2, i+1]]})

 
    def set_Spot_SD(self, Spot, SD):
        self.stdev = SD
        self.spot  = Spot
        self.dt    =  1.0 / 252.0


    def ObjectiveFunctionHoLee(self, x, args):                
        c2   = 1.0 / 6.0;
        size = len(self.zeroPricesHL)
        res  = 0
        for i in range(size):
            maturity = self.zeroPricesHL[i]['args'][0][2]           
            ZCP = self.HoLeeZCB(x, maturity)
            res += args[0]*((ZCP - self.zeroPricesHL[i]['args'][0][0])**2) 

        return res
    
    def HoLeeZCB(self, x, T):
                
        c2  = 1.0 / 6.0
        B   = T
        A   = -self.Integrale(x, T) * self.dt + c2 * (self.stdev ** 2) * (T ** 3)
        ZCP = np.exp(A - self.spot * B)
        
        return ZCP

    def Integrale(self, parameter, T):
        res     = 0.0;
        tempo   = self.dt;        
        counter = 0
        while (tempo < (T - self.dt)):
            
            if (self.zeroPricesHL[counter]['args'][0][2] < tempo):            
                counter+=1                
            res   += (T - tempo) * parameter[counter];
            tempo += self.dt;
        
        return res;

    # Constraint on the HoLee ZCB. The difference between given DF and calculated DF must be zero!
    def ZeroCouponBondHL(self, x, args):
        price = self.HoLeeZCB(x, args[2])
        # return difference between calculated and market price
        return  ((price - args[0])**2) * args[1]
    
# *****************************************************************************************************************************
# End
# *****************************************************************************************************************************
