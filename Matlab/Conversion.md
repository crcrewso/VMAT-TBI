# Convert Algorithm to python

Source [CodingFleet](https://codingfleet.com/code-converter/matlab/python/)

Header

```Matlab
clear all;
[file,path] = uigetfile('*.dcm');       %select file
cd(path)        %set path to folder
Info = dicominfo(file);     %Dicom info
RPmodified = Info;          %New plan file
FN = fieldnames(Info.BeamSequence);     %Number of beams
BeamNumber = length(FN);
```

## Matlab Algorithm

```Matlab
for i=1:BeamNumber
    CPcount = Info.BeamSequence.(FN{i}).NumberOfControlPoints;
    CPList = fieldnames(Info.BeamSequence.(FN{i}).ControlPointSequence);
    ControlPoints = Info.BeamSequence.(FN{i}).ControlPointSequence;
    for j=1:CPcount
        if j==1
            Col = ControlPoints.(CPList{j}).BeamLimitingDeviceAngle;
            if Col > 180
                Col = Col-180;   
            else 
                Col = Col+180;
            end
            RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).BeamLimitingDeviceAngle = Col;
        end
        Dir = ControlPoints.(CPList{j}).GantryRotationDirection;
        if strcmp(Dir,'CW')
            RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).GantryRotationDirection = 'CC';
        elseif strcmp(Dir,'CC')
            RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).GantryRotationDirection = 'CW';
        elseif strcmp(Dir,'NONE')
            'Done'
            i
        else
            error('No Gantry')
        end
        swapAngle = ControlPoints.(CPList{CPcount-j+1}).GantryAngle;
        RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).GantryAngle = swapAngle;
    end
end
```

## Python equivalent

```Python
for i in range(len(Info.BeamSequence)):
    CPcount = len(Info.BeamSequence[i].NumberOfControlPoints)
    CPList = list(Info.BeamSequence[i].ControlPointSequence.keys())
    ControlPoints = Info.BeamSequence[i].ControlPointSequence
    for j in range(CPcount):
        if j == 0:
            Col = ControlPoints[CPList[j]].BeamLimitingDeviceAngle
            if Col > 180:
                Col = Col - 180
            else:
                Col = Col + 180
            RPmodified.BeamSequence[i].ControlPointSequence[CPList[j]].BeamLimitingDeviceAngle = Col
        Dir = ControlPoints[CPList[j]].GantryRotationDirection
        if Dir == 'CW':
            RPmodified.BeamSequence[i].ControlPointSequence[CPList[j]].GantryRotationDirection = 'CC'
        elif Dir == 'CC':
            RPmodified.BeamSequence[i].ControlPointSequence[CPList[j]].GantryRotationDirection = 'CW'
        elif Dir == 'NONE':
            print('Done')
            print(i)
        else:
            raise ValueError('No Gantry')
        swapAngle = ControlPoints[CPList[CPcount-j-1]].GantryAngle
        RPmodified.BeamSequence[i].ControlPointSequence[CPList[j]].GantryAngle = swapAngle

```

## Conversion Notes

Here's how the code has been converted:

1. The for loop iterating over BeamNumber has been replaced with a for loop iterating over the range of the length of Info.BeamSequence.
1. The CPcount variable is assigned the length of Info.BeamSequence[i].NumberOfControlPoints.
1. The CPList variable is assigned a list of the keys of Info.BeamSequence[i].ControlPointSequence.
1. The ControlPoints variable is assigned Info.BeamSequence[i].ControlPointSequence.
1. The inner for loop iterating over CPcount has been replaced with a for loop iterating over the range of CPcount.
1. The conditional statements checking the value of j have been modified to check if j is 0 (the first iteration) instead of 1.
1. The Col variable is assigned the value of ControlPoints[CPList[j]].BeamLimitingDeviceAngle, and the conditional statements modifying Col have been updated accordingly.
1. The assignment of RPmodified.BeamSequence[i].ControlPointSequence[CPList[j]].BeamLimitingDeviceAngle has been updated to use the new Col value.
1. The conditional statements checking the value of Dir have been updated to use the appropriate string comparisons in Python.
1. The swapAngle variable is assigned the value of ControlPoints[CPList[CPcount-j-1]].GantryAngle, as the indexing in Python starts from 0.
1. The assignment of RPmodified.BeamSequence[i].ControlPointSequence[CPList[j]].GantryAngle has been updated to use the new swapAngle value.
1. The error function has been replaced with a ValueError exception.
