using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InputIOManager;
using UnityEngine;

// Token: 0x02000048 RID: 72
public class PlayerControl : MonoBehaviour
{
	// Token: 0x0600071E RID: 1822 RVA: 0x0006134C File Offset: 0x0005F54C
	private void Start()
	{
		MainManager.player = this;
		this.entity = base.GetComponent<EntityControl>();
		base.tag = "Player";
		base.gameObject.layer = 11;
		this.flycooldown = 240f;
		this.entity.CreateDetector(new Vector3(0.35f, 0.6f, 0.1f), new Vector3(0f, 0.75f, this.entity.ccol.radius + 0.1f));
		this.entity.speed = (float)this.basespeed;
		this.entity.rigid.mass = 0.01f;
		this.npc = new List<NPCControl>();
		this.entity.overrridejump = false;
		this.bubbleshield = (Object.Instantiate(Resources.Load("Prefabs/Objects/BubbleShield")) as GameObject);
		this.bubbleshield.transform.parent = this.entity.rotater.transform;
		this.bubbleshield.transform.localScale = Vector3.zero;
		this.bubbleshield.transform.localPosition = new Vector3(0.5f, 1.25f);
		this.digicon = new SpriteRenderer[]
		{
			MainManager.NewSpriteObject(new Vector3(0f, 0.5f), base.transform, MainManager.guisprites[3]),
			MainManager.NewSpriteObject(base.transform.position + new Vector3(0f, 1.5f), null, MainManager.guisprites[6])
		};
		this.digicon[0].transform.localScale = new Vector3(0.35f, 2f, 1f);
		this.digicon[1].transform.localScale = Vector3.one * 0.8f;
		this.digicon[1].transform.parent = this.digicon[0].transform;
		for (int i = 0; i < this.digicon.Length; i++)
		{
			this.digicon[i].gameObject.layer = 15;
			this.digicon[i].enabled = false;
			this.digicon[i].material = MainManager.spritedefaultunity;
		}
	}

