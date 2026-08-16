using System;
using UnityEngine;

// Token: 0x0200001D RID: 29
public class FaderRange : MonoBehaviour
{
	// Token: 0x0600037D RID: 893 RVA: 0x000236C8 File Offset: 0x000218C8
	private void Start()
	{
		this.render = base.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < this.render.Length; i++)
		{
			this.render[i].tag = "NoMapColor";
		}
		for (int j = 0; j < this.render.Length; j++)
		{
			for (int k = 0; k < this.render[j].materials.Length; k++)
			{
				if (this.render[j].materials[k].shader == MainManager.outlinemain.shader)
				{
					this.render[j].materials[k].renderQueue = 3000;
				}
			}
		}
	}

	// Token: 0x0600037E RID: 894 RVA: 0x00023774 File Offset: 0x00021974
	private void LateUpdate()
	{
		if (this.framecount <= 0f && MainManager.player != null)
		{
			Vector3 vector = Vector3.zero;
			if (!this.player)
			{
				vector = MainManager.MainCamera.WorldToViewportPoint(base.transform.position + this.pivot);
			}
			else
			{
				vector = MainManager.player.transform.position;
			}
			for (int i = 0; i < this.render.Length; i++)
			{
				if (this.render[i].material.shader == MainManager.fakelight)
				{
					this.render[i].material.renderQueue = 3000 + (int)(vector.z * 100f);
				}
				else if (this.render[i].material.shader == MainManager.Fade3D.shader || this.render[i].material.shader == MainManager.Main3D.shader)
				{
					if (this.player)
					{
						float num = Vector3.Distance(base.transform.position + this.pivot, MainManager.player.transform.position);
						bool flag = num >= this.maxdistance || num <= this.mindistance;
						if (this.invert)
						{
							flag = !flag;
						}
						this.Fade(flag, i);
					}
					else if (vector.z > this.mindistance && vector.z < this.maxdistance)
					{
						this.Fade(this.invert, i);
					}
					else
					{
						this.Fade(!this.invert, i);
					}
					if (!this.ignorematerial)
					{
						if (this.render[i].material.color.a > 0.9f)
						{
							this.render[i].material.shader = MainManager.Main3D.shader;
							this.render[i].material.color = Color.white;
						}
						else
						{
							this.render[i].material.shader = MainManager.Fade3D.shader;
						}
					}
				}
			}
			this.framecount = (float)this.framestep;
			return;
		}
		this.framecount -= MainManager.TieFramerate(1f);
	}

	// Token: 0x0600037F RID: 895 RVA: 0x000239C8 File Offset: 0x00021BC8
	private void Fade(bool fade, int i)
	{
		if (fade)
		{
			if (this.render[i].material.shader == MainManager.Fade3D.shader || this.ignorematerial)
			{
				this.render[i].material.color = Color.Lerp(this.render[i].material.color, this.color, MainManager.TieFramerate(this.fadedelay));
				return;
			}
		}
		else
		{
			this.render[i].material.color = Color.Lerp(this.render[i].material.color, new Color(this.color.r, this.color.g, this.color.b, this.fadepercent), MainManager.TieFramerate(this.fadedelay));
		}
	}

	// Token: 0x040002A0 RID: 672
	public float maxdistance = 2f;

	// Token: 0x040002A1 RID: 673
	public float mindistance = 1f;

	// Token: 0x040002A2 RID: 674
	public float fadedelay = 0.075f;

	// Token: 0x040002A3 RID: 675
	public float fadepercent = 0.2f;

	// Token: 0x040002A4 RID: 676
	public Color color;

	// Token: 0x040002A5 RID: 677
	public Vector3 pivot;

	// Token: 0x040002A6 RID: 678
	public bool invert;

	// Token: 0x040002A7 RID: 679
	public bool player;

	// Token: 0x040002A8 RID: 680
	public bool ignorematerial;

	// Token: 0x040002A9 RID: 681
	public int framestep = 1;

	// Token: 0x040002AA RID: 682
	private float framecount;

	// Token: 0x040002AB RID: 683
	private Renderer[] render;
}
