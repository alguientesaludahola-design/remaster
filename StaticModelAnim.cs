using System;
using UnityEngine;

// Token: 0x02000058 RID: 88
public class StaticModelAnim : MonoBehaviour
{
	// Token: 0x0600078D RID: 1933 RVA: 0x00069060 File Offset: 0x00067260
	private void Start()
	{
		this.startpos = base.transform.position;
		this.startangle = base.transform.eulerAngles;
		this.render = base.gameObject.GetComponent<Renderer>();
		if (this.bobfreq != Vector3.zero || this.bobspeed != Vector3.zero)
		{
			base.gameObject.isStatic = false;
		}
	}

	// Token: 0x0600078E RID: 1934 RVA: 0x000690D0 File Offset: 0x000672D0
	private void Update()
	{
		this.current += this.speed * MainManager.framestep;
		if (this.internaltimer && (!this.pausetied || !MainManager.IsPaused()) && (this.requiresflag == -1 || MainManager.instance.flags[this.requiresflag]))
		{
			this.internalt += MainManager.framestep;
		}
	}

	// Token: 0x0600078F RID: 1935 RVA: 0x00069143 File Offset: 0x00067343
	private float Timer()
	{
		if (this.internaltimer)
		{
			return this.internalt * Time.deltaTime;
		}
		return Time.time;
	}

	// Token: 0x06000790 RID: 1936 RVA: 0x0006915F File Offset: 0x0006735F
	private void LateUpdate()
	{
		if (this.onlyonce)
		{
			base.enabled = false;
		}
	}

	// Token: 0x06000791 RID: 1937 RVA: 0x00069170 File Offset: 0x00067370
	private void FixedUpdate()
	{
		if (!this.pausetied || !MainManager.IsPaused() || this.firstcycle)
		{
			if (this.current.x > this.limitmax.x)
			{
				this.current = new Vector2(this.limitmin.x, this.current.y);
			}
			if (this.current.y > this.limitmax.y)
			{
				this.current = new Vector2(this.current.x, this.limitmin.y);
			}
			if (this.current.x < this.limitmin.x)
			{
				this.current = new Vector2(this.limitmax.x, this.current.y);
			}
			if (this.current.y < this.limitmin.y)
			{
				this.current = new Vector2(this.current.x, this.limitmax.y);
			}
			if (this.render != null)
			{
				this.render.material.SetTextureOffset("_MainTex", this.current);
			}
			if (!this.nomove && (this.bobspeed != Vector3.zero || this.bobspeed != Vector3.zero))
			{
				if (this.bobangle != Vector3.zero)
				{
					float num = this.startangle.x;
					float num2 = this.startangle.y;
					float num3 = this.startangle.z;
					if (this.bobangle.x != 0f)
					{
						num += Mathf.Sin(this.bobspeed.x * this.Timer()) * this.bobfreq.x * this.bobangle.x;
					}
					if (this.bobangle.y != 0f)
					{
						num2 += Mathf.Sin(this.bobspeed.y * this.Timer()) * this.bobfreq.y * this.bobangle.y;
					}
					if (this.bobangle.z != 0f)
					{
						num3 += Mathf.Sin(this.bobspeed.z * this.Timer()) * this.bobfreq.z * this.bobangle.z;
					}
					base.transform.eulerAngles = new Vector3(num, num2, num3);
				}
				else if (!this.stopbob)
				{
					base.transform.position = this.startpos + new Vector3(Mathf.Sin(this.bobspeed.x * Time.time) * this.bobfreq.x, Mathf.Sin(this.bobspeed.y * Time.time) * this.bobfreq.y, this.bobspeed.z * Time.time * this.bobfreq.z);
				}
			}
			this.firstcycle = false;
		}
	}

	// Token: 0x040007CA RID: 1994
	public Vector2 speed;

	// Token: 0x040007CB RID: 1995
	public Vector2 current;

	// Token: 0x040007CC RID: 1996
	public Vector2 limitmin;

	// Token: 0x040007CD RID: 1997
	public Vector2 limitmax = Vector2.one;

	// Token: 0x040007CE RID: 1998
	public Vector3 bobspeed;

	// Token: 0x040007CF RID: 1999
	public Vector3 bobfreq;

	// Token: 0x040007D0 RID: 2000
	public Vector3 bobangle;

	// Token: 0x040007D1 RID: 2001
	public Vector3 conveyor;

	// Token: 0x040007D2 RID: 2002
	public bool nomove;

	// Token: 0x040007D3 RID: 2003
	public bool pausetied;

	// Token: 0x040007D4 RID: 2004
	public bool internaltimer;

	// Token: 0x040007D5 RID: 2005
	public bool onlyonce;

	// Token: 0x040007D6 RID: 2006
	public bool firstcycle;

	// Token: 0x040007D7 RID: 2007
	public bool stopbob;

	// Token: 0x040007D8 RID: 2008
	public int requiresflag = -1;

	// Token: 0x040007D9 RID: 2009
	private Renderer render;

	// Token: 0x040007DA RID: 2010
	private int freezetime;

	// Token: 0x040007DB RID: 2011
	public float internalt;

	// Token: 0x040007DC RID: 2012
	private Vector3 startpos;

	// Token: 0x040007DD RID: 2013
	private Vector3 startangle;
}
