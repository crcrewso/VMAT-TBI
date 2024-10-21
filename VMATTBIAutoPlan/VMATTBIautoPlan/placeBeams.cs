using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;

namespace VMATTBIautoPlan
{
    class PlaceBeams
    {
        int numIsos;
        int numVMATIsos;
        int[] numBeams;
        public List<string> isoNames;
        bool checkIsoPlacement = false;
        double checkIsoPlacementLimit = 4.99; //TODO: Hack, possible rounding error when fixing outer isos. 
        double isoSeparation = 0;
        bool allVMAT = true;
        bool checkTTCollision = false;
        int extraIsocenters = 0;
        StructureSet selectedSS;
        Structure target = null;
        Tuple<int, DoseValue> prescription;
        string courseId = "";
        Course tbi;
        public ExternalPlanSetup plan = null;
        double[] collRot;
        double[] CW = { 181.0, 179.0 };
        double[] CCW = { 179.0, 181.0 };
        ExternalBeamMachineParameters ebmpArc;
        //ExternalBeamMachineParameters ebmpArc6X;
        //ExternalBeamMachineParameters ebmpStatic;
        List<VRect<double>> jawPos;
        private string calculationModel = "";
        private string optimizationModel = "";
        private string useGPUdose = "";
        private string useGPUoptimization = "";
        private string MRrestart = "";
        private bool useFlash;
        private bool contourOverlap = false;
        private double contourOverlapMargin;
        public List<Structure> jnxs = new List<Structure> { };
        Settings settings;


        //TODO: Fix constructor, too many parameters, use Settings class
        public PlaceBeams(bool vmat, int extra, bool collision, StructureSet ss, string cid, Tuple<int, DoseValue> presc, List<string> i, int iso, int vmatIso, bool appaPlan, int[] beams, double[] coll, List<VRect<double>> jp, string linac, string energy, string calcModel, string optModel, string gpuDose, string gpuOpt, string mr, bool flash, Settings settings)
        {
            allVMAT = vmat;
            extraIsocenters = extra;
            checkTTCollision = collision;
            selectedSS = ss;
            courseId = cid;
            prescription = presc;
            isoNames = new List<string>(i);
            numIsos = iso;
            numVMATIsos = vmatIso;
            numBeams = beams;
            collRot = coll;
            jawPos = new List<VRect<double>>(jp);
            ebmpArc = new ExternalBeamMachineParameters(linac, energy, 600, "ARC", null);
            //if(allVMAT) ebmpArc6X = new ExternalBeamMachineParameters(linac, "6X", 600, "ARC", null);
            //AP/PA beams always use 6X
            //ebmpStatic = new ExternalBeamMachineParameters(linac, "6X", 600, "STATIC", null);
            //copy the calculation model
            calculationModel = calcModel;
            optimizationModel = optModel;
            useGPUdose = gpuDose;
            useGPUoptimization = gpuOpt;
            MRrestart = mr;
            useFlash = flash;
            this.settings = settings;
        }

        //TODO: Fix constructor, too many parameters, use Settings class

        public PlaceBeams(bool vmat, int extra, bool collision, StructureSet ss, string cid, Tuple<int, DoseValue> presc, List<string> i, int iso, int vmatIso, bool appaPlan, int[] beams, double[] coll, List<VRect<double>> jp, string linac, string energy, string calcModel, string optModel, string gpuDose, string gpuOpt, string mr, bool flash, double overlapMargin, Settings settings)
        {
            allVMAT = vmat;
            extraIsocenters = extra;
            checkTTCollision = collision;
            selectedSS = ss;
            courseId = cid;
            prescription = presc;
            isoNames = new List<string>(i);
            numIsos = iso;
            numVMATIsos = vmatIso;
            numBeams = beams;
            collRot = coll;
            jawPos = new List<VRect<double>>(jp);
            ebmpArc = new ExternalBeamMachineParameters(linac, energy, 600, "ARC", null);
            //if(allVMAT) ebmpArc6X = new ExternalBeamMachineParameters(linac, "6X", 600, "ARC", null);
            //AP/PA beams always use 6X
            //copy the calculation model
            calculationModel = calcModel;
            optimizationModel = optModel;
            useGPUdose = gpuDose;
            useGPUoptimization = gpuOpt;
            MRrestart = mr;
            useFlash = flash;
            //user wants to contour the overlap between fields in adjacent VMAT isocenters
            contourOverlap = true;
            contourOverlapMargin = overlapMargin;
            this.settings = settings;
        }

