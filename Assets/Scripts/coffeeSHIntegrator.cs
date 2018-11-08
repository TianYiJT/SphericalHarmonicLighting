using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public struct IntegratorTask
{
	public float[] coffeeSHResult;
}

public class SHFunc
{

	public static int[] LM16;

	public static void tranferK(int k,ref int l,ref int m)
	{
		l = (int)Mathf.Sqrt (k);
		m = k - l*(l+1);
	}

	public static void Init()
	{
		LM16 = new int[32];
		LM16 [0] = 0;
		LM16 [16] = 0;

		LM16 [1] = 1;
		LM16 [17] = -1;
		LM16 [2] = 1;
		LM16 [18] = 0;
		LM16 [3] = 1;
		LM16 [19] = 1;

		LM16 [4] = 2;
		LM16 [20] = -2;
		LM16 [5] = 2;
		LM16 [21] = -1;
		LM16 [6] = 2;
		LM16 [22] = 0;
		LM16 [7] = 2;
		LM16 [23] = 1;
		LM16 [8] = 2;
		LM16 [24] = 2;

		LM16 [9] = 3;
		LM16 [25] = -3;
		LM16 [10] = 3;
		LM16 [26] = -2;
		LM16 [11] = 3;
		LM16 [27] = -1;
		LM16 [12] = 3;
		LM16 [28] = 0;
		LM16 [13] = 3;
		LM16 [29] = 1;
		LM16 [14] = 3;
		LM16 [30] = 2;
		LM16 [15] = 3;
		LM16 [31] = 3;

	}

	//Evaluate an Associated Legendre Polynomial P(l, m) at x
	private static float P(int l, int m, float x)
	{
		//First generate the value of P(m, m) at x
		float pmm=1.0f;

		if(m>0)
		{
			float sqrtOneMinusX2=Mathf.Sqrt(1.0f-x*x);

			float fact=1.0f;

			for(int i=1; i<=m; ++i)
			{
				pmm*=(-fact)*sqrtOneMinusX2;
				fact+=2.0f;
			}
		}

		//If l==m, P(l, m)==P(m, m)
		if(l==m)
			return pmm;

		//Use rule 3 to calculate P(m+1, m) from P(m, m)
		float pmp1m=x*(2.0f*m+1.0f)*pmm;

		//If l==m+1, P(l, m)==P(m+1, m)
		if(l==m+1)
			return pmp1m;

		//Otherwise, l>m+1.
		//Iterate rule 1 to get the result
		float plm=0.0f;

		for(int i=m+2; i<=l; ++i)
		{
			plm=((2.0f*i-1.0f)*x*pmp1m-(i+m-1.0f)*pmm)/(i-m);
			pmm=pmp1m;
			pmp1m=plm;
		}

		return plm;
	}

	private static float K(int l, int m)
	{
		float temp=((2.0f*l+1.0f)*Factorial(l-m))/((4.0f*Mathf.PI)*Factorial(l+m));

		return Mathf.Sqrt(temp);
	}

	//Sample a spherical harmonic basis function Y(l, m) at a point on the unit sphere
	public static float SH(int l, int m, float theta, float phi)
	{
		float sqrt2=Mathf.Sqrt(2.0f);

		if(m==0)
			return K(l, 0)*P(l, m, Mathf.Cos(theta));

		if(m>0)
			return sqrt2*K(l, m)*Mathf.Cos(m*phi)*P(l, m, Mathf.Cos(theta));

		//m<0
		return sqrt2*K(l,-m)*Mathf.Sin(-m*phi)*P(l, -m, Mathf.Cos(theta));
	}


	//Calculate n! (n>=0)
	private static int Factorial(int n)
	{
		if(n<=1)
			return 1;

		int result=n;

		while(--n > 1)
			result*=n;

		return result;
	}
}


public class coffeeSHIntegrator : MonoBehaviour 
{

	public int maxPower;

	public int maxSamples;

	public float albedo;

	public Mesh SHMeshObject;

	private IntegratorTask[] AllIntegratorTasks;

	//private float preComputeTime;

	public string OutputPath;

	void Start ()
	{

		int allTaskNum = SHMeshObject.vertexCount;

		AllIntegratorTasks = new IntegratorTask[allTaskNum];
		int tempIndex = 0;
		for (int j = 0; j < SHMeshObject.vertexCount; j++)
		{
			AllIntegratorTasks[j].coffeeSHResult = new float[maxPower];
		}

		SHFunc.Init ();

		for (int i = 0; i < allTaskNum; i++)
		{
			preCompute (i);
		}
			
		Output ();

	}
		
	private void preCompute(int i)
	{
		Vector3 position = SHMeshObject.vertices[i];
		Vector3 normal = SHMeshObject.normals[i];

		Vector2[] randomVec = MCIntegrator (maxSamples);

		float[] result = new float[maxPower];
		for (int j = 0; j < maxPower; j++)
			result [j] = 0.0f;

		for(int j=0;j<maxSamples;j++)
		{
			Vector3 tempDir = tranfer (randomVec[j].x,randomVec[j].y);
			float csn = Mathf.Clamp01(Vector3.Dot (tempDir.normalized,normal.normalized));
			float shadow = RayCast (position,tempDir);
			for (int k = 0; k < maxPower; k++) 
			{
				int l = 0;
				int m=0;
				SHFunc.tranferK (k,ref l,ref m);
				float y = SHFunc.SH (l,m,randomVec[j].x,randomVec[j].y);
				result [k] += y * shadow * csn*albedo;
			}
		}

		float factor = 4.0f * Mathf.PI / maxSamples;

		for (int j = 0; j < maxPower; j++)
		{
			AllIntegratorTasks [i].coffeeSHResult [j] = result [j] * factor;
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
		
	private void Output()
	{
		string[] myOutput = new string[AllIntegratorTasks.Length];
		for (int i = 0; i < AllIntegratorTasks.Length; i++)
		{
			string temp = "";
			for (int j = 0; j < AllIntegratorTasks [i].coffeeSHResult.Length; j++) 
			{
				temp+=AllIntegratorTasks[i].coffeeSHResult[j].ToString()+" ";
			}
			myOutput [i] = temp;
		}
	//	myOutput [AllIntegratorTasks.Length] = preComputeTime.ToString ();
		System.IO.File.WriteAllLines(@OutputPath, myOutput, Encoding.UTF8);
	}


	private float RayCast(Vector3 Position,Vector3 Direction)
	{
		RaycastHit hit;
		if (Physics.Raycast (Position, Direction, 5000.0f))
		{
			return 0.0f;
		}
		else
		{
			return 1.0f;
		}
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

}
