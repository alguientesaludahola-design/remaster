using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002F RID: 47
[RequireComponent(typeof(BoxCollider))]
public class Hazards : MonoBehaviour
{
	// Token: 0x06000404 RID: 1028 RVA: 0x000292DC File Offset: 0x000274DC
	private void Start()
	{
		if (this.platformlimit > 3)
		{
			this.platformlimit = 3;
		}
		this.col = base.GetComponent<BoxCollider>();
		this.col.isTrigger = true;
		this.collidercenter = this.col.center;
		if (base.tag == "WaterFloat")
		{
			MainManager.map.waterfloat = this;
		}
		if (!this.keeplayer)
		{
			base.gameObject.layer = 0;
		}
		this.sanim = base.GetComponentInChildren<StaticModelAnim>();
		switch (this.type)
		{
		case Hazards.Type.Water:
		case Hazards.Type.Honey:
			this.tobj = Resources.Load<GameObject>("Prefabs/Objects/" + this.type.ToString() + "Splash");
			if (!this.noice)
			{
				this.iceplatforms = new Hazards.IcePlatforms[this.platformlimit];
				this.lastplatfomrs = new int[]
				{
					-1,
					-1
				};
				for (int i = 0; i < this.platformlimit; i++)
				{
					this.iceplatforms[i].obj = (Object.Instantiate(Resources.Load("Prefabs/Objects/iceplatform"), new Vector3(0f, -9999f, 554f), Quaternion.identity) as GameObject);
					this.iceplatforms[i].obj.transform.localScale = Vector3.zero;
					this.iceplatforms[i].obj.transform.parent = (this.iceparentedtomap ? MainManager.map.transform : base.transform);
					this.iceplatforms[i].obj.transform.GetChild(0).tag = "PushPlatform";
					this.iceplatforms[i].obj.transform.GetChild(0).gameObject.AddComponent<DestroyOnLayer>().SetUp("IceShatter", 1f, 16, Vector3.up, Vector3.zero, true);
				}
			}
			if (this.waterfloats != null)
			{
				for (int j = 0; j < this.waterfloats.Length; j++)
				{
					this.waterfloats[j].gameObject.isStatic = false;
				}
			}
			MainManager.map.lastwater = this;
			break;
		case Hazards.Type.Hole:
			MainManager.map.ylimit = -150f;
			this.taudio = Resources.Load<AudioClip>("Audio/Sounds/Falling");
			return;
		case Hazards.Type.WalkableSpike:
			break;
		case Hazards.Type.TempFire:
			this.timer = this.speed;
			this.fireanim = base.GetComponentInChildren<Animator>();
			return;
		default:
			return;
		}
	}

	// Token: 0x06000405 RID: 1029 RVA: 0x00029564 File Offset: 0x00027764
	private void Update()
	{
		if (this.type == Hazards.Type.TempFire)
		{
			if (!MainManager.IsPaused() || this.alwaysactive)
			{
				this.timer -= MainManager.framestep;
				if (this.timer <= 0f)
				{
					this.fire = !this.fire;
					if (this.fire)
					{
						MainManager.PlaySoundAt("OvenFire", 1f, this.collidercenter);
					}
					this.timer = this.speed;
					this.fireanim.Play(this.fire ? "Start" : "Stop");
				}
				this.col.center = (this.fire ? this.collidercenter : Hazards.offscreen);
				return;
			}
		}
		else if (this.type == Hazards.Type.Water)
		{
			if ((!MainManager.IsPaused() || this.alwaysactive) && !this.noice)
			{
				for (int i = 0; i < this.iceplatforms.Length; i++)
				{
					if (this.iceplatforms[i].active)
					{
						this.iceplatforms[i].obj.transform.position += this.riverammount * MainManager.framestep;
						this.iceplatforms[i].obj.transform.localScale = Vector3.Lerp(this.iceplatforms[i].obj.transform.localScale, (this.iceplatforms[i].timer > 80f) ? MainManager.ChildScale(new Vector3(1.25f, 0.9f, 1.25f), base.transform, true) : Vector3.zero, MainManager.TieFramerate((this.iceplatforms[i].timer > 80f) ? 0.05f : 0.005f));
						if (this.iceplatforms[i].timer > 0f)
						{
							Hazards.IcePlatforms[] array = this.iceplatforms;
							int num = i;
							array[num].timer = array[num].timer - MainManager.framestep;
						}
						else
						{
							this.BreakIcePlatform(i);
						}
					}
				}
			}
			float y = Mathf.Clamp(base.transform.position.y + this.centerpoint.y, this.minwaterfloat, float.PositiveInfinity);
			if (this.waterfloats.Length != 0)
			{
				for (int j = 0; j < this.waterfloats.Length; j++)
				{
					if (this.waterfloats[j] != null && (this.flagfloats[j] == -1 || MainManager.instance.flags[this.flagfloats[j]]))
					{
						this.waterfloats[j].position = Vector3.Lerp(this.waterfloats[j].position, new Vector3(this.waterfloats[j].position.x, y, this.waterfloats[j].position.z), MainManager.framestep * 0.3f);
					}
				}
				return;
			}
		}
		else if (this.type == Hazards.Type.WalkableSpike && MainManager.player != null)
		{
			this.col.center = (MainManager.player.shield ? Hazards.offscreen : this.collidercenter);
		}
	}

