import pydicom
import sys
import time
import os
import copy
from gooey import Gooey, GooeyParser


@Gooey
def main():
    parser = GooeyParser(description='Gantry Flip')
    parser.add_argument('InputFile', widget='FileChooser', help='Select the DICOM file you want to flip')
    args = parser.parse_args()
    sourceFile = args.InputFile
    saveFile = sourceFile.replace('.dcm', '-Reversed.dcm')
    filename = os.path.basename(sourceFile)
    path = os.path.dirname(sourceFile)
    referencePlan = pydicom.dcmread(sourceFile)
    # Create a copy of the original file
    flippedPlan = copy.deepcopy(referencePlan)
    # Modify the copy name to indicate it's been reversed
    flippedPlan.RTPlanLabel = referencePlan.RTPlanLabel + "-Reversed"
    flippedPlan.SOPInstanceUID = pydicom.uid.generate_uid()
    # Plan name safety, if the plan name is too long it will be cut
    if (len(flippedPlan.RTPlanLabel) > 16):
        flippedPlan.RTPlanLabel = referencePlan.RTPlanLabel[0:11] + "-Rev"

    for i in range(len(referencePlan.BeamSequence)):
        CPcount = len(referencePlan.BeamSequence[i].ControlPointSequence)
        CPList = list(referencePlan.BeamSequence[i].ControlPointSequence)
        ControlPoints = referencePlan.BeamSequence[i].ControlPointSequence

        for j in range(CPcount):
            if j == 0:
                Col = CPList[j].BeamLimitingDeviceAngle
                if Col > 180:
                    Col = Col - 180
                else:
                    Col = Col + 180
                flippedPlan.BeamSequence[i].ControlPointSequence[j].BeamLimitingDeviceAngle = Col
            Dir = CPList[j].GantryRotationDirection
            if Dir == 'CW':
                flippedPlan.BeamSequence[i].ControlPointSequence[j].GantryRotationDirection = 'CC'
            elif Dir == 'CC':
                flippedPlan.BeamSequence[i].ControlPointSequence[j].GantryRotationDirection = 'CW'
            elif Dir == 'NONE':
                print('Done')
                print(i)
            else:
                raise ValueError('No Gantry')
            swapAngle = CPList[CPcount - j - 1].GantryAngle
            flippedPlan.BeamSequence[i].ControlPointSequence[j].GantryAngle = swapAngle
    flippedPlan.save_as(saveFile)
    display_message('Gantry Flip Complete. File saved as: {}')

def display_message(program_message):
    message = program_message.format('\n'.join(sys.argv[1:])).split('\n')
    delay = 1.8 / len(message)

    for line in message:
        print(line)
        time.sleep(delay)

if __name__ == '__main__':
    main()

