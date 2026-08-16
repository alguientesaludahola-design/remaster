using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InputIOManager;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000046 RID: 70
public class PauseMenu : MonoBehaviour
{
	// Token: 0x060006FA RID: 1786 RVA: 0x00059CB4 File Offset: 0x00057EB4
	private void Start()
	{
		PauseMenu.librarybreak = ((MainManager.languageid > 0 && !MainManager.AsianLang()) ? 12.75f : 11f);
		PauseMenu.mfix = new Vector3(0f, 1f, 0f);
		MainManager.pausemenu = this;
		MainManager.instance.hudcooldown = 0f;
		MainManager.instance.showmoney = 0f;
		MainManager.instance.discoveryhud = 0f;
		base.gameObject.layer = 5;
		MainManager.instance.pause = true;
		base.transform.parent = MainManager.GUICamera.transform;
		base.transform.localPosition = new Vector3(0f, 0f, 10f);
		base.transform.localEulerAngles = Vector3.zero;
		this.dimmer = new GameObject("Dimmer").AddComponent<SpriteRenderer>();
		this.dimmer.color = Color.clear;
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.SetPixel(0, 0, Color.black);
		texture2D.Apply();
		this.dimmer.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
		this.dimmer.transform.localScale = new Vector3(3000f, 3000f, 1f);
		this.dimmer.transform.parent = base.transform;
		this.dimmer.transform.localPosition = Vector3.zero;
		this.dimmer.transform.localEulerAngles = Vector3.zero;
		this.dimmer.gameObject.layer = 5;
		this.dimmer.sortingOrder = -100;
		if (!this.calledfrommain)
		{
			this.enemydata = Resources.Load<TextAsset>("Data/EnemyData").ToString().Split(new char[]
			{
				'\n'
			});
			MainManager.instance.CheckAchievement();
		}
		this.ResetTempSettings();
		MainManager.listredirect = new int?(-1);
		base.StartCoroutine(this.BuildWindow());
	}

	// Token: 0x060006FB RID: 1787 RVA: 0x00059ED8 File Offset: 0x000580D8
	private void OnGUI()
	{
		if (this.getkey)
		{
			if (this.windowid == 5)
			{
				Event current = Event.current;
				if (current.isKey && current.keyCode != KeyCode.None && this.keycooldown <= 0f && MainManager.instance.inputcooldown <= 0f)
				{
					if (InputIO.bindingkeys.Contains(current.keyCode))
					{
						bool flag = true;
						int i = 0;
						while (i < InputIO.keys.Length)
						{
							if (current.keyCode == InputIO.keys[i])
							{
								if (current.keyCode == InputIO.keys[MainManager.instance.option])
								{
									flag = true;
									break;
								}
								MainManager.PlayBuzzer();
								flag = false;
								break;
							}
							else
							{
								i++;
							}
						}
						if (flag)
						{
							this.keycooldown = 5f;
							this.gottenkey = current.keyCode;
							this.getkey = false;
							return;
						}
					}
					else if (!MainManager.sounds[10].isPlaying)
					{
						MainManager.PlayBuzzer();
						return;
					}
				}
			}
			else
			{
				int joystickRaw = InputIO.GetJoystickRaw();
				if (joystickRaw != -55)
				{
					MainManager.joybinds[MainManager.instance.option] = joystickRaw;
					this.getkey = false;
				}
			}
		}
	}

	// Token: 0x060006FC RID: 1788 RVA: 0x00059FF0 File Offset: 0x000581F0
	private void FixedUpdate()
	{
		if (this.exit)
		{
			this.dimmer.color = Color.Lerp(this.dimmer.color, Color.clear, 0.23f);
			return;
		}
		this.dimmer.color = Color.Lerp(this.dimmer.color, new Color(1f, 1f, 1f, 0.5f), 0.15f);
	}

	// Token: 0x060006FD RID: 1789 RVA: 0x0005A064 File Offset: 0x00058264
	private void SettingsToggleSound()
	{
		MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/Confirm0"), 10, 1f, 1f);
		MainManager.sounds[10].volume = this.svolume;
	}

	// Token: 0x060006FE RID: 1790 RVA: 0x0005A098 File Offset: 0x00058298
	private bool[] NearSomething()
	{
		bool[] array = new bool[3];
		array[1] = true;
		bool[] array2 = array;
		for (int i = 0; i < MainManager.map.entities.Length; i++)
		{
			if (MainManager.map.entities[i] != null && MainManager.map.entities[i].npcdata != null)
			{
				if (MainManager.map.entities[i].npcdata.entitytype != NPCControl.NPCType.Object && (MainManager.map.entities[i].npcdata.entitytype != NPCControl.NPCType.NPC || !MainManager.map.entities[i].iskill) && Vector3.Distance(MainManager.player.transform.position, MainManager.map.entities[i].startpos.Value) < MainManager.map.entities[i].npcdata.radius + 2f)
				{
					array2[0] = (MainManager.map.entities[i].npcdata.entitytype == NPCControl.NPCType.NPC);
					array2[1] = MainManager.player.candig;
					array2[2] = (MainManager.map.entities[i].npcdata.entitytype == NPCControl.NPCType.Enemy);
					return array2;
				}
				if (MainManager.map.entities[i].npcdata.entitytype == NPCControl.NPCType.Object && MainManager.map.entities[i].gameObject.activeInHierarchy && Vector3.Distance(MainManager.map.entities[i].startpos.Value, MainManager.player.transform.position) < 4f)
				{
					NPCControl.ObjectTypes objecttype = MainManager.map.entities[i].npcdata.objecttype;
					if (objecttype - NPCControl.ObjectTypes.BeetleGrass <= 1 || objecttype == NPCControl.ObjectTypes.BreakableRock)
					{
						array2[0] = true;
						array2[1] = false;
						array2[2] = false;
						return array2;
					}
				}
			}
		}
		return array2;
	}

	// Token: 0x060006FF RID: 1791 RVA: 0x0005A26C File Offset: 0x0005846C
	private bool CanDig()
	{
		return !MainManager.map.cantcompass && MainManager.instance.insideid == -1 && MainManager.player != null && MainManager.player.conveyor == null && !MainManager.instance.flags[400] && !MainManager.instance.flags[401] && MainManager.instance.flags[10] && MainManager.player.candig;
	}

	// Token: 0x06000700 RID: 1792 RVA: 0x0005A2F0 File Offset: 0x000584F0
	private void UseKeyItem()
	{
		if (MainManager.instance.items[this.option].ToArray()[MainManager.instance.option] == 37 && MainManager.instance.flags[18] && MainManager.map.mapid != MainManager.Maps.AntTunnels)
		{
			MainManager.PlaySound("Confirm", 10);
			this.ChangeWindow(-1);
			string str;
			if (!MainManager.player.candig || MainManager.map.cantcompass || MainManager.instance.insideid > -1)
			{
				str = MainManager.menutext[162] + "|break||openpause,1|";
			}
			else
			{
				string str2 = "|prompt,menu,0,2,166,165,5,6|";
				if (this.CanDig())
				{
					str = MainManager.menutext[161] + str2;
				}
				else
				{
					str = MainManager.menutext[162] + "|break||openpause,1|";
				}
			}
			base.StartCoroutine(MainManager.SetText("|boxstyle,4||halfline||spd,0||center|" + str, true, new Vector3(0f, 0.45f), null, null));
			MainManager.maintextbox.GetComponent<Animator>().Play("4");
			return;
		}
		if (MainManager.instance.items[this.option].ToArray()[MainManager.instance.option] == 41)
		{
			MainManager.PlaySound("Confirm", 10);
			this.lastmedal = MainManager.SaveList();
			this.ChangeWindow(6);
			return;
		}
		if (MainManager.instance.items[this.option].ToArray()[MainManager.instance.option] != 89)
		{
			MainManager.PlayBuzzer();
			return;
		}
		bool[] array = this.NearSomething();
		bool flag = false;
		if (array != null)
		{
			flag = (!array[0] && !array[2]);
		}
		if (flag && this.CanDig() && MainManager.instance.inmusicrange == -1)
		{
			this.ChangeWindow(-1);
			base.StartCoroutine(MainManager.SetText("|boxstyle,4||spd,0||line||center|" + MainManager.menutext[186] + "|prompt,menu,0.75,2,187,165,5,6|", true, new Vector3(0f, 0.45f), null, null));
			MainManager.maintextbox.GetComponent<Animator>().Play("4");
			return;
		}
		this.ChangeWindow(-1);
		base.StartCoroutine(MainManager.SetText("|boxstyle,4||spd,0||line||center|" + MainManager.menutext[185] + "|break||openpause,1|", true, new Vector3(0f, 0.45f), null, null));
		MainManager.maintextbox.GetComponent<Animator>().Play("4");
	}

