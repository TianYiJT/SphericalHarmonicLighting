using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


struct SHData
{
	public Matrix4x4 mat0;	
};

public class SHRender1 : MonoBehaviour 
{
	public string[] LightSHPath;
	public Cubemap[] LightCubeMap;
	public string coffeeSHPath;

	private Vector3[][] AllLightSHs;

	private Vector3[] LightSH;

	private IntegratorTask[] AllCoffeeSH;

	public Mesh mMesh;

	private Vector3 LastMousePosition;

	public int maxPower;

	public Material mMat;

	private float rot = 0.0f;

	private int tempIndex = 0;

	private ComputeBuffer mBuffer;

	private SHData[] shData;

	private Vector2[] uvData;

	public Material mSHMat;

	void Start () 
	{
		LastMousePosition = Input.mousePosition;
		InputSH ();
		shData = new SHData[AllCoffeeSH.Length];
		uvData = new Vector2[AllCoffeeSH.Length];
		for (int i = 0; i < uvData.Length; i++)
		{
			uvData [i].x = i;
			shData [i] = new SHData ();
			shData [i].mat0.m00 = AllCoffeeSH [i].coffeeSHResult [0];
			shData [i].mat0.m01 = AllCoffeeSH [i].coffeeSHResult [1];
			shData [i].mat0.m02 = AllCoffeeSH [i].coffeeSHResult [2];
			shData [i].mat0.m03 = AllCoffeeSH [i].coffeeSHResult [3];

			shData [i].mat0.m10 = AllCoffeeSH [i].coffeeSHResult [4];
			shData [i].mat0.m11 = AllCoffeeSH [i].coffeeSHResult [5];
			shData [i].mat0.m12 = AllCoffeeSH [i].coffeeSHResult [6];
			shData [i].mat0.m13 = AllCoffeeSH [i].coffeeSHResult [7];

			shData [i].mat0.m20 = AllCoffeeSH [i].coffeeSHResult [8];
			shData [i].mat0.m21 = AllCoffeeSH [i].coffeeSHResult [9];
			shData [i].mat0.m22 = AllCoffeeSH [i].coffeeSHResult [10];
			shData [i].mat0.m23 = AllCoffeeSH [i].coffeeSHResult [11];

			shData [i].mat0.m30 = AllCoffeeSH [i].coffeeSHResult [12];
			shData [i].mat0.m31 = AllCoffeeSH [i].coffeeSHResult [13];
			shData [i].mat0.m32 = AllCoffeeSH [i].coffeeSHResult [14];
			shData [i].mat0.m33 = AllCoffeeSH [i].coffeeSHResult [15];
		}

		mBuffer = new ComputeBuffer (shData.Length,64);

		mBuffer.SetData (shData);

		mSHMat.SetBuffer ("shDataBuffer",mBuffer);
		mMesh.uv = uvData;
	}

	void tranferLight()
	{
		LightSH = new Vector3[AllLightSHs[tempIndex].Length];

		for (int i = 0; i < LightSH.Length; i++) 
		{
			LightSH [i] = AllLightSHs [tempIndex] [i];
		}

		mMat.SetTexture ("_Tex",LightCubeMap[tempIndex]);
	}

	void InputSH()
	{
		AllLightSHs = new Vector3[LightSHPath.Length][];
		for(int i=0;i<LightSHPath.Length;i++)
		{
			if (File.Exists (LightSHPath[i]))
			{
				string[] allLightSHLines = File.ReadAllLines (LightSHPath[i]);

				AllLightSHs[i] = new Vector3[allLightSHLines.Length];

				for (int j = 0; j < AllLightSHs[i].Length; j++)
				{
					string[] vArray = Regex.Split (allLightSHLines[j]," ",RegexOptions.IgnoreCase);
					Vector3 targetVec = Vector3.zero;
					targetVec.x = float.Parse (vArray[0]);
					targetVec.y = float.Parse (vArray[1]);
					targetVec.z = float.Parse (vArray[2]);

					AllLightSHs[i][j] = targetVec;
				}

			}
		}

		LightSH = new Vector3[AllLightSHs[0].Length];

		for (int i = 0; i < LightSH.Length; i++) 
		{
			LightSH [i] = AllLightSHs [0] [i];
		}

		mMat.SetTexture ("_Tex",LightCubeMap[0]);



		if (File.Exists (coffeeSHPath)) 
		{
			string[] allCoffeeSHLines = File.ReadAllLines (coffeeSHPath);

			AllCoffeeSH = new IntegratorTask[allCoffeeSHLines.Length];
			for (int i = 0; i < allCoffeeSHLines.Length; i++)
			{
				AllCoffeeSH[i].coffeeSHResult = new float[maxPower];
				string[] coffArray = Regex.Split (allCoffeeSHLines[i]," ",RegexOptions.IgnoreCase);
				for(int j=0;j<maxPower;j++)
				{
					AllCoffeeSH [i].coffeeSHResult [j] = float.Parse (coffArray[j]);
				}
			}

		}
	}

