using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VMATTBIautoPlan
{
    public static class IsoNameHelper
    {

        static readonly string[] isoNames = new string[] { "Head", "Thorax", "Abdomen", "Pelvis", "Legs", "Legs_Sup", "Legs_Inf", "Feet" };

        static readonly string[] top = isoNames.Take(4).ToArray();
        static readonly string[] bottom = isoNames.Skip(4).ToArray();

        public static bool IsTop(string isoName)
        {
            return top.Contains(isoName);
        }

        /// <summary>
        /// Returns the DICOM label of the orientation of the isocenter based on the isoName
        /// </summary>
        /// <param name="isoName"></param>
        /// <returns></returns>
        public static string Orientation(string isoName)
        {
            if (IsTop(isoName))
                return "Head First-Supine";
            else if ( IsBottom(isoName) )
            {
                return "Feet First-Supine";
            }
            return "Unknown";
        }

        public static bool IsValidIsoName(string isoName)
        {
            if( isoNames.Contains(isoName)) return true;
            return false;
        }



        public static bool IsBottom(string isoName)
        {
            return bottom.Contains(isoName);
        }



        public static List<string> GetIsoNames(int numVMATIsos) => GetIsoNames(numVMATIsos, numVMATIsos);


        public static List<string> GetIsoNames(int numVMATIsos, int numIsos)
        {
            List<string> isoNames = new List<string> { };
            switch (numVMATIsos)
            {
                case 2:
                    isoNames.Add(isoNames[0]);
                    isoNames.Add(isoNames[3]);
                    break;
                case 3:
                    isoNames.Add(isoNames[0]);
                    isoNames.Add(isoNames[3]);
                    isoNames.Add(isoNames[4]);
                    break;
                case 4:
                    isoNames.Add(isoNames[0]);
                    isoNames.Add(isoNames[1]);
                    isoNames.Add(isoNames[3]);
                    isoNames.Add(isoNames[4]);
                    break;
                case 5:
                    isoNames.Add(isoNames[0]);
                    isoNames.Add(isoNames[1]);
                    isoNames.Add(isoNames[3]);
                    isoNames.Add(isoNames[5]);
                    isoNames.Add(isoNames[6]);
                    break;
                case 6:
                    isoNames.Add(isoNames[0]);
                    isoNames.Add(isoNames[1]);
                    isoNames.Add(isoNames[3]);
                    isoNames.Add(isoNames[5]);
                    isoNames.Add(isoNames[6]);
                    isoNames.Add(isoNames[7]);
                    break;
                case 7:
                    isoNames.Add(isoNames[0]);
                    isoNames.Add(isoNames[1]);
                    isoNames.Add(isoNames[2]);
                    isoNames.Add(isoNames[3]);
                    isoNames.Add(isoNames[5]);
                    isoNames.Add(isoNames[6]);
                    isoNames.Add(isoNames[7]);
                    break;
                default:
                    for (int i = 0; i < numIsos; i++)
                        isoNames.Add("Iso " + i);
                    break;
            }
            return isoNames;
        }
    }
}