	// Token: 0x06000701 RID: 1793 RVA: 0x0005A564 File Offset: 0x00058764
	private void Update()
	{
		this.UpdateDynamicText();
		if (this.canpick && !MainManager.instance.message)
		{
			if (MainManager.GetKey(6, true))
			{
				this.skip = MainManager.listammount;
			}
			else
			{
				this.skip = 1;
			}
			int num = -1;
			switch (this.windowid)
			{
			case 0:
				this.mapfrommain = true;
				this.IconAnim(new int[]
				{
					13,
					14,
					15,
					16
				});
				if (MainManager.instance.inputcooldown <= 0f)
				{
					if (MainManager.GetKey(1, false) || MainManager.GetKey(3, false))
					{
						this.option++;
						if (this.option >= this.maxoptions)
						{
							this.option = 0;
						}
						MainManager.PlayScrollSound();
						this.UpdateText();
					}
					else if (MainManager.GetKey(0, false) || MainManager.GetKey(2, false))
					{
						this.option--;
						if (this.option < 0)
						{
							this.option = this.maxoptions - 1;
						}
						MainManager.PlayScrollSound();
						this.UpdateText();
					}
					if (MainManager.GetKey(4, false))
					{
						this.firstoption = this.option;
						this.windowid = this.option + 1;
						MainManager.PlaySound("Confirm", 10);
						base.StartCoroutine(this.BuildWindow());
					}
					else if (MainManager.GetKey(9) && this.sprites[18] != null)
					{
						this.windowid = 6;
						MainManager.PlaySound("Confirm", 10);
						base.StartCoroutine(this.BuildWindow());
					}
				}
				break;
			case 1:
				this.mapfrommain = false;
				if (MainManager.instance.cursor != null)
				{
					MainManager.instance.cursor.sortingOrder = 10;
					if (this.secondoption == -1)
					{
						MainManager.instance.cursor.transform.localPosition = Vector3.Lerp(MainManager.instance.cursor.transform.localPosition, new Vector3(-7f, 3.35f - (float)MainManager.listcursor * 0.9f, 10f), MainManager.TieFramerate(0.2f));
					}
					else
					{
						MainManager.instance.cursor.transform.localPosition = Vector3.Lerp(MainManager.instance.cursor.transform.localPosition, new Vector3(3.9f, 0.5f - (float)this.secondoption * 1.3f, 10f), MainManager.TieFramerate(0.2f));
					}
				}
				this.IconAnim(new int[]
				{
					9,
					10
				});
				if (MainManager.instance.inputcooldown <= 0f)
				{
					if (MainManager.GetKey(3, false) && this.secondoption == -1)
					{
						MainManager.listY = -1;
						this.option++;
						if (this.option >= this.maxoptions)
						{
							this.option = 0;
						}
						MainManager.ResetList();
						MainManager.SetUpList(this.option, true, false);
						this.UpdateText();
						MainManager.PlayScrollSound();
					}
					else if (MainManager.GetKey(2, false) && this.secondoption == -1)
					{
						MainManager.listY = -1;
						this.option--;
						if (this.option < 0)
						{
							this.option = this.maxoptions - 1;
						}
						MainManager.ResetList();
						MainManager.SetUpList(this.option, true, false);
						this.UpdateText();
						MainManager.PlayScrollSound();
					}
				}
				if (MainManager.instance.itemlist != null)
				{
					num = MainManager.instance.option;
					bool flag = false;
					if (MainManager.instance.inputcooldown <= 0f)
					{
						if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
						{
							if (this.secondoption == -1)
							{
								MainManager.instance.UpdateList(true, this.skip, true);
								flag = (MainManager.instance.option != num);
							}
							else
							{
								this.secondoption--;
								if (this.secondoption < 0)
								{
									this.secondoption = this.maxsecond - 1;
								}
							}
							if (MainManager.GetKey(7, true) && flag)
							{
								MainManager.PlaySound("BadgeEquip");
								int value = MainManager.instance.items[this.option][num];
								MainManager.instance.items[this.option][num] = MainManager.instance.items[this.option][MainManager.instance.option];
								MainManager.instance.items[this.option][MainManager.instance.option] = value;
								MainManager.listY = -1;
							}
							else
							{
								MainManager.PlayScrollSound();
							}
							if (flag)
							{
								this.UpdateText();
							}
						}
						else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
						{
							if (this.secondoption == -1)
							{
								MainManager.instance.UpdateList(false, this.skip, true);
								flag = (MainManager.instance.option != num);
							}
							else
							{
								this.secondoption++;
								if (this.secondoption >= this.maxsecond)
								{
									this.secondoption = 0;
								}
							}
							if (MainManager.GetKey(7, true) && flag)
							{
								MainManager.PlaySound("BadgeEquip");
								int value2 = MainManager.instance.items[this.option][num];
								MainManager.instance.items[this.option][num] = MainManager.instance.items[this.option][MainManager.instance.option];
								MainManager.instance.items[this.option][MainManager.instance.option] = value2;
								MainManager.listY = -1;
							}
							else
							{
								MainManager.PlayScrollSound();
							}
							if (flag)
							{
								this.UpdateText();
							}
						}
						else
						{
							MainManager.ResetKeyHold();
						}
					}
				}
				if (MainManager.instance.inputcooldown <= 0f && MainManager.GetKey(4, false) && !MainManager.GetKey(7, true) && MainManager.instance.items[this.option].ToArray().Length != 0 && MainManager.instance.inputcooldown <= 0f)
				{
					MainManager.instance.inputcooldown = 5f;
					MainManager.PlaySound("Confirm", 10);
					int id = MainManager.instance.items[this.option].ToArray()[MainManager.instance.option];
					this.itemuse = MainManager.GetItemUse(id, 0);
					if (this.secondoption == -1)
					{
						bool flag2 = true;
						bool flag3 = false;
						for (int i = 0; i < this.itemuse.usetype.Length; i++)
						{
							if ((this.itemuse.usetype[i] == MainManager.ItemUsage.None && this.option == 0) || (this.itemuse.usetype[i] == MainManager.ItemUsage.Battle || (this.itemuse.usetype[i] == MainManager.ItemUsage.ChargeUp && this.itemuse.usetype.Length == 1)) || (this.option == 1 && MainManager.battle != null))
							{
								flag2 = false;
							}
							else if (this.itemuse.usetype[i] == MainManager.ItemUsage.None && this.option == 1)
							{
								flag3 = true;
							}
						}
						if (flag2)
						{
							if (flag3)
							{
								this.UseKeyItem();
							}
							else
							{
								this.secondoption = 0;
							}
						}
						else
						{
							MainManager.PlayBuzzer();
						}
					}
					else
					{
						for (int j = 0; j < this.itemuse.usetype.Length; j++)
						{
							this.ItemParticles(this.itemuse);
							MainManager.DoItemEffect(this.itemuse.usetype[j], this.itemuse.values[j], new int?(this.secondoption));
						}
						this.secondoption = -1;
						MainManager.instance.items[this.option].RemoveAt(MainManager.instance.option);
						MainManager.instance.maxoptions--;
						MainManager.listY = -1;
						if (MainManager.instance.option >= MainManager.instance.maxoptions)
						{
							MainManager.instance.UpdateList(MainManager.Directions.Up);
						}
						this.UpdateText();
					}
				}
				break;
			case 2:
				if (MainManager.instance.cursor != null)
				{
					MainManager.instance.cursor.transform.localPosition = Vector3.Lerp(MainManager.instance.cursor.transform.localPosition, new Vector3(-7.7f, 3.35f - (float)MainManager.listcursor * 0.9f, 10f), MainManager.TieFramerate(0.2f));
				}
				if (MainManager.instance.inputcooldown <= 0f)
				{
					if (MainManager.GetKey(3, false))
					{
						this.option++;
						if (this.option >= this.maxoptions)
						{
							this.option = 0;
						}
						if (this.page == 2)
						{
							MainManager.ResetList();
						}
						this.UpdateText();
						MainManager.PlayScrollSound();
					}
					else if (MainManager.GetKey(2, false))
					{
						this.option--;
						if (this.option < 0)
						{
							this.option = this.maxoptions - 1;
						}
						if (this.page == 2)
						{
							MainManager.ResetList();
						}
						this.UpdateText();
						MainManager.PlayScrollSound();
					}
					num = MainManager.instance.option;
					if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
					{
						MainManager.instance.UpdateList(true, this.skip);
						if (MainManager.instance.option != num)
						{
							this.UpdateText();
						}
					}
					else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
					{
						MainManager.instance.UpdateList(false, this.skip);
						if (MainManager.instance.option != num)
						{
							this.UpdateText();
						}
					}
					else
					{
						MainManager.ResetKeyHold();
					}
					if (MainManager.GetKey(4, false) && MainManager.listvar.Length != 0 && this.page < 2 && MainManager.instance.inputcooldown <= 0f)
					{
						MainManager.instance.inputcooldown = 5f;
						int[] array = MainManager.instance.badges.ToArray()[MainManager.listvar[MainManager.instance.option]];
						if (array[1] > -2)
						{
							MainManager.PlaySound("BadgeDequip");
							MainManager.listY = -1;
							MainManager.instance.bp += Mathf.Clamp(Convert.ToInt32(MainManager.badgedata[array[0], 2]), 0, MainManager.instance.flags[613] ? 1 : 999);
							MainManager.instance.badges.ToArray()[MainManager.listvar[MainManager.instance.option]][1] = -2;
							this.UpdateText();
							MainManager.ApplyBadges();
						}
						else if (MainManager.instance.bp >= Mathf.Clamp(Convert.ToInt32(MainManager.badgedata[array[0], 2]), 0, MainManager.instance.flags[613] ? 1 : 999))
						{
							MainManager.listY = -1;
							MainManager.PlaySound("BadgeEquip");
							MainManager.instance.bp -= Mathf.Clamp(Convert.ToInt32(MainManager.badgedata[array[0], 2]), 0, MainManager.instance.flags[613] ? 1 : 999);
							if (Convert.ToBoolean(MainManager.badgedata[array[0], 3]))
							{
								MainManager.instance.badges.ToArray()[MainManager.listvar[MainManager.instance.option]][1] = -1;
							}
							else
							{
								MainManager.instance.badges.ToArray()[MainManager.listvar[MainManager.instance.option]][1] = MainManager.instance.playerdata[this.option].trueid;
							}
							this.UpdateText();
							MainManager.ApplyBadges();
						}
						else
						{
							MainManager.PlayBuzzer();
						}
					}
					else if (MainManager.GetKey(7, false) && MainManager.instance.inputcooldown <= 0f)
					{
						MainManager.instance.inputcooldown = 5f;
						MainManager.listY = -1;
						bool flag4 = this.page != 0;
						this.page++;
						if (this.page == 1)
						{
							MainManager.instance.multilist = MainManager.GetEquippedBadgeIDs();
						}
						if (this.page > 2)
						{
							this.page = 0;
						}
						if (!flag4)
						{
							this.lastmedal = MainManager.SaveList();
						}
						MainManager.ResetList();
						if (this.page == 0)
						{
							MainManager.LoadList(this.lastmedal);
						}
						this.UpdateText();
						MainManager.PlaySound("PageFlip");
					}
					else if (MainManager.GetKey(9) && this.page == 0 && MainManager.instance.badges.Count > 0 && MainManager.instance.inputcooldown <= 0f)
					{
						MainManager.instance.inputcooldown = 5f;
						int[][] array2 = MainManager.instance.badges.ToArray();
						for (int k = 0; k < MainManager.instance.badges.Count; k++)
						{
							array2[k][1] = -2;
						}
						if (MainManager.instance.bp != MainManager.instance.maxbp)
						{
							MainManager.instance.bp = MainManager.instance.maxbp;
						}
						MainManager.instance.badges = new List<int[]>(array2);
						MainManager.PlaySound("BadgeDequip");
						MainManager.listY = -1;
						MainManager.ApplyBadges();
						this.UpdateText();
					}
				}
				break;
			case 3:
				if (this.maxoptions == 4)
				{
					this.IconAnim(new int[]
					{
						1,
						2,
						3,
						4
					});
				}
				else
				{
					this.IconAnim(new int[]
					{
						1,
						2,
						3,
						4,
						5
					});
				}
				for (int l = 0; l < 2; l++)
				{
					if (this.sprites[6 + l] != null)
					{
						this.sprites[6 + l].transform.localPosition = new Vector3((PauseMenu.libraryarrow.x + Mathf.Abs(Mathf.Sin(Time.time * 3.75f) * 0.15f)) * (float)((l == 0) ? 1 : -1), this.sprites[6 + l].transform.localPosition.y, this.sprites[6 + l].transform.localPosition.z);
					}
				}
				if (MainManager.instance.cursor != null)
				{
					MainManager.instance.cursor.transform.localPosition = Vector3.Lerp(MainManager.instance.cursor.transform.localPosition, new Vector3(-6.55f, 2.4f - (float)MainManager.listcursor * 0.7f, 10f), MainManager.TieFramerate(0.2f));
				}
				if (this.boxes[3] != null)
				{
					if (this.secondoption > -1)
					{
						this.boxes[3].transform.localPosition = Vector3.Lerp(this.boxes[3].transform.localPosition, Vector3.zero, MainManager.TieFramerate(0.2f));
					}
					else
					{
						this.boxes[3].transform.localPosition = Vector3.Lerp(this.boxes[3].transform.localPosition, PauseMenu.pagehide, MainManager.TieFramerate(0.2f));
					}
				}
				if (MainManager.instance.inputcooldown <= 0f)
				{
					if (this.secondoption == -1)
					{
						if (MainManager.GetKey(3, false))
						{
							this.option++;
							if (this.option >= this.maxoptions)
							{
								this.option = 0;
							}
							MainManager.ResetList();
							this.UpdateText();
							MainManager.PlaySound("PageFlip");
						}
						else if (MainManager.GetKey(2, false))
						{
							this.option--;
							if (this.option < 0)
							{
								this.option = this.maxoptions - 1;
							}
							MainManager.ResetList();
							this.UpdateText();
							MainManager.PlaySound("PageFlip");
						}
						num = MainManager.instance.option;
						if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
						{
							MainManager.instance.UpdateList(true, this.skip);
							if (MainManager.instance.option != num)
							{
								this.UpdateText();
							}
						}
						else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
						{
							MainManager.instance.UpdateList(false, this.skip);
							if (MainManager.instance.option != num)
							{
								this.UpdateText();
							}
						}
						else
						{
							MainManager.ResetKeyHold();
						}
						if (MainManager.GetKey(4, false) && this.option != 3 && this.option != 3 && MainManager.instance.inputcooldown <= 0f)
						{
							MainManager.instance.inputcooldown = 5f;
							if (this.option == 4 || (this.option <= 1 && MainManager.instance.librarystuff[this.option, MainManager.listvar[MainManager.instance.option]]) || (this.option == 2 && MainManager.instance.librarystuff[this.option, MainManager.instance.option]) || (this.option != 2 && this.option != 1 && this.option != 0 && MainManager.instance.librarystuff[this.option, MainManager.libraryorder[this.option, MainManager.listvar[MainManager.instance.option]]]))
							{
								this.secondoption = 0;
								this.UpdateText();
								MainManager.PlaySound("PageFlip", -1, 0.7f, 1f);
							}
							else
							{
								MainManager.PlayBuzzer();
							}
						}
					}
					else
					{
						if (this.maxsecond > 1)
						{
							this.sprites[7].enabled = (this.secondoption > 0);
							this.sprites[6].enabled = (this.secondoption < this.maxsecond - 1);
							if (MainManager.GetKey(3, false))
							{
								this.secondoption++;
								if (this.secondoption >= this.maxsecond)
								{
									this.secondoption = 0;
								}
								this.UpdateText();
								MainManager.PlaySound("PageFlip");
							}
							else if (MainManager.GetKey(2, false))
							{
								this.secondoption--;
								if (this.secondoption < 0)
								{
									this.secondoption = this.maxsecond - 1;
								}
								this.UpdateText();
								MainManager.PlaySound("PageFlip");
							}
						}
						else
						{
							this.sprites[6].enabled = false;
							this.sprites[7].enabled = false;
						}
						if (MainManager.GetKey(4, false))
						{
							this.secondoption = -1;
							MainManager.PlaySound("Cancel");
						}
					}
				}
				break;
			case 4:
				if (MainManager.instance.cursor != null)
				{
					MainManager.instance.cursor.transform.localPosition = Vector3.Lerp(MainManager.instance.cursor.transform.localPosition, new Vector3(-5.5f, 1.75f - (float)MainManager.listcursor * 0.7f, 10f), MainManager.TieFramerate(0.2f));
				}
				if (MainManager.instance.inputcooldown <= 0f)
				{
					if (MainManager.GetKey(0, false) || MainManager.KeyHold(MainManager.Directions.Up))
					{
						MainManager.instance.UpdateList(true, this.skip);
						if (num != MainManager.listlow)
						{
							this.UpdateText();
						}
					}
					else if (MainManager.GetKey(1, false) || MainManager.KeyHold(MainManager.Directions.Down))
					{
						MainManager.instance.UpdateList(false, this.skip);
						if (num != MainManager.listlow)
						{
							this.UpdateText();
						}
					}
					else
					{
						MainManager.ResetKeyHold();
					}
					num = MainManager.listlow;
					if (MainManager.GetKey(4, false) && MainManager.instance.inputcooldown <= 0f)
					{
						MainManager.instance.inputcooldown = 5f;
						int num2 = MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]];
						if (num2 <= 36)
						{
							if (num2 != 35)
							{
								if (num2 == 36)
								{
									base.StartCoroutine(MainManager.SetText("|spd,0||boxstyle,4||halfline||center|" + MainManager.menutext[113] + "|fwait,0.1||prompt,menu,1,2,114,115,5,6|", true, Vector3.zero, null, null));
									MainManager.maintextbox.GetComponent<Animator>().Play("4");
									this.ChangeWindow(-1);
								}
							}
							else
							{
								this.ChangeWindow(5);
							}
						}
						else if (num2 != 231)
						{
							if (num2 == 256)
							{
								if (MainManager.SoundIsPlaying("ATKSuccess") == -1)
								{
									MainManager.PlaySound("ATKSuccess");
								}
								MainManager.joybinds = new int[]
								{
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55
								};
							}
						}
						else
						{
							this.ChangeWindow(7);
						}
					}
					if (MainManager.GetKey(9, false) && MainManager.instance.inputcooldown <= 0f)
					{
						MainManager.instance.inputcooldown = 5f;
						for (int m = 0; m < MainManager.music.Length; m++)
						{
							if (MainManager.instance.inmusicrange == -1)
							{
								MainManager.music[m].volume = MainManager.musicvolume;
							}
						}
						for (int n = 0; n < MainManager.sounds.Length; n++)
						{
							MainManager.sounds[n].volume = MainManager.soundvolume;
						}
						if (MainManager.map != null)
						{
							MainManager.map.RefreshSoundVolume();
						}
						MainManager.MainCamera.GetComponent<FXAA>().enabled = this.taliasing;
						MainManager.RefreshRenderTex();
						MainManager.nowindeffect = this.nwind;
						MainManager.enableoutline = this.outline;
						MainManager.particlelevel = this.particle;
						MainManager.analog = this.analog;
						MainManager.usejoystick = this.joystick;
						MainManager.forcejoystick = this.joystickid;
						MainManager.keepmusicafterbattle = this.keepmusic;
						MainManager.PlaySound("Cancel", 10);
						if (this.calledfrommain)
						{
							this.PrepareExit();
						}
						else
						{
							this.ResetTempSettings();
							this.ChangeWindow(0);
						}
					}
					if (MainManager.GetKey(3, false) || MainManager.GetKey(2, false))
					{
						MainManager.listY = -1;
						int num2 = MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]];
						if (num2 <= 183)
						{
							if (num2 > 116)
							{
								if (num2 <= 147)
								{
									if (num2 == 140)
									{
										this.SettingsToggleSound();
										MainManager.nowindeffect = !MainManager.nowindeffect;
										this.UpdateText();
										break;
									}
									if (num2 != 147)
									{
										break;
									}
								}
								else
								{
									switch (num2)
									{
									case 156:
										break;
									case 157:
										this.SettingsToggleSound();
										if (MainManager.GetKey(3, false))
										{
											this.joystick++;
											if (this.joystick > 5)
											{
												this.joystick = 0;
											}
										}
										else if (MainManager.GetKey(2, false))
										{
											this.joystick--;
											if (this.joystick < 0)
											{
												this.joystick = 5;
											}
										}
										if (this.joystick == 3)
										{
											this.joystickid = -1;
										}
										else if (this.joystick == 4 || this.joystick >= 5)
										{
											this.joystickid = 0;
										}
										this.UpdateText();
										goto IL_25ED;
									case 158:
									case 159:
										goto IL_25ED;
									case 160:
										goto IL_16E9;
									default:
										if (num2 != 183)
										{
											goto IL_25ED;
										}
										this.SettingsToggleSound();
										this.vsyc = ((this.vsyc == 1) ? 0 : 1);
										this.UpdateText();
										goto IL_25ED;
									}
								}
								this.SettingsToggleSound();
								if (MainManager.GetKey(3, false))
								{
									if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 147)
									{
										MainManager.enableoutline++;
										if (MainManager.enableoutline > 2)
										{
											MainManager.enableoutline = 0;
										}
									}
									else
									{
										MainManager.particlelevel++;
										if (MainManager.particlelevel > 2)
										{
											MainManager.particlelevel = 0;
										}
									}
								}
								else if (MainManager.GetKey(2, false))
								{
									if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 147)
									{
										MainManager.enableoutline--;
										if (MainManager.enableoutline < 0)
										{
											MainManager.enableoutline = 2;
										}
									}
									else
									{
										MainManager.particlelevel--;
										if (MainManager.particlelevel < 0)
										{
											MainManager.particlelevel = 2;
										}
									}
								}
								this.UpdateText();
								break;
							}
							switch (num2)
							{
							case 28:
								this.SettingsToggleSound();
								if (MainManager.GetKey(3, false))
								{
									this.resolutionid++;
									if (this.resolutionid >= MainManager.resolution.Length)
									{
										this.resolutionid = 0;
									}
								}
								else if (MainManager.GetKey(2, false))
								{
									this.resolutionid--;
									if (this.resolutionid < 0)
									{
										this.resolutionid = MainManager.resolution.Length - 1;
									}
								}
								this.UpdateText();
								goto IL_25ED;
							case 29:
								this.SettingsToggleSound();
								this.fulls = !this.fulls;
								this.UpdateText();
								goto IL_25ED;
							case 30:
							case 31:
								this.SettingsToggleSound();
								if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 30)
								{
									this.lowshadow = !this.lowshadow;
								}
								else
								{
									this.lowtex = !this.lowtex;
								}
								this.UpdateText();
								goto IL_25ED;
							case 32:
								this.SettingsToggleSound();
								if (MainManager.GetKey(3, false))
								{
									MainManager.downsample++;
									if (MainManager.downsample >= MainManager.downsamples.Length)
									{
										MainManager.downsample = 0;
									}
								}
								else if (MainManager.GetKey(2, false))
								{
									MainManager.downsample--;
									if (MainManager.downsample < 0)
									{
										MainManager.downsample = MainManager.downsamples.Length - 1;
									}
								}
								this.UpdateText();
								goto IL_25ED;
							case 33:
							case 34:
								break;
							default:
								if (num2 == 80)
								{
									this.SettingsToggleSound();
									if (MainManager.GetKey(3, false))
									{
										this.fps++;
										if (this.fps >= 2)
										{
											this.fps = 0;
										}
									}
									else if (MainManager.GetKey(2, false))
									{
										this.fps--;
										if (this.fps < 0)
										{
											this.fps = 1;
										}
									}
									this.UpdateText();
									goto IL_25ED;
								}
								if (num2 != 116)
								{
									goto IL_25ED;
								}
								this.SettingsToggleSound();
								MainManager.MainCamera.GetComponent<FXAA>().enabled = !MainManager.MainCamera.GetComponent<FXAA>().enabled;
								MainManager.RefreshRenderTex();
								this.UpdateText();
								goto IL_25ED;
							}
							IL_16E9:
							if (MainManager.GetKey(3, false))
							{
								if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 160)
								{
									this.dvolume = Mathf.Clamp01(this.dvolume + 0.1f);
									MainManager.PlayBleep(Resources.Load<AudioClip>("Audio/Sounds/Dialogue/Dialogue0"), 1f, 1f, 0);
								}
								else if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 33)
								{
									this.mvolume = Mathf.Clamp01(this.mvolume + 0.1f);
									for (int num3 = 0; num3 < MainManager.music.Length; num3++)
									{
										if (MainManager.instance.inmusicrange == -1)
										{
											MainManager.music[num3].volume = this.mvolume;
										}
									}
								}
								else
								{
									this.svolume = Mathf.Clamp01(this.svolume + 0.1f);
									if (MainManager.map != null)
									{
										MainManager.map.RefreshSoundVolume(this.svolume);
									}
								}
							}
							else if (MainManager.GetKey(2, false))
							{
								if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 160)
								{
									this.dvolume = Mathf.Clamp01(this.dvolume - 0.1f);
									MainManager.PlayBleep(Resources.Load<AudioClip>("Audio/Sounds/Dialogue/Dialogue0"), 1f, 1f, 0);
								}
								else if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] == 33)
								{
									this.mvolume = Mathf.Clamp01(this.mvolume - 0.1f);
									for (int num4 = 0; num4 < MainManager.music.Length; num4++)
									{
										if (MainManager.instance.inmusicrange == -1)
										{
											MainManager.music[num4].volume = this.mvolume;
										}
									}
								}
								else
								{
									this.svolume = Mathf.Clamp01(this.svolume - 0.1f);
									if (MainManager.map != null)
									{
										MainManager.map.RefreshSoundVolume(this.svolume);
									}
								}
							}
							if (MainManager.settingsindex[MainManager.listvar[MainManager.instance.option]] != 160)
							{
								this.SettingsToggleSound();
							}
							this.UpdateText();
						}
						else if (num2 <= 255)
						{
							if (num2 <= 239)
							{
								if (num2 != 222)
								{
									if (num2 == 239)
									{
										this.SettingsToggleSound();
										MainManager.keepmusicafterbattle = !MainManager.keepmusicafterbattle;
										this.UpdateText();
									}
								}
								else
								{
									string[] array3 = MainManager.Controllers();
									if (this.joystick == 4 || (array3.Length != 0 && this.joystickid < array3.Length))
									{
										this.SettingsToggleSound();
										if (MainManager.GetKey(3, false))
										{
											this.joystickid++;
											if (this.joystick == 3 && this.joystickid >= MainManager.Controllers().Length)
											{
												this.joystickid = -1;
											}
											else if (this.joystick == 4 && this.joystickid >= MainManager.preconfigjoy.Length)
											{
												this.joystickid = 0;
											}
										}
										else if (MainManager.GetKey(2, false))
										{
											this.joystickid--;
											if (this.joystick == 3 && this.joystickid < -1)
											{
												this.joystickid = MainManager.Controllers().Length - 1;
											}
											else if (this.joystick == 4 && this.joystickid < 0)
											{
												this.joystickid = MainManager.preconfigjoy.Length - 1;
											}
										}
									}
									else
									{
										this.joystickid = -1;
									}
									this.UpdateText();
								}
							}
							else if (num2 != 245)
							{
								if (num2 == 255)
								{
									this.SettingsToggleSound();
									this.pauseunfocus = !this.pauseunfocus;
									this.UpdateText();
								}
							}
							else
							{
								this.SettingsToggleSound();
								this.mash = !this.mash;
								this.UpdateText();
							}
						}
						else if (num2 <= 261)
						{
							if (num2 != 256)
							{
								if (num2 == 261)
								{
									this.SettingsToggleSound();
									this.monoaudio = !this.monoaudio;
									this.UpdateText();
								}
							}
							else
							{
								this.SettingsToggleSound();
								MainManager.joybinds = new int[]
								{
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55,
									-55
								};
							}
						}
						else if (num2 != 270)
						{
							if (num2 == 282)
							{
								this.SettingsToggleSound();
								this.snap = !this.snap;
								this.UpdateText();
							}
						}
						else
						{
							this.SettingsToggleSound();
							if (MainManager.GetKey(3, false))
							{
								this.analog++;
								if (this.analog > 2)
								{
									this.analog = 0;
								}
							}
							else if (MainManager.GetKey(2, false))
							{
								this.analog--;
								if (this.analog < 0)
								{
									this.analog = 2;
								}
							}
							this.UpdateText();
						}
					}
				}
				break;
			case 5:
				if (MainManager.instance.cursor != null)
				{
					MainManager.instance.cursor.transform.localPosition = Vector3.Lerp(MainManager.instance.cursor.transform.localPosition, new Vector3(-1f, 1.75f - (float)MainManager.listcursor * 0.7f, 10f), MainManager.TieFramerate(0.2f));
				}
				if (this.boxes != null && this.boxes.Length > 3)
				{
					this.boxes[3].shrink = !this.getkey;
				}
				if (!this.getkey && this.keycooldown <= 0f)
				{
					if (MainManager.GetKey(0, false))
					{
						for (int num5 = 0; num5 < this.skip; num5++)
						{
							MainManager.instance.UpdateList(MainManager.Directions.Up);
						}
						if (num != MainManager.listlow)
						{
							this.UpdateText();
						}
					}
					else if (MainManager.GetKey(1, false))
					{
						for (int num6 = 0; num6 < this.skip; num6++)
						{
							MainManager.instance.UpdateList(MainManager.Directions.Down);
						}
						if (num != MainManager.listlow)
						{
							this.UpdateText();
						}
					}
					num = MainManager.listlow;
					if (MainManager.instance.inputcooldown <= 0f && this.keycooldown <= 0f)
					{
						if (MainManager.GetKey(4, false))
						{
							MainManager.instance.inputcooldown = 5f;
							this.keycooldown = 5f;
							if (this.windowid == 5)
							{
								base.StartCoroutine(this.GetKey());
							}
						}
						else if (Input.GetKeyDown(KeyCode.F1))
						{
							if (this.windowid == 5)
							{
								InputIO.SetDefaultKeys();
							}
							else
							{
								for (int num7 = 0; num7 < MainManager.joybinds.Length; num7++)
								{
									MainManager.joybinds[num7] = -55;
								}
							}
							this.ChangeWindow(this.windowid);
							this.keycooldown = 5f;
						}
					}
				}
				if (this.keycooldown > 0f)
				{
					this.keycooldown -= MainManager.TieFramerate(1f);
				}
				break;
			case 6:
				if (this.sprites != null && this.sprites.Length != 0 && this.sprites[0] != null)
				{
					Vector3 vector = Vector3.zero;
					Vector3 normalized = new Vector3(-InputIO.JoyStick(0), 0f, InputIO.JoyStick(1)).normalized;
					if (MainManager.GetKey(0, true))
					{
						vector += Vector3.back;
					}
					else if (MainManager.GetKey(1, true))
					{
						vector += Vector3.forward;
					}
					if (MainManager.GetKey(2, true))
					{
						vector += Vector3.right;
					}
					else if (MainManager.GetKey(3, true))
					{
						vector += -Vector3.right;
					}
					if (normalized.magnitude > 0f)
					{
						vector = normalized.normalized;
					}
					if (vector != Vector3.zero)
					{
						if (this.option != -1)
						{
							this.option = -1;
							this.UpdateText();
						}
						this.sprites[0].transform.localPosition = this.sprites[0].transform.localPosition + vector.normalized * MainManager.TieFramerate(0.1f);
					}
					else
					{
						if (this.option == -1)
						{
							List<Transform> list = new List<Transform>();
							for (int num8 = 0; num8 < MainManager.areanames.Length; num8++)
							{
								if (this.sprites[num8 + 1] != null)
								{
									list.Add(this.sprites[num8 + 1].transform);
								}
							}
							Transform transform = (list.Count == 0) ? null : (from point in list
							orderby Vector3.Distance(this.sprites[0].transform.position, point.position)
							select point).ToList<Transform>().ToArray()[0];
							bool flag5 = false;
							for (int num9 = 0; num9 < MainManager.areanames.Length; num9++)
							{
								if (transform != null && this.sprites[num9 + 1] != null && this.sprites[num9 + 1].transform == transform && Vector3.Distance(this.sprites[0].transform.position, transform.position) < 1f)
								{
									this.option = num9;
									flag5 = true;
									break;
								}
							}
							if (!flag5)
							{
								this.option = -2;
							}
							else
							{
								this.secondoption = 0;
								this.UpdateText();
							}
						}
						if (this.option >= 0)
						{
							this.sprites[0].transform.localPosition = Vector3.Lerp(this.sprites[0].transform.localPosition, this.sprites[this.option + 1].transform.localPosition, MainManager.TieFramerate(0.1f));
						}
						if (MainManager.GetKey(9) || MainManager.GetKey(7))
						{
							Renderer[] array4 = this.mapicons.ToArray();
							for (int num10 = 0; num10 < array4.Length; num10++)
							{
								array4[num10].enabled = !array4[num10].enabled;
							}
						}
						if (this.maxsecond > 1 && vector == Vector3.zero)
						{
							int num11 = this.secondoption;
							if (MainManager.GetKey(6))
							{
								this.secondoption--;
								if (this.secondoption < 0)
								{
									this.secondoption = this.maxsecond - 1;
								}
								if (num11 != this.secondoption)
								{
									this.UpdateText();
									MainManager.PlaySound("PageFlip");
								}
							}
							else if (MainManager.GetKey(4))
							{
								this.secondoption++;
								if (this.secondoption >= this.maxsecond)
								{
									this.secondoption = 0;
								}
								if (num11 != this.secondoption)
								{
									this.UpdateText();
									MainManager.PlaySound("PageFlip");
								}
							}
						}
					}
					this.sprites[0].transform.localPosition = new Vector3(Mathf.Clamp(this.sprites[0].transform.localPosition.x, -6.5f, 6.5f), 2f, Mathf.Clamp(this.sprites[0].transform.localPosition.z, -2.75f, 3.65f));
				}
				break;
			case 7:
				if (MainManager.instance.inputcooldown <= 0f)
				{
					if (this.option < 10)
					{
						if (Input.GetKeyDown(InputIO.keys[6]))
						{
							this.option++;
							this.UpdateText();
						}
						else
						{
							this.JoyBinding();
						}
					}
					else if (MainManager.GetKey(2))
					{
						this.SettingsToggleSound();
						this.secondoption--;
						if (this.secondoption < 0)
						{
							this.secondoption = PauseMenu.joyicons.Length - 1;
						}
						MainManager.joyid = PauseMenu.joyicons[this.secondoption];
						this.UpdateText();
					}
					else if (MainManager.GetKey(3))
					{
						this.SettingsToggleSound();
						this.secondoption++;
						if (this.secondoption >= PauseMenu.joyicons.Length)
						{
							this.secondoption = 0;
						}
						MainManager.joyid = PauseMenu.joyicons[this.secondoption];
						this.UpdateText();
					}
					else if (MainManager.GetKey(4) || MainManager.GetKey(5) || MainManager.GetKey(8))
					{
						this.joystickid = PauseMenu.joyicons[this.secondoption];
						MainManager.instance.inputcooldown = 10f;
						this.ApplySettings();
						this.ChangeWindow(4);
					}
				}
				break;
			}
			IL_25ED:
			if (this.windowid != 7)
			{
				if (MainManager.GetKey(8, false) && this.windowid != 4 && this.windowid != 5 && !MainManager.AnyKeyButThis(8, false) && MainManager.instance.inputcooldown <= 0f)
				{
					this.PrepareExit();
					return;
				}
				if (MainManager.GetKey(5, false) && !MainManager.AnyKeyButThis(5, false) && MainManager.instance.inputcooldown <= 0f)
				{
					MainManager.instance.inputcooldown = 5f;
					MainManager.PlaySound("Cancel", 10);
					if (this.calledfrommain && this.windowid < 5)
					{
						this.ApplySettings();
						this.PrepareExit();
						StartMenu startMenu = Object.FindObjectOfType<StartMenu>();
						MainManager.instance.maxoptions = startMenu.selections.Length;
						startMenu.SetButtons();
						MainManager.instance.option = 0;
						return;
					}
					if (this.windowid == 6)
					{
						this.tempanim.Play("Close");
						MainManager.PlaySound("PageFlip", -1, 0.65f, 1f);
						Object.Destroy(this.tempanim.transform.GetChild(this.tempanim.transform.childCount - 1).gameObject);
						Object.Destroy(this.tempanim.gameObject, 0.25f);
						this.enemydata = Resources.Load<TextAsset>("Data/EnemyData").ToString().Split(new char[]
						{
							'\n'
						});
						this.ChangeWindow(this.mapfrommain ? 0 : 1);
						return;
					}
					if (this.windowid == 4)
					{
						this.ApplySettings();
						this.ChangeWindow(0);
						return;
					}
					if (this.windowid == 5)
					{
						if (this.keycooldown <= 0f && !this.getkey)
						{
							InputIO.LoadSettings(true);
							this.ChangeWindow(4);
							return;
						}
					}
					else
					{
						if (this.windowid == 0)
						{
							this.PrepareExit();
							return;
						}
						if (this.windowid != 5)
						{
							if (this.secondoption != -1)
							{
								this.secondoption = -1;
								return;
							}
							this.ChangeWindow(0);
						}
					}
				}
			}
		}
	}

	// Token: 0x06000702 RID: 1794 RVA: 0x0005CD58 File Offset: 0x0005AF58
	private void JoyBinding()
	{
		int joystickRaw = InputIO.GetJoystickRaw();
		if (joystickRaw != -55)
		{
			if ((this.option < 4 && joystickRaw >= 0) || (this.option >= 4 && joystickRaw < 0))
			{
				MainManager.PlayBuzzer();
				MainManager.instance.inputcooldown = 15f;
				return;
			}
			for (int i = 0; i < MainManager.joybinds.Length; i++)
			{
				if (MainManager.joybinds[i] == joystickRaw)
				{
					MainManager.PlayBuzzer();
					MainManager.instance.inputcooldown = 15f;
					return;
				}
			}
			MainManager.joybinds[this.option] = joystickRaw;
			MainManager.PlayScrollSound();
			this.option++;
			this.UpdateText();
			MainManager.instance.inputcooldown = 15f;
		}
	}

	// Token: 0x06000703 RID: 1795 RVA: 0x0005CE08 File Offset: 0x0005B008
	private void ItemParticles(MainManager.ItemUse data)
	{
		for (int i = 0; i < data.usetype.Length; i++)
		{
			switch (data.usetype[i])
			{
			case MainManager.ItemUsage.HPRecover:
			case MainManager.ItemUsage.HPRecoverFull:
				MainManager.HealParticle(this.sprites[this.secondoption + 5].transform, Vector3.one, Vector3.back, true);
				break;
			case MainManager.ItemUsage.TPRecover:
			case MainManager.ItemUsage.TPRecoverFull:
				MainManager.HealParticle(this.sprites[8].transform, Vector3.one, Vector3.back, true);
				break;
			case MainManager.ItemUsage.HPRecoverAll:
				for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
				{
					MainManager.HealParticle(this.sprites[j + 5].transform, Vector3.one, Vector3.back, true);
				}
				break;
			}
		}
	}

	// Token: 0x06000704 RID: 1796 RVA: 0x0005CED0 File Offset: 0x0005B0D0
	private void ApplySettings()
	{
		MainManager.fullscreen = this.fulls;
		MainManager.musicvolume = this.mvolume;
		MainManager.soundvolume = this.svolume;
		MainManager.bleepvolume = this.dvolume;
		MainManager.fps = this.fps;
		MainManager.lowshadows = this.lowshadow;
		MainManager.lowtexture = this.lowtex;
		MainManager.resolutionindex = this.resolutionid;
		MainManager.usejoystick = this.joystick;
		MainManager.forcejoystick = this.joystickid;
		MainManager.vsync = this.vsyc;
		MainManager.snapTo8 = this.snap;
		MainManager.mashcommandalt = this.mash;
		MainManager.monoaudio = this.monoaudio;
		MainManager.analog = this.analog;
		MainManager.pauseonfocus = this.pauseunfocus;
		if (MainManager.usejoystick == 5)
		{
			MainManager.forcejoystick = MainManager.joyid;
		}
		MonoBehaviour.print(MainManager.joyid);
		MainManager.ApplySettings();
		InputIO.LoadSettings(true);
	}

	// Token: 0x06000705 RID: 1797 RVA: 0x0005CFBA File Offset: 0x0005B1BA
	private IEnumerator GetKey()
	{
		MonoBehaviour.print("Waiting for key");
		this.gottenkey = KeyCode.None;
		this.getkey = true;
		this.keycooldown = 20f;
		this.countdown = 5;
		float frames = 0f;
		while (this.getkey)
		{
			if (this.countdown <= 0)
			{
				this.getkey = false;
				break;
			}
			frames += MainManager.TieFramerate(1f);
			if (frames >= 60f)
			{
				this.countdown--;
				frames = 0f;
			}
			yield return null;
		}
		if (this.gottenkey != KeyCode.None)
		{
			InputIO.keys[MainManager.instance.option] = this.gottenkey;
			this.ChangeWindow(5);
		}
		this.keycooldown = 20f;
		MonoBehaviour.print("no longer waiting");
		yield break;
	}

	// Token: 0x06000706 RID: 1798 RVA: 0x0005CFC9 File Offset: 0x0005B1C9
	public void ChangeWindow(int id)
	{
		if (id <= 0 && MainManager.instance.cursor != null)
		{
			Object.Destroy(MainManager.instance.cursor.gameObject);
		}
		this.windowid = id;
		base.StartCoroutine(this.BuildWindow());
	}

	// Token: 0x06000707 RID: 1799 RVA: 0x0005D00C File Offset: 0x0005B20C
	private void IconAnim(int[] values)
	{
		if (this.outlined != null)
		{
			this.outlined.transform.parent = this.sprites[values[this.option]].transform;
			this.outlined.transform.localPosition = Vector3.zero;
			this.outlined.transform.localScale = Vector3.one * 1.075f;
			this.outlined.sortingOrder = this.sprites[values[this.option]].sortingOrder + 1;
			this.outlined.enabled = true;
		}
		for (int i = 0; i < values.Length; i++)
		{
			if (this.sprites[values[i]] != null)
			{
				if (this.option == i)
				{
					float num = 0.85f + Mathf.Abs(Mathf.Sin(Time.time * 5f) / 5f);
					this.sprites[values[i]].transform.localScale = new Vector3(num, num, 1f);
					this.sprites[values[i]].transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * 5f) * 10f);
				}
				else
				{
					this.sprites[values[i]].transform.localScale = Vector3.one;
					this.sprites[values[i]].transform.localEulerAngles = Vector3.zero;
				}
			}
		}
	}

	// Token: 0x06000708 RID: 1800 RVA: 0x0005D190 File Offset: 0x0005B390
	private void UpdateDynamicText()
	{
		if (this.dynamictext != null)
		{
			switch (this.windowid)
			{
			case 0:
				if (this.dynamictext.Length >= 5)
				{
					if (this.dynamictext[0] != null)
					{
						this.dynamictext[0].text = MainManager.instance.tp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0');
					}
					if (this.dynamictext[1] != null)
					{
						this.dynamictext[1].text = MainManager.instance.money.ToString().PadLeft(3, '0');
					}
					if (this.dynamictext[5] != null)
					{
						this.dynamictext[5].text = string.Concat(new string[]
						{
							MainManager.instance.clockhour.ToString().PadLeft(3, '0'),
							":",
							MainManager.instance.clockmin.ToString().PadLeft(2, '0'),
							":",
							MainManager.instance.clocksec.ToString().PadLeft(2, '0')
						});
						return;
					}
				}
				break;
			case 1:
				if (this.dynamictext.Length >= 4)
				{
					for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
					{
						if (this.dynamictext[i] != null)
						{
							this.dynamictext[i].text = MainManager.instance.playerdata[i].hp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.playerdata[i].maxhp.ToString().PadLeft(2, '0');
						}
					}
					if (this.dynamictext[3] != null)
					{
						this.dynamictext[3].text = MainManager.instance.tp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0');
						return;
					}
				}
				break;
			case 2:
				if (this.dynamictext.Length >= 6)
				{
					if (this.option < MainManager.instance.playerdata.Length)
					{
						if (this.dynamictext[0] != null)
						{
							this.dynamictext[0].text = MainManager.instance.playerdata[this.option].hp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.playerdata[this.option].maxhp.ToString().PadLeft(2, '0');
						}
						if (this.dynamictext[1] != null)
						{
							this.dynamictext[1].text = MainManager.instance.playerdata[this.option].atk.ToString().PadLeft(2, '0');
						}
						if (this.dynamictext[2] != null)
						{
							this.dynamictext[2].text = MainManager.instance.playerdata[this.option].def.ToString().PadLeft(2, '0');
						}
					}
					if (this.dynamictext[3] != null)
					{
						this.dynamictext[3].text = MainManager.instance.tp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0');
					}
					if (this.dynamictext[4] != null)
					{
						this.dynamictext[4].text = MainManager.instance.partyexp.ToString().PadLeft(3, '0') + "/" + MainManager.instance.neededexp;
					}
					if (this.dynamictext[5] != null)
					{
						this.dynamictext[5].text = MainManager.instance.bp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxbp.ToString().PadLeft(2, '0');
					}
				}
				break;
			default:
				return;
			}
		}
	}

	// Token: 0x06000709 RID: 1801 RVA: 0x0005D5E8 File Offset: 0x0005B7E8
	private void PrepareExit()
	{
		MainManager.instance.inputcooldown = 30f;
		MainManager.RefreshHUDValues();
		if (MainManager.instance.cursor != null)
		{
			Object.Destroy(MainManager.instance.cursor.gameObject);
		}
		MainManager.instance.inlist = false;
		this.exit = true;
		this.canpick = false;
		for (int i = 0; i < this.boxes.Length; i++)
		{
			if (this.boxes[i] != null)
			{
				this.boxes[i].shrink = true;
			}
		}
		if (this.tempanim != null)
		{
			base.CancelInvoke("SetMapLines");
			Object.Destroy(this.tempanim.gameObject);
		}
		if (this.calledfrommain)
		{
			MainManager.instance.maxoptions = 3;
			MainManager.instance.option = 0;
		}
		else
		{
			MainManager.PlaySound("StartClose", -1, 1f, 0.5f);
			if (MainManager.instance.flags[614])
			{
				for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
				{
					if (MainManager.instance.playerdata[j].entity != null)
					{
						MainManager.instance.playerdata[j].entity.SetAnimator();
					}
				}
			}
		}
		if (MainManager.player != null)
		{
			MainManager.player.entity.onground = false;
			for (int k = 0; k < MainManager.instance.playerdata.Length; k++)
			{
				MainManager.instance.playerdata[k].entity.hologram = MainManager.BadgeIsEquipped(83);
				MainManager.instance.playerdata[k].entity.UpdateSpriteMat();
			}
		}
		base.Invoke("DestroyPause", 0.25f);
	}

	// Token: 0x0600070A RID: 1802 RVA: 0x0005D7B8 File Offset: 0x0005B9B8
	private void UpdateText()
	{
		switch (this.windowid)
		{
		case 0:
			MainManager.DestroyText(this.boxes[0].transform);
			base.StartCoroutine(MainManager.SetText("|single|" + MainManager.menutext[50 + this.option], 0, new float?((float)99999), false, false, new Vector3(-5f, 0.1f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
			base.StartCoroutine(MainManager.SetText("|center||single|" + MainManager.menutext[10 + this.option], 0, new float?((float)99999), false, false, new Vector3(0f, 7.5f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
			return;
		case 1:
			MainManager.listammount = 6;
			MainManager.DestroyText(this.boxes[0].transform);
			MainManager.DestroyText(this.boxes[1].transform);
			this.boxes[0].GetComponent<SpriteRenderer>().color = ((this.option == 0) ? new Color(0.75f, 1f, 0.75f) : new Color(1f, 0.75f, 0.75f));
			base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[24 + this.option], 0, new float?((float)99999), false, false, new Vector3(0f, 4.25f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
			if (MainManager.instance.items[this.option].ToArray().Length != 0)
			{
				MainManager.ShowItemList(this.option, Vector2.zero, false, false);
				MainManager.instance.itemlist.parent = this.boxes[0].transform;
				MainManager.instance.itemlist.localScale = Vector3.one;
				MainManager.instance.itemlist.localPosition = new Vector2(-1.65f, 3.05f);
				if (MainManager.instance.cursor == null)
				{
					MainManager.CreateCursor(this.boxes[0].transform);
					MainManager.instance.cursor.transform.parent = this.boxes[0].transform;
				}
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					"|single||singlebreak,",
					10f,
					"|",
					MainManager.itemdata[0, MainManager.instance.items[this.option].ToArray()[MainManager.instance.option], 2]
				}), 0, null, false, false, new Vector3(-5.65f, 0.75f), Vector3.zero, Vector2.one, this.boxes[1].transform, null));
			}
			else
			{
				if (MainManager.instance.itemlist != null)
				{
					Object.Destroy(MainManager.instance.itemlist.gameObject);
					MainManager.instance.inlist = false;
				}
				if (MainManager.instance.cursor != null)
				{
					Object.Destroy(MainManager.instance.cursor.gameObject);
				}
				base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[20 + this.option], 0, new float?((float)99999), false, false, new Vector3(0f, 0f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
			}
			if (this.option == 0)
			{
				this.boxes[3].shrink = false;
				MainManager.DestroyText(this.boxes[3].transform);
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					"|center||halfline||font,2||size,1.3||color,4|",
					MainManager.instance.items[0].ToArray().Length.ToString().PadLeft(2, '0'),
					"/",
					MainManager.instance.maxitems
				}), default(Vector3), this.boxes[3].transform));
				if (this.boxes[4] != null)
				{
					this.boxes[4].shrink = true;
					return;
				}
			}
			else
			{
				this.boxes[3].shrink = true;
				if (this.boxes[4] != null)
				{
					this.boxes[4].shrink = false;
					return;
				}
			}
			break;
		case 2:
			MainManager.listammount = 6;
			MainManager.DestroyText(this.boxes[0].transform);
			MainManager.DestroyText(this.boxes[1].transform);
			this.sprites[14].sprite = MainManager.guisprites[5 + MainManager.instance.playerdata[this.option].trueid];
			base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[(this.page == 0) ? 27 : ((this.page == 1) ? 260 : 61)], 0, new float?((float)99999), false, false, new Vector3(1.15f, 4.25f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
			if (this.page == 0 || this.page == 1)
			{
				if (this.page == 0)
				{
					MainManager.ShowItemList(3, Vector2.zero, false, false);
				}
				else
				{
					MainManager.ShowItemList(32, Vector2.zero, false, false);
				}
				if (MainManager.listvar.Length != 0)
				{
					MainManager.instance.itemlist.parent = this.boxes[0].transform;
					MainManager.instance.itemlist.localScale = Vector3.one;
					MainManager.instance.itemlist.localPosition = new Vector2(-5.65f, 3.05f);
					if (MainManager.instance.cursor == null)
					{
						MainManager.CreateCursor(this.boxes[0].transform);
						MainManager.instance.cursor.transform.parent = this.boxes[0].transform;
					}
					base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
					{
						"|single||singlebreak,",
						10f,
						"|",
						MainManager.badgedata[MainManager.instance.badges.ToArray()[MainManager.listvar[MainManager.instance.option]][0], 1]
					}), 0, null, false, false, new Vector3(-5.65f, 0.75f), Vector3.zero, Vector2.one, this.boxes[1].transform, null));
					return;
				}
				if (MainManager.instance.itemlist != null)
				{
					Object.Destroy(MainManager.instance.itemlist.gameObject);
					MainManager.instance.inlist = false;
				}
				if (MainManager.instance.cursor != null)
				{
					Object.Destroy(MainManager.instance.cursor.gameObject);
				}
				base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[23], 0, new float?((float)99999), false, false, new Vector3(0f, 0f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
				return;
			}
			else if (this.page == 2)
			{
				MainManager.RefreshSkills();
				MainManager.ShowItemList(-this.option - 1, Vector2.zero, false, false);
				MainManager.instance.itemlist.parent = this.boxes[0].transform;
				MainManager.instance.itemlist.localScale = Vector3.one;
				MainManager.instance.itemlist.localPosition = new Vector2(-5.3f, 3.05f);
				if (MainManager.instance.cursor == null)
				{
					MainManager.CreateCursor(this.boxes[0].transform);
					MainManager.instance.cursor.transform.parent = this.boxes[0].transform;
				}
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					"|single||singlebreak,",
					10f,
					"|",
					MainManager.skilldata[MainManager.listvar[MainManager.instance.option], 1]
				}), 0, null, false, false, new Vector3(-5.65f, 0.75f), Vector3.zero, Vector2.one, this.boxes[1].transform, null));
				return;
			}
			break;
		case 3:
			MainManager.listammount = 9;
			MainManager.DestroyText(this.boxes[0].transform);
			MainManager.DestroyText(this.boxes[1].transform);
			MainManager.DestroyText(this.boxes[2].transform);
			MainManager.DestroyText(this.boxes[3].transform);
			if (this.boxes[0].transform.childCount > 5)
			{
				if (this.maxoptions < 5)
				{
					this.boxes[0].transform.GetChild(5).gameObject.SetActive(this.option < 3);
				}
				else if (this.boxes[0].transform.childCount > 6)
				{
					this.boxes[0].transform.GetChild(6).gameObject.SetActive(this.option != 3);
				}
			}
			base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[54 + this.option], 0, new float?((float)99999), false, false, new Vector3(-4.23f, 3.64f), Vector3.zero, Vector2.one, this.boxes[0].transform, null));
			MainManager.ShowItemList((this.option < 4) ? (10 + this.option) : 21, Vector2.zero, false, false);
			MainManager.instance.itemlist.parent = this.boxes[1].transform;
			MainManager.instance.itemlist.localScale = Vector3.one;
			MainManager.instance.itemlist.localPosition = new Vector2(-4.88f, 2.4f);
			switch (this.option)
			{
			case 0:
				if (MainManager.instance.librarystuff[this.option, this.GetLibraryID()])
				{
					this.CreateIcon(MainManager.librarysprites[MainManager.discoveryicons[MainManager.instance.option]]);
				}
				else
				{
					this.CreateIcon(null);
				}
				break;
			case 1:
				if (MainManager.instance.librarystuff[this.option, MainManager.listvar[MainManager.instance.option]])
				{
					this.CreateIcon(MainManager.librarysprites[MainManager.GetEnemyPortrait(MainManager.listvar[MainManager.instance.option])]);
					string[] array = this.enemydata[MainManager.listvar[MainManager.instance.option]].Split(new char[]
					{
						','
					});
					int num = Convert.ToInt32(array[2]) + Convert.ToInt32(array[37]);
					if (MainManager.BadgeIsEquipped(11) || MainManager.instance.flags[614])
					{
						array[1] = string.Concat(Mathf.CeilToInt((float)(Convert.ToInt32(array[1]) + Convert.ToInt32(array[36])) * (MainManager.instance.flags[614] ? 1.15f : 1f)));
						array[2] = string.Concat(num + ((num >= 0 && MainManager.instance.flags[614] && MainManager.instance.flags[300]) ? 1 : 0));
					}
					base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
					{
						"|sort,1|",
						MainManager.menutext[14],
						": ",
						array[1],
						"|line|",
						MainManager.menutext[17],
						": ",
						(Convert.ToInt32(array[2]) >= 0) ? array[2].ToString() : "???",
						"|line|",
						MainManager.menutext[137],
						": ",
						MainManager.instance.enemyencounter[MainManager.listvar[MainManager.instance.option], 0],
						"|line|",
						MainManager.menutext[138],
						": ",
						MainManager.instance.enemyencounter[MainManager.listvar[MainManager.instance.option], 1]
					}), 0, new float?(11f), false, false, new Vector3(-1.75f, -0.85f), Vector3.zero, new Vector2(0.8f, 1f), this.boxes[2].transform, null));
				}
				else
				{
					this.CreateIcon(null);
				}
				break;
			case 2:
				if (MainManager.instance.librarystuff[this.option, this.GetLibraryID()])
				{
					Transform transform = this.CreateIcon(MainManager.itemsprites[0, MainManager.listvar[MainManager.instance.option]]);
					transform.transform.localScale = Vector3.one * 2f;
					MainManager.NewUIObject("back", transform.transform, default(Vector3), Vector3.one * 1.25f, MainManager.librarysprites[10], 2);
					string[] array2 = MainManager.librarydata[this.option, MainManager.instance.option, 0].Split(new char[]
					{
						','
					});
					string text = "";
					for (int i = 0; i < array2.Length; i++)
					{
						if (Convert.ToInt32(array2[i]) == -1)
						{
							text = MainManager.menutext[59];
							break;
						}
						if (MainManager.languageid == 2 || MainManager.languageid == 6 || MainManager.languageid == 4)
						{
							text += "|size,0.55,1|";
						}
						text += MainManager.itemdata[0, Convert.ToInt32(array2[i]), 0];
						if (array2.Length > 1 && i < array2.Length - 1)
						{
							text += "|line||size,0.8,1|+|line|";
						}
					}
					base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
					{
						"|sort,1|",
						MainManager.AsianLang() ? "|size,0.6,1|" : "",
						MainManager.menutext[149],
						"|line|",
						text
					}), 0, new float?(11f), false, false, new Vector3(-1.75f, -0.85f), Vector3.zero, new Vector2(0.8f, 1f), this.boxes[2].transform, null));
				}
				else
				{
					this.CreateIcon(null);
				}
				break;
			case 3:
				base.StartCoroutine(MainManager.SetText(((MainManager.languageid == 6) ? "|size,0.6,1|" : "") + MainManager.librarydata[this.option, MainManager.listvar[MainManager.instance.option], 1], 0, new float?((MainManager.languageid == 4) ? 3.3f : 4f), false, false, new Vector3(-1.75f, -0.85f), Vector3.zero, new Vector2(0.75f, 0.9f), this.boxes[2].transform, null));
				if (MainManager.instance.librarystuff[this.option, this.GetLibraryID()])
				{
					this.CreateIcon(MainManager.librarysprites[MainManager.achiveicons[MainManager.instance.option]]);
				}
				else
				{
					this.CreateIcon(null);
				}
				break;
			case 4:
				if (Mathf.Abs(MainManager.listvar[MainManager.instance.option]) >= 11 && Mathf.Abs(MainManager.listvar[MainManager.instance.option]) <= 17)
				{
					int num2 = Mathf.Abs(MainManager.listvar[MainManager.instance.option]) - 11;
					Transform transform2 = this.CreateIcon(Resources.LoadAll<Sprite>("Sprites/Objects/artifacts")[num2]);
					transform2.localPosition += new Vector3(0f, -1.25f + (float)((num2 == 0) ? -1 : 0), 0f);
					if (MainManager.SaveProgressIcons() < num2 + 1)
					{
						transform2.GetComponent<SpriteRenderer>().color = Color.black;
					}
				}
				else
				{
					if (MainManager.AsianLang())
					{
						base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
						{
							"|line,-0.65|",
							MainManager.menutext[104],
							"|line,1.15|",
							MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[MainManager.instance.option]), 2],
							"|line,1.15|",
							MainManager.menutext[105],
							"|line,1.4||stars,",
							MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[MainManager.instance.option]), 5],
							"||line|",
							MainManager.menutext[171],
							" ",
							MainManager.AsianLang() ? "|line,1.15|" : "",
							MainManager.menutext[(MainManager.listvar[MainManager.instance.option] > 0) ? 85 : 86]
						}), 0, new float?(4f), false, false, new Vector3(-1.75f, -0.85f), Vector3.zero, new Vector2(0.75f, 0.9f), this.boxes[2].transform, null));
					}
					else
					{
						base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
						{
							(MainManager.languageid == 6) ? "|size,0.6,1|" : "",
							MainManager.menutext[104],
							" ",
							MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[MainManager.instance.option]), 2],
							"|line|",
							MainManager.menutext[105],
							" |stars,",
							MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[MainManager.instance.option]), 5],
							"||line|",
							MainManager.menutext[171],
							" ",
							MainManager.AsianLang() ? "|line|" : "",
							MainManager.menutext[(MainManager.listvar[MainManager.instance.option] > 0) ? 85 : 86]
						}), 0, new float?(4f), false, false, new Vector3(-1.75f, -0.85f), Vector3.zero, new Vector2(0.75f, 0.9f), this.boxes[2].transform, null));
					}
					this.CreateIcon(MainManager.librarysprites[Convert.ToInt32(MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[MainManager.instance.option]), 4])]);
				}
				break;
			}
			if (this.secondoption > -1)
			{
				List<string> list = new List<string>();
				string text2;
				if (this.option == 4)
				{
					text2 = MainManager.boardquestdata[Mathf.Abs(MainManager.listvar[MainManager.instance.option]), 1].Replace("|line||line|", "|line||halfline|");
				}
				else if (this.option == 2)
				{
					text2 = this.GetRecipeDesc(MainManager.instance.option);
				}
				else
				{
					text2 = MainManager.librarydata[this.option, MainManager.listvar[MainManager.instance.option], 1];
				}
				if (this.option == 1)
				{
					text2 += "{";
					bool flag = MainManager.AsianLang();
					for (int j = 0; j < 3; j++)
					{
						string[] array3 = MainManager.librarydata[this.option, MainManager.listvar[MainManager.instance.option], 2 + j].Replace("|next|", "\n").Split(new char[]
						{
							'\n'
						});
						string str = string.Concat(new object[]
						{
							"|icon,",
							5 + j,
							",0.5,100| ",
							flag ? "|font,3||size,0.8,1|" : "",
							MainManager.menutext[172].Replace("@", MainManager.menutext[46 + j]),
							"|tab,7||line||halfline|"
						});
						text2 += str;
						if (flag)
						{
							text2 += "|halfline|";
						}
						for (int k = 0; k < array3.Length; k++)
						{
							if (array3[k].Replace("|next|", "").Replace("|librarybreak|", "").Length > 5)
							{
								string[] array4 = array3[k].Replace("|librarybreak|", "@{" + str + this.QuoteMarks(false).ToString()).Split(new char[]
								{
									'@'
								});
								for (int l = 0; l < array4.Length; l++)
								{
									text2 = text2 + this.QuoteMarks(false).ToString() + array4[l] + this.QuoteMarks(true).ToString();
								}
								for (int m = 0; m < 2; m++)
								{
									text2 = text2.Replace(this.QuoteMarks(m == 0).ToString() + this.QuoteMarks(m == 0).ToString() + this.QuoteMarks(m == 0).ToString(), "");
								}
								if (flag)
								{
									text2 = text2.Replace(this.QuoteMarks(false).ToString() + this.QuoteMarks(true).ToString() + this.QuoteMarks(false).ToString(), "");
									text2 = text2.Replace(this.QuoteMarks(true).ToString() + this.QuoteMarks(false).ToString() + this.QuoteMarks(true).ToString(), "");
								}
								if (k < array3.Length - 1)
								{
									text2 += "|line||halfline|";
								}
							}
						}
						if (j == 0)
						{
							text2 += "{";
						}
						else if (j == 1)
						{
							text2 += "}16}";
						}
					}
				}
				string text3 = "";
				for (int n = 0; n < text2.Length; n++)
				{
					if (text2[n] == '}')
					{
						string text4 = "";
						int num3 = n + 1;
						while (num3 < text2.Length && text2[num3] != '}')
						{
							text4 += text2[num3].ToString();
							num3++;
						}
						n += text4.Length + 1;
						if (!MainManager.instance.flags[Convert.ToInt32(text4)])
						{
							break;
						}
						list.Add(text3);
						text3 = "";
					}
					else if (text2[n] == '{')
					{
						list.Add(text3);
						text3 = "";
					}
					else
					{
						text3 += text2[n].ToString();
					}
				}
				list.Add(text3);
				this.maxsecond = list.ToArray().Length;
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					"|sort,25||single||singlebreak,",
					PauseMenu.librarybreak,
					"|",
					MainManager.AsianLang() ? ((this.option < 4) ? "|line|" : "|halfline|") : "",
					list.ToArray()[this.secondoption]
				}), 0, null, false, false, new Vector3((MainManager.languageid == 6) ? -6.5f : (MainManager.AsianLang() ? ((this.option == 1) ? -6.65f : -6f) : -6.25f), MainManager.AsianLang() ? 2.95f : 2.6f), Vector3.zero, Vector2.one, this.boxes[3].transform, null));
			}
			if (MainManager.instance.cursor == null)
			{
				MainManager.CreateCursor(this.boxes[0].transform);
				return;
			}
			break;
		case 4:
			MainManager.listammount = 9;
			MainManager.ShowItemList(17, Vector2.zero, false, false);
			MainManager.instance.itemlist.parent = this.boxes[0].transform;
			MainManager.instance.itemlist.localScale = Vector3.one;
			MainManager.instance.itemlist.localPosition = new Vector2(-4.75f, 2.45f);
			return;
		case 5:
			MainManager.listammount = 9;
			MainManager.ShowItemList((this.windowid == 5) ? 19 : 30, Vector2.zero, false, false);
			MainManager.instance.itemlist.parent = this.boxes[0].transform;
			MainManager.instance.itemlist.localScale = Vector3.one;
			MainManager.instance.itemlist.localPosition = new Vector2(-3.75f, 2.5f);
			return;
		case 6:
			MainManager.DestroyText(this.boxes[0].transform.GetChild(0));
			MainManager.DestroyText(this.boxes[0].transform);
			if (this.option > -1)
			{
				base.StartCoroutine(MainManager.SetText("|center||sort,30||size," + (((MainManager.languageid == 2 || MainManager.languageid == 6) && this.option == 0) ? "0.8" : ((MainManager.languageid == 4) ? "0.75" : "0.9")) + ",0.8|" + MainManager.areanames[this.option], new Vector3(0f, -0.1f), this.boxes[0].transform.GetChild(0)));
				string[] array5 = this.enemydata[this.option].Split(new char[]
				{
					'{'
				});
				this.maxsecond = array5.Length;
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					"|sort,30||size,0.85,0.815||setbreak,12,true||singlebreak,",
					(MainManager.languageid == 6) ? 10 : 11,
					"|",
					array5[this.secondoption]
				}), new Vector3(-6.15f, 0.35f), this.boxes[0].transform));
				if (this.boxes[0].transform.childCount > 1)
				{
					if (this.boxes[0].transform.GetChild(1) != null)
					{
						this.boxes[0].transform.GetChild(1).gameObject.SetActive(this.maxsecond > 1 && this.secondoption > 0);
					}
					if (this.boxes[0].transform.GetChild(2) != null)
					{
						this.boxes[0].transform.GetChild(2).gameObject.SetActive(this.maxsecond > 1 && this.secondoption < this.maxsecond - 1);
						return;
					}
				}
			}
			else if (this.boxes[0].transform.childCount > 10)
			{
				this.boxes[0].transform.GetChild(1).gameObject.SetActive(false);
				this.boxes[0].transform.GetChild(2).gameObject.SetActive(false);
				return;
			}
			break;
		case 7:
			if (MainManager.instance.cursor != null)
			{
				MainManager.instance.cursor.transform.position = Vector3.one * 999f;
			}
			MainManager.DestroyText(this.boxes[0].transform);
			if (this.option < 10)
			{
				if (this.ttemp == null)
				{
					this.ttemp = new GameObject("skip").AddComponent<ButtonSprite>().SetUp(6, 0, MainManager.menutext[257], new Vector3(-1.5f, -2f, 1f), Vector3.one * 0.5f, 99, MainManager.GUICamera.transform).transform;
					MainManager.NewUIObject("box", this.ttemp.transform, new Vector3(2.5f, 0f), new Vector3(1.25f, 3f, 1f), MainManager.guisprites[0], -5).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
				}
				this.sprites = null;
				MainManager.instance.flagstring[0] = MainManager.menutext[((this.option < 4) ? 250 : 88) + this.option];
				base.StartCoroutine(MainManager.SetText("|sort,30||size,0.85||center|" + MainManager.menutext[(this.option < 4) ? 249 : 248], new Vector3(0f, 0.35f), this.boxes[0].transform));
				return;
			}
			if (this.option == 10)
			{
				Object.Destroy(this.ttemp.gameObject);
				this.ApplySettings();
				Transform transform3 = new GameObject("confirm").AddComponent<ButtonSprite>().SetUp(4, 0, MainManager.menutext[42], new Vector3(-1.5f, -2f), Vector3.one * 0.5f, 99, this.boxes[0].transform).transform;
				MainManager.NewUIObject("box", transform3, new Vector3(2.5f, 0f), new Vector3(1.25f, 3f, 1f), MainManager.guisprites[0], -5).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
			}
			this.option = 11;
			this.sprites = new SpriteRenderer[12];
			for (int num4 = 0; num4 < 10; num4++)
			{
				this.sprites[num4] = MainManager.NewUIObject("button" + num4, this.boxes[0].transform, new Vector3(Mathf.Lerp(-4f, 4f, (float)num4 / 9f), -0.5f), Vector3.one, MainManager.guisprites[9]).GetComponent<SpriteRenderer>();
				this.sprites[num4].tag = "Text";
				ButtonSprite buttonSprite = this.sprites[num4].gameObject.AddComponent<ButtonSprite>();
				buttonSprite.SetUp(num4, 1, null, buttonSprite.transform.localPosition, Vector3.one * 0.4f, 50, this.boxes[0].transform);
			}
			for (int num5 = 0; num5 < 2; num5++)
			{
				this.sprites[num5 + 10] = MainManager.NewUIObject("arrow", this.boxes[0].transform, new Vector3((num5 == 0) ? -3.5f : 3.5f, 0.5f), Vector3.one, MainManager.guisprites[1], 50).GetComponent<SpriteRenderer>();
				this.sprites[num5 + 10].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
				if (num5 == 1)
				{
					this.sprites[num5 + 10].flipY = true;
				}
				this.sprites[num5 + 10].tag = "Text";
			}
			base.StartCoroutine(MainManager.SetText("|sort,30||size,0.85||center|" + MainManager.menutext[254], new Vector3(0f, 0.35f), this.boxes[0].transform));
			break;
		default:
			return;
		}
	}

	// Token: 0x0600070B RID: 1803 RVA: 0x0005F88E File Offset: 0x0005DA8E
	private char QuoteMarks(bool end)
	{
		if (MainManager.languageid != 3)
		{
			return '"';
		}
		if (!end)
		{
			return '「';
		}
		return '」';
	}

	// Token: 0x0600070C RID: 1804 RVA: 0x0005F8AC File Offset: 0x0005DAAC
	private string GetRecipeDesc(int id)
	{
		string text = MainManager.itemdata[0, MainManager.listvar[MainManager.instance.option], 2] + "|halfline||line|";
		string[] array = MainManager.librarydata[this.option, MainManager.instance.option, 0].Split(new char[]
		{
			','
		});
		for (int i = 0; i < array.Length; i++)
		{
			if (Convert.ToInt32(array[i]) == -1)
			{
				text += MainManager.menutext[150];
				break;
			}
			if (i == 0)
			{
				text = text + MainManager.menutext[(array.Length == 1) ? 153 : 151] + " ";
			}
			text = string.Concat(new string[]
			{
				text,
				(MainManager.languageid != 3) ? MainManager.itemdata[0, Convert.ToInt32(array[i]), 3] : "",
				" ",
				MainManager.itemdata[0, Convert.ToInt32(array[i]), 0],
				" "
			});
			if (array.Length > 1 && i == 0)
			{
				text = text + MainManager.menutext[152] + " ";
			}
			if (MainManager.languageid == 3)
			{
				text += "|line||halfline|";
			}
		}
		string str = "";
		for (int j = 0; j < text.Length - 1; j++)
		{
			str += text[j].ToString();
		}
		return str + ((MainManager.languageid == 3) ? '。' : '.').ToString();
	}

	// Token: 0x0600070D RID: 1805 RVA: 0x0005FA50 File Offset: 0x0005DC50
	private int GetLibraryID()
	{
		if (this.option == 2)
		{
			return MainManager.instance.option;
		}
		if (this.option == 0 || this.option == 3)
		{
			return MainManager.libraryorder[this.option, MainManager.instance.option];
		}
		return MainManager.libraryorder[this.option, MainManager.listvar[MainManager.instance.option]];
	}

	// Token: 0x0600070E RID: 1806 RVA: 0x0005FAC0 File Offset: 0x0005DCC0
	private Transform CreateIcon(Sprite sprite)
	{
		if (sprite == null)
		{
			sprite = MainManager.librarysprites[128];
		}
		Transform transform = MainManager.NewUIObject("Icon", this.boxes[2].transform, new Vector3(0f, 1.5f), Vector3.one * 2.5f, sprite, 3).transform;
		if (this.option == 2 && MainManager.instance.librarystuff[2, MainManager.instance.option])
		{
			transform.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.6f);
		}
		transform.tag = "Text";
		return transform;
	}

	// Token: 0x0600070F RID: 1807 RVA: 0x0005FB78 File Offset: 0x0005DD78
	private void DestroyPause()
	{
		MainManager.instance.pause = false;
		if (MainManager.instance.itemlist != null)
		{
			Object.Destroy(MainManager.instance.itemlist.gameObject);
		}
		MainManager.instance.Invoke("CheckAchievement", 0.5f);
		if (MainManager.player != null)
		{
			MainManager.player.pausecooldown = 7f;
		}
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000710 RID: 1808 RVA: 0x0005FBF1 File Offset: 0x0005DDF1
	private IEnumerator BuildWindow()
	{
		MainManager.instance.inputcooldown = 5f;
		this.canpick = false;
		if (this.boxes != null)
		{
			for (int i = 0; i < this.boxes.Length; i++)
			{
				if (this.boxes[i] != null)
				{
					this.boxes[i].shrink = true;
				}
			}
			yield return new WaitForSeconds(0.2f);
			for (int j = 0; j < this.boxes.Length; j++)
			{
				if (this.boxes[j] != null)
				{
					Object.Destroy(this.boxes[j].gameObject);
				}
			}
		}
		if (this.windowid == 0 || this.windowid == 1 || this.windowid == 3)
		{
			this.outlined = MainManager.NewUIObject("outline", base.transform, default(Vector3), Vector3.one, MainManager.guisprites[108]).GetComponent<SpriteRenderer>();
			this.outlined.enabled = false;
		}
		this.secondoption = -1;
		switch (this.windowid)
		{
		case 0:
		{
			if (MainManager.battle != null)
			{
				this.maxoptions = 2;
			}
			else
			{
				this.maxoptions = 4;
			}
			this.boxes = new DialogueAnim[4];
			this.boxes[0] = MainManager.Create9Box(new Vector3(-3.5f, -3.5f, 10f), new Vector2(11f, 2.5f), 0, -10, new Color(0.85f, 0.85f, 0f), true).GetComponent<DialogueAnim>();
			this.boxes[0].transform.parent = base.transform;
			this.boxes[1] = MainManager.Create9Box(new Vector3(5.5f, 0f, 10f), new Vector2(7.25f, 9.5f), 1, -20, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[1].transform.parent = base.transform;
			this.boxes[2] = MainManager.Create9Box(new Vector3(-3.5f, 0f, 10f), new Vector2(11f, 5f), 2, -15, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[2].transform.parent = base.transform;
			this.boxes[3] = MainManager.Create9Box(new Vector3(5.5f, 1.85f, 10f), new Vector2(6.75f, 1.75f), 4, -5, new Color(1f, 0.4f, 0.2f), true).GetComponent<DialogueAnim>();
			this.boxes[3].transform.parent = base.transform;
			int[] psprite = StartMenu.psprite;
			int num = MainManager.SaveProgressIcons();
			float[] array = MainManager.Divisions(num);
			for (int k = 0; k < num; k++)
			{
				MainManager.NewUIObject("prog" + k, this.boxes[3].transform, new Vector3(-3.5f + array[k] * 7f, 0f), Vector3.one * 1f, MainManager.guisprites[psprite[k]], 20 - k);
			}
			this.sprites = new SpriteRenderer[19];
			this.sprites[0] = MainManager.NewUIObject("NameBar", this.boxes[0].transform, new Vector3(0f, 7.8f)).AddComponent<SpriteRenderer>();
			this.sprites[0].sprite = MainManager.guisprites[0];
			this.sprites[0].color = new Color(1f, 1f, 1f, 0.5f);
			this.sprites[0].transform.localScale = new Vector3(1f, 1.5f, 1f);
			this.sprites[0].sortingOrder = -2;
			this.sprites[1] = MainManager.NewUIObject("TPBar", this.boxes[1].transform, new Vector3(-1.7f, -2.3f)).AddComponent<SpriteRenderer>();
			this.sprites[1].sprite = MainManager.guisprites[4];
			this.sprites[1].color = MainManager.instance.menucolors[3];
			this.sprites[1].transform.localScale = new Vector3(0.55f, 0.6f, 1f);
			this.sprites[2] = MainManager.NewUIObject("MoneyBar", this.boxes[1].transform, new Vector3(-1.7f, -3.7f)).AddComponent<SpriteRenderer>();
			this.sprites[2].sprite = MainManager.guisprites[4];
			this.sprites[2].color = Color.red;
			this.sprites[2].transform.localScale = new Vector3(0.55f, 0.6f, 1f);
			this.sprites[3] = MainManager.NewUIObject("TPIcon", this.boxes[1].transform, new Vector3(-2.85f, -2.3f)).AddComponent<SpriteRenderer>();
			this.sprites[3].sortingOrder = 1;
			this.sprites[3].transform.localScale = Vector3.one;
			this.sprites[3].sprite = MainManager.guisprites[28];
			this.sprites[4] = MainManager.NewUIObject("MoneyIcon", this.boxes[1].transform, new Vector3(-2.85f, -3.7f)).AddComponent<SpriteRenderer>();
			this.sprites[4].sortingOrder = 1;
			this.sprites[4].transform.localScale = Vector3.one;
			this.sprites[4].sprite = MainManager.guisprites[29];
			SpriteRenderer component = MainManager.NewUIObject("CBerryBar", this.boxes[1].transform, new Vector3(1.7f, -3.7f), new Vector3(0.55f, 0.6f, 1f), MainManager.guisprites[4], 0).GetComponent<SpriteRenderer>();
			component.color = Color.cyan;
			base.StartCoroutine(MainManager.SetText("|sort,10||color,4|" + MainManager.instance.flagvar[14].ToString().PadLeft(3, '0'), 2, null, false, false, new Vector3(-0.65f, -0.5f), Vector3.zero, Vector2.one * 1.75f, component.transform, null));
			component = MainManager.NewUIObject("BerryIcon", this.boxes[1].transform, new Vector3(0.5f, -3.7f), Vector3.one, MainManager.guisprites[83], 1).GetComponent<SpriteRenderer>();
			component = MainManager.NewUIObject("TimeBar", this.boxes[1].transform, new Vector3(0f, -1f), new Vector3(1.1f, 0.6f, 1f), MainManager.guisprites[4], 0).GetComponent<SpriteRenderer>();
			component.color = new Color(0f, 0.75f, 1f);
			component = MainManager.NewUIObject("TimeIcon", this.boxes[1].transform, new Vector3(-2.25f, -1f), Vector3.one * 0.75f, MainManager.guisprites[84], 1).GetComponent<SpriteRenderer>();
			component = MainManager.NewUIObject("levelbar", this.boxes[1].transform, new Vector3(0f, 0.25f), new Vector3(1.1f, 0.6f, 1f), MainManager.guisprites[4], 0).GetComponent<SpriteRenderer>();
			component.color = new Color(0f, 0.75f, 0.75f);
			base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
			{
				"|color,4||sort,2||size,",
				MainManager.AsianLang() ? 0.675f : ((MainManager.languageid == 2 || MainManager.languageid == 6) ? 0.7f : 0.8f),
				",0.8,lock|",
				MainManager.menutext[118],
				" ",
				MainManager.instance.partylevel
			}), 2, null, false, false, new Vector3(-1.5f, 0.15f), Vector3.zero, Vector2.one, this.boxes[1].transform, null));
			component = MainManager.NewUIObject("levelicon", this.boxes[1].transform, new Vector3(-2.25f, 0.25f), Vector3.one * 1.25f, MainManager.itemsprites[0, 27], 1).GetComponent<SpriteRenderer>();
			base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
			{
				"|single||size,0.72,0.75|",
				MainManager.menutext[111],
				"|line|\t",
				MainManager.menutext[274],
				MainManager.areanames[MainManager.instance.areaid]
			}), new Vector3(-2.25f, 3.6f), this.boxes[1].transform));
			component = MainManager.NewUIObject("expbar", this.boxes[1].transform, new Vector3(1.7f, -2.3f), new Vector3(0.55f, 0.6f, 1f), MainManager.guisprites[4], 0).GetComponent<SpriteRenderer>();
			component.color = Color.yellow;
			base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
			{
				"|sort,10||color,4|",
				MainManager.instance.partyexp.ToString().PadLeft(3, '0'),
				"|quarterline|    /|size,0.8|",
				MainManager.instance.neededexp
			}), 2, null, false, false, new Vector3(1.1f, -2.25f), Vector3.zero, Vector2.one * 0.8f, this.boxes[1].transform, null));
			component = MainManager.NewUIObject("expicon", this.boxes[1].transform, new Vector3(0.5f, -2.3f), Vector3.one * 0.75f, MainManager.guisprites[27], 1).GetComponent<SpriteRenderer>();
			if (MainManager.battle == null && MainManager.instance.items[1].Contains(41))
			{
				this.sprites[18] = MainManager.NewUIObject("mapicon", this.boxes[1].transform, new Vector3(-2.8f, 3.8f), Vector3.one * 0.75f, MainManager.itemsprites[0, 41], 15).GetComponent<SpriteRenderer>();
				new GameObject("button").AddComponent<ButtonSprite>().SetUp(9, -1, null, new Vector3(0f, -1f), Vector3.one * 0.5f, 5, this.sprites[18].transform);
			}
			List<Transform> list = new List<Transform>();
			if (MainManager.instance.flags[613])
			{
				list.Add(MainManager.NewUIObject("medalmode", this.boxes[1].transform, new Vector3(-2f, 4.5f, -0.1f), Vector3.one * 0.4f, MainManager.guisprites[61]).transform);
			}
			if (MainManager.instance.flags[614])
			{
				list.Add(MainManager.NewUIObject("hardermode", this.boxes[1].transform, new Vector3(-1f, 4.5f, -0.1f), Vector3.one * 0.7f, MainManager.itemsprites[1, 30]).transform);
			}
			if (MainManager.instance.flags[615])
			{
				list.Add(MainManager.NewUIObject("superblockmode", this.boxes[1].transform, new Vector3(0f, 4.5f, -0.1f), Vector3.one * 0.7f, MainManager.itemsprites[1, 19]).transform);
			}
			if (MainManager.instance.flags[656])
			{
				list.Add(MainManager.NewUIObject("expmode", this.boxes[1].transform, new Vector3(1f, 4.5f, -0.1f), Vector3.one * 0.7f, MainManager.itemsprites[1, 42]).transform);
			}
			if (MainManager.instance.flags[681])
			{
				list.Add(MainManager.NewUIObject("mysterymode", this.boxes[1].transform, new Vector3(1f, 4.5f, -0.1f), Vector3.one * 0.7f, MainManager.guisprites[190]).transform);
			}
			if (MainManager.instance.flags[616])
			{
				list.Add(MainManager.NewUIObject("tangmode", this.boxes[1].transform, new Vector3(2f, 4.5f, -0.1f), Vector3.one, MainManager.instance.projectilepsrites[20]).transform);
			}
			if (list.Count > 0)
			{
				for (int l = 0; l < list.Count; l++)
				{
					Transform transform = list.ToArray()[l];
					transform.localPosition = new Vector3(Mathf.Lerp((float)list.Count / -3f, (float)list.Count / 3f, (float)l / Mathf.Clamp((float)list.Count - 1f, 1f, float.PositiveInfinity)), transform.localPosition.y, transform.localPosition.z);
				}
			}
			float num2 = -3.2f;
			for (int m = 0; m < MainManager.instance.playerdata.Length; m++)
			{
				this.sprites[5 + m] = MainManager.NewUIObject("PlayerBox" + m, this.boxes[0].transform, new Vector3(num2, 2.45f), new Vector3(0.5f, 0.6f, 1f), MainManager.guisprites[4]).GetComponent<SpriteRenderer>();
				this.sprites[5 + m].sortingOrder = 1;
				this.sprites[5 + m].color = MainManager.instance.charcolor[MainManager.instance.playerdata[m].trueid];
				this.sprites[8 + m] = MainManager.NewUIObject("PlayerIcon" + m, this.boxes[0].transform, new Vector3(num2, 3.85f), Vector3.one * 1.15f, MainManager.guisprites[94 + MainManager.instance.playerdata[m].trueid], -1).GetComponent<SpriteRenderer>();
				this.sprites[11 + m] = MainManager.NewUIObject("HP", this.boxes[0].transform, new Vector3(num2 - 1.4f, 2.5f), Vector3.one * 0.6f, MainManager.guisprites[24]).GetComponent<SpriteRenderer>();
				this.sprites[11 + m].sortingOrder = 2;
				base.StartCoroutine(MainManager.SetText("|color,4||sort,3||single||dropshadow,0.5,-0.5|" + MainManager.instance.playerdata[m].hp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.playerdata[m].maxhp.ToString().PadLeft(2, '0'), 2, null, false, false, new Vector3(num2 - 0.75f, -1.3f), Vector3.zero, Vector2.one, this.boxes[2].transform, null));
				num2 += 3.35f;
			}
			num2 = -3f;
			for (int n = 0; n < this.maxoptions; n++)
			{
				this.sprites[13 + n] = MainManager.NewUIObject("menuicon" + n, this.boxes[2].transform, new Vector3(num2, 3f), Vector3.one, MainManager.guisprites[n + 74]).GetComponent<SpriteRenderer>();
				num2 += 2f;
			}
			this.dynamictext = new DynamicFont[6];
			this.dynamictext[0] = DynamicFont.SetUp(MainManager.instance.tp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1f, 1f), this.boxes[1].transform, new Vector3(-2.2f, -2.65f), Color.white);
			this.dynamictext[1] = DynamicFont.SetUp(MainManager.instance.money.ToString().PadLeft(3, '0'), false, true, 6f, 2, 1, new Vector2(1f, 1f), this.boxes[1].transform, new Vector3(-2f, -4.05f), Color.white);
			this.dynamictext[5] = DynamicFont.SetUp(true, 30f, 2, 1, new Vector2(1f, 1f), this.boxes[1].transform, new Vector3(-1f, -1.35f));
			this.option = this.firstoption;
			this.secondoption = -1;
			break;
		}
		case 1:
		{
			MainManager.SetUpList(0, true, false);
			this.boxes = new DialogueAnim[5];
			this.boxes[0] = MainManager.Create9Box(new Vector3(-2.75f, 0f, 10f), new Vector2(10.85f, 8.25f), 4, -10, new Color(0.7f, 1f, 0.7f), true).GetComponent<DialogueAnim>();
			this.boxes[0].transform.parent = base.transform;
			this.boxes[1] = MainManager.Create9Box(new Vector3(-2.75f, -3.35f, 10f), new Vector2(12.5f, 3.55f), 1, -5, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[1].transform.parent = base.transform;
			this.boxes[2] = MainManager.Create9Box(new Vector3(6f, -1.75f, 10f), new Vector2(5.7f, 7.5f), 2, -5, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[2].transform.parent = base.transform;
			this.boxes[3] = MainManager.Create9Box(new Vector3(6f, 3f, 10f), new Vector2(4.5f, 2.5f), 2, -5, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[3].transform.parent = base.transform;
			if (MainManager.instance.flags[351])
			{
				this.boxes[4] = MainManager.Create9Box(new Vector3(6f, 3f, 10f), new Vector2(6f, 2.5f), 2, -5, Color.white, true).GetComponent<DialogueAnim>();
				this.boxes[4].transform.parent = base.transform;
				this.boxes[4].shrink = true;
				MainManager.NewUIObject("icon", this.boxes[4].transform, new Vector3(-1.56f, 0.1f, -0.1f), Vector3.one * 1.45f, MainManager.itemsprites[0, 110], 10);
				base.StartCoroutine(MainManager.SetText("|center||halfline||font,2||size,1.3||color,4|x" + MainManager.instance.flagvar[27].ToString().PadLeft(4, '0'), new Vector3(0.5f, 0f), this.boxes[4].transform));
			}
			this.sprites = new SpriteRenderer[11];
			this.sprites[0] = MainManager.NewUIObject("NameBar", this.boxes[0].transform, new Vector3(0f, 4.45f)).AddComponent<SpriteRenderer>();
			this.sprites[0].sprite = MainManager.guisprites[0];
			this.sprites[0].color = new Color(1f, 1f, 1f, 0.5f);
			this.sprites[0].transform.localScale = new Vector3(1f, 1.5f, 1f);
			this.sprites[0].sortingOrder = -2;
			this.dynamictext = new DynamicFont[4];
			float num2 = 2.2f;
			for (int num3 = 0; num3 < MainManager.instance.playerdata.Length; num3++)
			{
				this.sprites[num3 + 1] = MainManager.NewUIObject("Bar" + num3, this.boxes[2].transform, new Vector3(0f, num2)).AddComponent<SpriteRenderer>();
				this.sprites[num3 + 1].sprite = MainManager.guisprites[4];
				this.sprites[num3 + 1].color = MainManager.instance.charcolor[MainManager.instance.playerdata[num3].trueid];
				this.sprites[num3 + 1].transform.localScale = new Vector3(0.7f, 0.7f, 1f);
				this.sprites[num3 + 5] = MainManager.NewUIObject("Char" + num3, this.boxes[2].transform, new Vector3(-1.3f, num2)).AddComponent<SpriteRenderer>();
				this.sprites[num3 + 5].sprite = MainManager.guisprites[5 + MainManager.instance.playerdata[num3].trueid];
				this.sprites[num3 + 5].sortingOrder = 4 - num3;
				this.sprites[num3 + 5].transform.localScale = new Vector3(0.8f, 0.8f, 1f);
				this.dynamictext[num3] = DynamicFont.SetUp(MainManager.instance.playerdata[num3].hp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.playerdata[num3].maxhp.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[2].transform, new Vector3(-0.45f, num2 - 0.45f), Color.white);
				this.dynamictext[num3].dropshadow = true;
				num2 -= 1.3f;
			}
			this.dynamictext[3] = DynamicFont.SetUp(MainManager.instance.tp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[2].transform, new Vector3(-0.45f, -2.75f), Color.white);
			this.dynamictext[3].dropshadow = true;
			this.sprites[4] = MainManager.NewUIObject("BarTP", this.boxes[2].transform, new Vector3(0f, -2.3f)).AddComponent<SpriteRenderer>();
			this.sprites[4].sprite = MainManager.guisprites[4];
			this.sprites[4].color = new Color(1f, 0.5f, 0f);
			this.sprites[4].transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			this.sprites[8] = MainManager.NewUIObject("TPIcon", this.boxes[2].transform, new Vector3(-1.3f, -2.3f)).AddComponent<SpriteRenderer>();
			this.sprites[8].sprite = MainManager.guisprites[28];
			this.sprites[8].transform.localScale = Vector3.one;
			this.sprites[8].sortingOrder = 1;
			num2 = -4.25f;
			for (int num4 = 0; num4 < 2; num4++)
			{
				this.sprites[num4 + 9] = MainManager.NewUIObject("ItemIcon" + num4, this.boxes[0].transform, new Vector3(num2, 4.45f)).AddComponent<SpriteRenderer>();
				this.sprites[num4 + 9].sprite = MainManager.guisprites[22 + num4];
				num2 += 8.5f;
			}
			this.option = 0;
			this.maxoptions = 2;
			this.secondoption = -1;
			this.maxsecond = MainManager.instance.playerdata.Length;
			break;
		}
		case 2:
		{
			this.page = 0;
			MainManager.SetUpList(3, true, false);
			MainManager.RefreshBadgeOrder();
			this.boxes = new DialogueAnim[3];
			this.dynamictext = new DynamicFont[6];
			this.boxes[0] = MainManager.Create9Box(new Vector3(-2.75f, 0f, 10f), new Vector2(11.5f, 8f), 0, -10, new Color(0.9f, 0.5f, 0.5f), true).GetComponent<DialogueAnim>();
			this.boxes[0].transform.parent = base.transform;
			this.boxes[1] = MainManager.Create9Box(new Vector3(-2.75f, -3.35f, 10f), new Vector2(12.5f, 3.5f), 1, -5, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[1].transform.parent = base.transform;
			this.boxes[2] = MainManager.Create9Box(new Vector3(6f, 0f, 10f), new Vector2(5.75f, 10.25f), 2, -5, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[2].transform.parent = base.transform;
			this.sprites = new SpriteRenderer[20];
			this.sprites[0] = MainManager.NewUIObject("NameBar", this.boxes[0].transform, new Vector3(1.15f, 4.45f)).AddComponent<SpriteRenderer>();
			this.sprites[0].sprite = MainManager.guisprites[0];
			this.sprites[0].color = new Color(1f, 1f, 1f, 0.5f);
			this.sprites[0].transform.localScale = new Vector3(1f, 1.5f, 1f);
			this.sprites[0].sortingOrder = -2;
			new GameObject().AddComponent<ButtonSprite>().SetUp(7, -1, null, new Vector3(4.3f, 0f), new Vector3(0.75f, 0.5f, 1f), 10, this.sprites[0].transform);
			MainManager.NewUIObject("switchicon", this.sprites[0].transform, new Vector3(InputIO.LongButton(7) ? 2.8f : 3.3f, 0f), new Vector3(0.5f, 0.35f, 1f), MainManager.guisprites[92], 11).GetComponent<SpriteRenderer>().flipX = true;
			float num2 = 2.2f;
			for (int num5 = 0; num5 < 5; num5++)
			{
				this.sprites[num5 + 1] = MainManager.NewUIObject("Bar" + num5, this.boxes[2].transform, new Vector3(0f, num2)).AddComponent<SpriteRenderer>();
				this.sprites[num5 + 1].sprite = MainManager.guisprites[4];
				this.sprites[num5 + 1].color = MainManager.instance.menucolors[num5];
				this.sprites[num5 + 1].transform.localScale = new Vector3(0.7f, 0.7f, 1f);
				this.sprites[num5 + 6] = MainManager.NewUIObject("Char" + num5, this.boxes[2].transform, new Vector3(-1.3f, num2)).AddComponent<SpriteRenderer>();
				this.sprites[num5 + 6].sortingOrder = 1;
				this.sprites[num5 + 6].transform.localScale = new Vector3(0.6f, 0.6f, 1f);
				switch (num5)
				{
				case 0:
					this.sprites[num5 + 6].sprite = MainManager.guisprites[24];
					this.dynamictext[num5] = DynamicFont.SetUp(MainManager.instance.playerdata[0].hp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.playerdata[0].maxhp.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[2].transform, new Vector3(-0.45f, num2 - 0.45f), Color.white);
					break;
				case 1:
					this.sprites[num5 + 6].sprite = MainManager.guisprites[25];
					this.dynamictext[num5] = DynamicFont.SetUp(MainManager.instance.playerdata[0].atk.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[2].transform, new Vector3(0f, num2 - 0.45f), Color.white);
					break;
				case 2:
					this.sprites[num5 + 6].sprite = MainManager.guisprites[26];
					this.dynamictext[num5] = DynamicFont.SetUp(MainManager.instance.playerdata[0].def.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[2].transform, new Vector3(0f, num2 - 0.45f), Color.white);
					break;
				case 3:
					this.sprites[num5 + 6].sprite = MainManager.guisprites[28];
					this.sprites[num5 + 6].transform.localScale = Vector3.one;
					this.dynamictext[num5] = DynamicFont.SetUp(MainManager.instance.tp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxtp.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[2].transform, new Vector3(-0.45f, num2 - 0.45f), Color.white);
					break;
				case 4:
					this.sprites[num5 + 6].sprite = MainManager.guisprites[27];
					this.dynamictext[num5] = DynamicFont.SetUp(MainManager.instance.partyexp.ToString().PadLeft(3, '0') + "/" + MainManager.instance.neededexp, false, true, 6f, 2, 1, new Vector2(1f, 1f), this.boxes[2].transform, new Vector3(-0.7f, num2 - 0.4f), Color.white);
					break;
				}
				this.dynamictext[num5].dropshadow = true;
				if (num5 == 2)
				{
					num2 -= 1.7f;
				}
				else
				{
					num2 -= 1.3f;
				}
			}
			this.sprites[12] = MainManager.NewUIObject("BPBar", this.boxes[0].transform, new Vector3(-4f, 4.45f)).AddComponent<SpriteRenderer>();
			this.sprites[12].sprite = MainManager.guisprites[4];
			this.sprites[12].color = MainManager.instance.menucolors[3];
			this.sprites[12].transform.localScale = new Vector3(0.7f, 0.7f, 1f);
			this.dynamictext[5] = DynamicFont.SetUp(MainManager.instance.bp.ToString().PadLeft(2, '0') + "/" + MainManager.instance.maxbp.ToString().PadLeft(2, '0'), false, true, 6f, 2, 1, new Vector2(1.2f, 1.2f), this.boxes[0].transform, new Vector3(-4.45f, 4f), Color.white);
			this.dynamictext[5].dropshadow = true;
			this.sprites[13] = MainManager.NewUIObject("BPIcon", this.sprites[12].transform, new Vector3(-1.74f, 0.03f), Vector3.one, MainManager.guisprites[61]).GetComponent<SpriteRenderer>();
			this.sprites[13].sortingOrder = 1;
			this.sprites[14] = MainManager.NewUIObject("PlayerHead", this.boxes[2].transform, new Vector3(0f, 3.8f)).AddComponent<SpriteRenderer>();
			this.sprites[14].sprite = MainManager.guisprites[5];
			this.sprites[14].sortingOrder = 2;
			this.sprites[14].transform.localScale = Vector3.one;
			this.option = 0;
			this.maxoptions = MainManager.instance.playerdata.Length;
			this.secondoption = -1;
			if (this.maxoptions > 1)
			{
				num2 = -1.6f;
				for (int num6 = 0; num6 < 2; num6++)
				{
					Transform transform2 = MainManager.NewUIObject("side", this.boxes[2].transform, new Vector3(num2, 3.8f), Vector3.one * 1.2f, MainManager.guisprites[1]).transform;
					if (num6 == 0)
					{
						transform2.localEulerAngles = new Vector3(0f, 0f, -90f);
					}
					else
					{
						transform2.localEulerAngles = new Vector3(0f, 0f, 90f);
					}
					num2 += 3.2f;
				}
			}
			break;
		}
		case 3:
		{
			MainManager.SetUpList(10, false, false);
			this.boxes = new DialogueAnim[5];
			this.boxes[0] = MainManager.Create9Box(new Vector3(0f, 0f, 10f), new Vector2(15.5f, 9.25f), 4, -20, Color.Lerp(Color.yellow, Color.black, 0.05f), true).GetComponent<DialogueAnim>();
			this.boxes[0].transform.parent = base.transform;
			this.boxes[1] = MainManager.Create9Box(new Vector3(-2.35f, -0.45f, 10f), new Vector2(9.5f, 7f), 4, -15, Color.Lerp(Color.white, Color.black, 0.05f), true).GetComponent<DialogueAnim>();
			this.boxes[1].transform.parent = base.transform;
			this.boxes[2] = MainManager.Create9Box(new Vector3(5f, -0.45f, 10f), new Vector2(4.5f, 7f), 4, -15, Color.Lerp(Color.white, Color.black, 0.05f), true).GetComponent<DialogueAnim>();
			this.boxes[2].transform.parent = base.transform;
			this.boxes[3] = MainManager.Create9Box(new Vector3(15f, -10f, 10f), new Vector2(15f, 7.9f), 1, 10, new Color(0.5f, 0.5f, 0.7f), true).GetComponent<DialogueAnim>();
			this.boxes[3].transform.parent = base.transform;
			this.boxes[4] = MainManager.Create9Box(new Vector3(0f, 0f, 10f), new Vector2(16.5f, 9.75f), 4, -25, new Color(0.6f, 0.4f, 0f), true).GetComponent<DialogueAnim>();
			this.boxes[4].transform.parent = base.transform;
			this.sprites = new SpriteRenderer[8];
			this.sprites[0] = MainManager.NewUIObject("NameBar", this.boxes[0].transform, new Vector3(-4.23f, 3.83f)).AddComponent<SpriteRenderer>();
			this.sprites[0].sprite = MainManager.guisprites[0];
			this.sprites[0].color = new Color(1f, 1f, 1f, 0.5f);
			this.sprites[0].transform.localScale = new Vector3(1f, 1.5f, 1f);
			this.sprites[0].sortingOrder = -2;
			this.sprites[6] = MainManager.NewUIObject("ArrowRight", this.boxes[3].transform, new Vector3(7.25f, 0f), Vector3.one * 1.5f, MainManager.guisprites[1], 15).GetComponent<SpriteRenderer>();
			this.sprites[6].transform.localEulerAngles = new Vector3(0f, 0f, 90f);
			this.sprites[7] = MainManager.NewUIObject("ArrowLeft", this.boxes[3].transform, new Vector3(-7.25f, 0f), Vector3.one * 1.5f, MainManager.guisprites[1], 15).GetComponent<SpriteRenderer>();
			this.sprites[7].transform.localEulerAngles = new Vector3(0f, 0f, -90f);
			new GameObject().AddComponent<ButtonSprite>().SetUp(4, -1, MainManager.menutext[158], new Vector3(-6f, -4.25f), Vector3.one * 0.45f, 10, this.boxes[0].transform);
			float num2 = -0.5f;
			this.maxoptions = (MainManager.instance.flags[15] ? 5 : 4);
			for (int num7 = 0; num7 < this.maxoptions; num7++)
			{
				this.sprites[num7 + 1] = MainManager.NewUIObject("Icon" + num7, this.boxes[0].transform, new Vector3(num2, 3.83f)).AddComponent<SpriteRenderer>();
				this.sprites[num7 + 1].sprite = MainManager.guisprites[30 + num7];
				this.sprites[num7 + 1].transform.localScale = Vector3.one;
				num2 += 1.75f;
			}
			this.option = 0;
			this.secondoption = -1;
			break;
		}
		case 4:
			MainManager.ResetList();
			MainManager.SetUpList(17, false, false);
			this.taliasing = MainManager.MainCamera.GetComponent<FXAA>().enabled;
			this.boxes = new DialogueAnim[2];
			this.boxes[0] = MainManager.Create9Box(new Vector3(0f, -1f, 10f), new Vector2(13.5f, 7.25f), 1, -20, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[1] = MainManager.Create9Box(new Vector3(0f, 3.75f, 10f), new Vector2(12.5f, 2f), 4, -10, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[0].transform.parent = base.transform;
			this.boxes[1].transform.parent = base.transform;
			new GameObject("confirmbutton").AddComponent<ButtonSprite>().SetUp(4, -1, MainManager.menutext[101], new Vector3(-4.5f, 0.25f), Vector3.one * 0.5f, 5, this.boxes[1].transform);
			new GameObject("cancelbutton").AddComponent<ButtonSprite>().SetUp(5, -1, MainManager.menutext[45], new Vector3(0.5f, 0.25f), Vector3.one * 0.5f, 5, this.boxes[1].transform);
			new GameObject("cancel2button").AddComponent<ButtonSprite>().SetUp(9, -1, MainManager.menutext[87], new Vector3(-4.5f, -0.5f), Vector3.one * 0.5f, 5, this.boxes[1].transform);
			this.ResetTempSettings();
			if (!InputIO.IsConsole)
			{
				base.StartCoroutine(MainManager.SetText("|center||color,4||size,0.5|" + MainManager.menutext[148], new Vector3(0f, -3.8f), this.boxes[0].transform));
			}
			this.option = 0;
			this.secondoption = -1;
			break;
		case 5:
			MainManager.ResetList();
			this.boxes = new DialogueAnim[4];
			this.boxes[0] = MainManager.Create9Box(new Vector3(3.5f, -1f, 10f), new Vector2(10f, 7f), 1, -20, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[1] = MainManager.Create9Box(new Vector3(0f, 3.75f, 10f), new Vector2(12.5f, 2f), 4, -10, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[2] = MainManager.Create9Box(new Vector3(-5.25f, -1f, 10f), new Vector2(6f, 7f), 4, -20, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[3] = MainManager.Create9Box(new Vector3(0f, 0f, 10f), new Vector2(8f, 2f), 0, 10, Color.white, true).GetComponent<DialogueAnim>();
			this.boxes[3].shrink = true;
			this.boxes[3].shrinkspeed = 0.2f;
			base.StartCoroutine(MainManager.SetText("|single|" + MainManager.menutext[(this.windowid == 7) ? 257 : 99], new Vector3(-2.35f, 2.5f), this.boxes[2].transform));
			base.StartCoroutine(MainManager.SetText("|sort,15||center|" + MainManager.menutext[98], new Vector3(0f, -0.15f), this.boxes[3].transform));
			new GameObject("confirmbutton").AddComponent<ButtonSprite>().SetUp(4, -1, "|single|" + MainManager.menutext[100], new Vector3(-4.5f, 0f), Vector3.one * 0.5f, 5, this.boxes[1].transform);
			new GameObject("returnbutton").AddComponent<ButtonSprite>().SetUp(5, -1, "|single|" + MainManager.menutext[45], new Vector3(0.5f, 0f), Vector3.one * 0.5f, 5, this.boxes[1].transform);
			if (this.windowid == 7 && this.joystickid > 2)
			{
				this.joystickid = 0;
			}
			this.option = 0;
			this.secondoption = -1;
			break;
		case 6:
			this.mapicons = new List<Renderer>();
			this.canpick = false;
			if (MainManager.instance.cursor != null)
			{
				Object.Destroy(MainManager.instance.cursor.gameObject);
			}
			MainManager.ResetList();
			MainManager.PlaySound("PageFlip");
			this.tempanim = (Object.Instantiate(Resources.Load("Prefabs/Objects/map"), new Vector3(0f, -10f, 10f), Quaternion.Euler(90f, 180f, 0f)) as GameObject).GetComponent<Animator>();
			this.tempanim.transform.parent = MainManager.GUICamera.transform;
			this.tempanim.transform.localPosition = new Vector3(0f, 0.5f, 5f);
			this.tempanim.speed = 1.5f;
			this.tempanim.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
			this.tempanim.Play("Open");
			base.Invoke("MapSetup", 0.25f);
			break;
		case 7:
			InputIO.GetNeutrals();
			this.option = 0;
			this.secondoption = 0;
			this.joystickid = 0;
			MainManager.instance.inputcooldown = 10f;
			MainManager.joybinds = new int[]
			{
				-55,
				-55,
				-55,
				-55,
				-55,
				-55,
				-55,
				-55,
				-55,
				-55
			};
			this.boxes = new DialogueAnim[]
			{
				MainManager.Create9Box(new Vector3(0f, 0f, 10f), new Vector2(11f, 2.75f), 1, 10, Color.white, true).GetComponent<DialogueAnim>()
			};
			break;
		}
		if (this.windowid != 6)
		{
			this.UpdateText();
			this.canpick = true;
		}
		yield return null;
		yield break;
	}

	// Token: 0x06000711 RID: 1809 RVA: 0x0005FC00 File Offset: 0x0005DE00
	private void MapSetup()
	{
		this.sprites = new SpriteRenderer[MainManager.areanames.Length + 1];
		this.areas = new List<int>();
		for (int i = 0; i < MainManager.areanames.Length; i++)
		{
			if (MainManager.instance.librarystuff[4, i])
			{
				Vector3 zero = Vector3.zero;
				switch (i)
				{
				case 0:
					zero = new Vector3(0.92f, 2f, 2.11f);
					break;
				case 1:
					zero = new Vector3(2.76f, 2f, 1.22f);
					break;
				case 2:
					zero = new Vector3(5.6f, 2f, 1.34f);
					break;
				case 3:
					zero = new Vector3(1.4f, 2f, 0.1f);
					break;
				case 4:
					zero = new Vector3(5.56f, 2f, -1.23f);
					break;
				case 5:
					zero = new Vector3(4.17f, 2f, 0f);
					break;
				case 6:
					zero = new Vector3(6.4f, 2f, -0.11f);
					break;
				case 7:
					zero = new Vector3(4.2f, 2f, 2.95f);
					break;
				case 8:
					zero = new Vector3(-0.4f, 2f, -1.37f);
					break;
				case 9:
					zero = new Vector3(-4.75f, 2f, -2.25f);
					break;
				case 10:
					zero = new Vector3(2.77f, 2f, -0.82f);
					break;
				case 11:
					zero = new Vector3(-3.1f, 2f, -0.7f);
					break;
				case 12:
					zero = new Vector3(3.39f, 2f, -2.33f);
					break;
				case 13:
					zero = new Vector3(5.32f, 2f, -2.73f);
					break;
				case 14:
					zero = new Vector3(-4.64f, 2f, -0.19f);
					break;
				case 15:
					zero = new Vector3(-6.24f, 2f, -0.26f);
					break;
				case 16:
					zero = new Vector3(-2.3f, 2f, 0.66f);
					break;
				case 17:
					zero = new Vector3(-3.27f, 2f, 1.03f);
					break;
				case 18:
					zero = new Vector3(-3.3f, 2f, 2.92f);
					break;
				case 19:
					zero = new Vector3(-0.33f, 2f, -2.65f);
					break;
				case 20:
					zero = new Vector3(-0.55f, 2f, 0.14f);
					break;
				case 21:
					zero = new Vector3(0f, 2f, -0.62f);
					break;
				case 22:
					zero = new Vector3(4.65f, 2f, -0.5f);
					break;
				case 23:
					zero = new Vector3(-5.76f, 2f, -1.5f);
					break;
				case 24:
					zero = new Vector3(6f, 2f, 0.43f);
					break;
				}
				this.areas.Add(i);
				this.sprites[i + 1] = MainManager.NewUIObject("pip " + (MainManager.Areas)i, this.tempanim.transform, zero, new Vector3(0.3f, 0.275f, 1f) * 0.85f, MainManager.guisprites[59], 8).GetComponent<SpriteRenderer>();
				this.sprites[i + 1].color = Color.yellow;
				this.sprites[i + 1].transform.localEulerAngles = new Vector3(90f, 0f, 0f);
				if (i == MainManager.instance.areaid)
				{
					this.sprites[i + 1].gameObject.AddComponent<SpinAround>().itself = new Vector3(0f, 0f, 5f);
					this.sprites[i + 1].sprite = MainManager.guisprites[42];
					this.sprites[i + 1].transform.localScale = new Vector3(0.5f, 0.5f, 1f);
					this.sprites[i + 1].color = MainManager.instance.menucolors[3];
					this.option = i;
				}
				this.mapicons.Add(this.sprites[i + 1]);
			}
		}
		this.boxes = new DialogueAnim[1];
		this.boxes[0] = MainManager.Create9Box(new Vector3(0f, -4f, 3f), new Vector2(13.5f, 2f), 0, 10, new Color(0.3f, 0.7f, 0.7f), true).GetComponent<DialogueAnim>();
		this.boxes[0].transform.parent = base.transform;
		MainManager.NewUIObject("textholder", this.boxes[0].transform, new Vector3(0f, 8.45f), new Vector3(1.25f, 1.5f, 1f), MainManager.guisprites[0], 20).GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.65f);
		this.sprites[0] = MainManager.NewUIObject("cursor", this.tempanim.transform, Vector3.zero, new Vector3(0.5f, 0.5f, 1f), MainManager.cursorsprite[0], 10).GetComponent<SpriteRenderer>();
		this.sprites[0].transform.localEulerAngles = new Vector3(90f, 0f, 0f);
		this.sprites[0].gameObject.AddComponent<SpriteBounce>().MessageBounce();
		this.sprites[0].GetComponent<SpriteBounce>().basescale = new Vector3(0.75f, 0.75f, 1f);
		this.sprites[0].transform.localEulerAngles = new Vector3(90f, -90f, 0f);
		ButtonSprite buttonSprite = new GameObject().AddComponent<ButtonSprite>();
		buttonSprite.SetUp(6, -1, "", new Vector3(6.85f, 0.6f, -1f), Vector3.one * 0.55f, 35, this.boxes[0].transform);
		buttonSprite.transform.parent = this.boxes[0].transform;
		ButtonSprite buttonSprite2 = new GameObject().AddComponent<ButtonSprite>();
		buttonSprite2.SetUp(4, -1, "", new Vector3(6.85f, -0.5f, -1f), Vector3.one * 0.55f, 35, this.boxes[0].transform);
		buttonSprite2.transform.parent = this.boxes[0].transform;
		int[] array = MainManager.OrganizeArrayInt(this.areas.ToArray(), new int[]
		{
			0,
			1,
			2,
			5,
			6,
			4,
			3,
			11,
			10,
			12,
			13,
			8,
			9,
			19,
			7,
			18,
			16,
			17,
			14,
			15,
			22,
			20,
			21
		});
		this.areas = new List<int>();
		for (int j = 0; j < array.Length; j++)
		{
			this.areas.Add(array[j]);
		}
		this.enemydata = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/AreaDesc").ToString().Split(new char[]
		{
			'\n'
		});
		this.maxoptions = this.areas.Count;
		this.secondoption = 0;
		this.canpick = true;
		this.SetMapLines();
		this.UpdateText();
	}

	// Token: 0x06000712 RID: 1810 RVA: 0x000603D0 File Offset: 0x0005E5D0
	private void SetMapLines()
	{
		GameObject gameObject = new GameObject("lineholder");
		gameObject.transform.parent = this.tempanim.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localEulerAngles = Vector3.zero;
		Vector3 start = Vector3.zero;
		if (MainManager.instance.librarystuff[4, 0])
		{
			start = this.sprites[1].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 2])
			{
				this.SetMapLine(start, this.sprites[3].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 1])
			{
				this.SetMapLine(start, this.sprites[2].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 3])
			{
				this.SetMapLine(start, this.sprites[4].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 7])
			{
				this.SetMapLine(start, this.sprites[8].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 16])
			{
				this.SetMapLine(start, this.sprites[17].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 5])
			{
				this.SetMapLine(start, this.sprites[6].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 2])
		{
			start = this.sprites[3].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 24])
			{
				this.SetMapLine(start, this.sprites[25].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 7])
		{
			start = this.sprites[8].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 18])
			{
				this.SetMapLine(start, this.sprites[19].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 14])
		{
			start = this.sprites[15].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 15])
			{
				this.SetMapLine(start, this.sprites[16].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 12])
		{
			start = this.sprites[13].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 13])
			{
				this.SetMapLine(start, this.sprites[14].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 10])
		{
			start = this.sprites[11].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 12])
			{
				this.SetMapLine(start, this.sprites[13].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 5])
		{
			start = this.sprites[6].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 6])
			{
				this.SetMapLine(start, this.sprites[7].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 22])
			{
				this.SetMapLine(start, this.sprites[23].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 6])
		{
			start = this.sprites[7].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 4])
			{
				this.SetMapLine(start, this.sprites[5].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 16])
		{
			start = this.sprites[17].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 17])
			{
				this.SetMapLine(start, this.sprites[18].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 14])
			{
				this.SetMapLine(start, this.sprites[15].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 8])
		{
			start = this.sprites[9].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 19])
			{
				this.SetMapLine(start, this.sprites[20].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 9])
			{
				this.SetMapLine(start, this.sprites[10].transform.localPosition);
			}
		}
		if (MainManager.instance.librarystuff[4, 3])
		{
			start = this.sprites[4].transform.localPosition;
			if (MainManager.instance.librarystuff[4, 11])
			{
				this.SetMapLine(start, this.sprites[12].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 10])
			{
				this.SetMapLine(start, this.sprites[11].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 8])
			{
				this.SetMapLine(start, this.sprites[9].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 21])
			{
				this.SetMapLine(start, this.sprites[22].transform.localPosition);
			}
			if (MainManager.instance.librarystuff[4, 20])
			{
				this.SetMapLine(start, this.sprites[21].transform.localPosition);
			}
		}
		for (int i = 0; i < this.sprites.Length; i++)
		{
			if (this.sprites[i] != null)
			{
				this.sprites[i].transform.parent = gameObject.transform;
			}
		}
	}

	// Token: 0x06000713 RID: 1811 RVA: 0x00060A24 File Offset: 0x0005EC24
	private void SetMapLine(Vector3 start, Vector3 end)
	{
		LineRenderer lineRenderer = new GameObject("line")
		{
			gameObject = 
			{
				layer = 5
			},
			transform = 
			{
				parent = this.tempanim.transform.GetChild(this.tempanim.transform.childCount - 1),
				localPosition = Vector3.zero,
				localEulerAngles = Vector3.zero
			}
		}.AddComponent<LineRenderer>();
		lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
		lineRenderer.material = MainManager.spritedefaultunity;
		lineRenderer.textureMode = LineTextureMode.Tile;
		lineRenderer.startWidth = 0.075f;
		lineRenderer.useWorldSpace = false;
		lineRenderer.sortingOrder = 1;
		lineRenderer.endWidth = lineRenderer.startWidth;
		lineRenderer.startColor = MainManager.instance.textcolors[9];
		lineRenderer.endColor = lineRenderer.startColor;
		lineRenderer.material.color = lineRenderer.startColor;
		lineRenderer.SetPositions(new Vector3[]
		{
			start + PauseMenu.mfix,
			end + PauseMenu.mfix
		});
		this.mapicons.Add(lineRenderer);
	}

	// Token: 0x06000714 RID: 1812 RVA: 0x00060B48 File Offset: 0x0005ED48
	private void ResetTempSettings()
	{
		this.resolutionid = MainManager.resolutionindex;
		this.fps = MainManager.fps;
		this.lowshadow = MainManager.lowshadows;
		this.lowtex = MainManager.lowtexture;
		this.mash = MainManager.mashcommandalt;
		this.mvolume = MainManager.musicvolume;
		this.svolume = MainManager.soundvolume;
		this.dvolume = MainManager.bleepvolume;
		this.fulls = MainManager.fullscreen;
		this.nwind = MainManager.nowindeffect;
		this.outline = MainManager.enableoutline;
		this.analog = MainManager.analog;
		this.particle = MainManager.particlelevel;
		this.joystick = MainManager.usejoystick;
		this.keepmusic = MainManager.keepmusicafterbattle;
		this.joystickid = MainManager.forcejoystick;
		this.monoaudio = MainManager.monoaudio;
		this.pauseunfocus = MainManager.pauseonfocus;
		this.vsyc = MainManager.vsync;
		this.snap = MainManager.snapTo8;
	}

	// Token: 0x0400069F RID: 1695
	public int windowid;

	// Token: 0x040006A0 RID: 1696
	public int resolutionid;

	// Token: 0x040006A1 RID: 1697
	public int fps;

	// Token: 0x040006A2 RID: 1698
	public int vsyc;

	// Token: 0x040006A3 RID: 1699
	public int joystick;

	// Token: 0x040006A4 RID: 1700
	public int joystickid;

	// Token: 0x040006A5 RID: 1701
	public int analog;

	// Token: 0x040006A6 RID: 1702
	private int option;

	// Token: 0x040006A7 RID: 1703
	private int maxoptions;

	// Token: 0x040006A8 RID: 1704
	private int firstoption;

	// Token: 0x040006A9 RID: 1705
	private int page;

	// Token: 0x040006AA RID: 1706
	private int maxpages;

	// Token: 0x040006AB RID: 1707
	private int secondoption;

	// Token: 0x040006AC RID: 1708
	private int maxsecond;

	// Token: 0x040006AD RID: 1709
	private int skip;

	// Token: 0x040006AE RID: 1710
	private int countdown;

	// Token: 0x040006AF RID: 1711
	private int outline;

	// Token: 0x040006B0 RID: 1712
	private int particle;

	// Token: 0x040006B1 RID: 1713
	private int[] lastmedal;

	// Token: 0x040006B2 RID: 1714
	private DialogueAnim[] boxes;

	// Token: 0x040006B3 RID: 1715
	private SpriteRenderer[] sprites;

	// Token: 0x040006B4 RID: 1716
	private SpriteRenderer dimmer;

	// Token: 0x040006B5 RID: 1717
	private SpriteRenderer outlined;

	// Token: 0x040006B6 RID: 1718
	private Transform ttemp;

	// Token: 0x040006B7 RID: 1719
	private bool exit;

	// Token: 0x040006B8 RID: 1720
	private bool canpick;

	// Token: 0x040006B9 RID: 1721
	private bool getkey;

	// Token: 0x040006BA RID: 1722
	private bool taliasing;

	// Token: 0x040006BB RID: 1723
	private List<Renderer> mapicons;

	// Token: 0x040006BC RID: 1724
	public float mvolume;

	// Token: 0x040006BD RID: 1725
	public float svolume;

	// Token: 0x040006BE RID: 1726
	public float dvolume;

	// Token: 0x040006BF RID: 1727
	private float keycooldown;

	// Token: 0x040006C0 RID: 1728
	public bool calledfrommain;

	// Token: 0x040006C1 RID: 1729
	public bool lowtex;

	// Token: 0x040006C2 RID: 1730
	public bool lowshadow;

	// Token: 0x040006C3 RID: 1731
	public bool fulls;

	// Token: 0x040006C4 RID: 1732
	public bool nwind;

	// Token: 0x040006C5 RID: 1733
	public bool inputted;

	// Token: 0x040006C6 RID: 1734
	public bool keepmusic;

	// Token: 0x040006C7 RID: 1735
	public bool mash;

	// Token: 0x040006C8 RID: 1736
	public bool monoaudio;

	// Token: 0x040006C9 RID: 1737
	public bool pauseunfocus;

	// Token: 0x040006CA RID: 1738
	public bool mapfrommain;

	// Token: 0x040006CB RID: 1739
	public bool snap;

	// Token: 0x040006CC RID: 1740
	private string[] enemydata;

	// Token: 0x040006CD RID: 1741
	private DynamicFont[] dynamictext;

	// Token: 0x040006CE RID: 1742
	private List<int> areas;

	// Token: 0x040006CF RID: 1743
	private KeyCode gottenkey;

	// Token: 0x040006D0 RID: 1744
	private MainManager.ItemUse itemuse;

	// Token: 0x040006D1 RID: 1745
	private Animator tempanim;

	// Token: 0x040006D2 RID: 1746
	private const int textfrequency = 6;

	// Token: 0x040006D3 RID: 1747
	private const float descbreak = 10f;

	// Token: 0x040006D4 RID: 1748
	private static float librarybreak;

	// Token: 0x040006D5 RID: 1749
	private static readonly Vector3 pagehide = new Vector3(20f, -2.5f, 0f);

	// Token: 0x040006D6 RID: 1750
	private static readonly Vector3 libraryarrow = new Vector3(7.25f, 0f);

	// Token: 0x040006D7 RID: 1751
	private static readonly int[] joyicons = new int[]
	{
		0,
		100,
		101,
		102,
		103,
		104,
		109,
		110,
		2,
		108,
		1,
		107,
		105,
		106,
		3,
		4,
		10
	};

	// Token: 0x040006D8 RID: 1752
	private const float ydist = 2f;

	// Token: 0x040006D9 RID: 1753
	private static Vector3 mfix;
}
