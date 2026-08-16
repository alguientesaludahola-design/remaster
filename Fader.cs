using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200001C RID: 28
public class Fader : MonoBehaviour
{
	// Token: 0x06000378 RID: 888 RVA: 0x00022B08 File Offset: 0x00020D08
	private void Start()
	{
		Fader.grasslands = ((MainManager.map != null && MainManager.map.faderchange) || MainManager.instance.areaid == 8 || MainManager.instance.areaid == 19 || MainManager.instance.areaid == 9);
		this.renders = base.GetComponentsInChildren<Renderer>(true);
		this.faderender = new List<Transform>();
		if (this.randomchildcolod)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Renderer component = base.transform.GetChild(i).GetComponent<Renderer>();
				if (component != null)
				{
					if (component.material.HasProperty("_EmissionColor"))
					{
						component.material.SetColor("_EmissionColor", new Color(Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f)));
					}
					if (component.material.HasProperty("_Color"))
					{
						component.material.color = new Color(Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f));
					}
				}
			}
		}
		List<Material[]> list = new List<Material[]>();
		this.modes = new ShadowCastingMode[this.renders.Length];
		List<bool[]> list2 = new List<bool[]>();
		for (int j = 0; j < this.renders.Length; j++)
		{
			list.Add(this.renders[j].materials);
			this.modes[j] = this.renders[j].shadowCastingMode;
			bool[] array = new bool[this.renders[j].sharedMaterials.Length];
			for (int k = 0; k < this.renders[j].sharedMaterials.Length; k++)
			{
				if (this.renders[j].sharedMaterials[k] != null && this.renders[j].sharedMaterials[k].HasProperty("_Color"))
				{
					array[k] = true;
				}
			}
			list2.Add(array);
		}
		this.hascolor = list2.ToArray();
		this.mats = list.ToArray();
	}

	// Token: 0x06000379 RID: 889 RVA: 0x00022D58 File Offset: 0x00020F58
	private bool CheckY()
	{
		return this.yoffset < 0f || (MainManager.player != null && Mathf.Abs(base.transform.position.y + this.pivotoffset.y - MainManager.player.transform.position.y) < this.yoffset);
	}

	// Token: 0x0600037A RID: 890 RVA: 0x00022DC4 File Offset: 0x00020FC4
	private void LateUpdate()
	{
		if (this.initialcolors == null || this.initialcolors.Count == 0)
		{
			this.initialcolors = new List<Color[]>();
			for (int i = 0; i < this.renders.Length; i++)
			{
				List<Color> list = new List<Color>();
				for (int j = 0; j < this.renders[i].materials.Length; j++)
				{
					Material material = this.renders[i].materials[j];
					if (material.HasProperty("_Color"))
					{
						list.Add(material.color);
					}
				}
				this.initialcolors.Add(list.ToArray());
			}
		}
		if ((!this.forcestayonpause || MainManager.FreePlayer(false)) && Time.frameCount % 3 == 0)
		{
			if (!Fader.grasslands && !this.alwaysfade)
			{
				for (int k = 0; k < this.renders.Length; k++)
				{
					if (!this.renders[k].gameObject.CompareTag("NoFader"))
					{
						if (!this.CheckY())
						{
							this.renders[k].shadowCastingMode = ShadowCastingMode.On;
						}
						else if (this.childtied && k > 0)
						{
							this.renders[k].shadowCastingMode = this.renders[0].shadowCastingMode;
						}
						else
						{
							MainManager.DisableRender(this.renders[k], this.zdistance, this.pivotoffset);
						}
					}
				}
				return;
			}
			Vector3 vector = MainManager.MainCamera.WorldToViewportPoint(base.transform.position + this.pivotoffset);
			for (int l = 0; l < this.renders.Length; l++)
			{
				if (!this.renders[l].gameObject.CompareTag("NoFader"))
				{
					for (int m = 0; m < this.renders[l].materials.Length; m++)
					{
						if (this.fadedistance > -1f && this.renders[l].material.shader != MainManager.fakelight && this.renders[l].material.shader != MainManager.emptymat.shader)
						{
							if (this.renders[l].materials[m].shader == MainManager.outlinemain.shader)
							{
								this.renders[l].materials[m].renderQueue = 3000;
							}
							else
							{
								this.UpdateShader(l, m);
							}
						}
					}
					if (this.insideid > -2 && this.insideid != MainManager.instance.insideid)
					{
						this.renders[l].enabled = false;
					}
					else if (this.childtied && l > 0)
					{
						this.renders[l].shadowCastingMode = this.renders[0].shadowCastingMode;
						this.renders[l].enabled = this.renders[0].enabled;
					}
					else if (MainManager.player != null && !this.CheckY())
					{
						this.renders[l].shadowCastingMode = ShadowCastingMode.On;
					}
					else if (this.zdistance > -1f)
					{
						if (this.checkx < 0.1f)
						{
							MainManager.DisableRender(this.renders[l], this.zdistance, this.pivotoffset);
						}
						else if (MainManager.GetDistance(MainManager.MainCamera.transform.position.x, base.transform.position.x + this.pivotoffset.x) > this.checkx)
						{
							this.renders[l].enabled = true;
						}
						else
						{
							MainManager.DisableRender(this.renders[l], this.zdistance, this.pivotoffset);
						}
					}
				}
			}
			if (this.ignoreY)
			{
				this.inrange = (MainManager.player != null && MainManager.GetSqrDistance(new Vector3(base.transform.position.x, 0f, base.transform.position.z) + new Vector3(this.pivotoffset.x, 0f, this.pivotoffset.z), new Vector3(MainManager.MainCamera.transform.position.x, 0f, MainManager.MainCamera.transform.position.z), true) < this.fadedistance);
				return;
			}
			this.inrange = (MainManager.player != null && MainManager.GetSqrDistance(base.transform.position + this.pivotoffset, MainManager.MainCamera.transform.position, true) < this.fadedistance && MainManager.MainCamera.WorldToViewportPoint(MainManager.player.transform.position).z > vector.z && this.CheckY());
		}
	}

	// Token: 0x0600037B RID: 891 RVA: 0x000232B4 File Offset: 0x000214B4
	private void UpdateShader(int i, int j)
	{
		if (this.hascolor[i][j])
		{
			if (this.renders[i].materials[j].shader == MainManager.Main3D.shader || this.renders[i].materials[j].shader == MainManager.Fade3D.shader)
			{
				if (this.renders[i].materials[j].color.a >= 0.9f)
				{
					this.renders[i].materials[j].shader = MainManager.Main3D.shader;
				}
				else
				{
					this.renders[i].materials[j].shader = MainManager.Fade3D.shader;
				}
			}
			else if (this.renders[i].materials[j].shader == MainManager.fadePlane.shader || this.renders[i].materials[j].shader == MainManager.mainPlane.shader)
			{
				if (!this.faderender.Contains(this.renders[i].transform) && !this.dontclone)
				{
					this.faderender.Add(this.renders[i].transform);
					Renderer component = Object.Instantiate<GameObject>(this.renders[i].gameObject).GetComponent<Renderer>();
					this.faderender.Add(component.transform);
					component.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
					component.receiveShadows = false;
					component.transform.position = this.renders[i].transform.position;
					component.transform.eulerAngles = this.renders[i].transform.eulerAngles;
					component.material.color = Color.white;
					component.gameObject.isStatic = true;
					component.gameObject.tag = "NoFader";
					component.transform.parent = this.renders[i].transform;
					component.transform.localScale = Vector3.one;
					if (component.GetComponent<Fader>() != null)
					{
						Object.Destroy(component.GetComponent<Fader>());
					}
				}
				if (this.renders[i].materials[j].color.a >= 0.9f)
				{
					this.renders[i].materials[j].shader = MainManager.mainPlane.shader;
				}
				else
				{
					this.renders[i].materials[j].shader = MainManager.fadePlane.shader;
				}
			}
			if (this.initialcolors != null && this.initialcolors.Count > 0 && this.initialcolors[i].Length != 0 && j <= this.initialcolors[i].Length - 1)
			{
				Color color = this.initialcolors[i][j];
				if (this.renders[i].materials[j].shader == MainManager.Fade3D.shader)
				{
					color = MainManager.map.skycolor;
				}
				color = new Color(color.r, color.g, color.b, this.renders[i].materials[j].color.a);
				if (this.inrange)
				{
					this.renders[i].materials[j].color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0.3f), MainManager.TieFramerate(this.fadespeed));
					return;
				}
				this.renders[i].materials[j].color = Color.Lerp(color, new Color(color.r, color.g, color.b, 1f), MainManager.TieFramerate(this.fadespeed));
			}
		}
	}

	// Token: 0x04000288 RID: 648
	public Renderer[] renders;

	// Token: 0x04000289 RID: 649
	private List<Renderer> rex = new List<Renderer>();

	// Token: 0x0400028A RID: 650
	public float fadedistance = 35f;

	// Token: 0x0400028B RID: 651
	public float zdistance;

	// Token: 0x0400028C RID: 652
	public float fadespeed = 0.075f;

	// Token: 0x0400028D RID: 653
	public float yoffset = -1f;

	// Token: 0x0400028E RID: 654
	public float checkx;

	// Token: 0x0400028F RID: 655
	public int insideid = -1;

	// Token: 0x04000290 RID: 656
	public bool randomchildcolod;

	// Token: 0x04000291 RID: 657
	public bool childtied;

	// Token: 0x04000292 RID: 658
	public bool ignoreY;

	// Token: 0x04000293 RID: 659
	public bool dontclone;

	// Token: 0x04000294 RID: 660
	public bool forcestayonpause;

	// Token: 0x04000295 RID: 661
	public bool alwaysfade;

	// Token: 0x04000296 RID: 662
	public Vector3 pivotoffset;

	// Token: 0x04000297 RID: 663
	private bool inrange;

	// Token: 0x04000298 RID: 664
	private static bool grasslands;

	// Token: 0x04000299 RID: 665
	private List<Color[]> initialcolors;

	// Token: 0x0400029A RID: 666
	private const float hidevalue = 0.3f;

	// Token: 0x0400029B RID: 667
	private const int framedelay = 3;

	// Token: 0x0400029C RID: 668
	private bool[][] hascolor;

	// Token: 0x0400029D RID: 669
	public Material[][] mats;

	// Token: 0x0400029E RID: 670
	public ShadowCastingMode[] modes;

	// Token: 0x0400029F RID: 671
	private List<Transform> faderender;
}
