using System;
using System.Collections;
using System.Collections.Generic;
using InputIOManager;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000057 RID: 87
public class StartMenu : MonoBehaviour
{
	// Token: 0x06000778 RID: 1912 RVA: 0x00066B2C File Offset: 0x00064D2C
	private void Start()
	{
		RenderSettings.skybox = Resources.Load<Material>("Materials/Skybox/Grass1");
		RenderSettings.skybox.SetColor("_Tint", Color.gray);
		base.transform.parent = MainManager.GUICamera.transform;
		MainManager.MainCamera.transform.parent.position = new Vector3(0f, 4.5f, -17f);
		this.sprites = base.GetComponentsInChildren<SpriteRenderer>();
		MainManager.instance.CancelInvoke("DoClock");
		this.copycursor = MainManager.NewUIObject("copycursor", base.transform, Vector3.zero, Vector3.one, MainManager.cursorsprite[0], 300).GetComponent<SpriteRenderer>();
		this.copycursor.color = Color.red;
		this.copycursor.flipX = true;
		this.copycursor.enabled = false;
		this.copycursor.gameObject.AddComponent<SpinAround>().itself = new Vector3(10f, 0f);
		if (!StartMenu.noload)
		{
			this.ReloadData();
		}
		else
		{
			Resources.UnloadUnusedAssets();
		}
		this.LoadModel();
		if (MainManager.languageid == -1)
		{
			MainManager.SetUpList(20, false, false);
			MainManager.ShowItemList(20, new Vector2(-1f, -0.35f), false, false);
			this.sprites[3].gameObject.SetActive(false);
		}
		else
		{
			MainManager.instance.SetVariables();
			base.StartCoroutine(this.Intro());
		}
		StartMenu.noload = false;
	}

