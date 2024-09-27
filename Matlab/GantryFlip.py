import pydicom
import sys
import time
import os
from gooey import Gooey, GooeyParser


@Gooey
def main():
    parser = GooeyParser(description='Gantry Flip')
    parser.add_argument('InputFile', widget='FileChooser', help='Select the DICOM file you want to flip')
    args = parser.parse_args()
    file = args.InputFile
    filename = os.path.basename(file)
    path = os.path.dirname(file)
    Info = pydicom.dcmread(file)
    RPmodified = Info

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
    RPmodified.save_as(os.path.join(path, filename + '-GantryFlipped.dcm'))
    display_message('Gantry Flip Complete. File saved as: {}-GantryFlipped.dcm')

def display_message(program_message):
    message = program_message.format('\n'.join(sys.argv[1:])).split('\n')
    delay = 1.8 / len(message)

    for line in message:
        print(line)
        time.sleep(delay)

if __name__ == '__main__':
    main()