	void Update () 
	{

		if (Input.GetKeyDown (KeyCode.Space))
		{
			tempIndex = (tempIndex + 1) % AllLightSHs.Length;
			tranferLight ();
		}

		Vector3 delta = Input.mousePosition - LastMousePosition;

		rot += delta.x*0.1f;

		mMat.SetFloat("_Rotation",rot*180.0f/Mathf.PI);

		Matrix4x4 W2O = Matrix4x4.Rotate(Quaternion.Euler(0.0f,rot*180.0f/Mathf.PI,0.0f)).inverse;

		Vector3[] rotLight = SHRotate.Rotate (LightSH,W2O);

		Matrix4x4 matR = new Matrix4x4 ();
		matR.m00 = rotLight [0].x;
		matR.m01 = rotLight [1].x;
		matR.m02 = rotLight [2].x;
		matR.m03 = rotLight [3].x;

		matR.m10 = rotLight [4].x;
		matR.m11 = rotLight [5].x;
		matR.m12 = rotLight [6].x;
		matR.m13 = rotLight [7].x;

		matR.m20 = rotLight [8].x;
		matR.m21 = rotLight [9].x;
		matR.m22 = rotLight [10].x;
		matR.m23 = rotLight [11].x;

		matR.m30 = rotLight [12].x;
		matR.m31 = rotLight [13].x;
		matR.m32 = rotLight [14].x;
		matR.m33 = rotLight [15].x;
		mSHMat.SetMatrix ("rotRLight",matR);

		Matrix4x4 matG = new Matrix4x4 ();
		matG.m00 = rotLight [0].y;
		matG.m01 = rotLight [1].y;
		matG.m02 = rotLight [2].y;
		matG.m03 = rotLight [3].y;

		matG.m10 = rotLight [4].y;
		matG.m11 = rotLight [5].y;
		matG.m12 = rotLight [6].y;
		matG.m13 = rotLight [7].y;

		matG.m20 = rotLight [8].y;
		matG.m21 = rotLight [9].y;
		matG.m22 = rotLight [10].y;
		matG.m23 = rotLight [11].y;

		matG.m30 = rotLight [12].y;
		matG.m31 = rotLight [13].y;
		matG.m32 = rotLight [14].y;
		matG.m33 = rotLight [15].y;
		mSHMat.SetMatrix ("rotGLight",matG);

		Matrix4x4 matB = new Matrix4x4 ();
		matB.m00 = rotLight [0].z;
		matB.m01 = rotLight [1].z;
		matB.m02 = rotLight [2].z;
		matB.m03 = rotLight [3].z;

		matB.m10 = rotLight [4].z;
		matB.m11 = rotLight [5].z;
		matB.m12 = rotLight [6].z;
		matB.m13 = rotLight [7].z;

		matB.m20 = rotLight [8].z;
		matB.m21 = rotLight [9].z;
		matB.m22 = rotLight [10].z;
		matB.m23 = rotLight [11].z;

		matB.m30 = rotLight [12].z;
		matB.m31 = rotLight [13].z;
		matB.m32 = rotLight [14].z;
		matB.m33 = rotLight [15].z;
		mSHMat.SetMatrix ("rotBLight",matB);


		LastMousePosition = Input.mousePosition;

	}
}
