using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000055 RID: 85
public class SpriteBounce : MonoBehaviour
{
	// Token: 0x0600076E RID: 1902 RVA: 0x000666B1 File Offset: 0x000648B1
	public void SetUp(float freq, float spd)
	{
		this.frequency = freq;
		this.speed = spd;
	}

	// Token: 0x0600076F RID: 1903 RVA: 0x000666C1 File Offset: 0x000648C1
	public IEnumerator Gradual(float freq, float spd, float frametime)
	{
		float a = 0f;
		float f = this.frequency;
		float s = this.speed;
		do
		{
			this.frequency = Mathf.Lerp(f, freq, a / frametime);
			this.speed = Mathf.Lerp(s, spd, a / frametime);
			a += MainManager.framestep;
			yield return null;
		}
		while (a < frametime + 1f);
		yield break;
	}

	// Token: 0x06000770 RID: 1904 RVA: 0x000666E8 File Offset: 0x000648E8
	private void Start()
	{
		if (base.GetComponent<SpriteRenderer>() != null)
		{
			base.GetComponent<SpriteRenderer>().material.color = this.spritecolor;
		}
		if (this.facecamera)
		{
			base.gameObject.AddComponent<FaceCamera>();
		}
		if (this.startscale)
		{
			this.basescale = base.transform.localScale;
		}
	}

	// Token: 0x06000771 RID: 1905 RVA: 0x00066748 File Offset: 0x00064948
	private void FixedUpdate()
	{
		if (this.CanHappen())
		{
			base.transform.localScale = this.basescale + new Vector3(Mathf.Sin(Time.time * this.speed) * this.frequency * this.maxx, Mathf.Cos(Time.time * this.speed + this.ydifference) * this.frequency * this.maxy, 0f);
		}
	}

	// Token: 0x06000772 RID: 1906 RVA: 0x000667C4 File Offset: 0x000649C4
	private bool CanHappen()
	{
		if (this.requiresflag > -1)
		{
			if (this.requiresflag >= 0 && !MainManager.instance.flags[this.requiresflag])
			{
				return false;
			}
			if (this.requiresflag < 0 && !MainManager.instance.regionalflags[Mathf.Abs(this.requiresflag)])
			{
				return false;
			}
		}
		return this.requiresentity <= -1 || !(MainManager.map.entities[this.requiresentity] != null) || !(MainManager.map.entities[this.requiresentity].npcdata != null) || MainManager.map.entities[this.requiresentity].npcdata.hit;
	}

	// Token: 0x06000773 RID: 1907 RVA: 0x0006687B File Offset: 0x00064A7B
	public void MessageBounce()
	{
		this.MessageBounce(1f);
	}

	// Token: 0x06000774 RID: 1908 RVA: 0x00066888 File Offset: 0x00064A88
	public void MessageBounce(float multiplier)
	{
		this.frequency = 0.1f * multiplier;
		this.speed = 7f * multiplier;
	}

	// Token: 0x0400079E RID: 1950
	public float maxx = 0.75f;

	// Token: 0x0400079F RID: 1951
	public float maxy = 0.75f;

	// Token: 0x040007A0 RID: 1952
	public float frequency = 0.5f;

	// Token: 0x040007A1 RID: 1953
	public float speed = 0.5f;

	// Token: 0x040007A2 RID: 1954
	public float ydifference = 1f;

	// Token: 0x040007A3 RID: 1955
	public Vector3 basescale = Vector3.one;

	// Token: 0x040007A4 RID: 1956
	public Color spritecolor = Color.white;

	// Token: 0x040007A5 RID: 1957
	public bool facecamera;

	// Token: 0x040007A6 RID: 1958
	public bool startscale;

	// Token: 0x040007A7 RID: 1959
	public int requiresflag = -1;

	// Token: 0x040007A8 RID: 1960
	public int requiresentity = -1;
}