        public ExternalPlanSetup Generate_beams()
        {
            if (CreatePlan()) return null;
            List<VVector> isoLocations = GetIsocenterPositions();
            if (contourOverlap) ContourFieldOverlap(isoLocations);
            Set_beams(isoLocations);

            if (checkIsoPlacement) MessageBox.Show(String.Format("WARNING: < {0:0.00} cm margin at most superior and inferior locations of body! Verify isocenter placement!", checkIsoPlacementLimit / 10));
            return plan;
        }

        private bool CreatePlan()
        {
            //look for a course with Id specified by the user. If it does not exit, create it, otherwise load it into memory
            if (!selectedSS.Patient.Courses.Where(x => x.Id == courseId).Any())
            {
                if (selectedSS.Patient.CanAddCourse())
                {
                    tbi = selectedSS.Patient.AddCourse();
                    tbi.Id = courseId;
                }
                else
                {
                    MessageBox.Show("Error! \nCan't add a treatment course to the patient!");
                    return true;
                }
            }
            else tbi = selectedSS.Patient.Courses.FirstOrDefault(x => x.Id == courseId);

            //6-10-2020 EAS, research system only!
            //if (tbi.ExternalPlanSetups.Where(x => x.Id == "_VMAT TBI").Any()) if (tbi.CanRemovePlanSetup((tbi.ExternalPlanSetups.First(x => x.Id == "_VMAT TBI")))) tbi.RemovePlanSetup(tbi.ExternalPlanSetups.First(x => x.Id == "_VMAT TBI"));
            if (tbi.ExternalPlanSetups.Where(x => x.Id == "_VMAT TBI").Any())
            {
                MessageBox.Show("A plan named '_VMAT TBI' Already exists! \nESAPI can't remove plans in the clinical environment! \nPlease manually remove this plan and try again.");
                return true;
            }
            // TODO: Set calculation models 
            // TODO: Remove Jaw Tracking. 
            plan = tbi.AddExternalPlanSetup(selectedSS);
            //100% dose prescribed in plan and plan ID is _VMAT TBI
            plan.SetPrescription(prescription.Item1, prescription.Item2, 1.0);
            plan.Id = "_VMAT TBI";
            //ask the user to set the calculation model if not calculation model was set in UI.xaml.cs (up near the top with the global parameters)
            if (calculationModel == "")
            {
                IEnumerable<string> models = plan.GetModelsForCalculationType(CalculationType.PhotonVolumeDose);
                selectItem SUI = new VMATTBIautoPlan.selectItem();
                SUI.title.Text = "No calculation model set!" + Environment.NewLine + "Please select a calculation model!";
                foreach (string s in plan.GetModelsForCalculationType(CalculationType.PhotonVolumeDose)) SUI.itemCombo.Items.Add(s);
                SUI.ShowDialog();
                if (!SUI.confirm) return true;
                //get the plan the user chose from the combobox
                calculationModel = SUI.itemCombo.SelectedItem.ToString();

                //just an FYI that the calculation will likely run out of memory and crash the optimization when Acuros is used
                if (calculationModel.ToLower().Contains("acuros") || calculationModel.ToLower().Contains("axb"))
                {
                    confirmUI CUI = new VMATTBIautoPlan.confirmUI();
                    CUI.message.Text = "Warning!" + Environment.NewLine + "The optimization will likely crash (i.e., run out of memory) if Acuros is used!" + Environment.NewLine + "Continue?!";
                    CUI.ShowDialog();
                    if (!CUI.confirm) return true;
                }
            }
            plan.SetCalculationModel(CalculationType.PhotonVolumeDose, calculationModel);
            plan.SetCalculationModel(CalculationType.PhotonVMATOptimization, optimizationModel);

            //Dictionary<string, string> d = plan.GetCalculationOptions(plan.GetCalculationModel(CalculationType.PhotonVMATOptimization));
            //string m = "";
            //foreach (KeyValuePair<string, string> t in d) m += String.Format("{0}, {1}", t.Key, t.Value) + System.Environment.NewLine;
            //MessageBox.Show(m);

            //set the GPU dose calculation option (only valid for acuros)
            if (useGPUdose == "Yes" && !calculationModel.Contains("AAA")) plan.SetCalculationOption(calculationModel, "UseGPU", useGPUdose);
            else plan.SetCalculationOption(calculationModel, "UseGPU", "No");

            //set MR restart level option for the photon optimization
            plan.SetCalculationOption(optimizationModel, "MRLevelAtRestart", MRrestart);
            plan.SetCalculationOption(optimizationModel, "ApertureShapeController", "Moderate");

            //set the GPU optimization option
            if (useGPUoptimization == "Yes") plan.SetCalculationOption(optimizationModel, "General/OptimizerSettings/UseGPU", useGPUoptimization);
            else plan.SetCalculationOption(optimizationModel, "General/OptimizerSettings/UseGPU", "No");

            //reference point can only be added for a plan that IS CURRENTLY OPEN
            //plan.AddReferencePoint(selectedSS.Structures.First(x => x.Id == "TS_PTV_VMAT"), null, "VMAT TBI", "VMAT TBI");

            //6-10-2020 EAS, research system only!
            if ((numIsos > numVMATIsos) && tbi.ExternalPlanSetups.Where(x => x.Id.ToLower().Contains("legs")).Any())
            {
                MessageBox.Show("Plan(s) with the string 'legs' already exists! \nESAPI can't remove plans in the clinical environment! \nPlease manually remove this plan and try again.");
                return true;
            }

            //these need to be fixed
            //v16 of Eclipse allows for the creation of a plan with a named target structure and named primary reference point. Neither of these options are available in v15
            //plan.TargetVolumeID = selectedSS.Structures.First(x => x.Id == "TS_PTV_VMAT");
            //plan.PrimaryReferencePoint = plan.ReferencePoints.Fisrt(x => x.Id == "VMAT TBI");
            return false;
        }

