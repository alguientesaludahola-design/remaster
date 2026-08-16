using System;
using UnityEngine;

// Token: 0x02000016 RID: 22
public class ElecThing : MonoBehaviour
{
	// Token: 0x060001BE RID: 446 RVA: 0x00014FDC File Offset: 0x000131DC
	private void Start()
	{
		this.r = base.GetComponent<LineRenderer>();
		this.p = new Vector3[this.r.positionCount - 2];
		this.pp = new Vector3[this.p.Length];
		for (int i = 0; i < this.p.Length; i++)
		{
			this.p[i] = this.r.GetPosition(i + 1);
			this.pp[i] = this.p[i];
		}
	}

	// Token: 0x060001BF RID: 447 RVA: 0x00015068 File Offset: 0x00013268
	private void LateUpdate()
	{
		if (this.cooldown <= 0f)
		{
			for (int i = 0; i < this.p.Length; i++)
			{
				this.pp[i] = this.p[i] + MainManager.RandomVector(2f);
				this.r.SetPosition(i + 1, this.pp[i]);
			}
			this.cooldown = 30f + Random.Range(0f, 10f);
			return;
		}
		this.cooldown -= MainManager.framestep;
		for (int j = 0; j < this.p.Length; j++)
		{
			this.r.SetPosition(j + 1, this.pp[j] + MainManager.RandomVector(0.25f));
		}
	}

	// Token: 0x0400017B RID: 379
	private LineRenderer r;

	// Token: 0x0400017C RID: 380
	private Vector3[] p;

	// Token: 0x0400017D RID: 381
	private Vector3[] pp;

	// Token: 0x0400017E RID: 382
	private float delay;

	// Token: 0x0400017F RID: 383
	private float cooldown;

	// Token: 0x04000180 RID: 384
	private const float time = 30f;

	// Token: 0x04000181 RID: 385
	private const float off = 10f;

	// Token: 0x04000182 RID: 386
	private const float d = 2f;

	// Token: 0x04000183 RID: 387
	private const float dd = 0.25f;
}