	// Token: 0x0600071F RID: 1823 RVA: 0x0006158C File Offset: 0x0005F78C
	public void Ceiling()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/GroundDetector")) as GameObject;
		this.ceildetect = gameObject.GetComponent<GroundDetector>();
		this.ceildetect.transform.parent = base.transform;
		this.ceildetect.transform.localPosition = Vector3.up * 2.15f;
		this.ceildetect.transform.localEulerAngles = new Vector3(-90f, 0f);
		this.ceildetect.transform.localScale = new Vector3(this.entity.ccol.radius * 2f - 0.25f, this.entity.ccol.radius * 2f - 0.25f, 0.1f);
		this.ceildetect.parent = this.entity;
		this.ceildetect.ceilingdetector = true;
	}

	// Token: 0x06000720 RID: 1824 RVA: 0x0006167D File Offset: 0x0005F87D
	private void CancelUproot()
	{
		this.uproot = false;
	}

	// Token: 0x06000721 RID: 1825 RVA: 0x00061688 File Offset: 0x0005F888
	private void Update()
	{
		if (!MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message)
		{
			if (this.dashing)
			{
				this.DashBehavior();
				return;
			}
			this.delta = Vector3.zero;
			this.lastaxis = new Vector3(InputIO.JoyStick(0), 0f, InputIO.JoyStick(1));
			if (!this.lockkeys)
			{
				if (MainManager.GetKey(2, true))
				{
					if (this.lastaxis.x != 0f)
					{
						this.delta += -MainManager.instance.globalcamdir.right.normalized * Mathf.Abs(this.lastaxis.x);
						if (Mathf.Abs(this.lastaxis.x) > 0.1f || this.flying || this.shield)
						{
							this.entity.flip = false;
						}
					}
					else
					{
						this.delta += -MainManager.instance.globalcamdir.right.normalized;
						this.entity.flip = false;
					}
					this.trueflip = false;
					this.entity.backsprite = false;
				}
				else if (MainManager.GetKey(3, true))
				{
					if (this.lastaxis.x != 0f)
					{
						this.delta += MainManager.instance.globalcamdir.right.normalized * Mathf.Abs(this.lastaxis.x);
						if (Mathf.Abs(this.lastaxis.x) > 0.1f || this.flying || this.shield)
						{
							this.entity.flip = true;
						}
					}
					else
					{
						this.delta += MainManager.instance.globalcamdir.right.normalized;
						this.entity.flip = true;
					}
					this.trueflip = true;
					this.entity.backsprite = false;
				}
				if (MainManager.GetKey(0, true))
				{
					if (this.lastaxis.z != 0f)
					{
						this.delta += MainManager.instance.globalcamdir.forward.normalized * Mathf.Abs(this.lastaxis.z);
					}
					else
					{
						this.delta += MainManager.instance.globalcamdir.forward.normalized;
					}
					if (!this.flying && !this.shield)
					{
						if (this.lastaxis.z == 0f || Mathf.Abs(this.lastaxis.z) > 0.1f)
						{
							this.entity.backsprite = true;
						}
					}
					else
					{
						this.entity.backsprite = false;
					}
				}
				else if (MainManager.GetKey(1, true))
				{
					if (this.lastaxis.z != 0f)
					{
						this.delta += -MainManager.instance.globalcamdir.forward.normalized * Mathf.Abs(this.lastaxis.z);
					}
					else
					{
						this.delta += -MainManager.instance.globalcamdir.forward.normalized;
					}
					this.entity.backsprite = false;
				}
			}
			if (this.delta == Vector3.zero && !Input.anyKey && this.canpause)
			{
				if (this.idletime < 1000f)
				{
					this.idletime += MainManager.framestep;
				}
			}
			else
			{
				this.idletime = 0f;
			}
			switch (MainManager.analog)
			{
			case 0:
				this.walkdelta = this.delta.normalized * this.entity.speed;
				break;
			case 1:
				this.walkdelta = this.delta.normalized * (((double)Mathf.Abs(Vector3.Magnitude(this.delta)) < 0.4) ? 0.6f : 1f) * this.entity.speed;
				break;
			case 2:
				this.walkdelta = Vector3.ClampMagnitude(this.delta, 1f) * this.entity.speed;
				break;
			}
			this.delta = this.delta.normalized * this.entity.speed;
			if (this.delta != Vector3.zero)
			{
				this.lastdelta = this.delta.normalized;
				if (this.movecd < 10f)
				{
					this.movecd += MainManager.framestep;
				}
			}
			else
			{
				this.movecd = 0f;
			}
			this.entity.detect.transform.LookAt(base.transform.position + this.lastdelta);
			if (!this.lockkeys)
			{
				this.GetInput();
				return;
			}
		}
		else
		{
			if (MainManager.instance.message)
			{
				this.entity.StopMoving(this.entity.animstate);
				return;
			}
			if (MainManager.instance.pause && this.entity.animid == 1 && this.entity.animstate == 100)
			{
				this.CancelAction();
			}
		}
	}

	// Token: 0x06000722 RID: 1826 RVA: 0x00061C40 File Offset: 0x0005FE40
	private void GetInput()
	{
		if (MainManager.GetKey(9, false) && MainManager.instance.flags[10] && MainManager.HasPlayer(1) && this.entity.onground && this.switchcooldown <= 0f && this.actioncooldown <= 0f && this.entity.jumpcooldown <= 0f && this.entity.rigid.velocity.y >= -0.1f && !this.digging)
		{
			this.entity.detect.transform.LookAt(base.transform.position + this.lastdelta);
			MainManager.TeleportFollowers(5f, MainManager.TPDir.Away, this.entity.detect.transform);
			this.CancelAction();
			MainManager.instance.showmoney = 0f;
			if (this.npc.Count > 0 && this.npc[0].tattleid != -1)
			{
				MainManager.instance.StartCoroutine(MainManager.SetText("|kinematicplayer,temp|" + MainManager.GetDialogueText(this.npc[0].tattleid), 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, MainManager.GetEntity(-5).transform, null));
			}
			else
			{
				MainManager.instance.StartCoroutine(MainManager.SetText("|kinematicplayer,temp|" + MainManager.GetDialogueText(MainManager.map.tattleid), 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, MainManager.GetEntity(-5).transform, null));
			}
			this.tattling = true;
		}
		else if (this.entity.onground && MainManager.GetKey(6, false) && !this.action && this.switchcooldown <= 0f && this.beemerang == null && !this.buttonhold && !this.digging && !this.shield && !this.flying && !this.startdig && !this.lockkeys && !this.submarine)
		{
			if (MainManager.instance.playerdata.Length > 1)
			{
				this.SwitchOrder();
			}
		}
		else if (this.canpause && this.entity.onground && MainManager.GetKey(8, false) && !this.buttonhold && !this.digging && !this.shield && !this.flying && this.pausecooldown <= 0f)
		{
			this.Pause();
		}
		if (!MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.inevent)
		{
			if ((this.entity.onground || (this.entity.offgroundframes < 3f && this.entity.jumpcooldown <= 0f)) && MainManager.GetKey(4, false) && !this.action && !this.digging && !this.buttonhold && !this.shield && !this.flying && !MainManager.instance.pause)
			{
				if (this.npc.Count > 0 && MainManager.instance.insideid == this.npc[0].insideid && this.entity.onground)
				{
					if ((this.npc[0].entitytype == NPCControl.NPCType.NPC || this.npc[0].entitytype == NPCControl.NPCType.SemiNPC) && !this.npc[0].nointeract && this.switchcooldown <= 0f && this.npc[0].interactcd <= 0f)
					{
						this.CancelAction();
						if (this.npc != null && this.npc[0] != null && this.interactcd <= 0f)
						{
							this.npc[0].Interact(null);
						}
					}
					else if (!this.submarine)
					{
						this.DoJump();
					}
				}
				else if (!this.submarine)
				{
					this.DoJump();
				}
			}
			if (!this.action && this.beemerang == null && this.actioncooldown <= 0f && this.switchcooldown <= 0f && !MainManager.instance.isholdingskip && !this.dashing)
			{
				if (MainManager.GetKey(5, true))
				{
					this.actionhold += MainManager.framestep;
					if (this.actionhold >= 20f && !this.submarine)
					{
						this.DoActionHold();
					}
				}
				else if (this.digging && this.keepdig > 0f)
				{
					this.DoActionHold();
				}
				else
				{
					if (this.actionhold > 0f && this.actionhold < 20f && this.entity.onground)
					{
						this.actionroutine = base.StartCoroutine(this.DoActionTap());
					}
					this.actionhold = 0f;
					this.buttonhold = false;
					if (this.digging)
					{
						this.uproot = true;
						base.Invoke("CancelUproot", 0.1f);
					}
					if ((this.flying || this.digging || this.shield) && !this.submarine)
					{
						this.canfly = false;
						this.CancelAction();
					}
				}
			}
			else if (MainManager.instance.isholdingskip && !MainManager.GetKey(5, true))
			{
				MainManager.instance.isholdingskip = false;
			}
			if (MainManager.GetKey(7, false) && this.canpause)
			{
				if (MainManager.instance.hudcooldown <= 0f)
				{
					MainManager.PlaySound("HudDown", 13, 1f, 0.15f);
					MainManager.instance.hudcooldown = 300f;
					MainManager.instance.showmoney = 300f;
					return;
				}
				MainManager.PlaySound("HudUp", 13, 1f, 0.15f);
				MainManager.instance.hudcooldown = 1f;
				if (MainManager.instance.money == MainManager.instance.moneyt)
				{
					MainManager.instance.showmoney = 1f;
				}
			}
		}
	}

	// Token: 0x06000723 RID: 1827 RVA: 0x000622C8 File Offset: 0x000604C8
	public void Pause()
	{
		new GameObject("PauseMenu").AddComponent<PauseMenu>();
		MainManager.PlaySound("StartOpen", -1, 1f, 0.5f);
		MainManager.instance.pause = true;
		if (this.beemerang != null)
		{
			Object.Destroy(this.beemerang.gameObject);
		}
	}

	// Token: 0x06000724 RID: 1828 RVA: 0x00062324 File Offset: 0x00060524
	private void FixedUpdate()
	{
		if (this.startdig && !this.submarine)
		{
			this.entity.sprite.transform.localPosition = Vector3.Lerp(this.entity.sprite.transform.localPosition, Vector3.down * 2f, 0.03f);
			this.lockkeys = !this.digging;
			if (this.entity.sprite.transform.localPosition.y <= -1.5f)
			{
				this.digging = true;
			}
			else
			{
				this.entity.StopMoving(this.entity.animstate);
			}
			if (!this.entity.sound.isPlaying)
			{
				if (this.digging)
				{
					this.entity.PlaySound("Digging", 0.8f);
					this.entity.sound.loop = true;
					return;
				}
			}
			else if (this.digging)
			{
				this.entity.sound.pitch = ((this.delta.magnitude > 0.1f) ? 1.1f : 0.95f);
				return;
			}
		}
		else
		{
			if (this.submarine && this.entity.model != null)
			{
				if (!MainManager.instance.inevent)
				{
					this.entity.emoticonoffset = Vector3.up;
				}
				this.entity.flip = false;
				this.entity.overrideflip = true;
				this.entity.sprite.transform.localEulerAngles = Vector3.zero;
				this.entity.sprite.transform.localScale = Vector3.one;
				this.entity.model.localPosition = Vector3.Lerp(this.entity.model.localPosition, this.digging ? new Vector3(0f, -0.5f) : new Vector3(0f, Mathf.Sin(Time.time * 3f) * 0.15f), 0.05f);
				this.entity.extras[1].transform.localPosition = new Vector3(0.575f, 0f, Mathf.Lerp(this.entity.extras[1].transform.localPosition.z, this.digging ? 4f : 0.5f, 0.025f));
				this.entity.extras[0].transform.Rotate(15f * this.entity.rigid.velocity.magnitude, 0f, 0f);
				this.entity.model.transform.localEulerAngles = new Vector3(-90f, Mathf.LerpAngle(this.entity.model.transform.localEulerAngles.y, this.entity.detect.transform.localEulerAngles.y - 90f, MainManager.instance.inevent ? 1f : 0.1f), Mathf.Sin(Time.time * 3f) * (this.entity.rigid.velocity.magnitude / 2f));
				return;
			}
			this.entity.sprite.transform.localPosition = Vector3.Lerp(this.entity.sprite.transform.localPosition, Vector3.zero, 0.17f);
		}
	}

	// Token: 0x06000725 RID: 1829 RVA: 0x000626C4 File Offset: 0x000608C4
	private void LateUpdate()
	{
		if (this.digicon != null)
		{
			this.digicon[0].enabled = (this.digging && this.keepdig > 0f);
			this.digicon[1].enabled = this.digicon[0].enabled;
			if (this.digicon[0].enabled)
			{
				this.digicon[0].color = Color.Lerp(Color.white, Color.green, Mathf.Clamp01(Mathf.Sin(Time.time * 10f)));
			}
		}
		if (this.entity.onground)
		{
			this.canfly = true;
			if (this.setfolloweronground)
			{
				EntityControl entityControl = this.entity;
				for (int i = 1; i < MainManager.instance.playerdata.Length; i++)
				{
					MainManager.BattleData[] playerdata = MainManager.instance.playerdata;
					MainManager.instance.playerdata[i].entity.following = MainManager.instance.playerdata[i - 1].entity;
				}
				this.setfolloweronground = false;
			}
		}
		if (this.flying && !this.buttonhold)
		{
			if (this.startheight != null && base.transform.position.y < this.startheight.Value + 1f)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, new Vector3(base.transform.position.x, this.startheight.Value + 1f, base.transform.position.z), 0.05f);
			}
			this.entity.overrridejump = true;
			this.flycooldown -= MainManager.framestep;
			if (!this.entity.sound.isPlaying)
			{
				this.entity.PlaySound("BeeFly2", 1f, 1.05f);
			}
			this.entity.rigid.velocity = new Vector3(this.entity.rigid.velocity.x, 0f, this.entity.rigid.velocity.z);
			if (this.flycooldown <= 0f)
			{
				this.canfly = false;
				this.CancelAction();
				this.entity.overrridejump = false;
				this.buttonhold = true;
			}
		}
		if (this.bubbleshield != null)
		{
			this.bubbleshield.transform.localEulerAngles = Vector3.zero;
			this.bubbleshield.transform.localScale = Vector3.Lerp(this.bubbleshield.transform.localScale, this.shield ? new Vector3(3f, 3f, 2f) : Vector3.zero, MainManager.TieFramerate(this.shield ? 0.05f : 0.125f));
			if (!this.shield && this.bubbleshield.transform.localScale.magnitude < 0.15f)
			{
				this.bubbleshield.transform.localScale = Vector3.zero;
			}
			this.bubbleshield.transform.localPosition = new Vector3(Mathf.Lerp(this.bubbleshield.transform.localPosition.x, this.entity.flip ? -0.5f : 0.5f, MainManager.TieFramerate(0.1f)), 1.25f);
		}
		if (!MainManager.instance.minipause && this.entity.animid == 1)
		{
			if (this.entity.sprite.transform.localPosition.y < -0.1f)
			{
				this.entity.animstate = 101;
				this.entity.overrideanim = true;
				this.entity.spin = new Vector3(0f, 20f);
			}
			else if (this.entity.overrideanim)
			{
				this.entity.overrideanim = false;
				this.entity.spin = Vector3.zero;
			}
		}
		if (!MainManager.instance.pause && !MainManager.instance.minipause)
		{
			if (this.boulderbreak > 0f)
			{
				this.boulderbreak -= MainManager.framestep;
			}
			if (this.interactcd > 0f)
			{
				this.interactcd -= MainManager.framestep;
			}
			if (this.keepdig > 0f)
			{
				this.keepdig -= MainManager.framestep;
			}
			if (this.idletime > 250f)
			{
				MainManager.instance.showmoney = 10f;
				MainManager.instance.hudcooldown = 10f;
			}
			if (this.entity.icooldown <= 0f && !this.submarine)
			{
				this.entity.sprite.enabled = !this.digging;
			}
			if (!this.digging && !this.flying && !this.startdig && !this.shield && !this.submarine)
			{
				this.CheckPushable();
			}
			if (this.npc.Count > 0 && !MainManager.instance.message && !this.digging && !this.flying && !this.startdig && !this.shield)
			{
				if (this.npc.Count > 1 && Time.frameCount % 3 == 0)
				{
					this.RefreshNPCs();
				}
				NPCControl npccontrol = this.npc[0];
				if (npccontrol != null && !npccontrol.entity.iskill && !npccontrol.entity.dead)
				{
					if (npccontrol.entitytype == NPCControl.NPCType.NPC || npccontrol.entitytype == NPCControl.NPCType.SemiNPC)
					{
						if (npccontrol.interacttype == NPCControl.Interaction.Talk || npccontrol.interacttype == NPCControl.Interaction.ShopKeeper || npccontrol.interacttype == NPCControl.Interaction.StorageAnt || npccontrol.interacttype == NPCControl.Interaction.VenusHeal)
						{
							npccontrol.entity.emoticonid = 0;
							npccontrol.entity.emoticoncooldown = 2f;
						}
						else if (npccontrol.interacttype == NPCControl.Interaction.LockedDoor || npccontrol.interacttype == NPCControl.Interaction.Check || npccontrol.interacttype == NPCControl.Interaction.Shop || npccontrol.interacttype == NPCControl.Interaction.QuestBoard || npccontrol.interacttype == NPCControl.Interaction.Event || npccontrol.interacttype == NPCControl.Interaction.CaravanBadge)
						{
							this.entity.emoticonid = 1;
							this.entity.emoticoncooldown = 2f;
						}
					}
					else
					{
						this.npc.Remove(npccontrol);
					}
					if (npccontrol.interacttype == NPCControl.Interaction.Shop || npccontrol.interacttype == NPCControl.Interaction.CaravanBadge)
					{
						if ((int)npccontrol.shopkeeper.dialogues[1].y != 1)
						{
							MainManager.instance.showmoney = 10f;
						}
						else
						{
							MainManager.instance.showmoney = 0f;
						}
					}
				}
				else
				{
					this.npc.Remove(npccontrol);
				}
			}
			else if (this.npc == null && !MainManager.instance.message && MainManager.instance.money != MainManager.instance.moneyt)
			{
				MainManager.instance.showmoney = 100f;
			}
		}
		if (this.switchcooldown > 0f)
		{
			this.switchcooldown -= MainManager.framestep;
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				float y = -20f;
				if (MainManager.instance.playerdata[j].entity != null)
				{
					if (MainManager.instance.playerdata[j].entity.flip)
					{
						y = 20f;
					}
					MainManager.instance.playerdata[j].entity.spin = new Vector3(0f, y, 0f);
				}
			}
		}
		else if (this.switchcooldown != -99999f)
		{
			for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
			{
				if (MainManager.instance.playerdata[k].entity != null)
				{
					MainManager.instance.playerdata[k].entity.spin = Vector3.zero;
					MainManager.instance.playerdata[k].entity.SetDialogueBleep();
				}
			}
			MainManager.RefreshEntities(true);
			this.switchcooldown = -99999f;
		}
		if (this.canpause && this.pausecooldown > 0f)
		{
			this.pausecooldown -= MainManager.framestep;
		}
		if (MainManager.map != null && base.transform.position.y < MainManager.map.ylimit)
		{
			base.transform.position = this.lastpos;
			this.entity.rigid.velocity = Vector3.zero;
		}
		if (!this.dashing)
		{
			this.Movement();
		}
		if (this.actioncooldown > 0f)
		{
			this.actioncooldown -= MainManager.framestep;
		}
		this.RefreshSpeed();
		if (this.tattling && !MainManager.instance.message)
		{
			if (!MainManager.instance.inevent && !MainManager.instance.pause)
			{
				this.entity.rigid.isKinematic = false;
			}
			this.tattling = false;
		}
		if (this.submarine && !this.digging)
		{
			if (!this.entity.sound.loop)
			{
				this.entity.PlaySound("Submarine");
				this.entity.sound.loop = true;
			}
			this.entity.sound.pitch = Mathf.Clamp(this.entity.rigid.velocity.magnitude, 0.65f, 1f);
		}
		if (!this.submarine && this.entity.sound.loop)
		{
			this.entity.sound.loop = false;
		}
	}

	// Token: 0x06000726 RID: 1830 RVA: 0x000630C4 File Offset: 0x000612C4
	private void CheckPushable()
	{
		if (this.entity.emoticoncooldown <= 0f && MainManager.instance.flags[17])
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Hornable");
			for (int i = 0; i < array.Length; i++)
			{
				if (MainManager.GetDistance(array[i].transform.position, base.transform.position) < 2.5f)
				{
					this.entity.Emoticon(MainManager.Emoticons.Pushable, 5);
					return;
				}
			}
		}
	}

	// Token: 0x06000727 RID: 1831 RVA: 0x00063140 File Offset: 0x00061340
	private void Movement()
	{
		if (!MainManager.instance.inevent && !MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message && this.entity != null && this.entity.rigid != null && !this.lockkeys)
		{
			if (MainManager.GetKey(-2, true))
			{
				if (!this.entity.hitwall)
				{
					if (this.flying)
					{
						this.spd = this.lastdelta * this.entity.speed / 2f;
					}
					else if (this.digging || this.shield)
					{
						this.spd = this.lastdelta * this.entity.speed / 1.5f;
					}
					else if (MainManager.analog == 0)
					{
						this.spd = this.lastdelta * this.entity.speed;
					}
					else
					{
						this.spd = this.walkdelta;
					}
					this.entity.rigid.velocity = new Vector3(this.spd.x, this.entity.rigid.velocity.y, this.spd.z);
					if (!this.flying)
					{
						this.entity.animstate = 1;
					}
				}
				else
				{
					this.entity.StopMoving(1);
				}
				if (MainManager.GetKey(1, true))
				{
					MainManager.instance.camspeed = Mathf.Lerp(MainManager.instance.camspeed, 0.25f, MainManager.TieFramerate(0.01f));
					if (!this.entity.hitwall)
					{
						MainManager.instance.camoffset2 = Vector3.Lerp(MainManager.instance.camoffset2, -MainManager.instance.globalcamdir.forward.normalized * 1.25f, MainManager.TieFramerate(0.025f));
					}
					else
					{
						this.ReturnOffset();
					}
				}
				else
				{
					if (!MainManager.instance.changecamspeed)
					{
						MainManager.instance.camspeed = 0.1f;
					}
					this.ReturnOffset();
				}
				if (this.submarine)
				{
					if (this.digging && (!this.entity.sound.isPlaying || this.entity.sound.clip.name != "Sonar"))
					{
						this.entity.PlaySound("Sonar", 0.75f);
						this.entity.sound.loop = false;
						return;
					}
				}
				else
				{
					this.entity.sound.loop = false;
					if (this.entity.onground && !this.entity.sound.isPlaying && !this.digging)
					{
						this.footstep -= MainManager.framestep;
						if (this.footstep <= 0f)
						{
							this.footstep = 7.5f;
							this.entity.PlaySound("Footstep");
							return;
						}
					}
				}
			}
			else
			{
				this.footstep = 0f;
				this.entity.ForceHitWall();
				if (!this.submarine)
				{
					this.entity.StopMoving(0);
				}
				else
				{
					this.entity.rigid.velocity = Vector3.Lerp(this.entity.rigid.velocity, Vector3.zero, MainManager.TieFramerate(0.025f));
				}
				this.ReturnOffset();
			}
		}
	}

	// Token: 0x06000728 RID: 1832 RVA: 0x000634E0 File Offset: 0x000616E0
	private void DashBehavior()
	{
		this.lastaxis = new Vector3(InputIO.JoyStick(0), 0f, InputIO.JoyStick(1));
		this.dashtarget = this.dashtarget.normalized;
		if (MainManager.GetKey(2, true))
		{
			if (this.lastaxis.x != 0f)
			{
				this.dashtarget += -MainManager.instance.globalcamdir.right.normalized * Mathf.Abs(this.lastaxis.x);
			}
			else
			{
				this.dashtarget += -MainManager.instance.globalcamdir.right.normalized;
			}
			this.entity.flip = false;
			this.entity.backsprite = false;
		}
		else if (MainManager.GetKey(3, true))
		{
			if (this.lastaxis.x != 0f)
			{
				this.dashtarget += MainManager.instance.globalcamdir.right.normalized * Mathf.Abs(this.lastaxis.x);
			}
			else
			{
				this.dashtarget += MainManager.instance.globalcamdir.right.normalized;
			}
			this.entity.flip = true;
			this.entity.backsprite = false;
		}
		if (MainManager.GetKey(0, true))
		{
			if (this.lastaxis.z != 0f)
			{
				this.dashtarget += MainManager.instance.globalcamdir.forward.normalized * Mathf.Abs(this.lastaxis.z);
			}
			else
			{
				this.dashtarget += MainManager.instance.globalcamdir.forward.normalized;
			}
		}
		else if (MainManager.GetKey(1, true))
		{
			if (this.lastaxis.z != 0f)
			{
				this.dashtarget += -MainManager.instance.globalcamdir.forward.normalized * Mathf.Abs(this.lastaxis.z);
			}
			else
			{
				this.dashtarget += -MainManager.instance.globalcamdir.forward.normalized;
			}
		}
		if (this.movecd < 10f)
		{
			this.movecd += MainManager.framestep;
		}
		this.dashdelta = Vector3.Lerp(this.dashdelta, this.dashtarget.normalized * 4f, MainManager.framestep * 0.025f);
		this.entity.Move(base.transform.position + this.dashdelta, 1f, MainManager.instance.flags[39] ? 117 : 116);
		this.entity.backsprite = false;
		this.entity.DetectDirection(base.transform.position + this.dashdelta);
		this.entity.sprite.transform.eulerAngles = new Vector3(0f, this.entity.detect.transform.eulerAngles.y + 90f, 0f);
		float num = this.entity.sprite.transform.localEulerAngles.y;
		if (num > 80f && num < 100f)
		{
			if (num > 91f)
			{
				num = Mathf.Clamp(num, 105f, 180f);
			}
			else
			{
				num = Mathf.Clamp(num, 0f, 75f);
			}
		}
		else if (num > 260f && num < 280f)
		{
			if (num > 271f)
			{
				num = Mathf.Clamp(num, 285f, 360f);
			}
			else
			{
				num = Mathf.Clamp(num, 180f, 255f);
			}
		}
		this.entity.sprite.transform.localEulerAngles = new Vector3(this.entity.sprite.transform.localEulerAngles.x, num, this.entity.sprite.transform.localEulerAngles.z);
		if (this.tbox != null)
		{
			this.tbox.transform.eulerAngles = this.entity.detect.transform.eulerAngles;
		}
		this.entity.forcemove = false;
		if (!this.entity.sound.isPlaying)
		{
			this.entity.PlaySound("BeetleDash", 0.5f, 1f);
		}
		if (this.entity.offgroundframes > 20f && this.entity.rigid.velocity.y > this.entity.jumpheight)
		{
			this.entity.rigid.velocity = new Vector3(this.entity.rigid.velocity.x, 0f, this.entity.rigid.velocity.z);
		}
		if (this.boulderbreak <= 0f && (this.entity.hitwall || MainManager.instance.pause))
		{
			base.StartCoroutine(this.StopDash(this.entity.hitwall));
			return;
		}
		if (MainManager.GetKey(5) || MainManager.GetKey(4))
		{
			base.StartCoroutine(this.StopDash(false));
		}
	}

	// Token: 0x06000729 RID: 1833 RVA: 0x00063A9F File Offset: 0x00061C9F
	public IEnumerator StopDash(bool wall)
	{
		if (wall)
		{
			MainManager.PlaySound("Death3");
			this.entity.forcemove = false;
			this.entity.animstate = 11;
			MainManager.ShakeScreen(Vector3.one * 0.1f, 0.2f);
		}
		this.smoke.transform.position = new Vector3(0f, -1000f);
		if (this.smoke != null)
		{
			Object.Destroy(this.smoke.gameObject, 1.5f);
		}
		if (this.tbox != null)
		{
			Object.Destroy(this.tbox.gameObject);
		}
		this.entity.detect.tag = "Untagged";
		yield return null;
		this.entity.rigid.velocity = new Vector3(0f, this.entity.rigid.velocity.y, 0f);
		this.diggingpart = null;
		this.dashing = false;
		yield return new WaitForSeconds(0.25f);
		this.entity.animstate = 0;
		this.entity.jumpcooldown = 5f;
		this.actioncooldown = 5f;
		this.movecd = 0f;
		if (MainManager.FreePlayer())
		{
			this.CancelAction();
		}
		yield break;
	}

	// Token: 0x0600072A RID: 1834 RVA: 0x00063AB5 File Offset: 0x00061CB5
	private void ReturnOffset()
	{
		MainManager.instance.camoffset2 = Vector3.Lerp(MainManager.instance.camoffset2, Vector3.zero, MainManager.TieFramerate(0.01f));
	}

	// Token: 0x0600072B RID: 1835 RVA: 0x00063ADF File Offset: 0x00061CDF
	private void DefaultCamOffset()
	{
		MainManager.instance.camoffsetspeed = 0.075f;
		MainManager.instance.camoffset = MainManager.defaultcamoffset;
	}

	// Token: 0x0600072C RID: 1836 RVA: 0x00063B00 File Offset: 0x00061D00
	private void RefreshSpeed()
	{
		if (this.submarine)
		{
			this.entity.speed = (float)this.basespeed / 1.8f;
			return;
		}
		if (this.dashing)
		{
			this.entity.speed = (float)this.basespeed * 2.5f;
			return;
		}
		this.entity.speed = ((float)this.basespeed + this.entity.ccol.material.dynamicFriction) * 1.3f;
	}

	// Token: 0x0600072D RID: 1837 RVA: 0x00063B80 File Offset: 0x00061D80
	private float GetAngle(float input, float limit)
	{
		float num = input + 90f;
		if (this.entity.flip)
		{
			num = Mathf.Clamp(num, 90f + limit, 270f - limit);
		}
		else if (num > 180f)
		{
			num = Mathf.Clamp(num, 270f + limit, 360f + limit);
		}
		else
		{
			num = 90f - limit;
		}
		return num;
	}

	// Token: 0x0600072E RID: 1838 RVA: 0x00063BE1 File Offset: 0x00061DE1
	private float GetAngle()
	{
		return this.GetAngle(this.entity.detect.transform.localEulerAngles.y, 45f);
	}

	// Token: 0x0600072F RID: 1839 RVA: 0x00063C08 File Offset: 0x00061E08
	private IEnumerator DoActionTap()
	{
		this.lastactionid = 0;
		if (this.submarine)
		{
			this.digging = !this.digging;
		}
		else
		{
			this.entity.flip = this.trueflip;
			this.entity.StopMoving(0);
			this.actionhold = 0f;
			this.entity.backsprite = false;
			this.action = true;
			this.lockkeys = true;
			float a = 0f;
			float angle = this.GetAngle();
			Vector3 iceclepos;
			switch (MainManager.instance.playerdata[0].animid)
			{
			case 0:
				if (!MainManager.instance.flags[41] || MainManager.instance.flags[11])
				{
					this.entity.animstate = 100;
					this.entity.PlaySoundSimple("Toss");
					if (!this.entity.flip)
					{
						this.entity.spin = new Vector3(0f, -30f, 0f);
					}
					else
					{
						this.entity.spin = new Vector3(0f, 30f, 0f);
					}
					yield return new WaitForSeconds(0.2f);
					this.entity.animstate = 101;
					this.entity.spin = Vector3.zero;
					GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Objects/Beerang"), base.transform.position, Quaternion.identity) as GameObject;
					this.beemerang = gameObject.GetComponent<NPCControl>();
					this.beemerang.insideid = MainManager.instance.insideid;
					EntityControl component = this.beemerang.GetComponent<EntityControl>();
					component.speed = 0.075f;
					gameObject.layer = 0;
					component.spin = new Vector3(0f, 0f, 20f);
					component.flip = this.entity.flip;
					this.entity.overrideflip = true;
					this.entity.sprite.transform.localEulerAngles = new Vector3(0f, angle, 0f);
					if (MainManager.snapTo8)
					{
						Vector3[] vectordata = this.beemerang.vectordata;
						int num = 0;
						Vector3 position = this.beemerang.transform.position;
						Vector3 vector = Vector3.one;
						vectordata[num] = position + MainManager.Snap(this.lastdelta, vector).normalized * 7.5f;
					}
					else
					{
						this.beemerang.vectordata[0] = this.beemerang.transform.position + this.lastdelta * 7.5f;
					}
					yield return new WaitForSeconds(0.25f);
				}
				break;
			case 1:
				this.lastactionid = 1;
				if (this.dashing)
				{
					this.entity.StopMoving(-1);
					this.dashing = false;
					this.entity.jumpcooldown = 5f;
				}
				else
				{
					this.entity.overrridejump = true;
					this.entity.overrideanim = true;
					this.entity.overrideflip = true;
					this.entity.animstate = 100;
					this.entity.sprite.transform.localEulerAngles = new Vector3(0f, angle, 0f);
					a = 15f;
					bool pressed = false;
					MainManager.PlaySound("Cut", -1, 1f, 0.5f);
					while (a > 0f)
					{
						if (MainManager.instance.flags[699] && MainManager.GetKey(5, false))
						{
							pressed = true;
							break;
						}
						a -= MainManager.framestep;
						yield return null;
					}
					this.tbox = new GameObject("cut").AddComponent<BoxCollider>();
					this.tbox.tag = "BeetleHorn";
					this.tbox.size = new Vector3(1f, 1.5f, 2.25f);
					this.tbox.center = new Vector3(0f, 1f);
					this.tbox.transform.eulerAngles = this.entity.sprite.transform.localEulerAngles;
					this.tbox.isTrigger = true;
					Transform transform = this.tbox.transform;
					Vector3 position2 = base.transform.position;
					Vector3 vector = this.entity.sprite.transform.right;
					transform.position = position2 - vector.normalized * 1.25f;
					if (!pressed)
					{
						Object.Destroy(this.tbox.gameObject, 0.15f);
						yield return new WaitForSeconds(0.15f);
					}
					else
					{
						MainManager.PlaySound("Spin4");
						this.smoke = (Object.Instantiate(Resources.Load("Prefabs/Particles/WalkDust")) as GameObject).GetComponent<ParticleSystem>();
						this.smoke.transform.eulerAngles = new Vector3(-90f, 0f);
						this.smoke.transform.parent = base.transform;
						this.smoke.transform.localPosition = new Vector3(0f, 0.25f, 0.1f);
						this.smoke.GetComponent<Renderer>().material.renderQueue = 3001;
						this.diggingpart = this.smoke.gameObject;
						ParticleSystem.EmissionModule se = this.smoke.emission;
						ParticleSystem.MainModule sd = this.smoke.main;
						sd.startLifetime = new ParticleSystem.MinMaxCurve(1f);
						se.rateOverTime = new ParticleSystem.MinMaxCurve(5f);
						sd.startSize = new ParticleSystem.MinMaxCurve(0.75f);
						this.tbox.enabled = true;
						this.tbox.transform.parent = this.entity.transform;
						this.tbox.transform.localPosition = Vector3.zero;
						this.tbox.center = new Vector3(0f, 1.5f, 1f);
						this.tbox.size = new Vector3(1f, 1.5f, 1f);
						this.entity.overrridejump = true;
						this.entity.animstate = 116;
						this.entity.Jump(5f);
						yield return new WaitForSeconds(0.1f);
						float fs = 120f;
						while (!this.entity.onground && fs > 0f)
						{
							fs -= MainManager.framestep;
							yield return null;
						}
						this.entity.offgroundframes = 0f;
						this.dashing = true;
						this.dashdelta = this.lastdelta.normalized * 4f;
						this.dashtarget = this.dashdelta;
						se.rateOverTime = new ParticleSystem.MinMaxCurve(10f);
						sd.startSize = new ParticleSystem.MinMaxCurve(1.1f);
						if (!MainManager.instance.flags[39])
						{
							this.tbox.tag = "BeetleHorn";
						}
						else
						{
							this.tbox.tag = "BeetleDash";
						}
						se = default(ParticleSystem.EmissionModule);
						sd = default(ParticleSystem.MainModule);
					}
				}
				break;
			case 2:
			{
				this.entity.animstate = 111;
				this.entity.overrideflip = true;
				this.entity.sprite.transform.localEulerAngles = new Vector3(0f, angle, 0f);
				iceclepos = base.transform.position + Vector3.up + this.lastdelta * 2f;
				GameObject gameObject2 = Object.Instantiate(Resources.Load("Prefabs/Particles/mothicenormal"), iceclepos, Quaternion.Euler(-90f, 0f, 0f)) as GameObject;
				BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
				MainManager.PlaySound("OverworldIce", -1, 1f, 0.8f);
				gameObject2.tag = "Icecle";
				boxCollider.size = new Vector3(2f, 1f, 1f);
				boxCollider.center = new Vector3(0.5f, 0f);
				boxCollider.transform.eulerAngles = new Vector3(0f, this.entity.detect.transform.eulerAngles.y + 90f);
				boxCollider.isTrigger = true;
				Object.Destroy(boxCollider, 0.25f);
				Object.Destroy(gameObject2, 1f);
				a = 0f;
				if (MainManager.instance.flags[171])
				{
					bool pressed = false;
					while (!MainManager.GetKey(5))
					{
						a += MainManager.framestep;
						yield return null;
						if (a >= 25f)
						{
							IL_9FE:
							if (pressed)
							{
								this.entity.PlaySound("Freeze", 0.5f, 1.25f);
								this.entity.overrideanim = true;
								this.entity.animstate = 108;
								if (this.icecle == null)
								{
									this.icecle = (Object.Instantiate(Resources.Load("Prefabs/Objects/icecle")) as GameObject).transform;
									this.iceclesize = 0f;
								}
								this.icecle.transform.position = iceclepos + Vector3.up * 3f;
								do
								{
									this.iceclesize += MainManager.TieFramerate(0.05f);
									this.icecle.transform.localEulerAngles += Vector3.up * 20f;
									this.icecle.transform.localScale = Vector3.one * this.iceclesize;
									yield return null;
								}
								while (this.iceclesize < 1f);
								this.DropIcecle();
								yield return new WaitForSeconds(0.2f);
								goto IL_B70;
							}
							goto IL_B70;
						}
					}
					pressed = true;
					a = 999f;
					goto IL_9FE;
				}
				yield return new WaitForSeconds(0.3f);
				break;
			}
			}
			IL_B70:
			iceclepos = default(Vector3);
			yield return null;
			if (this.entity.flip)
			{
				Vector3 vector = MainManager.instance.globalcamdir.transform.right;
				this.lastdelta = vector.normalized;
			}
			else
			{
				Vector3 vector = MainManager.instance.globalcamdir.transform.right;
				this.lastdelta = -vector.normalized;
			}
			this.action = false;
			this.lockkeys = false;
			this.entity.overrideanim = false;
			this.entity.overrideflip = false;
			this.entity.overrridejump = false;
			this.ActionCooldown();
		}
		this.actionroutine = null;
		yield break;
	}

	// Token: 0x06000730 RID: 1840 RVA: 0x00063C18 File Offset: 0x00061E18
	private void ActionCooldown()
	{
		int num = this.lastactionid;
		if (num == 1)
		{
			this.actioncooldown = 5f;
			return;
		}
		this.actioncooldown = 17f;
	}

	// Token: 0x06000731 RID: 1841 RVA: 0x00063C48 File Offset: 0x00061E48
	private void DropIcecle()
	{
		this.icecle.transform.localScale = Vector3.one;
		this.icecle.gameObject.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		this.icecle.gameObject.AddComponent<DestroyOnLayer>().SetUp("IceShatter", 2f, 8, Vector3.up / 2f, new Vector3(-90f, 0f, 0f));
		this.icecle.gameObject.AddComponent<DestroyOnLayer>().SetUp("IceShatter", 2f, 13, Vector3.up / 2f, new Vector3(-90f, 0f, 0f));
		this.icecle.GetComponent<BoxCollider>().enabled = false;
		BoxCollider boxCollider = this.icecle.gameObject.AddComponent<BoxCollider>();
		if (MainManager.map.entityonly != null && MainManager.map.entityonly.Length != 0)
		{
			for (int i = 0; i < MainManager.map.entityonly.Length; i++)
			{
				if (MainManager.map.entityonly[i] != null)
				{
					Physics.IgnoreCollision(boxCollider, MainManager.map.entityonly[i], true);
				}
			}
		}
		boxCollider.size = new Vector3(1f, 1.5f, 1f);
		boxCollider.isTrigger = true;
		Object.Destroy(this.icecle.gameObject, 1f);
		this.icecle = null;
	}

	// Token: 0x06000732 RID: 1842 RVA: 0x00063DC0 File Offset: 0x00061FC0
	private void DoActionHold()
	{
		this.lastactionid = 0;
		switch (MainManager.instance.playerdata[0].animid)
		{
		case 0:
			if (!MainManager.instance.flags[19])
			{
				if (this.beemerang == null)
				{
					this.actionroutine = base.StartCoroutine(this.DoActionTap());
					return;
				}
			}
			else if (MainManager.instance.playerdata.Length > 1 && !this.buttonhold && this.canfly && this.jumproutine == null && !this.ceiling)
			{
				if (this.startheight == null)
				{
					this.startheight = new float?(base.transform.position.y);
				}
				this.entity.animstate = 102;
				this.flying = true;
				this.entity.overrideanim = true;
				this.entity.rigid.useGravity = false;
				return;
			}
			break;
		case 1:
			if (MainManager.instance.flags[18] && this.candig && this.entity.onground && Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f, 256))
			{
				if (this.entity.animstate != 101 && !this.digging)
				{
					MainManager.PlaySound("Dig", -1, 1f, 0.7f);
				}
				this.entity.animstate = 101;
				this.entity.overrideanim = true;
				this.entity.spin = new Vector3(0f, 25f, 0f);
				this.startdig = true;
				if (!this.digging)
				{
					this.entity.StopMoving(this.entity.animstate);
					if (this.diggingpart == null)
					{
						this.diggingpart = MainManager.PlayParticle("DirtFlying", null, base.transform.position, new Vector3(-90f, 0f), -1f);
						return;
					}
				}
				else
				{
					MainManager.DestroyTemp(ref this.diggingpart, 1f);
					if (this.tunnelpart == null)
					{
						MainManager.instance.RefreshPlayer(true);
						this.tunnelpart = MainManager.PlayParticle("Digging", null, base.transform.position, new Vector3(-90f, 0f), -1f, 2760);
						this.tunnelpart.transform.parent = base.transform;
						return;
					}
				}
			}
			else
			{
				if (MainManager.instance.flags[18])
				{
					this.CancelAction();
					return;
				}
				if (this.actionroutine == null)
				{
					this.actionroutine = base.StartCoroutine(this.DoActionTap());
					return;
				}
			}
			break;
		case 2:
			if (MainManager.instance.flags[20])
			{
				this.entity.overrideanim = true;
				this.entity.animstate = ((this.delta.magnitude >= 0.1f) ? 123 : 122);
				if (!this.shield)
				{
					MainManager.TeleportFollowers();
					this.entity.PlaySound("Shield");
				}
				this.shield = true;
				return;
			}
			if (this.actionroutine == null)
			{
				this.actionroutine = base.StartCoroutine(this.DoActionTap());
			}
			break;
		default:
			return;
		}
	}

	// Token: 0x06000733 RID: 1843 RVA: 0x00064122 File Offset: 0x00062322
	public IEnumerator JumpTo(Vector3 position, float height)
	{
		this.jumproutine = base.StartCoroutine(this.JumpTo(position, height, 1f));
		yield return null;
		yield break;
	}

	// Token: 0x06000734 RID: 1844 RVA: 0x0006413F File Offset: 0x0006233F
	public IEnumerator JumpTo(Vector3 position, float height, float multiplier)
	{
		this.CancelAction();
		MainManager.instance.minipause = true;
		MainManager.instance.overridefollower = true;
		Vector3 startp = base.transform.position;
		Vector3 camoffset = MainManager.instance.camoffset;
		Vector3 camsp = MainManager.MainCamera.transform.parent.position;
		float t = 0f;
		float ts = MainManager.instance.camspeed;
		if (!MainManager.instance.inevent)
		{
			MainManager.instance.camspeed = 0.075f;
			MainManager.instance.camtarget = null;
			MainManager.instance.camtargetpos = null;
		}
		List<EntityControl> list = new List<EntityControl>();
		float num = 0f;
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			list.Add(MainManager.instance.playerdata[i].entity);
			MainManager.instance.playerdata[i].entity.rigid.velocity = Vector3.zero;
			num += 0.1f;
			if (i > 0)
			{
				MainManager.instance.playerdata[i].entity.transform.position = base.transform.position + new Vector3(0f, 0f, num);
			}
		}
		if (MainManager.map.tempfollowers.Count > 0)
		{
			for (int j = 0; j < MainManager.map.tempfollowers.Count; j++)
			{
				list.Add(MainManager.map.tempfollowers[j]);
				MainManager.map.tempfollowers[j].rigid.velocity = Vector3.zero;
				num += 0.1f;
				MainManager.map.tempfollowers[j].transform.position = base.transform.position + new Vector3(0f, 0f, num);
			}
		}
		EntityControl[] entities = list.ToArray();
		for (int k = 0; k < entities.Length; k++)
		{
			entities[k].rigid.useGravity = false;
			entities[k].rigid.isKinematic = true;
		}
		float b = (45f * (float)entities.Length - ((entities.Length > 2) ? Mathf.Pow((float)entities.Length, 3f) : -20f)) / multiplier;
		do
		{
			if (!MainManager.instance.inevent)
			{
				float d = Mathf.Clamp((0.5f - Mathf.Abs(t / b - 0.5f)) * 2f, 0f, 0.75f);
				MainManager.MainCamera.transform.parent.position = Vector3.Lerp(camsp, position, t / (b / 1.5f));
				MainManager.instance.camoffset = camoffset + -MainManager.instance.globalcamdir.forward.normalized * d + Vector3.up * d;
			}
			for (int l = 0; l < entities.Length; l++)
			{
				float t2 = Mathf.Clamp01(t / (b / 2f) - (float)l * (1f / (float)entities.Length));
				entities[l].transform.localPosition = MainManager.BeizierCurve3(startp, position + MainManager.instance.globalcamdir.forward * ((float)l / 10f), height, t2);
				if (entities[l].onground)
				{
					entities[l].anim.Play("Idle");
				}
				else
				{
					entities[l].anim.Play("Fall");
				}
			}
			t += MainManager.framestep;
			yield return null;
		}
		while (t < b - 10f);
		for (int m = 0; m < entities.Length; m++)
		{
			entities[m].rigid.velocity = Vector3.zero;
			entities[m].rigid.useGravity = true;
			entities[m].rigid.isKinematic = false;
		}
		yield return null;
		if (!MainManager.instance.inevent)
		{
			MainManager.instance.camoffset = camoffset;
			MainManager.instance.camtarget = base.transform;
			MainManager.instance.camtargetpos = null;
			MainManager.instance.camspeed = ts;
			MainManager.instance.minipause = false;
			MainManager.instance.overridefollower = false;
		}
		this.jumproutine = null;
		yield break;
	}

	// Token: 0x06000735 RID: 1845 RVA: 0x00064163 File Offset: 0x00062363
	public void CancelAction()
	{
		this.CancelAction(false);
	}

	// Token: 0x06000736 RID: 1846 RVA: 0x0006416C File Offset: 0x0006236C
	public void CancelAction(bool keepbeerang)
	{
		this.movecd = 0f;
		if (this.dashing)
		{
			base.StartCoroutine(this.StopDash(false));
		}
		this.entity.StopForceMove();
		if (this.tbox != null)
		{
			Object.Destroy(this.tbox.gameObject);
		}
		if (this.digging)
		{
			MainManager.PlaySound("DigPop2", -1, 1f, 0.7f);
		}
		this.flycooldown = 240f;
		this.shield = false;
		if (this.flying || this.digging)
		{
			this.entity.sound.Stop();
			MainManager.TeleportFollowers();
			if (MainManager.instance.playerdata.Length > 1)
			{
				for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
				{
					MainManager.instance.playerdata[i].entity.LockRigid(false);
					MainManager.instance.playerdata[i].entity.overrideanim = false;
					MainManager.instance.playerdata[i].entity.onground = false;
					MainManager.instance.playerdata[i].entity.spin = Vector3.zero;
					MainManager.instance.playerdata[i].entity.animstate = 0;
				}
			}
		}
		this.digging = false;
		this.flying = false;
		this.startdig = false;
		MainManager.DestroyTemp(ref this.diggingpart, 1f);
		MainManager.DestroyTemp(ref this.tunnelpart, 1f);
		this.lockkeys = false;
		this.entity.sound.Stop();
		this.entity.overrideanim = false;
		if (!this.submarine)
		{
			this.entity.overrideflip = false;
		}
		this.entity.overrridejump = false;
		this.entity.sound.Stop();
		this.startheight = null;
		this.entity.spin = Vector3.zero;
		this.entity.rigid.useGravity = true;
		if (this.actionroutine != null)
		{
			base.StopCoroutine(this.actionroutine);
		}
		if (this.beemerang != null && !keepbeerang)
		{
			Object.Destroy(this.beemerang.gameObject);
		}
		if (this.tempcamoffset != null)
		{
			MainManager.instance.camoffset = this.tempcamoffset.Value;
		}
		if (this.icecle != null)
		{
			Object.Destroy(Object.Instantiate(Resources.Load("Prefabs/Particles/IceShatter"), this.icecle.transform.position, Quaternion.Euler(-90f, 0f, 0f)) as GameObject, 2f);
			Object.Destroy(this.icecle.gameObject);
		}
		this.entity.sound.loop = false;
		this.tempcamoffset = null;
		this.action = false;
		this.pausecooldown = 10f;
		this.entity.onground = false;
		this.ActionCooldown();
	}

	// Token: 0x06000737 RID: 1847 RVA: 0x00064484 File Offset: 0x00062684
	private void SwitchOrder()
	{
		this.switchcooldown = 15f;
		MainManager.SwitchParty(false);
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.partyorder[i] = MainManager.instance.playerdata[i].animid;
		}
	}

	// Token: 0x06000738 RID: 1848 RVA: 0x000644DC File Offset: 0x000626DC
	private void RefreshNPCs()
	{
		try
		{
			if (this.npc != null && this.npc.Count > 0)
			{
				this.npc = (from point in this.npc
				orderby MainManager.GetDistance(base.transform.position, point.transform.position)
				select point).ToList<NPCControl>();
			}
		}
		catch
		{
		}
	}

	// Token: 0x06000739 RID: 1849 RVA: 0x00064538 File Offset: 0x00062738
	private void DoJump()
	{
		if (!this.ceiling)
		{
			this.entity.jumpcooldown = 10f;
			this.entity.Jump();
			this.entity.PlaySoundSimple("Jump");
			base.Invoke("DisableFly", 0.1f);
		}
	}

	// Token: 0x0600073A RID: 1850 RVA: 0x00064588 File Offset: 0x00062788
	private void DisableFly()
	{
		this.canfly = false;
	}

	// Token: 0x0600073B RID: 1851 RVA: 0x00064594 File Offset: 0x00062794
	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Pusher") || other.CompareTag("PPusher"))
		{
			if (!this.flying)
			{
				this.entity.onground = false;
				if (this.entity.rigid.velocity.y > 0f)
				{
					this.entity.rigid.velocity = new Vector3(this.entity.rigid.velocity.x, 0f, this.entity.rigid.velocity.z);
				}
				MainManager.PushAway(base.transform, other.transform.position);
				return;
			}
		}
		else if (other.CompareTag("Respawn"))
		{
			NPCControl component = other.GetComponent<NPCControl>();
			if (component != null && component.vectordata[0].magnitude > 0.1f)
			{
				this.lastpos = component.vectordata[0];
				return;
			}
			if (this.entity.onground)
			{
				this.respawncount += MainManager.framestep;
				if (this.respawncount > 15f)
				{
					this.lastpos = base.transform.position;
					this.respawncount = 0f;
					return;
				}
			}
		}
		else if (other.CompareTag("Conveyor"))
		{
			if (!(this.conveyor != null) || !(this.conveyor.transform == other.transform))
			{
				this.conveyor = other.GetComponent<StaticModelAnim>();
				return;
			}
			if (MainManager.FreePlayer())
			{
				base.transform.position += this.conveyor.conveyor * MainManager.framestep;
				return;
			}
		}
		else if (other.CompareTag("KeepDig"))
		{
			this.keepdig = 5f;
		}
	}

	// Token: 0x0600073C RID: 1852 RVA: 0x00064774 File Offset: 0x00062974
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Conveyor"))
		{
			this.conveyor = null;
			return;
		}
		if (other.CompareTag("KeepDig"))
		{
			this.keepdig = 0f;
			return;
		}
		if (other.CompareTag("Respawn"))
		{
			this.respawncount = 0f;
		}
	}

	// Token: 0x040006EB RID: 1771
	public EntityControl entity;

	// Token: 0x040006EC RID: 1772
	public NPCControl beemerang;

	// Token: 0x040006ED RID: 1773
	public Transform icecle;

	// Token: 0x040006EE RID: 1774
	public Transform frozencube;

	// Token: 0x040006EF RID: 1775
	public List<NPCControl> npc;

	// Token: 0x040006F0 RID: 1776
	public Vector3 lastpos;

	// Token: 0x040006F1 RID: 1777
	public float switchcooldown;

	// Token: 0x040006F2 RID: 1778
	public float flycooldown;

	// Token: 0x040006F3 RID: 1779
	public float actionhold;

	// Token: 0x040006F4 RID: 1780
	public float flyheight;

	// Token: 0x040006F5 RID: 1781
	public float actioncooldown;

	// Token: 0x040006F6 RID: 1782
	public float iceclesize;

	// Token: 0x040006F7 RID: 1783
	public float pausecooldown;

	// Token: 0x040006F8 RID: 1784
	public float boulderbreak;

	// Token: 0x040006F9 RID: 1785
	public float movecd;

	// Token: 0x040006FA RID: 1786
	public float interactcd;

	// Token: 0x040006FB RID: 1787
	private float? startheight;

	// Token: 0x040006FC RID: 1788
	public bool action;

	// Token: 0x040006FD RID: 1789
	public bool digging;

	// Token: 0x040006FE RID: 1790
	public bool shield;

	// Token: 0x040006FF RID: 1791
	public bool lockkeys;

	// Token: 0x04000700 RID: 1792
	public bool flying;

	// Token: 0x04000701 RID: 1793
	public bool startdig;

	// Token: 0x04000702 RID: 1794
	public bool buttonhold;

	// Token: 0x04000703 RID: 1795
	public bool canfly;

	// Token: 0x04000704 RID: 1796
	public bool uproot;

	// Token: 0x04000705 RID: 1797
	public bool setfolloweronground;

	// Token: 0x04000706 RID: 1798
	public bool canpause = true;

	// Token: 0x04000707 RID: 1799
	public bool candig;

	// Token: 0x04000708 RID: 1800
	public bool submarine;

	// Token: 0x04000709 RID: 1801
	public bool forceclosemove;

	// Token: 0x0400070A RID: 1802
	public bool tattling;

	// Token: 0x0400070B RID: 1803
	public bool ceiling;

	// Token: 0x0400070C RID: 1804
	public bool dashing;

	// Token: 0x0400070D RID: 1805
	public bool trueflip;

	// Token: 0x0400070E RID: 1806
	public int basespeed = 5;

	// Token: 0x0400070F RID: 1807
	private float idletime;

	// Token: 0x04000710 RID: 1808
	private float footstep;

	// Token: 0x04000711 RID: 1809
	private float respawncount;

	// Token: 0x04000712 RID: 1810
	private int lastactionid;

	// Token: 0x04000713 RID: 1811
	public const float holdammount = 20f;

	// Token: 0x04000714 RID: 1812
	public const float rangdistance = 7.5f;

	// Token: 0x04000715 RID: 1813
	public const float flymax = 1f;

	// Token: 0x04000716 RID: 1814
	public const float actioncdammount = 17f;

	// Token: 0x04000717 RID: 1815
	public const float deadzone = 0.1f;

	// Token: 0x04000718 RID: 1816
	public const float anglelimit = 45f;

	// Token: 0x04000719 RID: 1817
	public const float flyammount = 240f;

	// Token: 0x0400071A RID: 1818
	public const float pushradius = 2.5f;

	// Token: 0x0400071B RID: 1819
	private const float footstepdelay = 7.5f;

	// Token: 0x0400071C RID: 1820
	private const float coyotetime = 3f;

	// Token: 0x0400071D RID: 1821
	private Coroutine actionroutine;

	// Token: 0x0400071E RID: 1822
	public Coroutine jumproutine;

	// Token: 0x0400071F RID: 1823
	public Collider standingon;

	// Token: 0x04000720 RID: 1824
	private GameObject tunnelpart;

	// Token: 0x04000721 RID: 1825
	private GameObject diggingpart;

	// Token: 0x04000722 RID: 1826
	private GameObject bubbleshield;

	// Token: 0x04000723 RID: 1827
	private BoxCollider tbox;

	// Token: 0x04000724 RID: 1828
	private GroundDetector ceildetect;

	// Token: 0x04000725 RID: 1829
	private ParticleSystem smoke;

	// Token: 0x04000726 RID: 1830
	private float keepdig;

	// Token: 0x04000727 RID: 1831
	public Vector3 lastdelta;

	// Token: 0x04000728 RID: 1832
	public Vector3 delta;

	// Token: 0x04000729 RID: 1833
	public Vector3 spd;

	// Token: 0x0400072A RID: 1834
	public Vector3 walkdelta;

	// Token: 0x0400072B RID: 1835
	public Vector3 lastaxis;

	// Token: 0x0400072C RID: 1836
	public Vector3 lastloadzone;

	// Token: 0x0400072D RID: 1837
	public Vector3 dashdelta;

	// Token: 0x0400072E RID: 1838
	[HideInInspector]
	public StaticModelAnim conveyor;

	// Token: 0x0400072F RID: 1839
	private Vector3 dashtarget;

	// Token: 0x04000730 RID: 1840
	private Vector3? tempcamoffset;

	// Token: 0x04000731 RID: 1841
	private SpriteRenderer[] digicon;

	// Token: 0x04000732 RID: 1842
	private const float axistolerance = 0.1f;
}
