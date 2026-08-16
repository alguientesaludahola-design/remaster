using System;
using UnityEngine;

// Token: 0x02000012 RID: 18
public class DestroyOnLayer : MonoBehaviour
{
	// Token: 0x060001A8 RID: 424 RVA: 0x00014801 File Offset: 0x00012A01
	public void SetUp(string deathparticle, float particletime, int targetlayer, Vector3 partoffset, Vector3 partangle)
	{
		this.SetUp(deathparticle, particletime, targetlayer, partoffset, partangle, false);
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x00014811 File Offset: 0x00012A11
	public void SetUp(string deathparticle, float particletime, int targetlayer, Vector3 partoffset, Vector3 partangle, bool actuallydontdestroy)
	{
		this.particle = deathparticle;
		this.layerid = targetlayer;
		this.parttime = particletime;
		this.offset = partoffset;
		this.angle = partangle;
		this.dontdestroy = actuallydontdestroy;
	}

	// Token: 0x060001AA RID: 426 RVA: 0x00014840 File Offset: 0x00012A40
	public void Kill()
	{
		if (this.particle != null)
		{
			MainManager.PlayParticle(this.particle, null, base.transform.position + this.offset, this.angle, this.parttime);
		}
		if (!this.dontdestroy)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (base.transform.parent != null)
		{
			base.transform.parent.position = new Vector3(0f, -9999f);
			return;
		}
		base.transform.position = new Vector3(0f, -9999f);
	}

	// Token: 0x060001AB RID: 427 RVA: 0x000148E5 File Offset: 0x00012AE5
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == this.layerid)
		{
			this.Kill();
		}
	}

	// Token: 0x04000150 RID: 336
	private string particle;

	// Token: 0x04000151 RID: 337
	private float parttime;

	// Token: 0x04000152 RID: 338
	private int layerid;

	// Token: 0x04000153 RID: 339
	private bool dontdestroy;

	// Token: 0x04000154 RID: 340
	private Vector3 offset;

	// Token: 0x04000155 RID: 341
	private Vector3 angle;
}
