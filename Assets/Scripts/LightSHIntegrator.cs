using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

public class LightSHIntegrator : MonoBehaviour
{

	public Cubemap mCubeMap;

	public int maxSamples;

	public int maxPower;

	private Vector3[] LightSH;

	public string OutputPath;


	// Use this for initialization
	void Start () 
	{
		LightSH = new Vector3[maxPower];
		SHFunc.Init ();

		LightIntegrator ();

		Output ();

	}

	void LightIntegrator()
	{
		Vector2[] Samples = MCIntegrator (maxSamples);

		float factor = 4 * Mathf.PI / maxSamples;

		Vector3[] result = new Vector3[maxPower];
		for (int i = 0; i < maxPower; i++)
			result [i] = Vector3.zero;

		for (int i = 0; i < maxSamples; i++)
		{
			Color sampleColor = SampleCubeMap (Samples[i].x,Samples[i].y);
			for (int j = 0; j < maxPower; j++)
			{
				int l =0;
				int m =0;
				SHFunc.tranferK (j,ref l,ref m);
				float y = SHFunc.SH (l,m,Samples[i].x,Samples[i].y);
				result [j] += new Vector3 (sampleColor.r*y,sampleColor.g*y,sampleColor.b*y);
			}
		}

		for (int i = 0; i < maxPower; i++)
		{
			LightSH [i] = factor * result[i];
		}

	}


	void Output()
	{
		string[] myoutput = new string[maxPower];

		for (int i = 0; i < maxPower; i++) 
		{
			myoutput [i] = LightSH [i].x.ToString()+" "+LightSH [i].y.ToString()+" "+LightSH [i].z.ToString();
		}

		System.IO.File.WriteAllLines(@OutputPath, myoutput, Encoding.UTF8);

	}


	Color SampleCubeMap(float theta,float phi)
	{

		Vector3 CastVec = tranfer (theta,phi);

		CubemapFace HitFace = JudgeHitWhatFace (CastVec);

		Vector2 HitPixel = hitPoint (CastVec, HitFace);

		Color targetColor = mCubeMap.GetPixel (HitFace,(int)(HitPixel.x*mCubeMap.width+0.5f),(int)(HitPixel.y*mCubeMap.height+0.5f));

	//	Debug.Log (CastVec.ToString()+" "+HitFace.ToString()+" "+HitPixel.ToString());



		return targetColor;
	}

	CubemapFace JudgeHitWhatFace(Vector3 v)
	{
		float absX = Mathf.Abs (v.x);
		float absY = Mathf.Abs (v.y);
		float absZ = Mathf.Abs (v.z);

		if (absX > absY && absX > absZ && v.x > 0)
			return CubemapFace.PositiveX;
		if (absX > absY && absX > absZ && v.x < 0)
			return CubemapFace.NegativeX;
		if (absY > absX && absY > absZ && v.y > 0)
			return CubemapFace.PositiveY;
		if (absY > absX && absY > absZ && v.y < 0)
			return CubemapFace.NegativeY;
		if (absZ > absX && absZ > absY && v.z > 0)
			return CubemapFace.PositiveZ;
		if (absZ > absX && absZ > absY && v.z < 0)
			return CubemapFace.NegativeZ;

		return CubemapFace.Unknown;
	}

	Vector2 hitPoint(Vector3 Cast,CubemapFace face)
	{
		if (face == CubemapFace.PositiveX) 
		{
			Vector3 vertical= new Vector3 (1.0f,0.0f,0.0f);
			float costheta = Vector3.Dot (Cast,vertical);
			float Length = 0.5f / costheta;
			Vector3 hit = Cast * Length;
			return new Vector2 (hit.z + 0.5f,Mathf.Abs(hit.y-0.5f));
		}
		if (face == CubemapFace.NegativeX) 
		{
			Vector3 vertical= new Vector3 (-1.0f,0.0f,0.0f);
			float costheta = Vector3.Dot (Cast,vertical);
			float Length = 0.5f / costheta;
			Vector3 hit = Cast * Length;
			return new Vector2 (Mathf.Abs(hit.z-0.5f),Mathf.Abs(hit.y-0.5f));
		}
		if (face == CubemapFace.PositiveY) 
		{
			Vector3 vertical= new Vector3 (0.0f,1.0f,0.0f);
			float costheta = Vector3.Dot (Cast,vertical);
			float Length = 0.5f / costheta;
			Vector3 hit = Cast * Length;
			return new Vector2 (hit.x + 0.5f,Mathf.Abs(hit.z-0.5f));
		}
		if (face == CubemapFace.NegativeY) 
		{
			Vector3 vertical= new Vector3 (0.0f,-1.0f,0.0f);
			float costheta = Vector3.Dot (Cast,vertical);
			float Length = 0.5f / costheta;
			Vector3 hit = Cast * Length;
			return new Vector2 (Mathf.Abs(hit.x+0.5f),Mathf.Abs(hit.z+0.5f));
		}
		if (face == CubemapFace.PositiveZ) 
		{
			Vector3 vertical= new Vector3 (0.0f,0.0f,1.0f);
			float costheta = Vector3.Dot (Cast,vertical);
			float Length = 0.5f / costheta;
			Vector3 hit = Cast * Length;
			return new Vector2 (Mathf.Abs(hit.x-0.5f),Mathf.Abs(hit.y-0.5f));
		}
		if (face == CubemapFace.NegativeZ) 
		{
			Vector3 vertical= new Vector3 (0.0f,0.0f,-1.0f);
			float costheta = Vector3.Dot (Cast,vertical);
			float Length = 0.5f / costheta;
			Vector3 hit = Cast * Length;
			return new Vector2 (Mathf.Abs(hit.x+0.5f),Mathf.Abs(hit.y-0.5f));
		}
		return new Vector2 (0,0);
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

}
