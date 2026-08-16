using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000050 RID: 80
public class ScrewPlatform : MonoBehaviour
{
	// Token: 0x06000758 RID: 1880 RVA: 0x00065654 File Offset: 0x00063854
	private void Start()
	{
		this.startpos = (this.platformrotate ? (this.rotatelocal ? base.transform.localEulerAngles : base.transform.eulerAngles) : base.transform.position);
		this.startangle = base.transform.eulerAngles;
		if (!this.platformrotate)
		{
			this.target += this.startpos;
		}
		base.transform.tag = "PlatformNoClock";
		base.gameObject.layer = 13;
		List<NPCControl> list = new List<NPCControl>();
		for (int i = 0; i < this.linkedentities.Length; i++)
		{
			if (this.linkedentities[i] > -1 && MainManager.GetEntity(this.linkedentities[i]) != null)
			{
				list.Add(MainManager.GetEntity(this.linkedentities[i]).npcdata);
			}
		}
		this.switches = list.ToArray();
		this.icamwaittime = this.camwaittime;
		this.changecamposb = (this.changecampos.magnitude > 0.1f);
	}

	// Token: 0x06000759 RID: 1881 RVA: 0x0006576C File Offset: 0x0006396C
	private void Update()
	{
		if (!MainManager.instance.message && !MainManager.instance.minipause && !MainManager.instance.inevent && !MainManager.instance.pause)
		{
			this.oneactive = this.GetActive();
			bool flag = this.IsActive();
			float num = (this.switches.Length == 1 || this.oneactive > -1) ? (1f - this.switches[this.oneactive].actioncooldown / this.switches[this.oneactive].vectordata[0].z) : 1f;
			if (flag)
			{
				if (this.turnflagonactive >= 0)
				{
					MainManager.instance.flags[this.turnflagonactive] = true;
				}
				if (this.nonscrewswitch)
				{
					num = 0f;
				}
				if (this.changecamposb && !this.camchange && (this.changecamonlyabovethis <= 0.1f || this.a >= this.changecamonlyabovethis))
				{
					if (!this.resetcamtoplayer)
					{
						MainManager.SaveCameraPosition();
					}
					MainManager.instance.camtarget = null;
					MainManager.instance.camtargetpos = new Vector3?(this.changecampos);
					MainManager.instance.camspeed = this.camspeed;
					this.camchange = true;
					this.camwaittime = this.icamwaittime;
					ScrewPlatform.camischanging = true;
				}
				if (this.a < this.timeractive)
				{
					this.a += MainManager.TieFramerate(1f) - num;
				}
				if (this.eventFlagFrames.z > 1f && ((int)this.eventFlagFrames.y == -1 || !MainManager.instance.flags[(int)this.eventFlagFrames.y]) && this.eventframes < this.eventFlagFrames.z)
				{
					this.eventframes += MainManager.TieFramerate(1f);
				}
			}
			else
			{
				if (this.nonscrewswitch)
				{
					num = 1f;
				}
				this.eventframes = 0f;
				if (this.a > 0f)
				{
					this.a -= MainManager.TieFramerate(this.deactivemultiplier) * num;
				}
				if (this.changecamposb && this.camchange)
				{
					if (this.camwaittime > 0f)
					{
						this.camwaittime -= MainManager.TieFramerate(1f);
					}
					else
					{
						if (!this.resetcamtoplayer)
						{
							MainManager.LoadCameraPosition();
						}
						else
						{
							MainManager.ResetCamera();
						}
						this.camchange = false;
					}
				}
			}
			ScrewPlatform.Type type = this.type;
			if (type != ScrewPlatform.Type.Platform)
			{
				if (type == ScrewPlatform.Type.Rotater)
				{
					for (int i = 0; i < this.rotaters.Length; i++)
					{
						if (this.a / this.timeractive > 0f)
						{
							this.rotaters[i].transform.Rotate(this.rotateammount[i] * (this.a / this.timeractive) * MainManager.TieFramerate(1f));
						}
						else if (this.cardinalammount > 0f)
						{
							switch (this.cardinaldir)
							{
							case ScrewPlatform.Cardinal.X:
								this.rotaters[i].transform.localEulerAngles = new Vector3(Mathf.LerpAngle(this.rotaters[i].transform.localEulerAngles.x, (float)(Mathf.RoundToInt(this.rotaters[i].transform.localEulerAngles.x / 90f) * 90), MainManager.TieFramerate(this.cardinalammount)), 0f, 0f);
								break;
							case ScrewPlatform.Cardinal.Y:
								this.rotaters[i].transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(this.rotaters[i].transform.localEulerAngles.y, (float)(Mathf.RoundToInt(this.rotaters[i].transform.localEulerAngles.y / 90f) * 90), MainManager.TieFramerate(this.cardinalammount)), 0f);
								break;
							case ScrewPlatform.Cardinal.Z:
								this.rotaters[i].transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(this.rotaters[i].transform.localEulerAngles.z, (float)(Mathf.RoundToInt(this.rotaters[i].transform.localEulerAngles.z / 90f) * 90), MainManager.TieFramerate(this.cardinalammount)));
								break;
							case ScrewPlatform.Cardinal.StartAngle:
								this.rotaters[i].transform.eulerAngles = MainManager.LerpVectorAngle(this.rotaters[i].transform.eulerAngles, this.startangle, MainManager.TieFramerate(this.cardinalammount));
								break;
							}
						}
					}
				}
			}
			else if (this.platformrotate)
			{
				if (this.rotatelocal)
				{
					base.transform.localEulerAngles = MainManager.LerpVectorAngle(this.startpos, this.target, this.a / this.timeractive) + ((this.a > 1f) ? MainManager.RandomVector(this.shakewhenmoving) : Vector3.zero);
				}
				else
				{
					base.transform.eulerAngles = MainManager.LerpVectorAngle(this.startpos, this.target, this.a / this.timeractive) + ((this.a > 1f) ? MainManager.RandomVector(this.shakewhenmoving) : Vector3.zero);
				}
			}
			else
			{
				if (this.smoothmovement)
				{
					base.transform.position = MainManager.SmoothLerp(base.transform.position, Vector3.Lerp(this.startpos, this.target, this.a / this.timeractive) + ((this.a > 1f) ? MainManager.RandomVector(this.shakewhenmoving) : Vector3.zero), MainManager.framestep * 0.05f);
				}
				else
				{
					base.transform.position = Vector3.Lerp(this.startpos, this.target, this.a / this.timeractive) + ((this.a > 1f) ? MainManager.RandomVector(this.shakewhenmoving) : Vector3.zero);
				}
				if (this.rotateammount != null && this.rotateammount.Length != 0)
				{
					base.transform.eulerAngles = MainManager.LerpVectorAngle(this.startangle, this.rotateammount[0], this.a / this.timeractive);
				}
			}
			if ((int)this.eventFlagFrames.x > -1 && this.eventFlagFrames.z > 1f && ((int)this.eventFlagFrames.y == -1 || !MainManager.instance.flags[(int)this.eventFlagFrames.y]) && this.eventframes >= this.eventFlagFrames.z)
			{
				MainManager.events.StartEvent((int)this.eventFlagFrames.x, null);
				this.eventframes = 0f;
			}
			if (this.lastactive != flag)
			{
				if (this.soundonactive.Length > 0 && MainManager.SoundIsPlaying(this.soundonactive) == -1)
				{
					MainManager.PlaySound(this.soundonactive);
				}
				this.lastactive = flag;
			}
		}
	}

	// Token: 0x0600075A RID: 1882 RVA: 0x00065EB8 File Offset: 0x000640B8
	private void LateUpdate()
	{
		if (!ScrewPlatform.camischanging && this.changecamonlyabovethis > 0.1f && !this.IsActive() && this.changecampos.magnitude > 0.1f && MainManager.FreePlayer() && MainManager.instance.camtarget != MainManager.player.transform && MainManager.instance.camtargetpos != null && Vector3.Distance(MainManager.instance.camtargetpos.Value, this.changecampos) < 0.5f)
		{
			MainManager.ResetCamera();
			this.camwaittime = this.icamwaittime;
			this.camchange = false;
			ScrewPlatform.camischanging = false;
		}
	}

	// Token: 0x0600075B RID: 1883 RVA: 0x00065F70 File Offset: 0x00064170
	private int GetActive()
	{
		if (this.switches.Length == 1)
		{
			return 0;
		}
		for (int i = 0; i < this.switches.Length; i++)
		{
			if (this.switches[i].hit)
			{
				if (this.oneactive == -1)
				{
					this.oneactive = i;
				}
				else if (this.oneactive != i)
				{
					return -1;
				}
			}
		}
		return this.oneactive;
	}

	// Token: 0x0600075C RID: 1884 RVA: 0x00065FD0 File Offset: 0x000641D0
	private bool IsActive()
	{
		if (this.or)
		{
			for (int i = 0; i < this.switches.Length; i++)
			{
				if (this.switches[i].hit)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < this.switches.Length; j++)
		{
			if (!this.switches[j].hit)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x0400075E RID: 1886
	public MainManager.Maps map;

	// Token: 0x0400075F RID: 1887
	public ScrewPlatform.Cardinal cardinaldir;

	// Token: 0x04000760 RID: 1888
	public static bool camischanging;

	// Token: 0x04000761 RID: 1889
	public ScrewPlatform.Type type;

	// Token: 0x04000762 RID: 1890
	public int[] linkedentities;

	// Token: 0x04000763 RID: 1891
	private NPCControl[] switches;

	// Token: 0x04000764 RID: 1892
	public Transform[] rotaters;

	// Token: 0x04000765 RID: 1893
	public Vector3[] rotateammount;

	// Token: 0x04000766 RID: 1894
	public bool or;

	// Token: 0x04000767 RID: 1895
	public bool resetcamtoplayer;

	// Token: 0x04000768 RID: 1896
	public bool nonscrewswitch;

	// Token: 0x04000769 RID: 1897
	public bool platformrotate;

	// Token: 0x0400076A RID: 1898
	public bool rotatelocal;

	// Token: 0x0400076B RID: 1899
	public bool invertYZforentity = true;

	// Token: 0x0400076C RID: 1900
	public bool smoothmovement;

	// Token: 0x0400076D RID: 1901
	public bool camchange;

	// Token: 0x0400076E RID: 1902
	public string soundonactive;

	// Token: 0x0400076F RID: 1903
	public Vector3 target;

	// Token: 0x04000770 RID: 1904
	public Vector3 shakewhenmoving;

	// Token: 0x04000771 RID: 1905
	public Vector3 eventFlagFrames = new Vector3(-1f, -1f, -1f);

	// Token: 0x04000772 RID: 1906
	public Vector3 changecampos;

	// Token: 0x04000773 RID: 1907
	private Vector3 startpos;

	// Token: 0x04000774 RID: 1908
	private Vector3 startangle;

	// Token: 0x04000775 RID: 1909
	private int oneactive = -1;

	// Token: 0x04000776 RID: 1910
	public float timeractive = 60f;

	// Token: 0x04000777 RID: 1911
	public float deactivemultiplier = 0.25f;

	// Token: 0x04000778 RID: 1912
	public float cardinalammount;

	// Token: 0x04000779 RID: 1913
	public float camspeed = 0.02f;

	// Token: 0x0400077A RID: 1914
	public float changecamonlyabovethis;

	// Token: 0x0400077B RID: 1915
	public float camwaittime = 50f;

	// Token: 0x0400077C RID: 1916
	public float entityscale = 1f;

	// Token: 0x0400077D RID: 1917
	public int turnflagonactive = -1;

	// Token: 0x0400077E RID: 1918
	private float a;

	// Token: 0x0400077F RID: 1919
	private float eventframes;

	// Token: 0x04000780 RID: 1920
	private float icamwaittime;

	// Token: 0x04000781 RID: 1921
	private bool lastactive;

	// Token: 0x04000782 RID: 1922
	private bool changecamposb;

	// Token: 0x02000279 RID: 633
	public enum Type
	{
		// Token: 0x04002105 RID: 8453
		Platform,
		// Token: 0x04002106 RID: 8454
		Rotater
	}

	// Token: 0x0200027A RID: 634
	public enum Cardinal
	{
		// Token: 0x04002108 RID: 8456
		X,
		// Token: 0x04002109 RID: 8457
		Y,
		// Token: 0x0400210A RID: 8458
		Z,
		// Token: 0x0400210B RID: 8459
		StartAngle
	}
}
