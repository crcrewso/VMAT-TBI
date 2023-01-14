using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using VMATTBIautoPlan.Properties;

namespace VMATTBIautoPlan
{




    internal class parseSettings
    {
        #region parameters
        string configFile = "";
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// HARD-CODED MAIN PARAMETERS FOR THIS CLASS AND ALL OTHER CLASSES IN THIS DLL APPLICATION.
        /// ADJUST THESE PARAMETERS TO YOUR TASTE. THESE PARAMETERS WILL BE OVERWRITTEN BY THE CONFIG.INI FILE IF IT IS SUPPLIED.
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        double scleroDosePerFx = 200;
        int scleroNumFx = 4;
        //structure, constraint type, dose cGy, volume %, priority
        List<Tuple<string, string, double, double, int>> optConstDefaultSclero = new List<Tuple<string, string, double, double, int>>
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
        double myeloDosePerFx = 200;
        int myeloNumFx = 6;
        List<Tuple<string, string, double, double, int>> optConstDefaultMyelo = new List<Tuple<string, string, double, double, int>>
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

        //general tuning structures to be added (if selected for sparing) to all case types
        List<Tuple<string, string>> TS_structures = new List<Tuple<string, string>>
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
        List<Tuple<string, string>> scleroStructures = new List<Tuple<string, string>>
        {
            Tuple.Create("CONTROL","Lung_Block_L"),
            Tuple.Create("CONTROL","Lung_Block_R"),
            Tuple.Create("CONTROL","Lungs_Eval"),
            Tuple.Create("CONTROL","Kidney_Block_L"),
            Tuple.Create("CONTROL","Kidney_Block_R")
        };

        List<Tuple<string, string, double>> defaultSpareStruct = new List<Tuple<string, string, double>>
        {
            new Tuple<string, string, double>("Lungs", "Mean Dose < Rx Dose", 0.3),
            new Tuple<string, string, double>("Kidneys", "Mean Dose < Rx Dose", 0.0),
            new Tuple<string, string, double>("Bowel", "Dmax ~ Rx Dose", 0.0)
        };

        List<Tuple<string, string, double>> scleroSpareStruct = new List<Tuple<string, string, double>> { };

        List<Tuple<string, string, double>> myeloSpareStruct = new List<Tuple<string, string, double>>
        {
            new Tuple<string, string, double>("Lenses", "Mean Dose < Rx Dose", 0.1),
        };

        List<Tuple<string, string, double>> nonmyeloSpareStruct = new List<Tuple<string, string, double>>
        {
            new Tuple<string, string, double>("Ovaries", "Mean Dose < Rx Dose", 1.5),
            new Tuple<string, string, double>("Testes", "Mean Dose < Rx Dose", 2.0),
            new Tuple<string, string, double>("Brain", "Mean Dose < Rx Dose", -0.5),
            new Tuple<string, string, double>("Lenses", "Dmax ~ Rx Dose", 0.0),
            new Tuple<string, string, double>("Thyroid", "Mean Dose < Rx Dose", 0.0)
        };

        //flash option
        bool useFlashByDefault = true;
        //default flash type is global
        string defaultFlashType = "Global";
        //default flash margin of 0.5 cm
        string defaultFlashMargin = "0.5";
        //default inner PTV margin from body of 0.3 cm
        string defaultTargetMargin = "0.3";
        //option to contour overlap between VMAT fields in adjacent isocenters and default margin for contouring the overlap
        bool contourOverlap = true;
        string contourFieldOverlapMargin = "1.0";
        //point this to the directory holding the documentation files
        string documentationPath = @"\\enterprise.stanfordmed.org\depts\RadiationTherapy\Public\Users\ESimiele\Research\VMAT_TBI\documentation\";
        //default course ID
        string courseId = "VMAT TBI";
        //assign technique as VMAT for all fields
        bool allVMAT = false;
        //flag to see if user wants to check for potential couch collision (based on stanford experience)
        bool checkTTCollision = false;
        //used to keep track of how many VMAT isocenters should be inferior to matchline
        int extraIsos = 0;
        //treatment units and associated photon beam energies
        List<string> linacs = new List<string> { "LA16", "LA17" };
        List<string> beamEnergies = new List<string> { "6X", "10X" };
        //default number of beams per isocenter from head to toe
        int[] beamsPerIso = { 4, 3, 2, 2, 2, 2 };
        //collimator rotations for how to orient the beams (placeBeams class)
        double[] collRot = { 3.0, 357.0, 87.0, 93.0 };
        //jaw positions of the placed VMAT beams
        List<VRect<double>> jawPos = new List<VRect<double>> {
            new VRect<double>(-20.0, -200.0, 200.0, 200.0),
            new VRect<double>(-200.0, -200.0, 20.0, 200.0),
            new VRect<double>(-200.0, -200.0, 0.0, 200.0),
            new VRect<double>(0.0, -200.0, 200.0, 200.0) };
        //photon beam calculation model
        string calculationModel = "AAA_15605";
        //photon optimization algorithm
        string optimizationModel = "PO_15605";
        //use GPU for dose calculation (not optimization)
        string useGPUdose = "false";
        //use GPU for optimization
        string useGPUoptimization = "false";
        //what MR level should the optimizer restart at following intermediate dose calculation
        string MRrestartLevel = "MR3";
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        #endregion

