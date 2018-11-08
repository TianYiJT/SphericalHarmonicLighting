# -*- coding: utf-8 -*- 

import tensorflow as tf
from PIL import Image
import os 
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2'
import numpy as np
import math 
MAX_NUM_TRAIN_DATA = 9

DataPath = "D:/Output"

OutputPath = "D:/Train/train.txt"

ImagePath = "D:/global_7.png"

PhotonDistributionDatas = np.zeros((MAX_NUM_TRAIN_DATA,140,7))

rtData = np.zeros((MAX_NUM_TRAIN_DATA),np.float32)

gtData =  np.zeros((MAX_NUM_TRAIN_DATA),np.float32)

btData =  np.zeros((MAX_NUM_TRAIN_DATA),np.float32)

def Input_Data(path,imagepath):
	tempNum = 0
	PMDatas = np.zeros((MAX_NUM_TRAIN_DATA,6))

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
				if tempNum > MAX_NUM_TRAIN_DATA - 1:
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
					if tempPhoton == 140:
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


		if tempNum > MAX_NUM_TRAIN_DATA:
			break
	return tempNum

def Output_Data(path,array):
	file = open(path,'w')
	for i in range(4):
		line = ''
		for j in range(8):
			line = line + str(array[i][j])+' '
		file.write(line)
	file.close()

rNum = Input_Data(DataPath,ImagePath)

print('Input Over')

ParamTable = tf.Variable(tf.random_uniform((4,8),-5.0,5.0,tf.float32))

train_rData =  tf.Variable(tf.zeros((MAX_NUM_TRAIN_DATA),tf.float32))
train_gData =  tf.Variable(tf.zeros((MAX_NUM_TRAIN_DATA),tf.float32))
train_bData =  tf.Variable(tf.zeros((MAX_NUM_TRAIN_DATA),tf.float32))

for i in range(rNum):
	rV = 0.0
	gV = 0.0
	bV = 0.0
	for j in range(140):
		tempPhoton = PhotonDistributionDatas[i][j]
		for k in range(8):
			rV = rV + (ParamTable[0][k]*math.pow(tempPhoton[0],k+1)+ParamTable[1][k]*math.pow(tempPhoton[1],k+1)+
				ParamTable[2][k]*math.pow(tempPhoton[2],k+1)+ParamTable[3][k]*math.pow(tempPhoton[3],k+1))*tempPhoton[4]
			gV = gV + (ParamTable[0][k]*math.pow(tempPhoton[0],k+1)+ParamTable[1][k]*math.pow(tempPhoton[1],k+1)+
				ParamTable[2][k]*math.pow(tempPhoton[2],k+1)+ParamTable[3][k]*math.pow(tempPhoton[3],k+1))*tempPhoton[5]
			bV = bV + (ParamTable[0][k]*math.pow(tempPhoton[0],k+1)+ParamTable[1][k]*math.pow(tempPhoton[1],k+1)+
				ParamTable[2][k]*math.pow(tempPhoton[2],k+1)+ParamTable[3][k]*math.pow(tempPhoton[3],k+1))*tempPhoton[6]
	train_rData[i].assign(rV)
	train_gData[i].assign(gV)
	train_bData[i].assign(bV)
	print(i)


RTData = tf.constant([rtData[x] for x in range(len(rtData))])

GTData = tf.constant([gtData[x] for x in range(len(gtData))])

BTData = tf.constant([btData[x] for x in range(len(btData))])

print('Data Process Over')

print('loss')
loss = tf.reduce_mean(tf.square(RTData-train_rData)+tf.square(GTData - train_gData)+tf.square(BTData - train_bData))

print('optimizer')
optimizer = tf.train.GradientDescentOptimizer(0.02)

print('train')
train = optimizer.minimize(loss)

print('init')
init = tf.global_variables_initializer()

print('session')
ses = tf.Session()

print('runInit')
ses.run(init)

print('Learning Over')

for step in range(20):
	print(step)
	ses.run(train)

array = ses.run(ParamTable)

Output_Data(OutputPath,array)

print('Output Over')







