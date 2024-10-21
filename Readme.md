# Breaking from Upstream

This code base was originally forked from <https://github.com/esimiele/VMAT-TBI-CSI>

TODO: This document is a work in progress that needs help

## 2024-10-21

Version 3 is focused on simplification of VMAT only plans with added features and simplifications.
Simplifications include:

- No couch validations
- No matchline
- VMAT Fields Only
- Up to 7 isos
- Settings hard coded in Settings class, no ini file
  - This means changing settings requires recompiling, hopefully I'll work on a more modern settings file format, YAML/TOML perhaps?

## About

There are two main parts to this tooling, a binary plug-in to be used within Eclipse (15.6+), and a stand-alone executable

VMATTBIAutoplan/VMATTBIautoPlan --> binary plug in
VMATTBIAutoplan/VMATTBI_optLoopMT --> stand-alone executable

Some important notes to make the scripts run:

### binary plug-in

- For the most part, this script is pretty self-contained
- Most settings can be found in the Settings class.

### stand-alone executable

Please see the VMAT_TBI_install_and_run_guide.pdf file to help you get up and running quickly. Feel free to download the example (anonymized) patient data on the master branch for some examples of running the code on real
patient data.

If you have questions, leave a comment and i'll try and respond as quickly as I can.

### Python/Matlab plan reversal script

The original version is a Matlab script coded by Daniel Morton, which has been adapted to a Python script that can be compiled and distributed as a self-contained executable.

TODO: Document this tool.
