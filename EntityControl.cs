using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000017 RID: 23
public class EntityControl : MonoBehaviour
{
	// Token: 0x060001C1 RID: 449 RVA: 0x00015140 File Offset: 0x00013340
	private void Start()
	{
		this.transform = base.GetComponent<Transform>();
		if (this.spritetransform == null && this.sprite != null)
		{
			this.spritetransform = this.sprite.transform;
		}
		this.startscale = Vector3.one;
		if (EntityControl.icecubeprefab == null)
		{
			EntityControl.icecubeprefab = Resources.Load<GameObject>("Prefabs/Objects/icecube");
		}
		EntityControl.icecubeprefab.gameObject.layer = 13;
		if (base.gameObject.GetComponent<Rigidbody>() == null)
		{
			this.rigid = base.gameObject.AddComponent<Rigidbody>();
			this.rigid.constraints = RigidbodyConstraints.FreezeRotation;
		}
		if (base.gameObject.GetComponent<Animator>() == null)
		{
			this.anim = base.gameObject.AddComponent<Animator>();
		}
		this.digpart = new GameObject[2];
		this.rotater = base.gameObject.transform.GetChild(0);
		if (base.name.Contains("Holo"))
		{
			this.hologram = true;
		}
		if (base.name.Contains("TIME"))
		{
			this.extratimer = true;
		}
		if (base.name.Contains("COT"))
		{
			this.hologram = true;
			this.cotunknown = true;
			this.spritebasecolor = EntityControl.cotcolor;
			base.Invoke("RefreshCOT", 0.1f);
		}
		if (this.npcdata != null)
		{
			if (base.name.Contains("ShwKEY"))
			{
				this.showitem = true;
			}
			if (base.name.Contains("ICE"))
			{
				this.npcdata.extrafreeze = true;
			}
		}
		this.UpdateSpriteMat();
		this.sprite.gameObject.layer = 14;
		this.sprite.receiveShadows = true;
		this.ccol = base.GetComponent<CapsuleCollider>();
		this.ccol.material = MainManager.defaultpmat;
		this.initialcenter = this.ccol.center;
		this.initialcolliderdata = new Vector2(this.ccol.height, this.ccol.radius);
		if (MainManager.map != null)
		{
			if (MainManager.CurrentMap() == MainManager.Maps.MetalLake)
			{
				this.overridemovesmoke = true;
			}
			if (MainManager.map.icemap)
			{
				this.inice = true;
			}
		}
		if (this.startpos == null)
		{
			this.startpos = new Vector3?(this.transform.position);
		}
		this.spawnpoint = this.transform.position;
		if (!this.noemoticon)
		{
			this.emoticon = new GameObject("Emoticon").AddComponent<Animator>();
			this.emoticon.transform.parent = this.rotater;
			this.emoticon.transform.localPosition = this.emoticonoffset;
			this.emoticon.gameObject.layer = 15;
			this.emoticonsprite = this.emoticon.gameObject.AddComponent<SpriteRenderer>();
			this.emoticonsprite.material = MainManager.spritedefaultunity;
		}
		if (this.sound == null)
		{
			this.sound = base.gameObject.AddComponent<AudioSource>();
			this.sound.playOnAwake = false;
		}
		if (this.startvelocity != null)
		{
			this.rigid.velocity = this.startvelocity.Value;
		}
		this.CheckSpecialID();
		this.SetDialogueBleep();
		if (this.hasshadow && this.shadow == null)
		{
			this.CreateShadow();
		}
		if (this.item)
		{
			this.itemstate = this.animstate;
		}
		base.name = base.name.Replace("\n", "").Replace("\r", "");
		if (base.name.Contains("Fixed"))
		{
			this.fixedentity = true;
			base.Invoke("SetFixed", 0.1f);
		}
		if (base.name.Contains("FxdCol") || this.originalid == 23)
		{
			this.fixedentity = true;
			base.Invoke("SetFixedCollider", 0.1f);
		}
		if (base.name.Contains("ALW"))
		{
			this.alwaysactive = true;
		}
		if (base.name.Contains("ALF"))
		{
			this.alwaysflip = true;
		}
		if (base.name.Contains("PAU"))
		{
			this.activeonpause = true;
		}
		if (base.name.Contains("HIDE"))
		{
			this.hideinside = true;
		}
		if (base.name.Contains("ROT"))
		{
			this.lockrotater = true;
		}
		if (base.name.Contains("ShwEm"))
		{
			this.alwaysemoticon = true;
		}
		if (base.name.Contains("COG"))
		{
			RaycastHit raycastHit;
			Physics.Raycast(this.transform.position, Vector3.down, out raycastHit, float.PositiveInfinity, 8448);
			this.startpos = new Vector3?(raycastHit.point);
		}
		if (base.name.Contains("NGS"))
		{
			this.onground = false;
		}
		if (this.npcdata != null)
		{
			if (base.name.Contains("ITHD"))
			{
				this.npcdata.SetHitInteract(NPCControl.HitInteract.HornDash);
			}
			else if (base.name.Contains("ITAH"))
			{
				this.npcdata.SetHitInteract(NPCControl.HitInteract.AnyHorn);
			}
		}
		if (this.battle && this.isplayer)
		{
			this.CreateShield();
		}
		this.lastpos = this.startpos.Value;
		this.initialheight = this.height;
		if (!this.overrideminheight && this.height < this.minheight)
		{
			this.height = this.minheight;
		}
		if (this.sprite != null)
		{
			this.FlipAngle(true);
		}
		this.playerentity = (base.CompareTag("PFollower") || base.CompareTag("Player"));
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x0001571F File Offset: 0x0001391F
	public void UpdateSpriteMat()
	{
		this.sprite.material = (this.hologram ? MainManager.holosprite : MainManager.spritemat);
	}

	// Token: 0x060001C3 RID: 451 RVA: 0x00015740 File Offset: 0x00013940
	public void CreateShield()
	{
		if (this.bubbleshield == null)
		{
			this.bubbleshield = (Object.Instantiate(Resources.Load("Prefabs/Objects/BubbleShield")) as GameObject).AddComponent<DialogueAnim>();
			this.bubbleshield.transform.parent = this.rotater.transform;
			this.bubbleshield.shrink = true;
			this.bubbleshield.shrinkspeed = 0.075f;
			this.bubbleshield.targetscale = new Vector3(1.8f, 3.15f, 1f);
			this.bubbleshield.transform.localScale = Vector3.zero;
			this.bubbleshield.transform.localPosition = new Vector3(0f, 1.25f);
			Renderer component = this.bubbleshield.GetComponent<Renderer>();
			component.material.color = new Color(1f, 1f, 1f, 0.55f);
			component.material.renderQueue = 2505;
		}
	}

	// Token: 0x060001C4 RID: 452 RVA: 0x00015845 File Offset: 0x00013A45
	private void SetFixedCollider()
	{
		this.LockRigid(true);
		this.rigid.constraints = RigidbodyConstraints.FreezeAll;
		this.transform.position = this.startpos.Value;
	}

	// Token: 0x060001C5 RID: 453 RVA: 0x00015871 File Offset: 0x00013A71
	private void SetFixed()
	{
		this.ccol.enabled = false;
		this.LockRigid(true);
		this.rigid.constraints = RigidbodyConstraints.FreezeAll;
		this.transform.position = this.startpos.Value;
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x000158A9 File Offset: 0x00013AA9
	public void SetLate(Transform obj, Vector3 pos)
	{
		this.latetrans = obj;
		this.latepos = pos;
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x000158B9 File Offset: 0x00013AB9
	public void SetPosition(Vector3 pos)
	{
		this.startpos = new Vector3?(pos);
		this.lastpos = pos;
		this.transform.position = pos;
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x000158DA File Offset: 0x00013ADA
	public void StopLate()
	{
		this.latetrans = null;
	}

	// Token: 0x060001C9 RID: 457 RVA: 0x000158E3 File Offset: 0x00013AE3
	public void FacePlayer()
	{
		if (MainManager.player != null)
		{
			this.FaceTowards(MainManager.player.transform.position);
		}
	}

	// Token: 0x060001CA RID: 458 RVA: 0x00015908 File Offset: 0x00013B08
	public void CreateLine(Vector3 start, Vector3 end, float width, Color color, Transform parent)
	{
		this.line = new GameObject("line").AddComponent<LineRenderer>();
		this.line.startColor = color;
		this.line.material = MainManager.spritemat;
		this.line.material.color = color;
		this.line.startWidth = 1f;
		this.line.endWidth = 1f;
		this.line.widthMultiplier = width;
		if (parent != null)
		{
			this.line.transform.parent = parent;
		}
		else
		{
			parent = this.spritetransform;
		}
		this.line.transform.localPosition = start;
		this.line.SetPositions(new Vector3[]
		{
			start,
			end
		});
	}

	// Token: 0x060001CB RID: 459 RVA: 0x000159DD File Offset: 0x00013BDD
	public void InstantDig()
	{
		this.digging = true;
		this.instdig = true;
		this.digtime = 31f;
	}

	// Token: 0x060001CC RID: 460 RVA: 0x000159F8 File Offset: 0x00013BF8
	public void SetDialogueBleep()
	{
		if (this.animid + 1 <= MainManager.endata.Length - 1)
		{
			if (this.originalid > -1)
			{
				this.dialoguebleepid = MainManager.endata[this.animid + 1].bleepid;
				this.bleeppitch = MainManager.endata[this.animid + 1].bleeppitch;
				return;
			}
			this.dialoguebleepid = 0;
			this.bleeppitch = 1f;
		}
	}

	// Token: 0x060001CD RID: 461 RVA: 0x00015A6E File Offset: 0x00013C6E
	public void DelayedPosition(Vector3 pos)
	{
		base.StartCoroutine(this.SetLatePos(pos, -1f));
	}

	// Token: 0x060001CE RID: 462 RVA: 0x00015A83 File Offset: 0x00013C83
	public void DelayedPosition(Vector3 pos, float time)
	{
		base.StartCoroutine(this.SetLatePos(pos, time));
	}

	// Token: 0x060001CF RID: 463 RVA: 0x00015A94 File Offset: 0x00013C94
	private IEnumerator SetLatePos(Vector3 pos, float time)
	{
		this.forcemove = false;
		if (time > 0f)
		{
			yield return new WaitForSeconds(time);
		}
		else
		{
			yield return null;
		}
		this.transform.position = pos;
		yield break;
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x00015AB4 File Offset: 0x00013CB4
	public static void IgnoreColliders(EntityControl a, EntityControl b, bool ignore)
	{
		Collider[] componentsInChildren = a.GetComponentsInChildren<Collider>();
		Collider[] componentsInChildren2 = b.GetComponentsInChildren<Collider>();
		int num = 0;
		while (num < componentsInChildren.Length && num < componentsInChildren2.Length)
		{
			Physics.IgnoreCollision(componentsInChildren[num], componentsInChildren2[num], ignore);
			num++;
		}
	}

	// Token: 0x060001D1 RID: 465 RVA: 0x00015AF0 File Offset: 0x00013CF0
	public static void IgnoreColliders(EntityControl a, Collider b, bool ignore)
	{
		Collider[] componentsInChildren = a.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Physics.IgnoreCollision(componentsInChildren[i], b, ignore);
		}
	}

	// Token: 0x060001D2 RID: 466 RVA: 0x00015B1C File Offset: 0x00013D1C
	public void DetectDirection(Vector3 targetp)
	{
		if (this.detect == null)
		{
			this.CreateDetector();
		}
		if (this.detect != null)
		{
			this.detect.transform.LookAt(targetp);
			this.detect.transform.localEulerAngles = new Vector3(0f, this.detect.transform.localEulerAngles.y, 0f);
		}
	}

	// Token: 0x060001D3 RID: 467 RVA: 0x00015B90 File Offset: 0x00013D90
	public void UpdateAnimSpecific()
	{
		if (this.animspecific != null && this.animspecific.Length != 0)
		{
			for (int i = 0; i < this.animspecific.Length; i++)
			{
				Object.Destroy(this.animspecific[i]);
			}
		}
		if (!this.item)
		{
			MainManager.AnimIDs animIDs = this.animid + MainManager.AnimIDs.Bee;
			if (animIDs <= MainManager.AnimIDs.MotherChomper)
			{
				if (animIDs <= MainManager.AnimIDs.Seedling)
				{
					if (animIDs <= MainManager.AnimIDs.Moth)
					{
						if (animIDs != MainManager.AnimIDs.Bee)
						{
							if (animIDs != MainManager.AnimIDs.Moth)
							{
								return;
							}
						}
						else
						{
							if (this.animstate == 13)
							{
								this.animspecific = new GameObject[]
								{
									Object.Instantiate(Resources.Load("Prefabs/AnimSpecific/BeeBIdle")) as GameObject
								};
								this.animspecific[0].transform.parent = this.spritetransform;
								this.animspecific[0].transform.localPosition = new Vector3(0.53f, 1.38f);
								this.animspecific[0].transform.localEulerAngles = new Vector3(90f, 0f);
								this.animspecific[0].transform.localScale = Vector3.one;
								return;
							}
							return;
						}
					}
					else
					{
						if (animIDs == MainManager.AnimIDs.Armorpillar)
						{
							int num = this.animstate;
							return;
						}
						if (animIDs == MainManager.AnimIDs.Spuder)
						{
							goto IL_8CF;
						}
						if (animIDs != MainManager.AnimIDs.Seedling)
						{
							return;
						}
						if (this.animstate == 11 && this.height > 0.1f && this.spritetransform.childCount > 0)
						{
							MainManager.instance.StartCoroutine(MainManager.LerpObject(this.spritetransform.GetChild(0), this.transform.position + Vector3.up * 10f, 0.01f, true));
							this.spritetransform.GetChild(0).parent = null;
							return;
						}
						return;
					}
				}
				else if (animIDs <= MainManager.AnimIDs.Venus)
				{
					if (animIDs == MainManager.AnimIDs.AngryPlant)
					{
						this.spinextra[0] = ((this.height > 0.1f) ? new Vector3(0f, -20f) : Vector3.zero);
						this.extras[0].transform.localScale = ((this.height > 0.1f) ? new Vector3(1.15f, 0.2f, 1.15f) : new Vector3(1f, 0.2f, 1f));
						return;
					}
					if (animIDs != MainManager.AnimIDs.Venus)
					{
						return;
					}
					this.extraanims = new Animator[6];
					for (int j = 0; j < this.extraanims.Length; j++)
					{
						this.extraanims[j] = this.model.transform.GetChild(1).GetChild(j).gameObject.GetComponent<Animator>();
						this.extraanims[j].speed = (float)((this.animstate == 100 || this.animstate == 101) ? 0 : 1);
					}
					return;
				}
				else
				{
					if (animIDs == MainManager.AnimIDs.VenusGuardian)
					{
						if (this.extraanims == null || this.extraanims.Length == 0)
						{
							SpriteRenderer[] componentsInChildren = this.model.GetComponentsInChildren<SpriteRenderer>();
							for (int k = 0; k < componentsInChildren.Length; k++)
							{
								componentsInChildren[k].shadowCastingMode = ShadowCastingMode.TwoSided;
							}
							this.extraanims = new Animator[]
							{
								this.model.transform.GetChild(1).GetChild(2).GetComponent<Animator>(),
								this.model.transform.GetChild(1).GetChild(3).GetComponent<Animator>()
							};
							this.extras = new GameObject[]
							{
								this.model.transform.GetChild(1).GetChild(0).gameObject,
								this.model.transform.GetChild(1).GetChild(1).gameObject,
								this.model.transform.GetChild(2).GetChild(0).gameObject,
								this.extraanims[1].transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).gameObject
							};
							for (int l = 0; l < this.extras.Length; l++)
							{
								this.extras[l].SetActive(false);
							}
						}
						this.extras[2].SetActive(this.height < 0.1f);
						this.extras[3].SetActive(this.animstate >= 106 && this.animstate <= 108);
						int num2 = this.animstate;
						if (num2 <= 18)
						{
							if (num2 == 11)
							{
								this.extraanims[0].Play("ArmHurt1");
								this.extraanims[1].Play("ArmHurt2");
								return;
							}
							if (num2 == 18)
							{
								goto IL_158B;
							}
						}
						else
						{
							if (num2 == 21)
							{
								for (int m = 0; m < this.extraanims.Length; m++)
								{
									this.extraanims[m].Play("ArmFly");
								}
								return;
							}
							switch (num2)
							{
							case 100:
								if (MainManager.battle == null || MainManager.battle.enemydata[this.battleid].data[0] == 2)
								{
									for (int n = 0; n < this.extraanims.Length; n++)
									{
										this.extraanims[n].Play("ArmHold");
									}
									return;
								}
								this.extraanims[1].Play("ArmStomp1");
								return;
							case 102:
								if (MainManager.battle != null && MainManager.battle.enemydata[this.battleid].data[0] == 1)
								{
									this.extraanims[1].Play("ArmSweep");
									return;
								}
								if (MainManager.battle == null || MainManager.battle.enemydata[this.battleid].data[0] == 2)
								{
									for (int num3 = 0; num3 < this.extraanims.Length; num3++)
									{
										this.extraanims[num3].Play("ArmHold");
									}
									return;
								}
								this.extraanims[1].Play("ArmStomp2");
								return;
							case 105:
								goto IL_158B;
							case 106:
								this.extraanims[0].Play("ArmHold");
								this.extraanims[1].Play("ArmShoot1");
								return;
							case 108:
								this.extraanims[0].Play("ArmHold");
								this.extraanims[1].Play("ArmShoot3");
								return;
							case 109:
								for (int num4 = 0; num4 < this.extraanims.Length; num4++)
								{
									this.extraanims[num4].Play("ArmUp");
								}
								return;
							}
						}
						for (int num5 = 0; num5 < this.extraanims.Length; num5++)
						{
							if (this.height > 0.1f)
							{
								this.extraanims[num5].Play("ArmFly");
							}
							else
							{
								this.extraanims[num5].Play("ArmIdle");
							}
						}
						return;
						IL_158B:
						for (int num6 = 0; num6 < this.extraanims.Length; num6++)
						{
							this.extraanims[num6].Play("ArmDed");
						}
						return;
					}
					if (animIDs == MainManager.AnimIDs.Tanjerin)
					{
						int num2 = this.animstate;
						if (num2 != 0)
						{
							switch (num2)
							{
							case 5:
							case 9:
								this.walkstate = 101;
								return;
							case 6:
								break;
							case 7:
							case 8:
								return;
							default:
								return;
							}
						}
						this.walkstate = 1;
						return;
					}
					if (animIDs != MainManager.AnimIDs.MotherChomper)
					{
						return;
					}
					if (this.extras == null || this.extras.Length == 0)
					{
						this.extras = new GameObject[]
						{
							this.model.GetChild(1).GetChild(1).gameObject
						};
						return;
					}
					return;
				}
			}
			else if (animIDs <= MainManager.AnimIDs.JumpingSpider)
			{
				if (animIDs <= MainManager.AnimIDs.Scorpion)
				{
					if (animIDs != MainManager.AnimIDs.BeeBot)
					{
						if (animIDs != MainManager.AnimIDs.Scorpion)
						{
							return;
						}
						if (MainManager.instance.areaid == 15)
						{
							this.basestate = 150;
							this.walkstate = 151;
							goto IL_8CF;
						}
						goto IL_8CF;
					}
					else
					{
						if (!(this.model != null))
						{
							return;
						}
						StaticModelAnim[] componentsInChildren2 = this.model.transform.GetChild(0).GetComponentsInChildren<StaticModelAnim>(true);
						if (this.icecube == null)
						{
							for (int num7 = 0; num7 < componentsInChildren2.Length; num7++)
							{
								componentsInChildren2[num7].enabled = true;
							}
							return;
						}
						for (int num8 = 0; num8 < componentsInChildren2.Length; num8++)
						{
							componentsInChildren2[num8].enabled = false;
							componentsInChildren2[num8].transform.localEulerAngles = new Vector3((float)((num8 == 1) ? 10 : -10), 0f, -17f);
						}
						return;
					}
				}
				else if (animIDs != MainManager.AnimIDs.Watcher)
				{
					if (animIDs != MainManager.AnimIDs.Eremi)
					{
						if (animIDs != MainManager.AnimIDs.JumpingSpider)
						{
							return;
						}
						if (!this.battle || !(this.model != null))
						{
							goto IL_8CF;
						}
						SpriteRenderer component = this.model.GetChild(this.model.childCount - 1).GetComponent<SpriteRenderer>();
						if (component != null && MainManager.battle.enemydata.Length >= this.battleid + 1)
						{
							component.enabled = (MainManager.battle.enemydata[this.battleid].holditem > -1);
							goto IL_8CF;
						}
						goto IL_8CF;
					}
					else
					{
						if (this.animstate == 8)
						{
							this.walkstate = 23;
							this.basestate = 8;
							return;
						}
						return;
					}
				}
				else if (this.extras != null && this.extras.Length != 0 && MainManager.battle != null && this.extras[1] != null && MainManager.battle.enemydata[this.battleid].position != BattleControl.BattlePosition.Underground)
				{
					Object.Destroy(this.extras[1].gameObject);
					this.bobspeed = this.startbs;
					this.bobrange = this.startbf;
				}
			}
			else if (animIDs <= MainManager.AnimIDs.Ruffian)
			{
				if (animIDs != MainManager.AnimIDs.MimicSpider)
				{
					switch (animIDs)
					{
					case MainManager.AnimIDs.Strider:
						if (this.extraanims == null && this.model != null)
						{
							this.extraanims = new Animator[4];
							this.extras = new GameObject[]
							{
								this.model.GetChild(1).gameObject
							};
							for (int num9 = 0; num9 < this.extraanims.Length; num9++)
							{
								this.extraanims[num9] = this.model.GetChild(1).GetChild(num9).GetComponent<Animator>();
							}
						}
						if (this.extras != null && this.extras.Length != 0)
						{
							this.extras[0].transform.localScale = new Vector3(1f, 1f, (this.animstate == 1 || this.animstate == 23) ? 0.5f : 1f);
							return;
						}
						return;
					case MainManager.AnimIDs.DivingSpider:
						goto IL_8CF;
					case MainManager.AnimIDs.Cenn:
					case MainManager.AnimIDs.Pisci:
						return;
					case MainManager.AnimIDs.Ruffian:
					{
						if (this.extras != null && this.extras.Length != 0)
						{
							return;
						}
						this.extras = new GameObject[4];
						Sprite[] array = Resources.LoadAll<Sprite>("Sprites/Entities/ruffian");
						for (int num10 = 0; num10 < 3; num10++)
						{
							this.extras[num10] = MainManager.NewSpriteObject(this.transform.position, this.transform.parent, array[12]).gameObject;
						}
						this.extras[3] = MainManager.NewSpriteObject(this.transform.position, this.transform.parent, array[13]).gameObject;
						MidPos midPos = this.sprite.gameObject.AddComponent<MidPos>();
						midPos.links = new Transform[]
						{
							this.spritetransform,
							this.extras[0].transform,
							this.extras[1].transform,
							this.extras[2].transform,
							this.extras[3].transform
						};
						midPos.getstartandendfromlink = true;
						this.extras[3].AddComponent<FollowerLite>().SetUp(1.25f, this.transform, 0.05f);
						this.extras[3].AddComponent<ShadowLite>().SetUp(0.5f, 1f);
						this.extras[3].transform.position = this.spritetransform.position + new Vector3((float)(this.flip ? -1 : 1), 0f, -0.1f);
						if (this.hologram)
						{
							for (int num11 = 0; num11 < this.extras.Length; num11++)
							{
								SpriteRenderer component2 = this.extras[num11].GetComponent<SpriteRenderer>();
								component2.material = MainManager.holosprite;
								component2.color = this.sprite.color;
							}
							return;
						}
						return;
					}
					default:
						return;
					}
				}
				else
				{
					if (this.anim != null)
					{
						this.anim.SetBool("OutOfBattle", !this.battle);
					}
					if (this.animstate <= 1)
					{
						this.shadowsize = 1.75f;
						return;
					}
					this.shadowsize = 3f;
					return;
				}
			}
			else
			{
				if (animIDs == MainManager.AnimIDs.PeacockSpider)
				{
					goto IL_8CF;
				}
				if (animIDs == MainManager.AnimIDs.DeadLanderC)
				{
					if (this.extraanims == null || this.extraanims.Length == 0)
					{
						this.extraanims = new Animator[4];
					}
					int num12 = 0;
					while (num12 < this.extraanims.Length)
					{
						if (this.extraanims[num12] == null)
						{
							this.extraanims[num12] = this.model.GetChild(num12 + 1).GetComponentInChildren<Animator>();
						}
						int num2 = this.animstate;
						if (num2 <= 11)
						{
							if (num2 == 1)
							{
								goto IL_83D;
							}
							if (num2 != 11)
							{
								goto IL_828;
							}
							this.extraanims[num12].Play("Hurt");
						}
						else
						{
							if (num2 == 23)
							{
								goto IL_83D;
							}
							if (num2 - 100 > 1)
							{
								goto IL_828;
							}
							this.extraanims[num12].Play("Spread");
						}
						IL_895:
						num12++;
						continue;
						IL_828:
						this.extraanims[num12].Play("Idle");
						goto IL_895;
						IL_83D:
						if (num12 % 2 == 0)
						{
							this.extraanims[num12].Play("Walk1");
							goto IL_895;
						}
						this.extraanims[num12].Play("Walk2");
						goto IL_895;
					}
					return;
				}
				if (animIDs == MainManager.AnimIDs.EverlastingKing && this.animstate == 115)
				{
					this.shadowsize = 5f;
					return;
				}
				return;
			}
			if (this.animstate == 13)
			{
				this.animspecific = new GameObject[]
				{
					Object.Instantiate(Resources.Load("Prefabs/AnimSpecific/mothbattlesphere")) as GameObject
				};
				this.animspecific[0].transform.parent = this.spritetransform;
				if (this.animid + 1 == 3)
				{
					this.animspecific[0].transform.localPosition = new Vector3(0.75f, 1.55f);
				}
				else
				{
					this.animspecific[0].transform.localPosition = new Vector3(1.1f, 1.85f, -0.1f);
				}
				this.animspecific[0].transform.localEulerAngles = new Vector3(90f, 0f);
				this.animspecific[0].transform.localScale = Vector3.one;
				return;
			}
			if (this.animstate == 19)
			{
				this.animspecific = new GameObject[]
				{
					Object.Instantiate(Resources.Load("Prefabs/AnimSpecific/mothbattlesphere")) as GameObject
				};
				this.animspecific[0].transform.parent = this.spritetransform;
				this.animspecific[0].transform.localPosition = new Vector3(0f, 1.5f);
				this.animspecific[0].AddComponent<SpinAround>().StartUp(this.spritetransform, 2f, 0.75f, 3f, 0.5f, 1.5f);
				return;
			}
			return;
			IL_8CF:
			bool flag = this.animid + 1 != 202;
			bool flag2 = this.animid + 1 == 344;
			this.overrridejump = false;
			if (this.extraanims == null || this.extraanims.Length == 0)
			{
				this.extraanims = new Animator[flag ? 8 : 6];
				for (int num13 = 0; num13 < this.extraanims.Length; num13++)
				{
					if (num13 < this.extraanims.Length / 2)
					{
						this.extraanims[num13] = this.model.GetChild(1).GetChild(num13).gameObject.GetComponent<Animator>();
					}
					else
					{
						this.extraanims[num13] = this.model.GetChild(2).GetChild(num13 - this.extraanims.Length / 2).gameObject.GetComponent<Animator>();
					}
					if (this.animid + 1 == 234)
					{
						Renderer component3 = this.model.GetChild(3 + num13).GetComponent<Renderer>();
						if (component3 != null)
						{
							if (this.hologram)
							{
								Texture mainTexture = component3.material.mainTexture;
								component3.material = MainManager.holosprite;
								component3.material.mainTexture = mainTexture;
							}
							if (this.cotunknown)
							{
								component3.material.color = EntityControl.cot3d;
							}
						}
					}
				}
			}
			for (int num14 = 0; num14 < this.extraanims.Length; num14++)
			{
				animIDs = this.animid + MainManager.AnimIDs.Bee;
				if (animIDs != MainManager.AnimIDs.JumpingSpider)
				{
					if (animIDs != MainManager.AnimIDs.DivingSpider)
					{
						int num2 = this.animstate;
						if (num2 <= 14)
						{
							switch (num2)
							{
							case -1:
								break;
							case 0:
								goto IL_CD1;
							case 1:
								goto IL_D69;
							case 2:
							case 3:
							case 4:
								goto IL_F64;
							case 5:
								if (flag2 && (num14 == 0 || num14 == 4))
								{
									this.extraanims[num14].Play("PCExtended");
									goto IL_F64;
								}
								goto IL_CD1;
							default:
								if (num2 != 11)
								{
									if (num2 != 14)
									{
										goto IL_F64;
									}
								}
								else
								{
									if (num14 % 2 == 0)
									{
										this.extraanims[num14].Play("Hurt0");
										goto IL_F64;
									}
									this.extraanims[num14].Play("Hurt1");
									goto IL_F64;
								}
								break;
							}
						}
						else
						{
							if (num2 == 18)
							{
								this.extraanims[num14].Play("Extended");
								goto IL_F64;
							}
							switch (num2)
							{
							case 100:
								goto IL_DFA;
							case 101:
								if (flag2 && (num14 == 0 || num14 == 4))
								{
									if (num14 == 0)
									{
										this.extraanims[num14].Play("PCDanceB");
										goto IL_F64;
									}
									this.extraanims[num14].Play("PCDanceB2");
									goto IL_F64;
								}
								break;
							case 102:
								goto IL_CD1;
							case 103:
								if (flag2 && (num14 == 0 || num14 == 4))
								{
									this.extraanims[num14].Play("PCDanceB");
									goto IL_F64;
								}
								break;
							case 104:
								if (flag2 && (num14 == 0 || num14 == 4))
								{
									if (num14 == 0)
									{
										this.extraanims[num14].Play("IdlePC");
										goto IL_F64;
									}
									if (num14 == 4)
									{
										this.extraanims[num14].Play("PCAttack");
										goto IL_F64;
									}
									goto IL_F64;
								}
								break;
							case 105:
							case 106:
							case 107:
								break;
							case 108:
							case 109:
							case 110:
								if (flag)
								{
									this.extraanims[num14].Play("Hang");
									goto IL_F64;
								}
								goto IL_F64;
							case 111:
								if (flag)
								{
									this.extraanims[num14].Play("Hang2");
									goto IL_F64;
								}
								goto IL_F64;
							case 112:
								if (!flag)
								{
									goto IL_F64;
								}
								if (num14 == 0 || num14 == 4)
								{
									this.extraanims[num14].Play("HoldWeb");
									goto IL_F64;
								}
								goto IL_DFA;
							default:
								switch (num2)
								{
								case 150:
									goto IL_C7C;
								case 151:
									goto IL_D69;
								case 152:
									if (!flag && !flag2)
									{
										this.extraanims[num14].Play("Extended");
										goto IL_F64;
									}
									goto IL_D69;
								default:
									goto IL_F64;
								}
								break;
							}
							IL_EF3:
							if (flag)
							{
								this.extraanims[num14].Play("Stand");
								goto IL_F64;
							}
							goto IL_C7C;
							IL_DFA:
							if (!flag2 || (num14 != 0 && num14 != 4))
							{
								goto IL_EF3;
							}
							if (num14 == 0)
							{
								this.extraanims[num14].Play("PCDanceA");
								goto IL_F64;
							}
							this.extraanims[num14].Play("PCDanceA2");
							goto IL_F64;
						}
						IL_C7C:
						this.extraanims[num14].Play("Normal");
						goto IL_F64;
						IL_CD1:
						if (flag2 && num14 == 0 && this.animstate == 102)
						{
							this.extraanims[num14].Play("PCAttack");
							goto IL_F64;
						}
						if (this.animstate != 0 && !flag)
						{
							goto IL_F64;
						}
						if (!flag2 || (num14 != 0 && num14 != 4))
						{
							this.extraanims[num14].Play("Normal");
							goto IL_F64;
						}
						this.extraanims[num14].Play("IdlePC");
						goto IL_F64;
						IL_D69:
						if (!flag2 || (num14 != 0 && num14 != 4))
						{
							if (num14 % 2 == 0)
							{
								this.extraanims[num14].Play("Walk0");
							}
							else
							{
								this.extraanims[num14].Play("Walk1");
							}
						}
						else
						{
							this.extraanims[num14].Play("IdlePC");
						}
					}
					else
					{
						int num2 = this.animstate;
						if (num2 <= 11)
						{
							if (num2 != 1)
							{
								if (num2 != 11)
								{
									goto IL_BC3;
								}
								if (num14 % 2 == 0)
								{
									this.extraanims[num14].Play("Hurt0");
									goto IL_F64;
								}
								this.extraanims[num14].Play("Hurt1");
								goto IL_F64;
							}
						}
						else
						{
							if (num2 == 14)
							{
								goto IL_BC3;
							}
							if (num2 != 23)
							{
								if (num2 != 101)
								{
									goto IL_BC3;
								}
								if (flag)
								{
									this.extraanims[num14].Play("Stand");
									goto IL_F64;
								}
								goto IL_F64;
							}
						}
						if (num14 % 2 == 0)
						{
							this.extraanims[num14].Play("Walk0");
							goto IL_F64;
						}
						this.extraanims[num14].Play("Walk1");
						goto IL_F64;
						IL_BC3:
						this.extraanims[num14].Play("Normal");
					}
				}
				else
				{
					int num2 = this.animstate;
					if (num2 <= 3)
					{
						if (num2 == 1)
						{
							goto IL_A85;
						}
						if (num2 - 2 <= 1)
						{
							this.extraanims[num14].Play("Hang");
							goto IL_F64;
						}
					}
					else if (num2 != 11)
					{
						if (num2 != 14 && num2 == 101)
						{
							goto IL_A85;
						}
					}
					else
					{
						if (num14 % 2 == 0)
						{
							this.extraanims[num14].Play("Hurt0");
							goto IL_F64;
						}
						this.extraanims[num14].Play("Hurt1");
						goto IL_F64;
					}
					this.extraanims[num14].Play("Normal");
					goto IL_F64;
					IL_A85:
					if (num14 % 2 == 0)
					{
						this.extraanims[num14].Play("Walk0");
					}
					else
					{
						this.extraanims[num14].Play("Walk1");
					}
				}
				IL_F64:;
			}
			if (flag && this.animstate == 112)
			{
				this.animspecific = new GameObject[]
				{
					Object.Instantiate(Resources.Load("Prefabs/Objects/Web")) as GameObject
				};
				this.animspecific[0].transform.parent = this.extraanims[0].transform.GetChild(0).GetChild(0);
				this.animspecific[0].transform.localScale = new Vector3(150f, 45f, 10f);
				this.animspecific[0].transform.localPosition = new Vector3(-0.213f, 0.621f, -1.721f);
				this.animspecific[0].transform.localEulerAngles = new Vector3(0.65f, 287f, 30.275f);
				return;
			}
		}
	}

	// Token: 0x060001D4 RID: 468 RVA: 0x0001727A File Offset: 0x0001547A
	public GameObject GetExtras(int id, bool anim)
	{
		if (anim)
		{
			return this.extraanims[id].gameObject;
		}
		return this.extras[id];
	}

	// Token: 0x060001D5 RID: 469 RVA: 0x00017298 File Offset: 0x00015498
	public void Revive()
	{
		this.iskill = false;
		this.dead = false;
		this.nocondition = false;
		if (this.deathcoroutine != null)
		{
			base.StopCoroutine(this.deathcoroutine);
		}
		this.LockRigid(false);
		this.ccol.enabled = true;
		this.ccol.center = this.initialcenter;
		this.ccol.height = this.initialcolliderdata.x;
		this.ccol.radius = this.initialcolliderdata.y;
		this.transform.position = this.startpos.Value + new Vector3(0f, 0.25f);
		this.spin = Vector3.zero;
		this.spritetransform.localEulerAngles = Vector3.zero;
		this.SetOverrides(false, false, false, false, false, false);
	}

	// Token: 0x060001D6 RID: 470 RVA: 0x00017370 File Offset: 0x00015570
	public static void ChompyRibbon(SpriteRenderer sprite)
	{
		int num = MainManager.instance.flagvar[56];
		switch (num)
		{
		case 168:
			sprite.material.color = Color.Lerp(Color.red, Color.blue, 0.65f);
			return;
		case 169:
			sprite.material.color = Color.Lerp(Color.yellow, Color.black, 0.2f);
			return;
		case 170:
			sprite.material.color = Color.Lerp(Color.green, Color.black, 0.3f);
			return;
		default:
			if (num != 185)
			{
				sprite.material.color = Color.Lerp(Color.red, Color.white, 0.5f);
				return;
			}
			sprite.material.color = Color.Lerp(Color.white, Color.blue, 0.5f);
			return;
		}
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x00017450 File Offset: 0x00015650
	private void AnimSpecificQuirks()
	{
		MainManager.AnimIDs animIDs = this.animid + MainManager.AnimIDs.Bee;
		if (animIDs <= MainManager.AnimIDs.ChompyChan)
		{
			if (animIDs <= MainManager.AnimIDs.Midge)
			{
				if (animIDs <= MainManager.AnimIDs.Seedling)
				{
					if (animIDs == MainManager.AnimIDs.Zasp)
					{
						int num = this.animstate;
						if (num <= 13)
						{
							if (num == 11 || num == 13)
							{
								goto IL_726;
							}
						}
						else
						{
							if (num == 106)
							{
								goto IL_726;
							}
							if (num == 111)
							{
								this.shadowsize = 1.75f;
								goto IL_1604;
							}
						}
						this.shadowsize = 1f;
						goto IL_1604;
						IL_726:
						this.shadowsize = 1.5f;
						goto IL_1604;
					}
					if (animIDs != MainManager.AnimIDs.Seedling)
					{
						goto IL_1604;
					}
				}
				else if (animIDs != MainManager.AnimIDs.ShielderAnt)
				{
					if (animIDs != MainManager.AnimIDs.Midge)
					{
						goto IL_1604;
					}
					if (this.extras == null || this.extras.Length == 0)
					{
						goto IL_1604;
					}
					if (this.icecube == null)
					{
						int num = this.animstate;
						if (num <= 23)
						{
							if (num != 11 && num != 23)
							{
								goto IL_140A;
							}
						}
						else if (num != 26 && num != 30)
						{
							if (num - 100 > 1)
							{
								goto IL_140A;
							}
							for (int i = 0; i < this.extras.Length; i++)
							{
								if (this.extras[i] != null)
								{
									this.extras[i].transform.localPosition = new Vector3(0.15f, 0.6f);
									this.extras[i].transform.localEulerAngles = new Vector3((float)((i == 0) ? -1 : 1) * 45f, 0f, -15f);
								}
							}
							goto IL_1604;
						}
						bool flag = this.animstate == 23 || this.animstate == 26;
						float num2 = (float)(flag ? 40 : 15);
						float y = flag ? 0.5f : 0.8f;
						for (int j = 0; j < this.extras.Length; j++)
						{
							if (this.extras[j] != null)
							{
								this.extras[j].transform.localPosition = new Vector3(0f, y);
								this.extras[j].transform.localEulerAngles = new Vector3((float)((j == 0) ? -90 : 90) + Mathf.Sin(Time.time * num2 * (float)((j == 0) ? 1 : -1)) * 50f, 0f, -15f);
							}
						}
						goto IL_1604;
						IL_140A:
						for (int k = 0; k < this.extras.Length; k++)
						{
							float num3 = (this.animstate == 14) ? 10f : 35f;
							y = 1f;
							if (this.extras[k] != null)
							{
								this.extras[k].transform.localPosition = new Vector3(0f, y);
								if (this.height > 0.1f)
								{
									this.extras[k].transform.localEulerAngles = new Vector3((float)((k == 0) ? -90 : 90) + Mathf.Sin(Time.time * 35f * (float)((k == 0) ? 1 : -1)) * 50f, 0f, -15f);
								}
								else
								{
									this.extras[k].transform.localEulerAngles = new Vector3((float)((k == 0) ? -1 : 1) * 130f, 0f, -50f);
								}
							}
						}
						goto IL_1604;
					}
					for (int l = 0; l < this.extras.Length; l++)
					{
						if (this.extras[l] != null)
						{
							this.extras[l].transform.localPosition = new Vector3(0f, 1f);
							this.extras[l].transform.localEulerAngles = new Vector3((float)((l == 0) ? -1 : 1) * 140f, 0f, -15f);
						}
					}
					goto IL_1604;
				}
				else
				{
					int num = this.animstate;
					if (num != 18)
					{
						this.shadowsize = 1f;
						goto IL_1604;
					}
					this.shadowsize = 2f;
					goto IL_1604;
				}
			}
			else if (animIDs <= MainManager.AnimIDs.Scarlet)
			{
				if (animIDs != MainManager.AnimIDs.VenusGuardian)
				{
					if (animIDs != MainManager.AnimIDs.Scarlet)
					{
						goto IL_1604;
					}
					int num = this.animstate;
					if (num <= 10)
					{
						if (num <= 1 || num == 5 || num == 10)
						{
							goto IL_6E4;
						}
					}
					else if (num == 13 || num - 104 <= 2 || num - 109 <= 7)
					{
						goto IL_6E4;
					}
					this.shadowsize = 2.5f;
					goto IL_1604;
					IL_6E4:
					this.shadowsize = 1.25f;
					goto IL_1604;
				}
				else
				{
					if (this.extras == null || this.extras.Length == 0)
					{
						goto IL_1604;
					}
					this.extras[2].transform.eulerAngles = Vector3.zero;
					if (this.height > 0.1f && this.icecube == null)
					{
						for (int m = 0; m < 2; m++)
						{
							this.extras[m].transform.localEulerAngles = new Vector3((float)((m == 0) ? -90 : 90) + Mathf.Sin(Time.time * 10f * (float)((m == 0) ? 1 : -1)) * 30f, 0f, 0f);
						}
						goto IL_1604;
					}
					goto IL_1604;
				}
			}
			else if (animIDs != MainManager.AnimIDs.MotherChomper)
			{
				if (animIDs != MainManager.AnimIDs.ChompyChan)
				{
					goto IL_1604;
				}
				if (this.extrasprites == null || this.extrasprites.Length == 0)
				{
					this.extrasprites = new SpriteRenderer[]
					{
						this.model.GetChild(0).GetComponent<SpriteRenderer>()
					};
				}
				this.extrasprites[0].enabled = MainManager.instance.flags[404];
				EntityControl.ChompyRibbon(this.extrasprites[0]);
				goto IL_1604;
			}
			else
			{
				if (this.extras != null && this.extras.Length != 0)
				{
					this.extras[0].transform.eulerAngles = Vector3.zero;
					goto IL_1604;
				}
				goto IL_1604;
			}
		}
		else if (animIDs <= MainManager.AnimIDs.Watcher)
		{
			if (animIDs <= MainManager.AnimIDs.Krawler)
			{
				if (animIDs != MainManager.AnimIDs.BeeBot)
				{
					if (animIDs != MainManager.AnimIDs.Krawler)
					{
						goto IL_1604;
					}
				}
				else
				{
					if (this.iskill || this.deathcoroutine != null || this.dead)
					{
						goto IL_1604;
					}
					if (this.battle)
					{
						if (!(this.anim != null) || !(MainManager.battle != null) || MainManager.battle.enemydata.Length == 0 || this.battleid >= MainManager.battle.enemydata.Length || MainManager.battle.enemydata[this.battleid].data == null || MainManager.battle.enemydata[this.battleid].data.Length == 0)
						{
							goto IL_1604;
						}
						if (this.animstate <= 1 && MainManager.battle.enemydata[this.battleid].data[0] == 1)
						{
							this.anim.Play("Idle2");
							goto IL_1604;
						}
						if (this.animstate == 11 && MainManager.battle.enemydata[this.battleid].data[0] == 1)
						{
							this.anim.Play("Hurt2");
							goto IL_1604;
						}
						goto IL_1604;
					}
					else
					{
						if (!(this.npcdata != null) || (!this.npcdata.HasBehavior(NPCControl.ActionBehaviors.ShootProjectile) && !this.npcdata.HasBehavior(NPCControl.ActionBehaviors.ShootProjectilePredict)))
						{
							goto IL_1604;
						}
						if (this.animstate <= 1)
						{
							this.anim.Play("Idle2");
							goto IL_1604;
						}
						if (this.animstate == 11)
						{
							this.anim.Play("Hurt2");
							goto IL_1604;
						}
						goto IL_1604;
					}
				}
			}
			else if (animIDs != MainManager.AnimIDs.CursedSkull)
			{
				if (animIDs != MainManager.AnimIDs.Watcher)
				{
					goto IL_1604;
				}
				this.overrideshadow = this.digging;
				this.shadow.enabled = !this.digging;
				if (this.extras != null && this.extras.Length != 0)
				{
					Vector3 vector = this.spritetransform.localPosition + new Vector3(0f, this.height);
					this.extras[0].transform.localEulerAngles = new Vector3(-40f, 50f);
					if (this.digging || this.animstate == 18 || this.animstate == 105)
					{
						vector = new Vector3(0f, -2f);
					}
					else
					{
						int num = this.animstate;
						if (num != 13)
						{
							vector += new Vector3(-1.4f, 2.4f + Mathf.Sin(Time.time * 4f) * 0.1f, -0.1f);
						}
						else
						{
							vector += new Vector3(-1.3f, 3f + Mathf.Sin(Time.time * 4f) * 0.1f, -0.1f);
						}
					}
					this.extras[0].transform.localPosition = Vector3.Lerp(this.extras[0].transform.localPosition, vector, 0.1f);
					goto IL_1604;
				}
				goto IL_1604;
			}
			if (this.lastice == this.inice || (!(this.npcdata == null) && !(this.npcdata.disguiseobj == null) && this.npcdata.disguiseobj.gameObject.activeInHierarchy))
			{
				goto IL_1604;
			}
			MainManager.PlayParticle("IceShatter", this.transform.position + new Vector3(0f, 0.5f, -0.1f));
			if (this.inice)
			{
				this.extras[0].GetComponent<ParticleSystem>().Play();
				goto IL_1604;
			}
			this.extras[0].GetComponent<ParticleSystem>().Stop();
			goto IL_1604;
		}
		else if (animIDs <= MainManager.AnimIDs.MidgeBroodmother)
		{
			switch (animIDs)
			{
			case MainManager.AnimIDs.SeedlingKing:
				if (this.subentity != null && this.subentity.Length != 0)
				{
					for (int n = 0; n < this.subentity.Length; n++)
					{
						if (this.animstate == 11)
						{
							this.subentity[n].animstate = 11;
						}
						else
						{
							this.subentity[n].animstate = 23;
						}
					}
					goto IL_1604;
				}
				goto IL_1604;
			case MainManager.AnimIDs.Yin:
			case MainManager.AnimIDs.Plumpling:
				goto IL_1604;
			case MainManager.AnimIDs.Flowering:
				if (this.extras != null && this.extras.Length != 0 && this.extras[0] != null)
				{
					this.extras[0].gameObject.SetActive(this.flyinganim || this.animstate >= 100);
				}
				break;
			case MainManager.AnimIDs.JumpingSpider:
				if (this.animstate == 23)
				{
					this.walktype = EntityControl.WalkType.Jump;
					goto IL_1604;
				}
				this.walktype = EntityControl.WalkType.Normal;
				goto IL_1604;
			default:
				if (animIDs != MainManager.AnimIDs.MidgeBroodmother)
				{
					goto IL_1604;
				}
				if (this.model != null)
				{
					if (this.extras == null || this.extras.Length == 0)
					{
						this.extras = new GameObject[4];
						for (int num4 = 0; num4 < this.extras.Length; num4++)
						{
							this.extras[num4] = this.model.GetChild(0).GetChild(num4).gameObject;
						}
					}
					new float[0];
					float[] array = new float[]
					{
						1f,
						-1f,
						1f,
						-1f
					};
					for (int num5 = 0; num5 < this.extras.Length; num5++)
					{
						if (this.icecube == null)
						{
							if (this.flyinganim || this.animstate == 11)
							{
								this.extras[num5].transform.localEulerAngles = new Vector3(Mathf.Sin(Time.time * 40f * array[num5]) * 50f, 0f, -15f);
							}
						}
						else
						{
							this.extras[num5].transform.localEulerAngles = Vector3.zero;
						}
					}
					goto IL_1604;
				}
				goto IL_1604;
			}
		}
		else if (animIDs != MainManager.AnimIDs.UltimaxTank)
		{
			if (animIDs == MainManager.AnimIDs.DeadLanderB)
			{
				if (this.extrasprites == null)
				{
					this.spinextra = new Vector3[]
					{
						new Vector3(-0.9f, 1.65f, 0.2f),
						new Vector3(-0.45f, 1.8f, -0.125f),
						new Vector3(-0.1f, 2f, 0.15f),
						new Vector3(0.55f, 1.65f, -0.1f),
						new Vector3(0.9f, 1.45f, 0.25f)
					};
					this.extrasprites = new SpriteRenderer[this.spinextra.Length];
					this.extralines = new LineRenderer[this.spinextra.Length];
					this.speedbuffer = new float[]
					{
						0.5f,
						0.34f,
						0.15f,
						0.3f,
						0.6f
					};
					Sprite sprite = Resources.LoadAll<Sprite>("Sprites/Entities/deadlandera")[39];
					for (int num6 = 0; num6 < this.extrasprites.Length; num6++)
					{
						this.extrasprites[num6] = MainManager.NewSpriteObject(this.spinextra[num6], this.spritetransform, sprite);
						this.extrasprites[num6].transform.localEulerAngles = Vector3.zero;
						this.extrasprites[num6].transform.localScale = Vector3.one;
						if (this.hologram)
						{
							this.extrasprites[num6].material = MainManager.holosprite;
						}
						if (this.cotunknown)
						{
							this.sprite.material.color = EntityControl.cot3d;
							this.extrasprites[num6].material.color = EntityControl.cot3d;
							this.sprite.material.color = this.extrasprites[num6].material.color;
						}
						GameObject gameObject = new GameObject("line");
						gameObject.transform.parent = this.spritetransform;
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localScale = Vector3.one;
						this.extralines[num6] = gameObject.AddComponent<LineRenderer>();
						this.extralines[num6].useWorldSpace = false;
						this.extralines[num6].material = (this.cotunknown ? MainManager.holosprite : MainManager.spritemat);
						this.extralines[num6].material.color = (this.cotunknown ? this.extrasprites[num6].material.color : Color.black);
						this.extralines[num6].startWidth = 0.1f;
						this.extralines[num6].endWidth = 0.1f;
						this.extralines[num6].shadowCastingMode = ShadowCastingMode.Off;
					}
				}
				for (int num7 = 0; num7 < this.extrasprites.Length; num7++)
				{
					this.extrasprites[num7].transform.localPosition = this.spinextra[num7] + new Vector3(Mathf.Sin(Time.time * this.speedbuffer[num7]) * 0.15f, Mathf.Cos(Time.time * this.speedbuffer[num7]) * 0.15f) * (float)((num7 % 2 == 0) ? 1 : -1);
					this.extralines[num7].SetPositions(new Vector3[]
					{
						Vector3.Lerp(Vector3.left, Vector3.right, (float)(num7 + 1) / (float)this.extrasprites.Length) * 0.75f + new Vector3(0f, 0.75f, 0.1f),
						this.extrasprites[num7].transform.localPosition + new Vector3(0f, 0f, 0.05f)
					});
					this.extrasprites[num7].material.color = new Color(this.extrasprites[num7].material.color.r, this.extrasprites[num7].material.color.g, this.extrasprites[num7].material.color.b, this.sprite.material.color.a);
					this.extralines[num7].material.color = new Color(this.extralines[num7].material.color.r, this.extralines[num7].material.color.g, this.extralines[num7].material.color.b, this.sprite.material.color.a);
					this.extrasprites[num7].enabled = this.sprite.enabled;
					this.extralines[num7].enabled = this.sprite.enabled;
				}
				goto IL_1604;
			}
			switch (animIDs)
			{
			case MainManager.AnimIDs.KeyR:
			case MainManager.AnimIDs.KeyL:
				if (this.animstate == 100 || this.inice)
				{
					goto IL_1604;
				}
				if (this.animstate == 11)
				{
					this.spritetransform.eulerAngles = new Vector3(0f, 0f, (float)((Mathf.Sin(Time.time * 40f) > 0f) ? ((this.animid + 1 == 369) ? -20 : -35) : ((this.animid + 1 == 369) ? -5 : -25)));
					goto IL_1604;
				}
				this.spritetransform.eulerAngles = new Vector3(0f, 0f, (float)((this.animid + 1 == 370) ? -30 : -15));
				goto IL_1604;
			case MainManager.AnimIDs.Tablet:
				if (!(MainManager.battle != null) || MainManager.battle.enemydata.Length <= this.battleid || MainManager.battle.enemydata[this.battleid].hp >= 3)
				{
					goto IL_1604;
				}
				this.startscale = Vector3.one * 0.75f;
				if (MainManager.battle.enemydata[this.battleid].hp == 2)
				{
					this.sprite.sprite = MainManager.guisprites[162];
					goto IL_1604;
				}
				this.sprite.sprite = MainManager.guisprites[163];
				goto IL_1604;
			case MainManager.AnimIDs.EverlastingKing:
			case MainManager.AnimIDs.YinMoth:
				goto IL_1604;
			case MainManager.AnimIDs.Pitcher:
			case MainManager.AnimIDs.PitcherSummon:
				if (this.extras != null)
				{
					this.extras[1].transform.position = this.extras[0].transform.position + this.spinextra[0];
					goto IL_1604;
				}
				goto IL_1604;
			default:
				goto IL_1604;
			}
		}
		else
		{
			if (this.extras == null || this.extras.Length == 0)
			{
				this.extras = new GameObject[this.model.GetChild(0).childCount + 1];
				for (int num8 = 0; num8 < this.extras.Length - 2; num8++)
				{
					this.extras[num8] = this.model.GetChild(0).GetChild(num8 + 1).gameObject;
					this.extras[num8].transform.localScale = Vector3.one * 0.95f;
				}
				this.extras[this.extras.Length - 1] = this.model.GetChild(0).GetChild(0).gameObject;
				this.extras[this.extras.Length - 2] = this.model.GetChild(0).GetChild(0).GetChild(2).gameObject;
			}
			if (this.extras == null)
			{
				goto IL_1604;
			}
			bool flag2 = true;
			int num = this.animstate;
			if (num == 1 || num == 104)
			{
				for (int num9 = 0; num9 < this.extras.Length - 2; num9++)
				{
					this.extras[num9].transform.localEulerAngles += new Vector3(0f, -15f) * MainManager.framestep;
				}
			}
			if (flag2)
			{
				this.extras[this.extras.Length - 1].transform.localPosition = new Vector3(0f, 0f, (Mathf.Sin(Time.time * 30f) > 0f) ? 0f : 0.05f);
				goto IL_1604;
			}
			goto IL_1604;
		}
		bool flag3 = this.animid + 1 == 232;
		if ((flag3 || this.height > 0.1f) && this.animstate <= 1 && (this.extras == null || this.extras.Length == 0 || this.extras[0] == null))
		{
			this.extras = new GameObject[]
			{
				MainManager.NewSpriteObject("copter", new Vector3(0f, 1.5f, 0f), new Vector3(85f, 0f, 0f), this.spritetransform, MainManager.instance.projectilepsrites[flag3 ? 11 : 1], this.sprite.material).gameObject
			};
			this.spinextra = new Vector3[]
			{
				new Vector3(0f, 0f, 20f)
			};
			if (flag3)
			{
				this.extras[0].transform.localEulerAngles = new Vector3(85f, 25f, 25f);
				this.extras[0].transform.localPosition = new Vector3(0f, 0.25f);
				this.extras[0].transform.localScale = Vector3.one * 0.75f;
			}
			else
			{
				this.extras[0].transform.localScale = Vector3.one;
			}
			if (this.bobspeed < 0.1f)
			{
				this.bobspeed = 0.2f;
				this.startbs = this.bobspeed;
			}
			if (this.bobrange < 0.1f)
			{
				this.bobrange = 4f;
				this.startbf = this.bobrange;
			}
		}
		IL_1604:
		if (this.spinextra != null && this.originalid != 352 && this.originalid != 373 && this.originalid != 374)
		{
			for (int num10 = 0; num10 < this.spinextra.Length; num10++)
			{
				if (this.extras[num10] != null)
				{
					this.extras[num10].transform.localEulerAngles += this.spinextra[num10] * MainManager.framestep;
				}
			}
		}
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x00018AF0 File Offset: 0x00016CF0
	public void FlipSpriteAngleAt(Vector3 target, Vector3 offset)
	{
		this.overrideonlyflip = true;
		this.spritetransform.LookAt(target);
		this.spritetransform.localEulerAngles = new Vector3(0f, this.spritetransform.localEulerAngles.y, 0f) + offset;
	}

	// Token: 0x060001D9 RID: 473 RVA: 0x00018B40 File Offset: 0x00016D40
	public void FlipSpriteAngleAt(Vector3 target)
	{
		this.FlipSpriteAngleAt(target, Vector3.zero);
	}

	// Token: 0x060001DA RID: 474 RVA: 0x00018B50 File Offset: 0x00016D50
	public void CheckNear()
	{
		if (MainManager.player != null && base.name.Contains("NEAR") && MainManager.GetDistance(this.transform.position.z, MainManager.player.transform.position.z) > 30f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x060001DB RID: 475 RVA: 0x00018BB8 File Offset: 0x00016DB8
	private void LateStart()
	{
		string tag = base.tag;
		if (!(tag == "Follower") && !(tag == "PFollower"))
		{
			if (!(tag == "Player"))
			{
				if (!(tag == "NPC") && !(tag == "Enemy"))
				{
					goto IL_114;
				}
			}
			else
			{
				this.isplayer = true;
				if (this.battle)
				{
					this.CreateShield();
				}
			}
		}
		else
		{
			this.CreateDetector(new Vector3(0.8f, 0.7f, 0.3f), new Vector3(0f, 0.5f, 0.65f));
			Physics.IgnoreCollision(this.detect, this.following.ccol, true);
			this.isfollower = true;
		}
		if (!this.item)
		{
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Particles/WalkDust")) as GameObject;
			this.movesmoke = gameObject.transform;
			this.movesmoke.parent = this.transform;
			this.movesmoke.localPosition = Vector3.zero;
		}
		if (this.anim != null)
		{
			this.anim.cullingMode = AnimatorCullingMode.CullCompletely;
		}
		IL_114:
		if (this.item)
		{
			this.overrideanim = true;
			this.overrridejump = true;
			this.oldstate = -1;
			this.CreateFeet();
		}
		if (MainManager.map != null)
		{
			this.originalmap = MainManager.map.transform;
		}
		if (this.npcdata != null)
		{
			this.npcdata.currentdialogueindex = this.npcdata.GetDialogueIndex();
			if (this.npcdata.entitytype == NPCControl.NPCType.Object && this.npcdata.objecttype == NPCControl.ObjectTypes.PushRock && this.npcdata.internalcollider != null)
			{
				if (this.feet == null)
				{
					this.CreateFeet();
				}
				for (int i = 0; i < this.npcdata.internalcollider.Length; i++)
				{
					if (this.npcdata.internalcollider[i] != null)
					{
						Physics.IgnoreCollision(this.feet.GetComponent<Collider>(), this.npcdata.internalcollider[i], true);
					}
				}
			}
		}
		if (this.CheckForCharacterEntity() && this.feet == null)
		{
			this.CreateFeet();
		}
		if (this.startpos != null)
		{
			this.transform.position = this.startpos.Value;
		}
		else
		{
			this.startpos = new Vector3?(this.transform.position);
		}
		this.startbf = this.bobrange;
		this.startbs = this.bobspeed;
		this.truescale = this.startscale;
		this.UpdateMoveSmoke();
		this.setup = true;
		this.oldstate = -1;
	}

	// Token: 0x060001DC RID: 476 RVA: 0x00018E58 File Offset: 0x00017058
	public void CreateShadow()
	{
		this.shadow = new GameObject("shadow").AddComponent<SpriteRenderer>();
		this.shadow.transform.parent = this.transform;
		this.shadow.transform.position = new Vector3(0f, -999f);
		this.shadowtransform = this.shadow.transform;
		this.shadow.sprite = MainManager.shadowsprite;
		this.shadow.color = new Color(1f, 1f, 1f, 0.4f);
		this.shadow.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
		this.shadow.material.renderQueue = 2900;
	}

	// Token: 0x060001DD RID: 477 RVA: 0x00018F2D File Offset: 0x0001712D
	public void ResetOverrides()
	{
		this.SetOverrides(false, false, false, false, false, false);
	}

	// Token: 0x060001DE RID: 478 RVA: 0x00018F3C File Offset: 0x0001713C
	public void CreateFeet()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/GroundDetector")) as GameObject;
		this.feet = gameObject.GetComponent<GroundDetector>();
		this.feet.transform.parent = this.transform;
		this.feet.transform.localPosition = Vector3.zero;
		this.feet.transform.localEulerAngles = new Vector3(-90f, 0f);
		this.feet.transform.localScale = new Vector3(this.ccol.radius * 2f - 0.25f, this.ccol.radius * 2f - 0.25f, 0.1f);
		this.feet.parent = this;
	}

	// Token: 0x060001DF RID: 479 RVA: 0x00019008 File Offset: 0x00017208
	public void CheckSpecialID()
	{
		if (!this.overrideshadow)
		{
			this.hasshadow = (this.animid > -1);
		}
		this.originalid = this.animid;
		Vector3 offset = Vector3.zero;
		MainManager.AnimIDs animIDs = this.originalid + MainManager.AnimIDs.Bee;
		if (animIDs != MainManager.AnimIDs.Strider)
		{
			if (animIDs - MainManager.AnimIDs.KeyR <= 2)
			{
				this.nomodel = true;
				this.overrideanim = true;
				this.overrideanimfunc = true;
				this.overridemovesmoke = true;
				this.overrridejump = true;
				this.minheight = MainManager.endata[this.originalid + 1].minheight;
				this.startbf = this.bobrange;
				this.startbs = this.bobspeed;
				switch (this.originalid + 1)
				{
				case 369:
					this.sprite.sprite = Resources.LoadAll<Sprite>("Sprites/Objects/artifacts")[3];
					goto IL_362;
				case 370:
					this.sprite.sprite = Resources.LoadAll<Sprite>("Sprites/Objects/artifacts")[2];
					goto IL_362;
				case 371:
					this.nomodel = true;
					this.sprite.sprite = Resources.LoadAll<Sprite>("Sprites/Objects/artifacts")[1];
					goto IL_362;
				default:
					goto IL_362;
				}
			}
		}
		else
		{
			this.overridemovesmoke = true;
		}
		if (this.originalid > -1)
		{
			offset = MainManager.endata[this.originalid + 1].modeloffset;
			this.startscale = MainManager.endata[this.originalid + 1].startscale;
			this.hasiceanim = MainManager.endata[this.originalid + 1].hasiceanim;
			this.nofallfrozen = MainManager.endata[this.originalid + 1].freezenofall;
			this.minheight = MainManager.endata[this.originalid + 1].minheight;
			if (this.bobrange < 0.1f)
			{
				this.bobrange = MainManager.endata[this.originalid + 1].startbobfreq;
			}
			if (this.bobspeed < 0.1f)
			{
				this.bobspeed = MainManager.endata[this.originalid + 1].startbobspd;
			}
			if (!this.battle && this.minheight > 0.1f && this.height < this.minheight)
			{
				this.height = this.minheight;
			}
			this.overridefly = MainManager.endata[this.originalid + 1].noflyanim;
			this.diganim = MainManager.endata[this.originalid + 1].diganim;
			this.shakeondrop = MainManager.endata[this.originalid + 1].shakeondrop;
			this.hasshadow = !MainManager.endata[this.originalid + 1].noshadow;
			this.overrideshadow = MainManager.endata[this.originalid + 1].forceshadow;
			this.overrridejump = !MainManager.endata[this.originalid + 1].dontoverridejump;
			this.shadowsize = MainManager.endata[this.originalid + 1].shadowsize;
			this.walktype = MainManager.endata[this.originalid + 1].walktype;
			if (this.overrideshadow)
			{
				this.CreateShadow();
			}
			this.startbf = this.bobrange;
			this.startbs = this.bobspeed;
		}
		else
		{
			this.overrridejump = true;
		}
		IL_362:
		bool flag = false;
		animIDs = this.animid + MainManager.AnimIDs.Bee;
		if (animIDs <= MainManager.AnimIDs.Turret)
		{
			if (animIDs <= MainManager.AnimIDs.AngryPlant)
			{
				if (animIDs != MainManager.AnimIDs.None)
				{
					switch (animIDs)
					{
					case MainManager.AnimIDs.CoilyVine:
						break;
					case MainManager.AnimIDs.Spuder:
						goto IL_F51;
					case MainManager.AnimIDs.CrystalBerry:
						this.AddModel("Prefabs/Objects/" + (this.animid + MainManager.AnimIDs.Bee).ToString(), offset);
						this.animid = 3;
						this.spin = new Vector3(0f, 2f, 0f);
						this.item = true;
						goto IL_F51;
					case MainManager.AnimIDs.Seedling:
						if (this.speed > 0f)
						{
							goto IL_F51;
						}
						this.speed = 2.5f;
						if (this.npcdata != null)
						{
							this.npcdata.speedmultiplier = 1f;
							goto IL_F51;
						}
						goto IL_F51;
					default:
					{
						if (animIDs != MainManager.AnimIDs.AngryPlant)
						{
							goto IL_F51;
						}
						this.extras = new GameObject[]
						{
							Object.Instantiate(Resources.Load("Prefabs/Objects/LeafSkirt")) as GameObject
						};
						Renderer[] componentsInChildren = this.extras[0].GetComponentsInChildren<Renderer>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].shadowCastingMode = ShadowCastingMode.Off;
							if (this.hologram)
							{
								Texture mainTexture = componentsInChildren[i].material.mainTexture;
								componentsInChildren[i].material = MainManager.holosprite;
								componentsInChildren[i].material.mainTexture = mainTexture;
								if (this.cotunknown)
								{
									componentsInChildren[i].material.color = EntityControl.cot3d;
								}
							}
						}
						this.extras[0].transform.parent = this.spritetransform;
						this.extras[0].transform.localScale = new Vector3(1f, 0.2f, 1f);
						this.extras[0].transform.localPosition = new Vector3(0.1f, 0.125f, 0f);
						this.extras[0].transform.localEulerAngles = Vector3.zero;
						this.spinextra = new Vector3[1];
						goto IL_F51;
					}
					}
				}
				this.rigid.useGravity = false;
				this.rigid.constraints = RigidbodyConstraints.FreezeAll;
				this.ccol.enabled = false;
				if (this.animid + 1 == 42)
				{
					flag = true;
				}
			}
			else if (animIDs <= MainManager.AnimIDs.GoldenSeedling)
			{
				if (animIDs != MainManager.AnimIDs.Midge)
				{
					if (animIDs == MainManager.AnimIDs.GoldenSeedling)
					{
						Transform transform = (Object.Instantiate(Resources.Load("Prefabs/Particles/GoldStars"), this.transform.position, Quaternion.identity) as GameObject).transform;
						transform.parent = this.transform;
						transform.localPosition = new Vector3(0f, 0.5f, -0.2f);
					}
				}
				else if (this.extras == null || this.extras.Length == 0)
				{
					this.extras = new GameObject[2];
					for (int j = 0; j < this.extras.Length; j++)
					{
						this.extras[j] = new GameObject("wing" + (j + 1));
						SpriteRenderer spriteRenderer = this.extras[j].AddComponent<SpriteRenderer>();
						spriteRenderer.material = this.sprite.material;
						spriteRenderer.sprite = Resources.LoadAll<Sprite>("Sprites/Entities/Midge")[13];
						spriteRenderer.gameObject.layer = this.sprite.gameObject.layer;
						spriteRenderer.transform.parent = this.spritetransform;
						spriteRenderer.transform.localPosition = new Vector3(0f, 1.5f);
					}
					this.extras[1].transform.localScale = new Vector3(1f, 1f, -1f);
				}
			}
			else if (animIDs != MainManager.AnimIDs.BeeBot)
			{
				if (animIDs == MainManager.AnimIDs.Turret)
				{
					this.extras = new GameObject[]
					{
						Object.Instantiate(Resources.Load("Prefabs/Objects/turretbase")) as GameObject
					};
					this.extras[0].transform.parent = this.spritetransform;
					this.extras[0].transform.localPosition = Vector3.zero;
					if (this.cotunknown)
					{
						this.extras[0].GetComponent<Renderer>().material.color = EntityControl.cot3d;
					}
				}
			}
			else if (this.battle && (MainManager.battle.enemydata[this.battleid].data == null || MainManager.battle.enemydata[this.battleid].data.Length == 0))
			{
				MainManager.battle.enemydata[this.battleid].data = new int[]
				{
					Random.Range(0, 2)
				};
				this.UpdateAnimSpecific();
			}
		}
		else if (animIDs <= MainManager.AnimIDs.SeedlingKing)
		{
			if (animIDs <= MainManager.AnimIDs.Ahoneynation)
			{
				if (animIDs != MainManager.AnimIDs.BeeBoss)
				{
					if (animIDs == MainManager.AnimIDs.Ahoneynation)
					{
						this.rotater.gameObject.AddComponent<SpriteBounce>().SetUp(0.03f, 9f);
					}
				}
				else
				{
					GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Particles/BeeBossBottomSmoke"), new Vector3(0f, -999f), Quaternion.Euler(-90f, 0f, 0f)) as GameObject;
					gameObject.transform.parent = this.shadowtransform;
					gameObject.transform.localPosition = new Vector3(0f, 0.1f, 0f);
					this.shadowtransform.localPosition = Vector3.zero;
				}
			}
			else
			{
				switch (animIDs)
				{
				case MainManager.AnimIDs.Krawler:
				case MainManager.AnimIDs.CursedSkull:
				{
					bool flag2 = this.forcefire || (MainManager.instance.areaid == 15 && MainManager.map.mapid != MainManager.Maps.GiantLairFridgeInside && MainManager.map.mapid != MainManager.Maps.GiantLairFridgeOutside);
					this.extras = new GameObject[]
					{
						Object.Instantiate(Resources.Load("Prefabs/Particles/" + (flag2 ? "Flame" : "Snowflakes"))) as GameObject
					};
					this.extras[0].transform.parent = this.spritetransform;
					this.extras[0].transform.localPosition = new Vector3(0f, 0.75f);
					if (!this.inice && !flag2)
					{
						ParticleSystem component = this.extras[0].GetComponent<ParticleSystem>();
						component.Stop();
						component.main.playOnAwake = false;
					}
					if (flag2)
					{
						animIDs = this.animid + MainManager.AnimIDs.Bee;
						if (animIDs != MainManager.AnimIDs.Krawler)
						{
							if (animIDs == MainManager.AnimIDs.CursedSkull)
							{
								this.animid = 401;
							}
						}
						else
						{
							this.animid = 399;
						}
					}
					break;
				}
				case MainManager.AnimIDs.Cape:
				case MainManager.AnimIDs.Watcher:
					if (this.animid + 1 == 212)
					{
						this.extras = new GameObject[2];
						this.extras[0] = (Object.Instantiate(Resources.Load("Prefabs/Objects/WatcherBook")) as GameObject);
						this.extras[0].transform.parent = this.rotater.transform;
						this.extras[0].transform.localPosition = new Vector3(-1f, 2f);
						if (this.hologram)
						{
							SpriteRenderer[] componentsInChildren2 = this.extras[0].GetComponentsInChildren<SpriteRenderer>();
							for (int k = 0; k < componentsInChildren2.Length; k++)
							{
								componentsInChildren2[k].material = MainManager.holosprite;
							}
						}
						this.nodigpart = true;
					}
					else
					{
						bool flag3 = this.forcefire || (MainManager.instance.areaid == 15 && MainManager.map.mapid != MainManager.Maps.GiantLairFridgeInside);
						this.extras = new GameObject[]
						{
							Object.Instantiate(Resources.Load("Prefabs/Particles/" + (flag3 ? "Flame" : "Snowflakes"))) as GameObject
						};
						this.extras[0].transform.parent = this.spritetransform;
						this.extras[0].transform.localPosition = new Vector3(0f, 1.5f);
						if (flag3)
						{
							this.animid = 400;
						}
					}
					break;
				case MainManager.AnimIDs.IcePillarObj:
				case MainManager.AnimIDs.OldMoth:
				case MainManager.AnimIDs.PinkMoth:
					break;
				default:
					if (animIDs == MainManager.AnimIDs.SeedlingKing)
					{
						if (this.battle)
						{
							this.height = 0.75f;
							this.initialheight = this.height;
						}
						if (this.height > 0.1f)
						{
							Vector3[] array = new Vector3[]
							{
								new Vector3(-1.2f, 0f, 0.15f),
								new Vector3(-0.5f, 0f, -0.2f),
								new Vector3(0.5f, 0f, -0.3f),
								new Vector3(1.2f, 0f, 0.15f)
							};
							this.subentity = new EntityControl[4];
							for (int l = 0; l < this.subentity.Length; l++)
							{
								this.subentity[l] = EntityControl.CreateNewEntity("seedling" + l, 44, this.transform.position + array[l]);
								this.subentity[l].transform.parent = this.transform;
								this.subentity[l].hologram = this.hologram;
								this.subentity[l].battle = this.battle;
								this.subentity[l].animstate = 23;
								this.subentity[l].gameObject.layer = 9;
							}
						}
					}
					break;
				}
			}
		}
		else if (animIDs <= MainManager.AnimIDs.Zombeetle)
		{
			if (animIDs != MainManager.AnimIDs.WaspDriller)
			{
				if (animIDs == MainManager.AnimIDs.Zombeetle)
				{
					if (this.showitem && this.npcdata.vectordata[0].y == -2f)
					{
						this.extras = new GameObject[]
						{
							MainManager.NewSpriteObject(new Vector3(-0.27f, 0.665f, -0.025f), this.spritetransform, MainManager.itemsprites[0, (int)this.npcdata.vectordata[0].x]).gameObject
						};
						this.extras[0].GetComponent<SpriteRenderer>().flipX = true;
						this.extras[0].transform.localEulerAngles = new Vector3(0f, 0f, 30f);
						this.extras[0].transform.localScale = Vector3.one * 0.6f;
					}
				}
			}
			else if (this.showitem && this.npcdata.vectordata[0].y == -2f)
			{
				this.extras = new GameObject[]
				{
					MainManager.NewSpriteObject(new Vector3(0.45f, 1f, -0.025f), this.spritetransform, MainManager.itemsprites[0, (int)this.npcdata.vectordata[0].x]).gameObject
				};
				this.extras[0].GetComponent<SpriteRenderer>().flipX = true;
			}
		}
		else if (animIDs != MainManager.AnimIDs.RizGrandpa)
		{
			if (animIDs == MainManager.AnimIDs.WaspTwinA)
			{
				if (this.following != null)
				{
					this.basestate = 6;
				}
			}
		}
		else
		{
			this.CreateLine(new Vector3(2.25f, 1.4f, 0.1f), this.transform.position + new Vector3(2.75f, -5f), 0.1f, Color.black, this.transform);
		}
		IL_F51:
		if ((!this.nomodel && this.originalid > -1 && MainManager.endata[this.originalid + 1].Object) || flag)
		{
			this.notalk = true;
			this.modelscale = MainManager.endata[this.originalid + 1].startscale;
			this.AddModel("Prefabs/Objects/" + (this.animid + MainManager.AnimIDs.Bee).ToString(), offset);
			this.hasshadow = false;
			if (this.animid + 1 != 25 || this.animid + 1 != 71)
			{
				this.animid = -1;
			}
		}
		if (!this.nomodel && this.originalid > -1)
		{
			if (MainManager.endata[this.originalid + 1].ismodel && !this.item)
			{
				animIDs = this.animid + MainManager.AnimIDs.Bee;
				if (animIDs == MainManager.AnimIDs.TrappedMoth)
				{
					offset = new Vector3(0f, -this.height, 0f);
				}
				this.AddModel("Prefabs/Objects/" + (this.animid + MainManager.AnimIDs.Bee).ToString(), offset);
				animIDs = this.animid + MainManager.AnimIDs.Bee;
				if (animIDs != MainManager.AnimIDs.JumpingSpider)
				{
					if (animIDs != MainManager.AnimIDs.Submarine)
					{
						if (animIDs - MainManager.AnimIDs.Pitcher <= 1)
						{
							this.extras = new GameObject[]
							{
								GameObject.FindGameObjectWithTag("PitcherEnd"),
								this.model.GetChild(1).gameObject
							};
							this.extras[0].tag = "Untagged";
							this.spinextra = new Vector3[]
							{
								new Vector3(0f, 0f, -0.2f)
							};
							SpriteRenderer[] componentsInChildren3 = this.extras[1].GetComponentsInChildren<SpriteRenderer>();
							for (int m = 0; m < componentsInChildren3.Length; m++)
							{
								componentsInChildren3[m].shadowCastingMode = ShadowCastingMode.TwoSided;
								if (this.hologram)
								{
									componentsInChildren3[m].material = MainManager.holosprite;
									componentsInChildren3[m].material.color = new Color(componentsInChildren3[m].material.color.r, componentsInChildren3[m].material.color.g, componentsInChildren3[m].material.color.b, 0.5f);
								}
							}
						}
					}
					else if (MainManager.player != null && MainManager.player.transform != this.transform && this.npcdata != null)
					{
						this.npcdata.colliderheight = 20f;
						this.ccol.radius = 2.5f;
						base.Invoke("EnableCol", 0.2f);
					}
				}
				else
				{
					for (int n = 0; n < 8; n++)
					{
						this.model.GetChild(3 + n).GetComponent<Renderer>().material.color = new Color(0.25f, 0.25f, 0.25f);
					}
				}
				if (MainManager.endata[this.originalid + 1].modelscale != Vector3.one)
				{
					this.model.transform.localEulerAngles = MainManager.endata[this.originalid + 1].modelscale;
				}
				this.UpdateAnimSpecific();
			}
			if (this.emoticonoffset.magnitude < 0.1f)
			{
				this.emoticonoffset = MainManager.endata[this.originalid + 1].freezeflipoffset;
			}
			if (this.freezesize.magnitude < 0.1f || this.freezeoffset.magnitude < 0.1f)
			{
				this.freezesize = MainManager.endata[this.originalid + 1].freezesize;
				this.freezeoffset = MainManager.endata[this.originalid + 1].freezeoffset;
			}
			else
			{
				this.freezesize = new Vector3(2f, 2f, 1f);
				this.freezeoffset = new Vector3(0f, 1f, 0f);
			}
			this.initialfrezeoffset = this.freezeoffset;
			if (MainManager.endata[this.originalid + 1].preloaddata != null && MainManager.endata[this.originalid + 1].preloaddata.Length != 0)
			{
				List<Sprite> list = new List<Sprite>();
				List<GameObject> list2 = new List<GameObject>();
				for (int num = 0; num < MainManager.endata[this.originalid + 1].preloaddata.Length; num++)
				{
					if (MainManager.endata[this.originalid + 1].preloaddata[num][0] != '$' || MainManager.battle != null)
					{
						string text = MainManager.endata[this.originalid + 1].preloaddata[num].Replace("$", "");
						if (text[0] == '&')
						{
							list.Add(Resources.Load<Sprite>("Resources/" + text.Replace("&", "")));
						}
						else
						{
							list2.Add(Resources.Load<GameObject>("Resources/" + text));
						}
					}
				}
				this.preloadedobjects = list2.ToArray();
				this.preloadedsprites = list.ToArray();
			}
			if (MainManager.map != null && !this.battle)
			{
				if (MainManager.CurrentMap() == MainManager.Maps.MetalLake)
				{
					this.startscale *= 0.35f;
					this.emoticon.transform.localScale = Vector3.one * 0.65f;
				}
				if (MainManager.map.waterfloat != null)
				{
					animIDs = this.originalid + MainManager.AnimIDs.Bee;
					if (animIDs == MainManager.AnimIDs.Strider)
					{
						this.ignorewater = true;
						this.alwaysactive = true;
						EntityControl.IgnoreColliders(this, MainManager.map.waterfloat.GetComponent<Collider>(), true);
						this.transform.position = new Vector3(this.transform.position.x, MainManager.map.waterfloat.transform.position.y, this.transform.position.z);
					}
				}
			}
		}
		animIDs = this.animid + MainManager.AnimIDs.Bee;
		if (animIDs == MainManager.AnimIDs.Scorpion && MainManager.instance.areaid == 15)
		{
			this.basestate = 150;
			this.walkstate = 151;
		}
	}

	// Token: 0x060001E0 RID: 480 RVA: 0x0001A5E4 File Offset: 0x000187E4
	public void EnableCol()
	{
		this.ccol.enabled = true;
	}

	// Token: 0x060001E1 RID: 481 RVA: 0x0001A5F2 File Offset: 0x000187F2
	public IEnumerator OverrideJumpTemp()
	{
		this.overrridejump = true;
		yield return new WaitForSeconds(0.5f);
		while (!this.onground)
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.5f);
		this.overrridejump = false;
		yield break;
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x0001A601 File Offset: 0x00018801
	public IEnumerator FadeSprite(float frametime, bool destroy)
	{
		float a = 0f;
		Color ic = this.sprite.material.color;
		do
		{
			this.sprite.material.color = new Color(ic.r, ic.g, ic.b, Mathf.Lerp(ic.a, 0f, a / frametime));
			a += MainManager.framestep;
			yield return null;
		}
		while (a < frametime);
		if (destroy)
		{
			this.LockRigid(true);
			if (this.npcdata != null)
			{
				this.npcdata.enabled = false;
			}
		}
		yield return null;
		if (destroy)
		{
			Object.Destroy(base.gameObject);
		}
		yield break;
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x0001A620 File Offset: 0x00018820
	public void ExtraAnimPlay(string arg)
	{
		if (this.extraanims != null && this.extraanims.Length != 0)
		{
			string[] array = arg.Split(new char[]
			{
				','
			});
			for (int i = 0; i < this.extraanims.Length; i++)
			{
				if (array.Length == 1)
				{
					this.extraanims[i].Play(array[0]);
				}
				else
				{
					this.extraanims[i].Play(array[i]);
				}
			}
		}
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x0001A68C File Offset: 0x0001888C
	public void AddModel(string path, Vector3 offset)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load(path)) as GameObject;
		this.model = gameObject.transform;
		gameObject.transform.parent = this.spritetransform;
		gameObject.transform.localPosition = offset;
		if (this.model.localScale.magnitude < 0.1f)
		{
			this.model.localScale = Vector3.one;
		}
		this.modelscale = this.model.localScale;
		if (gameObject.GetComponent<Animator>() != null)
		{
			this.anim = gameObject.GetComponent<Animator>();
		}
		if (this.hologram)
		{
			SpriteRenderer[] componentsInChildren = this.model.GetComponentsInChildren<SpriteRenderer>(true);
			SkinnedMeshRenderer[] componentsInChildren2 = this.model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			MeshRenderer[] componentsInChildren3 = this.model.GetComponentsInChildren<MeshRenderer>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material = MainManager.holosprite;
				if (this.cotunknown)
				{
					componentsInChildren[i].material.color = EntityControl.cot3d;
					componentsInChildren[i].color = Color.black;
				}
			}
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Texture mainTexture = componentsInChildren2[j].material.mainTexture;
				Color color = componentsInChildren2[j].material.color;
				componentsInChildren2[j].material = MainManager.holosprite;
				componentsInChildren2[j].material.mainTexture = mainTexture;
				if (this.cotunknown)
				{
					componentsInChildren2[j].material.color = EntityControl.cot3d;
				}
				else
				{
					componentsInChildren2[j].material.color = new Color(color.r, color.g, color.b, 0.5f);
				}
			}
			for (int k = 0; k < componentsInChildren3.Length; k++)
			{
				Texture mainTexture2 = componentsInChildren3[k].material.mainTexture;
				Color color2 = componentsInChildren3[k].material.color;
				componentsInChildren3[k].material = MainManager.holosprite;
				componentsInChildren3[k].material.mainTexture = mainTexture2;
				if (this.cotunknown)
				{
					componentsInChildren3[k].material.color = EntityControl.cot3d;
				}
				else
				{
					componentsInChildren3[k].material.color = new Color(color2.r, color2.g, color2.b, 0.5f);
				}
			}
		}
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x0001A8EC File Offset: 0x00018AEC
	private void Update()
	{
		if (this.activeonpause || (!MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message && !this.dead))
		{
			if (!this.overrideanimspeed)
			{
				if (this.icecube == null)
				{
					if (this.anim.speed != 1f)
					{
						this.anim.speed = 1f;
					}
				}
				else
				{
					if (this.anim.speed != 0f)
					{
						this.anim.speed = 0f;
					}
					this.animstate = 11;
				}
			}
			if (this.sprite != null)
			{
				if (MainManager.player != null && MainManager.player.transform == this.transform && MainManager.player.submarine)
				{
					this.sprite.enabled = false;
				}
				else
				{
					if (this.icooldown > 0f && MainManager.FreePlayer(false))
					{
						this.sprite.enabled = !this.sprite.enabled;
					}
					else if (!this.item && (!(this.npcdata != null) || !(this.npcdata.disguiseobj != null)))
					{
						this.sprite.enabled = true;
					}
					for (int i = 0; i < this.spritetransform.childCount; i++)
					{
						if ((this.npcdata == null || !this.npcdata.trapped) && this.spritetransform.GetChild(0).gameObject.activeSelf != this.sprite.enabled)
						{
							this.spritetransform.GetChild(0).gameObject.SetActive(this.sprite.enabled);
						}
					}
				}
			}
			if (this.icooldown > 0f)
			{
				this.icooldown -= MainManager.framestep;
			}
			if (this.lastvelocity != null)
			{
				this.rigid.useGravity = this.lastgravity;
				this.sound.volume = this.lastvolume;
				this.rigid.velocity = this.lastvelocity.Value;
				this.lastvelocity = null;
				this.pausepos = null;
				return;
			}
		}
		else if (MainManager.instance.pause)
		{
			if (this.lastvelocity == null)
			{
				this.pausepos = new Vector3?(this.transform.position);
				this.lastgravity = this.rigid.useGravity;
				this.lastvolume = this.sound.volume;
				this.rigid.useGravity = false;
				this.sound.volume = 0f;
				this.lastvelocity = new Vector3?(this.rigid.velocity);
				this.rigid.velocity = Vector3.zero;
			}
			if (!this.battle)
			{
				if (this.anim.speed != 0f)
				{
					this.anim.speed = 0f;
				}
				this.StopMoving(this.animstate);
				return;
			}
			if (this.anim.speed != 1f)
			{
				this.anim.speed = 1f;
				return;
			}
		}
		else
		{
			bool minipause = MainManager.instance.minipause;
		}
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x0001AC44 File Offset: 0x00018E44
	private void FixedUpdate()
	{
		if (!MainManager.instance.pause)
		{
			if (this.forcemove)
			{
				if (MainManager.GetSqrDistance(this.transform.position, this.forcetarget, this.ignorey) > 0.3f)
				{
					this.Move(this.forcetarget, this.forcemultiplier * Mathf.Clamp01(MainManager.GetSqrDistance(this.transform.position, this.forcetarget)), this.forceanim);
					if (this.forcejump)
					{
						if (this.detect == null)
						{
							this.CreateDetector();
						}
						this.detect.transform.LookAt(this.forcetarget);
						this.detect.transform.localEulerAngles = new Vector3(0f, this.detect.transform.localEulerAngles.y, 0f);
						if (this.hitwall && this.onground && this.jumpcooldown <= 0f)
						{
							this.Jump();
						}
					}
				}
				else
				{
					this.forcemove = false;
					this.forcejump = false;
					this.ignorey = false;
					this.StopMoving(this.forcestop);
				}
			}
			if (this.leiffly && MainManager.player != null && !MainManager.instance.inevent)
			{
				this.transform.position = Vector3.Lerp(this.transform.position, MainManager.player.transform.position + MainManager.player.entity.spritetransform.right.normalized * 1.5f + MainManager.instance.globalcamdir.forward.normalized * 0.2f, MainManager.framestep * 0.05f);
			}
		}
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x0001AE24 File Offset: 0x00019024
	private void Follow()
	{
		if (this.following != null && !MainManager.instance.overridefollower && !this.dead && !this.iskill && !this.overridefollow)
		{
			if (!this.tempfollower && (this.following.CompareTag("Player") || this.following.CompareTag("PFollower")) && MainManager.player != null)
			{
				if (MainManager.player.digging || MainManager.player.startdig)
				{
					this.backsprite = false;
					this.spin = new Vector3(0f, 30f, 0f);
					this.spritetransform.localScale = Vector3.Lerp(this.spritetransform.localScale, Vector3.zero, 0.075f);
					this.transform.position = Vector3.Lerp(this.transform.position, this.following.transform.position + MainManager.MainCamera.transform.forward / 2f, 0.1f);
					this.rigid.isKinematic = true;
					this.rigid.useGravity = false;
					this.ccol.enabled = false;
					if (this.shadow.gameObject.activeSelf)
					{
						this.shadow.gameObject.SetActive(false);
					}
					this.StopMoving(-1);
					return;
				}
				if (MainManager.player.flying)
				{
					this.backsprite = false;
					this.rigid.useGravity = false;
					this.overrridejump = true;
					this.overrideanim = true;
					this.StopForceMove(this.animstate, false);
					if (this.animid == 1)
					{
						this.animstate = 102;
						this.transform.position = MainManager.player.transform.position + MainManager.instance.globalcamdir.forward * 0.2f;
						this.flip = MainManager.player.entity.flip;
						return;
					}
					this.rigid.useGravity = false;
					this.overrideanim = true;
					this.animstate = 101;
					this.flip = MainManager.player.entity.flip;
					this.leiffly = true;
					return;
				}
				else
				{
					if (MainManager.player.shield)
					{
						this.ShieldMove(false);
						return;
					}
					this.leiffly = false;
					this.ReturnFromAction();
					this.DoFollow();
					return;
				}
			}
			else if (this.tempfollower)
			{
				if (MainManager.player.shield)
				{
					this.ShieldMove(true);
				}
				else if (MainManager.player.flying)
				{
					this.animstate = this.basestate;
					this.rigid.useGravity = false;
					this.flip = MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity.flip;
					this.transform.position = MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity.transform.position + new Vector3(MainManager.instance.playerdata[MainManager.instance.playerdata.Length - 1].entity.flip ? -0.35f : 0.35f, 0.75f, 0.1f) * (float)(this.tempfollowerid + 1);
				}
				else
				{
					this.DoFollow();
				}
				this.rigid.useGravity = true;
				this.nodigpart = true;
				this.digging = (MainManager.player.digging || MainManager.player.startdig);
				if (this.digging)
				{
					this.transform.position = MainManager.player.transform.position + MainManager.instance.globalcamdir.forward * 0.1f;
					return;
				}
			}
			else
			{
				this.DoFollow();
			}
		}
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x0001B234 File Offset: 0x00019434
	public IEnumerator ChangeScale(Vector3 target, float frametime, bool force)
	{
		Vector3 ss = force ? this.rotater.localScale : this.startscale;
		float a = 0f;
		do
		{
			if (force)
			{
				this.rotater.localScale = Vector3.Lerp(ss, target, a / frametime);
			}
			else
			{
				this.startscale = Vector3.Lerp(ss, target, a / frametime);
			}
			a += MainManager.framestep;
			yield return null;
		}
		while (a < frametime);
		yield break;
	}

	// Token: 0x060001E9 RID: 489 RVA: 0x0001B258 File Offset: 0x00019458
	private float GetFlipSpeed()
	{
		if (this.npcdata != null && this.npcdata.startlife < 50f)
		{
			this.FlipAngle(true);
			return 1f;
		}
		return this.flipspeed;
	}

	// Token: 0x060001EA RID: 490 RVA: 0x0001B290 File Offset: 0x00019490
	private void ShieldMove(bool tempf)
	{
		this.FaceTowards(MainManager.player.transform.position);
		this.StopForceMove(this.walkstate, false);
		Vector3 b = Vector3.zero;
		if (!tempf)
		{
			b = MainManager.player.entity.spritetransform.right.normalized * (0.5f * (float)(this.animid + 1)) + MainManager.instance.globalcamdir.forward.normalized * 0.2f + new Vector3(0f, 0f, 0.1f * (float)(this.animid + 1));
		}
		else
		{
			b = MainManager.player.entity.spritetransform.right.normalized * 0.75f + MainManager.instance.globalcamdir.forward.normalized * 0.5f;
		}
		this.transform.position = Vector3.Lerp(this.transform.position, MainManager.player.transform.position + b, MainManager.framestep * 0.25f);
	}

	// Token: 0x060001EB RID: 491 RVA: 0x0001B3CC File Offset: 0x000195CC
	public void DestroyConditionIcons()
	{
		if (this.statusicons != null && this.statusicons.Length != 0)
		{
			for (int i = 0; i < this.statusicons.Length; i++)
			{
				if (this.statusicons[i] != null)
				{
					Object.Destroy(this.statusicons[i].gameObject);
				}
			}
			this.statusicons = null;
		}
	}

	// Token: 0x060001EC RID: 492 RVA: 0x0001B428 File Offset: 0x00019628
	public void UpdateConditionBubbles(bool right, MainManager.BattleData data)
	{
		float y = this.digging ? 1f : (data.cursoroffset.y + this.height);
		float x = -0.5f + data.cursoroffset.x;
		if (right)
		{
			x = 0.5f + data.cursoroffset.x;
		}
		this.DestroyConditionIcons();
		if (!this.nocondition && data.hp > 0 && data.eatenby == null && !this.iskill && !this.dead && this.battleid > -1)
		{
			this.statuscooldown = 0f;
			this.statusid = 0;
			List<Transform> list = new List<Transform>();
			if (this.isplayer && MainManager.battle.delprojs != null && MainManager.battle.delprojs.Length != 0)
			{
				int num = -1;
				for (int i = 0; i < MainManager.battle.delprojs.Length; i++)
				{
					if (MainManager.battle.partypointer[MainManager.battle.delprojs[i].position] == this.battleid && (MainManager.battle.delprojs[i].turns < num || num == -1))
					{
						num = MainManager.battle.delprojs[i].turns;
					}
				}
				if (num > -1)
				{
					SpriteRenderer spriteRenderer = new GameObject("Warning").AddComponent<SpriteRenderer>();
					spriteRenderer.sprite = MainManager.guisprites[111];
					spriteRenderer.sortingOrder = 99;
					spriteRenderer.gameObject.layer = 15;
					spriteRenderer.transform.parent = this.transform;
					spriteRenderer.transform.localPosition = new Vector3(x, y, data.cursoroffset.z - 0.1f);
					base.StartCoroutine(MainManager.SetText("|triui||color,1||center||sort,100|" + num, 2, null, false, false, new Vector3(0f, -0.3f, -0.01f), Vector3.zero, Vector2.one, spriteRenderer.transform, null));
					list.Add(spriteRenderer.transform);
				}
			}
			if (data.cantmove < 0)
			{
				SpriteRenderer spriteRenderer2 = new GameObject("Extra Turn").AddComponent<SpriteRenderer>();
				spriteRenderer2.sprite = MainManager.instance.conditionsprites[0];
				spriteRenderer2.sortingOrder = 99;
				spriteRenderer2.transform.parent = this.transform;
				spriteRenderer2.gameObject.layer = 15;
				spriteRenderer2.transform.localPosition = new Vector3(x, y, data.cursoroffset.z - 0.1f);
				base.StartCoroutine(MainManager.SetText("|triui||center||sort,100|x" + (Mathf.Abs(data.cantmove) + 1), 2, null, false, false, new Vector3(0f, -0.1f, -0.01f), Vector3.zero, Vector2.one, spriteRenderer2.transform, null));
				list.Add(spriteRenderer2.transform);
			}
			if (data.moreturnnextturn > 0)
			{
				SpriteRenderer spriteRenderer3 = new GameObject("More Turn").AddComponent<SpriteRenderer>();
				spriteRenderer3.sprite = MainManager.guisprites[214];
				spriteRenderer3.sortingOrder = 99;
				spriteRenderer3.transform.parent = this.transform;
				spriteRenderer3.gameObject.layer = 15;
				spriteRenderer3.color = Color.yellow;
				spriteRenderer3.transform.localPosition = new Vector3(x, y, data.cursoroffset.z - 0.1f);
				base.StartCoroutine(MainManager.SetText("|triui||center||sort,100|+" + data.moreturnnextturn, 2, null, false, false, new Vector3(0f, -0.1f, -0.01f), Vector3.zero, Vector2.one, spriteRenderer3.transform, null));
				list.Add(spriteRenderer3.transform);
			}
			if (data.charge > 0)
			{
				SpriteRenderer spriteRenderer4 = new GameObject("Charges").AddComponent<SpriteRenderer>();
				spriteRenderer4.sprite = MainManager.guisprites[150];
				spriteRenderer4.sortingOrder = 99;
				spriteRenderer4.transform.parent = this.transform;
				spriteRenderer4.gameObject.layer = 15;
				spriteRenderer4.transform.localPosition = new Vector3(x, y, data.cursoroffset.z - 0.1f);
				base.StartCoroutine(MainManager.SetText("|triui||center||sort,100||color,4|+" + data.charge, 2, null, false, false, new Vector3(0f, -0.1f, -0.01f), Vector3.zero, Vector2.one, spriteRenderer4.transform, null));
				list.Add(spriteRenderer4.transform);
			}
			for (int j = 0; j < data.condition.Count; j++)
			{
				int[] array = data.condition[j];
				if (array[0] != 16 && array[0] != 10 && array[0] != 12 && array[0] != 15 && (!this.isplayer || array[0] != 1 || !MainManager.BadgeIsEquipped(27, data.trueid)))
				{
					SpriteRenderer spriteRenderer5 = new GameObject("Status" + j).AddComponent<SpriteRenderer>();
					spriteRenderer5.sprite = MainManager.instance.conditionsprites[array[0] + 1];
					spriteRenderer5.gameObject.layer = 15;
					spriteRenderer5.sortingOrder = 99;
					spriteRenderer5.transform.parent = this.transform;
					spriteRenderer5.transform.localPosition = new Vector3(x, y, data.cursoroffset.z - 0.1f);
					string text = (array[0] == 3) ? "" : "|color,4|";
					if (array[0] != 11 && array[0] != 8 && array[0] != 9 && (array[0] != 20 || MainManager.BadgeHowManyEquipped(61, data.trueid) > 1))
					{
						base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
						{
							text,
							"|triui||center||sort,100|",
							(array[0] == 17) ? "|dropshadow,0.05,-0.05|" : "",
							(array[0] == 20) ? "x" : "",
							(array[1] <= 99) ? string.Concat(array[1]) : "∞"
						}) ?? "", 2, null, false, false, new Vector3(0f, -0.2f, -0.01f), Vector3.zero, Vector2.one * 0.9f, spriteRenderer5.transform, null));
					}
					if (array[0] == 3 && !this.isplayer && MainManager.BadgeIsEquipped(81))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 81], spriteRenderer5.transform.localPosition));
					}
					list.Add(spriteRenderer5.transform);
				}
			}
			if (this.isplayer)
			{
				Vector3 pos = new Vector3(x, y, data.cursoroffset.z - 0.1f);
				if (data.plating)
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 77], pos));
				}
				if (MainManager.HasCondition(MainManager.BattleCondition.Poison, data) > -1)
				{
					if (MainManager.BadgeIsEquipped(6, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 6], pos));
					}
					if (MainManager.BadgeIsEquipped(9, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 9], pos));
					}
					if (MainManager.BadgeIsEquipped(44, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 44], pos));
					}
					if (MainManager.BadgeIsEquipped(26, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 26], pos));
					}
				}
				if (MainManager.HasCondition(MainManager.BattleCondition.Numb, data) > -1 && MainManager.BadgeIsEquipped(34, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 34], pos));
				}
				if (MainManager.HasCondition(MainManager.BattleCondition.Sleep, data) > -1 && MainManager.BadgeIsEquipped(47, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 47], pos));
				}
				if (MainManager.HasCondition(MainManager.BattleCondition.Freeze, data) > -1 && MainManager.BadgeIsEquipped(46, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 46], pos));
				}
				if (MainManager.HasCondition(MainManager.BattleCondition.Poison, data) > -1 && MainManager.BadgeIsEquipped(27, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 27], pos));
				}
				if (data.hp <= 4)
				{
					if (MainManager.BadgeIsEquipped(3, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 3], pos));
					}
					if (MainManager.BadgeIsEquipped(4, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 4], pos));
					}
					if (MainManager.BadgeIsEquipped(87, data.trueid))
					{
						list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 87], pos));
					}
				}
				if (MainManager.BadgeIsEquipped(82, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 82], pos));
				}
				if (MainManager.BadgeIsEquipped(54, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 54], pos));
				}
				if (MainManager.BadgeIsEquipped(75, data.trueid))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 75], pos));
				}
				if (MainManager.BadgeIsEquipped(36) && MainManager.IsPlayerInPos(MainManager.instance.playerdata.Length - 1, this.transform))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 36], pos));
				}
				if (MainManager.BadgeIsEquipped(35) && MainManager.IsPlayerInPos(0, this.transform))
				{
					list.Add(this.NewConditionIcon(MainManager.itemsprites[1, 35], pos));
				}
			}
			this.statusicons = list.ToArray();
			for (int k = 1; k < this.statusicons.Length; k++)
			{
				this.statusicons[k].gameObject.SetActive(false);
			}
			this.statusid = 0;
			this.statuscooldown = 60f;
		}
		if (this.isplayer && data.hp <= 0 && !MainManager.battle.lockmmatter && MainManager.BadgeIsEquipped(68, data.trueid))
		{
			SpriteRenderer spriteRenderer6 = new GameObject("miracle").AddComponent<SpriteRenderer>();
			spriteRenderer6.sprite = MainManager.itemsprites[1, 68];
			spriteRenderer6.sortingOrder = 99;
			spriteRenderer6.transform.parent = this.transform;
			spriteRenderer6.gameObject.layer = 15;
			spriteRenderer6.transform.localPosition = new Vector3(x, y, data.cursoroffset.z - 0.1f);
			base.StartCoroutine(MainManager.SetText("|triui||center||sort,100||color,4||dropshadow,0.05,-0.05|" + (Mathf.Clamp(2 - (MainManager.BadgeHowManyEquipped(68, data.trueid) - 1), 1, 3) - data.turnssincedeath), 2, null, false, false, new Vector3(0f, -0.1f, -0.01f), Vector3.zero, Vector2.one, spriteRenderer6.transform, null));
			this.statusicons = new Transform[]
			{
				spriteRenderer6.transform
			};
		}
	}

	// Token: 0x060001ED RID: 493 RVA: 0x0001C008 File Offset: 0x0001A208
	private Transform NewConditionIcon(Sprite csprite, Vector3 pos)
	{
		SpriteRenderer spriteRenderer = new GameObject("StatusIcon").AddComponent<SpriteRenderer>();
		spriteRenderer.sprite = csprite;
		spriteRenderer.gameObject.layer = 15;
		spriteRenderer.sortingOrder = 99;
		spriteRenderer.transform.parent = this.transform;
		spriteRenderer.transform.localPosition = pos;
		return spriteRenderer.transform;
	}

	// Token: 0x060001EE RID: 494 RVA: 0x0001C064 File Offset: 0x0001A264
	public void UpdateItem()
	{
		if (this.sprite != null)
		{
			if (this.animid == 3)
			{
				this.sprite.enabled = false;
			}
			else if (this.animid < 2)
			{
				this.sprite.sprite = MainManager.itemsprites[0, this.itemstate];
			}
			else if (MainManager.instance.flags[681] && !this.overridemovesmoke && this.itemstate != 59)
			{
				this.sprite.sprite = MainManager.guisprites[190];
			}
			else
			{
				this.sprite.sprite = MainManager.itemsprites[1, this.itemstate];
			}
			if (this.sprite.enabled && this.sprite.sprite != null)
			{
				this.spritetransform.localPosition = new Vector2(0f, this.sprite.sprite.bounds.extents.y);
			}
		}
	}

	// Token: 0x060001EF RID: 495 RVA: 0x0001C16F File Offset: 0x0001A36F
	public void LockRigid(bool value)
	{
		this.LockRigid(value, true);
	}

	// Token: 0x060001F0 RID: 496 RVA: 0x0001C179 File Offset: 0x0001A379
	public void LockRigid(bool value, bool resetvelocity)
	{
		if (this.rigid != null)
		{
			if (resetvelocity)
			{
				this.rigid.velocity = Vector3.zero;
			}
			this.rigid.useGravity = !value;
			this.rigid.isKinematic = value;
		}
	}

	// Token: 0x060001F1 RID: 497 RVA: 0x0001C1B8 File Offset: 0x0001A3B8
	private bool CloseMove()
	{
		return MainManager.player != null && !MainManager.player.dashing && ((MainManager.instance.flags[401] && MainManager.map.closemove && this.mainparty) || (this.mainparty && MainManager.player.transform.parent != null) || MainManager.player.forceclosemove);
	}

	// Token: 0x060001F2 RID: 498 RVA: 0x0001C234 File Offset: 0x0001A434
	private void DoFollow()
	{
		if (!MainManager.instance.overridefollower && !this.dead && !this.iskill && !this.overridefollow)
		{
			if (!this.springcooldown && this.jumpcooldown <= 15f)
			{
				this.rigid.velocity = new Vector3(this.rigid.velocity.x, Mathf.Clamp(this.rigid.velocity.y, -20f, this.jumpheight), this.rigid.velocity.z);
			}
			this.FaceTowards(this.following.transform.position);
			if (Time.frameCount % 2 == 0)
			{
				return;
			}
			if (!this.usebuffer)
			{
				float sqrDistance = MainManager.GetSqrDistance(this.transform.position, this.following.transform.position, true);
				bool flag = this.CloseMove();
				float num = flag ? 0.5f : 1f;
				if (sqrDistance > this.followlimit || (!MainManager.instance.minipause && MainManager.GetDistance(this.transform.position.y, this.following.transform.position.y) > MainManager.map.followerylimit))
				{
					this.transform.position = this.following.transform.position + MainManager.instance.globalcamdir.forward.normalized * 0.1f;
					this.rigid.velocity = Vector3.zero;
				}
				if (sqrDistance > this.followdistance * num)
				{
					this.MoveTowards(this.following.transform.position + MainManager.MainCamera.transform.forward.normalized * this.followoffset * num, flag ? 1.25f : ((sqrDistance - 1f) / 2f), this.walkstate, this.basestate, true);
					if (this.detect != null)
					{
						this.DetectDirection(this.following.transform.position);
						if ((!Physics.Raycast(this.transform.position + this.detect.transform.forward.normalized + Vector3.up * 0.3f, Vector3.down, 2.5f, 8448) && this.onground) || (MainManager.GetDistance(this.transform.position.y, this.following.transform.position.y) > 0.5f && MainManager.GetSqrDistance(this.transform.position, this.following.transform.position, true) > this.followjump && this.onground && this.hitwall))
						{
							this.Jump();
							return;
						}
					}
				}
				else
				{
					this.StopForceMove(this.basestate, true);
					if (this.animstate != 1)
					{
						this.deltavelocity = Vector2.zero;
					}
				}
			}
		}
	}

	// Token: 0x060001F3 RID: 499 RVA: 0x0001C564 File Offset: 0x0001A764
	public void PlayAnimSpecific(int index)
	{
		if (this.animspecific[index] != null && this.animspecific[0].GetComponent<ParticleSystem>() != null)
		{
			if (this.flip)
			{
				this.animspecific[0].transform.localEulerAngles = new Vector3(90f, 0f);
			}
			else
			{
				this.animspecific[0].transform.localEulerAngles = new Vector3(-90f, 180f);
			}
			this.animspecific[0].GetComponent<ParticleSystem>().Play();
		}
	}

	// Token: 0x060001F4 RID: 500 RVA: 0x0001C5F4 File Offset: 0x0001A7F4
	public void SetAnim(string args)
	{
		this.SetAnim(args, false);
	}

	// Token: 0x060001F5 RID: 501 RVA: 0x0001C5FE File Offset: 0x0001A7FE
	private void OnEnable()
	{
		this.SetAnim("", true);
		if (this.emoticon != null)
		{
			this.emoticon.Play("-1");
			this.emoticoncooldown = 0f;
		}
	}

	// Token: 0x060001F6 RID: 502 RVA: 0x0001C638 File Offset: 0x0001A838
	public void SetAnim(string args, bool force)
	{
		if (this.anim != null)
		{
			if (this.inice && this.hasiceanim)
			{
				args += "i";
			}
			if (force)
			{
				args = args.Replace("u", "");
			}
			string text = this.animstate + args;
			int num = this.animstate;
			if (num == 30)
			{
				num = 11;
				if (this.flyinganim && !args.Contains("f"))
				{
					args.Insert(0, "f");
				}
			}
			if (force || text != this.laststate || (MainManager.player != null && MainManager.player.entity == this) || base.CompareTag("PFollower"))
			{
				this.laststate = text;
				if (this.animstate < 100)
				{
					this.anim.CrossFadeInFixedTime((MainManager.Animations)num + args, this.animspeed);
				}
				else
				{
					this.anim.CrossFadeInFixedTime(text, this.animspeed);
				}
				this.UpdateAnimSpecific();
			}
		}
	}

	// Token: 0x060001F7 RID: 503 RVA: 0x0001C750 File Offset: 0x0001A950
	public IEnumerator Drop()
	{
		if (this.dead || this.iskill || this.deathcoroutine != null || this.battleid > MainManager.battle.enemydata.Length - 1)
		{
			this.droproutine = null;
			yield break;
		}
		bool tover = this.overrideanim;
		this.overrideanim = false;
		do
		{
			this.animstate = 11;
			yield return null;
		}
		while (!MainManager.battle.startdrop);
		if (this.battleid < MainManager.battle.enemydata.Length)
		{
			if (MainManager.battle.enemydata[this.battleid].hp <= 0 && !MainManager.battle.inevent)
			{
				this.droproutine = null;
				yield break;
			}
			if (MainManager.HasCondition(MainManager.BattleCondition.Freeze, MainManager.battle.enemydata[this.battleid]) > -1)
			{
				this.tempheightoverride = true;
			}
		}
		if (this.originalid + 1 == 122)
		{
			for (int i = 0; i < 2; i++)
			{
				MainManager.PlayParticle("leafexplode", null, this.extras[i].transform.position, new Vector3((float)((i == 0) ? -140 : -50), 0f), 1.5f).transform.localScale = Vector3.one * 2.25f;
				this.extras[i].gameObject.SetActive(false);
			}
			BattleControl.SetDefaultCamera(true);
		}
		if (MainManager.SoundIsPlaying("Fall") == -1)
		{
			MainManager.PlaySound("Fall");
		}
		this.rigid.velocity = Vector3.zero;
		this.overrideanim = true;
		if (this.originalid + 1 == 43)
		{
			if (this.line != null)
			{
				Object.Destroy(this.line.gameObject);
			}
			this.basestate = 0;
		}
		float a = 0.1f;
		this.bobspeed = 0f;
		this.bobrange = 0f;
		float cd = 300f;
		while (this.spritetransform.localPosition.y > (this.tempheightoverride ? 0f : this.minheight) && !this.nofallfrozen && cd > 0f)
		{
			this.height -= MainManager.framestep * a;
			a *= 1.1f;
			cd -= MainManager.framestep;
			yield return null;
		}
		if (this.shakeondrop)
		{
			MainManager.PlayParticle("impactsmoke", this.transform.position);
			MainManager.ShakeScreen(Vector3.one * 0.5f, 0.45f);
			MainManager.PlaySound("Thud3");
			yield return new WaitForSeconds(0.25f);
		}
		if (this.battleid < MainManager.battle.enemydata.Length)
		{
			MainManager.battle.enemydata[this.battleid].position = BattleControl.BattlePosition.Ground;
		}
		yield return new WaitForSeconds(0.4f);
		if (this.height > 0.1f)
		{
			this.bobspeed = this.startbs;
			this.bobrange = this.startbf;
		}
		this.overrideanim = tover;
		if (this.battleid < MainManager.battle.enemydata.Length && MainManager.HasCondition(MainManager.BattleCondition.Topple, MainManager.battle.enemydata[this.battleid]) > -1)
		{
			MainManager.RemoveCondition(MainManager.BattleCondition.Topple, MainManager.battle.enemydata[this.battleid]);
			this.basestate = 0;
		}
		if (this.battleid < MainManager.battle.enemydata.Length && this.originalid + 1 == 208 && MainManager.battle.enemydata[this.battleid].charge > 0)
		{
			this.animstate = 101;
		}
		else
		{
			this.animstate = this.basestate;
		}
		this.droproutine = null;
		yield break;
	}

	// Token: 0x060001F8 RID: 504 RVA: 0x0001C75F File Offset: 0x0001A95F
	public void SetState(int state)
	{
		this.animstate = state;
	}

	// Token: 0x060001F9 RID: 505 RVA: 0x0001C768 File Offset: 0x0001A968
	public void PlaySound(AudioClip clip, float volume, float pitch)
	{
		if (MainManager.SoundVolume() && this.sound != null && this.campos.z < 25f && MainManager.instance.globalcooldown <= 0f && MainManager.InCameraRange(this.campos) && ((MainManager.player.beemerang != null && MainManager.player.beemerang.transform == this.transform) || this.npcdata == null || this.npcdata.startlife >= 15f))
		{
			this.sound.clip = clip;
			this.soundvolume = volume;
			this.sound.pitch = pitch;
			this.sound.Play();
			this.UpdateSound();
		}
	}

	// Token: 0x060001FA RID: 506 RVA: 0x0001C844 File Offset: 0x0001AA44
	public void PlaySound(string clipname, float volume, float pitch)
	{
		this.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + clipname), volume, pitch);
	}

	// Token: 0x060001FB RID: 507 RVA: 0x0001C85E File Offset: 0x0001AA5E
	public void PlaySound(string clipname, float volume)
	{
		this.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + clipname), volume, 1f);
	}

	// Token: 0x060001FC RID: 508 RVA: 0x0001C87C File Offset: 0x0001AA7C
	public void PlaySound(string clipname)
	{
		this.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + clipname), 1f, 1f);
	}

	// Token: 0x060001FD RID: 509 RVA: 0x0001C87C File Offset: 0x0001AA7C
	public void PlaySoundSimple(string clipname)
	{
		this.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/" + clipname), 1f, 1f);
	}

	// Token: 0x060001FE RID: 510 RVA: 0x0001C8A0 File Offset: 0x0001AAA0
	private void RefreshTrail()
	{
		if (this.trail)
		{
			if (this.traildata.trails == null)
			{
				this.traildata.trails = new SpriteRenderer[5];
				this.traildata.time = new float[this.traildata.trails.Length];
				this.traildata.pos = new Vector3[this.traildata.trails.Length];
				for (int i = 0; i < this.traildata.trails.Length; i++)
				{
					this.traildata.trails[i] = new GameObject("trail" + i).AddComponent<SpriteRenderer>();
					this.traildata.trails[i].transform.parent = this.transform;
					this.traildata.trails[i].color = new Color(1f, 1f, 1f, 0.5f);
				}
			}
			if (this.traildata.delay <= 0f)
			{
				this.traildata.trails[this.traildata.id].enabled = true;
				this.traildata.trails[this.traildata.id].sprite = this.sprite.sprite;
				this.traildata.trails[this.traildata.id].transform.eulerAngles = this.spritetransform.eulerAngles;
				this.traildata.pos[this.traildata.id] = this.spritetransform.position + MainManager.instance.globalcamdir.forward.normalized * 0.1f;
				this.traildata.time[this.traildata.id] = 20f;
				this.traildata.id = this.traildata.id + 1;
				if (this.traildata.id >= this.traildata.trails.Length)
				{
					this.traildata.id = 0;
				}
				this.traildata.delay = 5f;
			}
			else
			{
				this.traildata.delay = this.traildata.delay - MainManager.framestep;
			}
		}
		if (this.traildata.trails != null)
		{
			for (int j = 0; j < this.traildata.trails.Length; j++)
			{
				if (this.traildata.trails[j].enabled)
				{
					this.traildata.trails[j].transform.position = this.traildata.pos[j];
					this.traildata.time[j] -= MainManager.framestep;
					if (this.traildata.time[j] <= 0f)
					{
						this.traildata.trails[j].enabled = false;
					}
				}
			}
		}
	}

	// Token: 0x060001FF RID: 511 RVA: 0x0001CB94 File Offset: 0x0001AD94
	private void Numb()
	{
		bool flag = false;
		if (Time.frameCount % 60 == 0 && !base.CompareTag("Player") && MainManager.enemydata.Length - 1 <= this.battleid)
		{
			flag = (MainManager.HasCondition(MainManager.BattleCondition.Flipped, MainManager.battle.enemydata[this.battleid]) > -1);
		}
		this.animstate = (flag ? 16 : 11);
		if (Random.Range(0, 100) < 5)
		{
			this.spritetransform.localPosition += MainManager.RandomVector(0.1f, 0.05f, 0f);
			if (!this.cotunknown)
			{
				this.sprite.material.color = Color.yellow;
				return;
			}
		}
		else
		{
			this.spritetransform.localPosition = Vector3.zero;
		}
	}

	// Token: 0x06000200 RID: 512 RVA: 0x0001CC60 File Offset: 0x0001AE60
	private void ReturnFromAction()
	{
		if (!this.shadow.gameObject.activeSelf)
		{
			this.shadow.gameObject.SetActive(true);
		}
		this.rigid.isKinematic = false;
		this.ccol.enabled = true;
		this.spritetransform.localScale = Vector3.Lerp(this.spritetransform.localScale, Vector3.one, 0.1f);
		if (MainManager.player.switchcooldown <= 0f)
		{
			this.spin = Vector3.zero;
		}
		this.overrridejump = false;
		this.overrideanim = false;
		this.rigid.useGravity = true;
		this.leiffly = false;
	}

	// Token: 0x06000201 RID: 513 RVA: 0x0001CD0A File Offset: 0x0001AF0A
	public IEnumerator StopDig()
	{
		while (this.spritetransform.localScale.magnitude < 0.9f)
		{
			this.ReturnFromAction();
			yield return null;
		}
		yield break;
	}

	// Token: 0x06000202 RID: 514 RVA: 0x0001CD19 File Offset: 0x0001AF19
	public void ForceAnimator()
	{
		this.anim = base.GetComponent<Animator>();
		if (this.anim == null)
		{
			this.anim = base.gameObject.AddComponent<Animator>();
		}
		this.SetAnimator();
		this.UpdateAnimSpecific();
	}

	// Token: 0x06000203 RID: 515 RVA: 0x0001CD54 File Offset: 0x0001AF54
	public void SetAnimator()
	{
		if (MainManager.instance.flags[616] && this.originalid == 132)
		{
			this.anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimationControllers/Beetle/Beetle");
			this.animid = 1;
			return;
		}
		if (this.playerentity && MainManager.instance.flags[614] && MainManager.BadgeIsEquipped(11))
		{
			this.anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimationControllers/Pillbug/Pillbug");
			return;
		}
		this.anim.runtimeAnimatorController = ((MainManager.instance.flags[616] && this.model == null) ? Resources.Load<RuntimeAnimatorController>("AnimationControllers/" + ((this.originalid == 403) ? "Cerise/Cerise" : ((this.originalid == 290) ? "TangySeller/TangySeller" : "Tanjerin/Tanjerin"))) : Resources.Load<RuntimeAnimatorController>(string.Concat(new object[]
		{
			"AnimationControllers/",
			this.animid + MainManager.AnimIDs.Bee,
			"/",
			this.animid + MainManager.AnimIDs.Bee
		})));
		if (MainManager.instance.flags[616] && this.model == null)
		{
			this.overridefly = true;
			this.overrridejump = true;
		}
	}

	// Token: 0x06000204 RID: 516 RVA: 0x0001CEB0 File Offset: 0x0001B0B0
	public void RefreshCondition()
	{
		if (this.isplayer)
		{
			MainManager.instance.playerdata[this.battleid].isasleep = (MainManager.HasCondition(MainManager.BattleCondition.Sleep, MainManager.instance.playerdata[this.battleid]) > -1);
			MainManager.instance.playerdata[this.battleid].isnumb = (MainManager.HasCondition(MainManager.BattleCondition.Numb, MainManager.instance.playerdata[this.battleid]) > -1);
			return;
		}
		MainManager.battle.enemydata[this.battleid].isasleep = (MainManager.HasCondition(MainManager.BattleCondition.Sleep, MainManager.battle.enemydata[this.battleid]) > -1);
		MainManager.battle.enemydata[this.battleid].isnumb = (MainManager.HasCondition(MainManager.BattleCondition.Numb, MainManager.battle.enemydata[this.battleid]) > -1);
	}

	// Token: 0x06000205 RID: 517 RVA: 0x0001CFA8 File Offset: 0x0001B1A8
	private void LateUpdate()
	{
		if (Time.frameCount % 3 == 0)
		{
			if (MainManager.instance.pause && this.pausepos != null && !this.activeonpause && !MainManager.instance.inevent && !this.iskill && !this.dead)
			{
				this.transform.position = this.pausepos.Value;
			}
			this.UpdateCamPos();
		}
		if (this.latetrans != null)
		{
			this.latetrans.position = this.latepos;
		}
		if (this.forcemove && !MainManager.instance.pause)
		{
			if (((this.alwaysactive || (this.incamera && this.campos.z < 30f)) && !this.disabletimer) || this.playerentity)
			{
				this.forcetimer -= MainManager.framestep;
			}
			if (this.forcetimer <= 0f && !this.digging && (!MainManager.FreePlayer() || (!base.CompareTag("Follower") && !base.CompareTag("PFollower"))))
			{
				if (!this.shrink && this.transform.localScale.magnitude > 0.2f)
				{
					MainManager.DeathSmoke(this.transform.position);
				}
				this.transform.position = this.forcetarget;
				if (!this.shrink && this.transform.localScale.magnitude > 0.2f)
				{
					MainManager.DeathSmoke(this.forcetarget);
				}
			}
		}
		if (!this.dead && !this.iskill && this.battle && MainManager.battle != null && !MainManager.battle.cancelupdate && (this.isplayer || (this.battleid > -1 && MainManager.battle.enemydata.Length > this.battleid)))
		{
			if (this.isplayer)
			{
				if (MainManager.instance.playerdata[this.battleid].isnumb)
				{
					this.Numb();
				}
			}
			else if (this.battleid < MainManager.battle.enemydata.Length && MainManager.battle.enemydata[this.battleid].isnumb)
			{
				this.Numb();
			}
		}
		if (this.bubbleshield != null)
		{
			this.bubbleshield.shrink = !this.shieldenabled;
			this.bubbleshield.transform.localPosition = ((this.bubbleshield.transform.localScale.magnitude > 0.15f) ? (((this.overrideshieldpos != null) ? this.overrideshieldpos.Value : new Vector3(0f, 1.25f)) + new Vector3(0f, this.height)) : new Vector3(0f, -999f));
		}
		if (this.hpbar != null && this.hpbar.gameObject.activeInHierarchy)
		{
			MainManager.AnimIDs animIDs = this.originalid + MainManager.AnimIDs.Bee;
			if (animIDs != MainManager.AnimIDs.KeyR)
			{
				if (animIDs == MainManager.AnimIDs.Pitcher)
				{
					this.hpbar.transform.localPosition = new Vector3(-0.5f, -0.5f + MainManager.instance.camoffset.y - MainManager.battlecampos.y);
				}
				else
				{
					this.hpbar.transform.localPosition = new Vector3(0f, -0.5f + MainManager.instance.camoffset.y - MainManager.battlecampos.y);
				}
			}
			else
			{
				this.hpbar.transform.localPosition = new Vector3(0.5f, -0.5f + MainManager.instance.camoffset.y - MainManager.battlecampos.y);
			}
		}
		if (this.sprite != null && this.animid > -1)
		{
			this.RefreshTrail();
		}
		if (this.tempfollower && this.transform.parent == null)
		{
			this.transform.parent = MainManager.map.transform;
		}
		if (!this.setup)
		{
			this.LateStart();
		}
		if (!MainManager.instance.pause)
		{
			this.Follow();
		}
		if (this.alwaysflip)
		{
			this.UpdateFlip();
		}
		if (!MainManager.instance.pause && this.incamera && (MainManager.instance.inevent || ((this.npcdata == null || this.npcdata.startlife < 50f || this.campos.z < 25f) && this.campos.z < 30f)))
		{
			this.truescale = this.startscale;
			if (this.icecube != null)
			{
				this.icecube.transform.localScale = Vector3.Lerp(this.icecube.transform.localScale, this.freezesize, MainManager.framestep * 0.15f);
				if (!this.shakeice)
				{
					this.icecube.transform.localPosition = this.freezeoffset + Vector3.up * this.height;
				}
				else
				{
					this.icecube.transform.localPosition = new Vector3(this.freezeoffset.x + Random.Range(-0.1f, 0.1f), this.freezeoffset.y + this.height, this.freezeoffset.z + Random.Range(-0.1f, 0.1f));
				}
			}
			if (this.springcooldown && (this.rigid.velocity.y < 0f || this.onground))
			{
				this.springcooldown = false;
			}
			if (!MainManager.instance.pause)
			{
				if (this.battle || this.npcdata == null || this.npcdata.entitytype != NPCControl.NPCType.NPC || Time.frameCount % 2 == 0)
				{
					this.UpdateGround();
					if (Time.frameCount % 2 == 0)
					{
						this.UpdateGeneralAnim();
					}
					this.UpdateAirAnim();
					this.UpdateVelocity();
				}
				if (MainManager.instance.inevent || this.battle || Time.frameCount % 2 == 0)
				{
					this.UpdateEmoticon();
				}
				if (Time.frameCount % 3 == 0)
				{
					this.UpdateMoveSmoke();
				}
				if (!this.mapentity || (this.isfollower && MainManager.player != null && MainManager.player.flying) || this.npcdata == null || ((this.npcdata.entitytype == NPCControl.NPCType.Enemy || (this.npcdata.entitytype == NPCControl.NPCType.SemiNPC && Time.frameCount % 10 == 0) || (this.npcdata.entitytype != NPCControl.NPCType.SemiNPC && Time.frameCount % 2 != 0)) && this.npcdata.insideid == MainManager.instance.insideid))
				{
					this.RefreshShadow();
				}
				this.UpdateStatusIcons();
				this.UpdateSound();
				if (MainManager.battle == null || this.battle)
				{
					this.UpdateFlip();
				}
			}
		}
		else
		{
			if (this.soundonpause || (!this.battle && this.sound != null && (this.npcdata == null || this.npcdata.entitytype != NPCControl.NPCType.Object || this.npcdata.objecttype != NPCControl.ObjectTypes.MusicRange)))
			{
				this.sound.volume = 0f;
			}
			else
			{
				this.UpdateSound();
			}
			if (this.emoticon != null)
			{
				this.emoticoncooldown = 0f;
				this.emoticon.Play("-1");
			}
			if (this.movesmoke != null && this.movesmoke.gameObject.activeInHierarchy)
			{
				this.movesmoke.gameObject.SetActive(false);
			}
		}
		if (!MainManager.instance.pause)
		{
			if (Time.frameCount % 3 == 0)
			{
				if (!MainManager.instance.inevent)
				{
					this.UpdateCollider();
				}
				this.UpdateRotater();
			}
			if (this.incamera || this.battle || MainManager.instance.inevent)
			{
				if (this.battle || this.npcdata == null || this.npcdata.entitytype != NPCControl.NPCType.NPC || this.npcdata.startlife < 50f || MainManager.instance.inevent || this.alwaysactive)
				{
					this.UpdateHeight();
				}
				this.AnimSpecificQuirks();
				this.UpdateSprite();
				this.flyinganim = (!this.overridefly && this.height > 0.1f);
			}
			if (!MainManager.instance.inevent)
			{
				if (!this.onground && this.offgroundframes < 1000f)
				{
					this.offgroundframes += MainManager.framestep;
				}
				else
				{
					this.offgroundframes = 0f;
				}
			}
			else if (this.onground && this.offgroundframes > 0f)
			{
				this.offgroundframes -= MainManager.framestep;
			}
			if (this.jumpcooldown <= 0f && this.stopspinonground)
			{
				this.spin = Vector3.zero;
				this.stopspinonground = false;
			}
		}
		this.lastpos = this.transform.position;
		this.oldground = this.onground;
	}

	// Token: 0x06000206 RID: 518 RVA: 0x0001D918 File Offset: 0x0001BB18
	private void UpdateFlip()
	{
		if (!this.overrideflip && this.sprite != null)
		{
			if (this.digging && !this.iskill && !this.dead && this.deathcoroutine == null)
			{
				if (!this.diganim)
				{
					this.spritetransform.localScale = Vector3.Lerp(this.startscale, Vector3.zero, this.digtime / 30f);
					this.spritetransform.eulerAngles += new Vector3(0f, 15f);
					if (!this.nodigpart)
					{
						if (this.digtime < 30f)
						{
							if (this.digpart[0] == null && !this.instdig)
							{
								this.digpart[0] = (Object.Instantiate(Resources.Load("Prefabs/Particles/DirtFlying"), this.transform.position, Quaternion.Euler(new Vector3(-90f, 0f))) as GameObject);
								this.digpart[0].transform.parent = this.transform;
								if (this.npcdata != null && this.npcdata.entitytype == NPCControl.NPCType.Enemy)
								{
									this.digpart[0].GetComponent<Renderer>().material.renderQueue = 3000;
								}
							}
							this.digpart[0].transform.localPosition = Vector3.zero;
							if (this.digpart[1] != null)
							{
								this.digpart[1].transform.position = new Vector3(0f, -9999f);
							}
							this.digtime += MainManager.framestep;
						}
						else
						{
							if (this.digpart[0] != null)
							{
								this.digpart[0].transform.position = new Vector3(0f, -9999f);
							}
							if (this.digpart[1] == null)
							{
								this.digpart[1] = (Object.Instantiate(Resources.Load("Prefabs/Particles/Digging"), this.transform.position, Quaternion.Euler(new Vector3(-90f, 0f))) as GameObject);
								this.digpart[1].transform.parent = this.transform;
								this.digpart[1].transform.localScale = this.digscale;
								if (this.npcdata != null && this.npcdata.entitytype == NPCControl.NPCType.Enemy)
								{
									this.digpart[1].GetComponent<Renderer>().material.renderQueue = 3000;
								}
							}
							this.digpart[1].transform.localPosition = Vector3.zero;
						}
					}
					else if (this.digtime < 30f)
					{
						this.digtime += MainManager.framestep;
					}
				}
				this.instdig = false;
				return;
			}
			this.digtime = 0f;
			if (this.digpart[0] != null)
			{
				this.digpart[0].transform.position = new Vector3(0f, -9999f);
			}
			if (this.digpart[1] != null)
			{
				this.digpart[1].transform.position = new Vector3(0f, -9999f);
			}
			if (this.spin != Vector3.zero)
			{
				this.spritetransform.Rotate(this.spin);
				return;
			}
			if (!this.overrideonlyflip)
			{
				if (this.flip)
				{
					if (!this.isplayer || (MainManager.player != null && !MainManager.player.dashing))
					{
						this.spritetransform.localEulerAngles = new Vector3(this.spritetransform.localEulerAngles.x, Mathf.LerpAngle(this.spritetransform.localEulerAngles.y, 180f, this.GetFlipSpeed()), this.spritetransform.localEulerAngles.z);
					}
					this.spritetransform.localScale = new Vector3(this.truescale.x, this.truescale.y, -this.truescale.z);
					return;
				}
				if (!this.isplayer || (MainManager.player != null && !MainManager.player.dashing))
				{
					this.spritetransform.localEulerAngles = new Vector3(this.spritetransform.localEulerAngles.x, Mathf.LerpAngle(this.spritetransform.localEulerAngles.y, 0f, this.GetFlipSpeed()), this.spritetransform.localEulerAngles.z);
				}
				this.spritetransform.localScale = new Vector3(this.truescale.x, this.truescale.y, this.truescale.z);
			}
		}
	}

	// Token: 0x06000207 RID: 519 RVA: 0x0001DDFC File Offset: 0x0001BFFC
	private void UpdateStatusIcons()
	{
		if (this.statusicons != null && this.statusicons.Length != 0)
		{
			if (MainManager.battle != null && ((MainManager.battle.action && MainManager.battle.chompyattack == null) || MainManager.battle.enemy || MainManager.battle.inevent))
			{
				for (int i = 0; i < this.statusicons.Length; i++)
				{
					if (this.statusicons[i] != null && this.statusicons[i].gameObject.activeSelf)
					{
						this.statusicons[i].gameObject.SetActive(false);
					}
				}
				this.statuscooldown = 0f;
				return;
			}
			if (this.statuscooldown > 0f)
			{
				this.statuscooldown -= MainManager.framestep;
				return;
			}
			this.statusid++;
			if (this.statusid >= this.statusicons.Length)
			{
				this.statusid = 0;
			}
			for (int j = 0; j < this.statusicons.Length; j++)
			{
				bool flag = j == this.statusid;
				if (this.statusicons[j] != null && this.statusicons[j].gameObject.activeSelf != flag)
				{
					this.statusicons[j].gameObject.SetActive(flag);
				}
			}
			this.statuscooldown = 60f;
		}
	}

	// Token: 0x06000208 RID: 520 RVA: 0x0001DF5C File Offset: 0x0001C15C
	private void UpdateCamPos()
	{
		if (this.alwaysactive)
		{
			this.incamera = true;
		}
		else
		{
			this.incamera = (MainManager.InCameraRange(this.campos) && this.campos.z < 40f);
		}
		this.camdistance = MainManager.GetDistance(this.transform.position, MainManager.MainCamera.transform.position);
		this.campos = MainManager.MainCamera.WorldToViewportPoint(this.transform.position);
	}

	// Token: 0x06000209 RID: 521 RVA: 0x0001DFE4 File Offset: 0x0001C1E4
	private void UpdateSound()
	{
		if (this.sound != null && (this.npcdata == null || this.npcdata.objecttype != NPCControl.ObjectTypes.MusicRange))
		{
			if (this.sound.isPlaying)
			{
				this.sound.panStereo = Mathf.Clamp(Mathf.Lerp(-1f, 1f, this.campos.x), (float)((this.transform.position.x > MainManager.MainCamera.transform.position.x) ? 0 : -1), (float)((this.transform.position.x < MainManager.MainCamera.transform.position.x) ? 0 : 1));
			}
			this.sound.volume = this.GetSoundDistance();
			if (MainManager.pausemenu == null)
			{
				this.sound.volume = this.soundvolume * MainManager.soundvolume;
				return;
			}
			if (MainManager.pausemenu.windowid == 4 || MainManager.pausemenu.windowid == 5)
			{
				this.sound.volume = this.soundvolume * MainManager.pausemenu.svolume;
				return;
			}
			this.sound.volume = this.soundvolume * MainManager.soundvolume;
		}
	}

	// Token: 0x0600020A RID: 522 RVA: 0x0001E138 File Offset: 0x0001C338
	private void UpdateSprite()
	{
		if (this.animid > -1 && ((this.hasiceanim && this.lastice != this.inice) || this.oldid != this.animid || this.oldstate != this.animstate || this.oldback != this.backsprite || this.oldtalk != this.talking || this.oldfly != this.flyinganim))
		{
			if (this.item)
			{
				if (this.animid < 3)
				{
					this.UpdateItem();
				}
				else if (this.sprite.enabled)
				{
					this.sprite.enabled = false;
				}
			}
			else
			{
				if (this.oldid != this.animid && this.model == null)
				{
					this.SetDialogueBleep();
					if (this.anim == null)
					{
						this.ForceAnimator();
					}
					else
					{
						this.SetAnimator();
						this.UpdateAnimSpecific();
					}
				}
				if (this.digging && this.diganim)
				{
					if (this.animstate == 0)
					{
						this.animstate = 31;
					}
					else if (this.animstate == 1)
					{
						this.animstate = 32;
					}
				}
				if (this.anim.runtimeAnimatorController != null)
				{
					if (!this.overridefly && this.height > 0.1f && (this.animstate <= 1 || this.animstate == 23 || this.animstate == 4 || this.animstate == 27 || this.animstate == 14 || this.animstate == 11 || this.animstate == 13 || this.animstate == 21))
					{
						if (!this.notalk && this.talking)
						{
							this.SetAnim("ft");
						}
						else
						{
							this.SetAnim("f");
						}
					}
					else if (!this.notalk && this.talking && this.animstate < 100)
					{
						this.backsprite = false;
						this.SetAnim("t");
					}
					else if ((!MainManager.instance.flags[616] && (!MainManager.instance.inbattle || MainManager.battle.inevent) && this.backsprite && this.animstate <= 3 && (this.originalid <= 2 || this.originalid == 76)) || this.originalid == 193)
					{
						this.SetAnim("b");
					}
					else
					{
						this.SetAnim("");
					}
					if (this.animstate > 4)
					{
						this.backsprite = false;
					}
				}
			}
			if (!this.overrideanim && (!(this.npcdata != null) || !(this.npcdata.disguiseobj != null) || this.npcdata.entitytype != NPCControl.NPCType.Enemy))
			{
				this.sprite.enabled = (this.nomodel || this.model == null);
			}
			if (this.lastice != this.inice || this.oldstate != this.animstate)
			{
				this.AnimSpecificQuirks();
			}
			this.oldid = this.animid;
			this.oldstate = this.animstate;
			this.oldback = this.backsprite;
			this.oldtalk = this.talking;
			this.oldfly = this.flyinganim;
			this.lastice = this.inice;
		}
		else if (this.animid == -1)
		{
			if (this.npcdata == null || this.originalid == -1)
			{
				this.anim.runtimeAnimatorController = null;
			}
			if (!this.overrideanim && this.sprite != null)
			{
				this.sprite.sprite = null;
				if (this.sprite.enabled)
				{
					this.sprite.enabled = false;
				}
			}
		}
		if (!(this.sprite != null) || this.hologram || (!this.battle && !(this.npcdata == null) && this.npcdata.entitytype == NPCControl.NPCType.NPC && this.npcdata.startlife >= 50f))
		{
			if (this.hologram && this.battle)
			{
				this.sprite.material.renderQueue = 2500;
			}
			return;
		}
		if (this.sprite.material.color.a > 0.9f)
		{
			this.sprite.material.renderQueue = 2450;
			return;
		}
		this.sprite.material.renderQueue = 3000;
	}

	// Token: 0x0600020B RID: 523 RVA: 0x0001E5A8 File Offset: 0x0001C7A8
	private void UpdateHeight()
	{
		if (!this.overrideheight && MainManager.player != null && MainManager.player.entity != this && (this.npcdata == null || this.npcdata.entitytype != NPCControl.NPCType.Object))
		{
			if (this.height > 0.1f)
			{
				this.spritetransform.localPosition = new Vector3(this.spritetransform.localPosition.x, this.height + Mathf.Sin(Time.time * this.bobrange) * this.bobspeed, this.spritetransform.localPosition.z) + this.extraoffset;
				return;
			}
			if (!this.item)
			{
				this.spritetransform.localPosition = new Vector3(this.spritetransform.localPosition.x, 0f, this.spritetransform.localPosition.z) + this.extraoffset;
			}
		}
	}

	// Token: 0x0600020C RID: 524 RVA: 0x0001E6B4 File Offset: 0x0001C8B4
	private void UpdateVelocity()
	{
		if (this.npcdata == null || !this.npcdata.trapped || this.npcdata.entitytype != NPCControl.NPCType.NPC || Time.frameCount % 2 == 0)
		{
			if (this.item)
			{
				this.rigid.velocity = MainManager.ClampVectorBox(this.rigid.velocity, new Vector3(10f, 20f, 10f));
			}
			if (!this.item)
			{
				if (MainManager.player != null && MainManager.player.transform == this.transform && MainManager.player.dashing)
				{
					this.rigid.velocity = new Vector3(this.rigid.velocity.x, Mathf.Clamp(this.rigid.velocity.y, -20f, this.jumpheight * 1.5f), this.rigid.velocity.z);
				}
				else
				{
					this.rigid.velocity = new Vector3(Mathf.Clamp(this.rigid.velocity.x, -10f, 10f), Mathf.Clamp(this.rigid.velocity.y, -20f, float.PositiveInfinity), Mathf.Clamp(this.rigid.velocity.z, -10f, 10f));
				}
			}
		}
		if (this.shrink)
		{
			this.startscale = Vector3.Lerp(this.startscale, Vector3.zero, MainManager.framestep * 0.09f);
		}
		if (this.rigid != null && this.onground && this.rigid.velocity.y < -5f && (this.npcdata == null || this.npcdata.dizzytime <= 0f))
		{
			this.rigid.velocity = new Vector3(this.rigid.velocity.x, 0f, this.rigid.velocity.z);
		}
	}

	// Token: 0x0600020D RID: 525 RVA: 0x0001E8D8 File Offset: 0x0001CAD8
	private void UpdateRotater()
	{
		if (!this.lockrotater)
		{
			this.rotater.eulerAngles = new Vector3(this.rotater.eulerAngles.x, MainManager.MainCamera.transform.eulerAngles.y, this.rotater.eulerAngles.z);
		}
	}

	// Token: 0x0600020E RID: 526 RVA: 0x0001E934 File Offset: 0x0001CB34
	private void UpdateGeneralAnim()
	{
		if (this.sprite != null)
		{
			this.sprite.sortingOrder = (int)(MainManager.MainCamera.WorldToViewportPoint(this.transform.position).z * 1000f);
		}
		if (this.line != null)
		{
			this.line.SetPositions(new Vector3[]
			{
				this.line.transform.position,
				this.line.GetPosition(1)
			});
		}
		if (this.extraanims != null && this.extraanims.Length != 0)
		{
			for (int i = 0; i < this.extraanims.Length; i++)
			{
				if (this.extraanims[i].speed != this.anim.speed)
				{
					this.extraanims[i].speed = this.anim.speed;
				}
			}
		}
	}

	// Token: 0x0600020F RID: 527 RVA: 0x0001EA1C File Offset: 0x0001CC1C
	private void UpdateAirAnim()
	{
		if (!this.overridefly)
		{
			if (!this.overrridejump && !this.overrideanim && this.offgroundframes > 3f && (!this.isplayer || !MainManager.player.dashing) && !this.onground && this.height == 0f && this.rigid != null)
			{
				if (this.rigid.velocity.y > 0f)
				{
					this.animstate = 2;
				}
				else
				{
					this.animstate = 3;
				}
			}
			if (!this.overrideanim && this.onground)
			{
				if (!this.changedstate && this.npcdata != null && this.npcdata.dialogues.Length != 0 && this.animstate == 0 && !MainManager.instance.inevent)
				{
					if (this.npcdata != null && this.npcdata.currentdialogueindex > -1)
					{
						this.animstate = (int)this.npcdata.dialogues[this.npcdata.currentdialogueindex].z;
						return;
					}
					this.animstate = this.basestate;
					return;
				}
				else if (this.animstate == 3)
				{
					this.animstate = this.basestate;
				}
			}
		}
	}

	// Token: 0x06000210 RID: 528 RVA: 0x0001EB68 File Offset: 0x0001CD68
	private void UpdateCollider()
	{
		if (this.npcdata != null && Time.frameCount % 2 == 0)
		{
			if (!this.iskill && this.npcdata.colliderheight == 0f)
			{
				this.ccol.height = this.initialcolliderdata.x;
				this.ccol.radius = this.initialcolliderdata.y;
				this.npcdata.colliderheight = this.initialcolliderdata.x;
			}
			if (MainManager.FreePlayer(false))
			{
				if (this.npcdata.insideid == MainManager.instance.insideid && ((!this.dead && !this.iskill) || this.deathcoroutine == null))
				{
					this.lastpos = this.transform.position;
					return;
				}
				this.transform.position = this.lastpos;
				if (this.forcemove)
				{
					this.StopForceMove();
				}
			}
		}
	}

	// Token: 0x06000211 RID: 529 RVA: 0x0001EC58 File Offset: 0x0001CE58
	private void UpdateEmoticon()
	{
		if (this.emoticon != null)
		{
			if (Time.frameCount % 10 == 0)
			{
				this.emoticon.transform.eulerAngles = new Vector3(0f, MainManager.MainCamera.transform.eulerAngles.y);
				this.emoticon.transform.localPosition = this.emoticonoffset + Vector3.up * this.height + this.extraoffset;
				this.emoticonsprite.enabled = this.incamera;
			}
			if (this.emoticon.runtimeAnimatorController == null)
			{
				this.emoticon.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimationControllers/_Misc/Emoticon/emoticon");
			}
			if (this.emoticoncooldown > 0f)
			{
				this.emoticon.PlayInFixedTime((this.emoticonid == -1) ? EntityControl.disableEmoticon : EntityControl.emoticonIDs[this.emoticonid]);
				this.emoticoncooldown -= MainManager.framestep;
				return;
			}
			if (this.emoticoncooldown > -100f)
			{
				this.emoticon.Play(EntityControl.disableEmoticon);
				this.emoticoncooldown = -101f;
			}
		}
	}

	// Token: 0x06000212 RID: 530 RVA: 0x0001ED8C File Offset: 0x0001CF8C
	private void UpdateMoveSmoke()
	{
		if (this.movesmoke != null)
		{
			if (!this.movesmoke.gameObject.activeInHierarchy)
			{
				this.movesmoke.gameObject.SetActive(true);
			}
			if (!this.overridemovesmoke && this.height == 0f && !this.dead && (this.animstate == 1 || this.animstate == 23) && !MainManager.instance.pause && (this.npcdata == null || this.npcdata.freezecooldown <= 0f) && this.sprite != null && this.sprite.material.color.a > 0.9f && !this.digging)
			{
				this.movesmoke.transform.localPosition = Vector3.zero;
				return;
			}
			this.movesmoke.transform.localPosition = EntityControl.offscreen;
		}
	}

	// Token: 0x06000213 RID: 531 RVA: 0x0001EE8F File Offset: 0x0001D08F
	public float GetSoundDistance()
	{
		return MainManager.GetSoundDistance(this.camdistance, this.soundidstance);
	}

	// Token: 0x06000214 RID: 532 RVA: 0x0001EEA2 File Offset: 0x0001D0A2
	public IEnumerator GradualHeight(float frametime)
	{
		base.StartCoroutine(this.GradualHeight(this.initialheight, frametime));
		yield break;
	}

	// Token: 0x06000215 RID: 533 RVA: 0x0001EEB8 File Offset: 0x0001D0B8
	public IEnumerator GradualHeight(float newheight, float frametime)
	{
		float h = this.height;
		float a = 0f;
		do
		{
			this.height = Mathf.Lerp(h, newheight, a / frametime);
			a += MainManager.framestep;
			yield return null;
		}
		while (a < frametime);
		yield break;
	}

	// Token: 0x06000216 RID: 534 RVA: 0x0001EED5 File Offset: 0x0001D0D5
	public void SetHeight(float h, float range, float spd)
	{
		this.height = h;
		this.bobrange = range;
		this.bobspeed = spd;
	}

	// Token: 0x06000217 RID: 535 RVA: 0x0001EEEC File Offset: 0x0001D0EC
	public void FlipSimple()
	{
		this.flip = !this.flip;
	}

	// Token: 0x06000218 RID: 536 RVA: 0x0001EEFD File Offset: 0x0001D0FD
	public void FlipValue(float right)
	{
		this.flip = (right == 1f);
	}

	// Token: 0x06000219 RID: 537 RVA: 0x0001EF0D File Offset: 0x0001D10D
	public IEnumerator TempIgnoreColision(Collider c, float sectime)
	{
		Physics.IgnoreCollision(this.ccol, c, true);
		if (this.detect != null)
		{
			Physics.IgnoreCollision(this.detect, c, true);
		}
		yield return new WaitForSeconds(sectime);
		if (c != null)
		{
			Physics.IgnoreCollision(this.ccol, c, false);
			if (this.detect != null)
			{
				Physics.IgnoreCollision(this.detect, c, false);
			}
		}
		yield break;
	}

	// Token: 0x0600021A RID: 538 RVA: 0x0001EF2C File Offset: 0x0001D12C
	public void RefreshShadow()
	{
		if (this.shadow != null && ((this.npcdata != null && this.npcdata.freezecooldown > 0f) || (this.item && (this.npcdata == null || this.npcdata.timer > 0f)) || !this.onground || this.battle || Time.frameCount % 2 != 0 || MainManager.GetSqrDistance(this.lastshadow, this.transform.position) > 0.005f))
		{
			if (!this.iskill && (this.overrideshadow || this.sprite.enabled || this.model != null))
			{
				RaycastHit raycastHit;
				if (Physics.Raycast(this.transform.position + Vector3.up, Vector3.down, out raycastHit, 10f, 8448))
				{
					this.shadowtransform.position = new Vector3(this.transform.position.x, raycastHit.point.y + 0.025f, this.transform.position.z);
					this.shadowtransform.localScale = this.shadowsize * Vector3.ClampMagnitude(Vector3.one * Mathf.Clamp(this.ccol.radius * (1f - Mathf.Abs(raycastHit.point.y - (this.transform.position.y + this.height)) / 10f), 0f, float.PositiveInfinity), 1f);
					this.shadowtransform.localScale = new Vector3(this.shadowtransform.localScale.x, this.shadowtransform.localScale.y, 1f);
					this.shadowtransform.LookAt(this.shadowtransform.position + raycastHit.normal);
					this.shadow.color = new Color(0f, 0f, 0f, Mathf.Clamp(this.sprite.material.color.a, 0f, 0.4f));
				}
				this.shadow.enabled = (raycastHit.transform != null && !this.digging);
			}
			else
			{
				this.shadow.enabled = false;
			}
			this.lastshadow = this.transform.position;
		}
	}

	// Token: 0x0600021B RID: 539 RVA: 0x0001F1C8 File Offset: 0x0001D3C8
	public void ResetTrail()
	{
		if (this.traildata.trails != null)
		{
			this.traildata.id = 0;
			for (int i = 0; i < this.traildata.trails.Length; i++)
			{
				this.traildata.trails[i].enabled = false;
				this.traildata.time[i] = 0f;
			}
		}
	}

	// Token: 0x0600021C RID: 540 RVA: 0x0001F22B File Offset: 0x0001D42B
	public void FaceAhead()
	{
		this.FaceTowards(this.transform.position + new Vector3(1f, 0f, 0f));
	}

	// Token: 0x0600021D RID: 541 RVA: 0x0001F257 File Offset: 0x0001D457
	public void FaceBehind()
	{
		this.FaceTowards(this.transform.position + new Vector3(-1f, 0f, 0f));
	}

	// Token: 0x0600021E RID: 542 RVA: 0x0001F283 File Offset: 0x0001D483
	public void FaceUp()
	{
		this.FaceTowards(this.transform.position + new Vector3(0f, 0f, 1f));
	}

	// Token: 0x0600021F RID: 543 RVA: 0x0001F2AF File Offset: 0x0001D4AF
	public void FaceDown()
	{
		this.FaceTowards(this.transform.position + new Vector3(0f, 0f, -1f));
	}

	// Token: 0x06000220 RID: 544 RVA: 0x0001F2DB File Offset: 0x0001D4DB
	public IEnumerator SpecialAnimation(string animation)
	{
		bool toj = this.overrridejump;
		this.overrideanim = true;
		this.overrridejump = true;
		string a2 = animation.ToLower();
		if (a2 == "levelup")
		{
			SpriteRenderer t;
			Vector3 tp;
			switch (this.animid)
			{
			case 0:
			{
				this.animstate = 108;
				yield return new WaitForSeconds(0.3f);
				t = new GameObject().AddComponent<SpriteRenderer>();
				t.sprite = MainManager.instance.projectilepsrites[0];
				t.material = MainManager.spritemat;
				t.transform.position = this.transform.position + new Vector3(-0.5f, 2f, -0.1f);
				tp = t.transform.position;
				this.spin = new Vector3(0f, -15f);
				this.animstate = 109;
				float a = 0f;
				while (a < 1f)
				{
					t.transform.position = MainManager.BeizierCurve3(tp, tp + Vector3.right, 7.5f, a);
					t.transform.localEulerAngles += Vector3.back * MainManager.framestep * 10f;
					a += MainManager.framestep * 0.015f;
					yield return null;
				}
				Object.Destroy(t.gameObject);
				this.animstate = 110;
				this.spin = Vector3.zero;
				break;
			}
			case 1:
				this.animstate = 114;
				yield return new WaitForSeconds(1f);
				this.animstate = 115;
				this.spin = new Vector3(0f, 16f, 0f);
				yield return new WaitForSeconds(0.5f);
				this.spin = Vector3.zero;
				break;
			case 2:
			{
				this.animstate = 100;
				yield return new WaitForSeconds(0.6f);
				MainManager.PlayParticle("mothicenormal", this.transform.position + Vector3.forward * 0.5f + Vector3.up, 1.5f);
				DialogueAnim dialogueAnim = (Object.Instantiate(Resources.Load("Prefabs/Objects/icepillar"), this.transform.position + Vector3.forward * 0.5f + Vector3.down * 0.1f, Quaternion.Euler(-90f, 0f, 0f)) as GameObject).AddComponent<DialogueAnim>();
				dialogueAnim.SetUp(Vector3.zero, new Vector3(0.7f, 0.6f, 0.2f), Vector3.zero, 0.075f);
				dialogueAnim.shrinkspeed = 0.075f;
				dialogueAnim.transform.parent = this.transform;
				this.animstate = 102;
				yield return new WaitForSeconds(0.3f);
				this.animstate = 113;
				break;
			}
			}
			t = null;
			tp = default(Vector3);
		}
		yield return null;
		this.overrideanim = false;
		this.overrridejump = toj;
		this.specialanim = null;
		yield break;
	}

	// Token: 0x06000221 RID: 545 RVA: 0x0001F2F4 File Offset: 0x0001D4F4
	public void RefreshCOT()
	{
		if (this.cotunknown)
		{
			this.spritebasecolor = EntityControl.cotcolor;
		}
		if (this.extras != null && this.extras.Length != 0 && this.cotunknown)
		{
			this.refreshedcotu = true;
			for (int i = 0; i < this.extras.Length; i++)
			{
				if (this.extras[i] != null)
				{
					Renderer component = this.extras[i].GetComponent<Renderer>();
					if (component != null)
					{
						component.material.color = EntityControl.cot3d;
					}
					SpriteRenderer component2 = this.extras[i].GetComponent<SpriteRenderer>();
					if (component2 != null)
					{
						component2.color = EntityControl.cotcolor;
					}
				}
			}
		}
	}

	// Token: 0x06000222 RID: 546 RVA: 0x0001F3A4 File Offset: 0x0001D5A4
	public void ForceCOT()
	{
		this.spritebasecolor = EntityControl.cotcolor;
		this.sprite.material.color = EntityControl.cotcolor;
		this.sprite.color = this.sprite.material.color;
		this.RefreshCOT();
	}

	// Token: 0x06000223 RID: 547 RVA: 0x0001F3F2 File Offset: 0x0001D5F2
	public void OverrideOver()
	{
		this.overrideanim = false;
		this.animstate = this.basestate;
	}

	// Token: 0x06000224 RID: 548 RVA: 0x0001F407 File Offset: 0x0001D607
	public void ActivateDefenseTap(float ammount)
	{
		if (MainManager.battle != null)
		{
			MainManager.battle.CanInput(ammount);
		}
	}

	// Token: 0x06000225 RID: 549 RVA: 0x0001F421 File Offset: 0x0001D621
	public void Jump()
	{
		this.Jump(this.jumpheight);
	}

	// Token: 0x06000226 RID: 550 RVA: 0x0001F430 File Offset: 0x0001D630
	public void Jump(float h)
	{
		if (!this.item && MainManager.battle == null)
		{
			this.Unfix();
		}
		this.offgroundframes = 20f;
		this.rigid.velocity = new Vector3(this.rigid.velocity.x, h, this.rigid.velocity.z);
		this.jumpcooldown = 30f;
		if (this.flowerbed != null)
		{
			MainManager.PlayParticle("FlowerJump", this.transform.position);
		}
	}

	// Token: 0x06000227 RID: 551 RVA: 0x0001F4C4 File Offset: 0x0001D6C4
	public bool HasGroundAhead()
	{
		return this.HasGroundAhead(this.transform.position + this.detect.transform.forward.normalized * 2f + Vector3.up / 2f, 5f);
	}

	// Token: 0x06000228 RID: 552 RVA: 0x0001F522 File Offset: 0x0001D722
	public bool HasGroundAhead(Vector3 target)
	{
		if (this.detect == null)
		{
			this.CreateDetector();
		}
		this.detect.transform.LookAt(target);
		return this.HasGroundAhead();
	}

	// Token: 0x06000229 RID: 553 RVA: 0x0001F54F File Offset: 0x0001D74F
	public bool HasGroundAhead(Vector3 point, float checkdistance)
	{
		return Physics.Raycast(point, Vector3.down, checkdistance, 8448);
	}

	// Token: 0x0600022A RID: 554 RVA: 0x0001F564 File Offset: 0x0001D764
	public void CreateHPBar()
	{
		GameObject gameObject = new GameObject("HPBarHolder");
		gameObject.transform.parent = this.transform;
		gameObject.transform.localPosition = new Vector3(0f, -0.5f);
		gameObject.transform.localScale = Vector3.one;
		SpriteRenderer component = MainManager.NewUIObject("back", gameObject.transform, new Vector3(-0.4f, 0f), new Vector3(0.7f, 0.5f, 1f), MainManager.guisprites[64]).GetComponent<SpriteRenderer>();
		SpriteRenderer component2 = MainManager.NewUIObject("bar", component.transform, default(Vector3), Vector3.one, MainManager.guisprites[58]).GetComponent<SpriteRenderer>();
		SpriteRenderer component3 = MainManager.NewUIObject("numberholder", component.transform, default(Vector3), Vector3.one, MainManager.guisprites[65]).GetComponent<SpriteRenderer>();
		component2.color = Color.yellow;
		component2.sortingOrder = component.sortingOrder + 1;
		component3.color = Color.black;
		component3.sortingOrder = component.sortingOrder - 1;
		this.hpbarfont = DynamicFont.SetUp("", false, true, 5f, 2, component2.sortingOrder + 1, Vector2.one * 0.5f, gameObject.transform, new Vector3(0.45f, -0.2f, -0.1f), Color.white);
		this.hpbar = gameObject.transform;
		this.defstat = DynamicFont.SetUp("", false, true, 5f, 2, component.sortingOrder + 20, Vector2.one * 0.5f, gameObject.transform, new Vector3(-0.7f, -0.2f, -0.1f), Color.white);
		MainManager.NewUIObject("deficon", component.transform, new Vector3(-0.3f, 0f), new Vector3(0.9f, 1.3f, 1f), MainManager.guisprites[215], 10);
		this.hpbar.gameObject.SetActive(false);
	}

	// Token: 0x0600022B RID: 555 RVA: 0x0001F77C File Offset: 0x0001D97C
	public void StopMoving(int targetstate)
	{
		if (this.rigid != null)
		{
			this.rigid.velocity = new Vector3(0f, this.rigid.velocity.y, 0f);
		}
		this.deltavelocity = Vector2.zero;
		if (!this.overrideanim)
		{
			if (targetstate > -1)
			{
				this.animstate = Mathf.Clamp(targetstate, 0, 9999);
				return;
			}
			if (this.animstate == this.walkstate)
			{
				this.animstate = this.basestate;
			}
		}
	}

	// Token: 0x0600022C RID: 556 RVA: 0x0001F805 File Offset: 0x0001DA05
	private void LateGround()
	{
		this.onground = true;
	}

	// Token: 0x0600022D RID: 557 RVA: 0x0001F810 File Offset: 0x0001DA10
	public void Freeze()
	{
		this.freezeoffset = new Vector3(this.initialfrezeoffset.x * (float)(this.flip ? -1 : 1), this.initialfrezeoffset.y, this.initialfrezeoffset.z);
		if (!this.battle || MainManager.SoundIsPlaying("Freeze") == -1)
		{
			MainManager.PlaySound("Freeze");
		}
		this.onground = true;
		this.icecube = Object.Instantiate<GameObject>(EntityControl.icecubeprefab);
		this.icecube.transform.parent = this.transform;
		this.icecube.transform.localScale = Vector3.zero;
		this.StopForceMove();
		Physics.IgnoreCollision(this.icecube.GetComponentInChildren<Collider>(), this.ccol, true);
		this.spin = Vector3.zero;
		this.extraoffset = Vector3.zero;
		this.animstate = 11;
		this.anim.speed = 0f;
		if (!this.battle && this.npcdata != null)
		{
			this.npcdata.STOP();
		}
		SpriteBounce component = this.rotater.GetComponent<SpriteBounce>();
		if (component != null)
		{
			component.enabled = false;
		}
		this.UpdateAnimSpecific();
	}

	// Token: 0x0600022E RID: 558 RVA: 0x0001F947 File Offset: 0x0001DB47
	public void StopForceMove()
	{
		this.StopForceMove(-1, false);
	}

	// Token: 0x0600022F RID: 559 RVA: 0x0001F954 File Offset: 0x0001DB54
	public void StopForceMove(int targetstate, bool smooth)
	{
		this.forcemove = false;
		if (this.forcemoving != null)
		{
			base.StopCoroutine(this.forcemoving);
			this.forcemoving = null;
		}
		if (!smooth)
		{
			this.StopMoving(targetstate);
		}
		else
		{
			this.rigid.velocity = Vector3.Lerp(this.rigid.velocity, new Vector3(0f, this.rigid.velocity.y), 0.5f);
			if (targetstate > -1 && !this.overrideanim && new Vector2(this.rigid.velocity.x, this.rigid.velocity.z).magnitude <= 0.1f)
			{
				this.animstate = targetstate;
			}
		}
		if (this.looktowards != null)
		{
			base.StartCoroutine(this.DelayedLook(this.looktowards.Value, 0.22f));
			this.looktowards = null;
		}
	}

	// Token: 0x06000230 RID: 560 RVA: 0x0001FA49 File Offset: 0x0001DC49
	private IEnumerator DelayedLook(Vector3 target, float delay)
	{
		yield return new WaitForSeconds(delay);
		this.FaceTowards(target);
		yield break;
	}

	// Token: 0x06000231 RID: 561 RVA: 0x0001FA66 File Offset: 0x0001DC66
	public IEnumerator ShakeSprite(float intensity, float frametimer)
	{
		base.StartCoroutine(this.ShakeSprite(new Vector3(intensity, 0f), frametimer));
		yield break;
	}

	// Token: 0x06000232 RID: 562 RVA: 0x0001FA83 File Offset: 0x0001DC83
	public IEnumerator ShakeSprite(Vector3 intensity, float frametimer)
	{
		Vector3 startp = this.spritetransform.localPosition;
		while (frametimer > 0f)
		{
			this.spritetransform.localPosition = startp + new Vector3(Random.Range(-intensity.x, intensity.x), Random.Range(-intensity.y, intensity.y), Random.Range(-intensity.z, intensity.z));
			frametimer -= MainManager.framestep;
			yield return null;
		}
		this.spritetransform.localPosition = startp + this.extraoffset;
		yield break;
	}

	// Token: 0x06000233 RID: 563 RVA: 0x0001FAA0 File Offset: 0x0001DCA0
	public void Emoticon(MainManager.Emoticons emote, int time)
	{
		this.Emoticon(emote - MainManager.Emoticons.Talk, time);
	}

	// Token: 0x06000234 RID: 564 RVA: 0x0001FAAC File Offset: 0x0001DCAC
	public void Emoticon(MainManager.Emoticons emote)
	{
		this.Emoticon(emote - MainManager.Emoticons.Talk, 60);
	}

	// Token: 0x06000235 RID: 565 RVA: 0x0001FAB9 File Offset: 0x0001DCB9
	public void Emoticon(int type, int time)
	{
		this.emoticoncooldown = (float)time;
		this.emoticonid = type;
		this.UpdateEmoticon();
	}

	// Token: 0x06000236 RID: 566 RVA: 0x0001FAD0 File Offset: 0x0001DCD0
	private void UpdateGround()
	{
		if (this.npcdata == null || this.npcdata.objecttype != NPCControl.ObjectTypes.PushRock)
		{
			if (this.onground && this.deltavelocity.magnitude < 0.2f && this.jumpcooldown <= 0f)
			{
				if (MainManager.player == null || this.transform != MainManager.player.transform)
				{
					this.ccol.material.staticFriction = 1f;
					this.ccol.material.dynamicFriction = 1f;
				}
				else if (this.hitwall || this.animstate != 1)
				{
					this.ccol.material.staticFriction = 1f;
					this.ccol.material.dynamicFriction = 1f;
				}
				else
				{
					this.ccol.material.staticFriction = 0f;
					this.ccol.material.dynamicFriction = 0f;
				}
			}
			else
			{
				this.ccol.material.staticFriction = 0f;
				this.ccol.material.dynamicFriction = 0f;
			}
		}
		else if (this.npcdata == null || this.npcdata.objecttype != NPCControl.ObjectTypes.PushRock)
		{
			if (this.rigid.velocity.y >= -0.1f && this.rigid.velocity.y <= 0.1f)
			{
				this.ccol.material.staticFriction = 1f;
				this.ccol.material.dynamicFriction = 1f;
			}
			else
			{
				this.ccol.material.staticFriction = 0f;
				this.ccol.material.dynamicFriction = 0f;
			}
		}
		if (this.jumpcooldown > 0f)
		{
			this.jumpcooldown -= MainManager.framestep;
		}
	}

	// Token: 0x06000237 RID: 567 RVA: 0x0001FCE0 File Offset: 0x0001DEE0
	public void ForceMove(Vector3 target, float frametime, int movestate, int stopstate)
	{
		this.forcemoving = base.StartCoroutine(this.ForceMove(target, frametime, new int[]
		{
			movestate,
			stopstate
		}));
	}

	// Token: 0x06000238 RID: 568 RVA: 0x0001FD10 File Offset: 0x0001DF10
	public void ForceMove(Vector3 target, float frametime, bool changeanim)
	{
		if (changeanim)
		{
			this.forcemoving = base.StartCoroutine(this.ForceMove(target, frametime, new int[]
			{
				this.walkstate,
				this.basestate
			}));
			return;
		}
		this.forcemoving = base.StartCoroutine(this.ForceMove(target, frametime, new int[]
		{
			this.animstate,
			this.animstate
		}));
	}

	// Token: 0x06000239 RID: 569 RVA: 0x0001FD79 File Offset: 0x0001DF79
	public void StartDeath()
	{
		this.deathcoroutine = base.StartCoroutine(this.Death());
	}

	// Token: 0x0600023A RID: 570 RVA: 0x0001FD8D File Offset: 0x0001DF8D
	private IEnumerator ForceMove(Vector3 target, float frametime, int[] an)
	{
		this.animstate = an[0];
		float a = 0f;
		Vector3 p = this.transform.position;
		do
		{
			this.transform.position = Vector3.Lerp(p, target, a / frametime);
			a += MainManager.framestep;
			yield return null;
		}
		while (a < frametime + 1f);
		this.animstate = an[1];
		this.forcemoving = null;
		yield break;
	}

	// Token: 0x0600023B RID: 571 RVA: 0x0001FDB1 File Offset: 0x0001DFB1
	public void ForceHitWall()
	{
		this.hitwall = false;
	}

	// Token: 0x0600023C RID: 572 RVA: 0x0001FDBC File Offset: 0x0001DFBC
	public void Move(Vector3 pos, float multiplier, int state)
	{
		if (this.fixedentity)
		{
			this.Unfix();
		}
		this.rigid.constraints = RigidbodyConstraints.FreezeRotation;
		if (!this.overrideanim)
		{
			this.animstate = state;
		}
		this.moverotater.LookAt(pos);
		this.FaceTowards(pos);
		if (this.following == null)
		{
			this.flip = (MainManager.MainCamera.WorldToViewportPoint(this.transform.position).x < MainManager.MainCamera.WorldToViewportPoint(pos).x);
		}
		if (this.rigid.useGravity || this.ignorey)
		{
			this.rigid.velocity = new Vector3(this.moverotater.forward.normalized.x * this.speed * multiplier, this.rigid.velocity.y, this.moverotater.forward.normalized.z * this.speed * multiplier);
		}
		else
		{
			this.rigid.velocity = this.moverotater.forward.normalized * this.speed * multiplier;
		}
		if (this.walktype == EntityControl.WalkType.Jump && this.onground && this.jumpcooldown <= 0f)
		{
			this.Jump();
			MainManager.AnimIDs animIDs = this.originalid + MainManager.AnimIDs.Bee;
			if (animIDs != MainManager.AnimIDs.Ahoneynation)
			{
				if (animIDs == MainManager.AnimIDs.JumpingSpider)
				{
					this.PlaySound("Jump", 1f, 0.85f);
				}
			}
			else
			{
				this.PlaySound("AhoneynationHopJump");
			}
		}
		this.deltavelocity = new Vector2(this.moverotater.forward.normalized.x, this.moverotater.forward.normalized.z) * this.speed * multiplier;
	}

	// Token: 0x0600023D RID: 573 RVA: 0x0001FF9B File Offset: 0x0001E19B
	public void MoveTowards(Vector3 pos, float multiplier, int state, int stopstate, bool ignore_y, Vector3 lookaftermove)
	{
		this.looktowards = new Vector3?(lookaftermove);
		this.MoveTowards(pos, multiplier, state, stopstate, ignore_y);
	}

	// Token: 0x0600023E RID: 574 RVA: 0x0001FFB7 File Offset: 0x0001E1B7
	public void MoveTowards(Vector3 pos, float multiplier)
	{
		this.MoveTowards(pos, multiplier, this.walkstate, this.basestate, false);
	}

	// Token: 0x0600023F RID: 575 RVA: 0x0001FFCE File Offset: 0x0001E1CE
	public void MoveTowards(float x, float y, float z)
	{
		this.MoveTowards(new Vector3(x, y, z), 1f, this.walkstate, this.basestate, false);
	}

	// Token: 0x06000240 RID: 576 RVA: 0x0001FFF0 File Offset: 0x0001E1F0
	public void MoveTowards(float x, float z)
	{
		this.MoveTowards(new Vector3(x, 0f, z), 1f, this.walkstate, this.basestate, false);
	}

	// Token: 0x06000241 RID: 577 RVA: 0x00020016 File Offset: 0x0001E216
	public void MoveTowards(Vector3 pos)
	{
		this.MoveTowards(pos, 1f, this.walkstate, this.basestate, false);
	}

	// Token: 0x06000242 RID: 578 RVA: 0x00020031 File Offset: 0x0001E231
	public void MoveTowards(Vector3 pos, float multiplier, int state, int stopstate)
	{
		this.MoveTowards(pos, multiplier, state, stopstate, false);
	}

	// Token: 0x06000243 RID: 579 RVA: 0x00020040 File Offset: 0x0001E240
	public void MoveTowards(Vector3 pos, float multiplier, int state, int stopstate, bool ignore_y)
	{
		this.forcemultiplier = multiplier;
		Vector3 a = pos;
		Vector3 position = this.transform.position;
		this.forcetarget = a + MainManager.GetDirection(pos, position).normalized * 0.4f;
		this.forcestop = stopstate;
		this.forceanim = state;
		this.ignorey = ignore_y;
		if (this.playerentity)
		{
			this.forcetimer = 500f * (MainManager.instance.inevent ? 0.75f : 0.5f);
		}
		else
		{
			this.forcetimer = ((this.battle && MainManager.battle.checkingdead == null) ? 225f : 500f);
		}
		if (this.extratimer)
		{
			this.forcetimer *= 2f;
		}
		this.forcemove = true;
	}

	// Token: 0x06000244 RID: 580 RVA: 0x00020113 File Offset: 0x0001E313
	public void FaceTowards(Vector3 other)
	{
		this.FaceTowards(other, false, false);
	}

	// Token: 0x06000245 RID: 581 RVA: 0x00020113 File Offset: 0x0001E313
	public void FaceTowards(Vector3 other, bool noback)
	{
		this.FaceTowards(other, false, false);
	}

	// Token: 0x06000246 RID: 582 RVA: 0x00020120 File Offset: 0x0001E320
	public void FaceTowards(Vector3 other, bool noback, bool forceback)
	{
		this.flip = (MainManager.MainCamera.WorldToViewportPoint(this.transform.position).x < MainManager.MainCamera.WorldToViewportPoint(other).x);
		if (forceback || (!noback && !this.lockback && !this.talking && !MainManager.instance.message))
		{
			this.backsprite = (MainManager.MainCamera.WorldToViewportPoint(other).z + -0.5f > MainManager.MainCamera.WorldToViewportPoint(this.transform.position).z);
		}
	}

	// Token: 0x06000247 RID: 583 RVA: 0x000201B9 File Offset: 0x0001E3B9
	public void CreateDetector()
	{
		this.CreateDetector(new Vector3(0.8f, 0.7f, 0.15f), new Vector3(0f, 0.5f, 0.5f));
	}

	// Token: 0x06000248 RID: 584 RVA: 0x000201EC File Offset: 0x0001E3EC
	public void CreateDetector(Vector3 size, Vector3 center)
	{
		if (this.rotater != null)
		{
			this.detect = new GameObject("Detector").AddComponent<BoxCollider>();
			this.detect.transform.parent = this.rotater.transform;
			this.detect.transform.localPosition = Vector3.zero;
			this.detect.size = size;
			this.detect.center = center;
			this.detect.isTrigger = true;
			this.detect.gameObject.AddComponent<RayDetector>();
			Physics.IgnoreCollision(this.detect, this.ccol, true);
		}
	}

	// Token: 0x06000249 RID: 585 RVA: 0x00020298 File Offset: 0x0001E498
	public void DetectIgnoreSphere(bool ignore)
	{
		NPCControl[] array = Object.FindObjectsOfType<NPCControl>();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Physics.IgnoreCollision(this.detect, array[i].scol, ignore);
			}
		}
	}

	// Token: 0x0600024A RID: 586 RVA: 0x000202D0 File Offset: 0x0001E4D0
	public IEnumerator Death()
	{
		this.deathcoroutine = base.StartCoroutine(this.Death(true));
		yield return null;
		yield break;
	}

	// Token: 0x0600024B RID: 587 RVA: 0x000202E0 File Offset: 0x0001E4E0
	public Vector3 FlipAngle()
	{
		if (this.flip)
		{
			return new Vector3(this.spritetransform.localEulerAngles.x, 180f, this.spritetransform.localEulerAngles.z);
		}
		return new Vector3(this.spritetransform.localEulerAngles.x, 0f, this.spritetransform.localEulerAngles.z);
	}

	// Token: 0x0600024C RID: 588 RVA: 0x0002034C File Offset: 0x0001E54C
	public Vector3 FlipAngle(bool setangle)
	{
		if (setangle)
		{
			if (this.flip)
			{
				this.spritetransform.localEulerAngles = new Vector3(this.spritetransform.localEulerAngles.x, 180f, this.spritetransform.localEulerAngles.z);
			}
			else
			{
				this.spritetransform.localEulerAngles = new Vector3(this.spritetransform.localEulerAngles.x, 0f, this.spritetransform.localEulerAngles.z);
			}
			return this.spritetransform.localEulerAngles;
		}
		return this.FlipAngle();
	}

	// Token: 0x0600024D RID: 589 RVA: 0x000203E5 File Offset: 0x0001E5E5
	public IEnumerator SlowSpinStop(Vector3 spinammount, float frametime)
	{
		this.spritetransform.localScale = this.startscale;
		if (this.model != null && this.model.localScale.magnitude < 0.1f)
		{
			this.model.localScale = Vector3.one;
		}
		float st = frametime;
		this.spin = spinammount;
		while (frametime > 0f)
		{
			this.spin = Vector3.Lerp(spinammount, Vector3.zero, 1f - frametime / st);
			frametime -= MainManager.framestep;
			yield return null;
		}
		this.spin = Vector3.zero;
		yield break;
	}

	// Token: 0x0600024E RID: 590 RVA: 0x00020402 File Offset: 0x0001E602
	public void TempSpin(Vector3 s, float time)
	{
		this.spin = s;
		base.Invoke("StopSpin", time);
	}

	// Token: 0x0600024F RID: 591 RVA: 0x00020417 File Offset: 0x0001E617
	private void StopSpin()
	{
		this.spin = Vector3.zero;
	}

	// Token: 0x06000250 RID: 592 RVA: 0x00020424 File Offset: 0x0001E624
	public void BreakIce()
	{
		if (this.icecube != null)
		{
			Object.Destroy(Object.Instantiate(Resources.Load("Prefabs/Particles/IceShatter"), this.icecube.transform.position, Quaternion.Euler(-90f, 0f, 0f)) as GameObject, 1f);
			this.PlaySound("IceBreak", 0.65f);
			Object.Destroy(this.icecube);
			this.animstate = this.basestate;
			this.anim.speed = 1f;
			this.onground = false;
			this.Jump();
		}
		this.tempheightoverride = false;
		if (!this.overrideminheight && this.height < this.minheight)
		{
			this.height = this.minheight;
			this.bobspeed = this.startbs;
			this.bobrange = this.startbf;
		}
		if (this.battle && !this.isplayer && this.battleid > -1 && this.battleid < MainManager.battle.enemydata.Length)
		{
			MainManager.battle.enemydata[this.battleid].size = MainManager.battle.enemydata[this.battleid].initialsize;
		}
		this.shakeice = false;
		if (this.rotater != null)
		{
			SpriteBounce component = this.rotater.GetComponent<SpriteBounce>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		this.UpdateSpriteMat();
		base.Invoke("UpdateAnimSpecific", 0.1f);
	}

	// Token: 0x06000251 RID: 593 RVA: 0x000205B1 File Offset: 0x0001E7B1
	public IEnumerator Death(bool activatekill)
	{
		this.nocondition = true;
		this.BreakIce();
		this.StopForceMove(-1, false);
		this.dead = true;
		if (this.rigid != null)
		{
			this.rigid.velocity = Vector3.zero;
		}
		if (this.digpart != null && this.digpart.Length != 0)
		{
			for (int j = 0; j < this.digpart.Length; j++)
			{
				if (this.digpart[j] != null)
				{
					Object.Destroy(this.digpart[j]);
				}
			}
		}
		Vector3 localEulerAngles = this.spritetransform.localEulerAngles;
		Vector3 sp = this.spritetransform.position;
		if (this.npcdata != null)
		{
			this.npcdata.STOP();
			if (this.npcdata.regionalflag > -1)
			{
				MainManager.instance.regionalflags[this.npcdata.regionalflag] = true;
			}
			if (this.npcdata.disguiseobj != null)
			{
				Object.Destroy(this.npcdata.disguiseobj.gameObject);
			}
			if (this.npcdata.behaviorroutine != null)
			{
				this.npcdata.StopCoroutine(this.npcdata.behaviorroutine);
			}
			this.npcdata.inrange = false;
			this.npcdata.hit = true;
			MainManager.AnimIDs animIDs = this.originalid + MainManager.AnimIDs.Bee;
			if (animIDs == MainManager.AnimIDs.ToeBiter && this.npcdata.internaltransform != null && this.npcdata.internaltransform.Length != 0)
			{
				for (int k = 0; k < this.npcdata.internaltransform.Length; k++)
				{
					if (this.npcdata.internaltransform[k] != null)
					{
						Object.Destroy(this.npcdata.internaltransform[k].gameObject);
					}
				}
			}
		}
		this.overrideflip = false;
		this.icooldown = 0f;
		this.sprite.enabled = true;
		this.LockRigid(true);
		this.bobrange = 0f;
		this.bobspeed = 0f;
		switch (this.destroytype)
		{
		case NPCControl.DeathType.None:
			this.LockRigid(true);
			break;
		case NPCControl.DeathType.SpinSmoke:
		case NPCControl.DeathType.SpinNoSmoke:
		case NPCControl.DeathType.SpinKO:
		case NPCControl.DeathType.KO:
		case NPCControl.DeathType.SpinSmokeNoSprite:
			if (this.npcdata != null && this.npcdata.pusher != null)
			{
				this.npcdata.pusher.enabled = false;
			}
			if (this.destroytype == NPCControl.DeathType.SpinSmokeNoSprite)
			{
				this.animid = -1;
				this.sprite.enabled = false;
			}
			this.overrideanim = true;
			this.overridefollow = true;
			this.overrridejump = true;
			this.rigid.useGravity = false;
			this.rigid.velocity = Vector3.zero;
			this.ccol.center = new Vector3(0f, 9999f);
			this.ccol.height = 0f;
			yield return null;
			if (this.animstate == 15)
			{
				this.animstate = 16;
			}
			else
			{
				this.animstate = 11;
			}
			this.forceanim = this.animstate;
			if (this.destroytype == NPCControl.DeathType.SpinKO || this.destroytype == NPCControl.DeathType.SpinNoSmoke || this.destroytype == NPCControl.DeathType.SpinSmoke)
			{
				int num = MainManager.SoundIsPlaying("Death0");
				if (num == -1 || MainManager.sounds[num].time > 0.25f)
				{
					MainManager.PlaySound("Death0", -1, 1f, 0.8f);
				}
				this.spin = Vector3.up * 15f;
				yield return new WaitForSeconds(0.75f);
			}
			this.spritetransform.localEulerAngles = this.FlipAngle();
			if (this.destroytype == NPCControl.DeathType.SpinSmoke || this.destroytype == NPCControl.DeathType.SpinNoSmoke)
			{
				this.spin = -Vector3.right * 5f;
				yield return new WaitForSeconds(0.3f);
				this.sprite.enabled = false;
				if (this.destroytype == NPCControl.DeathType.SpinSmoke)
				{
					MainManager.DeathSmoke(this.spritetransform.position + Vector3.back / 2f);
				}
			}
			else if (this.destroytype == NPCControl.DeathType.KO || this.destroytype == NPCControl.DeathType.SpinKO)
			{
				this.animstate = 18;
			}
			break;
		case NPCControl.DeathType.Shrink:
		case NPCControl.DeathType.ShrinkNoSmoke:
		{
			string soundend = null;
			this.LockRigid(true);
			MainManager.AnimIDs animIDs = this.originalid + MainManager.AnimIDs.Bee;
			if (animIDs != MainManager.AnimIDs.icepillar)
			{
				if (animIDs - MainManager.AnimIDs.Pitcher <= 1)
				{
					this.animstate = 11;
					MainManager.PlaySound("ChargeDown");
					soundend = "ChargeDown2";
				}
			}
			else
			{
				MainManager.PlaySound("IceMelt");
			}
			float a = 0f;
			float b = 80f;
			Vector3 ss = this.startscale;
			do
			{
				this.startscale = Vector3.Lerp(ss, Vector3.zero, a / b);
				a += MainManager.framestep;
				yield return null;
			}
			while (a < b + 1f);
			if (this.destroytype == NPCControl.DeathType.Shrink)
			{
				MainManager.DeathSmoke(this.transform.position);
			}
			if (soundend != null)
			{
				MainManager.PlaySound(soundend);
			}
			soundend = null;
			ss = default(Vector3);
			break;
		}
		case NPCControl.DeathType.PlayerDeath:
		{
			this.overrideanim = true;
			this.animstate = 11;
			this.anim.Play("Hurt");
			int num2 = MainManager.SoundIsPlaying("Death2");
			if (num2 == -1 || MainManager.sounds[num2].time > 0.2f)
			{
				MainManager.PlaySound("Death2");
			}
			this.spin = Vector3.up * 15f;
			while (this.spin.magnitude > 2.5f)
			{
				this.spin = Vector3.Lerp(this.spin, Vector3.zero, MainManager.framestep * (this.isplayer ? 0.01f : 0.1f));
				yield return null;
			}
			this.overrideanim = false;
			MainManager.PlaySound("Drop");
			this.animstate = 18;
			this.spin = Vector3.up * 0.0001f;
			while ((this.spritetransform.localEulerAngles - this.FlipAngle()).magnitude > 0.5f)
			{
				this.spritetransform.localEulerAngles = Vector3.Lerp(this.spritetransform.localEulerAngles, this.FlipAngle(), MainManager.framestep * (this.isplayer ? 0.1f : 0.2f));
				yield return null;
			}
			this.spin = Vector3.zero;
			goto IL_F83;
		}
		case NPCControl.DeathType.NinjaLog:
			this.LockRigid(true);
			MainManager.DeathSmoke(this.transform.position, new Vector3(2f, 3f, 2f));
			MainManager.PlaySound("LeafDeathPoof");
			break;
		case NPCControl.DeathType.Sink:
		{
			float b = 0f;
			float a = 90f;
			Vector3 ss = this.spritetransform.position;
			Vector3 st = this.spritetransform.position + new Vector3(0f, -10f);
			this.overridefly = true;
			this.overrideflip = true;
			this.overrideheight = true;
			do
			{
				this.spritetransform.position = Vector3.Lerp(ss, st, b / a);
				b += MainManager.framestep;
				yield return null;
			}
			while (this.sprite != null && b < a + 1f);
			ss = default(Vector3);
			st = default(Vector3);
			if (this.sprite != null)
			{
				this.sprite.gameObject.SetActive(false);
			}
			break;
		}
		case NPCControl.DeathType.ExplodeAnim:
			MainManager.PlayParticle("explosionsmall", this.transform.position);
			MainManager.PlaySound("Explosion", -1, 1.1f, 0.75f);
			this.sprite.enabled = false;
			MainManager.ShakeScreen(0.1f, 0.5f, true);
			break;
		case NPCControl.DeathType.DropSprites:
		{
			this.overrideanim = true;
			this.anim.enabled = false;
			yield return null;
			Transform[] array = (this.model != null) ? this.model.GetComponentsInChildren<Transform>() : this.sprite.GetComponentsInChildren<Transform>();
			for (int l = 1; l < array.Length; l++)
			{
				Rigidbody rigidbody = array[l].gameObject.AddComponent<Rigidbody>();
				rigidbody.transform.parent = null;
				rigidbody.useGravity = true;
				rigidbody.velocity = MainManager.RandomItemBounce(2.5f, 12f);
				Object.Destroy(rigidbody.gameObject, 1f);
			}
			yield return EventControl.sec;
			break;
		}
		}
		while (MainManager.instance.pause || (!this.battle && MainManager.battle != null))
		{
			yield return null;
		}
		if (this.npcdata == null || this.npcdata.entitytype != NPCControl.NPCType.Enemy || this.npcdata.eventid <= 0)
		{
			if (this.spitmoney > 0)
			{
				for (int m = 0; m < this.spitmoney; m++)
				{
					Vector3 vector = MainManager.RandomItemBounce(4f, 10f);
					NPCControl npccontrol = EntityControl.CreateItem(sp + Vector3.up * 0.5f, 0, (this.spitmoney - m > 20) ? 186 : ((this.spitmoney - m > 5) ? 7 : 6), vector, 600);
					npccontrol.entity.TempIgnoreColision(this.ccol, 5f);
					npccontrol.entity.LateVelocity(vector);
					if (this.spitmoney - m > 20)
					{
						m += 19;
					}
					else if (this.spitmoney - m > 5)
					{
						m += 4;
					}
				}
			}
			if (this.npcdata != null && this.npcdata.entitytype == NPCControl.NPCType.Enemy && this.npcdata.vectordata != null && this.npcdata.vectordata.Length != 0 && !this.npcdata.HasBehavior(NPCControl.ActionBehaviors.SetPath) && !this.npcdata.HasBehavior(NPCControl.ActionBehaviors.SetPathJump))
			{
				int num3 = MainManager.BadgeHowManyEquipped(84);
				if (num3 > 0 && (this.spitmoney > 0 || !MainManager.BadgeIsEquipped(18)))
				{
					for (int n = 0; n < 2; n++)
					{
						int num4 = Random.Range(0, num3 + 3);
						if (num4 > 0)
						{
							for (int num5 = 0; num5 < num4; num5++)
							{
								GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Objects/Pip" + n)) as GameObject;
								gameObject.transform.position = sp + Vector3.up * 0.5f;
								gameObject.transform.parent = MainManager.map.transform;
							}
						}
					}
				}
				int num6 = Random.Range(MainManager.BadgeIsEquipped(18) ? -7 : ((MainManager.BadgeIsEquipped(11) || MainManager.instance.flags[614]) ? -1 : -3), this.npcdata.vectordata.Length);
				for (int num7 = 0; num7 < this.npcdata.vectordata.Length; num7++)
				{
					if ((int)this.npcdata.vectordata[num7].y == -2)
					{
						num6 = num7;
					}
				}
				for (int num8 = MainManager.instance.lastdefeated.Count - 1; num8 >= 0; num8--)
				{
					for (int num9 = 0; num9 < EntityControl.specialenemy[1].Length; num9++)
					{
						if (MainManager.instance.lastdefeated[num8] == EntityControl.specialenemy[1][num9])
						{
							if (Random.Range(0, 100) < EntityControl.specialenemy[0][num9])
							{
								Vector3 vector2 = MainManager.RandomItemBounce(4f, 10f);
								NPCControl npccontrol2 = EntityControl.CreateItem(sp + Vector3.up * 0.5f, 0, EntityControl.recipepool[num9][Random.Range(0, EntityControl.recipepool[num9].Length)], vector2, 600);
								npccontrol2.entity.TempIgnoreColision(this.ccol, 5f);
								npccontrol2.entity.LateVelocity(vector2);
							}
							MainManager.instance.lastdefeated.RemoveAt(num8);
							break;
						}
					}
				}
				MainManager.instance.lastdefeated = new List<int>();
				if (num6 >= 0 && ((int)this.npcdata.vectordata[num6].y <= 0 || MainManager.instance.flags[(int)this.npcdata.vectordata[num6].y]))
				{
					if ((int)this.npcdata.vectordata[num6].y == -2)
					{
						Vector3 vector3 = MainManager.RandomItemBounce(4f, 10f);
						NPCControl npccontrol3 = EntityControl.CreateItem(sp + Vector3.up * 0.5f, 1, (int)this.npcdata.vectordata[num6].x, vector3, -1);
						npccontrol3.activationflag = this.npcdata.limit[0];
						npccontrol3.entity.TempIgnoreColision(this.ccol, 5f);
						npccontrol3.entity.LateVelocity(vector3);
					}
					else
					{
						Vector3 vector4 = MainManager.RandomItemBounce(4f, 10f);
						NPCControl npccontrol4 = EntityControl.CreateItem(sp + Vector3.up * 0.5f, 0, (int)this.npcdata.vectordata[num6].x, vector4, 600);
						npccontrol4.entity.TempIgnoreColision(this.ccol, 5f);
						npccontrol4.entity.LateVelocity(vector4);
					}
				}
			}
		}
		yield return null;
		if (this == null)
		{
			yield break;
		}
		if (this.destroytype != NPCControl.DeathType.KO && this.destroytype != NPCControl.DeathType.SpinKO && this.destroytype != NPCControl.DeathType.None)
		{
			this.transform.position = new Vector3(0f, 9999f);
			if (MainManager.battle != null)
			{
				Object.Destroy(base.gameObject, 1f);
			}
		}
		this.ccol.enabled = false;
		IL_F83:
		yield return null;
		if (this.originalid == 331)
		{
			for (int num10 = 0; num10 < this.extras.Length; num10++)
			{
				Object.Destroy(this.extras[num10]);
			}
			Object.Destroy(this.sprite.GetComponent<MidPos>());
			this.extras = null;
		}
		if (activatekill)
		{
			this.iskill = true;
		}
		if (this.spitexp > 0)
		{
			int bigexp = Mathf.FloorToInt((float)(this.spitexp / 10));
			this.spitexp -= bigexp * 10;
			int num11;
			for (int i = 0; i < this.spitexp; i = num11 + 1)
			{
				GameObject gameObject2 = Object.Instantiate(Resources.Load("Prefabs/Objects/ExpOrb"), sp, Quaternion.identity) as GameObject;
				if (bigexp > 0)
				{
					num11 = bigexp;
					bigexp = num11 - 1;
					gameObject2.transform.localScale = Vector3.one * 0.5f;
				}
				else
				{
					gameObject2.transform.localScale = Vector3.one * 0.25f;
				}
				gameObject2.AddComponent<Rigidbody>().velocity = new Vector3((float)Random.Range(-4, 4), 15f, (float)Random.Range(-4, 4));
				Object.Destroy(gameObject2, 0.75f);
				yield return new WaitForSeconds(0.05f);
				num11 = i;
			}
		}
		this.deathcoroutine = null;
		yield break;
	}

	// Token: 0x06000252 RID: 594 RVA: 0x000205C7 File Offset: 0x0001E7C7
	public void SetAnimForce()
	{
		this.SetAnim("", true);
	}

	// Token: 0x06000253 RID: 595 RVA: 0x000205D8 File Offset: 0x0001E7D8
	private void RandomAnimationEvent(int chance)
	{
		if (Random.Range(0, 100) <= chance)
		{
			MainManager.AnimIDs animIDs = this.animid + MainManager.AnimIDs.Bee;
			if (animIDs == MainManager.AnimIDs.CordycepsAnt)
			{
				this.anim.Play("Idle" + Random.Range(0, 2));
			}
		}
	}

	// Token: 0x06000254 RID: 596 RVA: 0x0002061F File Offset: 0x0001E81F
	public void ReturnToIdle()
	{
		this.animstate = this.basestate;
	}

	// Token: 0x06000255 RID: 597 RVA: 0x0002062D File Offset: 0x0001E82D
	public static EntityControl CreateNewEntity(string name, int anim_id, Vector3 position)
	{
		return EntityControl.CreateNewEntity(name, anim_id, position, null);
	}

	// Token: 0x06000256 RID: 598 RVA: 0x00020638 File Offset: 0x0001E838
	public void ChangeAnimIfNotBattle(float id)
	{
		if (!this.battle)
		{
			this.animstate = (int)id;
		}
	}

	// Token: 0x06000257 RID: 599 RVA: 0x0002064A File Offset: 0x0001E84A
	public void Unfix()
	{
		this.Unfix(false);
	}

	// Token: 0x06000258 RID: 600 RVA: 0x00020654 File Offset: 0x0001E854
	public void Unfix(bool force)
	{
		if (!force && this.npcdata != null && (this.npcdata.entitytype == NPCControl.NPCType.Object || this.npcdata.entitytype == NPCControl.NPCType.SemiNPC || this.npcdata.interacttype == NPCControl.Interaction.CaravanBadge || this.npcdata.interacttype == NPCControl.Interaction.Shop))
		{
			return;
		}
		this.fixedentity = false;
		this.rigid.constraints = RigidbodyConstraints.FreezeRotation;
		this.LockRigid(false, false);
		this.ccol.enabled = true;
	}

	// Token: 0x06000259 RID: 601 RVA: 0x000206D4 File Offset: 0x0001E8D4
	public static EntityControl CreateNewEntity(string name, int anim_id, Vector3 position, EntityControl follow)
	{
		EntityControl entityControl = EntityControl.CreateNewEntity(name);
		entityControl.animid = anim_id;
		entityControl.transform.position = position;
		if (follow != null)
		{
			int num = 1;
			EntityControl entityControl2 = follow.following;
			if (entityControl2 != null)
			{
				while (entityControl2.following != null)
				{
					num++;
					entityControl2 = entityControl2.following;
				}
			}
			entityControl.following = follow;
			entityControl.followoffset = (float)num * 0.1f;
			entityControl.gameObject.layer = 9;
			entityControl.tag = "PFollower";
		}
		return entityControl;
	}

	// Token: 0x0600025A RID: 602 RVA: 0x0002075F File Offset: 0x0001E95F
	public void SetOverrides(bool animation, bool jumpanimation, bool flipbehavior, bool onlyflip, bool flyanimation, bool animationspeed)
	{
		this.overrideanim = animation;
		this.overrridejump = jumpanimation;
		this.overrideflip = flipbehavior;
		this.overridefly = flyanimation;
		this.overrideonlyflip = onlyflip;
		this.overrideanimspeed = animationspeed;
	}

	// Token: 0x0600025B RID: 603 RVA: 0x00020790 File Offset: 0x0001E990
	public static EntityControl CreateNewEntity(string name)
	{
		EntityControl entityControl = new GameObject(name).AddComponent<EntityControl>();
		entityControl.transform = entityControl.GetComponent<Transform>();
		new GameObject("Rotater").transform.parent = entityControl.transform;
		entityControl.sprite = new GameObject("Sprite").AddComponent<SpriteRenderer>();
		entityControl.spritetransform = entityControl.sprite.transform;
		entityControl.spritetransform.parent = entityControl.transform.GetChild(0);
		entityControl.moverotater = new GameObject("MoveRotater").transform;
		entityControl.moverotater.transform.parent = entityControl.transform;
		CapsuleCollider capsuleCollider = entityControl.gameObject.AddComponent<CapsuleCollider>();
		capsuleCollider.radius = 0.5f;
		capsuleCollider.height = 2f;
		capsuleCollider.center = new Vector3(0f, 1f, 0f);
		entityControl.gameObject.layer = 10;
		entityControl.ccol = capsuleCollider;
		return entityControl;
	}

	// Token: 0x0600025C RID: 604 RVA: 0x00020888 File Offset: 0x0001EA88
	public IEnumerator BounceAnim(float squashammount, float time, float squashspeed, bool gradual)
	{
		float t = 1f;
		float t2 = 1f;
		float initialtime = time;
		float initialspeed = squashspeed;
		Vector3 originalscale = this.spritetransform.localScale;
		bool increase = false;
		while (time > 0f)
		{
			this.spritetransform.localScale = new Vector3(t, t2, t);
			if (!increase)
			{
				t = Mathf.Lerp(t, squashammount + 0.1f, MainManager.framestep * squashspeed);
				t2 = Mathf.Lerp(t2, squashammount / 2f, MainManager.framestep * squashspeed);
				if (t >= squashammount)
				{
					increase = true;
				}
			}
			else
			{
				t = Mathf.Lerp(t, squashammount / 2f, MainManager.framestep * squashspeed);
				t2 = Mathf.Lerp(t2, squashammount, MainManager.framestep * squashspeed);
				if (t <= squashammount / 2f + 0.075f)
				{
					increase = false;
				}
			}
			if (gradual)
			{
				squashspeed = Mathf.Lerp(initialspeed, 0f, 1f - MainManager.framestep * (time / initialtime));
			}
			time -= MainManager.framestep;
			yield return null;
		}
		t = 0f;
		while (t < 1f)
		{
			this.spritetransform.localScale = Vector3.Lerp(this.spritetransform.localScale, originalscale, Mathf.Clamp01(t));
			t += MainManager.framestep * squashammount;
			yield return null;
		}
		this.spritetransform.localScale = originalscale;
		this.bounceanim = null;
		yield break;
	}

	// Token: 0x0600025D RID: 605 RVA: 0x000208B4 File Offset: 0x0001EAB4
	private bool CheckForCharacterEntity()
	{
		return base.CompareTag("Follower") || this.isplayer || base.CompareTag("NPC") || base.CompareTag("Enemy") || base.CompareTag("PFollower") || (this.npcdata != null && this.npcdata.objecttype == NPCControl.ObjectTypes.PushRock) || (this.npcdata != null && this.npcdata.objecttype == NPCControl.ObjectTypes.Item);
	}

	// Token: 0x0600025E RID: 606 RVA: 0x00020939 File Offset: 0x0001EB39
	public IEnumerator LateVelocity(Vector3 ammount, float delay, float onlyifmagnitude, bool ignorey)
	{
		base.StartCoroutine(this.LateVelocity(ammount, delay, onlyifmagnitude, ignorey, 0));
		yield return null;
		yield break;
	}

	// Token: 0x0600025F RID: 607 RVA: 0x00020965 File Offset: 0x0001EB65
	public IEnumerator LateVelocity(Vector3 ammount, float delay, float onlyifmagnitude, bool ignorey, int continuous)
	{
		while (this.rigid == null)
		{
			yield return null;
		}
		if (delay <= 0f)
		{
			yield return null;
		}
		else
		{
			yield return new WaitForSeconds(delay);
		}
		if (continuous == 0)
		{
			continuous = 1;
		}
		for (int i = continuous; i > 0; i--)
		{
			if ((ignorey ? new Vector2(this.rigid.velocity.x, this.rigid.velocity.z).magnitude : this.rigid.velocity.magnitude) < onlyifmagnitude)
			{
				this.rigid.velocity = ammount;
			}
		}
		yield return null;
		yield break;
	}

	// Token: 0x06000260 RID: 608 RVA: 0x00020999 File Offset: 0x0001EB99
	public IEnumerator LateVelocity(Vector3 ammount, float delay)
	{
		while (this.rigid == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(delay);
		this.rigid.velocity = ammount;
		yield break;
	}

	// Token: 0x06000261 RID: 609 RVA: 0x000209B6 File Offset: 0x0001EBB6
	public IEnumerator LateVelocity(Vector3 ammount)
	{
		while (this.rigid == null)
		{
			yield return null;
		}
		yield return null;
		this.rigid.velocity = ammount;
		yield break;
	}

	// Token: 0x06000262 RID: 610 RVA: 0x000209CC File Offset: 0x0001EBCC
	public IEnumerator ZaspWarp(bool appear, Vector3 pos)
	{
		this.transform.position = pos;
		yield return null;
		this.specialanim = base.StartCoroutine(this.ZaspWarp(appear));
		yield return null;
		yield break;
	}

	// Token: 0x06000263 RID: 611 RVA: 0x000209E9 File Offset: 0x0001EBE9
	public IEnumerator ZaspWarp(bool appear)
	{
		float a = 0f;
		this.animstate = 106;
		this.overrideanimspeed = true;
		this.anim.speed = 0f;
		SpriteRenderer[] ts = new SpriteRenderer[2];
		for (int i = 0; i < ts.Length; i++)
		{
			ts[i] = new GameObject("tsprite").AddComponent<SpriteRenderer>();
			ts[i].sprite = this.sprite.sprite;
			ts[i].color = new Color(1f, 1f, 1f, 0.35f);
			ts[i].transform.parent = this.spritetransform;
			ts[i].transform.localEulerAngles = Vector3.zero;
			ts[i].transform.localPosition = Vector3.zero;
			ts[i].enabled = false;
		}
		if (!appear)
		{
			this.PlaySound("Buzz1");
			this.LockRigid(true);
			this.overrideanim = true;
			while (a < 30f)
			{
				this.animstate = 109;
				this.sprite.enabled = !this.sprite.enabled;
				for (int j = 0; j < ts.Length; j++)
				{
					ts[j].enabled = !this.sprite.enabled;
					ts[j].sprite = this.sprite.sprite;
					if (ts[j].enabled)
					{
						ts[j].transform.localPosition = new Vector3(Random.Range(-1.25f, 1.25f) * (a / 30f), 0f);
					}
				}
				a += MainManager.framestep;
				yield return null;
			}
			this.sprite.enabled = false;
		}
		else
		{
			this.sprite.enabled = true;
			for (int k = 0; k < ts.Length; k++)
			{
				ts[k].enabled = true;
				ts[k].transform.localPosition = new Vector3(Random.Range(-1.25f, 1.25f), 0f);
			}
			a = 0f;
			MainManager.PlaySound("Buzz2");
			while (a < 20f)
			{
				this.animstate = 109;
				this.sprite.enabled = !this.sprite.enabled;
				for (int l = 0; l < ts.Length; l++)
				{
					ts[l].enabled = !this.sprite.enabled;
					ts[l].sprite = this.sprite.sprite;
					ts[l].transform.localPosition = new Vector3(Random.Range(-1.25f, 1.25f) * (1f - a / 20f), 0f);
				}
				a += MainManager.framestep;
				yield return null;
			}
			this.sprite.enabled = true;
		}
		this.overrideanimspeed = false;
		this.anim.speed = 1f;
		for (int m = 0; m < ts.Length; m++)
		{
			Object.Destroy(ts[m].gameObject);
		}
		this.specialanim = null;
		yield break;
	}

	// Token: 0x06000264 RID: 612 RVA: 0x00020A00 File Offset: 0x0001EC00
	public static NPCControl CreateItem(Vector3 startpos, int itemtype, int itemid, Vector3 direction, int timer)
	{
		NPCControl npccontrol = EntityControl.CreateNewEntity("tempitem").gameObject.AddComponent<NPCControl>();
		npccontrol.objecttype = NPCControl.ObjectTypes.Item;
		npccontrol.entitytype = NPCControl.NPCType.Object;
		npccontrol.entity = npccontrol.GetComponent<EntityControl>();
		npccontrol.entity.item = true;
		npccontrol.entity.animid = itemtype;
		npccontrol.entity.animstate = itemid;
		npccontrol.entity.itemstate = itemid;
		npccontrol.entity.basestate = itemid;
		npccontrol.touchcooldown = 30f;
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			Physics.IgnoreCollision(npccontrol.entity.ccol, MainManager.instance.playerdata[i].entity.ccol, true);
		}
		npccontrol.transform.position = startpos;
		npccontrol.entity.startpos = new Vector3?(startpos);
		npccontrol.transform.parent = MainManager.map.transform;
		npccontrol.insideid = MainManager.instance.insideid;
		npccontrol.timer = (float)timer;
		npccontrol.tempobject = true;
		npccontrol.data[0] = itemtype;
		npccontrol.bounces = 0;
		npccontrol.entity.onground = false;
		npccontrol.entity.startvelocity = new Vector3?(direction);
		npccontrol.entity.onground = false;
		if (itemtype == 3)
		{
			npccontrol.data[0] = itemid;
			npccontrol.entity.animid = 43;
		}
		return npccontrol;
	}

	// Token: 0x06000265 RID: 613 RVA: 0x00020B6C File Offset: 0x0001ED6C
	private void OnTriggerStay(Collider other)
	{
		if (Time.frameCount % 2 == 0)
		{
			if (!this.item && other.CompareTag("FlowerBed"))
			{
				if (this.flowerbed == null)
				{
					this.flowerbed = (Object.Instantiate(Resources.Load("Prefabs/Particles/flowerbed")) as GameObject);
					this.flowerbed.transform.parent = this.transform;
					this.flowerbed.transform.localPosition = new Vector3(0f, 0.65f, 0f);
					return;
				}
			}
			else if (other.CompareTag("IceRadius"))
			{
				this.inice = true;
			}
		}
	}

	// Token: 0x06000266 RID: 614 RVA: 0x00020C14 File Offset: 0x0001EE14
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("FlowerBed"))
		{
			if (this.flowerbed != null)
			{
				Object.Destroy(this.flowerbed.gameObject);
				return;
			}
		}
		else if (other.CompareTag("IceRadius"))
		{
			this.inice = false;
		}
	}

	// Token: 0x04000184 RID: 388
	public bool ignorewater;

	// Token: 0x04000185 RID: 389
	public bool fixedentity;

	// Token: 0x04000186 RID: 390
	public bool forcemove;

	// Token: 0x04000187 RID: 391
	public bool flip;

	// Token: 0x04000188 RID: 392
	public bool overrideanim;

	// Token: 0x04000189 RID: 393
	public bool overrridejump;

	// Token: 0x0400018A RID: 394
	public bool hasshadow;

	// Token: 0x0400018B RID: 395
	public bool iskill;

	// Token: 0x0400018C RID: 396
	public bool dead;

	// Token: 0x0400018D RID: 397
	public bool item;

	// Token: 0x0400018E RID: 398
	public bool incamera;

	// Token: 0x0400018F RID: 399
	public bool alwaysactive;

	// Token: 0x04000190 RID: 400
	public bool tempfollower;

	// Token: 0x04000191 RID: 401
	public bool shakeice;

	// Token: 0x04000192 RID: 402
	public bool overrideflip;

	// Token: 0x04000193 RID: 403
	public bool overridefollow;

	// Token: 0x04000194 RID: 404
	public bool overrideonlyflip;

	// Token: 0x04000195 RID: 405
	public bool oldground;

	// Token: 0x04000196 RID: 406
	public bool hologram;

	// Token: 0x04000197 RID: 407
	public bool diganim;

	// Token: 0x04000198 RID: 408
	public bool overrideheight;

	// Token: 0x04000199 RID: 409
	public bool overrideminheight;

	// Token: 0x0400019A RID: 410
	public bool overridemovesmoke;

	// Token: 0x0400019B RID: 411
	public bool notalk;

	// Token: 0x0400019C RID: 412
	public bool overrideanimfunc;

	// Token: 0x0400019D RID: 413
	public bool alwaysemoticon;

	// Token: 0x0400019E RID: 414
	public bool lockrotater;

	// Token: 0x0400019F RID: 415
	public bool cotunknown;

	// Token: 0x040001A0 RID: 416
	public bool refreshedcotu;

	// Token: 0x040001A1 RID: 417
	public bool tempheightoverride;

	// Token: 0x040001A2 RID: 418
	public bool forcefire;

	// Token: 0x040001A3 RID: 419
	public bool soundfix;

	// Token: 0x040001A4 RID: 420
	public bool activeinevents;

	// Token: 0x040001A5 RID: 421
	public bool extratimer;

	// Token: 0x040001A6 RID: 422
	public bool playerentity;

	// Token: 0x040001A7 RID: 423
	public bool nocondition;

	// Token: 0x040001A8 RID: 424
	private EntityControl.WalkType walktype;

	// Token: 0x040001A9 RID: 425
	private bool shakeondrop;

	// Token: 0x040001AA RID: 426
	private bool fixedcol;

	// Token: 0x040001AB RID: 427
	private bool isfollower;

	// Token: 0x040001AC RID: 428
	private bool leiffly;

	// Token: 0x040001AD RID: 429
	public Vector3 modelscale;

	// Token: 0x040001AE RID: 430
	public Vector3 spin;

	// Token: 0x040001AF RID: 431
	public Vector3 startscale;

	// Token: 0x040001B0 RID: 432
	public Vector3 truescale;

	// Token: 0x040001B1 RID: 433
	public Vector3 emoticonoffset = new Vector3(0f, 1.8f, -0.1f);

	// Token: 0x040001B2 RID: 434
	public Vector3 forcetarget;

	// Token: 0x040001B3 RID: 435
	public Vector3 oldpos;

	// Token: 0x040001B4 RID: 436
	public Vector3 extraoffset;

	// Token: 0x040001B5 RID: 437
	public Vector3 freezesize;

	// Token: 0x040001B6 RID: 438
	public Vector3 freezeoffset;

	// Token: 0x040001B7 RID: 439
	public Vector3 campos;

	// Token: 0x040001B8 RID: 440
	public Vector3 lastpos;

	// Token: 0x040001B9 RID: 441
	public Vector3 digscale = Vector3.one;

	// Token: 0x040001BA RID: 442
	public Vector3[] spinextra;

	// Token: 0x040001BB RID: 443
	public Vector3? startpos;

	// Token: 0x040001BC RID: 444
	public SpriteRenderer sprite;

	// Token: 0x040001BD RID: 445
	public SpriteRenderer shadow;

	// Token: 0x040001BE RID: 446
	public SpriteRenderer emoticonsprite;

	// Token: 0x040001BF RID: 447
	public Color spritebasecolor = Color.white;

	// Token: 0x040001C0 RID: 448
	public Rigidbody rigid;

	// Token: 0x040001C1 RID: 449
	public Coroutine deathcoroutine;

	// Token: 0x040001C2 RID: 450
	public Coroutine droproutine;

	// Token: 0x040001C3 RID: 451
	public Coroutine bounceanim;

	// Token: 0x040001C4 RID: 452
	public Coroutine specialanim;

	// Token: 0x040001C5 RID: 453
	public Coroutine forcemoving;

	// Token: 0x040001C6 RID: 454
	public EntityControl following;

	// Token: 0x040001C7 RID: 455
	public Animator anim;

	// Token: 0x040001C8 RID: 456
	public Animator emoticon;

	// Token: 0x040001C9 RID: 457
	public BoxCollider detect;

	// Token: 0x040001CA RID: 458
	public CapsuleCollider ccol;

	// Token: 0x040001CB RID: 459
	public NPCControl npcdata;

	// Token: 0x040001CC RID: 460
	public Transform rotater;

	// Token: 0x040001CD RID: 461
	public Transform model;

	// Token: 0x040001CE RID: 462
	public Transform moverotater;

	// Token: 0x040001CF RID: 463
	public Transform movesmoke;

	// Token: 0x040001D0 RID: 464
	public Transform hpbar;

	// Token: 0x040001D1 RID: 465
	public Transform spritetransform;

	// Token: 0x040001D2 RID: 466
	public Transform shadowtransform;

	// Token: 0x040001D3 RID: 467
	private GameObject[] animspecific;

	// Token: 0x040001D4 RID: 468
	private LineRenderer[] extralines;

	// Token: 0x040001D5 RID: 469
	public Animator[] extraanims;

	// Token: 0x040001D6 RID: 470
	[HideInInspector]
	public SpriteRenderer[] extrasprites;

	// Token: 0x040001D7 RID: 471
	public GroundDetector feet;

	// Token: 0x040001D8 RID: 472
	public GameObject icecube;

	// Token: 0x040001D9 RID: 473
	public NPCControl.DeathType destroytype;

	// Token: 0x040001DA RID: 474
	public Transform followedby;

	// Token: 0x040001DB RID: 475
	public Transform firepart;

	// Token: 0x040001DC RID: 476
	public DynamicFont hpbarfont;

	// Token: 0x040001DD RID: 477
	public DynamicFont defstat;

	// Token: 0x040001DE RID: 478
	public Transform originalmap;

	// Token: 0x040001DF RID: 479
	public new Transform transform;

	// Token: 0x040001E0 RID: 480
	public AudioSource sound;

	// Token: 0x040001E1 RID: 481
	public LineRenderer line;

	// Token: 0x040001E2 RID: 482
	public float[] speedbuffer;

	// Token: 0x040001E3 RID: 483
	public int itemstate;

	// Token: 0x040001E4 RID: 484
	public int animstate;

	// Token: 0x040001E5 RID: 485
	public int animid;

	// Token: 0x040001E6 RID: 486
	public int oldstate = -1;

	// Token: 0x040001E7 RID: 487
	public int oldid = -1;

	// Token: 0x040001E8 RID: 488
	public int forceanim;

	// Token: 0x040001E9 RID: 489
	public int forcestop;

	// Token: 0x040001EA RID: 490
	public int emoticonid = -1;

	// Token: 0x040001EB RID: 491
	public int spitexp;

	// Token: 0x040001EC RID: 492
	public int basestate;

	// Token: 0x040001ED RID: 493
	public int battleid = -1;

	// Token: 0x040001EE RID: 494
	public int spitmoney;

	// Token: 0x040001EF RID: 495
	public int rainbowoffset;

	// Token: 0x040001F0 RID: 496
	public int dialoguebleepid;

	// Token: 0x040001F1 RID: 497
	public int originalid;

	// Token: 0x040001F2 RID: 498
	public int walkstate = 1;

	// Token: 0x040001F3 RID: 499
	public int tempfollowerid;

	// Token: 0x040001F4 RID: 500
	public float minheight;

	// Token: 0x040001F5 RID: 501
	public float digtime;

	// Token: 0x040001F6 RID: 502
	public float speed = 5f;

	// Token: 0x040001F7 RID: 503
	public float jumpheight = 10f;

	// Token: 0x040001F8 RID: 504
	public float height;

	// Token: 0x040001F9 RID: 505
	public float bobspeed;

	// Token: 0x040001FA RID: 506
	public float bobrange;

	// Token: 0x040001FB RID: 507
	public float flipspeed = 0.2f;

	// Token: 0x040001FC RID: 508
	public float animspeed;

	// Token: 0x040001FD RID: 509
	public float icooldown;

	// Token: 0x040001FE RID: 510
	public float forcemultiplier;

	// Token: 0x040001FF RID: 511
	public float followlimit = 20f;

	// Token: 0x04000200 RID: 512
	public float followoffset;

	// Token: 0x04000201 RID: 513
	public float followdistance = 2f;

	// Token: 0x04000202 RID: 514
	public float emoticoncooldown;

	// Token: 0x04000203 RID: 515
	public float followjump = 3f;

	// Token: 0x04000204 RID: 516
	public float initialheight;

	// Token: 0x04000205 RID: 517
	public float bleeppitch;

	// Token: 0x04000206 RID: 518
	public float soundidstance = 25f;

	// Token: 0x04000207 RID: 519
	public float startbs;

	// Token: 0x04000208 RID: 520
	public float startbf;

	// Token: 0x04000209 RID: 521
	public float jumpcooldown;

	// Token: 0x0400020A RID: 522
	public float camdistance;

	// Token: 0x0400020B RID: 523
	public float shadowsize = 1f;

	// Token: 0x0400020C RID: 524
	public float offgroundframes;

	// Token: 0x0400020D RID: 525
	public bool inice;

	// Token: 0x0400020E RID: 526
	public bool mainparty;

	// Token: 0x0400020F RID: 527
	public bool onground = true;

	// Token: 0x04000210 RID: 528
	public bool hitwall;

	// Token: 0x04000211 RID: 529
	public bool battle;

	// Token: 0x04000212 RID: 530
	public bool ignorey;

	// Token: 0x04000213 RID: 531
	public bool lastgravity;

	// Token: 0x04000214 RID: 532
	public bool noemoticon;

	// Token: 0x04000215 RID: 533
	public bool backsprite;

	// Token: 0x04000216 RID: 534
	public bool oldback;

	// Token: 0x04000217 RID: 535
	public bool setup;

	// Token: 0x04000218 RID: 536
	public bool lockback;

	// Token: 0x04000219 RID: 537
	public bool usebuffer;

	// Token: 0x0400021A RID: 538
	public bool bufferjump;

	// Token: 0x0400021B RID: 539
	public bool talking;

	// Token: 0x0400021C RID: 540
	public bool oldtalk;

	// Token: 0x0400021D RID: 541
	public bool flyinganim;

	// Token: 0x0400021E RID: 542
	public bool oldfly;

	// Token: 0x0400021F RID: 543
	public bool changedstate;

	// Token: 0x04000220 RID: 544
	public bool overridefly;

	// Token: 0x04000221 RID: 545
	public bool springcooldown;

	// Token: 0x04000222 RID: 546
	public bool overrideanimspeed;

	// Token: 0x04000223 RID: 547
	public bool trail;

	// Token: 0x04000224 RID: 548
	public bool activeonpause;

	// Token: 0x04000225 RID: 549
	public bool overrideshadow;

	// Token: 0x04000226 RID: 550
	public bool shrink;

	// Token: 0x04000227 RID: 551
	public bool forcejump;

	// Token: 0x04000228 RID: 552
	public bool noclock;

	// Token: 0x04000229 RID: 553
	public bool digging;

	// Token: 0x0400022A RID: 554
	public bool shieldenabled;

	// Token: 0x0400022B RID: 555
	public bool nodigpart;

	// Token: 0x0400022C RID: 556
	public bool killonfall;

	// Token: 0x0400022D RID: 557
	public bool stopspinonground;

	// Token: 0x0400022E RID: 558
	public bool hideinside;

	// Token: 0x0400022F RID: 559
	public bool soundonpause;

	// Token: 0x04000230 RID: 560
	public bool disabletimer;

	// Token: 0x04000231 RID: 561
	public bool alwaysflip;

	// Token: 0x04000232 RID: 562
	public bool showitem;

	// Token: 0x04000233 RID: 563
	public Vector3? startvelocity;

	// Token: 0x04000234 RID: 564
	public Vector3? overrideshieldpos;

	// Token: 0x04000235 RID: 565
	public Vector3? pausepos;

	// Token: 0x04000236 RID: 566
	public Vector2 initialcolliderdata;

	// Token: 0x04000237 RID: 567
	public Vector3 spawnpoint;

	// Token: 0x04000238 RID: 568
	private Vector2 deltavelocity;

	// Token: 0x04000239 RID: 569
	private Vector3? lastvelocity;

	// Token: 0x0400023A RID: 570
	private Vector3? looktowards;

	// Token: 0x0400023B RID: 571
	private Vector3 initialcenter;

	// Token: 0x0400023C RID: 572
	private Vector3 initialfrezeoffset;

	// Token: 0x0400023D RID: 573
	private Vector3 lastshadow;

	// Token: 0x0400023E RID: 574
	public Transform[] statusicons;

	// Token: 0x0400023F RID: 575
	private string laststate;

	// Token: 0x04000240 RID: 576
	private float forcetimer;

	// Token: 0x04000241 RID: 577
	private int statusid;

	// Token: 0x04000242 RID: 578
	private int camcd;

	// Token: 0x04000243 RID: 579
	public GameObject flowerbed;

	// Token: 0x04000244 RID: 580
	public DialogueAnim bubbleshield;

	// Token: 0x04000245 RID: 581
	public GameObject[] digpart;

	// Token: 0x04000246 RID: 582
	public GameObject[] extras;

	// Token: 0x04000247 RID: 583
	public EntityControl[] subentity;

	// Token: 0x04000248 RID: 584
	private static Color cotcolor = new Color(0f, 0f, 0f, 1f);

	// Token: 0x04000249 RID: 585
	private static Color cot3d = new Color(0f, 0f, 0f, 0.5f);

	// Token: 0x0400024A RID: 586
	private static readonly int[][] recipepool = new int[][]
	{
		new int[]
		{
			77
		},
		new int[]
		{
			35,
			75,
			147,
			82,
			74,
			144,
			73
		},
		new int[]
		{
			10,
			20,
			19,
			69,
			65,
			129,
			34,
			36,
			46,
			16,
			53,
			33,
			47,
			67
		}
	};

	// Token: 0x0400024B RID: 587
	private static readonly int[][] specialenemy = new int[][]
	{
		new int[]
		{
			100,
			50,
			40
		},
		new int[]
		{
			32,
			70,
			81
		}
	};

	// Token: 0x0400024C RID: 588
	public const float defaultforcetime = 500f;

	// Token: 0x0400024D RID: 589
	private bool instdig;

	// Token: 0x0400024E RID: 590
	private bool hasiceanim;

	// Token: 0x0400024F RID: 591
	private bool lastice;

	// Token: 0x04000250 RID: 592
	private bool nofallfrozen;

	// Token: 0x04000251 RID: 593
	private bool nomodel;

	// Token: 0x04000252 RID: 594
	private bool isplayer;

	// Token: 0x04000253 RID: 595
	private bool mapentity;

	// Token: 0x04000254 RID: 596
	private Sprite[] preloadedsprites;

	// Token: 0x04000255 RID: 597
	private GameObject[] preloadedobjects;

	// Token: 0x04000256 RID: 598
	[HideInInspector]
	public Vector3 latepos;

	// Token: 0x04000257 RID: 599
	[HideInInspector]
	public Transform latetrans;

	// Token: 0x04000258 RID: 600
	private EntityControl.TrailData traildata;

	// Token: 0x04000259 RID: 601
	private static GameObject icecubeprefab;

	// Token: 0x0400025A RID: 602
	private float statuscooldown;

	// Token: 0x0400025B RID: 603
	private float soundvolume;

	// Token: 0x0400025C RID: 604
	private float lastvolume;

	// Token: 0x0400025D RID: 605
	private const float groundfix = 0.01f;

	// Token: 0x0400025E RID: 606
	private const float raydivider = 3.5f;

	// Token: 0x0400025F RID: 607
	private const float backoffset = -0.5f;

	// Token: 0x04000260 RID: 608
	private const float jumpdetect = 0.75f;

	// Token: 0x04000261 RID: 609
	private const float detectoffset = 0.3f;

	// Token: 0x04000262 RID: 610
	private const float jumpoffset = 6f;

	// Token: 0x04000263 RID: 611
	private const float jumptest = 2.5f;

	// Token: 0x04000264 RID: 612
	private const float conditionx = 0.5f;

	// Token: 0x04000265 RID: 613
	private const float conditiony = 1f;

	// Token: 0x04000266 RID: 614
	public const float forcetolerance = 0.3f;

	// Token: 0x04000267 RID: 615
	public const float shadowammount = 0.4f;

	// Token: 0x04000268 RID: 616
	public const float digammount = 30f;

	// Token: 0x04000269 RID: 617
	public const float groundtolerance = 0.05f;

	// Token: 0x0400026A RID: 618
	private const int camupdate = 3;

	// Token: 0x0400026B RID: 619
	private static readonly string[] emoticonIDs = new string[]
	{
		"0",
		"1",
		"2",
		"3",
		"4",
		"5",
		"6"
	};

	// Token: 0x0400026C RID: 620
	private static readonly string disableEmoticon = "-1";

	// Token: 0x0400026D RID: 621
	private static Vector3 offscreen = new Vector3(0f, 9999f, 0f);

	// Token: 0x020000D9 RID: 217
	public enum WalkType
	{
		// Token: 0x04000E05 RID: 3589
		Normal,
		// Token: 0x04000E06 RID: 3590
		Jump
	}

	// Token: 0x020000DA RID: 218
	private struct TrailData
	{
		// Token: 0x04000E07 RID: 3591
		public SpriteRenderer[] trails;

		// Token: 0x04000E08 RID: 3592
		public float[] time;

		// Token: 0x04000E09 RID: 3593
		public Vector3[] pos;

		// Token: 0x04000E0A RID: 3594
		public float delay;

		// Token: 0x04000E0B RID: 3595
		public int id;
	}
}
