using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000005 RID: 5
public class Audience : MonoBehaviour
{
	// Token: 0x0600001B RID: 27 RVA: 0x000023D0 File Offset: 0x000005D0
	private void Start()
	{
		this.entities = new Animator[this.ammount];
		this.time = new float[this.ammount];
		this.type = new int[this.ammount];
		this.startpos = new Vector3[this.ammount];
		for (int i = 0; i < this.ammount; i++)
		{
			this.entities[i] = (Object.Instantiate(Resources.Load("Prefabs/Objects/Audience")) as GameObject).GetComponent<Animator>();
			this.entities[i].transform.parent = base.transform;
			this.entities[i].transform.localPosition = MainManager.RandomVector(new Vector3(this.spawnarea.x / 2f, 0f, this.spawnarea.y / 2f));
			this.entities[i].transform.localEulerAngles = new Vector3(0f, 180f);
			this.entities[i].gameObject.AddComponent<ShadowLite>().SetUp(0.3f, 0.5f);
			this.startpos[i] = this.entities[i].transform.localPosition;
			this.time[i] = Random.Range(500f, 1500f);
			switch (this.animtype)
			{
			case Audience.Type.MothAntBeetle:
				this.type[i] = Random.Range(0, 3);
				break;
			case Audience.Type.OnlyMoth:
				this.type[i] = 0;
				break;
			case Audience.Type.OnlyAnt:
				this.type[i] = 1;
				break;
			case Audience.Type.OnlyBeetle:
				this.type[i] = 2;
				break;
			case Audience.Type.OnlyBee:
				this.type[i] = 3;
				break;
			case Audience.Type.All:
				this.type[i] = Random.Range(0, 4);
				break;
			case Audience.Type.Termites:
				this.type[i] = Random.Range(4, 6);
				break;
			}
			this.entities[i].Play(this.type[i] + "_0");
			this.SetColor(i);
		}
		this.delaycd = ((this.ammount > 20 || this.lowfps) ? 2f : 1.5f);
		this.currentammount = this.ammount;
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002610 File Offset: 0x00000810
	private void OnEnable()
	{
		if (this.entities != null && this.type != null && this.entities.Length != 0 && this.type.Length != 0)
		{
			for (int i = 0; i < this.ammount; i++)
			{
				if (this.entities[i] != null)
				{
					this.entities[i].Play(this.type[i] + "_0");
				}
			}
		}
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00002684 File Offset: 0x00000884
	public void Jump()
	{
		this.jumping = base.StartCoroutine(this.AudienceJump());
	}

	// Token: 0x0600001E RID: 30 RVA: 0x00002698 File Offset: 0x00000898
	private void SetColor(int id)
	{
		SpriteRenderer[] componentsInChildren = this.entities[id].GetComponentsInChildren<SpriteRenderer>();
		switch (this.type[id])
		{
		case 0:
			componentsInChildren[0].material.color = Color.Lerp(Color.yellow, Color.red, Random.Range(0.2f, 0.8f));
			break;
		case 1:
			componentsInChildren[0].material.color = Color.Lerp(Color.yellow, Color.red, Random.Range(0.5f, 0.9f));
			break;
		case 2:
			componentsInChildren[0].material.color = Color.Lerp(Color.green, Color.blue, Random.Range(0.2f, 0.8f));
			break;
		case 3:
			componentsInChildren[0].material.color = Color.Lerp(Color.yellow, Color.red, Random.Range(0.2f, 0.45f));
			break;
		case 4:
			componentsInChildren[0].material.color = Color.Lerp(Color.white, Color.yellow, Random.Range(0.2f, 0.5f));
			break;
		case 5:
			componentsInChildren[0].material.color = Color.Lerp(Color.red, Color.yellow, Random.Range(0.3f, 0.45f));
			break;
		}
		componentsInChildren[1].material.color = componentsInChildren[0].material.color;
	}

	// Token: 0x0600001F RID: 31 RVA: 0x00002810 File Offset: 0x00000A10
	public void RefreshAnim()
	{
		for (int i = 0; i < this.ammount; i++)
		{
			this.entities[i].Play(this.type[i] + "_0");
		}
	}

	// Token: 0x06000020 RID: 32 RVA: 0x00002852 File Offset: 0x00000A52
	private IEnumerator AudienceJump()
	{
		Vector2 cj = this.constantjump;
		this.constantjump = Vector2.zero;
		float a = 0f;
		int[] r = new int[this.ammount];
		Vector3[] rot = new Vector3[this.ammount];
		for (int i = 0; i < this.ammount; i++)
		{
			r[i] = Random.Range(0, 3);
			this.entities[i].Play(this.type[i] + "_1");
			rot[i] = this.entities[i].transform.localEulerAngles;
		}
		while (a < 30f)
		{
			for (int j = 0; j < this.ammount; j++)
			{
				if (r[j] >= 1)
				{
					this.entities[j].transform.localPosition = MainManager.BeizierCurve3(this.startpos[j], this.startpos[j], 3f, a / 30f);
				}
				if (r[j] == 2)
				{
					this.entities[j].transform.localEulerAngles += new Vector3(0f, 20f, 0f);
				}
			}
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		for (int k = 0; k < this.ammount; k++)
		{
			this.entities[k].Play(this.type[k] + "_0");
			this.entities[k].transform.localEulerAngles = rot[k];
		}
		this.jumping = null;
		this.constantjump = cj;
		yield return null;
		yield break;
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00002864 File Offset: 0x00000A64
	private void LateUpdate()
	{
		if (this.delay <= 0f)
		{
			if (this.constantjump.magnitude > 0.1f)
			{
				for (int i = 0; i < this.ammount; i++)
				{
					this.entities[i].transform.localPosition = this.startpos[i] + new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * this.constantjump.x * (this.time[i] / 100f)) * this.constantjump.y) - 0.15f);
				}
			}
			else if (this.jumping == null)
			{
				float num = MainManager.framestep * Mathf.Clamp(this.delaycd, 1f, float.PositiveInfinity);
				for (int j = 0; j < this.ammount; j++)
				{
					this.entities[j].transform.localEulerAngles = Vector3.Lerp(this.entities[j].transform.localEulerAngles, new Vector3(0f, (float)((this.time[j] > 0f) ? 180 : 0)), MainManager.TieFramerate(0.15f));
					this.time[j] -= num;
					if (this.time[j] < -40f)
					{
						this.time[j] = Random.Range(500f, 1500f);
					}
				}
			}
			this.delay = this.delaycd;
			return;
		}
		this.delay -= MainManager.framestep;
	}

	// Token: 0x04000005 RID: 5
	public Audience.Type animtype;

	// Token: 0x04000006 RID: 6
	public int ammount;

	// Token: 0x04000007 RID: 7
	public int currentammount;

	// Token: 0x04000008 RID: 8
	public Vector2 spawnarea;

	// Token: 0x04000009 RID: 9
	public Vector2 constantjump;

	// Token: 0x0400000A RID: 10
	public bool noflip;

	// Token: 0x0400000B RID: 11
	public bool lowfps;

	// Token: 0x0400000C RID: 12
	[HideInInspector]
	public Animator[] entities;

	// Token: 0x0400000D RID: 13
	private float[] time;

	// Token: 0x0400000E RID: 14
	private int[] type;

	// Token: 0x0400000F RID: 15
	private float delay;

	// Token: 0x04000010 RID: 16
	private float delaycd;

	// Token: 0x04000011 RID: 17
	private Vector3[] startpos;

	// Token: 0x04000012 RID: 18
	private const float defaulttime = 1500f;

	// Token: 0x04000013 RID: 19
	private const int maxammount = 4;

	// Token: 0x04000014 RID: 20
	private Coroutine jumping;

	// Token: 0x02000060 RID: 96
	public enum Type
	{
		// Token: 0x04000924 RID: 2340
		MothAntBeetle,
		// Token: 0x04000925 RID: 2341
		OnlyMoth,
		// Token: 0x04000926 RID: 2342
		OnlyAnt,
		// Token: 0x04000927 RID: 2343
		OnlyBeetle,
		// Token: 0x04000928 RID: 2344
		OnlyBee,
		// Token: 0x04000929 RID: 2345
		All,
		// Token: 0x0400092A RID: 2346
		Termites
	}
}
