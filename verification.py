# -*- coding: utf-8 -*- 

from PIL import Image
import os 
import numpy as np
import math

verificationPath = 'D:/verification'

ImagePath = 'D:/global_7.png'

TrainPath = 'D:/Train/train.txt'

loss = 0.0

MAX_ERROR = 0.02

MAX_NUM_VER_DATA = 20

PhotonDistributionDatas = np.zeros((MAX_NUM_VER_DATA,180,7))

rtData = np.zeros((MAX_NUM_VER_DATA),np.float32)

gtData =  np.zeros((MAX_NUM_VER_DATA),np.float32)

btData =  np.zeros((MAX_NUM_VER_DATA),np.float32)

PMDatas = np.zeros((MAX_NUM_VER_DATA,6))

ParamTable = np.zeros((4,4),np.float32)

def Input_Data(path,imagepath):
	tempNum = 0

	im = Image.open(imagepath)

	for file in os.listdir(path):
		file_path = os.path.join(path,file)
		fileLine = open(file_path,'r')
		DATA_START = False
		tempPhoton = 0
		tempLine = 0
		for line in fileLine:
			tempLine = tempLine + 1
			if '#' in line:
				DATA_START = True
				tempPhoton = 0
				if tempNum > MAX_NUM_VER_DATA - 1:
					break
				continue
			if '*' in line:
				DATA_START = False
				rtData[tempNum] = ((rtData[tempNum] - PMDatas[tempNum][0])/PMDatas[tempNum][3])
				gtData[tempNum] = ((gtData[tempNum] - PMDatas[tempNum][1])/PMDatas[tempNum][4])
				btData[tempNum] = ((btData[tempNum] - PMDatas[tempNum][2])/PMDatas[tempNum][5])
				tempNum = tempNum + 1
				continue
			if DATA_START:
				DataArray = line.split()
				if len(DataArray) == 2:
					try:
						r,g,b = im.getpixel((int(float(DataArray[0])),int(float(DataArray[1]))))
						rtData[tempNum] = (r)
						gtData[tempNum] = (g)
						btData[tempNum] = (b)
					except:
						DATA_START = False

				elif len(DataArray) == 6:
					try:
						PMDatas[tempNum][0] = float(DataArray[0])
						PMDatas[tempNum][1] = float(DataArray[1])
						PMDatas[tempNum][2] = float(DataArray[2])
						PMDatas[tempNum][3] = float(DataArray[3])
						PMDatas[tempNum][4] = float(DataArray[4])
						PMDatas[tempNum][5] = float(DataArray[5])
					except:
						DATA_START = False
				elif len(DataArray) == 7:
					if tempPhoton == 180:
						continue
					try:
						PhotonDistributionDatas[tempNum][tempPhoton][0] = (float(DataArray[0]))
						PhotonDistributionDatas[tempNum][tempPhoton][1] = (float(DataArray[1]))
						PhotonDistributionDatas[tempNum][tempPhoton][2] = (float(DataArray[2]))
						PhotonDistributionDatas[tempNum][tempPhoton][3] = (float(DataArray[3]))
						PhotonDistributionDatas[tempNum][tempPhoton][4] = (float(DataArray[4]))
						PhotonDistributionDatas[tempNum][tempPhoton][5] = (float(DataArray[5]))
						PhotonDistributionDatas[tempNum][tempPhoton][6] = (float(DataArray[6]))
						tempPhoton = tempPhoton+1
					except:
						DATA_START = False
		fileLine.close()


		if tempNum > MAX_NUM_VER_DATA:
			break
	return tempNum

def GetParamTable(paramPath):
	file = open(paramPath,'r')
	for l in file:
		lArr = l.split()
		for i in range(4):
			for j in range(4):
				ParamTable[i][j] = float(lArr[i*4+j])
		break
	file.close()

def verification(r,g,b,photondata = []):
	rC = 0.0
	gC = 0.0
	bC = 0.0
	for i in range(180):
		for k in range(4):
			rC = rC + (ParamTable[0][k]*math.pow(photondata[i][0],k+1) + ParamTable[1][k]*math.pow(photondata[i][1],k+1)+ 
				ParamTable[2][k]*math.pow(photondata[i][2],k+1) + ParamTable[3][k]*math.pow(photondata[i][3],k+1))*photondata[i][4]
			gC = gC + (ParamTable[0][k]*math.pow(photondata[i][0],k+1) + ParamTable[1][k]*math.pow(photondata[i][1],k+1)+ 
				ParamTable[2][k]*math.pow(photondata[i][2],k+1) + ParamTable[3][k]*math.pow(photondata[i][3],k+1))*photondata[i][5]
			bC = bC + (ParamTable[0][k]*math.pow(photondata[i][0],k+1) + ParamTable[1][k]*math.pow(photondata[i][1],k+1)+ 
				ParamTable[2][k]*math.pow(photondata[i][2],k+1) + ParamTable[3][k]*math.pow(photondata[i][3],k+1))*photondata[i][6]
	if (r - rC)*(r-rC) + (g - gC)*(g - gC)+(b - bC)*(b - bC)<MAX_ERROR * MAX_ERROR:
		return True
	else:
		return False 

rNum = Input_Data(verificationPath,ImagePath)

GetParamTable(TrainPath)

tNum = 0

for i in range(rNum):
	if verification(rtData[i],gtData[i],btData[i],PhotonDistributionDatas[i]):
		tNum = tNum + 1

loss = 1.0 - float(tNum)/float(rNum)

print(loss)
