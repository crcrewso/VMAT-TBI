using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.Types;

namespace VMATTBIautoPlan
{
    public class Settings
    {

        

        public Settings()
        {
            
        }

        public  string ConfigFile = "VMATTBIautoPlan.config";

        // General
        //flash option
        public  bool UseFlashByDefault = true;
        //default flash type is global
        public string DefaultFlashType = "Global";
        //default flash margin of 0.5 cm
        public string DefaultFlashMargin = "0.5";
        //default inner PTV margin from body of 0.3 cm
        public  string DefaultTargetMargin = "0.3";
        //option to contour overlap between VMAT fields in adjacent isocenters and default margin for contouring the overlap
        public bool ContourOverlap = true;
        public string ContourFieldOverlapMargin = "1.0";
        public double ContourOverlapMargin = 10.0;
        //point this to the directory holding the documentation files
        public readonly string DocumentationPath = @"S:\Physics";
        //default course ID
        public readonly string CourseId = "VMAT TBI";
        //assign technique as VMAT for all fields
        public readonly bool AllVMAT = true;
        //flag to see if user wants to check for potential couch collision (based on stanford experience)
        public readonly bool CheckTTCollision = false;
        //used to keep track of how many VMAT isocenters should be inferior to matchline
        public readonly int ExtraIsos = 0;
        //treatment units and associated photon beam energies
        public readonly List<string> Linacs = new List<string> { "TrueBeam1CC", "Mirage", "Solstice" };
        public readonly List<string> BeamEnergies = new List<string> { "6X", "10X" };
        //default number of beams per isocenter from head to toe
        public readonly int[] BeamsPerIso = { 3, 4, 4, 2, 2, 2, 2 };
        //collimator rotations for how to orient the beams (placeBeams class)
        public readonly double[] CollRot = { 3.0, 357.0, 87.0, 93.0 };
        //jaw positions of the placed VMAT beams
        public readonly List<VRect<double>> JawPos = new List<VRect<double>> {
            new VRect<double>(-20.0, -200.0, 200.0, 200.0),
            new VRect<double>(-200.0, -200.0, 20.0, 200.0),
            new VRect<double>(-200.0, -200.0, 0.0, 200.0),
            new VRect<double>(0.0, -200.0, 200.0, 200.0) };
        //photon beam calculation model
        public readonly string CalculationModel = "AAA_15605";
        //photon optimization algorithm
        public readonly string OptimizationModel = "PO_15605";
        //use GPU for dose calculation (not optimization)
        public readonly string UseGPUdose = "false";
        //use GPU for optimization
        public readonly string UseGPUoptimization = "false";
        //what MR level should the optimizer restart at following intermediate dose calculation
        public readonly string MRrestartLevel = "MR3";
        // TODO Add a setting for the MLC aperture shape controller (Moderate)





        // Sclero
        public double ScleroDosePerFx { get { return scleroDosePerFx; } }
        public int ScleroNumFx { get { return scleroNumFx; } }
        public List<Tuple<string, string, double, double, int>> OptConstDefaultSclero { get { return optConstDefaultSclero; } }

        // Myelo
        public double MyeloDosePerFx { get { return myeloDosePerFx; } }
        public int MyeloNumFx { get { return myeloNumFx; } }
        public List<Tuple<string, string, double, double, int>> OptConstDefaultMyelo { get { return optConstDefaultMyelo; } }

        // NonMyelo
        public double NonMyeloDosePerFx { get { return nonmyeloDosePerFx; } }
        public int NonMyeloNumFx { get { return nonmyeloNumFx; } }
        public List<Tuple<string, string, double, double, int>> OptConstDefaultNonMyelo { get { return optConstDefaultNonMyelo; } }


        // Tuning Structures
        public List<Tuple<string, string>> TS_Structures { get { return ts_structures; } }
        public List<Tuple<string, string>> ScleroStructures { get { return scleroStructures; } }

        // Optimization Constraints
        public List<Tuple<string, string, double>> DefaultSpareStruct { get { return defaultSpareStruct; } }
        public List<Tuple<string, string, double>> ScleroSpareStruct { get { return scleroSpareStruct; } }
        public List<Tuple<string, string, double>> MyeloSpareStruct { get { return myeloSpareStruct; } }
        public List<Tuple<string, string, double>> NonMyeloSpareStruct { get { return nonmyeloSpareStruct; } }




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
        readonly double myeloDosePerFx = 200;
        readonly int myeloNumFx = 6;
        readonly List<Tuple<string, string, double, double, int>> optConstDefaultMyelo =
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
        readonly double nonmyeloDosePerFx = 200;
        readonly int nonmyeloNumFx = 1;
        readonly List<Tuple<string, string, double, double, int>> optConstDefaultNonMyelo = new List<Tuple<string, string, double, double, int>>
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

        #region Tuning Structures

        //general tuning structures to be added (if selected for sparing) to all case types
        readonly List<Tuple<string, string>> ts_structures = new List<Tuple<string, string>>
        { Tuple.Create("CONTROL","Human_Body"),
          Tuple.Create("CONTROL","Lungs-1cm"),
          Tuple.Create("CONTROL","Lungs-2cm"),
          Tuple.Create("CONTROL","Liver-1cm"),
          Tuple.Create("CONTROL","Liver-2cm"),
          Tuple.Create("CONTROL","Kidneys-1cm"),
          Tuple.Create("CONTROL","Brain-0.5cm"),
          Tuple.Create("CONTROL","Brain-1cm"),
          Tuple.Create("CONTROL","Brain-2cm"),
          Tuple.Create("CONTROL","Brain-3cm"),
          Tuple.Create("PTV","PTV_Body"),
          Tuple.Create("CONTROL","TS_PTV_VMAT")
        };


        //scleroderma trial-specific tuning structures
        readonly List<Tuple<string, string>> scleroStructures = new List<Tuple<string, string>>
        {
            Tuple.Create("CONTROL","Lung_Block_L"),
            Tuple.Create("CONTROL","Lung_Block_R"),
            Tuple.Create("CONTROL","Lungs_Eval"),
            Tuple.Create("CONTROL","Kidney_Block_L"),
            Tuple.Create("CONTROL","Kidney_Block_R")
        };

        #endregion

        #region optimization constraints

        readonly List<Tuple<string, string, double>> defaultSpareStruct = new List<Tuple<string, string, double>>
        {
            new Tuple<string, string, double>("Lungs", "Mean Dose < Rx Dose", 0.3),
            new Tuple<string, string, double>("Kidneys", "Mean Dose < Rx Dose", 0.0),
            new Tuple<string, string, double>("Bowel", "Dmax ~ Rx Dose", 0.0)
        };

        readonly List<Tuple<string, string, double>> scleroSpareStruct = new List<Tuple<string, string, double>> { };

        readonly List<Tuple<string, string, double>> myeloSpareStruct = new List<Tuple<string, string, double>>
        {
            new Tuple<string, string, double>("Lenses", "Mean Dose < Rx Dose", 0.1),
        };

        readonly List<Tuple<string, string, double>> nonmyeloSpareStruct = new List<Tuple<string, string, double>>
        {
            new Tuple<string, string, double>("Ovaries", "Mean Dose < Rx Dose", 1.5),
            new Tuple<string, string, double>("Testes", "Mean Dose < Rx Dose", 2.0),
            new Tuple<string, string, double>("Brain", "Mean Dose < Rx Dose", -0.5),
            new Tuple<string, string, double>("Lenses", "Dmax ~ Rx Dose", 0.0),
            new Tuple<string, string, double>("Thyroid", "Mean Dose < Rx Dose", 0.0)
        };
        #endregion

    }
}
