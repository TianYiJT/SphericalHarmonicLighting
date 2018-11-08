import os
import shutil

srcPath = "D:/Output"

desPath = "D:/verification"

allNum = 0

for file in os.listdir(srcPath):
	allNum = allNum + 1

tempNum = 0
for file in os.listdir(srcPath):
	tempNum = tempNum + 1
	if tempNum < allNum * 0.7:
		pass
	else:
		file_path = os.path.join(srcPath,file)
		des_path = os.path.join(desPath,file)
		shutil.move(file_path,des_path)