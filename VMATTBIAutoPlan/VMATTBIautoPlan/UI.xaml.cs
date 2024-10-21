using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;

namespace VMATTBIautoPlan
{
    public partial class UI : UserControl
    {
        // TODO: Rip out all references to Settings files and replace with hard-coded values


        //data members
        public Patient pi = null;
        StructureSet selectedSS = null;
        private bool firstSpareStruct = true;
        private bool firstOptStruct = true;
        public int clearSpareBtnCounter = 0;
        public int clearOptBtnCounter = 0;
        List<Tuple<string, string>> optParameters = new List<Tuple<string, string>> { };
        ExternalPlanSetup VMATplan = null;
        int numIsos = 0;
        int numVMATIsos = 0;
        public List<string> isoNames = new List<string> { };
        Tuple<int, DoseValue> prescription = null;
        bool useFlash = false;
        string flashType = "";
        List<Structure> jnxs = new List<Structure> { };
        Structure flashStructure = null;
        PlanPrep prep = null;
        Settings settings = null;

        public UI(ScriptContext c)
        {
            InitializeComponent();
            settings = new Settings();
            //check the version information of Eclipse installed on this machine. If it is older than version 15.6, let the user know that this script may not work properly on their system
            if (!double.TryParse(c.VersionInfo.Substring(0, c.VersionInfo.LastIndexOf(".")), out double vinfo)) MessageBox.Show("Warning! Could not parse Eclise version number! Proceed with caution!");
            else if (vinfo < 15.6) MessageBox.Show(String.Format("Warning! Detected Eclipse version: {0:0.0} is older than v15.6! Proceed with caution!", vinfo));

            pi = c.Patient;
            //SSID is combobox defined in UI.xaml
            foreach (StructureSet s in pi.StructureSets) SSID.Items.Add(s.Id);
            //SSID default is the current structure set in the context
            if (c.StructureSet == null) MessageBox.Show("Warning! No structure set in context! Please select a structure set at the top of the GUI!");
            else SSID.Text = c.StructureSet.Id;

            // TODO - Remove this line
            // load script configuration and display the settings
            // if (configurationFile != "") loadConfigurationSettings(configurationFile);
            DisplayConfigurationParameters();

            //pre-populate the flash comboxes (set global flash as default)
            flashOption.Items.Add("Global");
            flashOption.Items.Add("Local");
            flashOption.Text = settings.DefaultFlashType;
            flashMarginTB.Text = settings.DefaultFlashMargin;

            //set default PTV inner margin from body
            targetMarginTB.Text = settings.DefaultTargetMargin;
        }

        private void Help_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(settings.DocumentationPath + "VMAT_TBI_guide.pdf");
        }

