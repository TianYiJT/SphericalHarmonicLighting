import os
import math

a = 1.4

b = 2.9

res = []

for i in range(1800):
	res1 = 0.0
	for j in range(10):
		res1 = res1 + math.pow(a,j)*math.pow(b,j)*i
	res.append(res1)

print(res)