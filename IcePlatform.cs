using System;
using UnityEngine;

// Token: 0x02000033 RID: 51
public class IcePlatform : MonoBehaviour
{
	// Token: 0x06000424 RID: 1060 RVA: 0x0002AB44 File Offset: 0x00028D44
	private void Start()
	{
		if (this.obj == null)
		{
			this.obj = base.transform.GetChild(0).gameObject;
		}
		this.part = base.GetComponentInChildren<ParticleSystem>();
		this.psize = this.obj.transform.localScale;
		this.obj.transform.localScale = Vector3.zero;
		this.growstep = this.growspeedstep;
		this.col = this.obj.GetComponentInChildren<Collider>();
	}

	// Token: 0x06000425 RID: 1061 RVA: 0x0002ABCC File Offset: 0x00028DCC
	private void LateUpdate()
	{
		float num = MainManager.TieFramerate(1f);
		if (this.check > 0f)
		{
			this.check -= num;
		}
		else
		{
			this.frozen = (MainManager.map.stencilid > -1 && MainManager.map.entities[MainManager.map.stencilid] != null && MainManager.map.entities[MainManager.map.stencilid].npcdata.hit && Vector3.Distance(base.transform.position + this.center, MainManager.map.entities[MainManager.map.stencilid].transform.position) < MainManager.map.entities[MainManager.map.stencilid].npcdata.vectordata[0].y * 1.85f);
			if (this.part != null)
			{
				if (this.frozen && this.part.isPlaying)
				{
					this.part.Stop();
				}
				else if (!this.frozen && !this.part.isPlaying)
				{
					this.part.Play();
				}
			}
			if (this.oldf != this.frozen && this.playsound)
			{
				MainManager.PlaySoundAt(this.frozen ? "Freeze" : "IceMelt", 1f, base.transform.position + this.center);
			}
			this.oldf = this.frozen;
			this.check = 4f;
			if (this.col != null)
			{
				this.col.enabled = (this.obj.transform.localScale.magnitude > 0.5f);
			}
		}
		if (this.frozen)
		{
			this.growstep -= num;
		}
		else
		{
			this.growstep += num;
		}
		this.growstep = Mathf.Clamp(this.growstep, 0f, this.growspeedstep);
		if (this.smoothgrow)
		{
			this.obj.transform.localScale = MainManager.SmoothLerp(this.psize, Vector3.zero, this.growstep / this.growspeedstep);
			return;
		}
		this.obj.transform.localScale = Vector3.Lerp(this.psize, Vector3.zero, this.growstep / this.growspeedstep);
	}

	// Token: 0x040003CF RID: 975
	public Vector3 center;

	// Token: 0x040003D0 RID: 976
	public float growspeedstep = 30f;

	// Token: 0x040003D1 RID: 977
	public bool smoothgrow = true;

	// Token: 0x040003D2 RID: 978
	public bool playsound = true;

	// Token: 0x040003D3 RID: 979
	private float check;

	// Token: 0x040003D4 RID: 980
	private float growstep;

	// Token: 0x040003D5 RID: 981
	private bool frozen;

	// Token: 0x040003D6 RID: 982
	private bool oldf;

	// Token: 0x040003D7 RID: 983
	private Collider col;

	// Token: 0x040003D8 RID: 984
	private ParticleSystem part;

	// Token: 0x040003D9 RID: 985
	public GameObject obj;

	// Token: 0x040003DA RID: 986
	private Vector3 psize;

	// Token: 0x040003DB RID: 987
	private const float checkinterval = 4f;
}
