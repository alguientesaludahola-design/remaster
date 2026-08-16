using System;
using UnityEngine;

// Token: 0x0200002B RID: 43
public class GlowTrigger : MonoBehaviour
{
	// Token: 0x060003DC RID: 988 RVA: 0x00028178 File Offset: 0x00026378
	private void Start()
	{
		if (this.targetentityid > -1)
		{
			this.parent = MainManager.GetEntity(this.targetentityid).npcdata;
		}
		if (this.glowparts == null || this.glowparts.Length == 0)
		{
			this.glowparts = new MeshRenderer[]
			{
				base.GetComponentInChildren<MeshRenderer>()
			};
		}
		if (this.getactivecolorfromstart)
		{
			this.tcolor = new Color[]
			{
				this.glowparts[0].materials[this.materialid].color,
				this.glowparts[0].materials[this.materialid].GetColor("_Emission")
			};
		}
		if (this.electime != 0f)
		{
			if (!this.nosound)
			{
				this.sound = base.gameObject.AddComponent<AudioSource>();
				this.sound.spatialBlend = 1f;
				this.sound.maxDistance = 1f;
				this.sound.playOnAwake = false;
				this.sound.volume = MainManager.soundvolume;
			}
			this.elecharz = base.GetComponent<Hazards>();
			if (this.elecharz == null)
			{
				this.elecharz = base.gameObject.AddComponent<Hazards>();
			}
			this.hbox = base.GetComponent<BoxCollider>();
			if (this.hbox == null)
			{
				BoxCollider component = this.elecharz.GetComponent<BoxCollider>();
				component.center = new Vector3(0f, 999f);
				component.isTrigger = true;
			}
			this.eleccd = this.electime;
			this.elecp = (Object.Instantiate(Resources.Load("Prefabs/Particles/Elec")) as GameObject).GetComponent<ParticleSystem>();
			this.elecp.Stop();
			this.elecp.transform.parent = base.transform;
			this.elecp.transform.localPosition = new Vector3(0f, -1f);
			this.elecp.transform.localScale = Vector3.one;
		}
		this.initialtcolor = this.tcolor;
		for (int i = 0; i < this.glowparts.Length; i++)
		{
			this.glowparts[i].tag = "NoMapColor";
		}
	}

	// Token: 0x060003DD RID: 989 RVA: 0x000283A4 File Offset: 0x000265A4
	private void PlaySound(string name)
	{
		this.PlaySound(name, false);
	}

	// Token: 0x060003DE RID: 990 RVA: 0x000283AE File Offset: 0x000265AE
	private void PlaySound(string name, bool loop)
	{
		this.sound.clip = Resources.Load<AudioClip>("Audio/Sounds/" + name);
		this.sound.loop = loop;
		this.sound.Play();
	}