        private string cropLine(string line, string cropChar) { return line.Substring(line.IndexOf(cropChar) + 1, line.Length - line.IndexOf(cropChar) - 1); }

        private Tuple<string, string> parseTS(string line)
        {
            //known array format --> can take shortcuts in parsing the data
            //structure id, sparing type, added margin in cm (ignored if sparing type is Dmax ~ Rx Dose)
            string dicomType = "";
            string TSstructure = "";
            line = cropLine(line, "{");
            dicomType = line.Substring(0, line.IndexOf(","));
            line = cropLine(line, ",");
            TSstructure = line.Substring(0, line.IndexOf("}"));
            return Tuple.Create(dicomType, TSstructure);
        }

        private Tuple<string, string, double> parseSparingStructure(string line)
        {
            //known array format --> can take shortcuts in parsing the data
            //structure id, sparing type, added margin in cm (ignored if sparing type is Dmax ~ Rx Dose)
            string structure = "";
            string spareType = "";
            double val = 0.0;
            line = cropLine(line, "{");
            structure = line.Substring(0, line.IndexOf(","));
            line = cropLine(line, ",");
            spareType = line.Substring(0, line.IndexOf(","));
            line = cropLine(line, ",");
            val = double.Parse(line.Substring(0, line.IndexOf("}")));
            return Tuple.Create(structure, spareType, val);
        }

        private Tuple<string, string, double, double, int> parseOptimizationConstraint(string line)
        {
            //known array format --> can take shortcuts in parsing the data
            //structure id, constraint type, dose (cGy), volume (%), priority
            string structure = "";
            string constraintType = "";
            double doseVal = 0.0;
            double volumeVal = 0.0;
            int priorityVal = 0;
            line = cropLine(line, "{");
            structure = line.Substring(0, line.IndexOf(","));
            line = cropLine(line, ",");
            constraintType = line.Substring(0, line.IndexOf(","));
            line = cropLine(line, ",");
            doseVal = double.Parse(line.Substring(0, line.IndexOf(",")));
            line = cropLine(line, ",");
            volumeVal = double.Parse(line.Substring(0, line.IndexOf(",")));
            line = cropLine(line, ",");
            priorityVal = int.Parse(line.Substring(0, line.IndexOf("}")));
            return Tuple.Create(structure, constraintType, doseVal, volumeVal, priorityVal);
        }

