using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000042 RID: 66
public class NPCControl : MonoBehaviour
{
	// Token: 0x060006A3 RID: 1699 RVA: 0x0004AE88 File Offset: 0x00049088
	public NPCControl IsDummy()
	{
		this.dummy = true;
		this.entity = base.GetComponent<EntityControl>();
		return this;
	}

	// Token: 0x060006A4 RID: 1700 RVA: 0x0004AEA0 File Offset: 0x000490A0
	private void Start()
	{
		if (!this.dummy)
		{
			if (base.GetComponent<EntityControl>() != null)
			{
				this.entity = base.GetComponent<EntityControl>();
				if (this.entitytype == NPCControl.NPCType.Object && this.objecttype != NPCControl.ObjectTypes.Item)
				{
					this.entity.ccol.enabled = false;
				}
			}
			if (MainManager.player != null && MainManager.player.beemerang != this)
			{
				this.scol = base.gameObject.AddComponent<SphereCollider>();
				this.scol.isTrigger = true;
			}
			else
			{
				this.entity.ccol.enabled = !this.entity.fixedentity;
			}
			this.SetUp();
			if (this.entitytype == NPCControl.NPCType.SemiNPC)
			{
				base.tag = "NPC";
			}
			else
			{
				base.tag = this.entitytype.ToString();
			}
			if (!this.entity.item)
			{
				if (this.entitytype == NPCControl.NPCType.Enemy)
				{
					this.entity.rigid.mass = 100f;
				}
				else
				{
					this.entity.rigid.mass = 10000f;
				}
			}
			if (this.disguiseobj != null)
			{
				this.entity.height = 0f;
				this.disguisecooldown = -1;
				this.entity.sprite.enabled = false;
				this.disguiseobj.gameObject.SetActive(true);
			}
			if (MainManager.CheckIfCanExist(this.requires, this.limit, this.regionalflag) || (this.entity.hideinside && this.insideid != MainManager.instance.insideid))
			{
				base.gameObject.SetActive(false);
			}
			if (this.entitytype == NPCControl.NPCType.Enemy && base.gameObject.activeSelf && !base.name.Contains("NGF"))
			{
				base.StartCoroutine(this.GravityFix());
			}
		}
	}

	// Token: 0x060006A5 RID: 1701 RVA: 0x0004B086 File Offset: 0x00049286
	private IEnumerator GravityFix()
	{
		yield return null;
		if (this.entity != null && this.entity.rigid != null && !this.entity.fixedentity)
		{
			bool g = this.entity.rigid.useGravity;
			bool i = this.entity.rigid.isKinematic;
			float a = 0f;
			RaycastHit raycastHit;
			if (Physics.Raycast(base.transform.position + Vector3.up, Vector3.down, out raycastHit, 5f, 8448) && raycastHit.transform != null)
			{
				base.transform.position = new Vector3(base.transform.position.x, raycastHit.point.y, base.transform.position.z);
			}
			Vector3 pos = base.transform.position;
			while (a < 60f || MainManager.instance.minipause || !this.hasenteredrange)
			{
				this.entity.rigid.useGravity = false;
				this.entity.rigid.isKinematic = true;
				this.entity.rigid.velocity = Vector3.zero;
				a += MainManager.framestep;
				base.transform.position = pos;
				yield return null;
			}
			int num;
			for (int j = 0; j < 5; j = num + 1)
			{
				base.transform.position = pos;
				MainManager.LatePos(base.transform, pos, 0.02f, false);
				this.entity.rigid.velocity = Vector3.zero;
				yield return null;
				num = j;
			}
			this.entity.rigid.useGravity = g;
			this.entity.rigid.isKinematic = i;
			pos = default(Vector3);
		}
		yield break;
	}