        private List<VVector> GetIsocenterPositions()
        {
            List<VVector> iso = new List<VVector> { };
            Image image = selectedSS.Image;
            VVector userOrigin = image.UserOrigin;
            //if the user requested to add flash to the plan, be sure to grab the ptv_body_flash structure (i.e., the ptv_body structure created from the body with added flash). 
            //This structure is named 'TS_FLASH_TARGET'
            if (useFlash) target = selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == "ts_flash_target");
            else target = selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == "ptv_body");

            //Adapted PR #22 based on iromero77 suggestion (based on Stanford experience)
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            double offsetY = 0.0;


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //matchline is present and not empty

            //All VMAT portions of the plans will ONLY have 3 isocenters
            //double isoSeparation = Math.Round(((target.MeshGeometry.Positions.Max(p => p.Z) - target.MeshGeometry.Positions.Min(p => p.Z) - 10.0*numIsos) / numIsos) / 10.0f) * 10.0f;
            //5-7-202 The equation below was determined assuming each VMAT plan would always use 3 isos. In addition, the -30.0 was empirically determined by comparing the calculated isocenter separations to those that were used in the clinical plans
            //isoSeparation = Math.Round(((target.MeshGeometry.Positions.Max(p => p.Z) - target.MeshGeometry.Positions.Min(p => p.Z) - 30.0) / 3) / 10.0f) * 10.0f;

            //however, the actual correct equation is given below:
            isoSeparation = Math.Round((target.MeshGeometry.Positions.Max(p => p.Z) - target.MeshGeometry.Positions.Min(p => p.Z) - 380.0) / (numVMATIsos - 1) / 10.0f) * 10.0f;

            //It is calculated by setting the most superior and inferior isocenters to be 19.0 cm from the target volume edge in the z-direction. The isocenter separtion is then calculated as half the distance between these two isocenters (sep = ((max-19cm)-(min+19cm)/2).
            //Tested on 5-7-2020. When the correct equation is rounded, it gives the same answer as the original empirical equation above, however, the isocenters are better positioned in the target volume (i.e., more symmetric about the target volume). 
            //The ratio of the actual to empirical iso separation equations can be expressed as r=(3/(numVMATIsos-1))((x-380)/(x-30)) where x = (max-min). The ratio is within +/-5% for max-min values (i.e., patient heights) between 99.0 cm (i.e., 3.25 feet) and 116.0 cm

            if (isoSeparation > 380.0)
            {
                var CUI = new VMATTBIautoPlan.confirmUI();
                CUI.message.Text = "Calculated isocenter separation > 38.0 cm, which reduces the overlap between adjacent fields!" + Environment.NewLine + Environment.NewLine + "Truncate isocenter separation to 38.0 cm?!";
                CUI.ShowDialog();
                if (CUI.confirm) isoSeparation = 380.0; // HACK Move this hard coded value to the Settings class.
            }

            for (int i = 0; i < numIsos; i++)
            {
                VVector v = new VVector
                {
                    x = userOrigin.x,
                    y = userOrigin.y + offsetY,
                    z = target.MeshGeometry.Positions.Max(p => p.Z) - i * isoSeparation - 190.0
                };
                //round z position to the nearest integer cm 
                v = plan.StructureSet.Image.DicomToUser(v, plan);
                v.y = Math.Round(v.y / 10.0f) * 10.0f;
                v.z = Math.Round(v.z / 10.0f) * 10.0f;
                v = plan.StructureSet.Image.UserToDicom(v, plan);
                iso.Add(v);
            }