        private void QuickStart_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(settings.DocumentationPath + "TBI_plugIn_quickStart_guide.pdf");
        }

        //method to display the loaded configuration settings
        private void DisplayConfigurationParameters()
        {
            configTB.Text = "";
            configTB.Text = String.Format("{0}", DateTime.Now.ToString()) + System.Environment.NewLine;
            if(settings.ConfigFile != "") configTB.Text += String.Format("Configuration file: {0}",settings.ConfigFile) + System.Environment.NewLine + System.Environment.NewLine;
            else configTB.Text += String.Format("Configuration file: none") + System.Environment.NewLine + System.Environment.NewLine;
            configTB.Text += String.Format("Documentation path: {0}",settings.DocumentationPath) + System.Environment.NewLine + System.Environment.NewLine;
            configTB.Text += String.Format("Default parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format("Course Id: {0}", settings.CourseId) + System.Environment.NewLine;
            configTB.Text += String.Format("Assign VMAT technique to all fields: {0}", settings.AllVMAT) + System.Environment.NewLine;
            configTB.Text += String.Format("Check for potential couch collision: {0}", settings.CheckTTCollision) + System.Environment.NewLine;
            configTB.Text += String.Format("Include flash by default: {0}", settings.UseFlashByDefault) + System.Environment.NewLine;
            configTB.Text += String.Format("Flash type: {0}", settings.DefaultFlashType) + System.Environment.NewLine;
            configTB.Text += String.Format("Flash margin: {0} cm", settings.DefaultFlashMargin) + System.Environment.NewLine;
            configTB.Text += String.Format("Target inner margin: {0} cm", settings.DefaultTargetMargin) + System.Environment.NewLine;
            configTB.Text += String.Format("Contour field ovelap: {0}", settings.ContourOverlap) + System.Environment.NewLine;
            configTB.Text += String.Format("Contour field overlap margin: {0} cm", settings.ContourFieldOverlapMargin) + System.Environment.NewLine;
            configTB.Text += String.Format("Available settings.Linacs:") + System.Environment.NewLine;
            foreach(string l in settings.Linacs) configTB.Text += l + System.Environment.NewLine;
            configTB.Text += String.Format("Available photon energies:") + System.Environment.NewLine;
            foreach (string e in settings.BeamEnergies) configTB.Text += e + System.Environment.NewLine;
            configTB.Text += String.Format("Beams per isocenter: ");
            for (int i = 0; i < settings.BeamsPerIso.Length; i++)
            {
                configTB.Text += String.Format("{0}", settings.BeamsPerIso.ElementAt(i));
                if(i != settings.BeamsPerIso.Length-1) configTB.Text += String.Format(", ");
            }
            configTB.Text += System.Environment.NewLine;
            configTB.Text += String.Format("Collimator rotation (deg) order: ");
            for (int i = 0; i < settings.CollRot.Length; i++)
            {
                configTB.Text += String.Format("{0:0.0}", settings.CollRot.ElementAt(i));
                if (i != settings.CollRot.Length - 1) configTB.Text += String.Format(", ");
            }
            configTB.Text += System.Environment.NewLine;
            configTB.Text += String.Format("Field jaw position (cm) order: ") + System.Environment.NewLine;
            configTB.Text += String.Format("(x1,y1,x2,y2)") + System.Environment.NewLine;
            foreach (VRect<double> j in settings.JawPos) configTB.Text += String.Format("({0:0.0},{1:0.0},{2:0.0},{3:0.0})", j.X1 / 10, j.Y1 / 10, j.X2 / 10, j.Y2 / 10) + System.Environment.NewLine;
            configTB.Text += String.Format("Photon dose calculation model: {0}", settings.CalculationModel) + System.Environment.NewLine;
            configTB.Text += String.Format("Use GPU for dose calculation: {0}", settings.UseGPUdose) + System.Environment.NewLine;
            configTB.Text += String.Format("Photon optimization model: {0}", settings.OptimizationModel) + System.Environment.NewLine;
            configTB.Text += String.Format("Use GPU for optimization: {0}", settings.UseGPUoptimization) + System.Environment.NewLine;
            configTB.Text += String.Format("MR level restart at: {0}", settings.MRrestartLevel) + System.Environment.NewLine + System.Environment.NewLine;

            configTB.Text += String.Format("Requested general tuning structures:") + System.Environment.NewLine;
            configTB.Text += String.Format(" {0, -10} | {1, -15} |", "DICOM type", "Structure Id") + System.Environment.NewLine;
            foreach (Tuple<string, string> ts in settings.TS_Structures) configTB.Text += String.Format(" {0, -10} | {1, -15} |" + System.Environment.NewLine, ts.Item1, ts.Item2);
            configTB.Text += System.Environment.NewLine;

            configTB.Text += String.Format("Default sparing structures:") + System.Environment.NewLine;
            configTB.Text += String.Format(" {0, -15} | {1, -19} | {2, -11} |", "structure Id", "sparing type", "margin (cm)") + System.Environment.NewLine;
            foreach (Tuple<string, string, double> spare in settings.DefaultSpareStruct) configTB.Text += String.Format(" {0, -15} | {1, -19} | {2,-11:N1} |" + System.Environment.NewLine, spare.Item1, spare.Item2, spare.Item3);
            configTB.Text += System.Environment.NewLine;

            configTB.Text += "-----------------------------------------------------------------------------" + System.Environment.NewLine;
            configTB.Text += String.Format("Scleroderma trial case parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format("Dose per fraction: {0} cGy", settings.ScleroDosePerFx) + System.Environment.NewLine;
            configTB.Text += String.Format("Number of fractions: {0}", settings.ScleroNumFx) + System.Environment.NewLine;
            if (settings.ScleroSpareStruct.Any())
            {
                configTB.Text += String.Format("Scleroderma case additional sparing structures:") + System.Environment.NewLine;
                configTB.Text += String.Format(" {0, -15} | {1, -19} | {2, -11} |", "structure Id", "sparing type", "margin (cm)") + System.Environment.NewLine;
                foreach (Tuple<string, string, double> spare in settings.ScleroSpareStruct) configTB.Text += String.Format(" {0, -15} | {1, -19} | {2,-11:N1} |" + System.Environment.NewLine, spare.Item1, spare.Item2, spare.Item3);
                configTB.Text += System.Environment.NewLine;
            }
            else configTB.Text += String.Format("No additional sparing structures for Scleroderma case") + System.Environment.NewLine + System.Environment.NewLine;
            configTB.Text += String.Format("Requested scleroderma trial tuning structures:") + System.Environment.NewLine;
            configTB.Text += String.Format(" {0, -10} | {1, -15} |", "DICOM type", "Structure Id") + System.Environment.NewLine;
            foreach (Tuple<string, string> sts in settings.ScleroStructures) configTB.Text += String.Format(" {0, -10} | {1, -15} |" + System.Environment.NewLine, sts.Item1, sts.Item2);
            configTB.Text += System.Environment.NewLine;
            configTB.Text += String.Format("Optimization parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format(" {0, -15} | {1, -16} | {2, -10} | {3, -10} | {4, -8} |", "structure Id", "constraint type", "dose (cGy)", "volume (%)", "priority") + System.Environment.NewLine;
            foreach (Tuple<string,string,double,double,int> opt in settings.OptConstDefaultSclero) configTB.Text += String.Format(" {0, -15} | {1, -16} | {2,-10:N1} | {3,-10:N1} | {4,-8} |" + System.Environment.NewLine, opt.Item1, opt.Item2, opt.Item3, opt.Item4, opt.Item5);
            configTB.Text += System.Environment.NewLine;

            configTB.Text += "-----------------------------------------------------------------------------" + System.Environment.NewLine;
            configTB.Text += String.Format("Myeloablative case parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format("Dose per fraction: {0} cGy", settings.MyeloDosePerFx) + System.Environment.NewLine;
            configTB.Text += String.Format("Number of fractions: {0}", settings.MyeloNumFx) + System.Environment.NewLine;
            if (settings.MyeloSpareStruct.Any())
            {
                configTB.Text += String.Format("Myeloablative case additional sparing structures:") + System.Environment.NewLine;
                configTB.Text += String.Format(" {0, -15} | {1, -19} | {2, -11} |", "structure Id", "sparing type", "margin (cm)") + System.Environment.NewLine;
                foreach (Tuple<string, string, double> spare in settings.MyeloSpareStruct) configTB.Text += String.Format(" {0, -15} | {1, -19} | {2,-11:N1} |" + System.Environment.NewLine, spare.Item1, spare.Item2, spare.Item3);
                configTB.Text += System.Environment.NewLine;
            }
            else configTB.Text += String.Format("No additional sparing structures for Myeloablative case") + System.Environment.NewLine + System.Environment.NewLine;
            configTB.Text += String.Format("Optimization parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format(" {0, -15} | {1, -16} | {2, -10} | {3, -10} | {4, -8} |", "structure Id", "constraint type", "dose (cGy)", "volume (%)", "priority") + System.Environment.NewLine;
            foreach (Tuple<string, string, double, double, int> opt in settings.OptConstDefaultMyelo) configTB.Text += String.Format(" {0, -15} | {1, -16} | {2,-10:N1} | {3,-10:N1} | {4,-8} |" + System.Environment.NewLine, opt.Item1, opt.Item2, opt.Item3, opt.Item4, opt.Item5);
            configTB.Text += System.Environment.NewLine;

            configTB.Text += "-----------------------------------------------------------------------------" + System.Environment.NewLine;
            configTB.Text += String.Format("Non-Myeloablative case parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format("Dose per fraction: {0} cGy", settings.NonMyeloDosePerFx) + System.Environment.NewLine;
            configTB.Text += String.Format("Number of fractions: {0}", settings.NonMyeloNumFx) + System.Environment.NewLine;
            if (settings.NonMyeloSpareStruct.Any())
            {
                configTB.Text += String.Format("Non-Myeloablative case additional sparing structures:") + System.Environment.NewLine;
                configTB.Text += String.Format(" {0, -15} | {1, -19} | {2, -11} |", "structure Id", "sparing type", "margin (cm)") + System.Environment.NewLine;
                foreach (Tuple<string, string, double> spare in settings.NonMyeloSpareStruct) configTB.Text += String.Format(" {0, -15} | {1, -19} | {2,-11:N1} |" + System.Environment.NewLine, spare.Item1, spare.Item2, spare.Item3);
                configTB.Text += System.Environment.NewLine;
            }
            else configTB.Text += String.Format("No additional sparing structures for Non-Myeloablative case") + System.Environment.NewLine + System.Environment.NewLine;
            configTB.Text += String.Format("Optimization parameters:") + System.Environment.NewLine;
            configTB.Text += String.Format(" {0, -15} | {1, -16} | {2, -10} | {3, -10} | {4, -8} |", "structure Id", "constraint type", "dose (cGy)", "volume (%)", "priority") + System.Environment.NewLine;
            foreach (Tuple<string, string, double, double, int> opt in settings.OptConstDefaultNonMyelo) configTB.Text += String.Format(" {0, -15} | {1, -16} | {2,-10:N1} | {3,-10:N1} | {4,-8} |" + System.Environment.NewLine, opt.Item1, opt.Item2, opt.Item3, opt.Item4, opt.Item5);
            configTB.Text += "-----------------------------------------------------------------------------" + System.Environment.NewLine;
            configScroller.ScrollToTop();
        }

        //flash stuff
        //simple method to either show or hide the relevant flash parameters depending on if the user wants to use flash (i.e., if the 'add flash' checkbox is checked)
        private void Flash_chkbox_Click(object sender, RoutedEventArgs e) { UpdateUseFlash(); }

        private void UpdateUseFlash()
        {
            //logic to hide or show the flash option in GUI
            if (flash_chkbox.IsChecked.Value)
            {
                flashOption.Visibility = Visibility.Visible;
                flashMarginLabel.Visibility = Visibility.Visible;
                flashMarginTB.Visibility = Visibility.Visible;
                if (flashType == "Local")
                {
                    flashVolumeLabel.Visibility = Visibility.Visible;
                    flashVolume.Visibility = Visibility.Visible;
                }
            }
            else
            {
                flashOption.Visibility = Visibility.Hidden;
                flashMarginLabel.Visibility = Visibility.Hidden;
                flashMarginTB.Visibility = Visibility.Hidden;
                if (flashType == "Local")
                {
                    flashVolumeLabel.Visibility = Visibility.Hidden;
                    flashVolume.Visibility = Visibility.Hidden;
                }
            }
            //update whether the user wants to user flash or not
            useFlash = flash_chkbox.IsChecked.Value;
        }

        private void FlashOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //update the flash type whenever the user changes the option in the combo box. If the flash type is local, show the flash volume combo box and label. If not, hide them
            flashType = flashOption.SelectedItem.ToString();
            if (flashType == "Global")
            {
                flashVolumeLabel.Visibility = Visibility.Hidden;
                flashVolume.Visibility = Visibility.Hidden;
            }
            else
            {
                flashVolumeLabel.Visibility = Visibility.Visible;
                flashVolume.Visibility = Visibility.Visible;
            }
        }

        //stuff related to TS Generation tab
        private void TargetMarginInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Specify the inner body margin (in cm) that should be used to create the PTV. Typical values range from 0.0 to 0.5 cm. Default value at Stanford University is 0.3 cm.");
        }

        private void ContourOverlapInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Selecting this option will contour the (approximate) overlap between fields in adjacent isocenters in the VMAT plan and assign the resulting structures as targets in the optimization.");
        }

        //add structure to spare to the list
        private void Add_str_click(object sender, RoutedEventArgs e)
        {
            //populate the comboboxes
            Add_sp_volumes(selectedSS, new List<Tuple<string, string, double>> { Tuple.Create("--select--", "--select--", 0.0) });
            spareStructScroller.ScrollToBottom();
        }

        //add the header to the structure sparing list (basically just add some labels to make it look nice)
        private void Add_sp_header()
        {
            StackPanel sp1 = new StackPanel
            {
                Height = 30,
                Width = structures_sp.Width,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 0, 5, 5)
            };

            Label strName = new Label
            {
                Content = "Structure Name",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
                FontSize = 14,
                Margin = new Thickness(27, 0, 0, 0)
            };

            Label spareType = new Label
            {
                Content = "Sparing Type",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
                FontSize = 14,
                Margin = new Thickness(10, 0, 0, 0)
            };

            Label marginLabel = new Label
            {
                Content = "Margin (cm)",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 0)
            };

            sp1.Children.Add(strName);
            sp1.Children.Add(spareType);
            sp1.Children.Add(marginLabel);
            structures_sp.Children.Add(sp1);

            //bool to indicate that the header has been added
            firstSpareStruct = false;
        }

        //populate the structure sparing list. This method is called whether the add structure or add defaults buttons are hit (because a vector containing the list of structures is passed as an argument to this method)
        private void Add_sp_volumes(StructureSet selectedSS, List<Tuple<string, string, double>> defaultList)
        {
            if (firstSpareStruct) Add_sp_header();
            for (int i = 0; i < defaultList.Count; i++)
            {
                StackPanel sp = new StackPanel
                {
                    Height = 30,
                    Width = structures_sp.Width,
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(10, 0, 5, 5)
                };

                ComboBox str_cb = new ComboBox
                {
                    Name = "str_cb",
                    Width = 150,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(5, 5, 0, 0)
                };

                str_cb.Items.Add("--select--");
                //this code is used to fix the issue where the structure exists in the structure set, but doesn't populate as the default option in the combo box.
                int index = 0;
                //j is initially 1 because we already added "--select--" to the combo box
                int j = 1;
                foreach (Structure s in selectedSS.Structures)
                {
                    str_cb.Items.Add(s.Id);
                    if (s.Id.ToLower() == defaultList[i].Item1.ToLower()) index = j;
                    j++;
                }
                str_cb.SelectedIndex = index;
                sp.Children.Add(str_cb);

                ComboBox type_cb = new ComboBox
                {
                    Name = "type_cb",
                    Width = 150,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, 5, 0, 0),
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                string[] types = new string[] { "--select--", "Mean Dose < Rx Dose", "Dmax ~ Rx Dose" };
                foreach (string s in types) type_cb.Items.Add(s);
                type_cb.Text = defaultList[i].Item2;
                type_cb.SelectionChanged += new SelectionChangedEventHandler(Type_cb_change);
                sp.Children.Add(type_cb);

                TextBox addMargin = new TextBox
                {
                    Name = "addMargin_tb",
                    Width = 120,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 5, 0, 0),
                    Text = Convert.ToString(defaultList[i].Item3)
                };
                if (defaultList[i].Item2 != "Mean Dose < Rx Dose") addMargin.Visibility = Visibility.Hidden;
                sp.Children.Add(addMargin);


                clearSpareBtnCounter++;
                Button clearStructBtn = new Button
                {
                    Name = "clearStructBtn" + clearSpareBtnCounter,
                    Content = "Clear"
                };
                clearStructBtn.Click += new RoutedEventHandler(this.ClearStructBtn_click);
                clearStructBtn.Width = 50;
                clearStructBtn.Height = sp.Height - 5;
                clearStructBtn.HorizontalAlignment = HorizontalAlignment.Left;
                clearStructBtn.VerticalAlignment = VerticalAlignment.Top;
                clearStructBtn.Margin = new Thickness(10, 5, 0, 0);
                sp.Children.Add(clearStructBtn);

                structures_sp.Children.Add(sp);
            }
        }

        private void Type_cb_change(object sender, System.EventArgs e)
        {
            //not the most elegent code, but it works. Basically, it finds the combobox where the selection was changed and increments one additional child to get the add margin text box. Then it can change
            //the visibility of this textbox based on the sparing type selected for this structure
            ComboBox c = (ComboBox)sender;
            bool row = false;
            foreach (object obj in structures_sp.Children)
            {
                foreach (object obj1 in ((StackPanel)obj).Children)
                {
                    //the btn has a unique tag to it, so we can just loop through all children in the structures_sp children list and find which button is equivalent to our button
                    if (row)
                    {
                        if (c.SelectedItem.ToString() != "Mean Dose < Rx Dose") (obj1 as TextBox).Visibility = Visibility.Hidden;
                        else (obj1 as TextBox).Visibility = Visibility.Visible;
                        return;
                    }
                    if (obj1.Equals(c)) row = true;
                }
            }
        }

        //method to clear and individual row in the structure sparing list (i.e., remove a single structure)
        private void ClearStructBtn_click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int i = 0;
            int k = 0;
            foreach (object obj in structures_sp.Children)
            {
                foreach (object obj1 in ((StackPanel)obj).Children)
                {
                    //the btn has a unique tag to it, so we can just loop through all children in the structures_sp children list and find which button is equivalent to our button
                    if (obj1.Equals(btn)) k = i;
                }
                if (k > 0) break;
                i++;
            }

            //clear entire list if there are only two entries (header + 1 real entry). Otherwise, remove the row containing the structure of interest
            if (structures_sp.Children.Count < 3) Clear_spare_list();
            else structures_sp.Children.RemoveAt(k);
        }

        private void SSID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //clear sparing structure list
            Clear_spare_list();

            //clear optimization structure list
            Clear_optimization_parameter_list();

            //update selected structure set
            selectedSS = pi.StructureSets.FirstOrDefault(x => x.Id == SSID.SelectedItem.ToString());

            //update volumes in flash volume combobox with the structures from the current structure set
            flashVolume.Items.Clear();
            foreach (Structure s in selectedSS.Structures) flashVolume.Items.Add(s.Id);
        }

        private void Add_defaults_click(object sender, RoutedEventArgs e)
        {
            //copy the sparing structures in the settings.DefaultSpareStruct list to a temporary vector
            List<Tuple<string, string, double>> templateList = new List<Tuple<string, string, double>>(settings.DefaultSpareStruct);
            //add the case-specific sparing structures to the temporary list
            if (nonmyelo_chkbox.IsChecked.Value) 
                templateList = new List<Tuple<string, string, double>>(AddCaseSpecificSpareStructures(settings.NonMyeloSpareStruct,templateList));
            else if (myelo_chkbox.IsChecked.Value) 
                templateList = new List<Tuple<string, string, double>>(AddCaseSpecificSpareStructures(settings.MyeloSpareStruct, templateList));

            string missOutput = "";
            string emptyOutput = "";
            int missCount = 0;
            int emptyCount = 0;
            List<Tuple<string, string, double>> defaultList = new List<Tuple<string, string, double>> { };
            foreach (Tuple<string, string, double> itr in templateList)
            {
                //check to ensure the structures in the templateList vector are actually present in the selected structure set and are actually contoured. If they are, add them to the defaultList vector, which will be passed 
                //to the add_sp_volumes method
                if (!selectedSS.Structures.Where(x => x.Id.ToLower() == itr.Item1.ToLower()).Any())
                {
                    if (missCount == 0) missOutput = String.Format("Warning! The following default structures are missing from the selected structure list:\n");
                    missOutput += String.Format("{0}\n", itr.Item1);
                    missCount++;
                }
                else if (selectedSS.Structures.First(x => x.Id.ToLower() == itr.Item1.ToLower()).IsEmpty)
                {
                    if (emptyCount == 0) emptyOutput = String.Format("Warning! The following default structures are present but empty:\n");
                    emptyOutput += String.Format("{0}\n", itr.Item1);
                    emptyCount++;
                }
                else defaultList.Add(Tuple.Create(selectedSS.Structures.First(x => x.Id.ToLower() == itr.Item1.ToLower()).Id, itr.Item2, itr.Item3));
            }

            Clear_spare_list();
            Add_sp_volumes(selectedSS, defaultList);
            if (missCount > 0) MessageBox.Show(missOutput);
            if (emptyCount > 0) MessageBox.Show(emptyOutput);
        }

        //helper method to easily add sparing structures to a sparing structure list. The reason this is its own method is because of the logic used to include/remove sex-specific organs
        private List<Tuple<string, string, double>> AddCaseSpecificSpareStructures(List<Tuple<string, string, double>> caseSpareStruct, List<Tuple<string, string, double>> template)
        {
            foreach (Tuple<string, string, double> s in caseSpareStruct)
            {
                if (s.Item1.ToLower() == "ovaries" || s.Item1.ToLower() == "testes") { if ((pi.Sex == "Female" && s.Item1.ToLower() == "ovaries") || (pi.Sex == "Male" && s.Item1.ToLower() == "testes")) template.Add(s); }
                else template.Add(s);
            }
            return template;
        }

        //wipe the displayed list of sparing structures
        private void Clear_list_click(object sender, RoutedEventArgs e) { Clear_spare_list(); }

        private void Clear_spare_list()
        {
            firstSpareStruct = true;
            structures_sp.Children.Clear();
            clearSpareBtnCounter = 0;
        }

        private void GenerateStruct(object sender, RoutedEventArgs e)
        {
            //check that there are actually structures to spare in the sparing list
            if (structures_sp.Children.Count == 0)
            {
                MessageBox.Show("No structures present to generate tuning structures!");
                return;
            }

            //get the relevant flash parameters if the user requested to add flash to the target volume(s)
            double flashMargin = 0.0;
            if (useFlash)
            {
                if (!double.TryParse(flashMarginTB.Text, out flashMargin))
                {
                    MessageBox.Show("Error! Added flash margin is NaN! \nExiting!");
                    return;
                }
                //ESAPI has a limit on the margin for structure of 5.0 cm. The margin should always be positive (i.e., an outer margin)
                if (flashMargin < 0.0 || flashMargin > 5.0)
                {
                    MessageBox.Show("Error! Added flash margin is either < 0.0 or > 5.0 cm \nExiting!");
                    return;
                }
                if (flashType == "Local")
                {
                    //if flash type is local, grab an instance of the structure class associated with the selected structure 
                    flashStructure = selectedSS.Structures.First(x => x.Id.ToLower() == flashVolume.SelectedItem.ToString().ToLower());
                    if (flashStructure == null || flashStructure.IsEmpty)
                    {
                        MessageBox.Show("Error! Selected local flash structure is either null or empty! \nExiting!");
                        return;
                    }
                }

            }
            if (!double.TryParse(targetMarginTB.Text, out double targetMargin))
            {
                MessageBox.Show("Error! PTV margin from body is NaN! \nExiting!");
                return;
            }
            if (targetMargin < 0.0 || targetMargin > 5.0)
            {
                MessageBox.Show("Error! PTV margin from body is either < 0.0 or > 5.0 cm \nExiting!");
                return;
            }

            List<Tuple<string, string, double>> structureSpareList = new List<Tuple<string, string, double>> { };
            string structure = "";
            string spareType = "";
            double margin = -1000.0;
            bool firstCombo = true;
            bool headerObj = true;
            foreach (object obj in structures_sp.Children)
            {
                //skip over the header row
                if (!headerObj)
                {
                    foreach (object obj1 in ((StackPanel)obj).Children)
                    {
                        if (obj1.GetType() == typeof(ComboBox))
                        {
                            //first combo box is the structure and the second is the sparing type
                            if (firstCombo)
                            {
                                structure = (obj1 as ComboBox).SelectedItem.ToString();
                                firstCombo = false;
                            }
                            else spareType = (obj1 as ComboBox).SelectedItem.ToString();
                        }
                        //try to parse the margin value as a double
                        else if (obj1.GetType() == typeof(TextBox)) if (!string.IsNullOrWhiteSpace((obj1 as TextBox).Text)) double.TryParse((obj1 as TextBox).Text, out margin);
                    }
                    if (structure == "--select--" || spareType == "--select--")
                    {
                        MessageBox.Show("Error! \nStructure or Sparing Type not selected! \nSelect an option and try again");
                        return;
                    }
                    //margin will not be assigned from the default value (-1000) if the input is empty, a whitespace, or NaN
                    else if (margin == -1000.0)
                    {
                        MessageBox.Show("Error! \nEntered margin value is invalid! \nEnter a new margin and try again");
                        return;
                    }
                    //only add the current row to the structure sparing list if all the parameters were successful parsed
                    else structureSpareList.Add(Tuple.Create(structure, spareType, margin));
                    firstCombo = true;
                    margin = -1000.0;
                }
                else headerObj = false;
            }

            ///////////////////////////////////////
            //modified from iromero77's pull request #20
            // If Rx >= 13.2Gy, crop Lung+5mm instead of 3mm, suggest increasing cropping margin
            if (double.Parse(dosePerFx.Text) * double.Parse(numFx.Text) >= 1320)
            {
                if(structureSpareList.FirstOrDefault(x => x.Item1.ToLower() == "lungs" && x.Item2 == "Mean Dose < Rx Dose" && x.Item3 < 0.5) != null)
                {
                    confirmUI CUI = new confirmUI();
                    CUI.message.Text = "Prescribed dose is greater than 13.2 Gy!" + Environment.NewLine + "I recommend increasing the cropping margin to 5 mm (to ensure lung sparing)." + Environment.NewLine + Environment.NewLine + "Increase lung crop margin to 5mm?";
                    CUI.button1.Text = "No";
                    CUI.button2.Text = "Yes";
                    CUI.ShowDialog();
                    if(CUI.confirm)
                    {
                        int idx = structureSpareList.FindIndex(x => x.Item1.ToLower() == "lungs" & x.Item2.ToLower().Contains("mean") & x.Item3 == 0.3);
                        structureSpareList.RemoveAt(idx);
                        if (idx > structureSpareList.Count) structureSpareList.Add(new Tuple<string, string, double>("Lungs", "Mean Dose < Rx Dose", 0.5));
                        else structureSpareList.Insert(idx, new Tuple<string, string, double>("Lungs", "Mean Dose < Rx Dose", 0.5));
                        Clear_spare_list();
                        Add_sp_volumes(selectedSS, structureSpareList);
                    }
                }
            }
            ///////////////////////////////////////

            //create an instance of the generateTS class, passing the structure sparing list vector, the selected structure set, and if this is the scleroderma trial treatment regiment
            //The scleroderma trial contouring/margins are specific to the trial, so this trial needs to be handled separately from the generic VMAT treatment type
            VMATTBIautoPlan.GenerateTS generate;
            //overloaded constructor depending on if the user requested to use flash or not. If so, pass the relevant flash parameters to the generateTS class
            if (!useFlash) generate = new VMATTBIautoPlan.GenerateTS(settings.TS_Structures, settings.ScleroStructures, structureSpareList, selectedSS, targetMargin, false, settings.AllVMAT);
            else generate = new VMATTBIautoPlan.GenerateTS(settings.TS_Structures, settings.ScleroStructures, structureSpareList, selectedSS, targetMargin, false, settings.AllVMAT, useFlash, flashStructure, flashMargin);
            pi.BeginModifications();
            if (generate.GenerateStructures()) return;
            //does the structure sparing list need to be updated? This occurs when structures the user elected to spare with option of 'Mean Dose < Rx Dose' are high resolution. Since Eclipse can't perform
            //boolean operations on structures of two different resolutions, code was added to the generateTS class to automatically convert these structures to low resolution with the name of
            // '<original structure Id>_lowRes'. When these structures are converted to low resolution, the updateSparingList flag in the generateTS class is set to true to tell this class that the 
            //structure sparing list needs to be updated with the new low resolution structures.
            if (generate.updateSparingList)
            {
                Clear_spare_list();
                //update the structure sparing list in this class and update the structure sparing list displayed to the user in TS Generation tab
                structureSpareList = generate.spareStructList;
                Add_sp_volumes(selectedSS, structureSpareList);
            }
            if (generate.optParameters.Count() > 0) optParameters = generate.optParameters;
            numIsos = generate.numIsos;
            numVMATIsos = generate.numVMATIsos;
            isoNames = generate.isoNames;
            //if (settings.AllVMAT) extraIsos = generate.extraIsos;

            //get prescription
            if (double.TryParse(dosePerFx.Text, out double dose_perFx) && int.TryParse(numFx.Text, out int numFractions)) prescription = Tuple.Create(numFractions, new DoseValue(dose_perFx, DoseValue.DoseUnit.cGy));
            else
            {
                MessageBox.Show("Warning! Entered prescription is not valid! \nSetting number of fractions to 1 and dose per fraction to 0.1 cGy/fraction!");
                prescription = Tuple.Create(1, new DoseValue(0.1, DoseValue.DoseUnit.cGy));
            }

            //populate the beams and optimization tabs
            PopulateBeamsTab();
            if (optParameters.Count() > 0) PopulateOptimizationTab();
        }

        //stuff related to beam placement tab
        private void ContourOverlapChecked(object sender, RoutedEventArgs e)
        {
            if (contourOverlap_chkbox.IsChecked.Value)
            {
                contourOverlapLabel.Visibility = Visibility.Visible;
                contourOverlapTB.Visibility = Visibility.Visible;
            }
            else
            {
                contourOverlapLabel.Visibility = Visibility.Hidden;
                contourOverlapTB.Visibility = Visibility.Hidden;
            }
        }

        private void PopulateBeamsTab()
        {
            //default option to contour overlap between fields in adjacent isocenters and assign the resulting structures as targets
            contourOverlap_chkbox.IsChecked = settings.ContourOverlap;
            contourOverlapLabel.Visibility = Visibility.Visible;
            contourOverlapTB.Visibility = Visibility.Visible;
            contourOverlapTB.Text = settings.ContourFieldOverlapMargin;

            BEAMS_SP.Children.Clear();


            StackPanel sp = new StackPanel
            {
                Height = 30,
                Width = structures_sp.Width,
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 5)
            };

            //select linac (LA-16 or LA-17)
            Label linac = new Label
            {
                Content = "Linac:",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 208,
                FontSize = 14,
                Margin = new Thickness(0, 0, 10, 0)
            };
            sp.Children.Add(linac);

            ComboBox linac_cb = new ComboBox
            {
                Name = "linac_cb",
                Width = 80,
                Height = sp.Height - 5,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 65, 0)
            };
            if (settings.Linacs.Count() > 0) foreach (string s in settings.Linacs) linac_cb.Items.Add(s);
            else
            {
                enterMissingInfo linacName = new VMATTBIautoPlan.enterMissingInfo();
                linacName.title.Text = "Enter the name of the linac you want to use";
                linacName.info.Text = "Linac:";
                linacName.ShowDialog();
                if (!linacName.confirm) return;
                linac_cb.Items.Add(linacName.value.Text);
            }
            linac_cb.SelectedIndex = 0;
            linac_cb.HorizontalContentAlignment = HorizontalAlignment.Center;
            sp.Children.Add(linac_cb);

            BEAMS_SP.Children.Add(sp);

            //select energy (6X or 10X)
            sp = new StackPanel
            {
                Height = 30,
                Width = structures_sp.Width,
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 5)
            };

            Label energy = new Label
            {
                Content = "Beam energy:",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 215,
                FontSize = 14,
                Margin = new Thickness(0, 0, 10, 0)
            };
            sp.Children.Add(energy);

            ComboBox energy_cb = new ComboBox
            {
                Name = "energy_cb",
                Width = 70,
                Height = sp.Height - 5,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 65, 0)
            };
            if (settings.BeamEnergies.Count() > 0) foreach (string s in settings.BeamEnergies) energy_cb.Items.Add(s);
            else
            {
                enterMissingInfo energyName = new VMATTBIautoPlan.enterMissingInfo();
                energyName.title.Text = "Enter the photon beam energy you want to use";
                energyName.info.Text = "Energy:";
                energyName.ShowDialog();
                if (!energyName.confirm) return;
                energy_cb.Items.Add(energyName.value.Text);
            }
            energy_cb.SelectedIndex = 0;
            energy_cb.HorizontalContentAlignment = HorizontalAlignment.Center;
            sp.Children.Add(energy_cb);

            BEAMS_SP.Children.Add(sp);

            //subtract a beam from the first isocenter (head) if the user is NOT interested in sparing the brain
            //if (!optParameters.Where(x => (x.Item1.ToLower().Contains("brain") || x.Item1.ToLower().Contains("head"))).Any()) settings.BeamsPerIso[0]--;
            //subtract a beam from the second isocenter (chest/abdomen area) if the user is NOT interested in sparing the kidneys
            //if (!optParameters.Where(x => x.Item1.ToLower().Contains("kidneys")).Any()) settings.BeamsPerIso[1]--;

            //add iso names and suggested number of beams
            for (int i = 0; i < numIsos; i++)
            {
                sp = new StackPanel
                {
                    Height = 30,
                    Width = structures_sp.Width,
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(2)
                };

                Label iso = new Label
                {
                    Content = String.Format("Isocenter {0} <{1}>:", (i + 1).ToString(), isoNames.ElementAt(i)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 230,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                sp.Children.Add(iso);

                TextBox beams_tb = new TextBox
                {
                    Name = "beams_tb",
                    Width = 40,
                    Height = sp.Height - 7,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 80, 0),
                    Text = settings.BeamsPerIso[i].ToString(),
                    TextAlignment = TextAlignment.Center
                };
                sp.Children.Add(beams_tb);

                BEAMS_SP.Children.Add(sp);
            }
            numVMATisosTB.Text = numVMATIsos.ToString();

        }

        private void UpdateVMATisos_Click(object sender, RoutedEventArgs e)
        {
            if (!isoNames.Any()) MessageBox.Show("Error! Please generate the tuning structures before updating the requested number of VMAT isocenters!");
            else if (VMATplan != null) MessageBox.Show("Error! VMAT plan has already been generated! Cannot place beams again!");
            else if (!int.TryParse(numVMATisosTB.Text, out int tmp)) MessageBox.Show("Error! Requested number of VMAT isocenters is NaN! Please try again!");
            else if (tmp == numVMATIsos) MessageBox.Show("Warning! Requested number of VMAT isocenters = current number of VMAT isocenters!");
            else if (!settings.AllVMAT && (tmp < 2 || tmp > 4)) MessageBox.Show("Error! Requested number of VMAT isocenters is less than 2 or greater than 4! Please try again!");
            else if (settings.AllVMAT && (tmp < 2 || tmp > 7)) MessageBox.Show("Error! Requested number of VMAT isocenters is less than 2 or greater than 6! Please try again!");
            else
            {
                if (!optParameters.Where(x => x.Item1.ToLower().Contains("brain")).Any()) settings.BeamsPerIso[0]++;
                numIsos += tmp - numVMATIsos;
                numVMATIsos = tmp;
                isoNames = new List<string>(new IsoNameHelper().GetIsoNames(numVMATIsos, numIsos));
                PopulateBeamsTab();
            }

        }

        private void Place_beams_Click(object sender, RoutedEventArgs e)
        {
            if (BEAMS_SP.Children.Count == 0)
            {
                MessageBox.Show("No isocenters present to place beams!");
                return;
            }

            int count = 0;
            bool firstCombo = true;
            string chosenLinac = "";
            string chosenEnergy = "";
            int[] numBeams = new int[numIsos];
            foreach (object obj in BEAMS_SP.Children)
            {
                foreach (object obj1 in ((StackPanel)obj).Children)
                {
                    if (obj1.GetType() == typeof(ComboBox))
                    {
                        //similar code to parsing the structure sparing list
                        if (firstCombo)
                        {
                            chosenLinac = (obj1 as ComboBox).SelectedItem.ToString();
                            firstCombo = false;
                        }
                        else chosenEnergy = (obj1 as ComboBox).SelectedItem.ToString();
                    }
                    if (obj1.GetType() == typeof(TextBox))
                    {
                        // MessageBox.Show(count.ToString());
                        if (!int.TryParse((obj1 as TextBox).Text, out numBeams[count]))
                        {
                            MessageBox.Show(String.Format("Error! \nNumber of beams entered in iso {0} is NaN!", isoNames.ElementAt(count)));
                            return;
                        }
                        else if (numBeams[count] < 1)
                        {
                            MessageBox.Show(String.Format("Error! \nNumber of beams entered in iso {0} is < 1!", isoNames.ElementAt(count)));
                            return;
                        }
                        else if (numBeams[count] > 4)
                        {
                            MessageBox.Show(String.Format("Error! \nNumber of beams entered in iso {0} is > 4!", isoNames.ElementAt(count)));
                            return;
                        }
                        count++;
                    }
                }
            }

            //AP/PA stuff (THIS NEEDS TO GO AFTER THE ABOVE CHECKS!). Ask the user if they want to split the AP/PA isocenters into two plans if there are two AP/PA isocenters
            bool singleAPPAplan = true;
            if (numIsos - numVMATIsos == 2)
            {
                selectItem SUI = new VMATTBIautoPlan.selectItem();
                SUI.title.Text = "What should I do with the AP/PA isocenters?" + Environment.NewLine + Environment.NewLine + Environment.NewLine + "Put them in:";
                SUI.title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                SUI.itemCombo.Items.Add("One plan");
                SUI.itemCombo.Items.Add("Separate plans");
                SUI.itemCombo.Text = "One plan";
                SUI.ShowDialog();
                if (!SUI.confirm) return;
                //get the option the user chose from the combobox
                if (SUI.itemCombo.SelectedItem.ToString() == "Separate plans") singleAPPAplan = false;
            }

            //Added code to account for the scenario where the user either requested or did not request to contour the overlap between fields in adjacent isocenters
            VMATTBIautoPlan.PlaceBeams place;
            if (contourOverlap_chkbox.IsChecked.Value)
            {
                //ensure the value entered in the added margin text box for contouring field overlap is a valid double
                if (!double.TryParse(contourOverlapTB.Text, out double contourOverlapMargin))
                {
                    MessageBox.Show("Error! The entered added margin for the contour overlap text box is NaN! Please enter a valid number and try again!");
                    return;
                }
                //convert from mm to cm
                contourOverlapMargin *= 10.0;
                //overloaded constructor for the placeBeams class
                place = new PlaceBeams(settings.AllVMAT, settings.ExtraIsos, settings.CheckTTCollision, selectedSS, settings.CourseId, prescription, isoNames, numIsos, numVMATIsos, singleAPPAplan, numBeams, settings.CollRot, settings.JawPos, chosenLinac, chosenEnergy, settings.CalculationModel, settings.OptimizationModel, settings.UseGPUdose, settings.UseGPUoptimization, settings.MRrestartLevel, useFlash, settings.ContourOverlapMargin);
            }
            else place = new PlaceBeams(settings.AllVMAT, settings.ExtraIsos, settings.CheckTTCollision, selectedSS, settings.CourseId, prescription, isoNames, numIsos, numVMATIsos, singleAPPAplan, numBeams, settings.CollRot, settings.JawPos, chosenLinac, chosenEnergy, settings.CalculationModel, settings.OptimizationModel, settings.UseGPUdose, settings.UseGPUoptimization, settings.MRrestartLevel, useFlash);

            VMATplan = place.Generate_beams();
            if (VMATplan == null) return;

            //if the user elected to contour the overlap between fields in adjacent isocenters, get this list of structures from the placeBeams class and copy them to the jnxs vector
            if (contourOverlap_chkbox.IsChecked.Value) jnxs = place.jnxs;

            //if the user requested to contour the overlap between fields in adjacent VMAT isocenters, repopulate the optimization tab (will include the newly added field junction structures)!
            if (contourOverlap_chkbox.IsChecked.Value) PopulateOptimizationTab();
        }

        //stuff related to optimization setup tab
        private void PopulateOptimizationTab()
        {
            List<Tuple<string, string, double, double, int>> tmp = new List<Tuple<string, string, double, double, int>> { };
            List<Tuple<string, string, double, double, int>> defaultList = new List<Tuple<string, string, double, double, int>> { };

            //non-meyloabalative regime
            if (nonmyelo_chkbox.IsChecked.Value) tmp = settings.OptConstDefaultNonMyelo;
            //meylo-abalative regime
            else if (myelo_chkbox.IsChecked.Value) tmp = settings.OptConstDefaultMyelo;
            //scleroderma trial regiment
            //no treatment template selected => scale optimization objectives by ratio of entered Rx dose to closest template treatment Rx dose
            else if (prescription != null)
            {
                double RxDose = prescription.Item2.Dose * prescription.Item1;
                double baseDose;
                List<Tuple<string, string, double, double, int>> dummy = new List<Tuple<string, string, double, double, int>> { };
                //use optimization objects of the closer of the two default regiments (6-18-2021)
                if (Math.Pow(RxDose - (settings.NonMyeloNumFx * settings.NonMyeloDosePerFx), 2) <= Math.Pow(RxDose - (settings.MyeloNumFx * settings.MyeloDosePerFx), 2))
                {
                    dummy = settings.OptConstDefaultNonMyelo;
                    baseDose = settings.NonMyeloDosePerFx * settings.NonMyeloNumFx;
                }
                else
                {
                    dummy = settings.OptConstDefaultMyelo;
                    baseDose = settings.MyeloDosePerFx * settings.MyeloNumFx;
                }
                foreach (Tuple<string, string, double, double, int> opt in dummy) tmp.Add(Tuple.Create(opt.Item1, opt.Item2, opt.Item3 * (RxDose / baseDose), opt.Item4, opt.Item5));
            }
            else
            {
                MessageBox.Show("Error: No template treatment regiment selected AND entered Rx dose is NOT valid! \nYou must enter the optimization constraints manually!");
                return;
            }

            if (optParameters.Any())
            {
                //there are items in the optParameters vector, indicating the TSgeneration was performed. Use the values in the OptParameters vector.
                foreach (Tuple<string, string, double, double, int> opt in tmp)
                {
                    //always add PTV objectives to optimization objectives list
                    if (opt.Item1.Contains("PTV"))
                    {
                        //if user requested to add flash, optimize on the TS_PTV_FLASH structure instead of the TS_PTV_VMAT structure!
                        if (useFlash) defaultList.Add(Tuple.Create(selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == "ts_ptv_flash").Id, opt.Item2, opt.Item3, opt.Item4, opt.Item5));
                        else defaultList.Add(opt);
                    }
                    //only add template optimization objectives for each structure to default list if that structure is present in the selected structure set and contoured
                    else if (optParameters.Where(x => x.Item1.ToLower().Contains(opt.Item1.ToLower())).Any())
                    {
                        //12-22-2020 coded added to account for the situation where the structure selected for sparing had to be converted to a low resolution structure
                        if (selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == (opt.Item1 + "_lowRes").ToLower()) != null && !selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == (opt.Item1 + "_lowRes").ToLower()).IsEmpty) defaultList.Add(Tuple.Create(optParameters.FirstOrDefault(x => x.Item1.ToLower() == (opt.Item1 + "_lowRes").ToLower()).Item1, opt.Item2, opt.Item3, opt.Item4, opt.Item5));
                        else if (!selectedSS.Structures.First(x => x.Id.ToLower() == opt.Item1.ToLower()).IsEmpty) defaultList.Add(Tuple.Create(optParameters.FirstOrDefault(x => x.Item1.ToLower() == opt.Item1.ToLower()).Item1, opt.Item2, opt.Item3, opt.Item4, opt.Item5));
                    }
                }
            }
            else
            {
                //No items in the optParameters vector, indicating the user just wants to set/reset the optimization parameters. 
                //In this case, just search through the structure set to see if any of the contoured structure IDs match the structures in the optimization parameter templates
                if (selectedSS.Structures.Where(x => x.Id.ToLower().Contains("ptv")).Any())
                {
                    foreach (Tuple<string, string, double, double, int> opt in tmp)
                    {
                        if (opt.Item1.Contains("PTV"))
                        {
                            //The user needs to check the use flash checkbox for this code to consider the ts_flash structures
                            if (useFlash) defaultList.Add(Tuple.Create(selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == "ts_ptv_flash").Id, opt.Item2, opt.Item3, opt.Item4, opt.Item5));
                            else defaultList.Add(opt);
                        }
                        else if (selectedSS.Structures.Where(x => x.Id.ToLower().Contains(opt.Item1.ToLower())).Any())
                        {
                            //12-22-2020 coded added to account for the situation where the structure selected for sparing had to be previously converted to a low resolution structure using this script
                            if (selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == (opt.Item1 + "_lowRes").ToLower()) != null && !selectedSS.Structures.First(x => x.Id.ToLower() == (opt.Item1 + "_lowRes").ToLower()).IsEmpty) defaultList.Add(Tuple.Create(selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == (opt.Item1 + "_lowRes").ToLower()).Id, opt.Item2, opt.Item3, opt.Item4, opt.Item5));
                            else if (!selectedSS.Structures.First(x => x.Id.ToLower() == opt.Item1.ToLower()).IsEmpty) defaultList.Add(Tuple.Create(selectedSS.Structures.FirstOrDefault(x => x.Id.ToLower() == opt.Item1.ToLower()).Id, opt.Item2, opt.Item3, opt.Item4, opt.Item5));

                        }
                        //else if (selectedSS.Structures.Where(x => x.Id.ToLower() == opt.Item1.ToLower()).Any() && !selectedSS.Structures.First(x => x.Id.ToLower() == opt.Item1.ToLower()).IsEmpty) defaultList.Add(opt);
                    }
                }
                else
                {
                    MessageBox.Show("Warning! No PTV structures in the selected structure set! Add a PTV structure and try again!");
                    return;
                }
            }

            //check if the user requested to contour the overlap between fields in adjacent isocenters and also check if there are any structures in the junction structure stack (jnxs)
            if (contourOverlap_chkbox.IsChecked.Value || jnxs.Any())
            {
                //we want to insert the optimization constraints for these junction structure right after the ptv constraints, so find the last index of the target ptv structure and insert
                //the junction structure constraints directly after the target structure constraints
                int index = defaultList.FindLastIndex(x => x.Item1.ToLower().Contains("ptv"));
                foreach (Structure s in jnxs)
                {
                    //per Nataliya's instructions, add both a lower and upper constraint to the junction volumes. Make the constraints match those of the ptv target
                    defaultList.Insert(++index, new Tuple<string, string, double, double, int>(s.Id, "Lower", prescription.Item2.Dose * prescription.Item1, 100.0, 100));
                    defaultList.Insert(++index, new Tuple<string, string, double, double, int>(s.Id, "Upper", prescription.Item2.Dose * prescription.Item1 * 1.01, 0.0, 100));
                }
            }

            //clear the current list of optimization objectives
            Clear_optimization_parameter_list();
            //add the default list of optimization objectives to the displayed list of optimization objectives
            Add_opt_volumes(selectedSS, defaultList);
