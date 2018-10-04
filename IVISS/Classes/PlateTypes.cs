using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.Classes
{
    class PlateTypes
    {
        public static string ToOrigin(int origin)
        {
            origin = origin / 1000;

            switch (origin)
            {
                case 210:
                    return "Saudia";

                case 211:
                    return "Kuwait";

                case 212:
                    return "UAE";

                case 2120:
                    return "UAE"; //ARE_CD

                case 2121:
                    return "Dubai UAE";

                case 2122:
                    return "AbuDhabi UAE";

                case 2123:
                    return "Ajman UAE";

                case 2124:
                    return "Fujairah UAE";

                case 2125:
                    return "Sharjah UAE";

                case 2126:
                    return "UmmalQuwain UAE";

                case 2127:
                    return "RasAlkhaimah UAE";

                case 2128:
                    return "Unknown UAE";

                default:
                    return "Unknown";

            }
        }

        public static int ToAccuracy(string accuracy)
        {
            int acc = 0;

            try
            {
                acc = int.Parse(accuracy);
            }
            catch (Exception ex)
            {
                acc = 0;
            }



            if (acc <= 10)
                return 90;

            else if (acc <= 20)
                return 91;

            else if (acc <= 30)
                return 92;

            else if (acc <= 40)
                return 93;

            else if (acc <= 50)
                return 94;

            else if (acc <= 60)
                return 95;

            else if (acc <= 70)
                return 96;

            else if (acc <= 80)
                return 97;

            else if (acc <= 90)
                return 98;

            else
                return 99;

        }

    }
}
