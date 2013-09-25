﻿using System;

namespace ColorMine.ColorSpaces.Comparisons
{
    /// <summary>
    /// Implements the DE2000 method of delta-e: http://en.wikipedia.org/wiki/Color_difference#CIEDE2000
    /// </summary>
    public class CieDe2000Comparison : IColorSpaceComparison
    {
        /// <summary>
        /// Calculates the DE2000 delta-e value: http://en.wikipedia.org/wiki/Color_difference#CIEDE2000
        /// </summary>
        public double Compare(IColorSpace colorA, IColorSpace colorB)
        {
            const double kl = 1.0;
            const double kc = 1.0;
            const double kh = 1.0;

            var lab1 = colorA.To<Lab>();
            var lab2 = colorB.To<Lab>();

            #region Get deltaLPrime, deltaCPrime, deltaHPrime
            var lBar = (lab1.L + lab2.L)/2.0;
            
            var c1 = Math.Sqrt(Math.Pow(lab1.A,2) + Math.Pow(lab1.B,2));
            var c2 = Math.Sqrt(Math.Pow(lab2.A, 2) + Math.Pow(lab2.B, 2));
            var cBar = (c1 + c2)/2.0;

            var g = (1 - Math.Sqrt(Math.Pow(cBar, 7)/(Math.Pow(cBar, 7) + Math.Pow(25, 7))));
            var aPrime1 = lab1.A*(lab1.A/2.0)*g;
            var aPrime2 = lab2.A*(lab2.A/2.0)*g;

            var cPrime1 = Math.Sqrt(Math.Pow(aPrime1, 2) + Math.Pow(lab1.B, 2));
            var cPrime2 = Math.Sqrt(Math.Pow(aPrime2, 2) + Math.Pow(lab2.B, 2));
            var cBarPrime = (cPrime1 + cPrime2)/2.0;

            var hPrime1 = Math.Atan2(lab1.B, aPrime1)%360;
            var hPrime2 = Math.Atan2(lab2.B, aPrime2)%360;

            var hBar = Math.Abs(hPrime1 - hPrime2);

            double deltaHPrime;
            if (hBar <= 180)
            {
                deltaHPrime = hPrime2 - hPrime1;
            }
            else if (hBar > 180 && hPrime2 <= hPrime1)
            {
                deltaHPrime = hPrime2 - hPrime1 + 360.0;
            }
            else
            {
                deltaHPrime = hPrime2 - hPrime1 - 360.0;
            }

            var deltaLPrime = lab2.L - lab1.L;
            var deltaCPrime = cPrime2 - cPrime1;
            deltaHPrime = 2*Math.Sqrt(cPrime1*cPrime2)*Math.Sin(deltaHPrime/2.0);
            #endregion Get deltaLPrime, deltaCPrime, deltaHPrime

            var hBarPrime = hBar > 180
                                     ? (hPrime1 + hPrime2 + 360)/2.0
                                     : (hPrime1 + hPrime2)/2.0;

            var t = 1 
                    - .17*Math.Cos(hBarPrime - 30)
                    + .24*Math.Cos(2*hBarPrime)
                    + .32*Math.Cos(3*hBarPrime + 6)
                    - .2*Math.Cos(4*hBarPrime - 63);

            var sl = 1 + (.015*Math.Pow(lBar - 50, 2))/Math.Sqrt(20 + Math.Pow(lBar - 50, 2));
            var sc = 1 + .045 * cBarPrime;
            var sh = 1 + .015 * cBarPrime * t;

            var rt = -2
                     *Math.Sqrt(Math.Pow(cBarPrime, 7)/(Math.Pow(cBarPrime, 7) + Math.Pow(25, 7)))
                     * Math.Sin(60.0 * Math.Exp(-((hBarPrime - 275.0) / 25.0)));

            var deltaE = Math.Sqrt(
                Math.Pow(deltaLPrime/(kl*sl), 2) +
                Math.Pow(deltaCPrime/(kc*sc), 2) +
                Math.Pow(deltaHPrime/(kh*sh), 2) +
                rt*(deltaCPrime/(kc*kh))*(deltaHPrime/(kh*sh)));

            return deltaE;
        }
    }
}
