using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardMaker.Card
{
    public static class MeasurementUtil
    {
        public static double GetInchesFromMillimeter(double dMillimeter)
        {
            return dMillimeter / 25.4d;
        }

        public static double GetInchesFromCentimeter(double dCentimeter)
        {
            return dCentimeter / 2.54d;
        }
    }
}