//
        }

        private void ScanSS_Click(object sender, RoutedEventArgs e)
        {
            //get prescription
            if (double.TryParse(dosePerFx.Text, out double dose_perFx) && int.TryParse(numFx.Text, out int numFractions)) prescription = Tuple.Create(numFractions, new DoseValue(dose_perFx, DoseValue.DoseUnit.cGy));
            else
            {
                MessageBox.Show("Warning! Entered prescription is not valid! \nSetting number of fractions to 1 and dose per fraction to 0.1 cGy/fraction!");
                prescription = Tuple.Create(1, new DoseValue(0.1, DoseValue.DoseUnit.cGy));
            }
            if (selectedSS.Structures.Where(x => x.Id.ToLower().Contains("ts_jnx")).Any()) jnxs = selectedSS.Structures.Where(x => x.Id.ToLower().Contains("ts_jnx")).ToList();

            PopulateOptimizationTab();
        }

        private void SetOptConst_Click(object sender, RoutedEventArgs e)
        {
            if (opt_parameters.Children.Count == 0)
            {
                MessageBox.Show("No optimization parameters present to assign to VMAT TBI plan!");
                return;
            }

            //get constraints
            List<Tuple<string, string, double, double, int>> optParametersList = new List<Tuple<string, string, double, double, int>> { };
            string structure = "";
            string constraintType = "";
            double dose = -1.0;
            double vol = -1.0;
            int priority = -1;
            int txtbxNum = 1;
            bool firstCombo = true;
            bool headerObj = true;
            foreach (object obj in opt_parameters.Children)
            {
                //skip over header row
                if (!headerObj)
                {
                    foreach (object obj1 in ((StackPanel)obj).Children)
                    {
                        if (obj1.GetType() == typeof(ComboBox))
                        {
                            if (firstCombo)
                            {
                                //first combobox is the structure
                                structure = (obj1 as ComboBox).SelectedItem.ToString();
                                firstCombo = false;
                            }
                            //second combobox is the constraint type
                            else constraintType = (obj1 as ComboBox).SelectedItem.ToString();
                        }
                        else if (obj1.GetType() == typeof(TextBox))
                        {
                            if (!string.IsNullOrWhiteSpace((obj1 as TextBox).Text))
                            {
                                //first text box is the volume percentage
                                if (txtbxNum == 1) double.TryParse((obj1 as TextBox).Text, out vol);
                                //second text box is the dose constraint
                                else if (txtbxNum == 2) double.TryParse((obj1 as TextBox).Text, out dose);
                                //third text box is the priority
                                else int.TryParse((obj1 as TextBox).Text, out priority);
                            }
                            txtbxNum++;
                        }
                    }
                    //do some checks to ensure the integrity of the data
                    if (structure == "--select--" || constraintType == "--select--")
                    {
                        MessageBox.Show("Error! \nStructure or Sparing Type not selected! \nSelect an option and try again");
                        return;
                    }
                    else if (dose == -1.0 || vol == -1.0 || priority == -1.0)
                    {
                        MessageBox.Show("Error! \nDose, volume, or priority values are invalid! \nEnter new values and try again");
                        return;
                    }
                    //if the row of data passes the above checks, add it the optimization parameter list
                    else optParametersList.Add(Tuple.Create(structure, constraintType, dose, vol, priority));
                    //reset the values of the variables used to parse the data
                    firstCombo = true;
                    txtbxNum = 1;
                    dose = -1.0;
                    vol = -1.0;
                    priority = -1;
                }
                else headerObj = false;
            }

            if (VMATplan == null)
            {
                if (VMS.TPS.Script.GetScriptContext().ExternalPlanSetup != null && VMS.TPS.Script.GetScriptContext().ExternalPlanSetup.Id.ToLower() == "_vmat tbi") VMATplan = VMS.TPS.Script.GetScriptContext().ExternalPlanSetup;
                else
                {
                    //a plan has not been assigned to the variable VMATplan, indicating that beam placement was not performed. 
                    //In this case, the user likely wants to set/reset the optimization constraints for an existing plan
                    confirmUI CUI = new VMATTBIautoPlan.confirmUI();
                    CUI.message.Text = "VMAT plan has NOT been created by this script instance!" + Environment.NewLine + Environment.NewLine + "Search for existing VMAT plan named 'VMAT TBI' and continue?!";
                    CUI.ShowDialog();
                    if (!CUI.confirm) return;
                    //search for a course named VMAT TBI. If it is found, search for a plan named _VMAT TBI inside the VMAT TBI course. If neither are found, throw an error and return
                    if (!pi.Courses.Where(x => x.Id == settings.CourseId).Any() || !pi.Courses.First(x => x.Id == settings.CourseId).PlanSetups.Where(x => x.Id == "_VMAT TBI").Any())
                    {
                        MessageBox.Show(String.Format("No course or plan named '{0}' and '_VMAT TBI' found! Exiting...", settings.CourseId));
                        return;
                    }
                    //if both are found, grab an instance of that plan
                    VMATplan = pi.Courses.First(x => x.Id == settings.CourseId).PlanSetups.First(x => x.Id == "_VMAT TBI") as ExternalPlanSetup;
                }
                pi.BeginModifications();
            }
            if (VMATplan.OptimizationSetup.Objectives.Count() > 0)
            {
                //the plan has existing objectives, which need to be removed be assigning the new objectives
                foreach (OptimizationObjective o in VMATplan.OptimizationSetup.Objectives) VMATplan.OptimizationSetup.RemoveObjective(o);
            }
            foreach (Tuple<string, string, double, double, int> opt in optParametersList)
            {
                //assign the constraints to the plan. I haven't found a use for the exact constraint yet, so I just wrote the script to throw a warning if the exact constraint was selected (that row of data will NOT be
                //assigned to the VMAT plan)
                if (opt.Item2 == "Upper") VMATplan.OptimizationSetup.AddPointObjective(VMATplan.StructureSet.Structures.First(x => x.Id == opt.Item1), OptimizationObjectiveOperator.Upper, new DoseValue(opt.Item3, DoseValue.DoseUnit.cGy), opt.Item4, (double)opt.Item5);
                else if (opt.Item2 == "Lower") VMATplan.OptimizationSetup.AddPointObjective(VMATplan.StructureSet.Structures.First(x => x.Id == opt.Item1), OptimizationObjectiveOperator.Lower, new DoseValue(opt.Item3, DoseValue.DoseUnit.cGy), opt.Item4, (double)opt.Item5);
                else if (opt.Item2 == "Mean") VMATplan.OptimizationSetup.AddMeanDoseObjective(VMATplan.StructureSet.Structures.First(x => x.Id == opt.Item1), new DoseValue(opt.Item3, DoseValue.DoseUnit.cGy), (double)opt.Item5);
                else if (opt.Item2 == "Exact") MessageBox.Show("Script not setup to handle exact dose constraints!");
                else MessageBox.Show("Constraint type not recognized!");
            }
            //turn on jaw tracking
            try { VMATplan.OptimizationSetup.UseJawTracking = true; }
            catch (Exception except) { MessageBox.Show(String.Format("Warning! Could not set jaw tracking to true for VMAT plan because: {0}\nJaw tacking will not be enabled!", except.Message)); }
            //set auto NTO priority to zero (i.e., shut it off). It has to be done this way because every plan created in ESAPI has an instance of an automatic NTO, which CAN'T be deleted.
            VMATplan.OptimizationSetup.AddAutomaticNormalTissueObjective(0.0);

            string message = "Optimization objectives have been successfully set! " +
                    "\n\nPlease review the generated structures, placed isocenters, placed beams, and optimization parameters! " +
                    "\n\nOnce you are satisified, save the plan, close the patient, and launch the optimization loop executable!";
            if (optParametersList.Where(x => x.Item1.ToLower().Contains("_lowres")).Any()) message += "\n\nBE SURE TO VERIFY THE ACCURACY OF THE GENERATED LOW-RESOLUTION CONTOURS!";
            if (numIsos != 0 && numIsos != numVMATIsos)
            {
                //VMAT only TBI plan was created with the script in this instance info or the user wants to only set the optimization constraints
                message += "\n\nFor the AP/PA Legs plan, be sure to change the orientation from head-first supine to feet-first supine!";
            }
            MessageBox.Show(message);
        }

        private void Add_constraint_Click(object sender, RoutedEventArgs e)
        {
            Add_opt_volumes(selectedSS, new List<Tuple<string, string, double, double, int>> { Tuple.Create("--select--", "--select--", 0.0, 0.0, 0) });
            optParamScroller.ScrollToBottom();
        }

        private void Add_opt_header()
        {
            StackPanel sp1 = new StackPanel
            {
                Height = 30,
                Width = structures_sp.Width,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 0, 5, 5)
            };

            Label strName = new Label
            {
                Content = "Structure",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 110,
                FontSize = 14,
                Margin = new Thickness(27, 0, 0, 0)
            };

            Label spareType = new Label
            {
                Content = "Constraint",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 90,
                FontSize = 14,
                Margin = new Thickness(2, 0, 0, 0)
            };

            Label volLabel = new Label
            {
                Content = "V (%)",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 60,
                FontSize = 14,
                Margin = new Thickness(18, 0, 0, 0)
            };

            Label doseLabel = new Label
            {
                Content = "D (cGy)",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 60,
                FontSize = 14,
                Margin = new Thickness(3, 0, 0, 0)
            };

            Label priorityLabel = new Label
            {
                Content = "Priority",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 65,
                FontSize = 14,
                Margin = new Thickness(13, 0, 0, 0)
            };

            sp1.Children.Add(strName);
            sp1.Children.Add(spareType);
            sp1.Children.Add(volLabel);
            sp1.Children.Add(doseLabel);
            sp1.Children.Add(priorityLabel);
            opt_parameters.Children.Add(sp1);

            firstOptStruct = false;
        }

        private void Add_opt_volumes(StructureSet selectedSS, List<Tuple<string, string, double, double, int>> defaultList)
        {
            if (firstOptStruct) Add_opt_header();
            for (int i = 0; i < defaultList.Count; i++)
            {
                StackPanel sp = new StackPanel
                {
                    Height = 30,
                    Width = opt_parameters.Width,
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5)
                };

                ComboBox opt_str_cb = new ComboBox
                {
                    Name = "opt_str_cb",
                    Width = 120,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, 5, 0, 0)
                };

                opt_str_cb.Items.Add("--select--");
                //this code is used to fix the issue where the structure exists in the structure set, but doesn't populate as the default option in the combo box.
                int index = 0;
                //j is initially 1 because we already added "--select--" to the combo box 
                int j = 1;
                foreach (Structure s in selectedSS.Structures)
                {
                    opt_str_cb.Items.Add(s.Id);
                    if (s.Id.ToLower() == defaultList[i].Item1.ToLower()) index = j;
                    j++;
                }
                opt_str_cb.SelectedIndex = index;
                opt_str_cb.HorizontalContentAlignment = HorizontalAlignment.Center;
                sp.Children.Add(opt_str_cb);

                ComboBox constraint_cb = new ComboBox
                {
                    Name = "type_cb",
                    Width = 100,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, 5, 0, 0),
                    Text = defaultList[i].Item2,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                string[] types = new string[] { "--select--", "Upper", "Lower", "Mean", "Exact" };
                foreach (string s in types) constraint_cb.Items.Add(s);
                constraint_cb.Text = defaultList[i].Item2;
                constraint_cb.HorizontalContentAlignment = HorizontalAlignment.Center;
                sp.Children.Add(constraint_cb);

                //the order of the dose and volume values are switched when they are displayed to the user. This way, the optimization objective appears to the user as it would in the optimization workspace.
                //However, due to the way ESAPI assigns optimization objectives via VMATplan.OptimizationSetup.AddPointObjective, they need to be stored in the order listed in the templates above
                TextBox dose_tb = new TextBox
                {
                    Name = "dose_tb",
                    Width = 65,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, 5, 0, 0),
                    Text = String.Format("{0:0.#}", defaultList[i].Item4),
                    TextAlignment = TextAlignment.Center
                };
                sp.Children.Add(dose_tb);

                TextBox vol_tb = new TextBox
                {
                    Name = "vol_tb",
                    Width = 70,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, 5, 0, 0),
                    Text = String.Format("{0:0.#}", defaultList[i].Item3),
                    TextAlignment = TextAlignment.Center
                };
                sp.Children.Add(vol_tb);

                TextBox priority_tb = new TextBox
                {
                    Name = "priority_tb",
                    Width = 65,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(5, 5, 0, 0),
                    Text = Convert.ToString(defaultList[i].Item5),
                    TextAlignment = TextAlignment.Center
                };
                sp.Children.Add(priority_tb);

                clearOptBtnCounter++;
                Button clearOptStructBtn = new Button
                {
                    Name = "clearOptStructBtn" + clearOptBtnCounter,
                    Content = "Clear",
                    Width = 50,
                    Height = sp.Height - 5,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10, 5, 0, 0)
                };
                clearOptStructBtn.Click += new RoutedEventHandler(this.ClearOptStructBtn_click);

                sp.Children.Add(clearOptStructBtn);

                opt_parameters.Children.Add(sp);
            }
        }

        private void Clear_optParams_Click(object sender, RoutedEventArgs e) { Clear_optimization_parameter_list(); }

        private void Clear_optimization_parameter_list()
        {
            firstOptStruct = true;
            opt_parameters.Children.Clear();
            clearOptBtnCounter = 0;
        }

        private void ClearOptStructBtn_click(object sender, EventArgs e)
        {
            //same deal as the clear sparing structure button (clearStructBtn_click)
            Button btn = (Button)sender;
            int i = 0;
            int k = 0;
            foreach (object obj in opt_parameters.Children)
            {
                foreach (object obj1 in ((StackPanel)obj).Children)
                {
                    if (obj1.Equals(btn)) k = i;
                }
                if (k > 0) break;
                i++;
            }

            //clear entire list if there are only two entries (header + 1 real entry)
            if (opt_parameters.Children.Count < 3) Clear_optimization_parameter_list();
            else opt_parameters.Children.RemoveAt(k);
        }

        // Sclero prescription is not used in the script. Commented out for now
        /*
        private void Sclero_chkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (sclero_chkbox.IsChecked.Value)
            {
                if (nonmyelo_chkbox.IsChecked.Value) nonmyelo_chkbox.IsChecked = false;
                if (myelo_chkbox.IsChecked.Value) myelo_chkbox.IsChecked = false;
                //first arguement is the dose perfraction and the second argument is the number of fractions
                setPresciptionInfo(scleroDosePerFx, scleroNumFx);
            }
            else if (!nonmyelo_chkbox.IsChecked.Value && !myelo_chkbox.IsChecked.Value && (dosePerFx.Text == scleroDosePerFx.ToString() && numFx.Text == scleroNumFx.ToString()))
            {
                dosePerFx.Text = "";
                numFx.Text = "";
                if(settings.UseFlashByDefault) flash_chkbox.IsChecked = false;
                updateUseFlash();
            }
        }
        */
        private void Myelo_chkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (myelo_chkbox.IsChecked.Value)
            {
                if (nonmyelo_chkbox.IsChecked.Value) nonmyelo_chkbox.IsChecked = false;
                // if (sclero_chkbox.IsChecked.Value) sclero_chkbox.IsChecked = false;
                SetPresciptionInfo(settings.MyeloDosePerFx, settings.MyeloNumFx);
            }
            else if (!nonmyelo_chkbox.IsChecked.Value && dosePerFx.Text == settings.MyeloDosePerFx.ToString() && numFx.Text == settings.MyeloNumFx.ToString())
            {
                dosePerFx.Text = "";
                numFx.Text = "";
                if (settings.UseFlashByDefault) flash_chkbox.IsChecked = false;
                UpdateUseFlash();
            }
        }

        private void NonMyelo_chkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (nonmyelo_chkbox.IsChecked.Value)
            {
                if (myelo_chkbox.IsChecked.Value) myelo_chkbox.IsChecked = false;
                SetPresciptionInfo(settings.NonMyeloDosePerFx, settings.NonMyeloNumFx);
            }
            else if (!myelo_chkbox.IsChecked.Value && dosePerFx.Text == settings.MyeloDosePerFx.ToString() && numFx.Text == settings.MyeloNumFx.ToString())
            {
                dosePerFx.Text = "";
                numFx.Text = "";
                if (settings.UseFlashByDefault) flash_chkbox.IsChecked = false;
                UpdateUseFlash();
            }
        }

        bool waitToUpdate = false;
        private void SetPresciptionInfo(double dose_perFx, int num_Fx)
        {
            if (dosePerFx.Text != dose_perFx.ToString() && numFx.Text != num_Fx.ToString()) waitToUpdate = true;
            dosePerFx.Text = dose_perFx.ToString();
            numFx.Text = num_Fx.ToString();
        }

        private void NumFx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(numFx.Text, out int newNumFx)) Rx.Text = "";
            else if (newNumFx < 1)
            {
                MessageBox.Show("Error! The number of fractions must be non-negative integer and greater than zero!");
                Rx.Text = "";
            }
            else ResetRxDose();
        }

        private void DosePerFx_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(dosePerFx.Text, out double newDoseFx)) Rx.Text = "";
            else if (newDoseFx <= 0)
            {
                MessageBox.Show("Error! The dose per fraction must be a number and non-negative!");
                Rx.Text = "";
            }
            else ResetRxDose();
        }

        private void ResetRxDose()
        {
            if (waitToUpdate) waitToUpdate = false;
            else if (int.TryParse(numFx.Text, out int newNumFx) && double.TryParse(dosePerFx.Text, out double newDoseFx))
            {
                Rx.Text = (newNumFx * newDoseFx).ToString();
                if (settings.UseFlashByDefault) flash_chkbox.IsChecked = true;
                UpdateUseFlash();
                if (myelo_chkbox.IsChecked.Value && newNumFx * newDoseFx != settings.MyeloDosePerFx * settings.MyeloNumFx) myelo_chkbox.IsChecked = false;
                else if (nonmyelo_chkbox.IsChecked.Value && newNumFx * newDoseFx != settings.NonMyeloDosePerFx * settings.MyeloNumFx) nonmyelo_chkbox.IsChecked = false;
            }
        }

        //methods related to plan preparation
        private void GenerateShiftNote_Click(object sender, RoutedEventArgs e)
        {
            if (prep == null)
            {
                //this method assumes no prior knowledge, so it will have to retrive the number of isocenters (vmat and total) and isocenter names explicitly
                Course c = pi.Courses.FirstOrDefault(x => x.Id.ToLower() == settings.CourseId.ToLower());
                ExternalPlanSetup vmatPlan = null;
                IEnumerable<ExternalPlanSetup> appaPlan = new List<ExternalPlanSetup> { };
                if (c == null)
                {
                    //vmat tbi course not found. Dealbreaker, exit method
                    MessageBox.Show(String.Format("{0} course not found! Exiting!",settings.CourseId));
                    return;
                }
                else
                {
                    //always try and get the AP/PA plans (it's ok if it returns null). NOTE: Nataliya sometimes separates the _legs plan into two separate plans for planning PRIOR to running the optimization loop
                    //therefore, look for all external beam plans that contain the string 'legs'. If 3 plans are found, one of them is the original '_Legs' plan, so we can exculde that from the list
                    appaPlan = c.ExternalPlanSetups.Where(x => x.Id.ToLower().Contains("legs"));
                    if (appaPlan.Count() > 2) appaPlan = c.ExternalPlanSetups.Where(x => x.Id.ToLower().Contains("legs")).Where(x => x.Id.ToLower() != "_legs").OrderBy(o => int.Parse(o.Id.Substring(0, 2).ToString()));
                    //get all plans in the course that don't contain the string 'legs' in the plan ID. If more than 1 exists, prompt the user to select the plan they want to prep
                    IEnumerable<ExternalPlanSetup> plans = c.ExternalPlanSetups.Where(x => !x.Id.ToLower().Contains("legs"));
                    if (plans.Count() > 1)
                    {
                        selectItem SUI = new VMATTBIautoPlan.selectItem();
                        SUI.title.Text = String.Format("Multiple plans found in {0} course!" + Environment.NewLine + "Please select a plan to prep!",settings.CourseId);
                        foreach (ExternalPlanSetup p in plans) SUI.itemCombo.Items.Add(p.Id);
                        SUI.itemCombo.Text = VMS.TPS.Script.GetScriptContext().ExternalPlanSetup.Id;
                        SUI.ShowDialog();
                        if (!SUI.confirm) return;
                        //get the plan the user chose from the combobox
                        vmatPlan = c.ExternalPlanSetups.FirstOrDefault(x => x.Id == SUI.itemCombo.SelectedItem.ToString());
                    }
                    else
                    {
                        //course found and only one or fewer plans inside course with Id != "_Legs", get vmat and ap/pa plans
                        vmatPlan = c.ExternalPlanSetups.FirstOrDefault(x => x.Id.ToLower() == "_vmat tbi");
                    }
                    if (vmatPlan == null)
                    {
                        //vmat plan not found. Dealbreaker, exit method
                        MessageBox.Show("VMAT plan not found! Exiting!");
                        return;
                    }
                }

                //create an instance of the planPep class and pass it the vmatPlan and appaPlan objects as arguments. Get the shift note for the plan of interest
                prep = new VMATTBIautoPlan.PlanPrep(vmatPlan, appaPlan);
            }
            if (prep.GetShiftNote()) return;

            //let the user know this step has been completed (they can now do the other steps like separate plans and calculate dose)
            shiftTB.Background = System.Windows.Media.Brushes.ForestGreen;
            shiftTB.Text = "YES";
        }

        private void SeparatePlans_Click(object sender, RoutedEventArgs e)
        {
            //The shift note has to be retrieved first! Otherwise, we don't have instances of the plan objects
            if (shiftTB.Text != "YES")
            {
                var messageBoxResult = MessageBox.Show("no shift note", "Please generate the shift note before separating the plans! \n Continue?", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                    return;
            }

            //separate the plans
            pi.BeginModifications();
            if (prep.Separate()) return;

            //let the user know this step has been completed
            separateTB.Background = System.Windows.Media.Brushes.ForestGreen;
            separateTB.Text = "YES";

            //if flash was removed, display the calculate dose button (to remove flash, the script had to wipe the dose in the original plan)
            if (prep.flashRemoved)
            {
                calcDose.Visibility = Visibility.Visible;
                calcDoseTB.Visibility = Visibility.Visible;
            }
        }

        private void CalcDose_Click(object sender, RoutedEventArgs e)
        {
            //the shift note must be retireved and the plans must be separated before calculating dose
            if (shiftTB.Text == "NO" || separateTB.Text == "NO")
            {
                MessageBox.Show("Error! \nYou must generate the shift note AND separate the plan before calculating dose to each plan!");
                return;
            }

            //ask the user if they are sure they want to do this. Each plan will calculate dose sequentially, which will take time
            confirmUI CUI = new VMATTBIautoPlan.confirmUI();
            CUI.message.Text = "Warning!" + Environment.NewLine + "This will take some time as each plan needs to be calculated sequentionally!" + Environment.NewLine + "Continue?!";
            CUI.ShowDialog();
            if (!CUI.confirm) return;

            //let the user know the script is working
            calcDoseTB.Background = System.Windows.Media.Brushes.Yellow;
            calcDoseTB.Text = "WORKING";

            prep.CalculateDose();

            //let the user know this step has been completed
            calcDoseTB.Background = System.Windows.Media.Brushes.ForestGreen;
            calcDoseTB.Text = "YES";
        }

        private void PlanSum_Click(object sender, RoutedEventArgs e)
        {
            //do nothing. Eclipse v15.6 doesn't have this capability, but v16 and later does. This method is a placeholder (the planSum button exists in the UI.xaml file, but its visibility is set to 'hidden')
        }

        //stuff related to script configuration
        // TODO - remove this
        private void LoadNewConfigFile_Click(object sender, RoutedEventArgs e)
        {
            //load a configuration file different from the default in the executing assembly folder
            settings.ConfigFile = "";
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\configuration\\",
                Filter = "ini files (*.ini)|*.ini|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog().Value) {
                if (!LoadConfigurationSettings(openFileDialog.FileName)) 
                    DisplayConfigurationParameters(); 
                else 
                    MessageBox.Show("Error! Selected file is NOT valid!"); }
        }
        
        
        
        // parse the relevant data in the configuration file
        // TODO: Delete this
        
        private bool LoadConfigurationSettings(string file)
        {
            settings.ConfigFile = file;
            //encapsulate everything in a try-catch statment so I can be a bit lazier about data checking of the configuration settings (i.e., if a parameter or value is bad the script won't crash)
            try
            {
                using (StreamReader reader = new StreamReader(settings.ConfigFile))
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
                                            if (parameter == "default flash margin") settings.DefaultFlashMargin = result.ToString();
                                            else if (parameter == "default target margin") settings.DefaultTargetMargin = result.ToString();
                                            else if (parameter == "contour field overlap margin") settings.ContourFieldOverlapMargin = result.ToString(); 
                                        }
                                        else if (parameter == "beams per iso")
                                        {
                                            //parse the default requested number of beams per isocenter
                                            line = CropLine(line, "{");
                                            List<int> b = new List<int> { };
                                            //second character should not be the end brace (indicates the last element in the array)
                                            while (line.Substring(1, 1) != "}")
                                            {
                                                b.Add(int.Parse(line.Substring(0, line.IndexOf(","))));
                                                line = CropLine(line, ",");
                                            }
                                            b.Add(int.Parse(line.Substring(0, line.IndexOf("}"))));
                                            //only override the requested number of beams in the settings.BeamsPerIso array  
                                            for (int i = 0; i < b.Count(); i++) { if (i < settings.BeamsPerIso.Count()) settings.BeamsPerIso[i] = b.ElementAt(i); }
                                        }
                                        else if (parameter == "collimator rotations")
                                        {
                                            //parse the default requested number of beams per isocenter
                                            line = CropLine(line, "{");
                                            List<double> c = new List<double> { };
                                            //second character should not be the end brace (indicates the last element in the array)
                                            while (line.Contains(","))
                                            {
                                                c.Add(double.Parse(line.Substring(0, line.IndexOf(","))));
                                                line = CropLine(line, ",");
                                            }
                                            c.Add(double.Parse(line.Substring(0, line.IndexOf("}"))));
                                            for (int i = 0; i < c.Count(); i++) { if (i < 5) settings.CollRot[i] = c.ElementAt(i); }
                                        }
                                        //other parameters that should be updated
                                        else if (parameter == "use flash by default") settings.UseFlashByDefault = bool.Parse(value);
                                        else if (parameter == "default flash type") { if (value != "") settings.DefaultFlashType = value; }
                                        else if (parameter == "contour field overlap") { if (value != "") settings.ContourOverlap = bool.Parse(value); }
                                    }
                                    else if (line.Contains("add default sparing structure")) defaultSpareStruct_temp.Add(ParseSparingStructure(line));
                                    else if (line.Contains("add TS")) TSstructures_temp.Add(ParseTS(line));
                                    else if (line.Contains("add sclero TS")) scleroTSstructures_temp.Add(ParseTS(line));
                                    else if (line.Contains("add linac"))
                                    {
                                        //parse the settings.Linacs that should be added. One entry per line
                                        line = CropLine(line, "{");
                                        linac_temp.Add(line.Substring(0, line.IndexOf("}")));
                                    }
                                    else if (line.Contains("add beam energy"))
                                    {
                                        //parse the photon energies that should be added. One entry per line
                                        line = CropLine(line, "{");
                                        energy_temp.Add(line.Substring(0, line.IndexOf("}")));
                                    }
                                    else if (line.Contains("add jaw position"))
                                    {
                                        //parse the default requested number of beams per isocenter
                                        line = CropLine(line, "{");
                                        List<double> tmp = new List<double> { };
                                        //second character should not be the end brace (indicates the last element in the array)
                                        while (line.Contains(","))
                                        {
                                            tmp.Add(double.Parse(line.Substring(0, line.IndexOf(","))));
                                            line = CropLine(line, ",");
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

                                                }
                                                else if (line.Contains("add sparing structure")) scleroSpareStruct_temp.Add(ParseSparingStructure(line));
                                                else if (line.Contains("add opt constraint")) optConstDefaultSclero_temp.Add(ParseOptimizationConstraint(line));
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

                                                }
                                                else if (line.Contains("add sparing structure")) myeloSpareStruct_temp.Add(ParseSparingStructure(line));
                                                else if (line.Contains("add opt constraint")) optConstDefaultMyelo_temp.Add(ParseOptimizationConstraint(line));
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
                                                }
                                                else if (line.Contains("add sparing structure")) nonmyeloSpareStruct_temp.Add(ParseSparingStructure(line));
                                                else if (line.Contains("add opt constraint")) optConstDefaultNonMyelo_temp.Add(ParseOptimizationConstraint(line));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                return false;
            }
            //let the user know if the data parsing failed
            catch (Exception e) { MessageBox.Show(String.Format("Error could not load configuration file because: {0}\n\nAssuming default parameters", e.Message)); return true; }
        }


        //very useful helper method to remove everything in the input string 'line' up to a given character 'cropChar'
        private string CropLine(string line, string cropChar) { return line.Substring(line.IndexOf(cropChar) + 1, line.Length - line.IndexOf(cropChar) - 1); }

        private Tuple<string, string> ParseTS(string line)
        {
            //known array format --> can take shortcuts in parsing the data
            //structure id, sparing type, added margin in cm (ignored if sparing type is Dmax ~ Rx Dose)
            string dicomType = "";
            string TSstructure = "";
            line = CropLine(line, "{");
            dicomType = line.Substring(0, line.IndexOf(","));
            line = CropLine(line, ",");
            TSstructure = line.Substring(0, line.IndexOf("}"));
            return Tuple.Create(dicomType, TSstructure);
        }

        private Tuple<string,string,double> ParseSparingStructure(string line)
        {
            //known array format --> can take shortcuts in parsing the data
            //structure id, sparing type, added margin in cm (ignored if sparing type is Dmax ~ Rx Dose)
            string structure = "";
            string spareType = "";
            double val = 0.0;
            line = CropLine(line, "{");
            structure = line.Substring(0, line.IndexOf(","));
            line = CropLine(line, ",");
            spareType = line.Substring(0, line.IndexOf(","));
            line = CropLine(line, ",");
            val = double.Parse(line.Substring(0, line.IndexOf("}")));
            return Tuple.Create(structure, spareType, val);
        }

        private Tuple<string,string,double,double,int> ParseOptimizationConstraint(string line)
        {
            //known array format --> can take shortcuts in parsing the data
            //structure id, constraint type, dose (cGy), volume (%), priority
            string structure = "";
            string constraintType = "";
            double doseVal = 0.0;
            double volumeVal = 0.0;
            int priorityVal = 0;
            line = CropLine(line, "{");
            structure = line.Substring(0, line.IndexOf(","));
            line = CropLine(line, ",");
            constraintType = line.Substring(0, line.IndexOf(","));
            line = CropLine(line, ",");
            doseVal = double.Parse(line.Substring(0, line.IndexOf(",")));
            line = CropLine(line, ",");
            volumeVal = double.Parse(line.Substring(0, line.IndexOf(",")));
            line = CropLine(line, ",");
            priorityVal = int.Parse(line.Substring(0, line.IndexOf("}")));
            return Tuple.Create(structure, constraintType, doseVal, volumeVal, priorityVal);
        }

        
    }
}
