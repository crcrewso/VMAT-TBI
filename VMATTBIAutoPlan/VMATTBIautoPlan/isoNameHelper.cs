using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMATTBIautoPlan
{
    class isoNameHelper
    {
        public List<string> getIsoNames(int numVMATIsos, int numIsos)
        {
            List<string> isoNames = new List<string> { };
            switch (numIsos)
            {
                case 2:
                    isoNames.Add("Head");
                    isoNames.Add("Pelvis");
                    break;
                case 3:
                    isoNames.Add("Head");
                    isoNames.Add("Chest");
                    isoNames.Add("Legs");
                    break;
                case 4:
                    isoNames.Add("Head");
                    isoNames.Add("Chest");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs");
                    break;
                case 5:
                    isoNames.Add("Head");
                    isoNames.Add("Chest");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs_Sup");
                    isoNames.Add("Legs_Inf");
                    break;
                case 6:
                    isoNames.Add("Head");
                    isoNames.Add("Chest");
                    isoNames.Add("Pelvis");
                    isoNames.Add("Legs_Sup");
                    isoNames.Add("Legs_Inf");
                    isoNames.Add("Feet");
                    break;
            }
            return isoNames;
        }
    }
}