	// Token: 0x060006A6 RID: 1702 RVA: 0x0004B098 File Offset: 0x00049298
	private void SetUp()
	{
		if (this.entitytype == NPCControl.NPCType.Object || this.entitytype == NPCControl.NPCType.SemiNPC)
		{
			switch (this.objecttype)
			{
			case NPCControl.ObjectTypes.BeetleGrass:
			case NPCControl.ObjectTypes.CameraChange:
			case NPCControl.ObjectTypes.DoorOtherMap:
			case NPCControl.ObjectTypes.DoorSameMap:
			case NPCControl.ObjectTypes.EventTrigger:
			case NPCControl.ObjectTypes.DialogueTrigger:
			case NPCControl.ObjectTypes.ANDBlock:
			case NPCControl.ObjectTypes.DigSpot:
			case NPCControl.ObjectTypes.BreakableRock:
			case NPCControl.ObjectTypes.MusicRange:
			case NPCControl.ObjectTypes.TempPlatform:
			case NPCControl.ObjectTypes.ResetCamera:
			case NPCControl.ObjectTypes.TriggerSwitch:
			case NPCControl.ObjectTypes.BattleMapChange:
				break;
			case NPCControl.ObjectTypes.PushRock:
				base.tag = "PushRock";
				this.entity.rotater.tag = "Hornable";
				this.entity.alwaysactive = true;
				base.gameObject.layer = 13;
				if (this.scol != null)
				{
					this.scol.enabled = false;
				}
				this.internalvector = new Vector3[2];
				this.internaldata = new float[1];
				if (this.entity.model != null)
				{
					this.internalcollider = this.entity.model.GetComponentsInChildren<Collider>();
					this.entity.model.tag = "PushRock";
					if (this.entity.model.childCount > 0)
					{
						for (int i = 0; i < this.entity.model.childCount; i++)
						{
							this.entity.model.GetChild(i).tag = this.entity.model.tag;
						}
					}
				}
				this.entity.alwaysactive = true;
				this.internalcollider = new Collider[]
				{
					this.entity.model.GetComponent<Collider>()
				};
				this.rotater = new GameObject("rotater").transform;
				this.rotater.parent = base.transform;
				this.rotater.localPosition = Vector3.zero;
				if (this.data[1] == 1)
				{
					this.entity.rigid.constraints = RigidbodyConstraints.None;
				}
				if (this.data.Length > 2 && this.data[2] > 0)
				{
					this.entity.onground = false;
					this.actioncooldown = 300f;
					if (this.boxcol.size.magnitude > 0.1f)
					{
						Transform child = this.entity.model.GetChild(0);
						child.localScale = this.boxcol.size;
						child.localPosition = new Vector3(0f, this.boxcol.size.y / 2f, 0f);
						this.boxcol.size = Vector3.zero;
					}
					this.internalcollider = new Collider[]
					{
						this.entity.model.GetComponent<Collider>()
					};
				}
				this.boxcol.enabled = false;
				goto IL_2755;
			case NPCControl.ObjectTypes.PressurePlate:
			{
				this.entity.alwaysactive = true;
				this.moveobj = this.entity.sprite.transform;
				MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
				if (animIDs != MainManager.AnimIDs.TestButton)
				{
					if (animIDs == MainManager.AnimIDs.AncientPressurePlate)
					{
						this.moveobj = this.entity.model.GetChild(0);
						GlowTrigger glowTrigger = this.moveobj.GetChild(0).gameObject.AddComponent<GlowTrigger>();
						glowTrigger.parent = this;
						glowTrigger.glowparts = new MeshRenderer[]
						{
							glowTrigger.GetComponent<MeshRenderer>()
						};
					}
					else
					{
						this.moveobj = this.entity.model.GetChild(0);
					}
				}
				this.entity.rigid.isKinematic = true;
				if (this.activationflag > -1 && MainManager.instance.flags[this.activationflag])
				{
					this.hit = true;
					this.moveobj.localPosition = this.vectordata[0];
				}
				break;
			}
			case NPCControl.ObjectTypes.ANDGate:
				this.entity.alwaysactive = true;
				break;
			case NPCControl.ObjectTypes.Item:
			{
				this.entity.onground = false;
				this.entity.alwaysactive = true;
				this.entity.ccol.material.bounciness = 0.75f;
				base.gameObject.layer = 0;
				NPCControl[] array = Object.FindObjectsOfType<NPCControl>();
				GameObject[] array2 = GameObject.FindGameObjectsWithTag("PFollower");
				for (int j = 0; j < array2.Length; j++)
				{
					Physics.IgnoreCollision(this.entity.ccol, array2[j].GetComponent<EntityControl>().ccol, true);
				}
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k].entitytype == NPCControl.NPCType.Object && array[k].objecttype == NPCControl.ObjectTypes.Item)
					{
						Physics.IgnoreCollision(this.entity.ccol, array[k].entity.ccol, true);
					}
				}
				for (int l = 1; l < MainManager.instance.playerdata.Length; l++)
				{
					Physics.IgnoreCollision(MainManager.instance.playerdata[l].entity.ccol, this.entity.ccol, true);
				}
				if (this.entity.animid == 3 && !this.tempobject)
				{
					this.data[0] = this.data[3];
					if (MainManager.instance.crystalbflags[this.data[0]])
					{
						this.entity.iskill = true;
					}
					else
					{
						this.entity.animstate = this.regionalflag;
						this.entity.sprite.transform.localPosition = Vector3.zero;
						this.entity.AddModel("Prefabs/Objects/CrystalBerry", Vector3.zero);
						this.entity.spin = new Vector3(0f, 2f, 0f);
						this.entity.item = true;
						this.entity.rigid.useGravity = true;
					}
				}
				this.AddPlayerTrigger();
				goto IL_2755;
			}
			case NPCControl.ObjectTypes.SetPlayerRespawn:
				if (this.vectordata[0].magnitude < 0.1f)
				{
					this.boxcol = new GameObject("Respawner").AddComponent<BoxCollider>();
					this.boxcol.transform.position = this.entity.startpos.Value;
					this.boxcol.transform.eulerAngles = base.transform.eulerAngles;
					this.boxcol.isTrigger = true;
					this.boxcol.transform.parent = MainManager.map.transform;
					this.boxcol.gameObject.isStatic = true;
					this.boxcol.tag = "Respawn";
					Object.Destroy(base.gameObject);
					goto IL_2755;
				}
				this.boxcol.gameObject.layer = 0;
				goto IL_2755;
			case NPCControl.ObjectTypes.Beemerang:
			{
				this.entity.sound.loop = true;
				this.entity.PlaySound("RangHold", 0.5f);
				GameObject[] array3 = GameObject.FindGameObjectsWithTag("Respawn");
				for (int m = 0; m < array3.Length; m++)
				{
					Collider component = array3[m].GetComponent<Collider>();
					if (component != null)
					{
						Physics.IgnoreCollision(component, this.entity.ccol, true);
					}
					if (this.scol != null)
					{
						Physics.IgnoreCollision(component, this.scol, true);
					}
				}
				if (MainManager.map.entityonly != null && MainManager.map.entityonly.Length != 0)
				{
					for (int n = 0; n < MainManager.map.entityonly.Length; n++)
					{
						if (MainManager.map.entityonly[n] != null)
						{
							Physics.IgnoreCollision(MainManager.map.entityonly[n], this.entity.ccol, true);
							if (this.scol != null)
							{
								Physics.IgnoreCollision(MainManager.map.entityonly[n], this.scol, true);
							}
						}
					}
					goto IL_2755;
				}
				goto IL_2755;
			}
			case NPCControl.ObjectTypes.SavePoint:
				base.gameObject.isStatic = true;
				if (this.scol != null)
				{
					this.scol.enabled = false;
				}
				this.entity.ccol.enabled = true;
				this.entity.ccol.radius = 1f;
				this.entity.overrideanim = true;
				this.entity.rigid.isKinematic = true;
				this.colliderheight = 3f;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				this.entity.rigid.useGravity = false;
				this.interacttype = NPCControl.Interaction.SavePoint;
				this.internaltransform = new Transform[2];
				this.internaltransform[0] = this.entity.sprite.transform.GetChild(0).GetChild(1);
				if (this.data[0] == 1)
				{
					Transform transform = (Object.Instantiate(Resources.Load("Prefabs/Objects/PolySphere")) as GameObject).transform;
					transform.parent = this.internaltransform[0];
					transform.transform.localPosition = new Vector3(0f, 0f, 0.01f);
					transform.transform.localScale = new Vector3(8f, 8f, 5f);
					this.internaltransform[1] = transform.transform;
					this.internalrender = transform.GetComponentsInChildren<MeshRenderer>();
					for (int num = 0; num < this.internalrender.Length; num++)
					{
						this.internalrender[num].material.renderQueue = 3000 + (int)(transform.transform.position.z * 100f);
						if (this.data[2] == 0)
						{
							this.internalrender[num].material.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, this.internalrender[num].material.color.a);
						}
					}
				}
				if (this.data[1] >= 10)
				{
					MeshRenderer component2 = this.entity.sprite.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>();
					component2.material.color = new Color(Color.red.r, Color.red.g, Color.red.b, component2.material.color.a);
					component2.material.SetColor("_Emission", Color.red);
					goto IL_2755;
				}
				if (this.data[2] == 0)
				{
					MeshRenderer component3 = this.entity.sprite.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>();
					component3.material.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, component3.material.color.a);
					component3.material.SetColor("_Emission", Color.yellow);
					goto IL_2755;
				}
				goto IL_2755;
			case NPCControl.ObjectTypes.JumpSpring:
				this.entity.activeonpause = true;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				this.entity.rigid.isKinematic = true;
				this.internalvector = new Vector3[]
				{
					this.boxcol.center,
					new Vector3(0f, 555f, 999f)
				};
				break;
			case NPCControl.ObjectTypes.Switch:
			case NPCControl.ObjectTypes.ScrewSwitch:
			case NPCControl.ObjectTypes.StencilSwitch:
			case NPCControl.ObjectTypes.WaterSwitch:
			{
				this.entity.rigid.isKinematic = true;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				this.entity.rigid.useGravity = false;
				this.nointeract = true;
				this.entity.alwaysactive = true;
				if (this.activationflag > -1 && MainManager.instance.flags[this.activationflag])
				{
					this.hit = true;
				}
				MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
				if (animIDs <= MainManager.AnimIDs.BigCrystalSwitch)
				{
					if (animIDs == MainManager.AnimIDs.SwitchCrystal || animIDs == MainManager.AnimIDs.BigCrystalSwitch)
					{
						GlowTrigger glowTrigger2 = this.entity.model.GetChild(0).gameObject.AddComponent<GlowTrigger>();
						glowTrigger2.parent = this;
						glowTrigger2.glowparts = new MeshRenderer[]
						{
							glowTrigger2.GetComponent<MeshRenderer>()
						};
					}
				}
				else if (animIDs == MainManager.AnimIDs.WoodenSwitch || animIDs == MainManager.AnimIDs.SteelSwitch)
				{
					this.internaldata = new float[]
					{
						(float)((this.entity.originalid + 1 == 347) ? -100 : -60)
					};
					this.moveobj = this.entity.model.transform.GetChild(0);
					if (this.hit)
					{
						this.moveobj.transform.localEulerAngles = new Vector3(0f, this.internaldata[0]);
					}
				}
				if (MainManager.player != null)
				{
					Physics.IgnoreCollision(this.boxcol, MainManager.player.entity.detect, true);
				}
				if (this.entity.originalid + 1 != 55 && this.entity.originalid + 1 != 106 && this.entity.originalid + 1 != 347)
				{
					this.AddPusher();
				}
				if (this.objecttype == NPCControl.ObjectTypes.Switch)
				{
					if (this.data.Length > 4 && this.data[4] == 1)
					{
						this.entity.rotater.tag = "Hornable";
					}
					if (this.data[1] == 1 && this.activationflag > -1)
					{
						this.hit = MainManager.instance.flags[this.activationflag];
						goto IL_2755;
					}
					goto IL_2755;
				}
				else
				{
					if (this.objecttype == NPCControl.ObjectTypes.StencilSwitch && !MainManager.CheckIfCanExist(this.requires, this.limit, this.regionalflag))
					{
						this.entity.activeonpause = true;
						this.entity.alwaysactive = true;
						this.internaltransform = new Transform[]
						{
							new GameObject("ice radius from " + base.name).transform
						};
						this.internaltransform[0].tag = "IceRadius";
						(Object.Instantiate(Resources.Load("Prefabs/Particles/IceRadius")) as GameObject).transform.parent = this.internaltransform[0];
						SphereCollider sphereCollider = this.internaltransform[0].gameObject.AddComponent<SphereCollider>();
						sphereCollider.radius = 0.8f;
						sphereCollider.isTrigger = true;
						if (this.data[2] == 1)
						{
							this.hit = true;
						}
						Physics.IgnoreCollision(this.boxcol, sphereCollider, true);
						if (this.data[1] > -1)
						{
							EntityControl entityControl = MainManager.GetEntity(this.data[1]);
							if (entityControl.model != null)
							{
								base.transform.parent = entityControl.model.transform;
							}
							else
							{
								base.transform.parent = entityControl.transform;
							}
							base.transform.position = entityControl.transform.position + this.vectordata[1];
							this.entity.startpos = new Vector3?(base.transform.position);
						}
						if (this.data[3] == 1 && this.entity.model != null)
						{
							Collider component4 = this.entity.model.GetComponent<Collider>();
							if (component4 != null)
							{
								component4.enabled = false;
							}
							base.Invoke("DisableAllColliders", 0.1f);
						}
						this.internaltransform[0].parent = MainManager.map.transform;
						this.internaltransform[0].position = base.transform.position;
						goto IL_2755;
					}
					if (this.objecttype == NPCControl.ObjectTypes.WaterSwitch)
					{
						this.entity.alwaysactive = true;
						this.internaltransform = new Transform[]
						{
							MainManager.map.mainmesh.GetChild(this.data[0])
						};
						this.data[0] = (int)this.vectordata[0].x;
						Vector3 vector = this.vectordata[0];
						this.vectordata = new Vector3[]
						{
							vector,
							new Vector3(this.internaltransform[0].position.x, vector.z, this.internaltransform[0].position.z),
							new Vector3(this.internaltransform[0].position.x, vector.y, this.internaltransform[0].position.z),
							new Vector3((float)this.data[0], 0f)
						};
						this.internaltransform[0].gameObject.isStatic = false;
						if (this.hit)
						{
							this.vectordata[0].x = this.vectordata[3].x;
						}
						else
						{
							this.vectordata[0] = Vector3.zero;
						}
						this.internaltransform[0].transform.position = new Vector3(this.internaltransform[0].transform.position.x, (!this.hit) ? vector.z : vector.y, this.internaltransform[0].transform.position.z);
						goto IL_2755;
					}
					goto IL_2755;
				}
				break;
			}
			case NPCControl.ObjectTypes.MusicChange:
			case NPCControl.ObjectTypes.DigWall:
			case NPCControl.ObjectTypes.ItemSpawner:
				goto IL_2755;
			case NPCControl.ObjectTypes.CoiledObject:
				this.entity.alwaysactive = true;
				this.entity.rigid.isKinematic = true;
				break;
			case NPCControl.ObjectTypes.FixedAnim:
				if (this.data[0] == 1)
				{
					this.entity.ccol.enabled = false;
					this.entity.rigid.useGravity = false;
				}
				else
				{
					this.entity.ccol.enabled = true;
				}
				this.entity.overrridejump = true;
				this.entity.overrideanim = true;
				this.entity.animstate = this.data[1];
				if (this.scol != null)
				{
					this.scol.enabled = false;
					goto IL_2755;
				}
				goto IL_2755;
			case NPCControl.ObjectTypes.EnemySpawner:
				this.entity.rigid.isKinematic = true;
				base.gameObject.isStatic = true;
				if (this.data[1] == 1)
				{
					this.actioncooldown = 0f;
				}
				else
				{
					this.actioncooldown = (float)this.data[4];
				}
				this.data[5] = this.entity.animid;
				this.entity.animid = -1;
				this.entity.sprite.enabled = false;
				this.entity.ccol.enabled = false;
				this.entity.alwaysactive = true;
				goto IL_2755;
			case NPCControl.ObjectTypes.Dropplet:
			{
				this.actionfrequency = new float[3];
				this.entity.hasshadow = true;
				this.entity.overrideshadow = true;
				if (this.entity.shadow == null)
				{
					this.entity.CreateShadow();
				}
				this.entity.alwaysactive = true;
				this.entity.activeonpause = true;
				this.internaltransform = new Transform[]
				{
					(Object.Instantiate(Resources.Load("Prefabs/Objects/icecube")) as GameObject).transform
				};
				this.internaltransform[0].transform.position = new Vector3(0f, -1000f, 0f);
				this.internaltransform[0].gameObject.layer = 13;
				this.internaltransform[0].localScale = Vector3.one * 1.5f;
				this.internaltransform[0].gameObject.AddComponent<Rigidbody>().isKinematic = true;
				this.internaltransform[0].gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
				this.internaltransform[0].GetComponent<Rigidbody>().mass = 100000f;
				this.internaltransform[0].parent = MainManager.map.transform;
				BoxCollider component5 = this.internaltransform[0].GetComponent<BoxCollider>();
				component5.enabled = true;
				component5.isTrigger = false;
				component5.size = Vector3.one;
				this.internaltransform[0].gameObject.AddComponent<DialogueAnim>().SetUp(Vector3.one * 1.5f, Vector3.one * 1.5f, Vector3.zero, 0.01f);
				this.internaltransform[0].gameObject.AddComponent<Hornable>().SetUp(new Vector2(this.vectordata[1].x, this.vectordata[1].y), false, true, this);
				this.internaltransform[0].GetChild(0).tag = "DroppletCube";
				BoxCollider boxCollider = this.internaltransform[0].GetChild(0).gameObject.AddComponent<BoxCollider>();
				boxCollider.size = Vector3.one * 0.01f;
				boxCollider.isTrigger = true;
				Physics.IgnoreCollision(boxCollider, MainManager.player.entity.ccol, true);
				Physics.IgnoreCollision(boxCollider, component5, true);
				this.vectordata[0] += this.entity.startpos.Value;
				this.internalparticle = new ParticleSystem[1];
				SpriteBounce component6 = (Object.Instantiate(Resources.Load("Prefabs/Objects/WaterBubble")) as GameObject).GetComponent<SpriteBounce>();
				component6.transform.parent = this.entity.sprite.transform;
				component6.transform.localPosition = Vector3.up / 2f;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
				this.entity.rigid.useGravity = true;
				this.entity.onground = false;
				goto IL_2755;
			}
			case NPCControl.ObjectTypes.PathPlatform:
			case NPCControl.ObjectTypes.RotatingPlatform:
			case NPCControl.ObjectTypes.Geizer:
			{
				this.entity.rigid.useGravity = false;
				base.gameObject.isStatic = false;
				this.nointeract = true;
				this.entity.soundonpause = true;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				this.entity.rigid.isKinematic = true;
				if (this.objecttype == NPCControl.ObjectTypes.RotatingPlatform)
				{
					this.entity.model.eulerAngles = this.vectordata[0];
				}
				if (this.objecttype == NPCControl.ObjectTypes.Geizer)
				{
					this.entity.alwaysactive = true;
					if (this.scol != null)
					{
						this.scol.enabled = false;
					}
					base.gameObject.layer = 0;
					float[] array4 = new float[2];
					array4[0] = (float)Random.Range(1, 10);
					this.actionfrequency = array4;
					this.actioncooldown = -1100f;
					this.internaltransform = new Transform[5];
					string str = "";
					if (this.data.Length != 0 && this.data[0] > 0)
					{
						str = this.data[0].ToString();
					}
					(Object.Instantiate(Resources.Load("Prefabs/Objects/Geizer" + str), this.entity.sprite.transform) as GameObject).transform.localPosition = Vector3.zero;
					for (int num2 = 0; num2 < 2; num2++)
					{
						this.internaltransform[num2] = this.entity.sprite.transform.GetChild(0).GetChild(num2);
						this.internaltransform[2 + num2] = this.internaltransform[0].GetChild(num2);
					}
					this.internaltransform[4] = this.internaltransform[1].GetChild(0);
					goto IL_2755;
				}
				if (this.dialogues != null && this.dialogues.Length > 2 && this.dialogues[2].x > 0.1f)
				{
					this.entity.model.transform.localScale *= this.dialogues[2].x / 10f;
				}
				if (this.objecttype == NPCControl.ObjectTypes.PathPlatform)
				{
					if ((int)this.dialogues[1].x == 1)
					{
						if ((int)this.dialogues[0].x == 1)
						{
							this.speedmultiplier = 1f;
						}
					}
					else
					{
						this.currentnode = (int)this.dialogues[0].x;
						base.transform.position = this.vectordata[this.currentnode];
						this.entity.startpos = new Vector3?(base.transform.position);
					}
					if (this.entity.originalid + 1 == 243)
					{
						this.scol.enabled = false;
						this.boxcol = base.gameObject.AddComponent<BoxCollider>();
						this.boxcol.size = new Vector3(5f, 1f, 5f);
						this.boxcol.isTrigger = true;
					}
				}
				this.entity.model.tag = "PlatformNoClock";
				this.entity.alwaysactive = true;
				if (this.entity.originalid != 190)
				{
					goto IL_2755;
				}
				GlowTrigger glowTrigger3 = this.entity.model.GetChild(0).gameObject.AddComponent<GlowTrigger>();
				glowTrigger3.getactivecolorfromstart = true;
				glowTrigger3.parent = this;
				glowTrigger3.glowparts = new MeshRenderer[]
				{
					glowTrigger3.GetComponent<MeshRenderer>()
				};
				if (this.dialogues.Length < 3 || (int)this.dialogues[2].y == 0)
				{
					glowTrigger3.electime = 260f;
					goto IL_2755;
				}
				glowTrigger3.electime = this.dialogues[2].y;
				goto IL_2755;
			}
			case NPCControl.ObjectTypes.RollingRock:
				this.entity.onground = false;
				this.scol.enabled = true;
				this.scol.isTrigger = true;
				this.entity.alwaysactive = true;
				this.entity.ccol.enabled = true;
				if (this.vectordata[1].y < 0.1f)
				{
					this.vectordata[1] = new Vector3(this.vectordata[1].x, 3f, this.vectordata[1].z);
				}
				this.scol.center = new Vector3(0f, this.vectordata[1].y);
				this.entity.model.transform.localScale = Vector3.one * this.vectordata[1].y;
				this.entity.model.transform.localPosition = new Vector3(0f, this.vectordata[1].y);
				this.scol.radius = this.vectordata[1].y;
				this.entity.jumpheight /= 2f;
				this.internaldata = new float[]
				{
					0f,
					100f
				};
				if (MainManager.player != null)
				{
					Physics.IgnoreCollision(this.entity.ccol, MainManager.player.entity.ccol, true);
				}
				if (this.data.Length > 2 && this.data[2] == 1)
				{
					this.actioncooldown = this.vectordata[1].z;
					this.internaltransform = new Transform[2];
					this.internaltransform[0] = (Object.Instantiate(Resources.Load("Prefabs/Objects/Cannon")) as GameObject).transform;
					this.internaltransform[1] = this.internaltransform[0].GetChild(0);
					this.internaltransform[0].position = this.entity.startpos.Value;
					this.internaltransform[0].localScale = Vector3.one * (this.vectordata[1].y / 2f);
					base.transform.position = new Vector3(0f, 999f);
					this.entity.LockRigid(true);
					this.internaltransform[0].parent = MainManager.map.transform;
					MainManager.FaceTowardsY(this.internaltransform[0], this.internaltransform[0].transform.position + this.vectordata[0]);
					this.internaltransform[0].localEulerAngles += new Vector3(0f, 0f, -90f);
				}
				this.entity.CreateFeet();
				goto IL_2755;
			case NPCControl.ObjectTypes.WindPusher:
			{
				this.entity.alwaysactive = true;
				this.boxcol = base.gameObject.AddComponent<BoxCollider>();
				this.boxcol.isTrigger = true;
				float num3 = Vector3.Distance(base.transform.position, this.vectordata[0]);
				this.internalvector = new Vector3[1];
				MainManager.FaceTowardsY(base.transform, this.vectordata[0]);
				this.internalparticle = new ParticleSystem[]
				{
					(Object.Instantiate(Resources.Load("Prefabs/Particles/WindFunnel")) as GameObject).GetComponent<ParticleSystem>()
				};
				this.internalparticle[0].transform.parent = base.transform;
				this.internalparticle[0].transform.localEulerAngles = Vector3.zero;
				this.internalparticle[0].transform.localPosition = Vector3.up / 2f;
				if (MainManager.GetDistance(this.vectordata[0].y, base.transform.position.y) > 3f)
				{
					this.boxcol.size = new Vector3(this.vectordata[1].y, num3, this.vectordata[1].z);
					this.boxcol.center = new Vector3(0f, num3 / 2f, 0f);
					this.internalvector[0] = base.transform.up;
					this.internalparticle[0].transform.localEulerAngles = new Vector3(-90f, 0f);
				}
				else
				{
					this.boxcol.size = new Vector3(this.vectordata[1].y, this.vectordata[1].z, num3);
					this.boxcol.center = new Vector3(0f, 0f, num3 / 2f);
					this.internalvector[0] = base.transform.forward;
				}
				ParticleSystem.MainModule main = this.internalparticle[0].main;
				if (this.vectordata.Length < 3 || this.vectordata[2].x < 0.1f)
				{
					main.startLifetime = num3 / 5f;
				}
				else
				{
					main.startLifetime = this.vectordata[2].x;
				}
				if (this.vectordata.Length < 3 || this.vectordata[2].y < 0.1f)
				{
					main.startSpeed = new ParticleSystem.MinMaxCurve(main.startSpeed.constant * this.vectordata[1].x * 20f);
				}
				else
				{
					main.startSpeed = new ParticleSystem.MinMaxCurve(this.vectordata[2].y);
				}
				if (this.data[0] > -1 && !MainManager.GetEntity(this.data[0]).npcdata.hit)
				{
					this.internalparticle[0].Stop();
					main.prewarm = false;
					goto IL_2755;
				}
				this.internalparticle[0].Simulate(1f);
				this.internalparticle[0].Play();
				goto IL_2755;
			}
			default:
				goto IL_2755;
			}
			this.entity.alwaysactive = true;
			base.gameObject.isStatic = true;
			this.entity.rigid.isKinematic = true;
			this.nointeract = true;
			this.entity.rigid.useGravity = false;
			if (this.objecttype == NPCControl.ObjectTypes.ANDBlock)
			{
				this.entity.activeonpause = true;
				this.entity.alwaysactive = true;
				this.entity.lockrotater = true;
				this.entity.transform.localEulerAngles = Vector3.zero;
				base.gameObject.layer = 8;
				if (this.boxcol != null)
				{
					this.boxcol.material = MainManager.defaultpmat;
				}
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				if (this.vectordata.Length > 2 && this.vectordata[2].magnitude > 0.1f)
				{
					this.entity.startscale = this.vectordata[2];
					this.entity.sprite.transform.localScale = this.vectordata[2];
				}
				if (this.entity.originalid + 1 == 345 || this.entity.originalid + 1 == 346)
				{
					this.internaltransform = new Transform[]
					{
						this.entity.model.GetChild(1)
					};
					this.internaltransform[0].parent = MainManager.map.transform;
				}
			}
			else
			{
				base.gameObject.layer = 0;
			}
			if (this.scol != null)
			{
				this.scol.enabled = false;
			}
			if (this.objecttype == NPCControl.ObjectTypes.BeetleGrass)
			{
				if (this.data.Length > 1 && this.data[1] > -1 && MainManager.instance.crystalbflags[this.data[1]])
				{
					this.entity.iskill = true;
				}
				else
				{
					this.entity.rotater.tag = "Hornable";
					base.gameObject.layer = 8;
					this.boxcol.material = MainManager.defaultpmat;
					this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
					this.entity.overrideanim = true;
					this.entity.sprite.enabled = true;
					this.entity.sprite.sprite = MainManager.grasssprite[this.data[0] * 3];
					this.entity.sprite.shadowCastingMode = ShadowCastingMode.TwoSided;
					if (!MainManager.nowindeffect)
					{
						this.entity.sprite.material = MainManager.windShader;
						MainManager.RefreshWind(this.entity.sprite);
					}
				}
			}
			else if (this.objecttype == NPCControl.ObjectTypes.BreakableRock)
			{
				this.entity.AddModel("Prefabs/Objects/CrackedRock", new Vector3(-1f, 0f, 1.5f));
				Renderer component7 = this.entity.model.GetComponent<Renderer>();
				switch (this.data[0])
				{
				case 0:
					component7.material.color = Color.white;
					break;
				case 1:
					component7.material.color = new Color(0.93f, 0.57f, 0.13f);
					break;
				case 2:
					component7.material.color = new Color(0.88f, 0.85f, 0.45f);
					break;
				case 3:
					component7.material.color = new Color(0.74f, 0.82f, 0.93f);
					break;
				case 4:
					component7.material.color = new Color(0.52f, 0.75f, 0.42f);
					break;
				case 5:
					component7.material.color = new Color(0.63f, 0.25f, 0.44f);
					break;
				}
				base.gameObject.tag = "DroppletPass";
				this.entity.rotater.tag = "Hornable";
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				this.boxcol.center = new Vector3(0f, 2.5f);
				this.boxcol.size = new Vector3(5f, 5f, 5f);
				base.gameObject.layer = 13;
				this.AddPusher();
				this.pusher.height = 0f;
				this.pusher.radius = 3.5f;
				this.pusher.transform.localPosition = new Vector3(0f, 3f);
			}
			else if (this.objecttype == NPCControl.ObjectTypes.EventTrigger || this.objecttype == NPCControl.ObjectTypes.DialogueTrigger || this.objecttype == NPCControl.ObjectTypes.TriggerSwitch)
			{
				this.entity.alwaysactive = true;
			}
			else if (this.objecttype == NPCControl.ObjectTypes.MusicRange)
			{
				this.entity.alwaysactive = true;
				this.entity.activeonpause = true;
				this.data[0] = this.data[1];
				this.entity.sound.clip = Resources.Load<AudioClip>("Audio/Music/" + Enum.GetNames(typeof(MainManager.Musics))[this.data[2]]);
				this.entity.sound.volume = 0f;
				this.entity.sound.loop = true;
				this.entity.sound.Play();
				this.vectordata[1] = new Vector3(0f, MainManager.musicloop[this.data[2]][0], MainManager.musicloop[this.data[2]][1]);
			}
			else if (this.objecttype == NPCControl.ObjectTypes.TempPlatform)
			{
				this.entity.model.tag = "Platform";
			}
			else if (this.objecttype == NPCControl.ObjectTypes.DigSpot && this.data[0] == 1 && MainManager.instance.crystalbflags[this.data[1]])
			{
				this.entity.iskill = true;
			}
		}
		else if (this.entitytype == NPCControl.NPCType.Enemy)
		{
			this.SetInitialBehavior();
			if (this.scol != null)
			{
				this.scol.enabled = false;
			}
			this.AddPlayerTrigger();
			if (MainManager.player != null && MainManager.player.entity.detect != null)
			{
				Physics.IgnoreCollision(this.entity.ccol, MainManager.player.entity.detect, true);
				Physics.IgnoreCollision(this.secondcoll, MainManager.player.entity.detect, true);
			}
			this.arrow = HelpArrow.NewArrow(base.transform, Vector3.up * 0.75f, Color.cyan, Mathf.Clamp(this.entity.freezesize.x * 2f, 2.5f, this.entity.freezesize.x * 3.5f), 1.5f);
			if (this.eventid > 0)
			{
				this.entity.alwaysactive = true;
			}
		}
		else if (this.entitytype == NPCControl.NPCType.NPC)
		{
			if (this.scol != null)
			{
				this.scol.enabled = false;
			}
			if (!this.entity.item && this.entity.animid > -1 && this.interacttype != NPCControl.Interaction.Shop && this.interacttype != NPCControl.Interaction.CaravanBadge)
			{
				this.GetDialogue();
				this.AddPusher();
				this.SetInitialBehavior();
			}
		}
		IL_2755:
		if (this.entitytype != NPCControl.NPCType.Object && this.entitytype != NPCControl.NPCType.SemiNPC)
		{
			if (this.entitytype == NPCControl.NPCType.NPC && this.radius < 1.75f)
			{
				this.radius += 0.35f;
			}
			if (this.HasDisguiseBehavior())
			{
				this.entity.alwaysactive = true;
				this.disguiseobj = (Object.Instantiate(Resources.Load("Prefabs/Disguises/" + (this.entity.animid + MainManager.AnimIDs.Bee))) as GameObject).transform;
				this.disguiseobj.transform.parent = this.entity.sprite.transform;
				this.entity.sprite.enabled = false;
				this.disguiseobj.transform.localPosition = Vector3.zero;
				MainManager.AnimIDs animIDs = this.entity.animid + MainManager.AnimIDs.Bee;
				if (animIDs <= MainManager.AnimIDs.Cactus)
				{
					if (animIDs != MainManager.AnimIDs.Mushroom)
					{
						if (animIDs == MainManager.AnimIDs.Cactus)
						{
							this.disguiseobj.transform.localScale = new Vector3(70f, 25f, 70f);
						}
					}
					else
					{
						this.disguiseobj.transform.localScale = new Vector3(20f, 15f, 25f);
					}
				}
				else if (animIDs != MainManager.AnimIDs.CursedSkull)
				{
					if (animIDs == MainManager.AnimIDs.Plumpling)
					{
						this.disguiseobj.transform.localScale = Vector3.one * 0.2f;
						this.disguiseobj.transform.localEulerAngles = new Vector3(-90f, 0f, 180f);
					}
				}
				else
				{
					this.disguiseobj.transform.localScale = Vector3.one * 0.5f;
					this.disguiseobj.transform.localPosition = new Vector3(0f, 0f, -0.5f);
					this.disguiseobj.transform.localEulerAngles = new Vector3(-90f, 0f);
				}
			}
			if (NPCControl.HasBehavior(NPCControl.ActionBehaviors.StealthAI, this))
			{
				if (this.entity.detect == null)
				{
					this.entity.CreateDetector();
				}
				StealthCheck stealthCheck = new GameObject().AddComponent<StealthCheck>();
				stealthCheck.transform.parent = this.entity.detect.transform;
				stealthCheck.transform.localPosition = Vector3.zero;
				this.entity.alwaysactive = true;
				stealthCheck.parent = this;
				this.entity.extratimer = true;
			}
			if (this.HasBehavior(NPCControl.ActionBehaviors.SetPath) || this.HasBehavior(NPCControl.ActionBehaviors.StealthAI))
			{
				this.entity.alwaysactive = true;
			}
			if (this.behaviors != null && this.behaviors.Length != 0 && this.behaviors[0] == NPCControl.ActionBehaviors.Wander)
			{
				this.actioncooldown = 120f;
			}
		}
		else if (this.interacttype == NPCControl.Interaction.Shop && this.shopkeeper != null)
		{
			this.mmulti = ((this.shopkeeper.dialogues[2].y <= 0.1f) ? 1f : (this.shopkeeper.dialogues[2].y / 10f));
		}
		if (this.interacttype == NPCControl.Interaction.Shop || this.interacttype == NPCControl.Interaction.CaravanBadge)
		{
			this.CaravanMedalSet(false);
		}
		else if (this.interacttype == NPCControl.Interaction.ShopKeeper && (int)this.dialogues[10].x == 1)
		{
			this.SetBadgeShop(false);
		}
		if (this.entity.item)
		{
			this.colliderheight = 1f;
			this.entity.ccol.center = new Vector3(0f, 0.5f);
		}
		base.Invoke("CheckHidden", 1f);
		if (this.freezeconstraints || this.entity.fixedentity)
		{
			this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
		}
		this.entity.initialcolliderdata = new Vector2(this.entity.ccol.height, this.entity.ccol.radius);
	}

	// Token: 0x060006A7 RID: 1703 RVA: 0x0004DC08 File Offset: 0x0004BE08
	private void CheckHidden()
	{
		if (this.HasHiddenItem() && !MainManager.CheckIfCanExist(this.requires, this.limit, this.regionalflag))
		{
			MainManager.map.hiddenitem = new int?(100);
			MonoBehaviour.print(string.Concat(new object[]
			{
				base.name,
				" (",
				this.mapid,
				")"
			}));
		}
	}

	// Token: 0x060006A8 RID: 1704 RVA: 0x0004DC80 File Offset: 0x0004BE80
	private bool HasHiddenItem()
	{
		if (this.entitytype == NPCControl.NPCType.Object && MainManager.BadgeIsEquipped(2) && this.interacttype != NPCControl.Interaction.CaravanBadge && this.interacttype != NPCControl.Interaction.Shop && !base.name.Contains("NDTCT") && (!base.name.Contains("DDIST") || Mathf.Abs(MainManager.player.transform.position.z - base.transform.position.z) < 20f))
		{
			NPCControl.ObjectTypes objectTypes = this.objecttype;
			if (objectTypes != NPCControl.ObjectTypes.BeetleGrass)
			{
				if (objectTypes != NPCControl.ObjectTypes.Item)
				{
					if (objectTypes == NPCControl.ObjectTypes.DigSpot)
					{
						if (this.data[0] == 1)
						{
							if (!MainManager.instance.crystalbflags[this.data[1]])
							{
								return true;
							}
						}
						else if (this.data[1] == 1)
						{
							return this.data[2] == 52;
						}
					}
				}
				else
				{
					int animid = this.entity.animid;
					if (animid - 1 <= 1)
					{
						return this.activationflag > -1 && !MainManager.instance.flags[this.activationflag];
					}
					if (animid == 3)
					{
						return !MainManager.instance.crystalbflags[this.data[0]];
					}
				}
			}
			else if (this.data.Length > 1 && this.data[1] > -1 && !MainManager.instance.crystalbflags[this.data[1]])
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060006A9 RID: 1705 RVA: 0x0004DDF1 File Offset: 0x0004BFF1
	private bool HasDisguiseBehavior()
	{
		return this.HasBehavior(NPCControl.ActionBehaviors.Disguise) || this.HasBehavior(NPCControl.ActionBehaviors.DisguiseOnce) || this.HasBehavior(NPCControl.ActionBehaviors.DisguiseOnceJumpForward);
	}

	// Token: 0x060006AA RID: 1706 RVA: 0x0004DE10 File Offset: 0x0004C010
	private void DisableAllColliders()
	{
		if (this.boxcol != null)
		{
			this.boxcol.enabled = false;
		}
		if (this.entity.ccol != null)
		{
			this.entity.ccol.enabled = false;
		}
		if (this.scol != null)
		{
			this.scol.enabled = false;
		}
		if (this.pusher != null)
		{
			this.pusher.gameObject.SetActive(false);
		}
	}

	// Token: 0x060006AB RID: 1707 RVA: 0x0004DE94 File Offset: 0x0004C094
	private void SetInitialBehavior()
	{
		MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
		if (animIDs == MainManager.AnimIDs.BeeGuard && this.tattleid == -1)
		{
			this.tattleid = -97;
		}
		if (MainManager.player != null && !MainManager.instance.inevent)
		{
			NPCControl.ActionBehaviors actionBehaviors = this.behaviors[0];
			if (actionBehaviors <= NPCControl.ActionBehaviors.TurnRandomly)
			{
				if (actionBehaviors == NPCControl.ActionBehaviors.FacePlayer)
				{
					this.entity.FacePlayer();
					this.entity.Invoke("FacePlayer", 0.5f);
					this.entity.Invoke("FacePlayer", 1f);
					return;
				}
				if (actionBehaviors != NPCControl.ActionBehaviors.TurnRandomly)
				{
					return;
				}
			}
			else
			{
				switch (actionBehaviors)
				{
				case NPCControl.ActionBehaviors.TurnFixedInterval:
					break;
				case NPCControl.ActionBehaviors.Disguise:
				case NPCControl.ActionBehaviors.DisguiseOnce:
				case NPCControl.ActionBehaviors.FollowPlayer:
				case NPCControl.ActionBehaviors.WalkAwayFromPlayer:
					return;
				case NPCControl.ActionBehaviors.FaceAhead:
					this.entity.FaceAhead();
					this.entity.Invoke("FaceAhead", 0.5f);
					this.entity.Invoke("FaceAhead", 1f);
					return;
				case NPCControl.ActionBehaviors.FaceBehind:
					this.entity.FaceBehind();
					this.entity.Invoke("FaceBehind", 0.5f);
					this.entity.Invoke("FaceBehind", 1f);
					return;
				case NPCControl.ActionBehaviors.FaceUp:
					this.entity.FaceUp();
					this.entity.Invoke("FaceUp", 0.5f);
					this.entity.Invoke("FaceUp", 1f);
					return;
				case NPCControl.ActionBehaviors.FaceDown:
					this.entity.FaceDown();
					this.entity.Invoke("FaceDown", 0.5f);
					this.entity.Invoke("FaceDown", 1f);
					return;
				default:
					if (actionBehaviors != NPCControl.ActionBehaviors.WanderUnderground)
					{
						return;
					}
					this.entity.InstantDig();
					return;
				}
			}
			this.actioncooldown = this.actionfrequency[0];
			return;
		}
	}

	// Token: 0x060006AC RID: 1708 RVA: 0x0004E05C File Offset: 0x0004C25C
	private void CaravanMedalSet(bool reroll)
	{
		if (this.interacttype == NPCControl.Interaction.CaravanBadge)
		{
			this.shopkeeper = MainManager.GetEntity(this.data[0]).npcdata;
			int[] array = MainManager.PrizeBadges(true);
			if (array == null)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				MainManager.RandomSort(ref array);
				MainManager.caravanorder = array;
				this.entity.item = true;
				this.entity.animid = 2;
				this.entity.animstate = array[0];
				this.entity.itemstate = this.entity.animstate;
				this.entitytype = NPCControl.NPCType.SemiNPC;
				if (reroll)
				{
					MainManager.DeathSmoke(base.transform.position);
				}
				List<int> list = new List<int>(MainManager.caravanorder);
				list.RemoveAt(0);
				MainManager.caravanorder = list.ToArray();
			}
		}
		this.entity.rigid.useGravity = false;
		this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
		this.entity.transform.position = this.entity.startpos.Value;
		this.entity.ccol.enabled = false;
		if (this.descwindow != null)
		{
			Object.Destroy(this.descwindow.gameObject);
		}
		if (this.scol != null)
		{
			this.scol.enabled = false;
		}
	}

	// Token: 0x060006AD RID: 1709 RVA: 0x0004E1B0 File Offset: 0x0004C3B0
	public void SetBadgeShop(bool refresh)
	{
		if (this.interacttype == NPCControl.Interaction.CaravanBadge)
		{
			this.CaravanMedalSet(refresh);
		}
		else
		{
			if (refresh)
			{
				if (this.shopitems != null && this.shopitems.Length != 0)
				{
					for (int i = 0; i < this.shopitems.Length; i++)
					{
						if (this.shopitems[i] != null)
						{
							if (this.shopitems[i].npcdata != null && this.shopitems[i].npcdata.descwindow != null)
							{
								this.shopitems[i].npcdata.DestroyDescWindow();
							}
							Object.Destroy(this.shopitems[i].gameObject);
						}
					}
				}
				MainManager.UpdateShops();
			}
			this.shopitems = new EntityControl[this.data.Length];
			for (int j = 0; j < this.data.Length; j++)
			{
				EntityControl entityControl = EntityControl.CreateNewEntity("shop" + j);
				entityControl.startpos = new Vector3?(this.vectordata[j]);
				entityControl.animid = (int)this.dialogues[10].x;
				entityControl.animid = 2;
				entityControl.name = "badgeshop" + j;
				int num = (int)this.dialogues[9].x;
				if (j < MainManager.instance.avaliablebadgepool[num].Count)
				{
					if (MainManager.instance.avaliablebadgepool[num][j] == -1)
					{
						entityControl.iskill = true;
					}
					else
					{
						entityControl.animstate = MainManager.instance.avaliablebadgepool[num][j];
					}
				}
				else
				{
					entityControl.iskill = true;
				}
				entityControl.item = true;
				entityControl.hasshadow = false;
				entityControl.npcdata = entityControl.gameObject.AddComponent<NPCControl>();
				entityControl.npcdata.entitytype = NPCControl.NPCType.SemiNPC;
				entityControl.npcdata.interacttype = NPCControl.Interaction.Shop;
				entityControl.emoticonoffset = new Vector3(0f, -1000f, 0f);
				entityControl.npcdata.shopkeeper = this;
				entityControl.npcdata.radius = this.dialogues[8].x / 10f;
				if (entityControl.npcdata.radius < 0.1f)
				{
					entityControl.npcdata.radius = 1.5625f;
				}
				entityControl.npcdata.insideid = this.insideid;
				entityControl.npcdata.colliderheight = 0.5f;
				MainManager.map.AddInEntity(entityControl);
				this.shopitems[j] = entityControl;
				if (refresh && !entityControl.iskill)
				{
					MainManager.DeathSmoke(entityControl.startpos.Value);
				}
			}
		}
		if (refresh)
		{
			MainManager.instance.showmoney = 10f;
		}
	}

	// Token: 0x060006AE RID: 1710 RVA: 0x0004E460 File Offset: 0x0004C660
	private void AddPlayerTrigger()
	{
		if (MainManager.player != null)
		{
			this.secondcoll = this.entity.gameObject.AddComponent<CapsuleCollider>();
			this.secondcoll.center = this.entity.ccol.center;
			this.secondcoll.height = this.entity.ccol.height;
			this.secondcoll.radius = this.entity.ccol.radius;
			this.secondcoll.isTrigger = true;
			Physics.IgnoreCollision(this.entity.ccol, MainManager.player.entity.ccol, true);
		}
	}

	// Token: 0x060006AF RID: 1711 RVA: 0x0004E510 File Offset: 0x0004C710
	private void AddPusher()
	{
		this.pusher = new GameObject().AddComponent<CapsuleCollider>();
		this.pusher.transform.parent = this.entity.sprite.transform.parent;
		this.pusher.center = new Vector3(0f, this.colliderheight + this.entity.height, 0f);
		this.pusher.radius = this.entity.ccol.radius / 2f;
		this.pusher.height = this.colliderheight * 2f;
		this.pusher.isTrigger = true;
		this.pusher.transform.localPosition = Vector3.zero;
		this.pusher.gameObject.tag = "Pusher";
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x0004E5EC File Offset: 0x0004C7EC
	public static bool HasBehavior(NPCControl.ActionBehaviors target, NPCControl obj)
	{
		for (int i = 0; i < obj.behaviors.Length; i++)
		{
			if (obj.behaviors[i] == target)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x0004E61C File Offset: 0x0004C81C
	private bool ColliderNotThis(Vector3 pos, float radius)
	{
		Collider[] array = Physics.OverlapSphere(pos, radius, 73984);
		List<Collider> list = new List<Collider>(this.internalcollider);
		if (array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if ((this.boxcol == null || array[i] != this.boxcol) && (this.entity.ccol == null || array[i] != this.entity.ccol) && (this.scol == null || array[i] != this.scol) && (this.pusher == null || array[i] != this.pusher) && (list.Count == 0 || !list.Contains(array[i])))
				{
					return false;
				}
			}
		}
		return array.Length != 0;
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x0004E6FC File Offset: 0x0004C8FC
	private void Update()
	{
		if (!this.dummy)
		{
			if (this.arrow != null && this.entitytype == NPCControl.NPCType.Enemy)
			{
				this.arrow.lockarrow = (this.freezecooldown <= 0f);
			}
			if (this.entity.alwaysactive || (this.entity.incamera && this.insideid == MainManager.instance.insideid && (this.startlife < 50f || this.entity.campos.z < (float)(MainManager.map.limitbehavior ? 15 : 25)) && (this.startlife < 50f || !MainManager.map.limitbehavior || this.entity.forcemove || (this.entity.campos.x > 0.25f && this.entity.campos.x < 0.75f))))
			{
				if ((this.entity.activeinevents && (MainManager.instance.minipause || MainManager.instance.inevent) && !this.trapped) || this.entity.activeonpause || (!MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message && !this.entity.dead && !this.entity.iskill && !this.entity.item && !this.trapped))
				{
					if (this.freezecooldown > 0f && this.entitytype == NPCControl.NPCType.Enemy)
					{
						if (this.entity.rigid.velocity.y < -0.1f || this.entity.rigid.velocity.y > 0.1f)
						{
							this.entity.rigid.velocity = new Vector3(this.icevel.x, this.entity.rigid.velocity.y, this.icevel.z);
						}
						else
						{
							this.icevel = Vector3.zero;
						}
						base.transform.position = MainManager.LimitRadius(base.transform.position, this.entity.startpos.Value, this.radiuslimit, true);
						if (this.pusher != null)
						{
							this.pusher.enabled = false;
						}
						this.dizzytime = -1000f;
						this.entity.spin = Vector3.zero;
						this.disguisecooldown = 120;
						if (this.disguiseobj != null)
						{
							this.entity.sprite.enabled = true;
						}
						this.entity.emoticoncooldown = -1f;
						if (this.disguiseobj != null)
						{
							this.disguiseobj.gameObject.SetActive(false);
						}
						if (this.templayer < 0)
						{
							this.templayer = base.gameObject.layer;
							base.gameObject.layer = 13;
						}
						if (this.entity.icecube == null)
						{
							this.entity.Freeze();
						}
						else
						{
							Physics.IgnoreCollision(this.scol, this.entity.icecube.GetComponent<BoxCollider>(), true);
						}
						if (this.boxcol == null)
						{
							this.boxcol = base.gameObject.AddComponent<BoxCollider>();
							this.boxcol.size = this.entity.freezesize;
							this.boxcol.isTrigger = false;
						}
						this.boxcol.center = this.entity.freezeoffset;
						this.entity.ccol.enabled = false;
						this.boxcol.enabled = true;
						this.entity.height = Mathf.Clamp(this.entity.height - MainManager.TieFramerate(0.075f), 0f, 999f);
						if (this.freezecooldown < 100f)
						{
							this.entity.shakeice = !this.entity.inice;
						}
						else
						{
							this.entity.shakeice = false;
						}
						if (this.entity.onground && !this.entity.inice && !MainManager.map.icemap)
						{
							this.freezecooldown -= MainManager.framestep;
						}
						if (!this.entity.onground)
						{
							this.freezeaircooldown += MainManager.framestep;
							if (this.freezeaircooldown > 300f)
							{
								this.freezecooldown = 0f;
							}
						}
						else
						{
							this.freezeaircooldown = 0f;
						}
						this.entity.animspeed = 0f;
						this.entity.overrideanim = true;
						if (this.entity.hasshadow)
						{
							this.entity.shadow.enabled = false;
						}
					}
					else if (this.entity.icecube != null)
					{
						if (MainManager.player != null && MainManager.player.standingon == this.boxcol)
						{
							MainManager.player.entity.onground = false;
							MainManager.player.standingon = null;
						}
						this.entity.BreakIce();
						if (this.templayer > -1)
						{
							base.gameObject.layer = this.templayer;
							this.templayer = -1;
						}
						if (this.entity.hasshadow)
						{
							this.entity.shadow.enabled = true;
						}
						this.entity.ccol.enabled = true;
						if (this.boxcol != null)
						{
							this.boxcol.enabled = false;
						}
						this.entity.overrideanim = false;
						this.entity.oldstate = -1;
						this.entity.oldfly = !this.entity.flyinganim;
						if (this.behaviorroutine == null)
						{
							if (this.returntoheight)
							{
								this.entity.SetAnim("f", true);
							}
							else
							{
								this.entity.SetAnim("", true);
							}
						}
					}
					else if (this.entitytype == NPCControl.NPCType.Enemy && this.dizzytime > 0f)
					{
						if (this.disguiseobj != null)
						{
							this.disguiseobj.gameObject.SetActive(false);
						}
						if (this.startlife > 20f && !MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message && this.entitytype == NPCControl.NPCType.Enemy && this.freezecooldown <= 0f && MainManager.player != null && MainManager.GetDistance(base.transform.position, MainManager.player.transform.position) <= this.entity.ccol.radius + 1.1f)
						{
							this.StartBattle();
						}
						this.entity.height = Mathf.Clamp(this.entity.height - 0.075f, 0f, 999f);
						this.entity.spin = new Vector3(0f, Mathf.Clamp(this.dizzytime / 5f, 0f, 15f), 0f);
						this.dizzytime -= MainManager.TieFramerate(1f);
						this.entity.overrideanim = true;
						this.entity.animstate = 11;
						this.entity.emoticonid = 1;
						this.entity.emoticoncooldown = 10f;
						if (this.entity.onground && this.touchcooldown <= 0f)
						{
							this.entity.StopForceMove(-1, false);
						}
						base.transform.position = MainManager.LimitRadius(base.transform.position, this.entity.startpos.Value, this.radiuslimit, true);
					}
					else if (!this.trapped)
					{
						if (this.entitytype == NPCControl.NPCType.Enemy && this.dizzytime > -999f)
						{
							this.entity.spin = Vector3.zero;
							this.dizzytime = -1000f;
							this.entity.animstate = this.entity.basestate;
							this.entity.overrideanim = false;
						}
						if (this.returntoheight && this.entity.initialheight > 0.1f && this.entity.height < this.entity.initialheight - 0.05f && this.disguisecooldown != -1)
						{
							this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, MainManager.TieFramerate(0.1f));
							if (this.entity.height < this.entity.initialheight - 0.2f)
							{
								this.entity.oldfly = false;
							}
						}
						NPCControl.NPCType npctype = this.entitytype;
						if (npctype == NPCControl.NPCType.Object)
						{
							switch (this.objecttype)
							{
							case NPCControl.ObjectTypes.PushRock:
								if (this.data.Length > 2)
								{
									switch (this.data[2])
									{
									case 0:
										this.PushRockStuff();
										break;
									case 1:
										base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, this.entity.startpos.Value.z);
										break;
									case 2:
										base.transform.position = new Vector3(this.entity.startpos.Value.x, base.transform.position.y, base.transform.position.z);
										break;
									}
									if (this.data.Length > 2 && this.data[2] > 0)
									{
										if (this.hit)
										{
											Vector3 pos = base.transform.position + new Vector3(0f, 1f) + this.internalvector[0] * 2f;
											if (this.ColliderNotThis(pos, 1.5f) || this.data[2] < 3 || (!this.entity.inice && !MainManager.map.icemap) || (this.internaldata[0] > 10f && MainManager.FrameDifference(3) && Vector3.Distance(base.transform.position, this.internalvector[1]) < this.vectordata[0].z / 2f))
											{
												this.hit = false;
												this.entity.rigid.velocity = Vector3.zero;
											}
											else
											{
												this.internalvector[1] = base.transform.position;
												if (this.entity.onground || (this.data.Length > 3 && this.data[3] == 1))
												{
													switch (this.data[2])
													{
													case 1:
														this.entity.rigid.constraints = (RigidbodyConstraints)120;
														break;
													case 2:
														this.entity.rigid.constraints = (RigidbodyConstraints)114;
														break;
													case 3:
														if (this.data.Length > 3 && this.data[3] == 1)
														{
															this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
														}
														else
														{
															this.entity.rigid.constraints = (RigidbodyConstraints)116;
														}
														break;
													}
												}
												else
												{
													this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
												}
												base.transform.position += this.internalvector[0] * this.vectordata[0].z * MainManager.framestep;
											}
											this.internaldata[0] += MainManager.framestep;
											this.actioncooldown = 300f;
										}
										if (!this.entity.inice && !MainManager.map.icemap)
										{
											if (this.entity.model != null)
											{
												if (this.actioncooldown < 100f)
												{
													this.entity.model.transform.localPosition = MainManager.RandomVector(0.1f, 0f);
												}
												else
												{
													this.entity.model.transform.localPosition = Vector3.zero;
												}
											}
											if (this.actioncooldown <= 0f)
											{
												this.BreakIceRock();
											}
											else
											{
												this.actioncooldown -= MainManager.framestep;
											}
										}
									}
								}
								else
								{
									this.PushRockStuff();
								}
								if (this.icevel.magnitude > 0.1f && (this.entity.rigid.velocity.y < -0.1f || this.entity.rigid.velocity.y > 0.1f))
								{
									this.entity.rigid.velocity = new Vector3(this.icevel.x, this.entity.rigid.velocity.y, this.icevel.z);
									goto IL_381E;
								}
								this.icevel = Vector3.zero;
								goto IL_381E;
							case NPCControl.ObjectTypes.PressurePlate:
								if (this.activationflag > -1 && MainManager.instance.flags[this.activationflag])
								{
									this.hit = true;
								}
								else if (this.actioncooldown <= 0f)
								{
									this.hit = false;
								}
								else
								{
									this.hit = true;
									this.actioncooldown -= MainManager.framestep;
								}
								if (!(this.moveobj != null))
								{
									goto IL_381E;
								}
								if (this.hit)
								{
									this.moveobj.localPosition = Vector3.Lerp(this.moveobj.localPosition, this.vectordata[0], MainManager.TieFramerate(0.2f));
									goto IL_381E;
								}
								this.moveobj.localPosition = Vector3.Lerp(this.moveobj.localPosition, Vector3.zero, MainManager.TieFramerate(0.2f));
								goto IL_381E;
							case NPCControl.ObjectTypes.ANDGate:
							case NPCControl.ObjectTypes.ANDBlock:
								if (this.data.Length == 2 && this.data[1] == -1 && this.activationflag != -1)
								{
									if (this.activationflag >= 0)
									{
										this.hit = MainManager.instance.flags[this.activationflag];
									}
									else
									{
										this.hit = !MainManager.instance.flags[Mathf.Abs(this.activationflag)];
									}
								}
								else if (this.data[0] == -2)
								{
									this.hit = true;
									for (int i = 1; i < this.data.Length; i++)
									{
										if ((this.data[0] >= 0 && !MainManager.map.entities[Mathf.Abs(this.data[i])].npcdata.hit) || (this.data[i] < 0 && !MainManager.instance.flags[Mathf.Abs(this.data[i])]))
										{
											this.hit = false;
											break;
										}
									}
								}
								else
								{
									for (int j = 1; j < this.data.Length; j++)
									{
										if (MainManager.map.entities[Mathf.Abs(this.data[j])].npcdata.hit != this.data[j] >= 0)
										{
											this.hit = false;
											break;
										}
										if (j >= this.data.Length - 1)
										{
											if (this.objecttype == NPCControl.ObjectTypes.ANDGate)
											{
												if (this.data[0] == -1)
												{
													this.hit = true;
												}
												else
												{
													MainManager.events.StartEvent(this.data[0], null);
													Object.Destroy(base.gameObject);
												}
											}
											else if (this.objecttype == NPCControl.ObjectTypes.ANDBlock)
											{
												this.hit = true;
											}
										}
									}
								}
								if (this.objecttype != NPCControl.ObjectTypes.ANDBlock)
								{
									goto IL_381E;
								}
								if (this.hit)
								{
									this.entity.sprite.transform.localPosition = Vector3.Lerp(this.entity.sprite.transform.localPosition, this.vectordata[0], this.vectordata[1].x);
									goto IL_381E;
								}
								this.entity.sprite.transform.localPosition = Vector3.Lerp(this.entity.sprite.transform.localPosition, Vector3.zero, this.vectordata[1].x);
								goto IL_381E;
							case NPCControl.ObjectTypes.Beemerang:
								base.tag = "BeeRang";
								if (!this.entity.sound.isPlaying)
								{
									this.entity.PlaySound("RangHold");
									this.entity.sound.loop = true;
								}
								if (!this.hit)
								{
									if (MainManager.GetDistance(base.transform.position, this.vectordata[0]) > 0.2f)
									{
										base.transform.position = Vector3.Lerp(base.transform.position, this.vectordata[0], MainManager.TieFramerate(this.entity.speed));
										goto IL_381E;
									}
									if (MainManager.GetKey(5, true) && !this.heldonce && MainManager.instance.flags[21] && !WackaWorm.disablehold)
									{
										this.entity.sound.pitch = 1.25f;
										this.timer = 99f;
										this.entity.spin = new Vector3(0f, 0f, 30f);
										if (this.particles == null)
										{
											this.particles = (Object.Instantiate(Resources.Load("Prefabs/Particles/ContinuousSmokeCloud"), base.transform.position + Vector3.up * 0.2f, Quaternion.Euler(new Vector3(-90f, 0f)), base.transform) as GameObject);
											goto IL_381E;
										}
										goto IL_381E;
									}
									else
									{
										if (this.timer == 99f)
										{
											MainManager.DestroyTemp(ref this.particles, 1f);
											this.entity.sound.pitch = 1.1f;
											this.heldonce = true;
											this.entity.speed = 0.15f;
											Vector3[] array = this.vectordata;
											int num = 0;
											Vector3 value = this.entity.startpos.Value;
											Vector3 vector = base.transform.position;
											Vector3 value2 = this.entity.startpos.Value;
											array[num] = value - MainManager.GetDirection(vector, value2) * 5f;
											this.timer = -1f;
											goto IL_381E;
										}
										this.hit = true;
										goto IL_381E;
									}
								}
								else
								{
									if (this.timer >= -2f)
									{
										this.timer -= MainManager.framestep;
									}
									if (MainManager.GetDistance(base.transform.position, MainManager.player.transform.position) <= 0.45f)
									{
										Object.Destroy(base.gameObject);
										goto IL_381E;
									}
									if (this.timer < -2f && MainManager.GetKey(5, true) && MainManager.instance.flags[21] && !this.heldonce && !WackaWorm.disablehold)
									{
										this.entity.sound.pitch = 1.25f;
										this.entity.spin = new Vector3(0f, 0f, 30f);
										this.timer = -50f;
										if (this.particles == null)
										{
											this.particles = (Object.Instantiate(Resources.Load("Prefabs/Particles/ContinuousSmokeCloud"), base.transform.position + Vector3.up * 0.2f, Quaternion.Euler(new Vector3(-90f, 0f)), base.transform) as GameObject);
											goto IL_381E;
										}
										goto IL_381E;
									}
									else
									{
										if (!MainManager.GetKey(5, true) || this.timer != -50f)
										{
											if (this.particles != null)
											{
												MainManager.DestroyTemp(ref this.particles, 1f);
												this.particles = null;
											}
											this.heldonce = true;
											this.timer = -100f;
											base.transform.position = Vector3.Lerp(base.transform.position, MainManager.player.transform.position, MainManager.TieFramerate(0.3f));
											goto IL_381E;
										}
										goto IL_381E;
									}
								}
								break;
							case NPCControl.ObjectTypes.EventTrigger:
							case NPCControl.ObjectTypes.DialogueTrigger:
								if (this.data.Length >= 3 && this.data[2] == 1 && !MainManager.instance.inevent && !MainManager.instance.minipause && !MainManager.instance.pause)
								{
									if (this.objecttype == NPCControl.ObjectTypes.DialogueTrigger)
									{
										MainManager.player.CancelAction();
										MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.map.dialogues[this.data[0]], 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, base.transform, null));
										if (MainManager.map.useglobalcommand)
										{
											MainManager.map.currentline = this.data[0];
										}
									}
									else
									{
										MainManager.events.StartEvent(this.data[0], null);
									}
									Object.Destroy(base.gameObject);
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.SavePoint:
								this.internaltransform[0].Rotate(0f, 0f, 0.3f);
								goto IL_381E;
							case NPCControl.ObjectTypes.JumpSpring:
								this.boxcol.center = this.internalvector[MainManager.instance.itempicked ? 1 : 0];
								goto IL_381E;
							case NPCControl.ObjectTypes.Switch:
							{
								if (this.data[1] == 0 && this.data.Length > 2 && this.data[2] > -1)
								{
									if (this.actioncooldown > 0f)
									{
										this.actioncooldown -= MainManager.TieFramerate(1f);
										if (this.data.Length > 3 && this.data[3] == 1 && (this.actioncooldown <= 180f || Mathf.FloorToInt(this.actioncooldown / 10f) % 60 != 0) && Mathf.FloorToInt(this.actioncooldown / 10f) % 40 == 0)
										{
										}
									}
									else if (this.actioncooldown <= 0f && this.actioncooldown > -1000f)
									{
										this.hit = false;
										this.actioncooldown = -1100f;
									}
									if (this.activationflag > -1 && MainManager.instance.flags[this.activationflag])
									{
										this.hit = true;
									}
								}
								else if (this.data[1] == 1 && this.actioncooldown > 0f)
								{
									this.actioncooldown -= MainManager.TieFramerate(1f);
								}
								MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
								if ((animIDs == MainManager.AnimIDs.WoodenSwitch || animIDs == MainManager.AnimIDs.SteelSwitch) && this.moveobj != null)
								{
									this.moveobj.transform.localEulerAngles = new Vector3(0f, Mathf.LerpAngle(this.moveobj.transform.localEulerAngles.y, this.hit ? this.internaldata[0] : 0f, MainManager.TieFramerate(0.1f)));
									goto IL_381E;
								}
								goto IL_381E;
							}
							case NPCControl.ObjectTypes.CoiledObject:
								if (!this.hit)
								{
									goto IL_381E;
								}
								if (!this.boxcol.enabled)
								{
									this.entity.model.transform.localScale = Vector3.Lerp(this.entity.model.transform.localScale, new Vector3(100f, 100f, 0f), MainManager.TieFramerate(0.2f));
									goto IL_381E;
								}
								this.entity.model.transform.localScale = Vector3.Lerp(this.entity.model.transform.localScale, new Vector3(100f, 100f, 120f), MainManager.TieFramerate(0.2f));
								if (this.entity.model.transform.localScale.z > 115f)
								{
									this.boxcol.enabled = false;
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.FixedAnim:
								this.entity.animstate = this.data[1];
								if (Mathf.Sin(Time.deltaTime) >= 0.8f)
								{
									this.entity.oldstate = -1;
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.EnemySpawner:
								if (this.actioncooldown > 0f && (this.spawned == null || this.spawned.entity.iskill))
								{
									this.actioncooldown -= MainManager.TieFramerate(1f);
									goto IL_381E;
								}
								if (this.spawned == null || this.spawned.entity.iskill)
								{
									if (this.spawned == null)
									{
										this.spawned = MainManager.GetEntity(this.data[0]).npcdata;
									}
									else if (this.spawned.entity != null)
									{
										this.RespawnEnemy(this.spawned, this.vectordata[0] + MainManager.RandomVector(this.vectordata[1]) + Vector3.up * 0.5f);
									}
									this.actioncooldown = (float)this.data[4];
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.Dropplet:
								if (!this.entity.rigid.isKinematic)
								{
									this.entity.rigid.velocity = new Vector3(this.entity.rigid.velocity.x, -10f, this.entity.rigid.velocity.z);
									this.entity.RefreshShadow();
								}
								if (this.actioncooldown > 0f)
								{
									this.actioncooldown -= MainManager.TieFramerate(1f);
								}
								else
								{
									this.entity.LockRigid(false, false);
								}
								if (this.entity.shadow != null)
								{
									this.entity.shadow.enabled = (this.data[2] == 0 || !this.hit);
								}
								if (this.actionfrequency[2] <= 0f)
								{
									if (this.actionfrequency[1] <= 0f || this.entity.transform.position.y < -25f)
									{
										base.transform.position = this.entity.startpos.Value;
										this.entity.LockRigid(false);
										this.actionfrequency[1] = 600f;
										this.entity.rigid.isKinematic = false;
										this.entity.rigid.velocity = Vector3.zero;
									}
									else
									{
										this.actionfrequency[1] -= MainManager.framestep;
									}
									if (this.actionfrequency[0] > 0f)
									{
										this.actionfrequency[0] -= MainManager.TieFramerate(1f);
									}
									else if ((this.data[2] == 0 || !this.hit) && this.actionfrequency[0] > -1000f)
									{
										base.transform.position = this.entity.startpos.Value;
										this.entity.LockRigid(false);
										this.entity.rigid.isKinematic = false;
										this.entity.rigid.velocity = Vector3.zero;
										this.actionfrequency[0] = -1100f;
										this.actionfrequency[1] = 600f;
									}
									else
									{
										this.entity.LockRigid(false, false);
										this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
									}
									if (!this.hit || MainManager.instance.minipause || MainManager.instance.pause)
									{
										goto IL_381E;
									}
									if (this.data[1] > 0 && this.hit && Vector3.Distance(this.internaltransform[0].transform.position, this.vectordata[0]) > (float)this.data[1])
									{
										this.ShatterDroppletIce();
									}
									if (this.data.Length <= 3 || this.data[3] <= 0)
									{
										goto IL_381E;
									}
									if (this.actioncooldown <= 0f)
									{
										this.ShatterDroppletIce();
										this.internaltransform[0].GetComponent<Hornable>().ServerGeizer();
										goto IL_381E;
									}
									if (this.actioncooldown < 100f && Mathf.FloorToInt(this.actioncooldown) % 3 == 0)
									{
										DialogueAnim component = this.internaltransform[0].GetComponent<DialogueAnim>();
										if (component.targetscale != Vector3.zero)
										{
											AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/Sounds/IceMelt"), this.internaltransform[0].position, MainManager.GetSoundDistance(this.internaltransform[0].position) * 2f * MainManager.soundvolume);
										}
										component.targetscale = Vector3.zero;
										component.shrinkspeed = 0.01f;
										goto IL_381E;
									}
									goto IL_381E;
								}
								else
								{
									if (!MainManager.instance.pause)
									{
										this.actionfrequency[2] -= MainManager.framestep;
										goto IL_381E;
									}
									goto IL_381E;
								}
								break;
							case NPCControl.ObjectTypes.PathPlatform:
							case NPCControl.ObjectTypes.RotatingPlatform:
								if (this.boxcol != null)
								{
									if (this.entity.originalid + 1 == 243)
									{
										this.boxcol.enabled = true;
									}
									else
									{
										this.boxcol.enabled = this.hit;
									}
								}
								if ((int)this.dialogues[1].x == 1)
								{
									this.hit = true;
									if ((int)this.dialogues[0].x > -1)
									{
										this.speedmultiplier = (float)((int)this.dialogues[0].x);
										this.dialogues[0].x = -1f;
									}
									if (MainManager.ObjectsAreActive(this.data, true))
									{
										this.speedmultiplier += MainManager.TieFramerate(this.dialogues[0].y / 1000f);
									}
									else
									{
										this.speedmultiplier -= MainManager.TieFramerate(this.dialogues[0].y / 1000f);
									}
									this.speedmultiplier = Mathf.Clamp01(this.speedmultiplier);
									base.transform.position = Vector3.Lerp(this.vectordata[0], this.vectordata[1], this.speedmultiplier);
									if (!this.entity.sound.isPlaying && this.speedmultiplier > 0f && this.speedmultiplier < 1f && this.entity.originalid + 1 != 243)
									{
										this.entity.PlaySound("PlatformMove");
										this.entity.sound.loop = true;
										goto IL_381E;
									}
									goto IL_381E;
								}
								else if (!this.hit)
								{
									this.entity.sound.loop = false;
									this.entity.model.transform.tag = "Platform";
									if (this.data.Length == 0 || MainManager.ObjectsAreActive(this.data, true))
									{
										this.hit = true;
										this.actioncooldown = this.dialogues[1].y;
										this.bounces = this.currentnode;
										this.speedmultiplier = 0f;
										if (this.currentnode + 1 < this.vectordata.Length)
										{
											this.currentnode++;
											goto IL_381E;
										}
										this.currentnode = 0;
										goto IL_381E;
									}
									else
									{
										if (this.dialogues[1].y <= 0f)
										{
											goto IL_381E;
										}
										if (this.actioncooldown > 0f)
										{
											this.actioncooldown -= MainManager.framestep;
											goto IL_381E;
										}
										if (this.currentnode > 0)
										{
											this.bounces = this.currentnode;
											this.currentnode--;
											this.actioncooldown = this.dialogues[1].y;
											this.hit = true;
											goto IL_381E;
										}
										goto IL_381E;
									}
								}
								else
								{
									if (this.bounces == this.currentnode)
									{
										goto IL_381E;
									}
									this.entity.model.transform.tag = "PlatformNoClock";
									if (!this.entity.sound.isPlaying && this.entity.originalid + 1 != 243)
									{
										this.entity.PlaySound("PlatformMove");
										this.entity.sound.loop = true;
									}
									this.speedmultiplier += MainManager.TieFramerate(this.dialogues[0].y / 1000f);
									if (this.speedmultiplier >= 1f)
									{
										this.speedmultiplier = 0f;
										this.hit = false;
										goto IL_381E;
									}
									if (this.objecttype == NPCControl.ObjectTypes.PathPlatform)
									{
										if (this.vectordata.Length <= 2)
										{
											base.transform.position = MainManager.SmoothLerp(this.vectordata[this.bounces], this.vectordata[this.currentnode], this.speedmultiplier);
											goto IL_381E;
										}
										base.transform.position = Vector3.Lerp(this.vectordata[this.bounces], this.vectordata[this.currentnode], this.speedmultiplier);
										goto IL_381E;
									}
									else
									{
										if (this.entity.model != null)
										{
											this.entity.model.eulerAngles = Vector3.Lerp(this.vectordata[this.bounces], this.vectordata[this.currentnode], this.speedmultiplier);
											goto IL_381E;
										}
										goto IL_381E;
									}
								}
								break;
							case NPCControl.ObjectTypes.Geizer:
								if (this.internaltransform != null)
								{
									this.internaltransform[3].transform.localScale = Vector3.one;
									if ((this.internalrender == null || this.internalrender.Length == 0) && this.internaltransform[1].childCount > 0)
									{
										this.internalrender = new MeshRenderer[]
										{
											this.internaltransform[1].GetChild(0).GetComponent<MeshRenderer>()
										};
										this.internalrender[0].enabled = false;
									}
								}
								if (this.data.Length != 1 && this.data[1] != -1 && (!(MainManager.map.entities[this.data[1]] != null) || !MainManager.map.entities[this.data[1]].npcdata.hit))
								{
									if (this.actioncooldown > 0f)
									{
										this.GeizerBreak();
										this.actioncooldown = 0f;
									}
									Transform transform = base.transform;
									Vector3 position = base.transform.position;
									Vector3 value3 = this.entity.startpos.Value;
									Vector3 vector = base.transform.up;
									transform.position = Vector3.Lerp(position, value3 + -vector.normalized * 10f, MainManager.TieFramerate(0.025f));
									goto IL_381E;
								}
								if (this.startlife > 20f && !this.hit)
								{
									this.internaltransform[3].GetComponentInChildren<ParticleSystem>().Play();
									this.hit = true;
								}
								base.transform.position = Vector3.Lerp(base.transform.position, this.entity.startpos.Value, MainManager.framestep * 0.025f);
								if (this.internaltransform == null || this.internaltransform.Length == 0)
								{
									goto IL_381E;
								}
								if (this.actioncooldown > 0f)
								{
									if (this.internalrender != null && (this.data.Length < 4 || this.data[3] == 0))
									{
										this.internalrender[0].enabled = true;
									}
									this.entity.sound.loop = false;
									this.actioncooldown -= MainManager.framestep;
									if (this.actioncooldown < 100f)
									{
										this.internaltransform[1].localPosition = new Vector3(Random.Range(-0.05f, 0.05f), 0f, 0f);
										goto IL_381E;
									}
									this.internaltransform[1].localPosition = Vector3.zero;
									goto IL_381E;
								}
								else
								{
									if (this.actioncooldown > -1000f)
									{
										this.internaltransform[0].gameObject.SetActive(true);
										this.internaltransform[1].gameObject.SetActive(false);
										this.internaltransform[3].gameObject.SetActive(true);
										if (this.boxcol != null)
										{
											this.boxcol.enabled = true;
										}
										if (this.startlife >= 15f)
										{
											this.GeizerBreak();
										}
										this.internaltransform[3].GetComponentInChildren<ParticleSystem>().Play();
										this.entity.sound.loop = false;
										this.actioncooldown = -1100f;
										goto IL_381E;
									}
									this.internaltransform[2].Rotate(0f, 0f, 5f);
									this.internaltransform[3].Rotate(0f, -5f, 0f);
									this.boxcol.center = new Vector3(0f, 3f + this.internaltransform[0].parent.transform.localPosition.y);
									Transform transform2 = this.internaltransform[0].parent.transform;
									Vector3 position2 = this.internaltransform[0].parent.transform.position;
									Vector3 value4 = this.entity.startpos.Value;
									Vector3 vector = base.transform.up;
									transform2.position = Vector3.Lerp(position2, value4 + vector.normalized * (Mathf.Sin(this.actionfrequency[1] * this.vectordata[0].x + this.actionfrequency[0]) * this.vectordata[0].y), MainManager.TieFramerate(0.1f));
									this.actionfrequency[1] += Time.deltaTime;
									if (!this.entity.sound.isPlaying)
									{
										this.entity.PlaySound("Waterfall1", 0.075f, 1f);
										this.entity.sound.loop = true;
									}
									if (this.data.Length > 2 && this.data[2] == 1 && MainManager.map.lastwater != null)
									{
										this.internaltransform[3].position = new Vector3(this.internaltransform[3].position.x, MainManager.map.lastwater.transform.position.y, this.internaltransform[3].position.z);
									}
									if (this.internalrender != null)
									{
										this.internalrender[0].enabled = false;
										this.internalrender[0].transform.position = this.internaltransform[3].transform.position;
										goto IL_381E;
									}
									goto IL_381E;
								}
								break;
							case NPCControl.ObjectTypes.MusicRange:
								if (this.data.Length >= 4 && this.data[3] != -1)
								{
									goto IL_381E;
								}
								if (this.data[0] <= 0)
								{
									this.hit = (Vector3.Distance(base.transform.position, MainManager.player.transform.position) < this.vectordata[0].x);
									if (this.hit)
									{
										MainManager.instance.inmusicrange = this.mapid;
									}
									else if (MainManager.instance.inmusicrange == this.mapid)
									{
										MainManager.instance.inmusicrange = -1;
									}
									if (this.hit)
									{
										MainManager.CheckSamira(this.entity.sound.clip);
									}
									this.data[0] = this.data[1];
								}
								else
								{
									this.data[0]--;
								}
								if (MainManager.instance.inmusicrange == this.mapid && !this.entity.sound.isPlaying)
								{
									this.entity.sound.Play();
								}
								this.entity.sound.volume = Mathf.Lerp(this.entity.sound.volume, (MainManager.instance.inmusicrange == this.mapid) ? (this.vectordata[0].z * ((MainManager.pausemenu == null) ? MainManager.musicvolume : MainManager.pausemenu.mvolume)) : 0f, this.vectordata[0].y);
								if (this.mapid == MainManager.map.musicrangemain)
								{
									MainManager.music[0].volume = Mathf.Lerp(MainManager.music[0].volume, (MainManager.instance.inmusicrange > -1) ? 0f : MainManager.musicvolume, this.vectordata[0].y);
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.TempPlatform:
								MainManager.player.forceclosemove = this.hit;
								if (!this.hit)
								{
									this.actioncooldown = (float)this.data[0];
									this.entity.model.transform.localPosition = Vector3.zero;
									goto IL_381E;
								}
								if (this.actioncooldown > 0f)
								{
									if (this.data[3] == 0)
									{
										this.entity.model.transform.localPosition = new Vector3(Random.Range(-0.05f, 0.05f), 0f, 0f);
									}
									this.actioncooldown -= MainManager.TieFramerate(1f);
									goto IL_381E;
								}
								if (this.actioncooldown > -99999f)
								{
									if (this.data[1] == 1 && this.data[2] == 1)
									{
										base.StartCoroutine(this.RespawnPlayer(true));
									}
									this.actioncooldown = -100000f;
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.ScrewSwitch:
							{
								if (!(MainManager.player != null))
								{
									goto IL_381E;
								}
								this.entity.soundfix = true;
								this.hit = (MainManager.player.beemerang != null && MainManager.GetDistance(base.transform.position, MainManager.player.beemerang.transform.position) < 1.75f);
								if (this.hit)
								{
									if (this.actioncooldown < this.vectordata[0].z)
									{
										this.actioncooldown += MainManager.TieFramerate(this.vectordata[0].x);
									}
								}
								else if (this.actioncooldown > 0f)
								{
									this.actioncooldown -= MainManager.TieFramerate(this.vectordata[0].y);
								}
								float num2 = Mathf.Clamp01(MainManager.TieFramerate(this.actioncooldown / this.vectordata[0].z));
								this.entity.model.transform.localEulerAngles += this.vectordata[1] * num2;
								if (num2 > 0f)
								{
									if (!this.entity.sound.isPlaying)
									{
										this.entity.PlaySound("SpinSwitch0", 0.5f);
									}
									this.entity.sound.loop = true;
									this.entity.sound.pitch = 0.5f + num2;
									goto IL_381E;
								}
								this.entity.sound.loop = false;
								goto IL_381E;
							}
							case NPCControl.ObjectTypes.StencilSwitch:
								if (this.internaltransform != null && this.internaltransform.Length != 0)
								{
									this.internaltransform[0].localScale = Vector3.Lerp(this.internaltransform[0].localScale, this.hit ? (Vector3.one * (this.vectordata[0].y * 2f)) : Vector3.zero, MainManager.TieFramerate(this.vectordata[0].x));
									if (this.hit)
									{
										this.internaltransform[0].transform.position = base.transform.position;
									}
									else
									{
										this.internaltransform[0].transform.position = new Vector3(0f, -999f);
									}
								}
								if (this.data[1] > -1)
								{
									base.transform.localPosition = this.vectordata[1];
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.RollingRock:
								if (this.data.Length > 2 && this.data[2] == 1)
								{
									float num3 = MainManager.TieFramerate(1f);
									if (this.internaldata[0] <= 0f)
									{
										if (this.actioncooldown > 0f)
										{
											base.transform.position = new Vector3(0f, 999f);
											if (this.CheckOtherEntityActive(this.data[3]))
											{
												this.actioncooldown -= num3;
												if (this.actioncooldown < 60f)
												{
													this.internaltransform[1].localScale = Vector3.Lerp(this.internaltransform[1].localScale, new Vector3(0.5f, 1.25f, 1.25f), num3 * 0.05f);
												}
											}
										}
										else if (this.actioncooldown > -1000f)
										{
											this.entity.rigid.velocity = Vector3.zero;
											base.transform.position = this.entity.startpos.Value + this.vectordata[0].normalized * this.vectordata[1].y + Vector3.up * 0.25f;
											this.entity.LockRigid(false);
											this.actioncooldown = -1100f;
											this.internaldata[0] = this.internaldata[1];
											this.internaltransform[1].localScale = new Vector3(1.25f, 0.75f, 0.75f);
										}
									}
									else
									{
										this.internaldata[0] -= num3;
									}
									if (this.actioncooldown <= -1000f)
									{
										this.internaltransform[1].localScale = Vector3.Lerp(this.internaltransform[1].localScale, Vector3.one, MainManager.TieFramerate(0.05f));
									}
								}
								if ((this.internaltransform == null && (this.data[0] == 0 || this.entity.onground)) || (this.internaltransform != null && this.actioncooldown < 0f))
								{
									if (!this.hit && this.data[0] == 1)
									{
										MainManager.PlayParticle("impactsmoke", base.transform.position);
										if (MainManager.GetDistance(MainManager.player.transform.position, base.transform.position) < 15f)
										{
											MainManager.PlaySound("Thud");
											MainManager.ShakeScreen(0.1f, 0.35f, true);
										}
									}
									this.hit = true;
								}
								if (this.hit)
								{
									this.entity.rigid.velocity = new Vector3(this.vectordata[0].x, this.entity.rigid.velocity.y, this.vectordata[0].z);
									this.entity.model.transform.eulerAngles += this.vectordata[2] * MainManager.framestep;
									if (this.data.Length < 4 || this.CheckOtherEntityActive(this.data[3]))
									{
										if (!this.entity.sound.isPlaying)
										{
											this.entity.PlaySound("RollingRock");
											this.entity.sound.loop = true;
										}
									}
									else
									{
										this.entity.sound.Stop();
									}
								}
								else
								{
									if (this.entity.sound.isPlaying && this.entity.sound.clip != null && this.entity.sound.clip.name == "RollingRock")
									{
										this.entity.sound.Stop();
									}
									this.entity.sound.loop = false;
								}
								if (base.transform.position.y < this.vectordata[1].x)
								{
									this.WarpRock();
									goto IL_381E;
								}
								goto IL_381E;
							case NPCControl.ObjectTypes.WindPusher:
								if (this.data[0] <= -1 || this.internalparticle == null || !(this.internalparticle[0] != null))
								{
									goto IL_381E;
								}
								if (MainManager.GetEntity(this.data[0]).npcdata.hit || this.hit)
								{
									if (!this.internalparticle[0].isPlaying)
									{
										this.internalparticle[0].Play();
										goto IL_381E;
									}
									goto IL_381E;
								}
								else
								{
									if (this.internalparticle[0].isPlaying)
									{
										this.internalparticle[0].Stop();
										goto IL_381E;
									}
									goto IL_381E;
								}
								break;
							case NPCControl.ObjectTypes.WaterSwitch:
								if (this.internaltransform == null || this.internaltransform.Length == 0)
								{
									goto IL_381E;
								}
								this.internaltransform[0].transform.position = MainManager.SmoothLerp(this.vectordata[1], this.vectordata[2], (this.startlife < 20f) ? 1f : (this.vectordata[0].x / this.vectordata[3].x));
								if (this.hit)
								{
									if (this.vectordata[0].x < this.vectordata[3].x)
									{
										Vector3[] array2 = this.vectordata;
										int num4 = 0;
										array2[num4].x = array2[num4].x + MainManager.TieFramerate(1f);
										goto IL_381E;
									}
									goto IL_381E;
								}
								else
								{
									if (this.vectordata[0].x > 0f)
									{
										Vector3[] array3 = this.vectordata;
										int num5 = 0;
										array3[num5].x = array3[num5].x - MainManager.TieFramerate(1f);
										goto IL_381E;
									}
									goto IL_381E;
								}
								break;
							}
							if (this.timer > 0f)
							{
								this.timer = Mathf.Clamp(this.timer - MainManager.TieFramerate(1f), 0f, float.PositiveInfinity);
							}
							else if (this.timer == 0f && !this.entity.dead)
							{
								base.StartCoroutine(this.entity.Death());
							}
						}
						else
						{
							if (this.startlife > 20f && !MainManager.instance.pause && !MainManager.instance.minipause && !MainManager.instance.message && this.entitytype == NPCControl.NPCType.Enemy && this.freezecooldown <= 0f && MainManager.player != null && MainManager.GetSqrDistance(base.transform.position, MainManager.player.transform.position) <= this.entity.ccol.radius + 1.1f)
							{
								this.StartBattle();
							}
							int num6 = Convert.ToInt32(this.inrange);
							if (this.inrange && this.behaviors[0] == NPCControl.ActionBehaviors.DisguiseOnce)
							{
								this.behaviors[0] = NPCControl.ActionBehaviors.Wander;
								if (this.disguiseobj != null)
								{
									Object.Destroy(this.disguiseobj.gameObject);
								}
							}
							if (MainManager.player != null && (MainManager.player.digging || MainManager.player.entity.icooldown > 0f))
							{
								num6 = 0;
							}
							if (!this.overridebehavior && this.behaviorroutine == null && this.behaviors != null && this.behaviors.Length != 0 && !this.entity.iskill && !this.entity.dead && this.entity.deathcoroutine == null && (this.entitytype == NPCControl.NPCType.Enemy || this.entity.campos.z < 20f || this.entity.alwaysactive))
							{
								this.hasenteredrange = true;
								if (this.forcebehavior != null)
								{
									this.DoBehavior(this.forcebehavior.Value, this.actionfrequency[num6]);
								}
								else
								{
									this.DoBehavior(ref this.behaviors[num6], this.actionfrequency[num6]);
								}
							}
						}
					}
				}
				else if (this.entity.item && !this.trapped)
				{
					if (this.objecttype == NPCControl.ObjectTypes.Item && this.beerang != null)
					{
						if (this.timer > -1f)
						{
							this.timer = 300f;
						}
						base.transform.position = this.beerang.position + Vector3.up;
						if (this.entity.fixedentity)
						{
							this.entity.Unfix();
						}
					}
					if (this.secondcoll != null)
					{
						if (this.touchcooldown > 0f)
						{
							if (MainManager.player != null)
							{
								Physics.IgnoreCollision(MainManager.player.entity.ccol, this.secondcoll, true);
							}
						}
						else if (this.touchcooldown != -9999f)
						{
							if (MainManager.player != null)
							{
								Physics.IgnoreCollision(this.secondcoll, MainManager.player.entity.ccol, false);
							}
							this.touchcooldown = -9999f;
						}
						else if (this.interacttype != NPCControl.Interaction.Shop)
						{
							this.entity.ccol.enabled = true;
						}
					}
					if (this.entity.onground)
					{
						this.entity.Jump(Mathf.Abs(this.entity.rigid.velocity.y));
						if (this.bounces < 3)
						{
							this.bounces++;
							if (this.interacttype != NPCControl.Interaction.Shop && this.startlife > 15f)
							{
								this.entity.PlaySound("ItemBounce" + ((this.entity.animid == 3) ? "1" : "0"));
							}
						}
					}
					if (this.bounces >= 3)
					{
						this.entity.StopForceMove();
					}
					if (this.entity.sprite != null)
					{
						if (this.timer > 0f && !MainManager.instance.minipause && !MainManager.instance.pause)
						{
							this.timer = Mathf.Clamp(this.timer - MainManager.TieFramerate(1f), 0f, float.PositiveInfinity);
						}
						else if (this.timer == 0f)
						{
							Object.Destroy(base.gameObject);
						}
						if (this.timer < 100f && this.timer > -1f && !MainManager.instance.minipause && !MainManager.instance.inevent)
						{
							this.entity.sprite.enabled = !this.entity.sprite.enabled;
						}
						else
						{
							this.entity.sprite.enabled = true;
						}
					}
				}
				IL_381E:
				if (this.touchcooldown > 0f)
				{
					this.touchcooldown -= MainManager.TieFramerate(1f);
				}
			}
			else
			{
				if (!this.trapped)
				{
					base.transform.position = this.entity.lastpos;
				}
				if (this.disguiseobj != null && this.disguisecooldown == -1)
				{
					this.disguiseobj.gameObject.SetActive(true);
					this.entity.sprite.enabled = (this.entity.campos.z <= 5f);
					this.entity.overrideminheight = true;
					this.entity.height = 0f;
				}
				if (Time.frameCount % 3 == 0 && (this.entitytype == NPCControl.NPCType.Enemy || this.entitytype == NPCControl.NPCType.NPC) && this.entity.forcemove && !this.HasBehavior(NPCControl.ActionBehaviors.SetPath) && !this.HasBehavior(NPCControl.ActionBehaviors.StealthAI))
				{
					this.entity.StopForceMove();
				}
			}
		}
		if (this.entitytype != NPCControl.NPCType.Object && this.entitytype != NPCControl.NPCType.SemiNPC && this.behaviors != null && this.behaviors.Length >= 2 && this.behaviors[this.inrange ? 1 : 0] == NPCControl.ActionBehaviors.ChasePlayer && (MainManager.instance.minipause || MainManager.instance.inevent))
		{
			this.entity.StopForceMove();
		}
		if (MainManager.player != null && !MainManager.player.digging && MainManager.instance.message && this.entitytype != NPCControl.NPCType.Object && this.HasBehavior(NPCControl.ActionBehaviors.StealthAI))
		{
			this.actioncooldown = 1f;
		}
	}

	// Token: 0x060006B3 RID: 1715 RVA: 0x000520C8 File Offset: 0x000502C8
	public void ShatterDroppletIce()
	{
		this.hit = false;
		MainManager.PlayParticle("IceShatter", null, this.internaltransform[0].transform.position + Vector3.up, new Vector3(-90f, 0f), 1f);
		AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/Sounds/IceBreak"), this.internaltransform[0].position, MainManager.GetSoundDistance(this.internaltransform[0].position) * MainManager.soundvolume);
		base.StartCoroutine(MainManager.DelayedPosition(this.internaltransform[0], new Vector3(0f, -1000f, 0f), -1f, false));
		this.internaltransform[0].GetComponent<Rigidbody>().isKinematic = true;
		this.internaltransform[0].transform.parent = MainManager.map.transform;
		this.internaltransform[0].GetComponent<DialogueAnim>().enabled = true;
	}

	// Token: 0x060006B4 RID: 1716 RVA: 0x000521BC File Offset: 0x000503BC
	private void PushRockStuff()
	{
		this.entity.activeinevents = true;
		if (this.arrow == null)
		{
			this.arrow = HelpArrow.NewArrow(base.transform, Vector3.up * 0.75f, Color.green, 2.5f, 1.5f);
		}
		if (this.freezecooldown <= 0f)
		{
			this.entity.rigid.velocity = new Vector3(0f, this.entity.rigid.velocity.y, 0f);
			this.internalcollider[0].material.dynamicFriction = 0f;
			this.internalcollider[0].material.staticFriction = 0f;
		}
		else
		{
			this.freezecooldown -= MainManager.framestep;
		}
		if (this.actioncooldown > 0f)
		{
			this.actioncooldown -= MainManager.framestep;
		}
		if (this.entity.rigid.velocity.y >= -0.15f && this.entity.rigid.velocity.y <= 0.15f && this.entity.onground)
		{
			this.icevel = Vector3.zero;
			this.internalcollider[0].material.dynamicFriction = 1f;
			this.internalcollider[0].material.staticFriction = 1f;
			return;
		}
		this.entity.rigid.velocity = new Vector3(this.icevel.x, this.entity.rigid.velocity.y, this.icevel.z);
		this.internalcollider[0].material.dynamicFriction = 0f;
		this.internalcollider[0].material.staticFriction = 0f;
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x000523A4 File Offset: 0x000505A4
	private void DoBehavior(NPCControl.ActionBehaviors behavior, float frequency)
	{
		if (behavior != NPCControl.ActionBehaviors.None)
		{
			NPCControl.ActionBehaviors actionBehaviors = behavior;
			this.DoBehavior(ref actionBehaviors, frequency);
		}
	}

	// Token: 0x060006B6 RID: 1718 RVA: 0x000523BF File Offset: 0x000505BF
	private void RespawnEnemy(NPCControl enemy)
	{
		this.RespawnEnemy(enemy, enemy.entity.spawnpoint);
	}

	// Token: 0x060006B7 RID: 1719 RVA: 0x000523D4 File Offset: 0x000505D4
	public void RespawnEnemy(NPCControl enemy, Vector3 pos)
	{
		pos += Vector3.up * 0.5f;
		enemy.entity.destroytype = NPCControl.DeathType.SpinSmoke;
		enemy.attacking = false;
		if (enemy.behaviorroutine != null)
		{
			base.StopCoroutine(enemy.behaviorroutine);
		}
		enemy.behaviorroutine = null;
		enemy.entity.StopAllCoroutines();
		enemy.entity.deathcoroutine = null;
		enemy.entity.iskill = false;
		enemy.entity.dead = false;
		enemy.entity.spin = Vector3.zero;
		enemy.entity.rigid.useGravity = true;
		enemy.entity.rigid.isKinematic = false;
		enemy.entity.rigid.velocity = Vector3.zero;
		enemy.entity.sprite.transform.localEulerAngles = Vector3.zero;
		enemy.entity.animstate = enemy.entity.basestate;
		enemy.entity.sprite.enabled = true;
		enemy.entity.overrideanim = false;
		enemy.entity.onground = false;
		enemy.entity.bobspeed = enemy.entity.startbs;
		enemy.entity.bobrange = enemy.entity.startbf;
		enemy.dizzytime = 0f;
		enemy.respawntimer = -100f;
		enemy.entity.lastpos = pos;
		if (MainManager.instance.pause)
		{
			enemy.entity.pausepos = new Vector3?(pos);
		}
		else
		{
			enemy.entity.pausepos = null;
		}
		enemy.freezecooldown = 0f;
		enemy.entity.StopForceMove();
		if (enemy.HasBehavior(NPCControl.ActionBehaviors.SetPath))
		{
			this.actioncooldown = 0f;
			this.currentnode = 0;
		}
		enemy.entity.ccol.enabled = true;
		enemy.entity.ccol.isTrigger = false;
		enemy.hit = false;
		enemy.transform.position = pos;
		enemy.entity.startpos = new Vector3?(enemy.transform.position);
		if (this.DisguiseBehavior(this.behaviors[0]))
		{
			if (this.disguiseobj != null)
			{
				Object.Destroy(this.disguiseobj.gameObject);
			}
			enemy.entity.speed = 2f;
			this.disguisecooldown = -1;
			this.behaviors[0] = NPCControl.ActionBehaviors.Wander;
			enemy.entity.Invoke("LateGround", 0.5f);
		}
		enemy.entity.LockRigid(false);
		if (enemy.pusher != null)
		{
			enemy.pusher.enabled = true;
		}
		MainManager.DeathSmoke(enemy.entity.sprite.transform.position);
	}

	// Token: 0x060006B8 RID: 1720 RVA: 0x00052698 File Offset: 0x00050898
	private bool DisguiseBehavior(NPCControl.ActionBehaviors behavior)
	{
		return behavior == NPCControl.ActionBehaviors.Disguise || behavior == NPCControl.ActionBehaviors.DisguiseOnce || behavior == NPCControl.ActionBehaviors.DisguiseOnceJumpForward;
	}

	// Token: 0x060006B9 RID: 1721 RVA: 0x000526AC File Offset: 0x000508AC
	private void DoBehavior(ref NPCControl.ActionBehaviors behavior, float frequency)
	{
		if (!this.DisguiseBehavior(behavior) && this.disguiseobj != null)
		{
			if (this.disguisecooldown < 120)
			{
				this.entity.TempSpin(Vector3.up * 20f, 0.2f);
			}
			this.disguisecooldown = 120;
			this.entity.overrideminheight = false;
			this.disguiseobj.gameObject.SetActive(false);
		}
		bool flag = false;
		this.entity.digging = ((behavior == NPCControl.ActionBehaviors.WanderUnderground || behavior == NPCControl.ActionBehaviors.ChargeAttackUnderground) && this.behaviorroutine == null);
		switch (behavior)
		{
		case NPCControl.ActionBehaviors.None:
			this.entity.StopForceMove(-1, false);
			goto IL_F61;
		case NPCControl.ActionBehaviors.FacePlayer:
			this.entity.StopForceMove(-1, false);
			if (MainManager.player != null)
			{
				this.entity.FaceTowards(MainManager.player.transform.position);
				goto IL_F61;
			}
			goto IL_F61;
		case NPCControl.ActionBehaviors.ChasePlayer:
		case NPCControl.ActionBehaviors.ChargeAndAttack:
		case NPCControl.ActionBehaviors.ChargeAttackUnderground:
		case NPCControl.ActionBehaviors.ChaseOnWater:
			break;
		case NPCControl.ActionBehaviors.FleeFromPlayer:
		case NPCControl.ActionBehaviors.WalkAwayFromPlayer:
			if (this.entity.forcemove)
			{
				this.entity.StopForceMove();
			}
			if (!MainManager.player.tattling || MainManager.instance.pause)
			{
				EntityControl entityControl = this.entity;
				Vector3 position = base.transform.position;
				Vector3 position2 = base.transform.position;
				Vector3 position3 = MainManager.player.transform.position;
				entityControl.Move(position + MainManager.GetDirection(position2, position3).normalized, 1f, 1);
				goto IL_F61;
			}
			goto IL_F61;
		case NPCControl.ActionBehaviors.TurnRandomly:
		case NPCControl.ActionBehaviors.TurnFixedInterval:
			this.entity.StopForceMove(-1, false);
			if (this.actioncooldown > 0f)
			{
				this.actioncooldown -= MainManager.TieFramerate(1f);
				goto IL_F61;
			}
			this.entity.flip = !this.entity.flip;
			if (behavior == NPCControl.ActionBehaviors.TurnRandomly)
			{
				this.actioncooldown = Random.Range(frequency / 2f, frequency * 2f);
				goto IL_F61;
			}
			this.actioncooldown = frequency;
			goto IL_F61;
		case NPCControl.ActionBehaviors.Wander:
		case NPCControl.ActionBehaviors.WanderUnderground:
		case NPCControl.ActionBehaviors.WanderOffscreen:
		case NPCControl.ActionBehaviors.WanderNoWarp:
		case NPCControl.ActionBehaviors.WanderOnWater:
			goto IL_8AF;
		case NPCControl.ActionBehaviors.FaceAwayFromPlayer:
			this.entity.StopForceMove(-1, false);
			if (MainManager.player != null)
			{
				this.entity.flip = (base.transform.position.x > MainManager.player.entity.transform.position.x);
				goto IL_F61;
			}
			goto IL_F61;
		case NPCControl.ActionBehaviors.Disguise:
		case NPCControl.ActionBehaviors.DisguiseOnce:
			if (this.disguisecooldown > 0)
			{
				if (this.disguisecooldown < 20 && MainManager.GetDistance(base.transform.position, this.entity.startpos.Value) > 2f)
				{
					this.entity.DetectDirection(this.entity.startpos.Value);
					this.entity.moverotater.LookAt(this.entity.startpos.Value);
					if (this.entity.HasGroundAhead(MainManager.player.transform.position))
					{
						this.entity.MoveTowards(this.entity.startpos.Value, 1f, 1, 0, true);
					}
					else
					{
						this.entity.StopForceMove(this.entity.animstate, false);
					}
					this.entity.animstate = this.entity.walkstate;
					this.entity.oldstate = -1;
					if (this.entity.hitwall)
					{
						base.transform.position = this.entity.startpos.Value;
						this.entity.rigid.velocity = Vector3.zero;
						if (this.entity.incamera)
						{
							MainManager.DeathSmoke(this.entity.sprite.transform.position);
						}
					}
				}
				else
				{
					this.entity.StopForceMove();
					this.disguisecooldown--;
					if (this.disguisecooldown == 80 || this.disguisecooldown == 40)
					{
						this.entity.flip = !this.entity.flip;
					}
				}
			}
			else if (this.disguisecooldown == 0)
			{
				this.entity.TempSpin(Vector3.up * 20f, 0.2f);
				this.disguisecooldown = -1;
				this.entity.StopForceMove();
			}
			this.entity.sprite.enabled = (this.disguisecooldown > 0);
			if (this.disguiseobj != null)
			{
				this.disguiseobj.gameObject.SetActive(this.disguisecooldown <= 0);
				goto IL_F61;
			}
			goto IL_F61;
		case NPCControl.ActionBehaviors.FollowPlayer:
		case NPCControl.ActionBehaviors.Unmoveable:
		case NPCControl.ActionBehaviors.DisguiseOnceJumpForward:
			goto IL_F61;
		case NPCControl.ActionBehaviors.FaceAhead:
		case NPCControl.ActionBehaviors.FaceBehind:
		case NPCControl.ActionBehaviors.FaceUp:
		case NPCControl.ActionBehaviors.FaceDown:
			this.entity.StopForceMove(-1, false);
			if (behavior == NPCControl.ActionBehaviors.FaceBehind)
			{
				this.entity.FaceTowards(base.transform.position - base.transform.right);
				goto IL_F61;
			}
			if (behavior == NPCControl.ActionBehaviors.FaceUp)
			{
				this.entity.FaceTowards(base.transform.position + base.transform.forward);
				goto IL_F61;
			}
			if (behavior == NPCControl.ActionBehaviors.FaceDown)
			{
				this.entity.FaceTowards(base.transform.position - base.transform.forward);
				goto IL_F61;
			}
			if (behavior == NPCControl.ActionBehaviors.FaceAhead)
			{
				this.entity.FaceTowards(base.transform.position + base.transform.right);
				goto IL_F61;
			}
			goto IL_F61;
		case NPCControl.ActionBehaviors.SetPath:
		case NPCControl.ActionBehaviors.StealthAI:
		case NPCControl.ActionBehaviors.SetPathJump:
			if (this.returntoheight)
			{
				this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, 0.1f);
			}
			if (behavior == NPCControl.ActionBehaviors.StealthAI && (int)frequency == 5555)
			{
				this.entity.animstate = 14;
				goto IL_F61;
			}
			if (!this.entity.forcemove && !this.entity.hitwall && this.vectordata != null && this.vectordata.Length != 0)
			{
				if (this.actioncooldown > 0f)
				{
					this.actioncooldown -= MainManager.framestep;
					goto IL_F61;
				}
				if (MainManager.GetSqrDistance(base.transform.position, this.vectordata[this.currentnode]) < 0.375f)
				{
					this.currentnode++;
				}
				if (this.currentnode >= this.vectordata.Length)
				{
					this.currentnode = 0;
				}
				this.entity.MoveTowards(this.vectordata[this.currentnode], 1f, 1, 0);
				this.actioncooldown = frequency;
				if (behavior == NPCControl.ActionBehaviors.SetPathJump)
				{
					this.entity.forcejump = true;
					goto IL_F61;
				}
				goto IL_F61;
			}
			else
			{
				if (this.entity.detect != null)
				{
					this.entity.DetectDirection(this.entity.forcetarget);
				}
				if (this.entity.hitwall)
				{
					this.entity.StopForceMove(-1, false);
					goto IL_F61;
				}
				goto IL_F61;
			}
			break;
		case NPCControl.ActionBehaviors.ChargeAtPlayer:
		case NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite:
			if (this.returntoheight)
			{
				this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, 0.1f);
			}
			this.behaviorroutine = base.StartCoroutine(this.ChargeAtPlayer(behavior, this.actioncooldown));
			goto IL_F61;
		case NPCControl.ActionBehaviors.ShootProjectile:
		case NPCControl.ActionBehaviors.ShootProjectilePredict:
			if (this.returntoheight)
			{
				this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, 0.1f);
			}
			if (this.entity.forcemove)
			{
				this.entity.StopForceMove();
			}
			this.behaviorroutine = base.StartCoroutine(this.ShootProjectile(behavior));
			goto IL_F61;
		case NPCControl.ActionBehaviors.AlwaysWander:
			flag = true;
			goto IL_8AF;
		case NPCControl.ActionBehaviors.ChangeSpriteInRandius:
			if (!(MainManager.player != null))
			{
				goto IL_F61;
			}
			if (Vector3.Distance(MainManager.player.transform.position, base.transform.position) < this.wanderradius)
			{
				this.entity.animstate = (int)this.actionfrequency[0];
				goto IL_F61;
			}
			this.entity.animstate = (int)this.actionfrequency[1];
			goto IL_F61;
		case NPCControl.ActionBehaviors.ChaseWhenAnim:
			if (this.entity.animstate != 23)
			{
				if (this.entity.animstate != (int)frequency)
				{
					this.behaviorcooldown = 20f;
					this.entity.StopForceMove();
					this.entity.animstate = (int)frequency;
					goto IL_F61;
				}
				goto IL_F61;
			}
			break;
		case NPCControl.ActionBehaviors.WalkWhenAnim:
			if (this.entity.animstate == 1 || (this.entity.animstate == 0 && this.behaviorcooldown <= 0f))
			{
				goto IL_8AF;
			}
			if (this.entity.animstate != (int)frequency)
			{
				this.behaviorcooldown = 20f;
				this.entity.StopForceMove();
				this.entity.animstate = (int)frequency;
				goto IL_F61;
			}
			goto IL_F61;
		default:
			goto IL_F61;
		}
		if (this.returntoheight)
		{
			this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, 0.1f);
		}
		this.entity.sprite.enabled = true;
		this.entity.forcemove = false;
		this.entity.FaceTowards(MainManager.player.transform.position);
		this.entity.emoticonid = 2;
		this.entity.emoticoncooldown = 2f;
		if (MainManager.player != null && this.entity.HasGroundAhead(MainManager.player.transform.position))
		{
			this.entity.Move(MainManager.player.transform.position, this.speedmultiplier, 23);
		}
		else
		{
			this.entity.StopForceMove(this.entity.animstate, false);
		}
		this.entity.oldstate = -1;
		this.entity.oldfly = false;
		this.entity.animstate = 23;
		base.transform.position = MainManager.LimitRadius(base.transform.position, this.entity.startpos.Value, this.radiuslimit, true);
		if (frequency > 10f || frequency < 0f)
		{
			frequency = 2f;
		}
		if (((behavior == NPCControl.ActionBehaviors.ChargeAttackUnderground && this.entity.digtime >= 30f) || behavior == NPCControl.ActionBehaviors.ChargeAndAttack) && Vector3.Distance(base.transform.position, MainManager.player.transform.position) < frequency)
		{
			this.behaviorroutine = base.StartCoroutine(this.ChargeAndAttack());
			goto IL_F61;
		}
		goto IL_F61;
		IL_8AF:
		if (this.entity.rigid.isKinematic || !this.entity.rigid.useGravity)
		{
			this.entity.LockRigid(false);
		}
		if (this.returntoheight)
		{
			this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, 0.1f);
		}
		if (this.entity.forcemove && (this.entity.hitwall || !this.entity.onground || (this.entity.detect != null && !this.entity.HasGroundAhead(this.entity.forcetarget))))
		{
			this.entity.StopForceMove(this.entity.basestate, false);
		}
		else if (this.actioncooldown <= 0f || flag)
		{
			if (flag && (MainManager.player.tattling || MainManager.instance.pause))
			{
				this.entity.StopForceMove();
			}
			else
			{
				if (this.entity.detect == null)
				{
					this.entity.CreateDetector();
				}
				Vector3 vector = this.entity.startpos.Value + MainManager.RandomVector(this.wanderradius, 0f, this.wanderradius) / 3f + Vector3.up * 0.5f;
				this.entity.moverotater.LookAt(vector);
				if (Vector3.Distance(base.transform.position, vector) < this.radiuslimit)
				{
					this.entity.MoveTowards(vector, 1f, this.entity.walkstate, this.entity.basestate, true);
					this.entity.detect.transform.LookAt(vector);
					this.actioncooldown = Random.Range(frequency / 3f, frequency);
					this.maxtries = 0;
				}
				else
				{
					this.trycount++;
				}
				if (this.trycount >= 50)
				{
					this.actioncooldown = Random.Range(frequency / 3f, frequency);
					this.trycount = 0;
					this.maxtries++;
				}
			}
		}
		else if (this.maxtries == 10)
		{
			this.entity.MoveTowards(this.entity.startpos.Value, 0.3f, this.entity.walkstate, this.entity.basestate, true);
			this.maxtries = 11;
		}
		else if (behavior != NPCControl.ActionBehaviors.WanderOnWater && behavior != NPCControl.ActionBehaviors.WanderNoWarp && (MainManager.GetDistance(base.transform.position, this.entity.startpos.Value) > this.teleportradius || this.maxtries >= 20))
		{
			if (this.entity.incamera)
			{
				MainManager.DeathSmoke(base.transform.position);
			}
			base.transform.position = this.entity.startpos.Value;
			this.entity.rigid.velocity = Vector3.zero;
			if (this.entity.incamera)
			{
				MainManager.DeathSmoke(base.transform.position);
			}
			this.maxtries = 0;
			this.actioncooldown = 100f;
		}
		else if (this.entity.forcemove)
		{
			if (flag && (MainManager.player.tattling || MainManager.instance.pause))
			{
				this.entity.StopForceMove();
			}
			else
			{
				if (this.walkcooldown > 60f && new Vector2(this.entity.rigid.velocity.x, this.entity.rigid.velocity.z).magnitude < this.entity.speed / 2f && this.entity.forcemove)
				{
					this.entity.StopForceMove(-1, false);
				}
				this.walkcooldown += MainManager.TieFramerate(1f);
			}
		}
		else if (!this.entity.forcemove)
		{
			this.walkcooldown = 0f;
			this.trycount = 0;
			this.entity.StopForceMove(-1, false);
			this.actioncooldown -= MainManager.TieFramerate(1f);
		}
		IL_F61:
		if (this.entity != null)
		{
			this.entity.ccol.center = new Vector3(0f, this.entity.ccol.height / 2f, 0f);
		}
		if (this.entity.animid > -1 && !this.entity.item && !this.entity.fixedentity)
		{
			if (this.entity.rigid.velocity == Vector3.zero)
			{
				this.entity.rigid.constraints = (RigidbodyConstraints)122;
				return;
			}
			this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}

	// Token: 0x060006BA RID: 1722 RVA: 0x000536C6 File Offset: 0x000518C6
	public IEnumerator StealthSpot()
	{
		RaycastHit raycastHit;
		Physics.Linecast(base.transform.position + Vector3.up, MainManager.player.transform.position + Vector3.up, out raycastHit, 10496);
		if (this.startlife > 20f && raycastHit.transform != null && MainManager.IsParty(raycastHit.transform) && MainManager.FreePlayer(false) && !MainManager.player.digging && this.battleids[0] > -1)
		{
			this.entity.StopForceMove();
			MainManager.events.StartEvent(this.battleids[0], this);
		}
		yield return null;
		yield break;
	}

	// Token: 0x060006BB RID: 1723 RVA: 0x000536D5 File Offset: 0x000518D5
	private IEnumerator ChargeAndAttack()
	{
		this.entity.StopForceMove();
		this.entity.overrideanim = true;
		MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
		if (animIDs <= MainManager.AnimIDs.Underling)
		{
			if (animIDs == MainManager.AnimIDs.FlyTrap)
			{
				goto IL_9D;
			}
			if (animIDs == MainManager.AnimIDs.Underling)
			{
				goto IL_233;
			}
		}
		else
		{
			if (animIDs == MainManager.AnimIDs.Sandworm)
			{
				goto IL_233;
			}
			if (animIDs == MainManager.AnimIDs.LeafbugClubber)
			{
				goto IL_9D;
			}
		}
		IL_67C:
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		if (!this.inrange)
		{
			this.entity.Emoticon(1, 60);
		}
		this.entity.overrideanim = false;
		this.attacking = false;
		this.StopForceBehavior();
		yield break;
		IL_9D:
		bool club = this.entity.originalid + 1 == 237;
		this.entity.animstate = 100;
		if (club)
		{
			this.entity.PlaySound("Toss3", 1f, 1.1f);
		}
		else
		{
			this.entity.PlaySound("Chew");
		}
		yield return new WaitForSeconds(0.2f);
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		this.entity.animstate = 102;
		this.attacking = true;
		if (club)
		{
			this.entity.PlaySound("Toss7");
		}
		else
		{
			this.entity.PlaySound("Bite");
		}
		yield return new WaitForSeconds(0.1f);
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		if (Vector3.Distance(base.transform.position, MainManager.player.transform.position) < 2f && !MainManager.instance.minipause)
		{
			this.StartBattle();
			this.entity.overrideanim = false;
			this.StopForceBehavior();
			yield break;
		}
		yield return new WaitForSeconds(0.5f);
		this.entity.animstate = 0;
		goto IL_67C;
		IL_233:
		club = (this.entity.originalid + 1 == 131);
		Vector3 pos = base.transform.position;
		Vector3 position = MainManager.player.transform.position;
		Vector3 position2 = base.transform.position;
		Vector3 dir = MainManager.GetDirection(position, position2).normalized;
		Vector3 tpos = MainManager.LimitRadius(MainManager.player.transform.position + dir * 1.5f, this.entity.startpos.Value, this.radiuslimit);
		float a = 0f;
		float b = 40f;
		this.entity.digging = false;
		this.entity.digtime = 0f;
		this.entity.overrridejump = false;
		this.entity.animstate = 100;
		yield return null;
		this.entity.sprite.transform.localScale = this.entity.startscale;
		if (this.entity.digpart != null && this.entity.digpart.Length != 0 && this.entity.digpart[1] != null)
		{
			this.entity.digpart[1].transform.position = new Vector3(0f, -9999f);
		}
		this.entity.overrideflip = true;
		if (this.dirtcd <= 0f)
		{
			MainManager.PlayParticle("DirtExplodeLight", base.transform.position, 1f).transform.localScale = Vector3.one * 0.75f;
		}
		this.dirtcd = 30f;
		this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
		for (;;)
		{
			base.transform.position = MainManager.BeizierCurve3(pos, tpos, 4f, a / b);
			if (club)
			{
				this.entity.sprite.transform.localEulerAngles = new Vector3(this.entity.sprite.transform.localEulerAngles.x, this.entity.sprite.transform.localEulerAngles.y, Mathf.Lerp(0f, 180f, a / b));
			}
			this.attacking = true;
			if (Vector3.Distance(base.transform.position, MainManager.player.transform.position) < 1.15f && !MainManager.instance.minipause)
			{
				break;
			}
			this.entity.DetectDirection(base.transform.position - dir);
			if (this.entity.hitwall)
			{
				goto IL_5C3;
			}
			yield return null;
			a += MainManager.TieFramerate(1f);
			while (MainManager.IsPaused())
			{
				yield return null;
			}
			if (a >= b)
			{
				goto IL_5C3;
			}
		}
		this.attacking = true;
		this.StartBattle();
		this.entity.overrideanim = false;
		this.StopForceBehavior();
		this.entity.overrideflip = false;
		yield break;
		IL_5C3:
		if (this.dirtcd <= 0f)
		{
			MainManager.PlayParticle("DirtExplode", base.transform.position, 1f).transform.localScale = Vector3.one * 0.75f;
		}
		this.dirtcd = 30f;
		this.entity.overrideflip = false;
		if (this.inrange)
		{
			this.entity.digging = true;
			this.entity.digtime = 100f;
		}
		pos = default(Vector3);
		dir = default(Vector3);
		tpos = default(Vector3);
		goto IL_67C;
	}

	// Token: 0x060006BC RID: 1724 RVA: 0x000536E4 File Offset: 0x000518E4
	private IEnumerator ShootProjectile(NPCControl.ActionBehaviors type)
	{
		this.entity.Emoticon(2, 60);
		if (this.projectiles == null)
		{
			this.projectiles = new List<OverworldProjectile>();
		}
		List<int> list = new List<int>();
		for (int i = 0; i < this.projectiles.Count; i++)
		{
			if (this.projectiles[i] == null)
			{
				list.Add(i);
			}
		}
		int[] array = list.ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			if (j < this.projectiles.Count)
			{
				this.projectiles.RemoveAt(array[j]);
			}
		}
		this.entity.FaceTowards(MainManager.player.transform.position);
		MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
		bool turret;
		float b;
		Vector3 t;
		if (animIDs <= MainManager.AnimIDs.WaspScout)
		{
			if (animIDs <= MainManager.AnimIDs.SneilEnemy)
			{
				if (animIDs == MainManager.AnimIDs.Bandit)
				{
					goto IL_FC9;
				}
				if (animIDs != MainManager.AnimIDs.SneilEnemy)
				{
					goto IL_1317;
				}
				if (this.projectiles.Count >= 1)
				{
					goto IL_1317;
				}
				this.entity.animstate = 100;
				this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
				yield return new WaitForSeconds(0.5f);
				while (MainManager.IsPaused())
				{
					yield return null;
				}
				if (this.dizzytime > 0f || this.freezecooldown > 0f)
				{
					goto IL_1390;
				}
				this.entity.PlaySound("Lazer");
				this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
				List<OverworldProjectile> list2 = this.projectiles;
				int spriteindex = 4;
				Vector3 startpos = base.transform.position - this.entity.sprite.transform.right + new Vector3(0f, 1f);
				Vector3 a2 = MainManager.player.transform.position + new Vector3(0f, 2f);
				Vector3 vector = MainManager.player.transform.position;
				Vector3 position = base.transform.position;
				list2.Add(OverworldProjectile.NewProjectile(this, spriteindex, startpos, a2 + MainManager.GetDirection(vector, position).normalized * 1.5f, default(Vector3), new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, 90f), Vector3.one * 0.75f, null, 0f, 60f, 0.25f));
				yield return new WaitForSeconds(0.75f);
			}
			else if (animIDs != MainManager.AnimIDs.BeeBot && animIDs != MainManager.AnimIDs.Turret)
			{
				if (animIDs != MainManager.AnimIDs.WaspScout)
				{
					goto IL_1317;
				}
				goto IL_D5B;
			}
			else if (this.projectiles.Count < 2)
			{
				turret = (this.entity.originalid + 1 == 179);
				this.entity.animstate = 100;
				this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
				yield return new WaitForSeconds(turret ? 0.7f : 0.5f);
				while (MainManager.IsPaused())
				{
					yield return null;
				}
				if (this.dizzytime > 0f || this.freezecooldown > 0f)
				{
					goto IL_1390;
				}
				if (turret)
				{
					this.entity.animstate = 102;
				}
				this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
				List<OverworldProjectile> list3 = this.projectiles;
				int spriteindex2 = 10;
				Vector3 startpos2 = base.transform.position - this.entity.sprite.transform.right + new Vector3(0f, 1.25f);
				Vector3 position2 = MainManager.player.transform.position;
				Vector3 vector = MainManager.player.transform.position;
				Vector3 position = base.transform.position;
				list3.Add(OverworldProjectile.NewProjectile(this, spriteindex2, startpos2, position2 + MainManager.GetDirection(vector, position).normalized * 1.5f, default(Vector3), new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, 0f), Vector3.one * 0.75f, "HoneyExplode", 2f, 60f, 0.25f));
				yield return new WaitForSeconds(turret ? 0.4f : 0.3f);
			}
		}
		else if (animIDs <= MainManager.AnimIDs.WaspBomber)
		{
			if (animIDs == MainManager.AnimIDs.LeafbugArcher)
			{
				goto IL_D5B;
			}
			if (animIDs == MainManager.AnimIDs.ChomperBrute)
			{
				goto IL_FC9;
			}
			if (animIDs != MainManager.AnimIDs.WaspBomber)
			{
				goto IL_1317;
			}
			if (this.projectiles.Count < 1)
			{
				this.entity.animstate = 101;
				List<OverworldProjectile> list4 = this.projectiles;
				int spriteindex3 = -30;
				Vector3 startpos3 = base.transform.position - this.entity.sprite.transform.right + new Vector3(0.25f, 1.25f, -0.1f);
				Vector3 position3 = MainManager.player.transform.position;
				Vector3 vector = MainManager.player.transform.position;
				Vector3 position = base.transform.position;
				list4.Add(OverworldProjectile.NewProjectile(this, spriteindex3, startpos3, position3 + MainManager.GetDirection(vector, position).normalized * 1.5f, new Vector3(0f, 0f, 20f), new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, 0f), Vector3.one, "explosionsmall", 5f, 80f, 0.5f));
				yield return EventControl.sec;
				this.entity.animstate = 0;
			}
		}
		else
		{
			if (animIDs == MainManager.AnimIDs.WildChomper)
			{
				goto IL_FC9;
			}
			if (animIDs != MainManager.AnimIDs.ToeBiter)
			{
				if (animIDs != MainManager.AnimIDs.DeadLanderA)
				{
					goto IL_1317;
				}
				if (this.projectiles.Count < 1)
				{
					this.entity.animstate = 103;
					yield return EventControl.sec;
					while (MainManager.IsPaused())
					{
						yield return null;
					}
					if (this.dizzytime > 0f || this.freezecooldown > 0f)
					{
						goto IL_1390;
					}
					this.entity.FaceTowards(MainManager.player.transform.position);
					this.entity.animstate = 104;
					List<OverworldProjectile> list5 = this.projectiles;
					int spriteindex4 = 10;
					Vector3 startpos4 = base.transform.position - this.entity.sprite.transform.right + new Vector3(0f, 1.25f);
					Vector3 position4 = MainManager.player.transform.position;
					Vector3 vector = MainManager.player.transform.position;
					Vector3 position = base.transform.position;
					list5.Add(OverworldProjectile.NewProjectile(this, spriteindex4, startpos4, position4 + MainManager.GetDirection(vector, position).normalized * 1.5f, default(Vector3), new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, 0f), Vector3.one * 0.75f, "HoneyExplode", 2f, 50f, 0.25f));
					yield return EventControl.halfsec;
				}
			}
			else if (this.projectiles.Count < 1)
			{
				this.entity.animstate = 100;
				yield return EventControl.halfsec;
				this.entity.animstate = 101;
				if (this.dizzytime > 0f)
				{
					goto IL_1390;
				}
				if (this.freezecooldown > 0f)
				{
					goto IL_1390;
				}
				while (!MainManager.FreePlayer(false) && !MainManager.player.digging)
				{
					yield return null;
				}
				if (this.internaltransform == null || this.internaltransform.Length == 0 || this.internaltransform[0] == null)
				{
					this.internaltransform = new Transform[]
					{
						MainManager.CreateRock(base.transform.position + new Vector3(0f, -2f), Vector3.one * 0.5f, Vector3.zero).transform
					};
				}
				float a = 0f;
				b = 10f;
				this.internaltransform[0].parent = MainManager.map.transform;
				this.destroyOnBattle.Add(this.internaltransform[0].gameObject);
				Vector3 p = this.internaltransform[0].position;
				Vector3 position5 = this.entity.sprite.transform.position;
				Vector3 vector = this.entity.transform.right;
				t = position5 + vector.normalized * -0.5f + new Vector3(0f, 3f);
				if (this.dizzytime > 0f || this.freezecooldown > 0f)
				{
					goto IL_1390;
				}
				do
				{
					this.internaltransform[0].position = Vector3.Lerp(p, t, a / b);
					if (this.dizzytime > 0f || this.freezecooldown > 0f)
					{
						goto IL_1390;
					}
					if (!MainManager.instance.pause)
					{
						a += MainManager.TieFramerate(1f);
					}
					yield return null;
				}
				while (a < b + 1f);
				yield return EventControl.quartersec;
				while (!MainManager.FreePlayer(false))
				{
					yield return null;
				}
				this.entity.animstate = 28;
				while (!MainManager.FreePlayer(false))
				{
					yield return null;
				}
				if (this.dizzytime > 0f || this.freezecooldown > 0f)
				{
					goto IL_1390;
				}
				MainManager.PlaySound("Toss3", -1, 0.9f, 1f);
				List<OverworldProjectile> list6 = this.projectiles;
				int spriteindex5 = 17;
				Vector3 position6 = this.internaltransform[0].transform.position;
				Vector3 position7 = MainManager.player.transform.position;
				vector = MainManager.player.transform.position;
				Vector3 position = base.transform.position;
				list6.Add(OverworldProjectile.NewProjectile(this, spriteindex5, position6, position7 + MainManager.GetDirection(vector, position).normalized * 1.5f, default(Vector3), new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, 90f), Vector3.one * 0.5f, "Rock", 3f, 40f, 0.25f));
				OverworldProjectile overworldProjectile = this.projectiles[0];
				this.internaltransform[0].parent = overworldProjectile.transform;
				this.internaltransform = null;
				yield return EventControl.halfsec;
				p = default(Vector3);
				t = default(Vector3);
			}
		}
		IL_1371:
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		yield return null;
		goto IL_1390;
		IL_D5B:
		if (this.projectiles.Count >= 1)
		{
			goto IL_1371;
		}
		turret = (this.entity.originalid + 1 == 236);
		this.entity.animstate = 100;
		if (turret)
		{
			this.entity.PlaySound("Rope1");
		}
		this.entity.FaceTowards(MainManager.player.transform.position);
		yield return new WaitForSeconds(turret ? 0.5f : 0.25f);
		this.entity.animstate = 102;
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		if (this.dizzytime <= 0f && this.freezecooldown <= 0f)
		{
			this.entity.PlaySound("Toss");
			this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
			List<OverworldProjectile> list7 = this.projectiles;
			int spriteindex6 = turret ? 12 : 2;
			Vector3 startpos5 = this.entity.transform.position - this.entity.sprite.transform.right + (turret ? new Vector3(1f, 1f, -0.1f) : new Vector3(0f, 1.75f));
			Vector3 position8 = MainManager.player.transform.position;
			Vector3 vector = MainManager.player.transform.position;
			Vector3 position = base.transform.position;
			list7.Add(OverworldProjectile.NewProjectile(this, spriteindex6, startpos5, position8 + MainManager.GetDirection(vector, position).normalized * 1.5f, Vector3.zero, new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, (float)(turret ? -90 : -60)), Vector3.one * 0.75f, null, (float)(turret ? 2 : 0), 45f, 0.25f));
			yield return new WaitForSeconds(0.75f);
			goto IL_1371;
		}
		goto IL_1390;
		IL_FC9:
		if (this.projectiles.Count >= 1)
		{
			goto IL_1371;
		}
		int projid = 3;
		int targanim = 100;
		string tosssound = "Spin3";
		float seconds = 0.5f;
		b = 0f;
		t = MainManager.instance.globalcamdir.forward * 0.1f;
		animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
		if (animIDs != MainManager.AnimIDs.Bandit)
		{
			if (animIDs == MainManager.AnimIDs.ChomperBrute || animIDs == MainManager.AnimIDs.WildChomper)
			{
				t = new Vector3(0f, 2f);
				tosssound = "PingShot";
				targanim = 102;
				b = 0.2f;
				projid = -38;
				this.entity.PlaySound("Clomp");
				this.entity.animstate = 100;
			}
		}
		else
		{
			t = new Vector3(0f, 1f);
			this.entity.animstate = 100;
		}
		this.entity.FaceTowards(MainManager.player.transform.position);
		yield return new WaitForSeconds(seconds);
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		this.entity.animstate = targanim;
		if (b > 0f)
		{
			yield return new WaitForSeconds(b);
		}
		while (MainManager.IsPaused())
		{
			yield return null;
		}
		if (this.dizzytime <= 0f && this.freezecooldown <= 0f)
		{
			this.entity.PlaySound(tosssound);
			this.entity.FlipSpriteAngleAt(MainManager.player.transform.position, new Vector3(0f, 90f));
			List<OverworldProjectile> list8 = this.projectiles;
			int spriteindex7 = projid;
			Vector3 startpos6 = base.transform.position - this.entity.sprite.transform.right + t;
			Vector3 a3 = MainManager.player.transform.position + new Vector3(0f, 1.25f);
			Vector3 vector = MainManager.player.transform.position;
			Vector3 position = base.transform.position;
			list8.Add(OverworldProjectile.NewProjectile(this, spriteindex7, startpos6, a3 + MainManager.GetDirection(vector, position).normalized * 2f, new Vector3(0f, 0f, 20f), new Vector3(0f, this.entity.sprite.transform.eulerAngles.y, 90f), Vector3.one * 0.75f, null, 1.25f, 60f, 0.25f));
			yield return new WaitForSeconds(0.75f);
			tosssound = null;
			t = default(Vector3);
			goto IL_1371;
		}
		goto IL_1390;
		IL_1317:
		this.entity.animstate = this.entity.basestate;
		this.entity.overrideonlyflip = false;
		yield return null;
		this.StopForceBehavior();
		yield break;
		IL_1390:
		this.entity.animstate = this.entity.basestate;
		if (!this.inrange)
		{
			this.entity.overrideonlyflip = false;
		}
		yield return null;
		this.StopForceBehavior();
		if (!this.inrange)
		{
			this.entity.Emoticon(1, 60);
		}
		yield break;
	}

	// Token: 0x060006BD RID: 1725 RVA: 0x000536F3 File Offset: 0x000518F3
	private IEnumerator ChargeAtPlayer(NPCControl.ActionBehaviors type, float cooldown)
	{
		this.entity.LockRigid(false);
		this.entity.PlaySound("Find");
		float startc = cooldown;
		this.entity.StopForceMove();
		int state = 23;
		if (this.entity.height > 0.1f)
		{
			state = 26;
		}
		this.entity.animstate = state;
		if (this.entity.onground)
		{
			this.entity.Jump();
		}
		this.entity.emoticonid = 2;
		this.entity.emoticoncooldown = 300f;
		yield return new WaitForSeconds(0.25f);
		this.entity.overrideonlyflip = false;
		this.attacking = true;
		EntityControl entityControl = this.entity;
		Vector3 position = MainManager.player.transform.position;
		Vector3 position2 = MainManager.player.transform.position;
		Vector3 position3 = base.transform.position;
		entityControl.MoveTowards(MainManager.LimitRadius(position + MainManager.GetDirection(position2, position3).normalized * 1.5f, this.entity.startpos.Value, this.radiuslimit), this.speedmultiplier, state, 0, true);
		if (type == NPCControl.ActionBehaviors.ChargeAtPlayerFlipSprite)
		{
			this.entity.overrideonlyflip = true;
			this.entity.sprite.transform.LookAt(MainManager.player.transform.position);
			this.entity.sprite.transform.localEulerAngles = new Vector3(0f, this.entity.sprite.transform.localEulerAngles.y + 90f);
		}
		if (this.entity == null)
		{
			this.entity.CreateDetector();
		}
		this.entity.DetectDirection(MainManager.player.transform.position);
		while (this.entity.forcemove && !this.entity.hitwall)
		{
			this.entity.emoticonid = 2;
			this.entity.emoticoncooldown = 5f;
			if (this.entity.initialheight > 0.1f)
			{
				this.entity.height = Mathf.Lerp(this.entity.initialheight, 0.25f, MainManager.GetSqrDistance(this.entity.transform.position, this.entity.forcetarget, true) / MainManager.GetSqrDistance(this.entity.startpos.Value, this.entity.forcetarget, true));
			}
			yield return null;
		}
		yield return null;
		this.entity.sprite.transform.localEulerAngles = this.entity.FlipAngle();
		this.attacking = false;
		this.entity.overrideonlyflip = false;
		this.entity.StopForceMove(0, false);
		if (this.entity.onground)
		{
			this.entity.Jump();
		}
		yield return new WaitForSeconds(0.2f);
		while (!this.entity.onground)
		{
			yield return null;
		}
		yield return null;
		while (cooldown > 0f)
		{
			this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, 1f - cooldown / startc);
			cooldown -= MainManager.TieFramerate(1f);
			yield return null;
		}
		yield return null;
		this.entity.height = this.entity.initialheight;
		this.StopForceBehavior();
		yield break;
	}

	// Token: 0x060006BE RID: 1726 RVA: 0x00053710 File Offset: 0x00051910
	public void StopForceBehavior()
	{
		if (this.dummy)
		{
			return;
		}
		this.forcebehavior = null;
		if (this.behaviorroutine != null)
		{
			base.StopCoroutine(this.behaviorroutine);
		}
		this.behaviorroutine = null;
		if (!MainManager.instance.inevent)
		{
			this.entity.StopForceMove();
		}
		if (this.entity.overrideonlyflip)
		{
			this.entity.overrideonlyflip = false;
		}
		this.attacking = false;
		if (this.entitytype == NPCControl.NPCType.Enemy && !MainManager.instance.inevent)
		{
			this.entity.sprite.transform.localEulerAngles = new Vector3(0f, this.entity.FlipAngle().y);
		}
	}

	// Token: 0x060006BF RID: 1727 RVA: 0x000537C8 File Offset: 0x000519C8
	public bool HasBehavior(NPCControl.ActionBehaviors target)
	{
		if (this.behaviors != null)
		{
			for (int i = 0; i < this.behaviors.Length; i++)
			{
				if (this.behaviors[i] == target)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060006C0 RID: 1728 RVA: 0x00053800 File Offset: 0x00051A00
	private void LateUpdate()
	{
		if ((float.IsNaN(base.transform.position.x) || float.IsNaN(base.transform.position.y) || float.IsNaN(base.transform.position.z)) && this.entity.startpos != null)
		{
			if (this.trapped)
			{
				for (int i = 0; i < MainManager.map.entities.Length; i++)
				{
					if (MainManager.map.entities[i] != null && MainManager.map.entities[i].npcdata != null && MainManager.map.entities[i].npcdata.entitytype == NPCControl.NPCType.Object && MainManager.map.entities[i].npcdata.objecttype == NPCControl.ObjectTypes.CoiledObject && MainManager.map.entities[i].npcdata.data[0] == this.mapid)
					{
						base.transform.localPosition = MainManager.map.entities[i].npcdata.vectordata[0];
						this.entity.LockRigid(true);
						break;
					}
				}
			}
			else
			{
				base.transform.position = this.entity.startpos.Value;
				this.entity.onground = false;
			}
		}
		if (this.destroyOnBattle != null && this.destroyOnBattle.Count > 10)
		{
			this.destroyOnBattle.RemoveAll((GameObject x) => x == null);
		}
		if (!this.dummy)
		{
			if (!this.entity.iskill && !this.entity.dead && this.entity.deathcoroutine == null && !this.entity.activeonpause && this.entitytype == NPCControl.NPCType.NPC)
			{
				if (!MainManager.instance.inevent)
				{
					if (this.entity.originalid != -1 && this.entity.rigid.isKinematic != (MainManager.instance.insideid != this.insideid))
					{
						this.entity.LockRigid(MainManager.instance.insideid != this.insideid);
					}
					if (this.entity.forcemove && MainManager.instance.insideid != this.insideid)
					{
						this.entity.StopForceMove();
					}
				}
				else if (!this.entity.fixedentity && this.entity.originalid > -1 && this.entity.forcemove)
				{
					if (MainManager.instance.insideid == this.insideid)
					{
						this.entity.LockRigid(false, false);
					}
					else
					{
						this.entity.StopForceMove();
					}
				}
			}
			if (this.entity.incamera)
			{
				if (this.pusher != null)
				{
					this.pusher.enabled = (!MainManager.instance.inevent && !MainManager.instance.message && !MainManager.instance.minipause);
				}
				if (this.startlife < 300f)
				{
					this.startlife += MainManager.framestep;
					if (this.entity != null && !this.entity.activeonpause && this.behaviors != null && this.HasBehavior(NPCControl.ActionBehaviors.WanderOffscreen))
					{
						this.entity.activeonpause = true;
						this.entity.alwaysactive = true;
					}
				}
				if (this.dirtcd > 0f)
				{
					this.dirtcd -= MainManager.framestep;
				}
				if (this.startlife > 20f && this.entitytype != NPCControl.NPCType.Object && this.entity.animid > -1 && this.entity.model == null && this.entity.sprite != null && !this.entity.hologram && this.insideid == MainManager.instance.insideid)
				{
					this.entity.sprite.material.color = Color.Lerp(this.entity.sprite.material.color, new Color(this.entity.sprite.material.color.r, this.entity.sprite.material.color.g, this.entity.sprite.material.color.b, (this.entity.campos.z < 2.5f) ? 0.3f : 1f), MainManager.TieFramerate(0.1f));
				}
				if (this.interactcd > 0f)
				{
					this.interactcd -= MainManager.framestep;
				}
				if (this.interacttype == NPCControl.Interaction.StorageAnt && !MainManager.instance.message && !MainManager.instance.inevent && !this.entity.item)
				{
					this.entity.animstate = this.entity.basestate;
				}
				if (this.pusher != null)
				{
					this.pusher.center = new Vector3(0f, this.colliderheight + this.entity.height, 0f);
				}
				if (!this.hit && this.entitytype == NPCControl.NPCType.Object && this.objecttype == NPCControl.ObjectTypes.CoiledObject && this.moveobj == null && !this.entity.iskill)
				{
					EntityControl entityControl = MainManager.GetEntity(this.data[0]);
					if (entityControl != null)
					{
						entityControl.npcdata.trapped = true;
						entityControl.LockRigid(true);
						entityControl.rigid.velocity = Vector3.zero;
						entityControl.transform.parent = this.entity.sprite.transform;
						entityControl.transform.localScale = Vector3.one;
						entityControl.transform.localPosition = this.vectordata[0];
						this.moveobj = entityControl.transform;
					}
				}
				if (!MainManager.instance.minipause && !MainManager.instance.pause && !MainManager.instance.inevent && this.entitytype == NPCControl.NPCType.Enemy && this.eventid > 0 && this.entity.iskill)
				{
					if (this.respawntimer <= -100f)
					{
						this.respawntimer = (float)this.eventid;
					}
					else if (this.respawntimer > 0f)
					{
						this.respawntimer -= MainManager.TieFramerate(1f);
					}
					else
					{
						this.RespawnEnemy(this);
						this.entity.iskill = false;
						this.respawntimer = -110f;
					}
				}
				if (this.disguiseobj != null)
				{
					if (this.freezecooldown <= 0f && this.dizzytime <= 0f && !MainManager.instance.minipause)
					{
						if (this.disguisecooldown > -1)
						{
							this.entity.height = Mathf.Lerp(this.entity.height, this.entity.initialheight, MainManager.TieFramerate(0.1f));
						}
						else
						{
							this.entity.height = Mathf.Lerp(this.entity.height, 0f, MainManager.framestep * 0.1f);
						}
					}
					else if (MainManager.instance.minipause && this.disguiseobj.gameObject.activeSelf == this.disguisecooldown > -1)
					{
						this.disguiseobj.gameObject.SetActive(this.disguisecooldown == -1);
						this.entity.sprite.enabled = !this.disguiseobj.gameObject.activeSelf;
					}
				}
				if (Time.frameCount % 3 == 0)
				{
					if ((!MainManager.map.limitbehavior || this.entity.incamera) && !MainManager.instance.inevent && !MainManager.instance.minipause && this.insideid == MainManager.instance.insideid && !this.entity.dead && !this.entity.iskill && this.entity.deathcoroutine == null)
					{
						this.RefreshPlayer(MainManager.player != null && MainManager.GetDistance(base.transform.position, MainManager.player.transform.position, false) < this.radius && MainManager.instance.insideid == this.insideid);
						if (MainManager.player != null && this != MainManager.player.beemerang)
						{
							if (this.inrange && !MainManager.player.npc.Contains(this))
							{
								MainManager.player.npc.Add(this);
							}
							else if (!this.inrange && MainManager.player.npc.Contains(this))
							{
								MainManager.player.npc.Remove(this);
							}
							if (MainManager.player.canpause && this.entitytype == NPCControl.NPCType.NPC && ((MainManager.player.npc.Count > 0 && MainManager.player.npc[0] != this) || MainManager.player.npc.Count == 0))
							{
								this.CheckEmoteFlag();
							}
						}
					}
					if (MainManager.player != null && MainManager.player.beemerang != this && this.entity.ccol != null && this.entity.ccol.height != this.colliderheight)
					{
						this.entity.ccol.height = this.colliderheight;
						this.entity.ccol.center = new Vector3(0f, this.colliderheight / 2f, 0f);
					}
					if (this.entity.iskill && base.transform.position.y > -999f)
					{
						this.entity.rigid.useGravity = false;
						this.entity.ccol.enabled = false;
						if (this.boxcol != null)
						{
							this.boxcol.enabled = false;
						}
						base.transform.position = new Vector3(0f, -1000f, 0f);
					}
				}
				if (this.behaviorcooldown > 0f)
				{
					this.behaviorcooldown -= MainManager.TieFramerate(1f);
				}
				this.SpecialTattles();
				if (Time.frameCount % 3 == 0 && (this.interacttype == NPCControl.Interaction.CaravanBadge || this.interacttype == NPCControl.Interaction.Shop))
				{
					if (!MainManager.instance.message && this.inrange && MainManager.player.npc.Count > 0 && MainManager.player.npc[0] == this && this.descwindow == null && !MainManager.instance.pause && MainManager.instance.insideid == this.insideid)
					{
						if (this.interacttype == NPCControl.Interaction.CaravanBadge || (int)this.shopkeeper.dialogues[1].y != 1)
						{
							MainManager.instance.showmoney = 1f;
						}
						else
						{
							MainManager.instance.showmoney = 0f;
						}
						this.CreateDescWindow(true);
					}
					else if (this.descwindow != null && !MainManager.instance.message && (MainManager.instance.insideid != this.insideid || MainManager.instance.pause || MainManager.player.npc.Count == 0 || MainManager.player.npc[0] != this || MainManager.instance.itempicked))
					{
						this.DestroyDescWindow();
					}
				}
				if (this.entitytype == NPCControl.NPCType.Object && this.objecttype == NPCControl.ObjectTypes.Geizer && !this.hit)
				{
					if (this.internaltransform != null)
					{
						this.internaltransform[3].transform.localScale = Vector3.one;
					}
					RaycastHit raycastHit;
					Physics.Raycast(base.transform.position + Vector3.up * 10f, Vector3.down, out raycastHit, 10f, 8448);
					if (!this.attacking)
					{
						Fader fader = this.entity.gameObject.AddComponent<Fader>();
						fader.forcestayonpause = true;
						fader.childtied = true;
						fader.fadedistance = 0f;
						fader.pivotoffset = new Vector3(0f, raycastHit.point.y - this.entity.startpos.Value.y, 0f);
						this.attacking = true;
					}
					if (this.data.Length > 3 && this.data[3] == 1 && this.startlife < 20f)
					{
						base.transform.position = this.entity.startpos.Value + -base.transform.up.normalized * 10f;
					}
					if (raycastHit.transform != null)
					{
						this.internaltransform[3].parent = this.entity.sprite.transform;
						this.internaltransform[3].position = raycastHit.point;
					}
				}
				if (!this.entity.iskill)
				{
					if (base.transform.position.y < MainManager.map.ylimit && this.objecttype != NPCControl.ObjectTypes.Dropplet)
					{
						base.transform.position = this.entity.startpos.Value;
						if (this.entity.animid > -1 && this.entity.incamera && !this.entity.dead && !this.entity.iskill)
						{
							MainManager.DeathSmoke(this.entity.sprite.transform.position);
						}
					}
					if (this.entitytype != NPCControl.NPCType.Object && MainManager.map.waterfloat != null && (this.HasBehavior(NPCControl.ActionBehaviors.WanderOnWater) || this.HasBehavior(NPCControl.ActionBehaviors.ChaseOnWater)))
					{
						base.transform.position = new Vector3(base.transform.position.x, Mathf.Clamp(MainManager.map.waterfloat.transform.position.y, MainManager.map.waterfloat.minwaterfloat - 0.5f, float.PositiveInfinity), base.transform.position.z);
						Vector3 value = this.entity.startpos.Value;
						this.entity.startpos = new Vector3?(new Vector3(value.x, base.transform.position.y, value.z));
					}
				}
			}
		}
		this.collisionammount = 0;
		this.ignoreconstraint = false;
	}

	// Token: 0x060006C1 RID: 1729 RVA: 0x00054768 File Offset: 0x00052968
	private void SpecialTattles()
	{
		NPCControl.Interaction interaction = this.interacttype;
		if (interaction == NPCControl.Interaction.QuestBoard)
		{
			this.tattleid = -1;
			return;
		}
		if (interaction != NPCControl.Interaction.VenusHeal)
		{
			return;
		}
		this.tattleid = -117;
	}

	// Token: 0x060006C2 RID: 1730 RVA: 0x00054798 File Offset: 0x00052998
	public void CreateDescWindow(bool shop)
	{
		MainManager.instance.discoveryhud = 0f;
		if (this.entity.item)
		{
			this.entity.animstate = this.entity.itemstate;
		}
		this.descwindow = MainManager.Create9Box(new Vector3(-3.5f, -3.9f, 10f), new Vector2(11f, 3f), 0, -3, Color.white, true).GetComponent<DialogueAnim>();
		if (!shop)
		{
			MainManager.instance.showmoney = 0f;
			this.descwindow.transform.localPosition = new Vector3(0f, this.descwindow.transform.localPosition.y - 0.5f, this.descwindow.transform.localPosition.z);
		}
		if (this.shopkeeper != null && (int)this.shopkeeper.dialogues[1].y == 1)
		{
			SpriteRenderer component = MainManager.NewUIObject("CBerryBar", this.descwindow.transform, new Vector3(10.65f, -0.45f), new Vector3(0.55f, 0.6f, 1f), MainManager.guisprites[4], 0).GetComponent<SpriteRenderer>();
			component.color = Color.cyan;
			base.StartCoroutine(MainManager.SetText("|sort,10||color,4||single||dropshadow,0.75,-0.75|" + MainManager.instance.flagvar[14].ToString().PadLeft(3, '0'), 2, null, false, false, new Vector3(-0.65f, -0.5f), Vector3.zero, Vector2.one * 1.75f, component.transform, null));
			component = MainManager.NewUIObject("BerryIcon", this.descwindow.transform, new Vector3(9.45f, -0.45f), Vector3.one, MainManager.guisprites[83], 1).GetComponent<SpriteRenderer>();
		}
		string text = "|single||singlebreak," + MainManager.itemdescbreak + "|";
		if (this.entity.animid == 2)
		{
			MainManager.instance.flagvar[10] = ((this.shopkeeper != null && MainManager.instance.flags[681]) ? 35 : Convert.ToInt32(MainManager.badgedata[this.entity.animstate, 5]));
			if (this.shopkeeper != null && (int)this.shopkeeper.dialogues[1].y == 1)
			{
				MainManager.instance.flagvar[10] = ((this.shopkeeper != null && MainManager.instance.flags[681]) ? ((MainManager.instance.flagvar[66] >= 2) ? 4 : 3) : Convert.ToInt32(MainManager.badgedata[this.entity.animstate, 7]));
			}
			if (shop)
			{
				text = string.Concat(new string[]
				{
					text,
					MainManager.instance.flags[681] ? MainManager.menutext[59] : MainManager.badgedata[this.entity.animstate, 0],
					" ",
					(MainManager.languageid == 4) ? "—" : "-",
					" ",
					((int)this.shopkeeper.dialogues[1].y == 1) ? (MainManager.instance.flagvar[10] + " " + MainManager.menutext[(MainManager.instance.flagvar[10] == 1) ? 112 : 169]) : MainManager.menutext[49],
					"|line|"
				});
			}
			text += ((this.shopkeeper != null && MainManager.instance.flags[681]) ? MainManager.menutext[59] : MainManager.badgedata[this.entity.animstate, 1]);
		}
		else
		{
			int num = (this.entity.animid == 1) ? 0 : this.entity.animid;
			MainManager.instance.flagvar[10] = Mathf.CeilToInt((float)Convert.ToInt32(MainManager.itemdata[num, this.entity.animstate, 4]) * this.mmulti);
			if (shop)
			{
				text = string.Concat(new string[]
				{
					text,
					MainManager.itemdata[num, this.entity.animstate, 0],
					" ",
					(MainManager.languageid == 4) ? "—" : "-",
					" ",
					MainManager.menutext[49],
					"|line|"
				});
			}
			text += MainManager.itemdata[num, this.entity.animstate, 2];
		}
		MainManager.instance.StartCoroutine(MainManager.SetText(text, 0, null, false, false, new Vector3(-5.2f, 0.65f), Vector3.zero, new Vector2(0.675f, 0.675f), this.descwindow.transform, null));
	}

	// Token: 0x060006C3 RID: 1731 RVA: 0x00054CDC File Offset: 0x00052EDC
	public void CreateDescWindow(int type, int id)
	{
		this.descwindow = MainManager.Create9Box(new Vector3(0f, -4.4f, 10f), new Vector2(11f, 3f), 0, -3, Color.white, true).GetComponent<DialogueAnim>();
		string text = "|single||singlebreak," + MainManager.itemdescbreak + "|";
		if (type == 2)
		{
			MainManager.instance.flagvar[10] = Convert.ToInt32(MainManager.badgedata[id, 5]);
			text += MainManager.badgedata[id, 1];
		}
		else
		{
			MainManager.instance.flagvar[10] = Convert.ToInt32(MainManager.itemdata[type, id, 4]);
			text += MainManager.itemdata[type, id, 2];
		}
		MainManager.instance.StartCoroutine(MainManager.SetText(text, 0, null, false, false, new Vector3(-5.2f, 0.65f), Vector3.zero, new Vector2(0.675f, 0.675f), this.descwindow.transform, null));
	}

	// Token: 0x060006C4 RID: 1732 RVA: 0x00054DF8 File Offset: 0x00052FF8
	public void DestroyDescWindow()
	{
		if (this.descwindow != null)
		{
			this.descwindow.shrink = true;
			this.descwindow.transform.position += MainManager.instance.globalcamdir.forward.normalized * 0.1f;
			Object.Destroy(this.descwindow.gameObject, 0.5f);
			this.descwindow = null;
			if (this.inrange && (this.interacttype == NPCControl.Interaction.Shop || this.interacttype == NPCControl.Interaction.CaravanBadge))
			{
				this.inrange = false;
				MainManager.player.npc = new List<NPCControl>();
			}
		}
	}

	// Token: 0x060006C5 RID: 1733 RVA: 0x00054EAC File Offset: 0x000530AC
	private void CheckEmoteFlag()
	{
		if (MainManager.player != null && (this.entity.alwaysemoticon || (MainManager.GetDistance(MainManager.player.transform.position.y, base.transform.position.y) < 2f && MainManager.GetDistance(MainManager.player.transform.position.z, base.transform.position.z) < 15f)))
		{
			for (int i = this.emoticonflag.Length - 1; i >= 0; i--)
			{
				if ((i > 0 && (int)this.emoticonflag[i].x > -1 && MainManager.instance.flags[(int)this.emoticonflag[i].x]) || i == 0)
				{
					this.SetEmoticonTemp((int)this.emoticonflag[i].y - 1);
					return;
				}
			}
		}
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x00054FA4 File Offset: 0x000531A4
	private void SetEmoticonTemp(int id)
	{
		this.entity.emoticonid = id;
		this.entity.emoticoncooldown = 5f;
	}

	// Token: 0x060006C7 RID: 1735 RVA: 0x00054FC4 File Offset: 0x000531C4
	public string GetDialogue()
	{
		if (this.interacttype != NPCControl.Interaction.Shop)
		{
			if (this.overridediag > -1)
			{
				if (MainManager.map.useglobalcommand)
				{
					MainManager.map.currentline = this.overridediag;
				}
				return MainManager.map.dialogues[this.overridediag];
			}
			if (this.interacttype != NPCControl.Interaction.SavePoint)
			{
				for (int i = this.dialogues.Length - 1; i >= 0; i--)
				{
					if ((int)this.dialogues[i].x == -1 || ((int)this.dialogues[i].x > -1 && MainManager.instance.flags[(int)this.dialogues[i].x]))
					{
						this.entity.animstate = (int)this.dialogues[i].z;
						this.entity.basestate = this.entity.animstate;
						this.currentdialogueindex = i;
						return MainManager.GetDialogueText((int)this.dialogues[i].y);
					}
				}
			}
		}
		return "|color,1|Invalid message.";
	}

	// Token: 0x060006C8 RID: 1736 RVA: 0x000550DC File Offset: 0x000532DC
	public int GetDialogueIndex()
	{
		if (this.overridediag > -1)
		{
			return this.overridediag;
		}
		for (int i = this.dialogues.Length - 1; i >= 0; i--)
		{
			if ((int)this.dialogues[i].x == -1 || ((int)this.dialogues[i].x > -1 && MainManager.instance.flags[(int)this.dialogues[i].x]))
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x060006C9 RID: 1737 RVA: 0x0005515C File Offset: 0x0005335C
	private bool CheckOtherEntityActive(int id)
	{
		return id == -1 || (MainManager.map.entities[Mathf.Abs(id)] != null && MainManager.map.entities[Mathf.Abs(id)].npcdata != null && MainManager.map.entities[Mathf.Abs(id)].npcdata.hit == id > 0);
	}

	// Token: 0x060006CA RID: 1738 RVA: 0x000551CC File Offset: 0x000533CC
	public void Interact(string args)
	{
		this.entity.StopForceMove(-1, false);
		MainManager.player.entity.StopMoving(0);
		MainManager.player.CancelAction();
		if (this.behaviors != null && this.behaviors.Length > 1 && this.behaviors[1] == NPCControl.ActionBehaviors.FacePlayer)
		{
			this.entity.FaceTowards(MainManager.player.transform.position);
		}
		switch (this.interacttype)
		{
		case NPCControl.Interaction.Talk:
		case NPCControl.Interaction.Check:
		case NPCControl.Interaction.SavePoint:
		case NPCControl.Interaction.TalkReturnToOriginalFlip:
		case NPCControl.Interaction.ShopKeeper:
		case NPCControl.Interaction.StorageAnt:
		case NPCControl.Interaction.VenusHeal:
			break;
		case NPCControl.Interaction.Event:
			MainManager.events.StartEvent(this.eventid, this);
			return;
		case NPCControl.Interaction.Shop:
		case NPCControl.Interaction.CaravanBadge:
			if (this.entity.item)
			{
				this.entity.animstate = this.entity.itemstate;
			}
			if (this.entity.animid == 2 || this.interacttype == NPCControl.Interaction.CaravanBadge)
			{
				MainManager.instance.flagvar[0] = this.entity.animstate;
				MainManager.instance.flagvar[1] = (MainManager.instance.flags[681] ? 35 : Convert.ToInt32(MainManager.badgedata[this.entity.animstate, 5]));
				MainManager.instance.flagstring[0] = (MainManager.instance.flags[681] ? MainManager.menutext[59] : MainManager.badgedata[this.entity.animstate, 0]);
			}
			else
			{
				MainManager.instance.flagvar[0] = this.entity.animstate;
				MainManager.instance.flagvar[1] = Mathf.CeilToInt((float)Convert.ToInt32(MainManager.itemdata[this.entity.animid, this.entity.animstate, 4]) * this.mmulti);
				MainManager.instance.flagstring[0] = MainManager.itemdata[this.entity.animid, this.entity.animstate, 0];
			}
			if (this.interacttype == NPCControl.Interaction.CaravanBadge)
			{
				EntityControl entityControl = MainManager.GetEntity(this.data[0]);
				MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.GetDialogueText(this.data[1]), 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, entityControl.transform, this));
				for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
				{
					MainManager.instance.playerdata[i].entity.FaceTowards(entityControl.transform.position, false, true);
				}
				return;
			}
			args = "buy";
			break;
		case NPCControl.Interaction.QuestBoard:
			if (this.data[2] > -1 && !MainManager.instance.flags[this.data[2]])
			{
				MainManager.GetEntity(this.data[0]).npcdata.Interact("");
				return;
			}
			MainManager.instance.StartCoroutine(MainManager.OpenQuestBoard(MainManager.GetEntity(this.data[0]), this));
			return;
		case NPCControl.Interaction.LockedDoor:
			MainManager.events.StartEvent(59, this);
			return;
		default:
			return;
		}
		string text = this.GetDialogue();
		Transform transform = base.transform;
		if (this.interacttype == NPCControl.Interaction.StorageAnt)
		{
			MainManager.instance.flags[349] = true;
			if (!MainManager.instance.flags[180])
			{
				text = "|anim,caller,Happy|" + MainManager.commondialogue[97];
			}
			else
			{
				text = MainManager.commondialogue[1];
			}
			MainManager.instance.flags[180] = true;
		}
		else if (this.interacttype == NPCControl.Interaction.VenusHeal)
		{
			text = MainManager.commondialogue[60];
		}
		else if (this.interacttype == NPCControl.Interaction.SavePoint)
		{
			text = "|boxstyle,4||bleep,2,1,1|" + MainManager.menutext[4] + "|prompt,menu,0.7,2,7,78,5,6|";
		}
		else if (this.interacttype == NPCControl.Interaction.ShopKeeper || this.interacttype == NPCControl.Interaction.Shop)
		{
			if (args == "buy")
			{
				text = MainManager.GetDialogueText((int)this.shopkeeper.dialogues[6].y);
				transform = this.shopkeeper.transform;
			}
			else
			{
				text = MainManager.GetDialogueText((int)this.dialogues[0].y);
			}
		}
		MainManager.instance.StartCoroutine(MainManager.SetText(text, 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, transform, this));
		for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
		{
			MainManager.instance.playerdata[j].entity.FaceTowards(transform.position, false, true);
		}
	}

	// Token: 0x060006CB RID: 1739 RVA: 0x00055670 File Offset: 0x00053870
	private void WarpRock()
	{
		if (this.data.Length > 2 && this.data[2] == 1)
		{
			this.entity.LockRigid(true);
			this.actioncooldown = this.vectordata[1].z;
			return;
		}
		base.transform.position = new Vector3(0f, 999f);
		base.StartCoroutine(MainManager.LatePos(base.transform, this.entity.startpos.Value, this.vectordata[1].z, false));
		this.hit = false;
		this.entity.rigid.velocity = Vector3.zero;
		this.entity.onground = false;
	}

	// Token: 0x060006CC RID: 1740 RVA: 0x00055730 File Offset: 0x00053930
	public void BreakRock()
	{
		if (MainManager.GetDistance(MainManager.player.transform.position, base.transform.position) < 15f && MainManager.SoundIsPlaying("RockBreak") == -1)
		{
			MainManager.PlaySound("RockBreak", -1, 1f, 0.5f);
		}
		if (this.objecttype != NPCControl.ObjectTypes.RollingRock)
		{
			if (!this.hit)
			{
				(Object.Instantiate(Resources.Load("Prefabs/Objects/CrackRockBreak"), base.transform.position, Quaternion.identity) as GameObject).GetComponent<CrackRockBreak>().initialcolor = this.entity.model.GetComponent<Renderer>().material.color;
				this.ActivateRegion();
				this.hit = true;
				this.boxcol.enabled = false;
				MainManager.ShakeScreen(Vector3.one * 0.3f, 0.5f);
				base.StartCoroutine(this.entity.Death(true));
				MainManager.player.boulderbreak = 5f;
			}
			return;
		}
		this.entity.sound.Stop();
		if (MainManager.GetDistance(MainManager.player.transform.position, base.transform.position) < 15f)
		{
			MainManager.ShakeScreen(0.1f, 0.35f, true);
		}
		Transform transform = (Object.Instantiate(Resources.Load("Prefabs/Objects/CrackRockBreak"), base.transform.position, Quaternion.identity) as GameObject).transform;
		transform.GetComponent<CrackRockBreak>().initialcolor = this.entity.model.GetComponent<Renderer>().material.color;
		transform.localScale = Vector3.one * (this.vectordata[1].y / 2f);
		this.entity.rigid.velocity = Vector3.zero;
		this.entity.onground = false;
		if (this.data.Length > 2 && this.data[2] == 1)
		{
			base.transform.position = new Vector3(0f, 999f);
			this.entity.LockRigid(true);
			this.actioncooldown = this.vectordata[1].z;
			return;
		}
		this.WarpRock();
	}

	// Token: 0x060006CD RID: 1741 RVA: 0x00055970 File Offset: 0x00053B70
	private void OnTriggerEnter(Collider other)
	{
		if (this.entitytype == NPCControl.NPCType.Object)
		{
			NPCControl.ObjectTypes objectTypes;
			if (MainManager.player != null && other.transform == MainManager.player.transform)
			{
				objectTypes = this.objecttype;
				if (objectTypes <= NPCControl.ObjectTypes.PathPlatform)
				{
					if (objectTypes != NPCControl.ObjectTypes.Item)
					{
						if (objectTypes == NPCControl.ObjectTypes.PathPlatform)
						{
							if (MainManager.map.areaid == MainManager.Areas.WildGrasslands && this.entity.originalid + 1 == 243 && !MainManager.player.entity.onground)
							{
							}
						}
					}
					else if (!MainManager.instance.pause && !MainManager.instance.minipause && (this.timer == -1f || this.timer > 1f) && this.insideid == MainManager.instance.insideid)
					{
						base.StartCoroutine(this.CheckItem());
						this.collisionammount++;
					}
				}
				else if (objectTypes != NPCControl.ObjectTypes.ResetCamera)
				{
					if (objectTypes == NPCControl.ObjectTypes.WindPusher)
					{
						Physics.IgnoreCollision(MainManager.player.entity.detect, this.boxcol, true);
						MainManager.player.entity.hitwall = false;
					}
				}
				else
				{
					MainManager.ResetCamera();
				}
			}
			else
			{
				this.collisionammount++;
			}
			objectTypes = this.objecttype;
			if (objectTypes <= NPCControl.ObjectTypes.SavePoint)
			{
				if (objectTypes <= NPCControl.ObjectTypes.PushRock)
				{
					if (objectTypes != NPCControl.ObjectTypes.BeetleGrass)
					{
						if (objectTypes == NPCControl.ObjectTypes.PushRock)
						{
							if (other.CompareTag("BeetleHorn") && !this.trapped)
							{
								this.entity.rigid.useGravity = true;
								this.entity.rigid.isKinematic = false;
								if (!this.entity.sound.isPlaying || this.entity.sound.clip.name != "Damage0" || this.entity.sound.clip.name == "Thud" || this.entity.sound.time > 0.5f)
								{
									this.entity.PlaySound("Damage0", 0.6f, 0.5f);
								}
								MainManager.HitPart(base.transform.position + Vector3.up / 2f);
								this.rotater.LookAt(MainManager.player.transform.position);
								if (this.data.Length > 2 && (this.entity.onground || (this.data.Length > 3 && this.data[3] == 1)))
								{
									this.internaldata[0] = 0f;
									this.internalvector[1] = Vector3.one * 999f;
									switch (this.data[2])
									{
									case 0:
										this.entity.rigid.velocity = new Vector3(-this.rotater.forward.x * this.vectordata[0].z, this.vectordata[0].y, -this.rotater.forward.z * this.vectordata[0].z);
										this.icevel = this.entity.rigid.velocity;
										this.entity.onground = false;
										this.actioncooldown = 5f;
										this.freezecooldown = 150f;
										this.entity.feet.overridecd = 5f;
										break;
									case 1:
										this.hit = true;
										break;
									case 2:
										this.hit = true;
										break;
									case 3:
										this.internalvector[0] = MainManager.CardinalSnap(this.rotater.eulerAngles);
										this.hit = true;
										break;
									}
								}
								else
								{
									this.entity.rigid.velocity = new Vector3(-this.rotater.forward.x * this.vectordata[0].z, this.vectordata[0].y, -this.rotater.forward.z * this.vectordata[0].z);
									this.icevel = this.entity.rigid.velocity;
									this.entity.onground = false;
									this.actioncooldown = 5f;
									this.freezecooldown = 150f;
									this.entity.feet.overridecd = 5f;
								}
							}
							else if (other.CompareTag("RockLimit") && this.data[2] == 3)
							{
								this.BreakIceRock();
							}
						}
					}
					else if ((other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash")) && !this.hit && Vector3.Distance(base.transform.position, other.transform.position) < 3.5f)
					{
						this.CutGrass();
						MainManager.instance.RefreshPlayer(false);
					}
				}
				else if (objectTypes != NPCControl.ObjectTypes.Item)
				{
					if (objectTypes == NPCControl.ObjectTypes.SavePoint)
					{
						if (!MainManager.instance.minipause && !MainManager.instance.message && !MainManager.instance.pause && MainManager.player != null && (other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash") || other.CompareTag("Icecle") || (MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)))
						{
							if (other.CompareTag("Icecle") && other.GetComponent<DestroyOnLayer>() != null)
							{
								other.GetComponent<DestroyOnLayer>().Kill();
							}
							this.entity.anim.Play("BounceUp");
							this.entity.PlaySound("Save", 0.5f);
							if (this.data[2] == 0)
							{
								MainManager.Heal();
							}
							MainManager.HitPart(base.transform.position + Vector3.up / 2f);
							if (MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)
							{
								MainManager.player.beemerang.hit = true;
							}
							if (this.data[1] >= 10)
							{
								if (!this.hit)
								{
									DeadLanderOmega.GetOmega(this.data[1] - 10).ForceLook(this.vectordata[0]);
									this.hit = true;
								}
							}
							else if (!MainManager.timeddemo && MainManager.GetSqrDistance(base.transform.position, MainManager.player.transform.position) <= 30f)
							{
								this.Interact("save");
							}
						}
					}
				}
				else if (other.CompareTag("BeeRang") && (this.data.Length < 3 || this.data[2] == 0) && this.beerang == null && !MainManager.instance.minipause && !MainManager.instance.pause && !MainManager.instance.message && MainManager.player != null && MainManager.player.beemerang != null && !MainManager.player.beemerang.hit && MainManager.instance.insideid == this.insideid)
				{
					this.beerang = other.transform;
				}
			}
			else
			{
				if (objectTypes <= NPCControl.ObjectTypes.CoiledObject)
				{
					if (objectTypes != NPCControl.ObjectTypes.Switch)
					{
						if (objectTypes != NPCControl.ObjectTypes.CoiledObject)
						{
							goto IL_158A;
						}
						if (!MainManager.instance.minipause && !MainManager.instance.message && !MainManager.instance.pause && MainManager.player != null && (other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash") || other.CompareTag("Icecle") || (MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)) && this.moveobj != null && !this.hit)
						{
							this.entity.PlaySound("Coiled");
							EntityControl component = this.moveobj.GetComponent<EntityControl>();
							MainManager.HitPart(base.transform.position + Vector3.up / 2f);
							component.LockRigid(false);
							component.rigid.velocity = Vector3.zero;
							component.npcdata.trapped = false;
							component.onground = false;
							component.transform.parent = MainManager.map.transform;
							component.transform.localScale = Vector3.one;
							if (this.data[1] > -1)
							{
								MainManager.instance.flags[this.data[1]] = true;
							}
							if (this.regionalflag > -1)
							{
								MainManager.instance.regionalflags[this.regionalflag] = true;
							}
							if (MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)
							{
								MainManager.player.beemerang.hit = true;
								MainManager.PlaySound("WoodHit");
							}
							this.hit = true;
							goto IL_158A;
						}
						goto IL_158A;
					}
				}
				else
				{
					switch (objectTypes)
					{
					case NPCControl.ObjectTypes.Dropplet:
						if (other.CompareTag("Icecle") || other.CompareTag("IceRadius"))
						{
							if (this.hit)
							{
								MainManager.PlayParticle("IceShatter", this.internaltransform[0].position);
								AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/Sounds/IceBreak"), this.internaltransform[0].position, MainManager.GetSoundDistance(this.internaltransform[0].position) * 1.5f * MainManager.soundvolume);
								this.internaltransform[0].GetComponent<Hornable>().ServerGeizer();
							}
							this.hit = true;
							this.actionfrequency[0] = -1100f;
							this.actionfrequency[1] = 600f;
							this.internaltransform[0].position = other.transform.position + new Vector3(0f, 0.25f);
							this.internaltransform[0].parent = MainManager.map.transform;
							this.internaltransform[0].localScale = Vector3.zero;
							this.internaltransform[0].GetComponent<Hornable>().ingeizer = null;
							DialogueAnim component2 = this.internaltransform[0].GetComponent<DialogueAnim>();
							component2.targetscale = Vector3.one * 1.5f;
							component2.shrinkspeed = 0.1f;
							component2.enabled = true;
							Rigidbody component3 = this.internaltransform[0].GetComponent<Rigidbody>();
							component3.isKinematic = false;
							component3.velocity = Vector3.zero;
							if (Vector3.Distance(this.internaltransform[0].transform.position, other.transform.position) > 5f)
							{
								this.internaltransform[0].position = other.transform.position + new Vector3(0f, 0.25f);
							}
							if (this.collisionammount < 2)
							{
								MainManager.PlaySound("Freeze", 0.6f);
							}
							this.collisionammount++;
							if (this.data[2] == 1)
							{
								if (this.data.Length > 3 && this.data[3] > 0)
								{
									this.actioncooldown = (float)this.data[3];
								}
								this.entity.rigid.isKinematic = true;
								base.StartCoroutine(MainManager.DelayedPosition(base.transform, new Vector3(0f, -2000f), 0.1f, false));
							}
							else
							{
								base.transform.position = new Vector3(0f, -2000f);
							}
							this.actionfrequency[2] = (float)this.data[0];
							goto IL_158A;
						}
						goto IL_158A;
					case NPCControl.ObjectTypes.PathPlatform:
					case NPCControl.ObjectTypes.RotatingPlatform:
					case NPCControl.ObjectTypes.MusicRange:
					case NPCControl.ObjectTypes.ResetCamera:
						goto IL_158A;
					case NPCControl.ObjectTypes.BreakableRock:
						if (this.collisionammount <= 1)
						{
							if (other.CompareTag("BeetleDash"))
							{
								this.BreakRock();
							}
							else if (other.CompareTag("BeetleHorn"))
							{
								base.StartCoroutine(this.entity.ShakeSprite(new Vector3(0.1f, 0.05f), 10f));
								this.entity.PlaySound("Rock2", 1f, Random.Range(0.9f, 1.1f));
							}
							this.collisionammount = 2;
							goto IL_158A;
						}
						goto IL_158A;
					case NPCControl.ObjectTypes.Geizer:
						if (other.CompareTag("Icecle") || (other.CompareTag("IceRadius") && (this.data.Length < 5 || this.data[4] == 0)) || other.CompareTag("Icefall"))
						{
							if (this.moveobj != null)
							{
								MainManager.LaunchObject(this.moveobj, new Vector3(0f, 15f, 0f));
								this.moveobj.transform.parent = MainManager.map.transform;
								this.moveobj.GetComponent<Hornable>().ServerGeizer();
							}
							if (this.actioncooldown <= 0f)
							{
								if (this.boxcol != null)
								{
									this.boxcol.enabled = false;
								}
								this.internaltransform[0].gameObject.SetActive(false);
								this.internaltransform[1].gameObject.SetActive(true);
								this.internaltransform[3].gameObject.SetActive(false);
								this.internaltransform[4].gameObject.SetActive(MainManager.map.mapid != MainManager.Maps.UpperSnekGeizerRoom);
								this.entity.sound.Stop();
								this.entity.sound.time = 0f;
								MainManager.PlaySound("Freeze", 0.5f);
							}
							this.actioncooldown = this.vectordata[0].z * (float)(MainManager.BadgeIsEquipped(59) ? 3 : 1);
							goto IL_158A;
						}
						if ((other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash")) && this.actioncooldown > 0f)
						{
							MainManager.player.entity.hitwall = true;
							this.actioncooldown = 0f;
							this.GeizerBreak();
							if (this.boxcol != null)
							{
								this.boxcol.enabled = true;
								goto IL_158A;
							}
							goto IL_158A;
						}
						else
						{
							if (other.CompareTag("DroppletCube") && this.actioncooldown <= 0f)
							{
								if (this.moveobj != null)
								{
									MainManager.LaunchObject(this.moveobj, MainManager.RandomVector(5f, 10f));
									this.moveobj.GetComponent<Hornable>().ServerGeizer();
								}
								Rigidbody component4 = other.transform.parent.GetComponent<Rigidbody>();
								component4.velocity = Vector3.zero;
								component4.useGravity = false;
								component4.isKinematic = true;
								other.transform.parent.parent = this.internaltransform[0];
								this.moveobj = other.transform.parent;
								this.moveobj.GetComponent<Hornable>().ingeizer = this;
								goto IL_158A;
							}
							if (other.CompareTag("PFollower") || (MainManager.player != null && other.transform == MainManager.player.transform))
							{
								other.transform.parent = null;
								goto IL_158A;
							}
							goto IL_158A;
						}
						break;
					case NPCControl.ObjectTypes.TempPlatform:
						if (MainManager.player != null && MainManager.player.entity.feet.transform == other.transform)
						{
							if (this.data[3] == 1)
							{
								this.entity.anim.Play("Shaking");
							}
							this.hit = true;
							goto IL_158A;
						}
						goto IL_158A;
					case NPCControl.ObjectTypes.ScrewSwitch:
						if (other.CompareTag("BeetleHorn"))
						{
							this.hit = true;
							this.actioncooldown = Mathf.Clamp(this.actioncooldown + this.vectordata[0].x * 10f, 0f, this.vectordata[0].z);
							goto IL_158A;
						}
						goto IL_158A;
					case NPCControl.ObjectTypes.StencilSwitch:
						break;
					default:
						if (objectTypes != NPCControl.ObjectTypes.WaterSwitch)
						{
							goto IL_158A;
						}
						break;
					}
				}
				if (!MainManager.instance.minipause && !MainManager.instance.message && !MainManager.instance.pause && MainManager.player != null && this.collisionammount <= 1 && (other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash") || other.CompareTag("Icefall") || other.CompareTag("Icecle") || (MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)) && (this.objecttype == NPCControl.ObjectTypes.StencilSwitch || this.data.Length < 5 || this.data[4] == 0 || (this.data[4] == 1 && (other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash")))))
				{
					this.collisionammount++;
					MainManager.HitPart(base.transform.position + Vector3.up / 2f);
					if (this.objecttype == NPCControl.ObjectTypes.WaterSwitch)
					{
						this.hit = !this.hit;
						MainManager.PlaySound(this.hit ? "WaterFill" : "WaterFill2");
						if (this.activationflag > -1)
						{
							MainManager.instance.flags[this.activationflag] = this.hit;
						}
						this.actioncooldown = 30f;
					}
					else if (this.objecttype == NPCControl.ObjectTypes.StencilSwitch)
					{
						Physics.IgnoreCollision(this.entity.ccol, this.internaltransform[0].GetComponent<SphereCollider>(), true);
						MainManager.PlaySoundAt(this.hit ? "IceMelt" : "Freeze", 1f, base.transform.position);
						this.hit = !this.hit;
						this.actioncooldown = 30f;
						if (this.hit)
						{
							this.ActivateRegion();
						}
						this.DeactivateOtherStencil();
					}
					else
					{
						bool flag = base.name.Contains("TOG");
						if (this.data[0] == 1 && (flag || !this.hit))
						{
							if (this.data[1] > -1)
							{
								MainManager.events.StartEvent(this.data[1], this);
							}
							else
							{
								this.ActivateRegion();
							}
							this.hit = (!flag || !this.hit);
							if (this.entity.originalid == -1)
							{
								this.entity.iskill = true;
							}
							if (this.entity.originalid + 1 == 106)
							{
								this.moveobj.transform.localEulerAngles = new Vector3(0f, -60f);
							}
						}
						else if (this.data[0] == 0)
						{
							if (this.data[1] == 1)
							{
								if (this.actioncooldown <= 0f)
								{
									this.hit = !this.hit;
									this.actioncooldown = 30f;
									if (this.activationflag > -1)
									{
										MainManager.instance.flags[this.activationflag] = this.hit;
									}
									for (int i = 0; i < MainManager.map.entities.Length; i++)
									{
										if (MainManager.map.entities[i] != null && MainManager.map.entities[i].npcdata != null && MainManager.map.entities[i].npcdata != this && MainManager.map.entities[i].npcdata.entitytype == NPCControl.NPCType.Object && MainManager.map.entities[i].npcdata.objecttype == NPCControl.ObjectTypes.Switch && MainManager.map.entities[i].npcdata.data[0] == 0 && MainManager.map.entities[i].npcdata.data[1] == 1 && this.activationflag > -1 && MainManager.map.entities[i].npcdata.activationflag == this.activationflag)
										{
											MainManager.map.entities[i].npcdata.hit = this.hit;
										}
									}
								}
							}
							else
							{
								this.hit = true;
								this.ActivateRegion();
							}
						}
						if (this.data.Length > 2 && this.data[2] > -1)
						{
							this.actioncooldown = (float)this.data[2];
						}
					}
					if (MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)
					{
						MainManager.player.beemerang.hit = true;
						this.entity.PlaySound("WoodHit");
					}
					if (this.entity.originalid > -1)
					{
						this.SwitchSound(true);
					}
				}
			}
			IL_158A:
			if (other.gameObject.layer == 8 || other.gameObject.layer == 13 || (this.objecttype == NPCControl.ObjectTypes.Beemerang && other.gameObject.layer == 19))
			{
				objectTypes = this.objecttype;
				if (objectTypes == NPCControl.ObjectTypes.Beemerang)
				{
					if (!this.hit)
					{
						MainManager.PlaySound("WoodHit", -1, 1.2f, 0.5f);
						MainManager.HitPart(base.transform.position + Vector3.up);
					}
					this.hit = true;
					this.timer = -3f;
					return;
				}
				if (objectTypes != NPCControl.ObjectTypes.Dropplet)
				{
					return;
				}
				if (!other.CompareTag("DroppletPass") && this.actionfrequency[1] < 595f)
				{
					this.actionfrequency[1] = 600f;
					if (!this.entity.rigid.isKinematic)
					{
						if (this.internalparticle[0] == null)
						{
							this.internalparticle[0] = MainManager.PlayParticle("WaterSplash", null, base.transform.position, new Vector3(-90f, 0f), -1f).GetComponent<ParticleSystem>();
							this.internalparticle[0].transform.parent = MainManager.map.transform;
						}
						else
						{
							this.internalparticle[0].transform.position = base.transform.position;
							this.internalparticle[0].Play();
						}
					}
					if (!MainManager.instance.pause && this.startlife > 60f)
					{
						AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/Sounds/WaterSplash"), base.transform.position, this.entity.GetSoundDistance() * MainManager.soundvolume * ((this.vectordata[1].z == 0f) ? 1f : this.vectordata[1].z));
					}
					this.entity.rigid.isKinematic = true;
					this.entity.sprite.gameObject.SetActive(false);
					base.Invoke("HidePos", 0.16666667f);
					this.actionfrequency[0] = -1110f;
					this.actionfrequency[2] = (float)this.data[0];
					return;
				}
			}
		}
		else if (this.entitytype == NPCControl.NPCType.Enemy)
		{
			if (other.CompareTag("Icecle"))
			{
				if (!this.entity.digging && this.entity.animid != 400 && this.entity.animid != 399 && this.entity.animid != 401 && this.entity.animid != 327)
				{
					if (other.CompareTag("Icecle") && other.GetComponent<Rigidbody>() != null)
					{
						Object.Destroy(other.gameObject);
					}
					if (MainManager.map.mapid == MainManager.Maps.GiantLairDeadLands1 || MainManager.map.mapid == MainManager.Maps.GiantLairDeadLands2)
					{
						this.freezecooldown = 30f;
					}
					else if (MainManager.BadgeIsEquipped(59) || this.extrafreeze)
					{
						this.freezecooldown = Mathf.Clamp((this.freezetime > 10f) ? this.freezetime : 600f, 600f, float.PositiveInfinity);
					}
					else
					{
						this.freezecooldown = 300f;
					}
					if (this.entity.originalid + 1 == 308 && this.internaltransform != null && this.internaltransform.Length != 0 && this.internaltransform[0] != null)
					{
						Object.Destroy(this.internaltransform[0].gameObject);
					}
					this.entity.rigid.velocity = new Vector3(0f, this.entity.rigid.velocity.y, 0f);
					this.entity.onground = true;
					this.StopForceBehavior();
					return;
				}
			}
			else if ((other.CompareTag("BeetleHorn") && !MainManager.player.dashing) || other.CompareTag("BeetleDash"))
			{
				if (this.collisionammount == 0 && !this.entity.dead && !this.entity.digging && this.touchcooldown <= 0f)
				{
					base.StartCoroutine(this.entity.TempIgnoreColision(other, 0.5f));
					this.entity.PlaySound("Damage0");
					if (other.CompareTag("BeetleDash"))
					{
						this.freezecooldown = 0f;
					}
					float time = 120f;
					Vector3 position = base.transform.position;
					Vector3 position2 = MainManager.player.transform.position;
					this.GetDizzy(time, MainManager.GetDirection(position, position2, true).normalized * 5f, true);
					this.collisionammount++;
					return;
				}
			}
			else if (other.CompareTag("BeeRang"))
			{
				if (this.collisionammount == 0 && !this.entity.dead && !this.entity.digging && this.touchcooldown <= 0f)
				{
					this.entity.PlaySound("WoodHit");
					this.GetDizzy(80f, Vector3.zero);
					this.collisionammount++;
					return;
				}
			}
			else if (MainManager.player != null && other.transform == MainManager.player.transform && !MainManager.instance.minipause && !MainManager.instance.pause && !MainManager.instance.inevent)
			{
				this.StartBattle();
				return;
			}
		}
		else if (this.entitytype == NPCControl.NPCType.NPC)
		{
			if (this.specialinteract != NPCControl.HitInteract.None)
			{
				NPCControl.HitInteract hitInteract = this.specialinteract;
				if (hitInteract != NPCControl.HitInteract.HornDash)
				{
					if (hitInteract == NPCControl.HitInteract.AnyHorn)
					{
						if (other.CompareTag("BeetleDash") || other.CompareTag("BeetleHorn"))
						{
							this.interactedwithhit = true;
							this.Interact(null);
							base.Invoke("SInter", 1f);
						}
					}
				}
				else if (other.CompareTag("BeetleDash"))
				{
					this.interactedwithhit = true;
					this.Interact(null);
					base.Invoke("SInter", 1f);
				}
			}
			if (MainManager.player != null && MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform && this.HasBehavior(NPCControl.ActionBehaviors.StealthAI) && MainManager.GetSqrDistance(base.transform.position, MainManager.player.transform.position) < 30f)
			{
				base.StartCoroutine(this.StealthSpot());
			}
		}
	}

	// Token: 0x060006CE RID: 1742 RVA: 0x000575C1 File Offset: 0x000557C1
	private void HidePos()
	{
		base.transform.position = new Vector3(0f, -2000f, 0f);
		this.entity.sprite.gameObject.SetActive(true);
	}

	// Token: 0x060006CF RID: 1743 RVA: 0x000575F8 File Offset: 0x000557F8
	private void SInter()
	{
		this.interactedwithhit = false;
	}

	// Token: 0x060006D0 RID: 1744 RVA: 0x00057604 File Offset: 0x00055804
	private void DeactivateOtherStencil()
	{
		for (int i = 0; i < MainManager.map.entities.Length; i++)
		{
			if (i != this.mapid && !MainManager.map.entities[i].iskill && MainManager.map.entities[i].npcdata.entitytype == NPCControl.NPCType.Object && MainManager.map.entities[i].npcdata.objecttype == NPCControl.ObjectTypes.StencilSwitch)
			{
				MainManager.map.entities[i].npcdata.hit = false;
			}
		}
	}

	// Token: 0x060006D1 RID: 1745 RVA: 0x00057690 File Offset: 0x00055890
	private void GeizerBreak()
	{
		MainManager.PlaySoundAt("IceBreak", 0.75f, base.transform.position + Vector3.up);
		MainManager.PlayParticle("IceShatter", new Vector3(base.transform.position.x, -this.entity.startpos.Value.y - 0.45f, base.transform.position.z - 0.5f)).transform.localScale = new Vector3(2f, 3f, 1f);
		Transform[] componentsInChildren = base.GetComponentsInChildren<Transform>();
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (MainManager.instance.playerdata[i].entity.transform.parent == componentsInChildren[j])
				{
					MainManager.instance.playerdata[i].entity.transform.parent = null;
				}
			}
		}
	}

	// Token: 0x060006D2 RID: 1746 RVA: 0x000577A8 File Offset: 0x000559A8
	public void STOP()
	{
		if (!this.entity.dead)
		{
			if (this.behaviorroutine != null)
			{
				base.StopCoroutine(this.behaviorroutine);
			}
			this.forcebehavior = null;
			this.behaviorroutine = null;
			this.StopForceBehavior();
			this.entity.StopForceMove();
			this.entity.overrideflip = false;
			this.attacking = false;
			this.entity.overrideonlyflip = false;
		}
	}

	// Token: 0x060006D3 RID: 1747 RVA: 0x00057819 File Offset: 0x00055A19
	private void GetDizzy(float time, Vector3 pushforce, bool forcelaunch)
	{
		base.StartCoroutine(this.Dizzy(time, pushforce, forcelaunch));
	}

	// Token: 0x060006D4 RID: 1748 RVA: 0x0005782B File Offset: 0x00055A2B
	private void GetDizzy(float time, Vector3 pushforce)
	{
		base.StartCoroutine(this.Dizzy(time, pushforce, false));
	}

	// Token: 0x060006D5 RID: 1749 RVA: 0x0005783D File Offset: 0x00055A3D
	private IEnumerator Dizzy(float time, Vector3 pushforce, bool forcelaunch)
	{
		if (!this.entity.dead && this.entity.deathcoroutine == null && !this.entity.iskill)
		{
			this.entity.LockRigid(false);
			if (this.disguiseobj != null)
			{
				this.disguiseobj.gameObject.SetActive(false);
				this.entity.sprite.enabled = true;
			}
			MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
			if (animIDs == MainManager.AnimIDs.ToeBiter && this.internaltransform != null && this.internaltransform.Length != 0 && this.internaltransform[0] != null)
			{
				this.internaltransform[0].gameObject.AddComponent<Rigidbody>().velocity = MainManager.RandomItemBounce(3f, 12f);
				Object.Destroy(this.internaltransform[0].gameObject, 3f);
				this.internaltransform = null;
			}
			bool launch = false;
			this.touchcooldown = 30f;
			this.entity.sprite.transform.localEulerAngles = Vector3.zero;
			this.entity.digging = false;
			this.dizzytime = time;
			this.STOP();
			this.entity.onground = false;
			yield return null;
			if (!this.HasBehavior(NPCControl.ActionBehaviors.Unmoveable))
			{
				this.ignoreconstraint = true;
				this.entity.rigid.useGravity = true;
				this.entity.rigid.isKinematic = false;
				if (this.entity.detect == null)
				{
					this.entity.CreateDetector();
				}
				this.entity.detect.transform.LookAt(MainManager.player.transform.position);
				this.entity.detect.transform.localEulerAngles = new Vector3(this.entity.detect.transform.localEulerAngles.x, this.entity.detect.transform.localEulerAngles.y * -1f, this.entity.detect.transform.localEulerAngles.z);
				this.entity.hitwall = false;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
				pushforce = new Vector3(pushforce.x, (launch || forcelaunch) ? 13.5f : 0f, pushforce.z);
				this.entity.rigid.velocity = pushforce;
				this.icevel = this.entity.rigid.velocity;
				launch = true;
				if (new Vector2(pushforce.x, pushforce.z).magnitude > 0.1f)
				{
					this.entity.StartCoroutine(this.entity.TempIgnoreColision(MainManager.player.entity.ccol, 0.5f));
					this.entity.StartCoroutine(this.entity.TempIgnoreColision(MainManager.player.entity.detect, 0.5f));
				}
			}
			MainManager.HitPart(this.entity.sprite.transform.position + Vector3.up / 2f);
			yield return null;
			if (!this.HasBehavior(NPCControl.ActionBehaviors.Unmoveable) && (launch || forcelaunch) && this.entity.rigid.velocity.y < 5f)
			{
				while (this.entity.rigid.velocity.y < 5f)
				{
					this.entity.Jump(13.5f);
					yield return null;
				}
			}
		}
		yield break;
	}

	// Token: 0x060006D6 RID: 1750 RVA: 0x00057864 File Offset: 0x00055A64
	private void SwitchSound(bool press)
	{
		if (this.startlife >= 15f)
		{
			this.entity.PlaySound("Button", 1f, press ? 1f : 0.5f);
			MainManager.AnimIDs animIDs = this.entity.originalid + MainManager.AnimIDs.Bee;
			if (animIDs == MainManager.AnimIDs.SwitchCrystal || animIDs == MainManager.AnimIDs.AncientPressurePlate || animIDs == MainManager.AnimIDs.BigCrystalSwitch)
			{
				MainManager.PlaySound("Glow", -1, press ? 1f : 0.5f, this.entity.GetSoundDistance());
			}
		}
	}

	// Token: 0x060006D7 RID: 1751 RVA: 0x000578E5 File Offset: 0x00055AE5
	public void ActivateRegion()
	{
		if (this.regionalflag > -1)
		{
			MainManager.instance.regionalflags[this.regionalflag] = true;
		}
		if (this.activationflag > 0)
		{
			MainManager.instance.flags[this.activationflag] = true;
		}
	}

	// Token: 0x060006D8 RID: 1752 RVA: 0x00057920 File Offset: 0x00055B20
	private void RefreshPlayer(bool ins)
	{
		if (this.inrange != ins)
		{
			this.actioncooldown = 0f;
			if (this.entitytype == NPCControl.NPCType.Enemy)
			{
				if (!ins)
				{
					if (this.freezecooldown <= 0f && this.dizzytime < 0f && this.behaviorroutine == null)
					{
						this.entity.emoticonid = 1;
						this.entity.emoticoncooldown = 70f;
						this.entity.StopForceMove(0, false);
						this.entity.PlaySound("Lost");
						this.entity.overrideonlyflip = false;
					}
				}
				else
				{
					if (this.freezecooldown > 0f || this.dizzytime >= 0f)
					{
						return;
					}
					if (!MainManager.player.digging)
					{
						this.entity.PlaySound("Find");
					}
				}
			}
		}
		this.inrange = ins;
		if (this.entitytype != NPCControl.NPCType.Object && this.inrange && this.HasBehavior(NPCControl.ActionBehaviors.StealthAI))
		{
			base.StartCoroutine(this.StealthSpot());
		}
	}

	// Token: 0x060006D9 RID: 1753 RVA: 0x00057A2D File Offset: 0x00055C2D
	private int GetItemTimer(int type)
	{
		if (type == 0)
		{
			return 300;
		}
		return -1;
	}

	// Token: 0x060006DA RID: 1754 RVA: 0x00057A3C File Offset: 0x00055C3C
	private void OnTriggerStay(Collider other)
	{
		if (this.entitytype == NPCControl.NPCType.Object)
		{
			NPCControl.ObjectTypes objectTypes = this.objecttype;
			switch (objectTypes)
			{
			case NPCControl.ObjectTypes.PressurePlate:
			{
				Hornable component = other.GetComponent<Hornable>();
				if ((MainManager.player.beemerang == null || MainManager.player.beemerang.transform != other.transform) && ((this.activationflag > -1 && !MainManager.instance.flags[this.activationflag]) || this.activationflag == -1) && ((this.data[0] == 1 && MainManager.player != null && other.transform == MainManager.player.transform) || (this.data[1] == 1 && other.GetComponent<NPCControl>() != null && other.GetComponent<EntityControl>().icecube != null) || component != null || other.CompareTag("PushRock")))
				{
					if (!this.hit)
					{
						this.SwitchSound(true);
					}
					if (component != null)
					{
						component.onground = true;
					}
					if (this.data.Length > 2 && this.data[2] > -1)
					{
						MainManager.events.StartEvent(this.data[2], this);
						this.data[2] = -1;
					}
					this.hit = true;
					this.actioncooldown = 10f;
					return;
				}
				return;
			}
			case NPCControl.ObjectTypes.ANDGate:
			case NPCControl.ObjectTypes.Item:
			case NPCControl.ObjectTypes.Beemerang:
			case NPCControl.ObjectTypes.ANDBlock:
			case NPCControl.ObjectTypes.SavePoint:
				return;
			case NPCControl.ObjectTypes.CameraChange:
				if (!(MainManager.player != null) || !(other.transform == MainManager.player.transform) || (!MainManager.FreePlayer(false) && !MainManager.player.digging))
				{
					return;
				}
				if (this.data[0] == 1)
				{
					MainManager.instance.camoffset = this.vectordata[0];
				}
				if (this.data[1] == 1)
				{
					if (this.vectordata[1].magnitude > 0.1f)
					{
						MainManager.map.camlimitpos = this.vectordata[1];
					}
					else
					{
						MainManager.map.RestoreLimit(false);
					}
					if (this.vectordata[2].magnitude > 0.1f)
					{
						MainManager.map.camlimitneg = this.vectordata[2];
					}
					else
					{
						MainManager.map.RestoreLimit(false);
					}
				}
				if (this.data[2] == 1)
				{
					MainManager.instance.changecamspeed = true;
					MainManager.instance.camspeed = this.vectordata[3].x;
				}
				if (this.data[3] == 1)
				{
					MainManager.instance.camangleoffset = this.vectordata[4];
				}
				if (this.data[4] == 1)
				{
					MainManager.instance.camtarget = MainManager.GetEntity(this.data[5]).transform;
				}
				if (this.data[6] == 1)
				{
					MainManager.instance.camtargetpos = new Vector3?(this.vectordata[5]);
				}
				if (this.data.Length > 7 && this.data[7] == 1)
				{
					MainManager.instance.camanglespeed = this.vectordata[3].y;
					MainManager.instance.camanglechange = true;
					return;
				}
				return;
			case NPCControl.ObjectTypes.DoorOtherMap:
			case NPCControl.ObjectTypes.DoorSameMap:
			case NPCControl.ObjectTypes.EventTrigger:
			case NPCControl.ObjectTypes.DialogueTrigger:
				break;
			case NPCControl.ObjectTypes.SetPlayerRespawn:
				if (this.vectordata[0].magnitude > 0.1f && MainManager.player != null && other.transform == MainManager.player.transform)
				{
					MainManager.player.lastpos = this.vectordata[0];
					return;
				}
				return;
			case NPCControl.ObjectTypes.JumpSpring:
			{
				if ((!(MainManager.player == null) && !(other.transform != MainManager.player.transform) && !MainManager.FreePlayer(false)) || MainManager.instance.pause || (MainManager.map.mapid == MainManager.Maps.RubberPrisonGym && other.transform.position.y <= this.entity.transform.position.y))
				{
					return;
				}
				this.entity.overrideflip = true;
				EntityControl component2 = other.GetComponent<EntityControl>();
				if (component2 != null && !component2.iskill && (this.data[1] == 0 || (this.data[1] == 1 && !component2.onground && component2.rigid.velocity.y < 0f)) && (component2.npcdata == null || component2.npcdata.entitytype != NPCControl.NPCType.Object || component2.item))
				{
					if (other.transform == MainManager.player.entity.transform && MainManager.GetDistance(base.transform.position, MainManager.player.transform.position) < 10f)
					{
						this.entity.PlaySound("Boing0", 0.75f);
					}
					if (this.data[0] == 1 && other.transform == MainManager.player.entity.transform)
					{
						if (this.collisionammount < 100)
						{
							MainManager.player.transform.position = base.transform.position + Vector3.up * 0.75f;
							MainManager.player.StartCoroutine(MainManager.player.JumpTo(this.vectordata[1], this.vectordata[0].x, (this.vectordata.Length > 2) ? Mathf.Clamp(this.vectordata[2].x, 1f, 99f) : 1f));
							if (this.data[2] > 0)
							{
								NPCControl npcdata = MainManager.GetEntity(this.data[2]).npcdata;
								MainManager.map.StartCoroutine(MainManager.map.MoveInside(npcdata, false));
							}
							this.collisionammount = 110;
						}
					}
					else
					{
						component2.springcooldown = true;
						component2.jumpcooldown = 30f;
						bool flag = false;
						if (other.transform == MainManager.player.entity.transform)
						{
							MainManager.player.CancelAction(true);
							if (MainManager.instance.itempicked)
							{
								MainManager.player.entity.onground = true;
								flag = true;
							}
						}
						if (!flag)
						{
							this.entity.springcooldown = true;
							if (this.vectordata[0].x > 1f && (component2.npcdata == null || component2.npcdata.objecttype != NPCControl.ObjectTypes.RollingRock))
							{
								component2.Jump(this.vectordata[0].x);
							}
							else
							{
								component2.Jump(component2.jumpheight);
							}
						}
					}
					if (this.entity.bounceanim != null)
					{
						this.entity.StopCoroutine(this.entity.bounceanim);
					}
					this.entity.bounceanim = this.entity.StartCoroutine(this.entity.BounceAnim(1.2f, 50f, 0.2f, true));
					return;
				}
				if (other.CompareTag("BeetleHorn") || other.CompareTag("BeetleDash") || other.CompareTag("Icecle"))
				{
					if (this.entity.bounceanim != null)
					{
						this.entity.StopCoroutine(this.entity.bounceanim);
					}
					this.entity.bounceanim = this.entity.StartCoroutine(this.entity.BounceAnim(1.2f, 50f, 0.2f, true));
					return;
				}
				return;
			}
			case NPCControl.ObjectTypes.DigSpot:
				if (!this.entity.iskill && MainManager.player != null && other.transform == MainManager.player.transform && MainManager.player.uproot)
				{
					MainManager.StopSound("DigPop2", 0f);
					MainManager.PlaySound("DigPop", -1, 1f, 0.7f);
					if (this.data[0] < 2)
					{
						NPCControl npccontrol = EntityControl.CreateItem(base.transform.position, (this.data[0] == 1) ? 3 : this.data[1], (this.data[0] == 1) ? this.data[1] : this.data[2], MainManager.RandomItemBounce(4f, 15f), (this.data[0] == 1) ? -1 : this.GetItemTimer(this.data[1]));
						if (this.data[0] == 1)
						{
							npccontrol.data = new int[]
							{
								this.data[1]
							};
						}
						else
						{
							npccontrol.activationflag = this.activationflag;
							npccontrol.regionalflag = this.regionalflag;
						}
						EntityControl.IgnoreColliders(this.entity, npccontrol.entity, true);
					}
					else
					{
						base.StartCoroutine(this.WaitForEvent());
					}
					this.entity.iskill = true;
					return;
				}
				return;
			default:
				switch (objectTypes)
				{
				case NPCControl.ObjectTypes.RollingRock:
					if (MainManager.IsPaused())
					{
						return;
					}
					if (MainManager.player != null && other.transform == MainManager.player.transform && !MainManager.player.digging)
					{
						MainManager.events.StartEvent(116, this);
						return;
					}
					if (other.CompareTag("RockLimit"))
					{
						this.BreakRock();
						return;
					}
					return;
				case NPCControl.ObjectTypes.TriggerSwitch:
					break;
				case NPCControl.ObjectTypes.WindPusher:
					if (MainManager.FreePlayer(false) && MainManager.player.transform == other.transform && (this.data[0] == -1 || MainManager.GetEntity(this.data[0]).npcdata.hit))
					{
						MainManager.player.transform.position += this.internalvector[0] * MainManager.TieFramerate(this.vectordata[1].x) * (MainManager.player.entity.onground ? 0.25f : 1.05f);
						return;
					}
					return;
				default:
					return;
				}
				break;
			}
			if (MainManager.player != null && other.transform == MainManager.player.transform && !MainManager.instance.inevent && !this.hit && !MainManager.instance.pause)
			{
				this.ActivateRegion();
				objectTypes = this.objecttype;
				switch (objectTypes)
				{
				case NPCControl.ObjectTypes.DoorOtherMap:
					if (!MainManager.instance.minipause)
					{
						MainManager.instance.StartCoroutine(MainManager.TransferMap(this.data[0], this.vectordata[0], this.vectordata[1], this.vectordata[2], this));
						return;
					}
					break;
				case NPCControl.ObjectTypes.SetPlayerRespawn:
				case NPCControl.ObjectTypes.Beemerang:
					break;
				case NPCControl.ObjectTypes.DoorSameMap:
					if (!MainManager.instance.minipause && !MainManager.instance.pause)
					{
						base.StartCoroutine(MainManager.map.MoveInside(this));
						return;
					}
					break;
				case NPCControl.ObjectTypes.EventTrigger:
					if (!MainManager.instance.minipause && !MainManager.instance.pause)
					{
						base.StartCoroutine(this.WaitForEvent());
						return;
					}
					break;
				case NPCControl.ObjectTypes.DialogueTrigger:
					if (!MainManager.instance.minipause && !MainManager.instance.pause)
					{
						base.StartCoroutine(this.WaitForEvent());
						return;
					}
					break;
				default:
					if (objectTypes != NPCControl.ObjectTypes.TriggerSwitch)
					{
						if (objectTypes != NPCControl.ObjectTypes.BattleMapChange)
						{
							return;
						}
						MainManager.map.battlemap = (MainManager.BattleMaps)this.data[0];
						return;
					}
					else
					{
						NPCControl npccontrol2 = this;
						if (this.data[0] > -1)
						{
							npccontrol2 = MainManager.GetEntity(this.data[0]).npcdata;
						}
						if (this.data[1] == -1)
						{
							npccontrol2.hit = !npccontrol2.hit;
						}
						else
						{
							npccontrol2.hit = (this.data[1] == 1);
						}
						if (MainManager.player.beemerang != null && this.data[2] == 1)
						{
							Object.Destroy(MainManager.player.beemerang.gameObject);
						}
					}
					break;
				}
			}
		}
	}

	// Token: 0x060006DB RID: 1755 RVA: 0x00058677 File Offset: 0x00056877
	private IEnumerator WaitForEvent()
	{
		this.hit = true;
		MainManager.instance.minipause = true;
		while (MainManager.player.switchcooldown > 0f)
		{
			yield return null;
		}
		yield return null;
		MainManager.player.CancelAction();
		yield return null;
		NPCControl.ObjectTypes objectTypes = this.objecttype;
		if (objectTypes != NPCControl.ObjectTypes.EventTrigger)
		{
			if (objectTypes != NPCControl.ObjectTypes.DialogueTrigger)
			{
				if (objectTypes == NPCControl.ObjectTypes.DigSpot)
				{
					MainManager.events.StartEvent(this.data[1], this);
				}
			}
			else
			{
				MainManager.instance.StartCoroutine(MainManager.SetText(MainManager.GetDialogueText(this.data[0]), 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, base.transform, null));
			}
		}
		else
		{
			MainManager.events.StartEvent(this.data[0], this);
		}
		this.hit = false;
		objectTypes = this.objecttype;
		if (objectTypes - NPCControl.ObjectTypes.EventTrigger <= 1 && (this.data.Length == 1 || this.data[1] == 0))
		{
			Object.Destroy(base.gameObject);
		}
		yield break;
	}

	// Token: 0x060006DC RID: 1756 RVA: 0x00058686 File Offset: 0x00056886
	public void SetHitInteract(NPCControl.HitInteract type)
	{
		this.interactedwithhit = false;
		this.specialinteract = type;
		if (this.entity.originalid == -1)
		{
			this.entity.ccol.enabled = true;
			this.entity.ccol.isTrigger = true;
		}
	}

	// Token: 0x060006DD RID: 1757 RVA: 0x000586C8 File Offset: 0x000568C8
	private void OnTriggerExit(Collider other)
	{
		if (this.entitytype == NPCControl.NPCType.Object)
		{
			NPCControl.ObjectTypes objectTypes = this.objecttype;
			if (objectTypes != NPCControl.ObjectTypes.PressurePlate)
			{
				if (objectTypes != NPCControl.ObjectTypes.TempPlatform)
				{
					if (objectTypes != NPCControl.ObjectTypes.TriggerSwitch)
					{
						return;
					}
					if (MainManager.player != null && other.transform == MainManager.player.transform && this.data[0] == -1)
					{
						this.hit = (this.data[1] == 0);
					}
				}
				else if (MainManager.player != null && (MainManager.player.transform == other.transform || MainManager.player.entity.feet.transform == other.transform))
				{
					this.hit = false;
					if (!MainManager.instance.minipause)
					{
						this.entity.anim.Play("StayOpen");
						return;
					}
				}
			}
			else if ((MainManager.player.beemerang == null || other.transform != MainManager.player.beemerang.transform) && ((this.data[0] == 1 && MainManager.player != null && other.transform == MainManager.player.transform) || (this.data[1] == 1 && other.GetComponent<NPCControl>() != null) || other.GetComponent<Hornable>() != null || other.CompareTag("PushRock")))
			{
				this.hit = false;
				this.actioncooldown = 0f;
				this.SwitchSound(false);
				return;
			}
		}
	}

	// Token: 0x060006DE RID: 1758 RVA: 0x0005885F File Offset: 0x00056A5F
	private IEnumerator CheckItem()
	{
		bool ismoney = this.entity.animstate == 6 || this.entity.animstate == 7 || this.entity.animstate == 186;
		while ((MainManager.instance.minipause || MainManager.instance.pause) && !this.hit)
		{
			yield return null;
		}
		if (this.entity.animid > 0 || !ismoney)
		{
			MainManager.player.CancelAction();
		}
		MainManager.instance.itempicked = true;
		if (!ismoney)
		{
			MainManager.player.entity.icooldown = 0f;
		}
		this.hit = true;
		this.tossed = false;
		bool firstflip = MainManager.player.entity.flip;
		EntityControl.IgnoreColliders(this.entity, MainManager.player.entity, true);
		if (this.objecttype == NPCControl.ObjectTypes.Item && this.touchcooldown <= 0f && !this.trapped && !MainManager.instance.minipause)
		{
			if (this.entity.animid > 1 || !ismoney)
			{
				MainManager.player.lockkeys = true;
				float temp = 0f;
				while (!MainManager.player.entity.onground)
				{
					MainManager.player.entity.rigid.velocity = new Vector3(0f, MainManager.player.entity.rigid.velocity.y, 0f);
					MainManager.instance.minipause = true;
					base.transform.position = new Vector3(0f, 999f);
					if (temp >= 300f)
					{
						MainManager.player.transform.position = MainManager.player.lastpos;
						MainManager.DeathSmoke(MainManager.player.transform.position);
						temp = 0f;
						break;
					}
					temp += MainManager.framestep;
					yield return null;
				}
				MainManager.player.lockkeys = false;
			}
			if (ismoney)
			{
				this.ActivateRegion();
			}
			if (this.entity.animid == 3 || this.entity.animid == 2 || !ismoney)
			{
				this.beerang = null;
				this.entity.spin = Vector3.zero;
				this.entity.sprite.transform.localEulerAngles = Vector3.zero;
				MainManager.player.CancelAction();
				this.entity.ccol.enabled = false;
				this.entity.rigid.useGravity = false;
				this.entity.rigid.constraints = RigidbodyConstraints.FreezeAll;
				base.transform.parent = MainManager.player.entity.sprite.transform;
				base.transform.localPosition = new Vector3(0f, 2.25f, -0.1f);
				MainManager.instance.flagvar[0] = this.entity.animstate;
				SpriteRenderer spriteRenderer = MainManager.NewSpriteObject("back", new Vector3(0f, 0f, 0.2f), Vector3.zero, this.entity.sprite.transform, MainManager.guisprites[85], this.entity.sprite.material);
				spriteRenderer.transform.localScale = Vector3.zero;
				spriteRenderer.gameObject.layer = 14;
				spriteRenderer.gameObject.AddComponent<DialogueAnim>();
				MainManager.instance.flagstring[1] = MainManager.menutext[125];
				if (this.entity.animid == 3)
				{
					spriteRenderer.transform.localPosition += Vector3.up * 0.5f;
					this.entity.model.transform.localScale = new Vector3(this.entity.model.transform.localScale.x, 0.1f, this.entity.model.transform.localScale.z);
					spriteRenderer.material.color = Color.cyan;
					MainManager.instance.crystalbflags[this.data[0]] = true;
					MainManager.instance.flagstring[0] = MainManager.menutext[112];
					MainManager.instance.flagvar[14]++;
				}
				else if (this.entity.animid != 2)
				{
					if (this.entity.animid == 0)
					{
						spriteRenderer.material.color = new Color(0f, 0.7f, 0.7f);
					}
					else
					{
						spriteRenderer.material.color = new Color(1f, 0.3f, 0.4f);
					}
					MainManager.instance.flagstring[0] = MainManager.itemdata[0, this.entity.animstate, 0];
					MainManager.instance.flagstring[1] = MainManager.itemdata[0, this.entity.animstate, 3];
				}
				else
				{
					if (MainManager.instance.flags[681] && (this.entity.animstate != 59 || !MainManager.instance.flags[696]))
					{
						int randomMedal = MainManager.GetRandomMedal();
						this.entity.basestate = randomMedal;
						this.entity.itemstate = randomMedal;
						this.entity.animstate = randomMedal;
						MainManager.instance.flagvar[0] = randomMedal;
						this.entity.overridemovesmoke = true;
						this.entity.UpdateItem();
					}
					spriteRenderer.material.color = new Color(1f, 0.5f, 0f);
					MainManager.instance.flagstring[0] = MainManager.GetBadgeName(this.entity.animstate);
					MainManager.instance.flagstring[1] = MainManager.badgedata[this.entity.animstate, 6];
				}
				if (this.entity.animid < 3)
				{
					this.CreateDescWindow(false);
				}
				string text = MainManager.menutext[2];
				string text2 = "";
				string text3 = "";
				if (!MainManager.instance.flags[31] && this.entity.animid == 2)
				{
					text2 += "|flag,31,true||tail,null||center,true||destroydescbox||goto,-32,break,end|";
				}
				else if (!MainManager.instance.flags[108] && this.entity.animid == 3)
				{
					text2 += "|flag,108,true||tail,null||center,true||destroydescbox||goto,-88,break,end|";
				}
				if (this.data.Length > 1 && this.data[1] > -1)
				{
					if (text2.Length > 0)
					{
						text2 += "|break|";
					}
					text2 = string.Concat(new object[]
					{
						text2,
						"|event,",
						this.data[1],
						"|"
					});
				}
				if (this.entity.animid != 3)
				{
					if (this.activationflag > 0)
					{
						text3 = string.Concat(new object[]
						{
							text3,
							"|flag,",
							this.activationflag,
							",true|"
						});
					}
					if (this.regionalflag > -1)
					{
						text3 = string.Concat(new object[]
						{
							text3,
							"|regionalflag,",
							this.regionalflag,
							",true|"
						});
					}
				}
				MainManager.PlaySound("ItemGet" + this.entity.animid);
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					"|lockmovement||boxstyle,4||halfline||spd,0||anim,-1,4||center|",
					text,
					"|stopskip||fwait,0.45||break|",
					text3,
					"|additemtoss,",
					this.entity.animid,
					",var,0|",
					text2
				}), 0, new float?(MainManager.messagebreak), true, false, Vector3.zero, Vector3.zero, Vector2.one, null, this));
				this.timer = -1f;
				this.entity.overrideflip = true;
				while (MainManager.instance.message)
				{
					MainManager.player.entity.flip = firstflip;
					MainManager.player.entity.animstate = 4;
					yield return null;
				}
				if (!this.tossed)
				{
					Object.Destroy(base.gameObject);
				}
			}
			else if (this.touchcooldown <= 0f)
			{
				this.touchcooldown = 99999f;
				MainManager.Items animstate = (MainManager.Items)this.entity.animstate;
				if (animstate != MainManager.Items.MoneySmall)
				{
					if (animstate != MainManager.Items.MoneyMedium)
					{
						if (animstate == MainManager.Items.MoneyBig)
						{
							MainManager.instance.money += 20;
							MainManager.instance.showmoney = 150f;
						}
					}
					else
					{
						MainManager.instance.money += 5;
						MainManager.instance.showmoney = 150f;
					}
				}
				else
				{
					MainManager.instance.money++;
					MainManager.instance.showmoney = 150f;
				}
				if (ismoney)
				{
					MainManager.PlaySound("Money");
					this.entity.spin = new Vector3(0f, 30f);
					base.StartCoroutine(this.BerryBounce());
					this.dummy = true;
					MainManager.instance.money = Mathf.Clamp(MainManager.instance.money, 0, 999);
				}
				this.entity.iskill = true;
			}
		}
		MainManager.instance.itempicked = false;
		yield break;
	}

	// Token: 0x060006DF RID: 1759 RVA: 0x0005886E File Offset: 0x00056A6E
	private IEnumerator BerryBounce()
	{
		if (this.entity.sprite != null)
		{
			float a = 0f;
			Transform t = this.entity.sprite.transform;
			Vector3 startpos = t.position;
			this.entity.sprite.enabled = true;
			t.parent = MainManager.map.transform;
			this.entity.sprite = null;
			this.entity.animid = -1;
			do
			{
				t.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, a / 50f);
				t.position = MainManager.BeizierCurve3(startpos, MainManager.player.transform.position + Vector3.up, 5f, a / 50f);
				a += MainManager.framestep;
				yield return null;
			}
			while (a < 50f);
			yield return null;
			Object.Destroy(t.gameObject);
			Object.Destroy(base.gameObject);
			t = null;
			startpos = default(Vector3);
		}
		yield break;
	}

	// Token: 0x060006E0 RID: 1760 RVA: 0x00058880 File Offset: 0x00056A80
	private void FixedUpdate()
	{
		if (this.entitytype == NPCControl.NPCType.Object && this.objecttype == NPCControl.ObjectTypes.MusicRange && this.entity.sound != null && this.entity.sound.isPlaying && this.entity.sound.time >= this.vectordata[1].y)
		{
			this.entity.sound.time = this.vectordata[1].z;
		}
	}

	// Token: 0x060006E1 RID: 1761 RVA: 0x0005890C File Offset: 0x00056B0C
	private void BreakIceRock()
	{
		MainManager.PlayParticle("IceShatter", null, base.transform.position + Vector3.up, new Vector3(-90f, 0f), 1f);
		AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audio/Sounds/IceBreak"), base.transform.position, MainManager.GetSoundDistance(base.transform.position) * MainManager.soundvolume);
		this.actioncooldown = 300f;
		base.transform.position = this.entity.startpos.Value;
		this.entity.onground = false;
		this.entity.rigid.constraints = RigidbodyConstraints.FreezeRotation;
	}

	// Token: 0x060006E2 RID: 1762 RVA: 0x000589C4 File Offset: 0x00056BC4
	private void OnCollisionEnter(Collision other)
	{
		if (this.entitytype == NPCControl.NPCType.NPC && this.HasBehavior(NPCControl.ActionBehaviors.StealthAI) && other.gameObject.CompareTag("Hornable"))
		{
			Hornable component = other.gameObject.GetComponent<Hornable>();
			if (component.type == Hornable.Type.IceCube)
			{
				component.parent.ShatterDroppletIce();
				return;
			}
		}
		else if (this.entitytype == NPCControl.NPCType.Object)
		{
			NPCControl.ObjectTypes objectTypes = this.objecttype;
			if (objectTypes == NPCControl.ObjectTypes.RollingRock)
			{
				NPCControl component2 = other.transform.GetComponent<NPCControl>();
				if (component2 != null && component2.entitytype == NPCControl.NPCType.Object && component2.objecttype == NPCControl.ObjectTypes.BreakableRock)
				{
					component2.BreakRock();
				}
			}
		}
	}

	// Token: 0x060006E3 RID: 1763 RVA: 0x00058A58 File Offset: 0x00056C58
	private int ExpEstimate()
	{
		int num = 0;
		for (int i = 0; i < this.battleids.Length; i++)
		{
			num += MainManager.GetEXP(Convert.ToInt32(MainManager.enemydata[this.battleids[i], 3]), (MainManager.Enemies)this.battleids[i]);
		}
		return num;
	}

	// Token: 0x060006E4 RID: 1764 RVA: 0x00058AA3 File Offset: 0x00056CA3
	public void StartBattle(bool adv)
	{
		if (adv)
		{
			this.attacking = true;
		}
		this.StartBattle();
	}

	// Token: 0x060006E5 RID: 1765 RVA: 0x00058AB8 File Offset: 0x00056CB8
	private bool CheckBump()
	{
		return MainManager.BadgeIsEquipped(18) && ((MainManager.instance.flags[555] && MainManager.instance.partylevel == 27 && MainManager.map.mapid != MainManager.Maps.GiantLairDeadLands1 && MainManager.map.mapid != MainManager.Maps.GiantLairDeadLands2) || (MainManager.instance.areaid != 15 && MainManager.map.mapid != MainManager.Maps.BugariaPlazaAttack && MainManager.map.mapid != MainManager.Maps.BugariaBridgeAttack && MainManager.map.mapid != MainManager.Maps.BugariaCastleAttack && (MainManager.instance.partylevel == 27 || this.ExpEstimate() <= this.battleids.Length)));
	}

	// Token: 0x060006E6 RID: 1766 RVA: 0x00058B74 File Offset: 0x00056D74
	public void StartBattle()
	{
		if (MainManager.player.shield && this.entity.originalid != 353)
		{
			if (this.touchcooldown <= 0f)
			{
				int num = MainManager.SoundIsPlaying("ShieldHit");
				if (num == -1 || MainManager.sounds[num].time > 0.5f)
				{
					MainManager.PlaySound("ShieldHit", -1, Random.Range(0.95f, 1.05f), 0.75f);
				}
				float time = 100f;
				Vector3 position = base.transform.position;
				Vector3 position2 = MainManager.player.transform.position;
				this.GetDizzy(time, MainManager.GetDirection(position, position2, true) * 5f);
				return;
			}
		}
		else if (MainManager.battle == null && !MainManager.instance.minipause)
		{
			if (MainManager.instance.entitytouchevent > -1)
			{
				MainManager.events.StartEvent(MainManager.instance.entitytouchevent, this);
				return;
			}
			this.entity.lastpos = base.transform.position;
			this.entity.rigid.velocity = Vector3.zero;
			if (MainManager.player.entity.icooldown <= 0f && this.freezecooldown <= 0f && !this.entity.dead && this.touchcooldown <= 0f)
			{
				if (this.CheckBump())
				{
					this.entity.rigid.isKinematic = true;
					this.entity.rigid.velocity = Vector3.zero;
					base.StartCoroutine(this.entity.Death(true));
					return;
				}
				if (this.destroyOnBattle != null && this.destroyOnBattle.Count > 0)
				{
					for (int i = 0; i < this.destroyOnBattle.Count; i++)
					{
						if (this.destroyOnBattle[i] != null)
						{
							Object.Destroy(this.destroyOnBattle[i]);
						}
					}
					this.destroyOnBattle.Clear();
				}
				List<int> list = new List<int>(this.battleids);
				string music = null;
				if (list.Contains(25) || list.Contains(28) || list.Contains(27) || list.Contains(26))
				{
					music = "Battle3";
				}
				MainManager.SetEntityLastPos(true);
				MainManager.player.CancelAction();
				this.entity.spin = Vector3.zero;
				int adv = -1;
				if (this.attacking && this.dizzytime <= 0f && !MainManager.BadgeIsEquipped(71))
				{
					adv = 3;
					MainManager.PlaySound("Damage0");
				}
				this.attacking = false;
				this.StopForceBehavior();
				MainManager.instance.StartCoroutine(BattleControl.StartBattle(this.battleids, -1, adv, music, this, true));
			}
		}
	}

	// Token: 0x060006E7 RID: 1767 RVA: 0x00058E44 File Offset: 0x00057044
	public void CutGrass()
	{
		this.entity.PlaySound("rustling1");
		this.entity.rotater.tag = "Object";
		this.hit = true;
		this.boxcol.size = Vector3.zero;
		this.boxcol.center = new Vector3(9999f, 9999f);
		this.boxcol.enabled = false;
		this.entity.sprite.sprite = MainManager.grasssprite[this.data[0] * 3 + 1];
		this.entity.sprite.material = MainManager.spritemat;
		this.entity.ccol.enabled = false;
		if (this.scol != null)
		{
			this.scol.enabled = false;
		}
		if (this.data.Length > 1 && this.data[1] > -1)
		{
			if (!MainManager.instance.crystalbflags[this.data[1]])
			{
				this.tempitem = EntityControl.CreateItem(base.transform.position + Vector3.up * 0.5f, 3, this.data[1], MainManager.RandomItemBounce(4f, 12f), -1);
				this.tempitem.data = new int[]
				{
					this.data[1]
				};
			}
		}
		else
		{
			if (this.vectordata.Length != 0)
			{
				int num = Random.Range(0, this.vectordata.Length);
				if (this.vectordata[num].x > -1f)
				{
					this.tempitem = EntityControl.CreateItem(base.transform.position + Vector3.up * 0.5f, 0, (int)this.vectordata[num].x, MainManager.RandomItemBounce(4f, 12f), 600);
					this.tempitem.regionalflag = this.regionalflag;
					this.tempitem.activationflag = this.activationflag;
				}
				else
				{
					this.ActivateRegion();
				}
			}
			else
			{
				this.ActivateRegion();
			}
			if (Random.Range(0, 100) <= 12)
			{
				EntityControl.CreateItem(base.transform.position + Vector3.up * 0.5f, 0, 6, MainManager.RandomItemBounce(4f, 12f), 600);
			}
		}
		if (this.tempitem != null)
		{
			EntityControl.IgnoreColliders(this.entity, this.tempitem.entity, true);
		}
		base.StartCoroutine(this.GrassFade());
	}

	// Token: 0x060006E8 RID: 1768 RVA: 0x000590D9 File Offset: 0x000572D9
	private IEnumerator RespawnPlayer(bool hide)
	{
		MainManager.instance.minipause = true;
		if (MainManager.player.beemerang != null)
		{
			Object.Destroy(MainManager.player.beemerang.gameObject);
		}
		if (this.objecttype == NPCControl.ObjectTypes.TempPlatform && this.entity.originalid + 1 == 71)
		{
			if (MainManager.player.beemerang != null)
			{
				Object.Destroy(MainManager.player.beemerang.gameObject);
			}
			Hornable[] array = Object.FindObjectsOfType<Hornable>();
			for (int i = 0; i < array.Length; i++)
			{
				if (Vector3.Distance(array[i].transform.position, base.transform.position + Vector3.up * 10f) < 4.5f && array[i].type == Hornable.Type.IceCube)
				{
					array[i].parent.ShatterDroppletIce();
				}
			}
			MainManager.instance.camtarget = null;
			MainManager.instance.camtargetpos = null;
			MainManager.PlaySound("Bite");
			this.entity.anim.Play("Close");
			MainManager.TeleportFollowers();
			yield return new WaitForSeconds(0.07f);
			for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
			{
				MainManager.instance.playerdata[j].entity.rigid.isKinematic = true;
			}
			MainManager.player.transform.position = base.transform.position + new Vector3(0f, 1000f);
			MainManager.TeleportFollowers();
			float time = this.vectordata[0].x;
			while (time > 0f)
			{
				time -= MainManager.TieFramerate(1f);
				yield return null;
			}
			MainManager.PlayTransition(0, 0, 0.1f, Color.black);
			yield return new WaitForSeconds(1f);
			MainManager.player.transform.position = MainManager.player.lastpos;
			MainManager.TeleportFollowers();
			this.entity.anim.Play("Open");
			MainManager.ResetCamera(true);
			yield return null;
			MainManager.PlayTransition(1, 0, 0.1f, Color.black);
			for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
			{
				MainManager.instance.playerdata[k].entity.rigid.isKinematic = false;
			}
		}
		MainManager.instance.minipause = false;
		yield return null;
		yield break;
	}

	// Token: 0x060006E9 RID: 1769 RVA: 0x000590E8 File Offset: 0x000572E8
	private IEnumerator GrassFade()
	{
		SpriteRenderer grass = new GameObject("grass").AddComponent<SpriteRenderer>();
		grass.tag = "DelAftBtl";
		grass.transform.parent = MainManager.map.transform;
		grass.shadowCastingMode = ShadowCastingMode.TwoSided;
		grass.transform.position = base.transform.position;
		grass.material = MainManager.spritemat;
		grass.sprite = MainManager.grasssprite[this.data[0] * 3 + 2];
		SphereCollider sphereCollider = grass.gameObject.AddComponent<SphereCollider>();
		sphereCollider.radius = 0.25f;
		sphereCollider.center = Vector3.zero;
		BoxCollider boxCollider = grass.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(1f, 1.5f, 0.15f);
		boxCollider.center = new Vector3(0f, 0.75f);
		grass.gameObject.layer = 9;
		Rigidbody r = grass.gameObject.AddComponent<Rigidbody>();
		r.AddTorque(MainManager.RandomItemBounce(5f, 0f));
		r.velocity = Vector3.up * 10f;
		if (this.tempitem != null)
		{
			Physics.IgnoreCollision(sphereCollider, this.tempitem.entity.ccol, true);
			Physics.IgnoreCollision(boxCollider, this.tempitem.entity.ccol, true);
		}
		float a = 0f;
		bool up = true;
		while (a < 180f)
		{
			if (r.velocity.y < 0f && up)
			{
				up = false;
				this.entity.PlaySound("rustling2");
			}
			else if (r.velocity.y > 0f)
			{
				up = true;
			}
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		grass.material = MainManager.spritematlit;
		grass.material.renderQueue = 3000;
		while (grass.material.color.a > 0.1f)
		{
			grass.material.color = new Color(grass.material.color.r, grass.material.color.g, grass.material.color.b, grass.material.color.a - MainManager.TieFramerate(0.05f));
			yield return null;
		}
		Object.Destroy(grass.gameObject);
		yield break;
	}

	// Token: 0x0400060F RID: 1551
	public NPCControl.ObjectTypes objecttype;

	// Token: 0x04000610 RID: 1552
	public NPCControl.Interaction interacttype;

	// Token: 0x04000611 RID: 1553
	public NPCControl.NPCType entitytype;

	// Token: 0x04000612 RID: 1554
	public NPCControl.ActionBehaviors[] behaviors;

	// Token: 0x04000613 RID: 1555
	private NPCControl.ActionBehaviors? forcebehavior;

	// Token: 0x04000614 RID: 1556
	public NPCControl[] gates;

	// Token: 0x04000615 RID: 1557
	public NPCControl shopkeeper;

	// Token: 0x04000616 RID: 1558
	public NPCControl tempitem;

	// Token: 0x04000617 RID: 1559
	public NPCControl spawned;

	// Token: 0x04000618 RID: 1560
	public EntityControl entity;

	// Token: 0x04000619 RID: 1561
	public int[] battleids;

	// Token: 0x0400061A RID: 1562
	public int[] requires;

	// Token: 0x0400061B RID: 1563
	public int[] limit;

	// Token: 0x0400061C RID: 1564
	public int[] data = new int[1];

	// Token: 0x0400061D RID: 1565
	public Vector3[] vectordata = new Vector3[]
	{
		Vector3.zero
	};

	// Token: 0x0400061E RID: 1566
	public Vector3[] dialogues = new Vector3[]
	{
		new Vector3(-1f, 0f, 0f)
	};

	// Token: 0x0400061F RID: 1567
	public Transform disguiseobj;

	// Token: 0x04000620 RID: 1568
	public Transform moveobj;

	// Token: 0x04000621 RID: 1569
	public Transform beerang;

	// Token: 0x04000622 RID: 1570
	private Transform rotater;

	// Token: 0x04000623 RID: 1571
	private GameObject particles;

	// Token: 0x04000624 RID: 1572
	private Vector3 icevel;

	// Token: 0x04000625 RID: 1573
	private HelpArrow arrow;

	// Token: 0x04000626 RID: 1574
	public CapsuleCollider pusher;

	// Token: 0x04000627 RID: 1575
	public CapsuleCollider secondcoll;

	// Token: 0x04000628 RID: 1576
	public bool tossed;

	// Token: 0x04000629 RID: 1577
	public bool shrink;

	// Token: 0x0400062A RID: 1578
	public bool hit;

	// Token: 0x0400062B RID: 1579
	public bool originalflip;

	// Token: 0x0400062C RID: 1580
	public bool inrange;

	// Token: 0x0400062D RID: 1581
	public bool heldonce;

	// Token: 0x0400062E RID: 1582
	public bool nointeract;

	// Token: 0x0400062F RID: 1583
	public bool freezeconstraints;

	// Token: 0x04000630 RID: 1584
	public bool tempobject;

	// Token: 0x04000631 RID: 1585
	public bool trapped;

	// Token: 0x04000632 RID: 1586
	public bool dummy;

	// Token: 0x04000633 RID: 1587
	public bool attacking;

	// Token: 0x04000634 RID: 1588
	public bool returntoheight;

	// Token: 0x04000635 RID: 1589
	public bool interactedwithhit;

	// Token: 0x04000636 RID: 1590
	public bool overridebehavior;

	// Token: 0x04000637 RID: 1591
	public bool extrafreeze;

	// Token: 0x04000638 RID: 1592
	private NPCControl.HitInteract specialinteract;

	// Token: 0x04000639 RID: 1593
	private bool started;

	// Token: 0x0400063A RID: 1594
	private bool ignoreconstraint;

	// Token: 0x0400063B RID: 1595
	private bool hasenteredrange;

	// Token: 0x0400063C RID: 1596
	public Vector3 savepos;

	// Token: 0x0400063D RID: 1597
	public Vector2[] emoticonflag = new Vector2[]
	{
		new Vector2(-1f, -1f)
	};

	// Token: 0x0400063E RID: 1598
	public Collider[] internalcollider;

	// Token: 0x0400063F RID: 1599
	private float[] internaldata;

	// Token: 0x04000640 RID: 1600
	private ParticleSystem[] internalparticle;

	// Token: 0x04000641 RID: 1601
	public Color tagcolor;

	// Token: 0x04000642 RID: 1602
	public int bounces;

	// Token: 0x04000643 RID: 1603
	public int eventid;

	// Token: 0x04000644 RID: 1604
	public int overridediag = -1;

	// Token: 0x04000645 RID: 1605
	public int insideid;

	// Token: 0x04000646 RID: 1606
	public int tattleid = -1;

	// Token: 0x04000647 RID: 1607
	public int currentdialogueindex = -1;

	// Token: 0x04000648 RID: 1608
	public int regionalflag = -1;

	// Token: 0x04000649 RID: 1609
	public int disguisecooldown = -1;

	// Token: 0x0400064A RID: 1610
	public int activationflag = -1;

	// Token: 0x0400064B RID: 1611
	public int mapid = -1;

	// Token: 0x0400064C RID: 1612
	public float[] actionfrequency;

	// Token: 0x0400064D RID: 1613
	public float behaviorcooldown;

	// Token: 0x0400064E RID: 1614
	public float freezetime = 600f;

	// Token: 0x0400064F RID: 1615
	public float radius = 1.25f;

	// Token: 0x04000650 RID: 1616
	public float colliderheight = 3f;

	// Token: 0x04000651 RID: 1617
	public float wanderradius = 3f;

	// Token: 0x04000652 RID: 1618
	public float speedmultiplier = 1f;

	// Token: 0x04000653 RID: 1619
	public float radiuslimit = 6f;

	// Token: 0x04000654 RID: 1620
	public float timer = -1f;

	// Token: 0x04000655 RID: 1621
	public float teleportradius = 9f;

	// Token: 0x04000656 RID: 1622
	public float thisdistance;

	// Token: 0x04000657 RID: 1623
	public float touchcooldown;

	// Token: 0x04000658 RID: 1624
	public float freezecooldown;

	// Token: 0x04000659 RID: 1625
	public float startlife;

	// Token: 0x0400065A RID: 1626
	public float dizzytime;

	// Token: 0x0400065B RID: 1627
	public float actioncooldown;

	// Token: 0x0400065C RID: 1628
	public float freezeaircooldown;

	// Token: 0x0400065D RID: 1629
	public float interactcd;

	// Token: 0x0400065E RID: 1630
	private float awaycountdown = 300f;

	// Token: 0x0400065F RID: 1631
	private float walkcooldown;

	// Token: 0x04000660 RID: 1632
	private float mmulti;

	// Token: 0x04000661 RID: 1633
	private float dirtcd;

	// Token: 0x04000662 RID: 1634
	public SphereCollider scol;

	// Token: 0x04000663 RID: 1635
	public BoxCollider boxcol;

	// Token: 0x04000664 RID: 1636
	private List<OverworldProjectile> projectiles;

	// Token: 0x04000665 RID: 1637
	private List<GameObject> destroyOnBattle = new List<GameObject>();

	// Token: 0x04000666 RID: 1638
	private DialogueAnim descwindow;

	// Token: 0x04000667 RID: 1639
	public Transform[] internaltransform;

	// Token: 0x04000668 RID: 1640
	private MeshRenderer[] internalrender;

	// Token: 0x04000669 RID: 1641
	public Coroutine behaviorroutine;

	// Token: 0x0400066A RID: 1642
	private Vector3[] internalvector;

	// Token: 0x0400066B RID: 1643
	private int templayer = -1;

	// Token: 0x0400066C RID: 1644
	private int trycount;

	// Token: 0x0400066D RID: 1645
	private int maxtries;

	// Token: 0x0400066E RID: 1646
	private int currentnode;

	// Token: 0x0400066F RID: 1647
	private int collisionammount;

	// Token: 0x04000670 RID: 1648
	public const float itembounce = 1.2f;

	// Token: 0x04000671 RID: 1649
	public const float battleoffset = 1.1f;

	// Token: 0x04000672 RID: 1650
	public const float defaulttalkrange = 1.25f;

	// Token: 0x04000673 RID: 1651
	private const float dropplettime = 600f;

	// Token: 0x04000674 RID: 1652
	private const int tryammount = 50;

	// Token: 0x04000675 RID: 1653
	private EntityControl[] shopitems;

	// Token: 0x04000676 RID: 1654
	private float respawntimer = -100f;

	// Token: 0x0200025B RID: 603
	public enum NPCType
	{
		// Token: 0x04001FFF RID: 8191
		NPC,
		// Token: 0x04002000 RID: 8192
		Enemy,
		// Token: 0x04002001 RID: 8193
		Object,
		// Token: 0x04002002 RID: 8194
		SemiNPC
	}

	// Token: 0x0200025C RID: 604
	public enum Interaction
	{
		// Token: 0x04002004 RID: 8196
		None,
		// Token: 0x04002005 RID: 8197
		Talk,
		// Token: 0x04002006 RID: 8198
		Check,
		// Token: 0x04002007 RID: 8199
		SavePoint,
		// Token: 0x04002008 RID: 8200
		Event,
		// Token: 0x04002009 RID: 8201
		TalkReturnToOriginalFlip,
		// Token: 0x0400200A RID: 8202
		Shop,
		// Token: 0x0400200B RID: 8203
		ShopKeeper,
		// Token: 0x0400200C RID: 8204
		QuestBoard,
		// Token: 0x0400200D RID: 8205
		StorageAnt,
		// Token: 0x0400200E RID: 8206
		CaravanBadge,
		// Token: 0x0400200F RID: 8207
		VenusHeal,
		// Token: 0x04002010 RID: 8208
		LockedDoor
	}

	// Token: 0x0200025D RID: 605
	public enum DeathType
	{
		// Token: 0x04002012 RID: 8210
		None,
		// Token: 0x04002013 RID: 8211
		SpinSmoke,
		// Token: 0x04002014 RID: 8212
		Smoke,
		// Token: 0x04002015 RID: 8213
		Shrink,
		// Token: 0x04002016 RID: 8214
		PlayerDeath,
		// Token: 0x04002017 RID: 8215
		SpinNoSmoke,
		// Token: 0x04002018 RID: 8216
		SpinKO,
		// Token: 0x04002019 RID: 8217
		KO,
		// Token: 0x0400201A RID: 8218
		SpinSmokeNoSprite,
		// Token: 0x0400201B RID: 8219
		ShrinkNoSmoke,
		// Token: 0x0400201C RID: 8220
		NinjaLog,
		// Token: 0x0400201D RID: 8221
		Sink,
		// Token: 0x0400201E RID: 8222
		ExplodeAnim,
		// Token: 0x0400201F RID: 8223
		DropSprites
	}

	// Token: 0x0200025E RID: 606
	public enum ShopData
	{
		// Token: 0x04002021 RID: 8225
		Greeting,
		// Token: 0x04002022 RID: 8226
		BuyDiag,
		// Token: 0x04002023 RID: 8227
		Sell,
		// Token: 0x04002024 RID: 8228
		SellNothing,
		// Token: 0x04002025 RID: 8229
		SellOK,
		// Token: 0x04002026 RID: 8230
		Quit,
		// Token: 0x04002027 RID: 8231
		BuyExplanation,
		// Token: 0x04002028 RID: 8232
		BuyFull,
		// Token: 0x04002029 RID: 8233
		Radius,
		// Token: 0x0400202A RID: 8234
		BadgePoolID,
		// Token: 0x0400202B RID: 8235
		ItemType
	}

	// Token: 0x0200025F RID: 607
	public enum PauseWindows
	{
		// Token: 0x0400202D RID: 8237
		MainPause,
		// Token: 0x0400202E RID: 8238
		Items,
		// Token: 0x0400202F RID: 8239
		Badges,
		// Token: 0x04002030 RID: 8240
		Library,
		// Token: 0x04002031 RID: 8241
		Settings,
		// Token: 0x04002032 RID: 8242
		KeyBinds
	}

	// Token: 0x02000260 RID: 608
	public enum HitInteract
	{
		// Token: 0x04002034 RID: 8244
		None,
		// Token: 0x04002035 RID: 8245
		HornDash,
		// Token: 0x04002036 RID: 8246
		AnyHorn
	}

	// Token: 0x02000261 RID: 609
	public enum ActionBehaviors
	{
		// Token: 0x04002038 RID: 8248
		None,
		// Token: 0x04002039 RID: 8249
		FacePlayer,
		// Token: 0x0400203A RID: 8250
		ChasePlayer,
		// Token: 0x0400203B RID: 8251
		FleeFromPlayer,
		// Token: 0x0400203C RID: 8252
		TurnRandomly,
		// Token: 0x0400203D RID: 8253
		Wander,
		// Token: 0x0400203E RID: 8254
		FaceAwayFromPlayer,
		// Token: 0x0400203F RID: 8255
		TurnFixedInterval,
		// Token: 0x04002040 RID: 8256
		Disguise,
		// Token: 0x04002041 RID: 8257
		DisguiseOnce,
		// Token: 0x04002042 RID: 8258
		FollowPlayer,
		// Token: 0x04002043 RID: 8259
		WalkAwayFromPlayer,
		// Token: 0x04002044 RID: 8260
		FaceAhead,
		// Token: 0x04002045 RID: 8261
		FaceBehind,
		// Token: 0x04002046 RID: 8262
		FaceUp,
		// Token: 0x04002047 RID: 8263
		FaceDown,
		// Token: 0x04002048 RID: 8264
		SetPath,
		// Token: 0x04002049 RID: 8265
		ChargeAtPlayer,
		// Token: 0x0400204A RID: 8266
		ChargeAtPlayerFlipSprite,
		// Token: 0x0400204B RID: 8267
		ShootProjectile,
		// Token: 0x0400204C RID: 8268
		ShootProjectilePredict,
		// Token: 0x0400204D RID: 8269
		ChargeAndAttack,
		// Token: 0x0400204E RID: 8270
		AlwaysWander,
		// Token: 0x0400204F RID: 8271
		Unmoveable,
		// Token: 0x04002050 RID: 8272
		ChargeAttackUnderground,
		// Token: 0x04002051 RID: 8273
		WanderUnderground,
		// Token: 0x04002052 RID: 8274
		StealthAI,
		// Token: 0x04002053 RID: 8275
		SetPathJump,
		// Token: 0x04002054 RID: 8276
		DisguiseOnceJumpForward,
		// Token: 0x04002055 RID: 8277
		ChangeSpriteInRandius,
		// Token: 0x04002056 RID: 8278
		ChaseWhenAnim,
		// Token: 0x04002057 RID: 8279
		WalkWhenAnim,
		// Token: 0x04002058 RID: 8280
		WanderOffscreen,
		// Token: 0x04002059 RID: 8281
		WanderNoWarp,
		// Token: 0x0400205A RID: 8282
		WanderOnWater,
		// Token: 0x0400205B RID: 8283
		ChaseOnWater
	}

	// Token: 0x02000262 RID: 610
	public enum ObjectTypes
	{
		// Token: 0x0400205D RID: 8285
		None,
		// Token: 0x0400205E RID: 8286
		BeetleGrass,
		// Token: 0x0400205F RID: 8287
		PushRock,
		// Token: 0x04002060 RID: 8288
		PressurePlate,
		// Token: 0x04002061 RID: 8289
		ANDGate,
		// Token: 0x04002062 RID: 8290
		CameraChange,
		// Token: 0x04002063 RID: 8291
		Item,
		// Token: 0x04002064 RID: 8292
		DoorOtherMap,
		// Token: 0x04002065 RID: 8293
		SetPlayerRespawn,
		// Token: 0x04002066 RID: 8294
		DoorSameMap,
		// Token: 0x04002067 RID: 8295
		Beemerang,
		// Token: 0x04002068 RID: 8296
		EventTrigger,
		// Token: 0x04002069 RID: 8297
		DialogueTrigger,
		// Token: 0x0400206A RID: 8298
		ANDBlock,
		// Token: 0x0400206B RID: 8299
		SavePoint,
		// Token: 0x0400206C RID: 8300
		JumpSpring,
		// Token: 0x0400206D RID: 8301
		DigSpot,
		// Token: 0x0400206E RID: 8302
		Switch,
		// Token: 0x0400206F RID: 8303
		MusicChange,
		// Token: 0x04002070 RID: 8304
		CoiledObject,
		// Token: 0x04002071 RID: 8305
		FixedAnim,
		// Token: 0x04002072 RID: 8306
		DigWall,
		// Token: 0x04002073 RID: 8307
		ItemSpawner,
		// Token: 0x04002074 RID: 8308
		EnemySpawner,
		// Token: 0x04002075 RID: 8309
		Dropplet,
		// Token: 0x04002076 RID: 8310
		PathPlatform,
		// Token: 0x04002077 RID: 8311
		BreakableRock,
		// Token: 0x04002078 RID: 8312
		RotatingPlatform,
		// Token: 0x04002079 RID: 8313
		Geizer,
		// Token: 0x0400207A RID: 8314
		MusicRange,
		// Token: 0x0400207B RID: 8315
		TempPlatform,
		// Token: 0x0400207C RID: 8316
		ScrewSwitch,
		// Token: 0x0400207D RID: 8317
		ResetCamera,
		// Token: 0x0400207E RID: 8318
		StencilSwitch,
		// Token: 0x0400207F RID: 8319
		RollingRock,
		// Token: 0x04002080 RID: 8320
		TriggerSwitch,
		// Token: 0x04002081 RID: 8321
		WindPusher,
		// Token: 0x04002082 RID: 8322
		WaterSwitch,
		// Token: 0x04002083 RID: 8323
		BattleMapChange
	}
}