            //evaluate the distance between the edge of the beam and the max/min of the PTV_body contour. If it is < checkIsoPlacementLimit, then warn the user that they might be fully covering the ptv_body structure.
            //7-17-2020, checkIsoPlacementLimit = 5 mm
            VVector firstIso = iso.First();
            VVector lastIso = iso.Last();
            if (!(firstIso.z + 200.0 - target.MeshGeometry.Positions.Max(p => p.Z) >= checkIsoPlacementLimit) ||
                !(target.MeshGeometry.Positions.Min(p => p.Z) - (lastIso.z - 200.0) >= checkIsoPlacementLimit)) checkIsoPlacement = true;
            // TODO: This is a hack to test for outer iso positions but those are set manually 
            //MessageBox.Show(String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
            //    firstIso.z,
            //    lastIso.z,
            //    target.MeshGeometry.Positions.Max(p => p.Z),
            //    target.MeshGeometry.Positions.Min(p => p.Z),
            //    (firstIso.z + 200.0 - target.MeshGeometry.Positions.Max(p => p.Z)),
            //    (target.MeshGeometry.Positions.Min(p => p.Z) - (lastIso.z - 200.0))));

            return iso;
        }


        // TODO: is this how I can recalculate the junction structures?
        //function used to contour the overlap between fields in adjacent isocenters for the VMAT Plan ONLY!
        //this option is requested by the user by selecting the checkbox on the main UI on the beam placement tab
        private void ContourFieldOverlap(List<VVector> isoLocations)
        {
            //grab the image and get the z resolution and dicom origin (we only care about the z position of the dicom origin)
            Image image = selectedSS.Image;
            double zResolution = image.ZRes;
            VVector dicomOrigin = image.Origin;
            //center position between adjacent isocenters, number of image slices to contour on, start image slice location for contouring
            List<Tuple<double, int, int>> overlap = new List<Tuple<double, int, int>> { };

            //calculate the center position between adjacent isocenters, number of image slices to contour on based on overlap and with additional user-specified margin (from main UI)
            //and the slice where the contouring should begin
            //string output = "";
            for (int i = 1; i < numVMATIsos; i++)
            {
                //calculate the center position between adjacent isocenters. NOTE: this calculation works from superior to inferior!
                double center = isoLocations.ElementAt(i - 1).z + (isoLocations.ElementAt(i).z - isoLocations.ElementAt(i - 1).z) / 2;
                //this is left as a double so I can cast it to an int in the second overlap item and use it in the calculation in the third overlap item
                double numSlices = Math.Ceiling(400.0 + contourOverlapMargin - Math.Abs(isoLocations.ElementAt(i).z - isoLocations.ElementAt(i - 1).z));
                overlap.Add(new Tuple<double, int, int>(
                    center,
                    (int)(numSlices / zResolution),
                    (int)(Math.Abs(dicomOrigin.z - center + numSlices / 2) / zResolution)));
                //add a new junction structure (named TS_jnx<i>) to the stack. Contours will be added to these structure later
                jnxs.Add(selectedSS.AddStructure("CONTROL", string.Format("TS_jnx{0}", i)));
                //output += String.Format("{0}, {1}, {2}\n", 
                //    isoLocations.ElementAt(i - 1).z + (isoLocations.ElementAt(i).z - isoLocations.ElementAt(i - 1).z) / 2,
                //    (int)Math.Ceiling((410.0 - Math.Abs(isoLocations.ElementAt(i).z - isoLocations.ElementAt(i - 1).z)) / zResolution),
                //    (int)(Math.Abs(dicomOrigin.z - (isoLocations.ElementAt(i - 1).z + ((isoLocations.ElementAt(i).z - isoLocations.ElementAt(i - 1).z) / 2)) + Math.Ceiling((410.0 - Math.Abs(isoLocations.ElementAt(i).z - isoLocations.ElementAt(i - 1).z))/2)) / zResolution));
            }
            //MessageBox.Show(output);

            //make a box at the min/max x,y positions of the target structure with 5 cm margin
            Point3DCollection targetPts = target.MeshGeometry.Positions;
            double xMax = targetPts.Max(p => p.X) + 50.0;
            double xMin = targetPts.Min(p => p.X) - 50.0;
            double yMax = targetPts.Max(p => p.Y) + 50.0;
            double yMin = targetPts.Min(p => p.Y) - 50.0;

            VVector[] pts = new[] {
                                    new VVector(xMax, yMax, 0),
                                    new VVector(xMax, 0, 0),
                                    new VVector(xMax, yMin, 0),
                                    new VVector(0, yMin, 0),
                                    new VVector(xMin, yMin, 0),
                                    new VVector(xMin, 0, 0),
                                    new VVector(xMin, yMax, 0),
                                    new VVector(0, yMax, 0)};

            //add the contours to each relevant plan for each structure in the jnxs stack
            // TODO: break this out and add a button to call to recalculate jnxs. 
            int count = 0;
            foreach (Tuple<double, int, int> value in overlap)
            {
                for (int i = value.Item3; i < (value.Item3 + value.Item2); i++) jnxs.ElementAt(count).AddContourOnImagePlane(pts, i);
                //only keep the portion of the box contour that overlaps with the target
                jnxs.ElementAt(count).SegmentVolume = jnxs.ElementAt(count).And(target.Margin(0));
                count++;
            }
        }

        private void Set_beams(List<VVector> isoLocations)
        {
            //DRR parameters (dummy parameters to generate DRRs for each field)
            DRRCalculationParameters DRR = new DRRCalculationParameters
            {
                DRRSize = 500.0,
                FieldOutlines = true,
                StructureOutlines = true
            };
            DRR.SetLayerParameters(1, 1.0, 100.0, 1000.0);

            //place the beams for the VMAT plan
            int count = 0;
            string beamName;
            VRect<double> jp;
            for (int i = 0; i < numVMATIsos; i++)
            {
                var isoName = isoNames.ElementAt(i);
                double[] collimatorRotations;
                if (IsoNameHelper.IsTop(isoName))
                {
                    collimatorRotations = settings.TopCollRot;
                }
                else {
                    collimatorRotations = settings.BottomCollRot;
                }
                
                for (int j = 0; j < numBeams[i]; j++)
                {
                    //second isocenter and third beam requires the x-jaw positions to be mirrored about the y-axis (these jaw positions are in the fourth element of the jawPos list)
                    //this is generally the isocenter located in the pelvis and we want the beam aimed at the kidneys-area
                    if (i == 1 && j == 2) jp = jawPos.ElementAt(j + 1);
                    else if (i == 1 && j == 3) jp = jawPos.ElementAt(j - 1);
                    else jp = jawPos.ElementAt(j);
                    Beam b;
                    beamName = "";
                    beamName += String.Format("{0} ", count + 1);
                    //zero collimator rotations of two main fields for beams in isocenter immediately superior to matchline. Adjust the third beam such that collimator rotation is 90 degrees. Do not adjust 4th beam
                    double coll = collimatorRotations[j];
                    // for allVMAT, if legs are present, last two isos have their collimator positions rotated 180 degrees

                    //all even beams (e.g., 2, 4, etc.) will be CCW and all odd beams will be CW
                    if (count % 2 == 0)
                    {
                        //if(allVMAT && i >= numVMATIsos - extraIsocenters) b = plan.AddArcBeam(ebmpArc6X, jp, coll, CCW[0], CCW[1], GantryDirection.CounterClockwise, 0, isoLocations.ElementAt(i));
                        //else b = plan.AddArcBeam(ebmpArc, jp, coll, CCW[0], CCW[1], GantryDirection.CounterClockwise, 0, isoLocations.ElementAt(i));
                        b = plan.AddArcBeam(ebmpArc, jp, coll, CCW[0], CCW[1], GantryDirection.CounterClockwise, 0, isoLocations.ElementAt(i));
                        if (j >= 2) beamName += String.Format("CCW {0}{1}", isoName, 90);
                        else beamName += String.Format("CCW {0}{1}", isoName, "");
                    }
                    else
                    {
                        //if (allVMAT && i >= numVMATIsos - extraIsocenters) b = plan.AddArcBeam(ebmpArc6X, jp, coll, CW[0], CW[1], GantryDirection.Clockwise, 0, isoLocations.ElementAt(i));
                        //else b = plan.AddArcBeam(ebmpArc, jp, coll, CW[0], CW[1], GantryDirection.Clockwise, 0, isoLocations.ElementAt(i));
                        b = plan.AddArcBeam(ebmpArc, jp, coll, CW[0], CW[1], GantryDirection.Clockwise, 0, isoLocations.ElementAt(i));
                        if (j >= 2) beamName += String.Format("CW {0}{1}", isoName, 90);
                        else beamName += String.Format("CW {0}{1}", isoName, "");
                    }
                    if (beamName.Length > 16) b.Id = beamName.Substring(0, 16);
                    else b.Id = beamName;
                    b.CreateOrReplaceDRR(DRR);
                    count++;
                }
            }

            
            MessageBox.Show("Beams placed successfully!\nPlease proceed to the optimization setup tab!");
        }
    }
}