	// Token: 0x06000406 RID: 1030 RVA: 0x000298A3 File Offset: 0x00027AA3
	private void LateUpdate()
	{
		this.hitonce = false;
		if (!MainManager.instance.minipause)
		{
			if (this.respawncooldown > 0f)
			{
				this.respawncooldown -= MainManager.framestep;
				return;
			}
			this.respawntries = 0;
		}
	}

	// Token: 0x06000407 RID: 1031 RVA: 0x000298E0 File Offset: 0x00027AE0
	private void BreakIcePlatform(int id)
	{
		MainManager.PlayParticle("IceShatter", this.iceplatforms[id].obj.transform.position);
		MainManager.PlaySoundAt("IceBreak", 1f, this.iceplatforms[id].obj.transform.position);
		this.iceplatforms[id].obj.transform.position = new Vector3(0f, -9999f, 554f);
		this.iceplatforms[id].active = false;
		this.iceplatforms[id].obj.transform.localScale = Vector3.zero;
	}

	// Token: 0x06000408 RID: 1032 RVA: 0x000299A0 File Offset: 0x00027BA0
	private void CreatePlatform(int id, Vector3 pos)
	{
		this.lastplatfomrs = new int[]
		{
			id,
			this.lastplatfomrs[0]
		};
		this.iceplatforms[id].obj.transform.position = pos;
		this.iceplatforms[id].timer = ((this.freezetime > 0f) ? this.freezetime : ((float)((MainManager.BadgeIsEquipped(59) ? 2 : 1) * 1000)));
		this.iceplatforms[id].obj.transform.localScale = Vector3.zero;
		this.iceplatforms[id].active = true;
	}

