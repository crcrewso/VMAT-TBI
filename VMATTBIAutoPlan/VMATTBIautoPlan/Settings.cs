using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMATTBIautoPlan
{
    public class Settings
    {
        // Sclero
        public double ScleroDosePerFx { get { return scleroDosePerFx; } }
        public int ScleroNumFx { get { return scleroNumFx; } }
        public List<Tuple<string, string, double, double, int>> OptConstDefaultSclero { get; }

        // Myelo
        public double MyeloDosePerFx { get { return myeloDosePerFx; } }
        public int MyeloNumFx { get { return myeloNumFx; } }
        public List<Tuple<string, string, double, double, int>> OptConstDefaultMyelo { get { return optConstDefaultMyelo; } }

        // NonMyelo
        public double NonMyeloDosePerFx { get { return nonmyeloDosePerFx; } }
        public int NonMyeloNumFx { get { return nonmyeloNumFx; } }
        public List<Tuple<string, string, double, double, int>> OptConstDefaultNonMyelo { get { return optConstDefaultNonMyelo; } }




        public Settings()
        {

        }

        #region sclero
        readonly double scleroDosePerFx = 200;
        readonly int scleroNumFx = 4;
        readonly List<Tuple<string, string, double, double, int>> optConstDefaultSclero = 
            new List<Tuple<string, string, double, double, int>>
            {
                new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Lower", 800.0, 100.0, 100),
                new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Upper", 808.0, 0.0, 100),
                new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Lower", 802.0, 98.0, 100),
                new Tuple<string, string, double, double, int>("Kidneys", "Mean", 100.0, 0.0, 80),
                new Tuple<string, string, double, double, int>("Kidneys-1cm", "Mean", 25.0, 0.0, 80),
                new Tuple<string, string, double, double, int>("Lungs", "Mean", 150.0, 0.0, 80),
                new Tuple<string, string, double, double, int>("Lungs-1cm", "Mean", 100.0, 0.0, 80),
                new Tuple<string, string, double, double, int>("Lungs-2cm", "Mean", 50.0, 0.0, 80),
                new Tuple<string, string, double, double, int>("Bowel", "Upper", 850.0, 0.0, 50)
            };

        #endregion

        #region myelo
        double myeloDosePerFx = 200;
        int myeloNumFx = 6;
        List<Tuple<string, string, double, double, int>> optConstDefaultMyelo =
            new List<Tuple<string, string, double, double, int>>
            {
                new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Lower", 1200.0, 100.0, 100),
                new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Upper", 1212.0, 0.0, 100),
                new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Lower", 1202.0, 98.0, 100),
                new Tuple<string, string, double, double, int>("Kidneys", "Mean", 750, 0.0, 80),
                new Tuple<string, string, double, double, int>("Kidneys-1cm", "Mean", 400.0, 0.0, 50),
                new Tuple<string, string, double, double, int>("Lenses", "Upper", 1140, 0.0, 50),
                new Tuple<string, string, double, double, int>("Lungs", "Mean", 600.0, 0.0, 90),
                new Tuple<string, string, double, double, int>("Lungs-1cm", "Mean", 300.0, 0.0, 80),
                new Tuple<string, string, double, double, int>("Lungs-2cm", "Mean", 200.0, 0.0, 70),
                new Tuple<string, string, double, double, int>("Bowel", "Upper", 1205.0, 0.0, 50)
            };
        #endregion

        #region nonmyleo
        double nonmyeloDosePerFx = 200;
        int nonmyeloNumFx = 1;
        List<Tuple<string, string, double, double, int>> optConstDefaultNonMyelo = new List<Tuple<string, string, double, double, int>>
        {
            new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Lower", 200.0, 100.0, 100),
            new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Upper", 202.0, 0.0, 100),
            new Tuple<string, string, double, double, int>("TS_PTV_VMAT", "Lower", 201.0, 98.0, 100),
            new Tuple<string, string, double, double, int>("Kidneys", "Mean", 120.0, 0.0, 80),
            new Tuple<string, string, double, double, int>("Kidneys-1cm", "Mean", 75.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Lungs", "Mean", 75.0, 0.0, 90),
            new Tuple<string, string, double, double, int>("Lungs-1cm", "Mean", 50.0, 0.0, 80),
            new Tuple<string, string, double, double, int>("Lungs-2cm", "Mean", 25.0, 0.0, 70),
            new Tuple<string, string, double, double, int>("Ovaries", "Mean", 50.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Ovaries", "Upper", 75.0, 0.0, 70),
            new Tuple<string, string, double, double, int>("Testes", "Mean", 50.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Testes", "Upper", 75.0, 0.0, 70),
            new Tuple<string, string, double, double, int>("Lenses", "Upper", 190.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Brain", "Mean", 150.0, 0.0, 60),
            new Tuple<string, string, double, double, int>("Brain-1cm", "Mean", 100.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Brain-2cm", "Mean", 75.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Brain-3cm", "Mean", 50.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Bowel", "Upper", 201.0, 0.0, 50),
            new Tuple<string, string, double, double, int>("Thyroid", "Mean", 100.0, 0.0, 50)
        };


        #endregion



    }
}
