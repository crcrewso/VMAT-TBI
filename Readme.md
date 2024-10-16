
# *Attention!!*

1/20/2024
*THIS REPOSITORY IS NO LONGER BEING ACTIVELY DEVELOPED! ONLY CRITICAL BUGS FIXES WILL BE ADDRESSED!*
The actively maintained version of the VMAT TBI autoplanning code can be found here:
https://github.com/esimiele/VMAT-TBI-CSI

See the readme in that repo for details (includes automated planning for VMAT CSI as well). 
*All users of this repository should begin migrating to the new version of the code as this repository will NOT be supported indefinitely!*

---
Updated 1/13/2023

Changes from 1/9/2023
-introduced fix for couch issue #23
-can add key strings identifiers in configuration file for script to identify support structures (not spinning manny!)

Changes from 1/8/2023
-addressed issue #25
-adjusted pre and post build events for both project files

Changes from 12/28/2022
-copied extra files from master branch to beta branch (example patients, excel worksheet, documentation, etc)
-removed old master branch
-Made beta branch default
-Renamed beta branch to 'master'

Changes from 12/8/2022
-fixed issue where multiple _VMAT TBI plans in separate courses caused the executable to ask the user multiple times to select a course whenever you add a constraint, get constraints from plan, start optimization loop, open a patient, etc.
-modified and incorporated pull request #22 (iromero77)
    -now the plugin has the feature of being able to check for potential collisions between the couch and linac
-revised window open location for all windows in plugin script

Changes from 10/14/2022
-fixed issue #24 (Wrong course found)
-modified and incorporated pull request #20 (iromero77)
-stand-alone executable generates VMS.TPS Application instance in constructor of MainWindow and does not crash program if it fails

Changes from 10/11/2022 push:
-Added initial functionality for all VMAT fields (AP/PA fields will NOT be generated)
-To use this feature, set the 'all fields VMAT' option in the configuration file to 'true'
-WARNING: this feature is not completed yet and has only been through limited testing --> Use at your own risk
	-Still need to add code to properly separate the fields in each isocenter (MLC positions and control points need to be reversed)

Changes from 10/4/22 push:
-Addressed issues #12 (Spinning Manny Notice) and #14 (default course name)
-Prior push history not shown due to merge conflicts, had to force push
-restructed code so now both projects are under one solution and build to the bin/ folder in the top directory
	-This makes it much easier to maintain the code and especially the configuration file as now only one configuration file is needed in the bin folder

Changes from 4/10/2022 push:
-added option to specify number of VMAT isocenters in the GUI (up to a maximum of 4).
-fixed an issue where flash could be added to the body structure multiple times. Now the script can be run on the same structure set multiple times without generating flash multiple times
-fixed an issue regarding GPU dose calculation option
-added option for user to elect to use GPU for optimization
-added error handling code in optimization loop to allow the program to fail gracefully if it encounters a problem during optimization (particularly if the user elects to use the GPU for dose calculation/optimization)
-fixed minor formatting issues
-fixed minor issues with the GUI

My current clinic has v16.1 of Eclipse whereas the scripts were built in v15.6 (during my residency). This may cause a bit of a mismatch between the code and what Eclipse requires. Both scripts will run in v16.1
as they were recompiled with the necessary libraries to run in v16.1. The VMS.TPS libraries have been updated to v16.1. Minimal changes were required for the plug-in script.
However, the executable binary is now compiled and placed in /ESAPI_MT/VMATTBI_optLoopMT/VMATTBI_optLoopMT/bin/x64/debug instead of /ESAPI_MT/VMATTBI_optLoopMT/VMATTBI_optLoopMT/bin/debug
You need to modify the .ini configuration file inside the /bin/x64/debug directory for any changes to take effect if your clinic is using v16. 

I will work to update the scripts using build configurations so it is easy to rebuild the scripts according to the Eclipse version at your clinic.

Latest code versions:
Plug-in --> v2.1
Executable --> v1.7

VMATTBIAutoplan/VMATTBIautoPlan --> binary plug in
VMATTBIAutoplan/VMATTBI_optLoopMT --> stand-alone executable

Some important notes to make the scripts run:
binary plug-in:
-for the most part, this script is pretty self-contained
-the main parameters that can be adjusted are the global variables noted at the beginning of the UI class (UI.xaml.cs file)
-if you want to adjust the default energy for the static AP/PA beams, you need to adjust the beam energy specifier used in assigning ebmpStatic 
(two total instances of "6X" in the constructors for the class)

stand-alone executable:
-

Please see the VMAT_TBI_install_and_run_guide.pdf file to help you get up and running quickly. Feel free to download the example (anonymized) patient data on the master branch for some examples of running the code on real
patient data.

If you have questions, leave a comment and i'll try and respond as quickly as I can.