        /// <summary>
        /// Parses and loads config file 
        /// </summary>
        /// <param name="file">full path to file being loaded </param>
        /// <returns> false iff load completed properly</returns>
        private bool loadConfigurationSettings(string file)
        {
            configFile = file;
            //encapsulate everything in a try-catch statment so I can be a bit lazier about data checking of the configuration settings (i.e., if a parameter or value is bad the script won't crash)
            try
            {
                using (StreamReader reader = new StreamReader(configFile))
                {
                    //setup temporary vectors to hold the parsed data
                    string line;
                    List<string> linac_temp = new List<string> { };
                    List<string> energy_temp = new List<string> { };
                    List<VRect<double>> jawPos_temp = new List<VRect<double>> { };
                    List<Tuple<string, string, double>> defaultSpareStruct_temp = new List<Tuple<string, string, double>> { };
                    List<Tuple<string, string>> TSstructures_temp = new List<Tuple<string, string>> { };
                    List<Tuple<string, string>> scleroTSstructures_temp = new List<Tuple<string, string>> { };
                    List<Tuple<string, string, double, double, int>> optConstDefaultSclero_temp = new List<Tuple<string, string, double, double, int>> { };
                    List<Tuple<string, string, double, double, int>> optConstDefaultMyelo_temp = new List<Tuple<string, string, double, double, int>> { };
                    List<Tuple<string, string, double, double, int>> optConstDefaultNonMyelo_temp = new List<Tuple<string, string, double, double, int>> { };
                    List<Tuple<string, string, double>> scleroSpareStruct_temp = new List<Tuple<string, string, double>> { };
                    List<Tuple<string, string, double>> myeloSpareStruct_temp = new List<Tuple<string, string, double>> { };
                    List<Tuple<string, string, double>> nonmyeloSpareStruct_temp = new List<Tuple<string, string, double>> { };

                    while ((line = reader.ReadLine()) != null)
                    {
                        //start actually reading the data when you find the begin plugin configuration tag
                        if (line.Equals(":begin plugin configuration:"))
                        {
                            while (!(line = reader.ReadLine()).Equals(":end plugin configuration:"))
                            {
                                //this line contains useful information (i.e., it is not a comment)
                                if (!string.IsNullOrEmpty(line) && line.Substring(0, 1) != "%")
                                {
                                    //useful info on this line in the format of parameter=value
                                    //parse parameter and value separately using '=' as the delimeter
                                    if (line.Contains("="))
                                    {
                                        //default configuration parameters
                                        string parameter = line.Substring(0, line.IndexOf("="));
                                        string value = line.Substring(line.IndexOf("=") + 1, line.Length - line.IndexOf("=") - 1);
                                        //check if it's a double value
                                        if (double.TryParse(value, out double result))
                                        {
                                            if (parameter == "default flash margin")
                                            {
                                                defaultFlashMargin = result.ToString();
                                                Settings.Default.defalutFlashMargin = result.ToString();
                                            }
                                            else if (parameter == "default target margin")
                                            {
                                                defaultTargetMargin = result.ToString();
                                                Settings.Default.defaultTargetMargin = result.ToString();
                                            }
                                            else if (parameter == "contour field overlap margin")
                                            {
                                                contourFieldOverlapMargin = result.ToString();
                                                Settings.Default.contourFieldOverlapMargin = result.ToString();
                                            }
                                        }
                                        else if (parameter == "documentation path")
                                        {
                                            documentationPath = value;
                                            if (documentationPath.LastIndexOf("\\") != documentationPath.Length - 1) documentationPath += "\\";
                                            Settings.Default.documentationPath = documentationPath;
                                        }
                                        else if (parameter == "beams per iso")
                                        {
                                            //parse the default requested number of beams per isocenter
                                            line = cropLine(line, "{");
                                            List<int> b = new List<int> { };
                                            //second character should not be the end brace (indicates the last element in the array)
                                            while (line.Substring(1, 1) != "}")
                                            {
                                                b.Add(int.Parse(line.Substring(0, line.IndexOf(","))));
                                                line = cropLine(line, ",");
                                            }
                                            b.Add(int.Parse(line.Substring(0, line.IndexOf("}"))));
                                            //only override the requested number of beams in the beamsPerIso array  
                                            for (int i = 0; i < b.Count(); i++) { if (i < beamsPerIso.Count()) beamsPerIso[i] = b.ElementAt(i); }
                                        }
                                        else if (parameter == "collimator rotations")
                                        {
                                            //parse the default requested number of beams per isocenter
                                            line = cropLine(line, "{");
                                            List<double> c = new List<double> { };
                                            //second character should not be the end brace (indicates the last element in the array)
                                            while (line.Contains(","))
                                            {
                                                c.Add(double.Parse(line.Substring(0, line.IndexOf(","))));
                                                line = cropLine(line, ",");
                                            }
                                            c.Add(double.Parse(line.Substring(0, line.IndexOf("}"))));
                                            for (int i = 0; i < c.Count(); i++) { if (i < 5) collRot[i] = c.ElementAt(i); }
                                        }
                                        else if (parameter == "course Id")
                                        {
                                            courseId = value;
                                            Settings.Default.courseId = value;
                                        }
                                        else if (parameter == "all fields VMAT")
                                        {
                                            if (value != "") allVMAT = bool.Parse(value);
                                            Settings.Default.allVMAT = bool.Parse(value);
                                        }
                                        else if (parameter == "check couch collision")
                                        {
                                            if (value != "") checkTTCollision = bool.Parse(value);
                                            Settings.Default.checkTTCollision = bool.Parse(value);
                                        }

                                        else if (parameter == "use GPU for dose calculation")
                                        {
                                            useGPUdose = value;
                                            Settings.Default.useGPUdose = value;
                                        }
                                        else if (parameter == "use GPU for optimization")
                                        {
                                            useGPUoptimization = value;
                                            Settings.Default.useGPUoptimization = value;
                                        }
                                        else if (parameter == "MR level restart")
                                        {
                                            MRrestartLevel = value;
                                            Settings.Default.MRRestartLevel= value;
                                        }

                                        //other parameters that should be updated
                                        else if (parameter == "use flash by default") useFlashByDefault = bool.Parse(value);
                                        else if (parameter == "default flash type") { if (value != "") defaultFlashType = value; }
                                        else if (parameter == "calculation model") { if (value != "") calculationModel = value; }
                                        else if (parameter == "optimization model") { if (value != "") optimizationModel = value; }
                                        else if (parameter == "contour field overlap") { if (value != "") contourOverlap = bool.Parse(value); }
                                    }
                                    else if (line.Contains("add default sparing structure")) defaultSpareStruct_temp.Add(parseSparingStructure(line));
                                    else if (line.Contains("add TS")) TSstructures_temp.Add(parseTS(line));
                                    else if (line.Contains("add sclero TS")) scleroTSstructures_temp.Add(parseTS(line));
                                    else if (line.Contains("add linac"))
                                    {
                                        //parse the linacs that should be added. One entry per line
                                        line = cropLine(line, "{");
                                        linac_temp.Add(line.Substring(0, line.IndexOf("}")));
                                    }
                                    else if (line.Contains("add beam energy"))
                                    {
                                        //parse the photon energies that should be added. One entry per line
                                        line = cropLine(line, "{");
                                        energy_temp.Add(line.Substring(0, line.IndexOf("}")));
                                    }
                                    else if (line.Contains("add jaw position"))
                                    {
                                        //parse the default requested number of beams per isocenter
                                        line = cropLine(line, "{");
                                        List<double> tmp = new List<double> { };
                                        //second character should not be the end brace (indicates the last element in the array)
                                        while (line.Contains(","))
                                        {
                                            tmp.Add(double.Parse(line.Substring(0, line.IndexOf(","))));
                                            line = cropLine(line, ",");
                                        }
                                        tmp.Add(double.Parse(line.Substring(0, line.IndexOf("}"))));
                                        if (tmp.Count != 4) MessageBox.Show("Error! Jaw positions not defined correctly!");
                                        else jawPos_temp.Add(new VRect<double>(tmp.ElementAt(0), tmp.ElementAt(1), tmp.ElementAt(2), tmp.ElementAt(3)));
                                    }
                                    else if (line.Equals(":begin scleroderma case configuration:"))
                                    {
                                        //parse the data specific to the scleroderma trial case setup
                                        while (!(line = reader.ReadLine()).Equals(":end scleroderma case configuration:"))
                                        {
                                            if (line.Substring(0, 1) != "%")
                                            {
                                                if (line.Contains("="))
                                                {
                                                    string parameter = line.Substring(0, line.IndexOf("="));
                                                    string value = line.Substring(line.IndexOf("=") + 1, line.Length - line.IndexOf("=") - 1);
                                                    if (parameter == "dose per fraction") { if (double.TryParse(value, out double result)) scleroDosePerFx = result; }
                                                    else if (parameter == "num fx") { if (int.TryParse(value, out int fxResult)) scleroNumFx = fxResult; }
                                                }
                                                else if (line.Contains("add sparing structure")) scleroSpareStruct_temp.Add(parseSparingStructure(line));
                                                else if (line.Contains("add opt constraint")) optConstDefaultSclero_temp.Add(parseOptimizationConstraint(line));
                                            }
                                        }
                                    }
                                    else if (line.Equals(":begin myeloablative case configuration:"))
                                    {
                                        //parse the data specific to the myeloablative case setup
                                        while (!(line = reader.ReadLine()).Equals(":end myeloablative case configuration:"))
                                        {
                                            if (line.Substring(0, 1) != "%")
                                            {
                                                if (line.Contains("="))
                                                {
                                                    string parameter = line.Substring(0, line.IndexOf("="));
                                                    string value = line.Substring(line.IndexOf("=") + 1, line.Length - line.IndexOf("=") - 1);
                                                    if (parameter == "dose per fraction") { if (double.TryParse(value, out double result)) myeloDosePerFx = result; }
                                                    else if (parameter == "num fx") { if (int.TryParse(value, out int fxResult)) myeloNumFx = fxResult; }
                                                }
                                                else if (line.Contains("add sparing structure")) myeloSpareStruct_temp.Add(parseSparingStructure(line));
                                                else if (line.Contains("add opt constraint")) optConstDefaultMyelo_temp.Add(parseOptimizationConstraint(line));
                                            }
                                        }
                                    }
                                    else if (line.Equals(":begin nonmyeloablative case configuration:"))
                                    {
                                        //parse the data specific to the non-myeloablative case setup
                                        while (!(line = reader.ReadLine()).Equals(":end nonmyeloablative case configuration:"))
                                        {
                                            if (line.Substring(0, 1) != "%")
                                            {
                                                if (line.Contains("="))
                                                {
                                                    string parameter = line.Substring(0, line.IndexOf("="));
                                                    string value = line.Substring(line.IndexOf("=") + 1, line.Length - line.IndexOf("=") - 1);
                                                    if (parameter == "dose per fraction") { if (double.TryParse(value, out double result)) nonmyeloDosePerFx = result; }
                                                    else if (parameter == "num fx") { if (int.TryParse(value, out int fxResult)) nonmyeloNumFx = fxResult; }
                                                }
                                                else if (line.Contains("add sparing structure")) nonmyeloSpareStruct_temp.Add(parseSparingStructure(line));
                                                else if (line.Contains("add opt constraint")) optConstDefaultNonMyelo_temp.Add(parseOptimizationConstraint(line));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //anything that is an array needs to be updated AFTER the while loop.
                    if (linac_temp.Any()) linacs = new List<string>(linac_temp);
                    if (energy_temp.Any()) beamEnergies = new List<string>(energy_temp);
                    if (jawPos_temp.Any() && jawPos_temp.Count == 4) jawPos = new List<VRect<double>>(jawPos_temp);
                    if (defaultSpareStruct_temp.Any()) defaultSpareStruct = new List<Tuple<string, string, double>>(defaultSpareStruct_temp);
                    if (TSstructures_temp.Any()) TS_structures = new List<Tuple<string, string>>(TSstructures_temp);
                    if (scleroTSstructures_temp.Any()) scleroStructures = new List<Tuple<string, string>>(scleroTSstructures_temp);
                    if (scleroSpareStruct_temp.Any()) scleroSpareStruct = new List<Tuple<string, string, double>>(scleroSpareStruct_temp);
                    if (myeloSpareStruct_temp.Any()) myeloSpareStruct = new List<Tuple<string, string, double>>(myeloSpareStruct_temp);
                    if (nonmyeloSpareStruct_temp.Any()) nonmyeloSpareStruct = new List<Tuple<string, string, double>>(nonmyeloSpareStruct_temp);
                    if (optConstDefaultSclero_temp.Any()) optConstDefaultSclero = new List<Tuple<string, string, double, double, int>>(optConstDefaultSclero_temp);
                    if (optConstDefaultMyelo_temp.Any()) optConstDefaultMyelo = new List<Tuple<string, string, double, double, int>>(optConstDefaultMyelo_temp);
                    if (optConstDefaultNonMyelo_temp.Any()) optConstDefaultNonMyelo = new List<Tuple<string, string, double, double, int>>(optConstDefaultNonMyelo_temp);
                }
                Settings.Default.Save();
                return false;
            }
            //let the user know if the data parsing failed
            catch (Exception e) { MessageBox.Show(String.Format("Error could not load configuration file because: {0}\n\nAssuming default parameters", e.Message)); return true; }
        }
    }
}
