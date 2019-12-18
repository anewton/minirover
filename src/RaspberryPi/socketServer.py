#!/usr/bin/env python3
import socket
import math
from gpiozero import PWMOutputDevice
from gpiozero import DigitalOutputDevice


HOST = '192.168.10.104'  # Standard loopback interface address (localhost)
PORT = 51717        # Port to listen on (non-privileged ports are > 1023)
MAXDATAITEMS = 4    # Max number of items in the data array split by a pipe character

MIN_WIDTH = 1000
MAX_WIDTH = 2000


#///////////////// Define Motor Driver GPIO Pins /////////////////
# Motor A, Left Side GPIO CONSTANTS
PWM_DRIVE_LEFT = 14             # ENA - H-Bridge enable pin
FORWARD_LEFT_PIN = 23   # IN1 - Forward Drive
REVERSE_LEFT_PIN = 18   # IN2 - Reverse Drive
# Motor B, Right Side GPIO CONSTANTS
PWM_DRIVE_RIGHT = 25            # ENB - H-Bridge enable pin
FORWARD_RIGHT_PIN = 8   # IN1 - Forward Drive
REVERSE_RIGHT_PIN = 7   # IN2 - Reverse Drive

# Initialise objects for H-Bridge GPIO PWM pins
# Set initial duty cycle to 0 and frequency to 1000
driveLeft = PWMOutputDevice(PWM_DRIVE_LEFT, True, 0, 1000)
driveRight = PWMOutputDevice(PWM_DRIVE_RIGHT, True, 0, 1000)

# Initialise objects for H-Bridge digital GPIO pins
forwardLeft = PWMOutputDevice(FORWARD_LEFT_PIN)
reverseLeft = PWMOutputDevice(REVERSE_LEFT_PIN)
forwardRight = PWMOutputDevice(FORWARD_RIGHT_PIN)
reverseRight = PWMOutputDevice(REVERSE_RIGHT_PIN)

def calcCartesianCoordinates(stickX, stickY):
    # convert to polar coordinates
    r = math.sqrt((pow(stickX,2) + pow(stickY,2))) # hypot(x, y);
    t = math.atan2(stickY, stickX)

    # rotate by 45 degrees
    t += math.pi / 4

    # convert back to cartesian
    x = r * math.cos(t)
    y = r * math.sin(t)

    # rescale the new coordinates
    x = x * math.sqrt(2)
    y = y * math.sqrt(2)

    # clamp to 1000/2000
    x = max(-100, min(x, 100))
    y = max(-100, min(y, 100))
#    print("Calculated x and y coordinates as: (%s, %s)" % (x, y))
    left = translateCoordinateToMotorWidth(y)
    right = translateCoordinateToMotorWidth(-1 * x)
 #   print("Left and Right: (%s, %s)" % (left, right))
    setMotorSpeedAndDirection(left, right)

def translateCoordinateToMotorWidth(coordinate):
    # translate desired motorPWMWidth from args
    if (coordinate < 0):
        tempWidth = 1500 - ( 500 * (-1 * coordinate) / 100 )
        motorWidth = int(tempWidth)
    elif (coordinate == 0):
        motorWidth = 1500
    else:
        tempWidth = 1500 + ( 500 * ( coordinate / 100 ) )
        motorWidth = int(tempWidth)

    if(motorWidth < MIN_WIDTH):
        motorWidth = MIN_WIDTH

    if(motorWidth > MAX_WIDTH):
        motorWidth = MAX_WIDTH

 #   print("Coordinate and Motor Width: (%s, %s)" % (coordinate, motorWidth))
    return motorWidth

def calculateDriveValue(vector):
    if (vector == 1500):
        driveValue = 0.0
    if (vector > 1500):
        driveValue = (vector - 1500) / 500
    elif (vector < 1500):
        driveValue = 1.0 - ((vector - 1000) / 500)
    return driveValue

def setMotorSpeedAndDirection(left, right):
    if (left == 1500):
        forwardLeft.value = False
        reverseLeft.value = False
    if (left > 1500):
        forwardLeft.value = True
        reverseLeft.value = False
    elif (left < 1500):
        forwardLeft.value = False
        reverseLeft.value = True
    driveLeft.value = calculateDriveValue(left)
    if (right == 1500):
        forwardRight.value = False
        reverseRight.value = False
    if (right > 1500):
        forwardRight.value = True
        reverseRight.value = False
    elif (right < 1500):
        forwardRight.value = False
        reverseRight.value = True
    driveRight.value = calculateDriveValue(right)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.bind((HOST, PORT))
    s.listen()
    conn, addr = s.accept()
    with conn:
        print('Connected by', addr)
        while True:
            data = conn.recv(1024)
            if data:
                dataString = data.decode("utf-8")
                dataLength = len(dataString.split("|"))
                if (dataLength == MAXDATAITEMS):
                    dataArray = dataString.split("|", MAXDATAITEMS)
                    leftStickX = float(dataArray[0])
                    leftStickY = float(dataArray[1])
                    rightStickX = float(dataArray[2])
                    rightStickY = float(dataArray[3])
                    calcCartesianCoordinates(leftStickX, leftStickY)
