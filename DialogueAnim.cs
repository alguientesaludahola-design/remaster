using System;
using UnityEngine;

// Token: 0x02000013 RID: 19
public class DialogueAnim : MonoBehaviour
{
	// Token: 0x060001AD RID: 429 RVA: 0x00014900 File Offset: 0x00012B00
	public void SetUp(Vector3 startsize, Vector3 tsize, Vector3 tpos, float spd)
	{
		this.speed = spd;
		this.targetpos = tpos;
		this.targetscale = tsize;
		base.transform.localScale = startsize;
	}

	// Token: 0x060001AE RID: 430 RVA: 0x00014924 File Offset: 0x00012B24
	public void SetUp(Vector3 tsize, Vector3 tpos, float spd)
	{
		this.speed = spd;
		this.targetpos = tpos;
		this.targetscale = tsize;
	}

	// Token: 0x060001AF RID: 431 RVA: 0x0001493B File Offset: 0x00012B3B
	public void SetUp(Vector3 tpos, float spd)
	{
		this.speed = spd;
		this.targetpos = tpos;
	}

	// Token: 0x060001B0 RID: 432 RVA: 0x0001494C File Offset: 0x00012B4C
	private void FixedUpdate()
	{
		if (this.flipx)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, new Vector3(0f, base.transform.localScale.y * this.multiplier, 1f), this.shrinkspeed);
		}
		else if (this.flipy)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, new Vector3(base.transform.localScale.x * this.multiplier, 0f, 1f), this.shrinkspeed);
		}
		else if (this.shrink)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, Vector3.zero, this.shrinkspeed);
		}
		else
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, this.targetscale * this.multiplier, this.shrinkspeed);
		}
		if (this.targetpos != Vector3.zero)
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this.targetpos, this.speed);
		}
	}

	// Token: 0x04000156 RID: 342
	public bool shrink;

	// Token: 0x04000157 RID: 343
	public bool flipx;

	// Token: 0x04000158 RID: 344
	public bool flipy;

	// Token: 0x04000159 RID: 345
	public bool localpos;

	// Token: 0x0400015A RID: 346
	public Vector3 targetpos;

	// Token: 0x0400015B RID: 347
	public Vector3 targetscale = Vector3.one;

	// Token: 0x0400015C RID: 348
	public float speed;

	// Token: 0x0400015D RID: 349
	public float multiplier = 1f;

	// Token: 0x0400015E RID: 350
	public float shrinkspeed = 0.5f;
}
