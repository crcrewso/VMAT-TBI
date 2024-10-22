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

        static readonly string[] hfs = isoNames.Take(4).ToArray();
        static readonly string[] ffs = isoNames.Skip(4).ToArray();

        public static bool IsHFS(string isoName)
        {
            return hfs.Contains(isoName);
        }

        public static bool IsHFSLong(string isoName)
        {
            for (int i = 0; i < hfs.Length; i++)
            {
                if (isoName.Contains(hfs[i]))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// The treatment order for HFS is Head, Thorax, Abdomen, Pelvis
        /// </summary>
        public static string[] HFSorder => hfs;

        /// <summary>
        /// The treatment order for FFS is feet, legs_inf, legs_sup, or legs
        /// </summary>
        public static string[] FFSorder => ffs.Reverse().ToArray();



        /// <summary>
        /// Returns the DICOM label of the orientation of the isocenter based on the isoName
        /// </summary>
        /// <param name="isoName"></param>
        /// <returns></returns>
        public static string Orientation(string isoName)
        {
            if (IsHFS(isoName))
                return "Head First-Supine";
            else if ( IsFFS(isoName) )
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



        public static bool IsFFS(string isoName)
        {
            return ffs.Contains(isoName);
        }

        // TODO Get rid of the 2 parameter version
        // TODO Move from List to array. 

        public static List<string> GetIsoNames(int numVMATIsos) => GetIsoNames(numVMATIsos, numVMATIsos);


        public static List<string> GetIsoNames(int numVMATIsos, int numIsos)
        {
            switch (numVMATIsos)
            {
                case 2:
                    return new List<string> { 
                        isoNames[0], 
                        isoNames[3] 
                    };                    
                case 3:
                    return new List<string> { 
                        isoNames[0], 
                        isoNames[3], 
                        isoNames[4] 
                    };
                case 4:
                    return new List<string> { 
                        isoNames[0], 
                        isoNames[1], 
                        isoNames[3], 
                        isoNames[4] 
                    };
                case 5:
                    return new List<string> { 
                        isoNames[0], 
                        isoNames[1], 
                        isoNames[3], 
                        isoNames[5], 
                        isoNames[6] 
                    };
                case 6:
                    return new List<string> { 
                        isoNames[0], 
                        isoNames[1], 
                        isoNames[3], 
                        isoNames[5], 
                        isoNames[6], 
                        isoNames[7] 
                    };
                case 7:
                    return new List<string> { 
                        isoNames[0], 
                        isoNames[1], 
                        isoNames[2], 
                        isoNames[3], 
                        isoNames[5], 
                        isoNames[6], 
                        isoNames[7] 
                    };

                default:
                    List<string> ret = new List<string> { };
                    for (int i = 0; i < numIsos; i++)
                        ret.Add("Iso " + i);
                    return ret;
            }
        }
    }
}