	// Token: 0x060003DF RID: 991 RVA: 0x000283E4 File Offset: 0x000265E4
	private void LateUpdate()
	{
		bool flag = this.Active() || this.force;
		if (!flag && this.sound != null && this.sound.isPlaying)
		{
			this.sound.Stop();
		}
		if (this.countdown == 0)
		{
			for (int i = 0; i < this.glowparts.Length; i++)
			{
				if (this.electime > 0f && this.eleccd < 100f && flag)
				{
					this.sin = Mathf.Sin(Time.time * 10f);
					Color color = (this.eleccd < 0f) ? Color.yellow : ((this.sin < 0f) ? Color.red : this.tcolor[0]);
					this.glowparts[i].materials[this.materialid].color = color;
					this.glowparts[i].materials[this.materialid].SetColor("_Emission", color);
				}
				else
				{
					if (flag)
					{
						if (this.getactivecolorfromstart)
						{
							this.glowparts[i].materials[this.materialid].color = Color.Lerp(this.glowparts[i].materials[this.materialid].color, this.tcolor[0], MainManager.TieFramerate(this.glowspeed));
							this.glowparts[i].materials[this.materialid].SetColor("_Emission", Color.Lerp(this.glowparts[i].materials[this.materialid].GetColor("_Emission"), this.tcolor[1], MainManager.TieFramerate(this.glowspeed)));
						}
						else
						{
							this.glowparts[i].materials[this.materialid].color = Color.Lerp(this.glowparts[i].materials[this.materialid].color, this.activecolor, MainManager.TieFramerate(this.glowspeed));
							this.glowparts[i].materials[this.materialid].SetColor("_Emission", this.glowparts[i].materials[this.materialid].color);
						}
					}
					else
					{
						if (this.sound != null)
						{
							this.sound.Stop();
						}
						this.glowparts[i].materials[this.materialid].color = Color.Lerp(this.glowparts[i].materials[this.materialid].color, this.deactivatedcolor, MainManager.TieFramerate(this.glowspeed));
						this.glowparts[i].materials[this.materialid].SetColor("_Emission", Color.Lerp(this.glowparts[i].materials[this.materialid].GetColor("_Emission"), Color.black, MainManager.TieFramerate(this.glowspeed)));
					}
					if (this.elecsound)
					{
						this.sound.Stop();
						this.elecsound = false;
					}
				}
			}
			this.countdown = this.refreshdelay;
		}
		else
		{
			this.countdown--;
		}
		if (flag && !MainManager.IsPaused())
		{
			if (this.electime > 0f && MainManager.FreePlayer(false))
			{
				if (this.eleccd > -this.elecstay)
				{
					this.eleccd -= MainManager.TieFramerate(1f);
					if (this.eleccd < 0f && MainManager.player != null && !MainManager.player.shield)
					{
						this.hbox.center = new Vector3(0f, 1f, 0f);
					}
					else
					{
						this.hbox.center = new Vector3(0f, 999f);
					}
					if (this.eleccd < 0f)
					{
						if (!this.elecp.isPlaying)
						{
							this.elecp.Play();
						}
					}
					else
					{
						this.elecp.Stop();
					}
				}
				else
				{
					this.hbox.center = new Vector3(0f, 999f);
					this.eleccd = this.electime;
				}
			}
		}
		else if (this.electime > 0f)
		{
			if (this.elecp.isPlaying)
			{
				this.elecp.Stop();
			}
			this.hbox.center = new Vector3(0f, 999f);
		}
		if (this.sound != null && flag)
		{
			if (this.eleccd > 100f)
			{
				this.sound.Stop();
			}
			else if (!this.sound.isPlaying)
			{
				if (this.eleccd < 0f)
				{
					this.PlaySound("ShockLoop", true);
				}
				else if (this.sin > 0f)
				{
					this.PlaySound("Alarm2");
				}
			}
			this.sound.volume = MainManager.soundvolume * (MainManager.FreePlayer(false) ? 1f : 0.5f);
		}
	}

	// Token: 0x060003E0 RID: 992 RVA: 0x00028908 File Offset: 0x00026B08
	private bool Active()
	{
		return ((this.parent != null && this.parent.hit) || (this.flagid > -1 && MainManager.instance.flags[this.flagid]) || ((int)this.flagvar.x > -1 && MainManager.instance.flagvar[(int)this.flagvar.x] >= (int)this.flagvar.y)) == !this.invert;
	}

	// Token: 0x04000363 RID: 867
	public NPCControl parent;

	// Token: 0x04000364 RID: 868
	public int refreshdelay = 2;

	// Token: 0x04000365 RID: 869
	public int targetentityid = -1;

	// Token: 0x04000366 RID: 870
	public int flagid = -1;

	// Token: 0x04000367 RID: 871
	public int materialid;

	// Token: 0x04000368 RID: 872
	private int countdown;

	// Token: 0x04000369 RID: 873
	public MeshRenderer[] glowparts;

	// Token: 0x0400036A RID: 874
	public float glowspeed = 0.2f;

	// Token: 0x0400036B RID: 875
	public float eleccd;

	// Token: 0x0400036C RID: 876
	public bool getactivecolorfromstart;

	// Token: 0x0400036D RID: 877
	public bool force;

	// Token: 0x0400036E RID: 878
	public bool nosound;

	// Token: 0x0400036F RID: 879
	public bool invert;

	// Token: 0x04000370 RID: 880
	private bool oldactive;

	// Token: 0x04000371 RID: 881
	private float sin;

	// Token: 0x04000372 RID: 882
	public Vector2 flagvar = new Vector2(-1f, 0f);

	// Token: 0x04000373 RID: 883
	public float electime;

	// Token: 0x04000374 RID: 884
	public float elecstay = 100f;

	// Token: 0x04000375 RID: 885
	private Color[] tcolor;

	// Token: 0x04000376 RID: 886
	private Color[] initialtcolor;

	// Token: 0x04000377 RID: 887
	public Color deactivatedcolor = Color.gray;

	// Token: 0x04000378 RID: 888
	private Hazards elecharz;

	// Token: 0x04000379 RID: 889
	private BoxCollider hbox;

	// Token: 0x0400037A RID: 890
	public ParticleSystem elecp;

	// Token: 0x0400037B RID: 891
	public Color activecolor = Color.cyan;

	// Token: 0x0400037C RID: 892
	private bool soundplay;

	// Token: 0x0400037D RID: 893
	private bool elecsound;

	// Token: 0x0400037E RID: 894
	private bool cansoundplay;

	// Token: 0x0400037F RID: 895
	private AudioSource sound;
}
