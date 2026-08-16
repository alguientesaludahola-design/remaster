using System;
using UnityEngine;

// Token: 0x02000045 RID: 69
public class ParticleRange : MonoBehaviour
{
	// Token: 0x060006F7 RID: 1783 RVA: 0x00059B10 File Offset: 0x00057D10
	private void Start()
	{
		this.part = base.GetComponentInChildren<ParticleSystem>();
		if (this.getmeshes)
		{
			this.render = base.GetComponentsInChildren<MeshRenderer>();
		}
		if (this.getmaterialcolor && this.render != null)
		{
			this.matcolor = this.render[0].material.color;
		}
	}

	// Token: 0x060006F8 RID: 1784 RVA: 0x00059B68 File Offset: 0x00057D68
	private void LateUpdate()
	{
		if (MainManager.player != null && this.part != null)
		{
			if (this.delay <= 0)
			{
				if (Vector3.Distance(MainManager.player.transform.position, base.transform.position) <= this.radius)
				{
					if (!this.part.isPlaying)
					{
						this.part.Play();
					}
					this.inrange = true;
				}
				else
				{
					if (this.part.isPlaying)
					{
						this.part.Stop();
					}
					this.inrange = false;
				}
				this.delay = 2;
			}
			else
			{
				this.delay--;
			}
		}
		if (this.render != null)
		{
			this.fadeamt = Mathf.Clamp(this.fadeamt + MainManager.TieFramerate((float)((this.inrange && !this.invertfademesh) ? 1 : -1)), 0f, this.fadeframeammount);
			for (int i = 0; i < this.render.Length; i++)
			{
				this.render[i].material.color = Color.Lerp(Color.clear, this.matcolor, this.fadeamt / this.fadeframeammount);
			}
		}
	}

	// Token: 0x04000692 RID: 1682
	public float radius;

	// Token: 0x04000693 RID: 1683
	public float fadeframeammount = 45f;

	// Token: 0x04000694 RID: 1684
	public bool getmaterialcolor;

	// Token: 0x04000695 RID: 1685
	public bool getmeshes;

	// Token: 0x04000696 RID: 1686
	public bool fademesh;

	// Token: 0x04000697 RID: 1687
	public bool invertfademesh;

	// Token: 0x04000698 RID: 1688
	public Color matcolor;

	// Token: 0x04000699 RID: 1689
	private ParticleSystem part;

	// Token: 0x0400069A RID: 1690
	public MeshRenderer[] render;

	// Token: 0x0400069B RID: 1691
	private int delay;

	// Token: 0x0400069C RID: 1692
	private float fadeamt;

	// Token: 0x0400069D RID: 1693
	private bool inrange;

	// Token: 0x0400069E RID: 1694
	private const int delayamt = 2;
}
