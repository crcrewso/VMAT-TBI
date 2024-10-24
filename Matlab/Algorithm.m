for i=1:BeamNumber          %Loop through beams
    CPcount = Info.Beams.(FN{i}).NumberOfCPs;      %Number of control points
    CPList = fieldnames(Info.Beams.(FN{i}).CPSeq);    %Control points
    CPs = Info.Beams.(FN{i}).CPSeq;

    for j=1:CPcount     %Loop through control points
        
        if j==1         %First control point set col angle
            Col = CPs.(CPList{j}).ColAngle;  %Angle 
            if Col > 180
                Col = Col-180;   
            else 
                Col = Col+180;   %Flip 180 degrees
            end
            RPmodified.Beams.(FN{i}).CPSeq.(CPList{j}).ColAngle = Col;
        end


        Dir = CPs.(CPList{j}).GantryRot;     %Gantry rotation direction
        if strcmp(Dir,'CW')     %If CW set to CC
            RPmodified.Beams.(FN{i}).CPSeq.(CPList{j}).GantryRot = 'CC';
        elseif strcmp(Dir,'CC')     %If CC set to CW
            RPmodified.Beams.(FN{i}).CPSeq.(CPList{j}).GantryRot = 'CW';
        elseif strcmp(Dir,'NONE')   %Last control point has direction set as 'NONE'
            'Done'
            i   %Do nothing
        else
            error('No Gantry')
        end
        
        swapAngle = CPs.(CPList{CPcount-j+1}).GantryAngle;  %Swap angle with opposite 1-178 2-177 3-176 etc...
        RPmodified.Beams.(FN{i}).CPSeq.(CPList{j}).GantryAngle = swapAngle;

    end
end