	// Token: 0x06000409 RID: 1033 RVA: 0x00029A50 File Offset: 0x00027C50
	private bool ObjectStay(NPCControl obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.entitytype == NPCControl.NPCType.Object)
		{
			NPCControl.ObjectTypes objecttype = obj.objecttype;
			if (objecttype <= NPCControl.ObjectTypes.JumpSpring)
			{
				if (objecttype <= NPCControl.ObjectTypes.Item)
				{
					if (objecttype != NPCControl.ObjectTypes.BeetleGrass && objecttype != NPCControl.ObjectTypes.Item)
					{
						return false;
					}
				}
				else if (objecttype != NPCControl.ObjectTypes.Beemerang && objecttype != NPCControl.ObjectTypes.JumpSpring)
				{
					return false;
				}
			}
			else if (objecttype <= NPCControl.ObjectTypes.PathPlatform)
			{
				if (objecttype != NPCControl.ObjectTypes.Switch && objecttype != NPCControl.ObjectTypes.PathPlatform)
				{
					return false;
				}
			}
			else if (objecttype != NPCControl.ObjectTypes.Geizer && objecttype != NPCControl.ObjectTypes.RollingRock)
			{
				return false;
			}
			return true;
		}
		if (obj.entity.fixedentity)
		{
			return true;
		}
		if (obj.entity.ignorewater)
		{
			return true;
		}
		if (obj.entitytype == NPCControl.NPCType.Enemy)
		{
			obj.entity.emoticoncooldown = 0f;
			if (obj.entity.killonfall)
			{
				obj.entity.iskill = true;
				obj.entity.dead = true;
				return false;
			}
			if (obj.entity.height > 0.05f || obj.entity.minheight > 0.05f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600040A RID: 1034 RVA: 0x00029B40 File Offset: 0x00027D40
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DelAftBtl") && this.type == Hazards.Type.Water)
		{
			if (!MainManager.instance.pause)
			{
				Object.Destroy(Object.Instantiate<GameObject>(this.tobj, other.transform.position, Quaternion.identity), 0.75f);
			}
			other.transform.position = Hazards.offscreen;
			return;
		}
		if (other.CompareTag("Icefall") && this.type == Hazards.Type.Water && !this.hitonce)
		{
			this.hitonce = true;
			if (!this.noice)
			{
				MainManager.PlayParticle("mothicenormal", null, other.transform.position);
				MainManager.PlaySound("Freeze", -1, 1f, 0.75f);
				for (int i = 0; i < this.iceplatforms.Length; i++)
				{
					if (!this.iceplatforms[i].active)
					{
						this.CreatePlatform(i, other.transform.position);
						break;
					}
					if (i == this.iceplatforms.Length - 1)
					{
						for (int j = 0; j < this.iceplatforms.Length; j++)
						{
							if (j != this.lastplatfomrs[0] && j != this.lastplatfomrs[1])
							{
								this.BreakIcePlatform(j);
								this.CreatePlatform(j, other.transform.position);
								break;
							}
						}
					}
				}
			}
			else
			{
				MainManager.PlayParticle("IceShatter", other.transform.position);
				MainManager.PlaySoundAt("IceBreak", 1f, other.transform.position);
			}
			Object.Destroy(other.gameObject);
			return;
		}
		if (!MainManager.instance.minipause || this.IsThisPlayer(other.transform))
		{
			if (this.playeronly && !this.IsThisPlayer(other.transform) && !other.CompareTag("PFollower"))
			{
				return;
			}
			EntityControl component = other.GetComponent<EntityControl>();
			if (component != null && !this.ObjectStay(component.npcdata) && (MainManager.player == null || MainManager.player.beemerang == null || component.transform != MainManager.player.beemerang.transform))
			{
				if (this.type == Hazards.Type.Water || this.type == Hazards.Type.Honey)
				{
					Object.Destroy(Object.Instantiate<GameObject>(this.tobj, component.transform.position, Quaternion.identity), 0.75f);
				}
				if (MainManager.player != null && other.transform == MainManager.player.transform)
				{
					component.StopForceMove(-1, false);
					switch (this.type)
					{
					case Hazards.Type.Spikes:
					case Hazards.Type.WalkableSpike:
					case Hazards.Type.TempFire:
						base.StartCoroutine(this.HazardAction(2));
						return;
					case Hazards.Type.SandFunnel:
						base.StartCoroutine(this.HazardAction(0));
						return;
					case Hazards.Type.Water:
					case Hazards.Type.Honey:
						base.StartCoroutine(this.HazardAction(1));
						return;
					case Hazards.Type.Hole:
						base.StartCoroutine(this.HazardAction(3));
						return;
					default:
						return;
					}
				}
				else if (component.following != null)
				{
					if (this.type == Hazards.Type.Spikes || this.type == Hazards.Type.WalkableSpike || this.type == Hazards.Type.Water || this.type == Hazards.Type.Honey || this.type == Hazards.Type.Hole)
					{
						base.StartCoroutine(this.ReturnEntity(component, 2f));
						return;
					}
				}
				else if (component.item)
				{
					if ((component.npcdata == null || component.npcdata.beerang == null) && (component.animid > 0 || (component.animstate != 6 && component.animstate != 7)))
					{
						base.StartCoroutine(this.ReturnEntity(component, 2f));
						return;
					}
				}
				else if (this.type == Hazards.Type.Hole || this.type == Hazards.Type.Water || this.type == Hazards.Type.Spikes)
				{
					if (component.npcdata != null)
					{
						component.npcdata.freezecooldown = 0f;
					}
					base.StartCoroutine(this.ReturnEntity(component, 2f));
				}
			}
		}
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x00029F3D File Offset: 0x0002813D
	private bool IsThisPlayer(Transform o)
	{
		return MainManager.player != null && o == MainManager.player.transform;
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x00029F5E File Offset: 0x0002815E
	private IEnumerator ReturnEntity(EntityControl t, float delay)
	{
		if (!t.CompareTag("PFollower"))
		{
			if (t.transform.parent == MainManager.map.transform && t.npcdata != null)
			{
				if (t.npcdata.tempobject)
				{
					if (this.type != Hazards.Type.WalkableSpike)
					{
						Object.Destroy(t.gameObject);
					}
				}
				else
				{
					float timer = t.npcdata.timer;
					if (timer > -1f)
					{
						t.npcdata.timer = delay * 100f;
					}
					t.rigid.useGravity = false;
					t.transform.position = new Vector3(0f, this.holdspace, 0f);
					yield return new WaitForSeconds(delay);
					t.rigid.useGravity = true;
					t.transform.position = t.startpos.Value + MainManager.instance.globalcamdir.forward.normalized * -0.1f;
					t.rigid.velocity = Vector3.zero;
					if (timer > -1f)
					{
						t.npcdata.timer = timer;
					}
					MainManager.DeathSmoke(t.transform.position);
				}
			}
			else if (this.type != Hazards.Type.WalkableSpike)
			{
				Object.Destroy(t.gameObject);
			}
		}
		else
		{
			t.overrideanim = true;
			t.overrridejump = true;
			t.animstate = 11;
			t.rigid.velocity = Vector3.zero;
			yield return new WaitForSeconds(0.5f);
			while (!t.following.onground && !MainManager.player.flying)
			{
				t.transform.position = new Vector3(0f, 999f, 0f);
				t.rigid.velocity = Vector3.zero;
				yield return null;
			}
			t.transform.position = t.following.transform.position + MainManager.instance.globalcamdir.forward.normalized * 0.1f;
			t.rigid.velocity = Vector3.zero;
			t.LateVelocity(Vector3.zero);
			t.animstate = t.basestate;
			t.overrridejump = false;
			t.overrideanim = false;
		}
		yield break;
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x00029F7B File Offset: 0x0002817B
	private IEnumerator HazardAction(int type)
	{
		MainManager.player.CancelAction();
		bool instant = false;
		if (!MainManager.instance.message && !MainManager.instance.inevent && !MainManager.instance.minipause && !MainManager.instance.pause)
		{
			MainManager.SaveCameraPosition(true);
			MainManager.instance.minipause = true;
			MainManager.instance.camtarget = null;
			MainManager.instance.camtargetpos = null;
			if (type == 0)
			{
				MainManager.PlaySound("SandFall");
				for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
				{
					MainManager.instance.playerdata[i].entity.rigid.isKinematic = true;
					MainManager.instance.playerdata[i].entity.rigid.useGravity = false;
					MainManager.instance.playerdata[i].entity.overrideanim = true;
					MainManager.instance.playerdata[i].entity.ccol.enabled = false;
					MainManager.instance.playerdata[i].entity.overridefollow = true;
					MainManager.instance.playerdata[i].entity.rigid.velocity = Vector3.zero;
					MainManager.instance.playerdata[i].entity.spin = new Vector3(0f, 10f, 0f);
				}
				while (MainManager.GetDistance(MainManager.player.transform.position, this.centerpoint) > 0.5f)
				{
					for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
					{
						MainManager.instance.playerdata[j].entity.anim.Play(MainManager.Animations.Hurt.ToString());
						MainManager.instance.playerdata[j].entity.transform.position = Vector3.Lerp(MainManager.instance.playerdata[j].entity.transform.position, this.centerpoint, MainManager.TieFramerate(this.speed));
						MainManager.instance.playerdata[j].entity.transform.localScale = Vector3.ClampMagnitude(Vector3.one * MainManager.GetDistance(MainManager.player.transform.position, this.centerpoint), 1f);
					}
					yield return null;
				}
			}
			yield return new WaitForSeconds(0.15f);
			if (type == 1 || type == 3)
			{
				MainManager.instance.camtarget = null;
				MainManager.instance.camtargetpos = null;
				MainManager.player.entity.StopForceMove(-1, false);
				MainManager.player.transform.position = new Vector3(MainManager.player.transform.position.x, this.holdspace, MainManager.player.transform.position.z);
				if (type == 3)
				{
					MainManager.PlaySound((MainManager.map.mapid == MainManager.Maps.DesertSandPitArea) ? "SandFall" : "Falling");
					yield return new WaitForSeconds(0.4f);
				}
				yield return new WaitForSeconds(0.6f);
			}
			else if (type == 2 || type == 6)
			{
				Vector3 position = MainManager.player.transform.position;
				MainManager.PlaySound("Damage0", -1, 1.5f, 1f);
				for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
				{
					MainManager.instance.playerdata[k].entity.overrideanim = true;
					MainManager.instance.playerdata[k].entity.overrridejump = true;
					MainManager.instance.playerdata[k].entity.animstate = 11;
					MainManager.instance.playerdata[k].entity.spin = new Vector3(0f, 20f);
					MainManager.instance.playerdata[k].entity.transform.position = position + MainManager.instance.globalcamdir.forward.normalized * ((float)k * 0.15f);
					MainManager.instance.playerdata[k].entity.rigid.useGravity = true;
					MainManager.instance.playerdata[k].entity.rigid.isKinematic = true;
					MainManager.instance.playerdata[k].entity.rigid.velocity = new Vector3(0f, Mathf.Clamp(15f * (1f + (float)k / 2f), 15f, float.PositiveInfinity), 0f);
				}
				yield return new WaitForSeconds(0.75f);
			}
			MainManager.PlayTransition(0, 0, 0.2f, Color.black);
			if (MainManager.instance.transitionobj != null && MainManager.instance.transitionobj.Length != 0)
			{
				while (MainManager.instance.transitionobj[0].GetComponent<SpriteRenderer>().color.a < 0.9f)
				{
					yield return null;
				}
			}
			yield return EventControl.halfsec;
		}
		else
		{
			instant = true;
		}
		for (int l = 0; l < MainManager.instance.playerdata.Length; l++)
		{
			if (this.respawntries > 3 && MainManager.player.movecd >= 10f)
			{
				MainManager.instance.playerdata[l].entity.transform.position = MainManager.player.lastloadzone + MainManager.MainCamera.transform.forward * 0.1f * (float)l;
				this.respawntries = 0;
			}
			else
			{
				MainManager.instance.playerdata[l].entity.transform.position = MainManager.player.lastpos + MainManager.MainCamera.transform.forward * 0.1f * (float)l;
			}
			MainManager.instance.playerdata[l].entity.rigid.isKinematic = false;
			MainManager.instance.playerdata[l].entity.overrideanim = false;
			MainManager.instance.playerdata[l].entity.overrridejump = false;
			MainManager.instance.playerdata[l].entity.overridefollow = false;
			MainManager.instance.playerdata[l].entity.ccol.enabled = true;
			MainManager.instance.playerdata[l].entity.rigid.useGravity = true;
			MainManager.instance.playerdata[l].entity.transform.localScale = Vector3.one;
			MainManager.instance.playerdata[l].entity.spin = Vector3.zero;
			MainManager.instance.playerdata[l].entity.transform.parent = null;
			MainManager.instance.playerdata[l].entity.transform.eulerAngles = Vector3.zero;
		}
		if (this.respawnentities != null && this.respawnentities.Length != 0)
		{
			for (int m = 0; m < this.respawnentities.Length; m++)
			{
				EntityControl entity = MainManager.GetEntity(this.respawnentities[m]);
				if (entity != null)
				{
					entity.transform.position = entity.startpos.Value;
				}
			}
		}
		Hornable[] array = Object.FindObjectsOfType<Hornable>();
		if (array != null && array.Length != 0)
		{
			for (int n = 0; n < array.Length; n++)
			{
				if (array[n].type == Hornable.Type.IceCube && MainManager.GetDistance(MainManager.player.transform.position, array[n].transform.position) < 4f)
				{
					array[n].parent.ShatterDroppletIce();
				}
			}
		}
		for (int num = 0; num < MainManager.map.entities.Length; num++)
		{
			if (MainManager.map.entities[num] != null && MainManager.map.entities[num].gameObject.activeInHierarchy && MainManager.map.entities[num].npcdata != null && MainManager.map.entities[num].npcdata.entitytype == NPCControl.NPCType.Enemy && MainManager.map.entities[num].npcdata.freezecooldown > 0f && MainManager.GetDistance(MainManager.player.transform.position, MainManager.map.entities[num].transform.position) < 4f)
			{
				MainManager.map.entities[num].npcdata.freezecooldown = 0f;
			}
		}
		if (!instant)
		{
			yield return new WaitForSeconds(0.075f);
			float temp = MainManager.instance.camspeed;
			MainManager.SaveCameraPosition(false);
			MainManager.instance.camspeed = 1f;
			MainManager.MainCamera.transform.parent.position = MainManager.player.transform.position;
			this.ResetPlatformCamera();
			yield return null;
			MainManager.instance.camspeed = temp;
			yield return new WaitForSeconds(0.2f);
			MainManager.PlayTransition(1, 0, 0.2f, Color.black);
			yield return new WaitForSeconds(0.3f);
			MainManager.instance.minipause = false;
		}
		else
		{
			this.ResetPlatformCamera();
		}
		this.respawncooldown = 60f;
		this.respawntries++;
		MainManager.player.npc = new List<NPCControl>();
		MainManager.player.entity.emoticoncooldown = 0f;
		yield break;
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x00029F94 File Offset: 0x00028194
	private void ResetPlatformCamera()
	{
		ScrewPlatform[] array = Object.FindObjectsOfType<ScrewPlatform>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].camchange = false;
		}
		if (ScrewPlatform.camischanging)
		{
			MainManager.ResetCamera();
		}
		ScrewPlatform.camischanging = false;
	}

	// Token: 0x04000392 RID: 914
	public Hazards.Type type;

	// Token: 0x04000393 RID: 915
	public Vector3 centerpoint;

	// Token: 0x04000394 RID: 916
	public Vector3 riverammount;

	// Token: 0x04000395 RID: 917
	public float speed = 0.1f;

	// Token: 0x04000396 RID: 918
	public float holdspace = -5f;

	// Token: 0x04000397 RID: 919
	public float minwaterfloat;

	// Token: 0x04000398 RID: 920
	public float freezetime;

	// Token: 0x04000399 RID: 921
	public bool noice;

	// Token: 0x0400039A RID: 922
	public bool keeplayer;

	// Token: 0x0400039B RID: 923
	public bool alwaysactive;

	// Token: 0x0400039C RID: 924
	public bool iceparentedtomap;

	// Token: 0x0400039D RID: 925
	public bool playeronly;

	// Token: 0x0400039E RID: 926
	private float timer;

	// Token: 0x0400039F RID: 927
	private bool hitonce;

	// Token: 0x040003A0 RID: 928
	private bool fire;

	// Token: 0x040003A1 RID: 929
	public int platformlimit = 3;

	// Token: 0x040003A2 RID: 930
	private int respawntries;

	// Token: 0x040003A3 RID: 931
	private float respawncooldown;

	// Token: 0x040003A4 RID: 932
	private int[] lastplatfomrs;

	// Token: 0x040003A5 RID: 933
	public Transform[] waterfloats;

	// Token: 0x040003A6 RID: 934
	public int[] flagfloats;

	// Token: 0x040003A7 RID: 935
	public int[] respawnentities;

	// Token: 0x040003A8 RID: 936
	private const float distancecheck = 0.5f;

	// Token: 0x040003A9 RID: 937
	private const float spikejump = 15f;

	// Token: 0x040003AA RID: 938
	private static readonly Vector3 offscreen = new Vector3(2222f, -99999f, 88282f);

	// Token: 0x040003AB RID: 939
	private StaticModelAnim sanim;

	// Token: 0x040003AC RID: 940
	private GameObject tobj;

	// Token: 0x040003AD RID: 941
	private Animator fireanim;

	// Token: 0x040003AE RID: 942
	private AudioClip taudio;

	// Token: 0x040003AF RID: 943
	private Vector3 collidercenter;

	// Token: 0x040003B0 RID: 944
	private BoxCollider col;

	// Token: 0x040003B1 RID: 945
	private Hazards.IcePlatforms[] iceplatforms;

	// Token: 0x020001F8 RID: 504
	public enum Type
	{
		// Token: 0x0400167A RID: 5754
		Spikes,
		// Token: 0x0400167B RID: 5755
		SandFunnel,
		// Token: 0x0400167C RID: 5756
		Water,
		// Token: 0x0400167D RID: 5757
		Hole,
		// Token: 0x0400167E RID: 5758
		WalkableSpike,
		// Token: 0x0400167F RID: 5759
		Honey,
		// Token: 0x04001680 RID: 5760
		TempFire
	}

	// Token: 0x020001F9 RID: 505
	private struct IcePlatforms
	{
		// Token: 0x04001681 RID: 5761
		public GameObject obj;

		// Token: 0x04001682 RID: 5762
		public float timer;

		// Token: 0x04001683 RID: 5763
		public bool active;
	}
}