	// Token: 0x06000779 RID: 1913 RVA: 0x00066CA7 File Offset: 0x00064EA7
	public IEnumerator Intro()
	{
		while (!MainManager.basicload || MainManager.languageid == -1)
		{
			yield return null;
		}
		yield return null;
		Sprite[] array = Resources.LoadAll<Sprite>("Sprites/GUI/title" + MainManager.languageid);
		if (array == null || array.Length == 0)
		{
			array = Resources.LoadAll<Sprite>("Sprites/GUI/title0");
		}
		this.sprites[1].sprite = array[0];
		if (MainManager.instance.hud != null)
		{
			MainManager.instance.hud[0].parent.gameObject.SetActive(false);
		}
		this.sprites[3].gameObject.SetActive(false);
		MainManager.ChangeMusic("Title");
		this.selections = new Transform[3];
		this.menu1 = new GameObject().transform;
		this.menu1.transform.parent = base.transform;
		this.menu1.transform.localPosition = Vector3.zero;
		this.menu1.transform.localEulerAngles = Vector3.zero;
		float a = 0f;
		float b = 100f;
		do
		{
			this.sprites[0].color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, a / b));
			if (InputIO.IsEditor)
			{
				a = b * 2f;
			}
			else
			{
				a += MainManager.framestep;
			}
			yield return null;
		}
		while (a < b + 1f);
		this.sprites[0].enabled = false;
		yield return new WaitForSeconds(0.5f);
		Vector3 localPosition = this.sprites[1].transform.localPosition;
		Vector3 ltp = new Vector3(0f, 3f, 10f);
		a = 0f;
		do
		{
			this.sprites[1].color = new Color(1f, 1f, 1f, a / b);
			this.sprites[1].transform.localPosition = Vector3.Lerp(this.sprites[1].transform.localPosition, ltp, MainManager.framestep * 0.075f);
			if (InputIO.IsEditor)
			{
				a = b * 2f;
			}
			else
			{
				a += MainManager.framestep;
			}
			yield return null;
		}
		while (a < b + 1f || (!InputIO.IsEditor && this.sprites[1].transform.localPosition.y < 2.975f));
		this.SetMenuText();
		yield break;
	}

	// Token: 0x0600077A RID: 1914 RVA: 0x00066CB8 File Offset: 0x00064EB8
	private void EntityBehavior()
	{
		if (this.entities != null && this.entitycd != null && this.entitycd.Length != 0)
		{
			float num = MainManager.TieFramerate(1f);
			if (this.entitycd[0] > 0f)
			{
				this.entitycd[0] -= num;
			}
			else
			{
				this.entitycd[0] = (float)Random.Range(70, 400);
				this.entitycd[1] = (float)(((int)this.entitycd[1] == 0) ? 1 : 0);
				int[] array = new int[]
				{
					0,
					0,
					0,
					8,
					10,
					5
				};
				this.entities[0].animstate = array[Random.Range(0, array.Length)];
				array = new int[]
				{
					0,
					0,
					0,
					8,
					5
				};
				this.entities[1].animstate = array[Random.Range(0, array.Length)];
			}
			int i = 0;
			while (i < this.entities.Length)
			{
				MainManager.AnimIDs animIDs = this.entities[i].animid + MainManager.AnimIDs.Bee;
				if (animIDs <= MainManager.AnimIDs.Burglar)
				{
					if (animIDs <= MainManager.AnimIDs.Mothiva)
					{
						switch (animIDs)
						{
						case MainManager.AnimIDs.Bee:
							this.entities[i].flip = true;
							this.entities[i].talking = ((int)this.entitycd[1] == 0);
							break;
						case MainManager.AnimIDs.Beetle:
							this.entities[i].talking = ((int)this.entitycd[1] == 1);
							break;
						case MainManager.AnimIDs.Moth:
							if (this.entitycd[i] <= 0f)
							{
								this.entities[i].flip = !this.entities[i].flip;
								this.entitycd[i] = (float)Random.Range(100, 600);
							}
							else
							{
								this.entitycd[i] -= num;
							}
							break;
						default:
							if (animIDs == MainManager.AnimIDs.Mothiva)
							{
								this.entities[i].talking = true;
								this.entities[i].flip = true;
							}
							break;
						}
					}
					else if (animIDs != MainManager.AnimIDs.Zasp)
					{
						if (animIDs == MainManager.AnimIDs.Burglar)
						{
							goto IL_27E;
						}
					}
					else
					{
						this.entities[i].animstate = 5;
					}
				}
				else if (animIDs <= MainManager.AnimIDs.HBAssistant)
				{
					if (animIDs != MainManager.AnimIDs.DocHB)
					{
						if (animIDs == MainManager.AnimIDs.HBAssistant)
						{
							this.entities[i].animstate = 29;
							if (this.entitycd[i] <= 0f)
							{
								this.entities[i].flip = !this.entities[i].flip;
								this.entitycd[i] = 100f;
							}
							else
							{
								this.entitycd[i] -= num;
							}
						}
					}
					else
					{
						this.entities[i].flip = true;
					}
				}
				else
				{
					if (animIDs == MainManager.AnimIDs.Astotheles)
					{
						goto IL_27E;
					}
					if (animIDs == MainManager.AnimIDs.Jayde)
					{
						this.entities[i].flip = true;
					}
				}
				IL_2CB:
				i++;
				continue;
				IL_27E:
				this.entities[i].animstate = 29;
				goto IL_2CB;
			}
		}
	}

	// Token: 0x0600077B RID: 1915 RVA: 0x00066FA4 File Offset: 0x000651A4
	private void LoadModel()
	{
		this.model = (Object.Instantiate(Resources.Load("Prefabs/Title/Title0"), default(Vector3), Quaternion.identity) as GameObject).transform;
		int num = 0;
		int num2 = 0;
		List<EntityControl> list = new List<EntityControl>();
		for (int i = 0; i < StartMenu.savedata.Length; i++)
		{
			if (StartMenu.savedata[i] != null && StartMenu.savedata[i].Value.progression > num)
			{
				num = StartMenu.savedata[i].Value.progression;
			}
		}
		if (num >= 1)
		{
			list.Add(EntityControl.CreateNewEntity("bee", 0, new Vector3(-5.6f, 0f, -4.27f)));
			list.Add(EntityControl.CreateNewEntity("beetle", 1, new Vector3(-4.25f, 0f, -3.7f)));
			list.Add(EntityControl.CreateNewEntity("moth", 2, new Vector3(2.85f, 0f, -3.85f)));
			num2 += 3;
		}
		if (num >= 2)
		{
			list.Add(EntityControl.CreateNewEntity("aria", 78, new Vector3(8.94f, 0f, -1.7f)));
			num2++;
		}
		if (num >= 3)
		{
			list.Add(EntityControl.CreateNewEntity("hb", 159, new Vector3(-10.15f, 0f, -0.25f)));
			list.Add(EntityControl.CreateNewEntity("crow", 160, new Vector3(-8.75f, 0f, -0.15f)));
			GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/Objects/CrowPaper")) as GameObject;
			gameObject.transform.parent = this.model.transform;
			gameObject.transform.position = new Vector3(-8.75f, 0.015f, -0.15f);
			num2 += 2;
		}
		if (num >= 4)
		{
			list.Add(EntityControl.CreateNewEntity("bandit", 138, new Vector3(-13.3f, 6.9f, 6.1f)));
			list.Add(EntityControl.CreateNewEntity("asto", 195, new Vector3(-10.45f, 6.9f, 5.9f)));
			num2 += 2;
		}
		if (num >= 5)
		{
			list.Add(EntityControl.CreateNewEntity("zasp", 20, new Vector3(10.4f, 0f, 5.75f)));
			list.Add(EntityControl.CreateNewEntity("mothiva", 10, new Vector3(8.6f, 0f, 5.4f)));
			num2 += 2;
		}
		if (num >= 6)
		{
			list.Add(EntityControl.CreateNewEntity("Jayde", 249, new Vector3(-6f, 0f, 3.25f)));
			num2++;
		}
		if (num >= 7)
		{
			list.Add(EntityControl.CreateNewEntity("queen", 96, new Vector3(4.77f, 0f, -4.5f)));
			num2++;
		}
		this.entitycd = new float[num2];
		this.entities = list.ToArray();
		for (int j = 0; j < this.entities.Length; j++)
		{
			this.entities[j].transform.parent = this.model;
		}
	}

	// Token: 0x0600077C RID: 1916 RVA: 0x000672E0 File Offset: 0x000654E0
	private void ResetTB()
	{
		if (this.tb != null && this.tb.Length != 0)
		{
			for (int i = 0; i < this.tb.Length; i++)
			{
				if (this.tb[i].gameObject != null)
				{
					Object.Destroy(this.tb[i].gameObject);
				}
			}
		}
	}

	// Token: 0x0600077D RID: 1917 RVA: 0x00067338 File Offset: 0x00065538
	private void SetMenuText()
	{
		this.ResetTB();
		MainManager.DestroyText(this.menu1);
		int[] array = new int[]
		{
			123,
			13,
			124
		};
		for (int i = 0; i < this.selections.Length; i++)
		{
			this.selections[i] = new GameObject("option" + i).transform;
			this.selections[i].parent = this.menu1;
			this.selections[i].transform.localPosition = new Vector3(0f, -0.5f - (float)i, 0f);
			this.selections[i].transform.localEulerAngles = Vector3.zero;
			MainManager.instance.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[array[i]], new Vector3(0f, 0f, 10f), this.selections[i]));
		}
		this.SetButtons();
		MainManager.DestroyText(this.menu1);
		MainManager.instance.StartCoroutine(MainManager.SetText("|size,0.45||halfline||color,4||font,0|v" + Application.version, new Vector3(-8.75f, -3.55f, 10f), this.menu1));
		MainManager.instance.StartCoroutine(MainManager.SetText("|center||size,0.65||halfline||font,0|" + MainManager.menutext[108], new Vector3(0f, -3.4f, 10f), this.menu1));
		MainManager.instance.maxoptions = this.selections.Length;
		MainManager.instance.option = 0;
		this.cd = 2f;
		this.canselect = true;
		this.menuid = 1;
	}

	// Token: 0x0600077E RID: 1918 RVA: 0x000674F4 File Offset: 0x000656F4
	public void SetButtons()
	{
		if (this.tb != null && this.tb.Length != 0)
		{
			for (int i = 0; i < this.tb.Length; i++)
			{
				Object.Destroy(this.tb[i].gameObject);
			}
		}
		this.tb = new Transform[InputIO.IsConsole ? 1 : 2];
		this.tb[0] = new GameObject().AddComponent<ButtonSprite>().SetUp(4, -1, MainManager.menutext[42], new Vector3(4f, -1f, 10f), Vector3.one * 0.5f, 0, this.menu1).transform;
		if (!InputIO.IsConsole)
		{
			this.tb[1] = new GameObject().AddComponent<ButtonSprite>().SetUp(5, -1, MainManager.menutext[37], new Vector3(4f, -2f, 10f), Vector3.one * 0.5f, 0, this.menu1).transform;
		}
	}

	// Token: 0x0600077F RID: 1919 RVA: 0x000675F4 File Offset: 0x000657F4
	private void ShowExitPrompt()
	{
		this.ResetTB();
		for (int i = 0; i < this.selections.Length; i++)
		{
			Object.Destroy(this.selections[i].gameObject);
		}
		this.tb = new Transform[2];
		MainManager.DestroyText(this.menu1);
		MainManager.instance.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[113], new Vector3(0f, -1f, 10f), this.menu1));
		this.tb[0] = new GameObject().AddComponent<ButtonSprite>().SetUp(4, -1, MainManager.menutext[5], new Vector3(-3f, -2f, 10f), Vector3.one * 0.5f, 0, this.menu1).transform;
		this.tb[1] = new GameObject().AddComponent<ButtonSprite>().SetUp(5, -1, MainManager.menutext[6], new Vector3(2.5f, -2f, 10f), Vector3.one * 0.5f, 0, this.menu1).transform;
		this.menuid = 12;
	}

	// Token: 0x06000780 RID: 1920 RVA: 0x00067728 File Offset: 0x00065928
	private void Update()
	{
		if (!MainManager.instance.inevent && MainManager.languageid > -1)
		{
			MainManager.instance.camspeed = 0.01f;
			MainManager.instance.camtargetpos = new Vector3?(new Vector3(0f, 0f, -5f));
			MainManager.instance.camangleoffset = new Vector3(5f + Mathf.Sin(Time.time / 7.5f) * 5f, Mathf.Sin(Time.time / 5f) * 10f);
			if (this.gctimer < 600f)
			{
				this.gctimer += MainManager.framestep;
			}
			else
			{
				this.gctimer = 0f;
				GC.Collect();
			}
		}
		this.EntityBehavior();
		if (MainManager.instance.cursor != null && MainManager.pausemenu == null)
		{
			if (this.menuid == 0)
			{
				MainManager.instance.cursor.sortingOrder = 20;
			}
			else if (this.menuid == 1)
			{
				if (MainManager.languageid > -1)
				{
					if (this.menu1 != null)
					{
						this.menu1.localPosition = Vector3.Lerp(this.menu1.localPosition, Vector3.zero, MainManager.TieFramerate(0.1f));
					}
					MainManager.instance.cursor.transform.localPosition = new Vector3(this.langoffset[MainManager.languageid], (float)(-(float)MainManager.instance.option) - 0.25f, 10f);
				}
			}
			else if (this.menuid == 2)
			{
				if (this.menu1 != null)
				{
					this.menu1.localPosition = Vector3.Lerp(this.menu1.localPosition, new Vector3(0f, -20f), MainManager.TieFramerate(0.1f));
				}
				if (this.saves == null || this.saves.Length == 0)
				{
					this.ShowSaves();
				}
				else if (MainManager.instance.option < 3)
				{
					MainManager.instance.cursor.transform.localPosition = new Vector3(-5.5f, 4.5f - 2.65f * (float)MainManager.instance.option, 10f);
				}
				else if (MainManager.instance.option == 3)
				{
					MainManager.instance.cursor.transform.localPosition = new Vector3(-7.25f, -3f, 10f);
				}
				else
				{
					MainManager.instance.cursor.transform.localPosition = new Vector3(2.75f, -3f, 10f);
				}
			}
			else if (this.menuid == 12)
			{
				MainManager.instance.cursor.transform.position = new Vector3(0f, -99999f);
				if (this.menu1 != null)
				{
					this.menu1.localPosition = Vector3.Lerp(this.menu1.localPosition, Vector3.zero, MainManager.framestep * 0.1f);
				}
			}
		}
		else if (MainManager.instance.cursor == null)
		{
			MainManager.CreateCursor(base.transform);
			MainManager.instance.cursor.transform.position = new Vector3(0f, 999f, 0f);
			MainManager.instance.cursor.transform.parent = base.transform;
		}
		if (this.menuid == 2)
		{
			this.sprites[1].color = new Color(1f, 1f, 1f, Mathf.Lerp(this.sprites[1].color.a, 0f, MainManager.framestep * 0.2f));
		}
		if (this.cd > 0f)
		{
			this.cd -= MainManager.framestep;
			return;
		}
		if (this.canselect && MainManager.pausemenu == null && this.cd <= 0f)
		{
			if (this.menuid == 1)
			{
				this.sprites[1].color = new Color(1f, 1f, 1f, Mathf.Lerp(this.sprites[1].color.a, 1f, MainManager.framestep * 0.1f));
				base.transform.localPosition = new Vector3(0f, -1f, 10f);
				this.sprites[1].transform.localPosition = new Vector3(0f, 3f);
				if (MainManager.GetKey(0, false))
				{
					MainManager.instance.option--;
					if (MainManager.instance.option < 0)
					{
						MainManager.instance.option = MainManager.instance.maxoptions - 1;
					}
					MainManager.PlayScrollSound();
					return;
				}
				if (MainManager.GetKey(1, false))
				{
					MainManager.instance.option++;
					if (MainManager.instance.option >= MainManager.instance.maxoptions)
					{
						MainManager.instance.option = 0;
					}
					MainManager.PlayScrollSound();
					return;
				}
				if (MainManager.GetKey(4, false))
				{
					MainManager.PlaySound("Confirm", -1);
					if (MainManager.instance.option == 0)
					{
						this.cd = 30f;
						this.menuid = 2;
						return;
					}
					if (MainManager.instance.option == 1)
					{
						Object.Destroy(MainManager.instance.cursor.gameObject);
						PauseMenu pauseMenu = new GameObject("PauseMenu").AddComponent<PauseMenu>();
						pauseMenu.windowid = 4;
						pauseMenu.calledfrommain = true;
						return;
					}
					if (MainManager.instance.option == 2)
					{
						MainManager.languageid = -1;
						StartMenu.noload = true;
						this.cd = 999999f;
						base.StartCoroutine(this.SetLang());
						return;
					}
				}
				else if (MainManager.GetKey(5) && !InputIO.IsConsole)
				{
					this.cd = 30f;
					this.ShowExitPrompt();
					return;
				}
			}
			else if (this.menuid == 2)
			{
				if ((MainManager.GetKey(0, false) || MainManager.GetKey(2, false)) && this.submenu < 3)
				{
					MainManager.instance.option--;
					if (MainManager.instance.option < 0)
					{
						MainManager.instance.option = MainManager.instance.maxoptions - 1;
					}
					MainManager.PlayScrollSound();
					return;
				}
				if ((MainManager.GetKey(1, false) || MainManager.GetKey(3, false)) && this.submenu < 3)
				{
					MainManager.instance.option++;
					if (MainManager.instance.option >= MainManager.instance.maxoptions)
					{
						MainManager.instance.option = 0;
					}
					MainManager.PlayScrollSound();
					return;
				}
				if (MainManager.GetKey(4, false))
				{
					if (this.submenu == 0)
					{
						MainManager.PlaySound("Confirm", -1);
						if (MainManager.instance.option >= 3)
						{
							this.submenu = MainManager.instance.option - 3 + 1;
							for (int i = 3; i < 5; i++)
							{
								this.saves[i].SetUp(new Vector3(0f, -10f), 0.1f);
							}
							base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[128 + this.submenu - 1] + "|line||button,5| " + MainManager.menutext[43], new Vector3(0f, -3f), base.transform));
							MainManager.instance.option = 0;
							this.selectedfile = -1;
							MainManager.instance.maxoptions = 3;
							return;
						}
						MainManager.saveslot = MainManager.instance.option;
						this.menuid = 3;
						this.started = true;
						this.canselect = false;
						MainManager.instance.hud[0].parent.gameObject.SetActive(true);
						if (StartMenu.savedata[MainManager.instance.option] != null && MainManager.instance.flagvar[0] == 0)
						{
							MainManager.events.StartEvent(22, null);
							MainManager.FadeMusic(0.05f);
							return;
						}
						MainManager.events.StartEvent(8, null);
						MainManager.instance.cursor.enabled = false;
						for (int j = 0; j < this.saves.Length; j++)
						{
							this.saves[j].shrink = true;
						}
						base.enabled = false;
						return;
					}
					else if (this.submenu == 1)
					{
						if (this.selectedfile == -1)
						{
							if (StartMenu.savedata[MainManager.instance.option] != null)
							{
								MainManager.PlaySound("Confirm", -1);
								this.selectedfile = MainManager.instance.option;
								MainManager.instance.option = 0;
								MainManager.DestroyText(base.transform);
								base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[130] + "|line||button,5| " + MainManager.menutext[43], new Vector3(0f, -3f), base.transform));
								this.copycursor.transform.position = this.saves[this.selectedfile].transform.position + new Vector3(5.5f, -0.1f);
								this.copycursor.enabled = true;
								return;
							}
							MainManager.PlayBuzzer();
							return;
						}
						else
						{
							if (this.selectedfile == MainManager.instance.option)
							{
								MainManager.PlayBuzzer();
								return;
							}
							MainManager.PlaySound("Confirm", -1);
							MainManager.DestroyText(base.transform);
							base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
							{
								"|center||size,0.75|",
								MainManager.menutext[131],
								"|line||quarterline||button,4| ",
								MainManager.menutext[42],
								"   |button,5| ",
								MainManager.menutext[43]
							}), new Vector3(0f, -2.5f), base.transform));
							this.submenu = 3;
							this.cd = 30f;
							return;
						}
					}
					else if (this.submenu == 2)
					{
						if (StartMenu.savedata[MainManager.instance.option] != null)
						{
							this.submenu = 4;
							this.selectedfile = MainManager.instance.option;
							MainManager.DestroyText(base.transform);
							MainManager.PlaySound("Confirm", -1);
							base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
							{
								"|center||size,0.75|",
								MainManager.menutext[131],
								"|line||quarterline||button,4| ",
								MainManager.menutext[42],
								"   |button,5| ",
								MainManager.menutext[43]
							}), new Vector3(0f, -2.5f), base.transform));
							return;
						}
						MainManager.PlayBuzzer();
						return;
					}
					else
					{
						if (this.submenu == 3)
						{
							base.StartCoroutine(this.DoCopy());
							return;
						}
						if (this.submenu == 4)
						{
							base.StartCoroutine(this.DoDelete());
							return;
						}
						if (this.submenu == 5)
						{
							this.CancelCopyDelete();
							return;
						}
					}
				}
				else if (MainManager.GetKey(5, false))
				{
					if (this.submenu == 5)
					{
						this.CancelCopyDelete();
						return;
					}
					if (this.submenu == 0)
					{
						MainManager.PlaySound("Cancel", 10);
						MainManager.instance.option = 0;
						MainManager.instance.maxoptions = this.selections.Length;
						this.menuid = 1;
						this.cd = 30f;
						this.DestroySaves();
						return;
					}
					this.CancelCopyDelete();
					return;
				}
				else if (MainManager.GetKey(9, false) && this.submenu == 0 && MainManager.instance.option < 3 && StartMenu.savedata[MainManager.instance.option] == null && MainManager.HowManyTrue(MainManager.secretunlocks) > 1)
				{
					MainManager.saveslot = MainManager.instance.option;
					this.menuid = 3;
					this.started = true;
					this.canselect = false;
					MainManager.instance.hud[0].parent.gameObject.SetActive(true);
					MainManager.instance.flagvar[0] = 985;
					MainManager.PlaySound("Confirm", -1);
					Object.Destroy(MainManager.instance.cursor.gameObject);
					MainManager.events.StartEvent(8, null);
					for (int k = 0; k < this.saves.Length; k++)
					{
						this.saves[k].shrink = true;
					}
					base.enabled = false;
					return;
				}
			}
			else
			{
				if (this.menuid == 3)
				{
					base.transform.localPosition = new Vector3(0f, 9999f, -9999f);
					return;
				}
				if (this.menuid == 12)
				{
					if (MainManager.GetKey(4))
					{
						Application.Quit();
						return;
					}
					if (MainManager.GetKey(5))
					{
						this.SetMenuText();
						return;
					}
				}
			}
		}
		else if (MainManager.pausemenu != null)
		{
			base.transform.localPosition = new Vector3(0f, 9999f, -9999f);
		}
	}

	// Token: 0x06000781 RID: 1921 RVA: 0x00068425 File Offset: 0x00066625
	private IEnumerator SetLang()
	{
		MainManager.FadeMusic(0.09f);
		MainManager.PlayTransition(0, 0, 0.075f, Color.black);
		yield return new WaitForSeconds(0.75f);
		InputIO.LoadSettings(true);
		SceneManager.LoadScene(0);
		yield break;
	}

	// Token: 0x06000782 RID: 1922 RVA: 0x0006842D File Offset: 0x0006662D
	private IEnumerator DoCopy()
	{
		this.ShowWaitMessage();
		yield return null;
		string text = "Saves/";
		if (!InputIO.CreateFile(string.Concat(new object[]
		{
			text,
			"save",
			MainManager.instance.option,
			".dat"
		}), InputIO.ReadFile(string.Concat(new object[]
		{
			text,
			"save",
			this.selectedfile,
			".dat"
		}))))
		{
			this.ShowError();
			yield break;
		}
		this.ReloadData();
		this.DestroySaves();
		this.copycursor.enabled = false;
		this.submenu = 0;
		MainManager.instance.option = 0;
		MainManager.instance.maxoptions = 5;
		this.cd = 30f;
		MainManager.DestroyText(base.transform);
		yield break;
	}

	// Token: 0x06000783 RID: 1923 RVA: 0x0006843C File Offset: 0x0006663C
	private IEnumerator DoDelete()
	{
		this.ShowWaitMessage();
		yield return null;
		string text = "Saves/";
		if (!InputIO.DeleteFile(string.Concat(new object[]
		{
			text,
			"save",
			MainManager.instance.option,
			".dat"
		})))
		{
			this.ShowError();
			yield break;
		}
		this.ReloadData();
		this.DestroySaves();
		this.submenu = 0;
		MainManager.instance.option = 0;
		MainManager.instance.maxoptions = 5;
		this.cd = 30f;
		MainManager.DestroyText(base.transform);
		yield break;
	}

	// Token: 0x06000784 RID: 1924 RVA: 0x0006844C File Offset: 0x0006664C
	private void ShowWaitMessage()
	{
		this.cd = 10f;
		MainManager.DestroyText(base.transform);
		base.StartCoroutine(MainManager.SetText("|center||single|" + MainManager.menutext[139], new Vector3(0f, -2.75f), base.transform));
	}

	// Token: 0x06000785 RID: 1925 RVA: 0x000684A8 File Offset: 0x000666A8
	private void ReloadData()
	{
		StartMenu.savedata = new MainManager.LoadData?[3];
		for (int i = 0; i < 3; i++)
		{
			StartMenu.savedata[i] = MainManager.Load(i, true);
		}
		this.selectedfile = -1;
	}

	// Token: 0x06000786 RID: 1926 RVA: 0x000684E8 File Offset: 0x000666E8
	private void ShowError()
	{
		MainManager.DestroyText(base.transform);
		base.StartCoroutine(MainManager.SetText("|center|" + MainManager.menutext[132], new Vector3(0f, -2.75f), base.transform));
		this.submenu = 5;
	}

	// Token: 0x06000787 RID: 1927 RVA: 0x00068540 File Offset: 0x00066740
	private string GetArea(MainManager.LoadData t)
	{
		string text = MainManager.areanames[t.areaid];
		for (int i = 0; i < StartMenu.extraareas.Length; i++)
		{
			if (t.mapid == StartMenu.extraareas[i][0])
			{
				text = MainManager.menutext[StartMenu.extraareas[i][1]];
				break;
			}
		}
		if (MainManager.languageid == 6)
		{
			text = "|sizemulti,0.8,1|" + text;
		}
		return text;
	}

	// Token: 0x06000788 RID: 1928 RVA: 0x000685A4 File Offset: 0x000667A4
	private void ShowSaves()
	{
		bool flag = MainManager.AsianLang();
		this.saves = new DialogueAnim[MainManager.timeddemo ? 3 : 5];
		float num = 4.6f;
		for (int i = 0; i < 3; i++)
		{
			this.saves[i] = MainManager.Create9Box(new Vector3(0f, 30f), new Vector2(12f, 2.65f), 1, -20 * (i + 1), Color.white, true).GetComponent<DialogueAnim>();
			this.saves[i].transform.parent = base.transform;
			this.saves[i].SetUp(new Vector3(0f, num), 0.1f);
			this.saves[i].transform.localEulerAngles = Vector3.zero;
			if (StartMenu.savedata[i] != null)
			{
				MainManager.LoadData value = StartMenu.savedata[i].Value;
				base.StartCoroutine(MainManager.SetText("|single||size,0.75||sort,10|" + value.filename, new Vector3(-5.25f, 0.3f), this.saves[i].transform));
				base.StartCoroutine(MainManager.SetText("|single|" + (MainManager.AsianLang() ? "|size,0.675,0.75|" : "|size,0.75|") + "|sort,10|" + this.GetArea(value), new Vector3(-4.5f, -0.3f), this.saves[i].transform));
				SpriteRenderer component = MainManager.NewUIObject("levelbar", this.saves[i].transform, new Vector3(3f, 0.5f), new Vector3(0.9f, 0.5f, 1f), MainManager.guisprites[4], 5).GetComponent<SpriteRenderer>();
				component.color = new Color(0f, 0.75f, 0.75f);
				MainManager.NewUIObject("levelicon", component.transform, new Vector3(-2.5f, 0f), new Vector3(1f, 1.75f, 1f), MainManager.itemsprites[0, 27], 6).GetComponent<SpriteRenderer>();
				base.StartCoroutine(MainManager.SetText("|size,0.75,1.5||single||color,4||font,2||sort,10|" + MainManager.menutext[118] + (flag ? "" : (" " + value.level)), new Vector3(-1.75f, -0.5f), component.transform));
				if (flag)
				{
					base.StartCoroutine(MainManager.SetText("|size,0.75,1.5||single||color,4||font,2||sort,10|" + value.level, new Vector3(1.65f, -0.6f), component.transform));
					component.transform.GetChild(1).transform.localScale = new Vector3(0.75f, 2f, 1f);
				}
				component = MainManager.NewUIObject("timebar", this.saves[i].transform, new Vector3(3f, -0.5f), new Vector3(0.9f, 0.5f, 1f), MainManager.guisprites[4], 5).GetComponent<SpriteRenderer>();
				component.color = new Color(0f, 0.75f, 1f);
				MainManager.NewUIObject("timeicon", component.transform, new Vector3(-2.5f, 0f), new Vector3(0.6f, 1f, 1f), MainManager.guisprites[84], 6).GetComponent<SpriteRenderer>();
				base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
				{
					"|single||size,1,1.5||color,4||font,2||sort,10|",
					value.timeh.ToString().PadLeft(3, '0'),
					":",
					value.timem.ToString().PadLeft(2, '0'),
					":",
					value.times.ToString().PadLeft(2, '0')
				}), new Vector3(-1.45f, -0.5f), component.transform));
				MainManager.NewUIObject("mapicon", this.saves[i].transform, new Vector3(-5f, -0.1f), Vector3.one * 0.6f, MainManager.itemsprites[0, 41], 5).GetComponent<SpriteRenderer>();
				float num2 = -5f;
				for (int j = 0; j < value.progression; j++)
				{
					MainManager.NewUIObject("icon" + j, this.saves[i].transform, new Vector3(num2, -0.8f), Vector3.one * 0.6f, MainManager.guisprites[StartMenu.psprite[j]], 30 - j);
					num2 += 0.8f;
				}
				if (value.challenges != null)
				{
					int num3 = MainManager.HowManyTrue(value.challenges);
					int num4 = 0;
					for (int k = 0; k < value.challenges.Length; k++)
					{
						if (value.challenges[k])
						{
							Sprite sprite = null;
							float d = 1f;
							switch (k)
							{
							case 0:
								sprite = MainManager.guisprites[61];
								d = 0.4f;
								break;
							case 1:
								sprite = MainManager.itemsprites[1, 30];
								d = 0.7f;
								break;
							case 2:
								sprite = MainManager.itemsprites[1, 19];
								d = 0.7f;
								break;
							case 3:
								sprite = MainManager.instance.projectilepsrites[20];
								d = 0.7f;
								break;
							case 4:
								sprite = MainManager.itemsprites[1, 42];
								d = 0.7f;
								break;
							case 5:
								sprite = MainManager.guisprites[190];
								d = 0.7f;
								break;
							}
							MainManager.NewUIObject("mode" + k, this.saves[i].transform, new Vector3(5.65f, Mathf.Lerp(1f, -1.5f, (float)num4 / (float)num3)), Vector3.one * d, sprite, 30 - k + i * 5);
							num4++;
						}
					}
				}
			}
			else
			{
				base.StartCoroutine(MainManager.SetText("|single||center|" + MainManager.menutext[109], new Vector3(0f, -0.25f), this.saves[i].transform));
			}
			num -= 2.6f;
		}
		if (this.saves.Length > 3)
		{
			for (int l = 0; l < 2; l++)
			{
				Color red = new Color(0f, 0.75f, 1f);
				if (l == 1)
				{
					red = Color.red;
				}
				this.saves[l + 3] = MainManager.Create9Box(new Vector3(0f, 30f), new Vector2(5f, 1.5f), 0, -5 * (l + 1), red, true).GetComponent<DialogueAnim>();
				this.saves[l + 3].transform.parent = base.transform;
				this.saves[l + 3].transform.localEulerAngles = Vector3.zero;
				base.StartCoroutine(MainManager.SetText("|single||center|" + MainManager.menutext[126 + l], new Vector3(0f, -0.25f), this.saves[l + 3].transform));
				if (l == 0)
				{
					this.saves[l + 3].SetUp(new Vector3(-5f, -3f), 0.1f);
					base.StartCoroutine(MainManager.SetText(string.Concat(new string[]
					{
						"|center||size,",
						(MainManager.languageid == 6) ? "0.675" : "0.75",
						"||button,4| ",
						MainManager.menutext[42],
						"|line||halfline||button,5| ",
						MainManager.menutext[43]
					}), new Vector3(3.5f, 0.2f), this.saves[l + 3].transform));
					this.saves[l + 3].transform.GetChild(this.saves[l + 3].transform.childCount - 1).localPosition = new Vector3((MainManager.languageid == 6) ? 3.1f : 3.5f, 0.2f);
				}
				else
				{
					this.saves[l + 3].SetUp(new Vector3(5f, -3f), 0.1f);
				}
			}
		}
		MainManager.instance.option = 0;
		MainManager.instance.maxoptions = this.saves.Length;
	}

	// Token: 0x06000789 RID: 1929 RVA: 0x00068E44 File Offset: 0x00067044
	private void DestroySaves()
	{
		for (int i = 0; i < this.saves.Length; i++)
		{
			if (this.saves[i] != null)
			{
				this.saves[i].SetUp(new Vector3(0f, 30f), 0.1f);
				Object.Destroy(this.saves[i].gameObject, 3f);
			}
		}
		this.saves = null;
	}

	// Token: 0x0600078A RID: 1930 RVA: 0x00068EB4 File Offset: 0x000670B4
	private void CancelCopyDelete()
	{
		this.submenu = 0;
		this.selectedfile = -1;
		MainManager.DestroyText(base.transform);
		MainManager.instance.option = 0;
		MainManager.instance.maxoptions = this.saves.Length;
		this.copycursor.enabled = false;
		for (int i = 0; i < 2; i++)
		{
			if (i == 0)
			{
				this.saves[i + 3].SetUp(new Vector3(-5f, -3f), 0.1f);
			}
			else
			{
				this.saves[i + 3].SetUp(new Vector3(5f, -3f), 0.1f);
			}
		}
	}

	// Token: 0x040007B5 RID: 1973
	private SpriteRenderer[] sprites;

	// Token: 0x040007B6 RID: 1974
	private bool canselect;

	// Token: 0x040007B7 RID: 1975
	private bool started;

	// Token: 0x040007B8 RID: 1976
	private int menuid;

	// Token: 0x040007B9 RID: 1977
	private int submenu;

	// Token: 0x040007BA RID: 1978
	private int selectedfile;

	// Token: 0x040007BB RID: 1979
	private float cd;

	// Token: 0x040007BC RID: 1980
	private float gctimer;

	// Token: 0x040007BD RID: 1981
	public Transform[] selections;

	// Token: 0x040007BE RID: 1982
	public Transform model;

	// Token: 0x040007BF RID: 1983
	private Transform[] tb;

	// Token: 0x040007C0 RID: 1984
	private Transform menu1;

	// Token: 0x040007C1 RID: 1985
	private DialogueAnim[] saves;

	// Token: 0x040007C2 RID: 1986
	private SpriteRenderer copycursor;

	// Token: 0x040007C3 RID: 1987
	private static MainManager.LoadData?[] savedata;

	// Token: 0x040007C4 RID: 1988
	private static bool noload;

	// Token: 0x040007C5 RID: 1989
	private float[] langoffset = new float[]
	{
		-2f,
		-2f,
		-2.075f,
		-3f,
		-2f,
		-1f,
		-2.25f,
		-2f,
		-2f,
		-2f
	};

	// Token: 0x040007C6 RID: 1990
	public static int[] psprite = new int[]
	{
		99,
		112,
		114,
		115,
		123,
		124,
		125
	};

	// Token: 0x040007C7 RID: 1991
	private static readonly int[][] extraareas = new int[][]
	{
		new int[]
		{
			31,
			197
		},
		new int[]
		{
			30,
			198
		},
		new int[]
		{
			185,
			199
		},
		new int[]
		{
			16,
			200
		},
		new int[]
		{
			54,
			201
		},
		new int[]
		{
			0,
			202
		},
		new int[]
		{
			187,
			206
		},
		new int[]
		{
			197,
			208
		}
	};

	// Token: 0x040007C8 RID: 1992
	private EntityControl[] entities;

	// Token: 0x040007C9 RID: 1993
	private float[] entitycd;
}
