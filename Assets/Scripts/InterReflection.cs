using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class InterReflection : MonoBehaviour 
{

	public string coffeeSHPath;

	public Mesh mMesh;

	private IntegratorTask[] AllCoffeeSH;

	public int maxSample;

	public int maxPower;

	private IntegratorTask[] OutputCoffeeSH;

	public string OutputPath;

	public float Albedo;

	void Start ()
	{
		Input ();
		for (int i = 0; i < OutputCoffeeSH.Length; i++)
		{
			InterReflectionIntegrator (i);
		}
		Output ();
	}

	void Input()
	{
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

		OutputCoffeeSH = new IntegratorTask[AllCoffeeSH.Length];

		for (int i = 0; i < AllCoffeeSH.Length; i++)
		{
			OutputCoffeeSH[i].coffeeSHResult = new float[maxPower];	
		}

	}

	public void InterReflectionIntegrator(int index)
	{

		Vector3 position = mMesh.vertices[index];
		Vector3 normal = mMesh.normals[index];

		Vector2[] Samples = MCIntegrator (maxSample);

		float factor = 4.0f * Mathf.PI / maxSample;

		float[] result = new float[maxPower];
		for (int i = 0; i < maxPower; i++)
			result [i] = 0.0f;

		for (int i = 0; i < maxSample; i++) 
		{
			Vector3 tempDir = tranfer (Samples[i].x,Samples[i].y);

			int triangleIndex = RayCast (position,tempDir);

			if (triangleIndex != -1) 
			{
				float csn = Mathf.Clamp01(Vector3.Dot (tempDir.normalized,normal.normalized));
				float[] shs = MeshSH ((int)triangleIndex);
				for (int j = 0; j < shs.Length; j++)
				{
					result [j] += csn * shs[j] * Albedo;
				}
			}

		}

		for (int i = 0; i < maxPower; i++)
		{
			OutputCoffeeSH [index].coffeeSHResult [i] = result [i] * factor + AllCoffeeSH[index].coffeeSHResult[i]; 
		}

	}

	public float[] MeshSH(int index)
	{
		int subMesh_ = mMesh.triangles [index];
		int[] subMesh = new int[3];
		if (subMesh_ % 3 == 0) 
		{
			subMesh [0] = subMesh_;
			subMesh [1] = subMesh_ + 1;
			subMesh [2] = subMesh_ + 2;
		}
		if (subMesh_ % 3 == 1) 
		{
			subMesh [0] = subMesh_-1;
			subMesh [1] = subMesh_;
			subMesh [2] = subMesh_+1;
		}
		if (subMesh_ % 3 == 2) 
		{
			subMesh [0] = subMesh_-2;
			subMesh [1] = subMesh_-1;
			subMesh [2] = subMesh_;
		}
		IntegratorTask task0 = AllCoffeeSH [subMesh[0]];
		IntegratorTask task1 = AllCoffeeSH [subMesh [1]];
		IntegratorTask task2 = AllCoffeeSH [subMesh [2]];

		float[] res = new float[task0.coffeeSHResult.Length];

		for (int i = 0; i < res.Length; i++)
		{
			res [i] = task0.coffeeSHResult [i] + task1.coffeeSHResult [i] + task2.coffeeSHResult [i]; 	
			res [i] /= 3.0f;
		}

		return res;
	}


	private int RayCast(Vector3 Position,Vector3 Direction)
	{
		RaycastHit hit;
		if (Physics.Raycast (Position, Direction,out hit,5000.0f))
		{
			return hit.triangleIndex;
		}
		else
		{
			return -1;
		}
	}

	Vector3 tranfer(float theta,float phi)
	{

		Vector3 normal = new Vector3 (0,0,1);
		Vector3 tangent = new Vector3 (0,1,0);
		Vector3 BiNormal = new Vector3 (1,0,0);
		Vector3 result = normal * Mathf.Cos (theta) + tangent * Mathf.Sin (theta) * Mathf.Sin (phi) + BiNormal * Mathf.Sin (theta) * Mathf.Cos (phi);
		return result;
	}
		
	private Vector2[] MCIntegrator(int maxCount)
	{
		Vector2[] res = new Vector2[maxCount];
		for (int i = 0; i < maxCount; i++) 
		{
			res [i].x = Random.Range (0.0f,1.0f);
			res [i].y = Random.Range (0.0f,1.0f);
			res [i].x = 2 * Mathf.Acos (Mathf.Sqrt(1-res[i].x));
			res [i].y = 2 * res [i].y * Mathf.PI;
		}
		return res;
	}

	void Output()
	{
		string[] myOutput = new string[OutputCoffeeSH.Length];
		for (int i = 0; i < OutputCoffeeSH.Length; i++)
		{
			string temp = "";
			for (int j = 0; j < OutputCoffeeSH [i].coffeeSHResult.Length; j++) 
			{
				temp+=OutputCoffeeSH[i].coffeeSHResult[j].ToString()+" ";
			}
			myOutput [i] = temp;
		}
		System.IO.File.WriteAllLines(@OutputPath, myOutput, Encoding.UTF8);
	}
}
