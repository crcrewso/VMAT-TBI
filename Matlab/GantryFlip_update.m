
clear all;
[file,path] = uigetfile('*.dcm');       %select file
cd(path)        %set path to folder

Info = dicominfo(file);     %Dicom info
RPmodified = Info;          %New plan file
FN = fieldnames(Info.BeamSequence);     %Number of beams
BeamNumber = length(FN);

for i=1:BeamNumber          %Loop through beams
    
    CPcount = Info.BeamSequence.(FN{i}).NumberOfControlPoints;      %Number of control points
    CPList = fieldnames(Info.BeamSequence.(FN{i}).ControlPointSequence);    %Control points
    ControlPoints = Info.BeamSequence.(FN{i}).ControlPointSequence;

    for j=1:CPcount     %Loop through control points
        
        if j==1         %First control point set col angle
           Col = ControlPoints.(CPList{j}).BeamLimitingDeviceAngle;  %Angle 
           if Col > 180
               Col = Col-180;   
           else 
              Col = Col+180;   %Flip 180 degrees
           end
           RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).BeamLimitingDeviceAngle = Col;
        end


        Dir = ControlPoints.(CPList{j}).GantryRotationDirection;     %Gantry rotation direction
        if strcmp(Dir,'CW')     %If CW set to CC
            RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).GantryRotationDirection = 'CC';
        elseif strcmp(Dir,'CC')     %If CC set to CW
            RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).GantryRotationDirection = 'CW';
        elseif strcmp(Dir,'NONE')   %Last control point has direction set as 'NONE'
            'Done'
            i   %Do nothing
        else
            error('Undefined Gantry')
        end
       
        swapAngle = ControlPoints.(CPList{CPcount-j+1}).GantryAngle;  %Swap angle with opposite 1-178 2-177 3-176 etc...
        RPmodified.BeamSequence.(FN{i}).ControlPointSequence.(CPList{j}).GantryAngle = swapAngle;
 
    end
end

if not(isfolder('Flip'))        %Check if new folder exists
    mkdir('Flip')
end
cd Flip
dicomwrite([],strcat('Flip',file),RPmodified,'CreateMode','copy');  %Write new dicom file

%Calc with preset values



