using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IVISS.Classes
{
    class DColor
    {
        public static string ToName(string hex)
        {
            switch (hex.ToUpper())
            {
                case "FFC0CB":
                    return "Pink";
                    break;

                case "000000":
                    return "Black";
                    break;

                case "FFFFFF":
                    return "White";
                    break;

                case "C0C0C0":
                    return "Silver";
                    break;

                case "800000":
                    return "Maroon";
                    break;

                case "0000FF":
                    return "Blue";
                    break;

                case "800080":
                    return "Purple";
                    break;

                case "FF00FF":
                    return "Fuchsia";
                    break;

                case "008000":
                    return "Green";
                    break;

                case "00FF00":
                    return "Lime";
                    break;

                case "808000":
                    return "Olive";
                    break;

                case "FFFF00":
                    return "Yellow";
                    break;

                case "000080":
                    return "Navy";
                    break;

                case "FF0000":
                    return "Red";
                    break;

                case "008080":
                    return "Teal";
                    break;

                case "00FFFF":
                    return "Aqua";
                    break;

                case "333333":
                case "666666":
                case "BEBEBE":
                case "999999":
                case "F0F0F0":
                case "F2F2F2":
                case "FAFAFA":
                case "F5F5F5":
                case "FCFCFC":
                case "F7F7F7":
                case "E5E5E5":
                case "FEFEFE":
                case "D3D3D3":
                case "D4D4D4":
                case "D6D6D6":
                case "E9E9E9":
                case "EDEDED":
                case "D9D9D9":
                case "CCCCCC":
                case "BFBFBF":
                case "ABABAB":
                case "7F7F7F":
                case "808080":
                    return "Grey";
                    break;
            }

            return string.Empty;
        }
    }
}
