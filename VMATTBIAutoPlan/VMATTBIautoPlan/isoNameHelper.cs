using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMATTBIautoPlan
{
    class IsoNameHelper
    {

        public List<string> GetIsoNames(int numVMATIsos)
        {
            return GetIsoNames(numVMATIsos, numVMATIsos);
        }



        public List<string> GetIsoNames(int numVMATIsos, int numIsos)
        {
            List<string> isoNames = new List<string> { };
            switch (numVMATIsos)
            {
                case 2:
                    isoNames.Add("Head");
                    isoNames.Add("Pelvis");
                    break;
                case 3:
                    isoNames.Add("Head");
                    isoNames.Add("Thorax");
                    isoNames.Add("Legs");
                    break;
                case 4:
                    isoNames.Add("Head");
                    isoNames.Add("Thorax");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs");
                    break;
                case 5:
                    isoNames.Add("Head");
                    isoNames.Add("Thorax");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs_Sup");
                    isoNames.Add("Legs_Inf");
                    break;
                case 6:
                    isoNames.Add("Head");
                    isoNames.Add("Thorax");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs_Sup");
                    isoNames.Add("Legs_Inf");
                    isoNames.Add("Feet");
                    break;
                case 7:
                    isoNames.Add("Head");
                    isoNames.Add("Thorax");
                    isoNames.Add("Abdomen");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs_Sup");
                    isoNames.Add("Legs_Inf");
                    isoNames.Add("Feet");
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
