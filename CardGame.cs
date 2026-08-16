using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x0200000B RID: 11
public class CardGame : MonoBehaviour
{
	// Token: 0x06000170 RID: 368 RVA: 0x00010E31 File Offset: 0x0000F031
	public static void StartGame(int opponentanimid, int mapid, int[] opponentdeck)
	{
		MainManager.instance.StartCoroutine(MainManager.instance.gameObject.AddComponent<CardGame>().StartCard(opponentanimid, mapid, opponentdeck));
	}

	// Token: 0x06000171 RID: 369 RVA: 0x00010E55 File Offset: 0x0000F055
	public IEnumerator StartCard(int opponentanimid, int mapid, int[] opponentdeck)
	{
		if (CardGame.order == null || CardGame.order.Length == 0)
		{
			string[] array = Resources.Load<TextAsset>("Data/CardOrder").ToString().Split(new char[]
			{
				'\n'
			});
			CardGame.order = new int[array.Length];
			for (int i = 0; i < CardGame.order.Length; i++)
			{
				CardGame.order[i] = Convert.ToInt32(array[i]);
			}
		}
		for (int j = 0; j < MainManager.instance.playerdata.Length; j++)
		{
			MainManager.instance.playerdata[j].entity.StopAllCoroutines();
		}
		MainManager.SaveCameraPosition();
		MainManager.instance.cardgame = this;
		this.pausev = new bool[]
		{
			MainManager.instance.minipause,
			MainManager.instance.inevent
		};
		MainManager.instance.minipause = true;
		MainManager.instance.inevent = true;
		MainManager.PlaySound("BattleStart0");
		MainManager.PlayTransition(2, 0, 0.05f, Color.green);
		MainManager.FadeMusic(0.03f);
		yield return new WaitForSeconds(2f);
		this.LoadCardData();
		this.attacknextturn = new int[2];
		this.cards = new List<int>[2];
		this.handcards = new List<CardGame.Cards>[2];
		this.playedcards = new List<CardGame.Cards>[2];
		for (int k = 0; k < 2; k++)
		{
			this.handcards[k] = new List<CardGame.Cards>();
			this.playedcards[k] = new List<CardGame.Cards>();
		}
		if (opponentdeck == null)
		{
			opponentdeck = this.GetRandomDeck(opponentanimid);
		}
		this.carddiag = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/CardDialogue").ToString().Split(new char[]
		{
			'\n'
		});
		this.cards[0] = new List<int>();
		this.cards[1] = new List<int>(opponentdeck);
		for (int l = 0; l < MainManager.instance.playerdata.Length; l++)
		{
			MainManager.instance.playerdata[l].entity.gameObject.SetActive(false);
		}
		if (MainManager.map != null)
		{
			MainManager.map.gameObject.SetActive(false);
		}
		MainManager.instance.flagvar[0] = 0;
		MainManager.SetCamera(null, new Vector3?(Vector3.zero), 1f);
		this.SetArena(mapid, opponentanimid);
		this.started = true;
		MainManager.ChangeMusic(this.finalboss ? "Bounty" : "Miniboss");
		this.cursor = MainManager.NewUIObject("cursor", MainManager.GUICamera.transform, new Vector3(0f, 999f), Vector3.one, MainManager.cursorsprite[0], 20).transform;
		this.cursor.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
		this.cursor.gameObject.AddComponent<SpriteBounce>().MessageBounce();
		yield return new WaitForSeconds(0.5f);
		MainManager.PlayTransition(3, 0, 0.05f, Color.green);
		yield return new WaitForSeconds(0.5f);
		base.StartCoroutine(this.BuildWindow());
		yield break;
	}

	// Token: 0x06000172 RID: 370 RVA: 0x00010E7C File Offset: 0x0000F07C
	private void SetArena(int mapid, int opponentid)
	{
		if (mapid == -1)
		{
			this.map = (Object.Instantiate(Resources.Load("Prefabs/BattleMaps/" + MainManager.map.battlemap.ToString())) as GameObject).transform;
		}
		else
		{
			this.map = (Object.Instantiate(Resources.Load("Prefabs/BattleMaps/" + (MainManager.BattleMaps)mapid)) as GameObject).transform;
		}
		this.audience = new GameObject().AddComponent<Audience>();
		this.audience.ammount = 20;
		this.audience.lowfps = true;
		this.audience.noflip = true;
		this.audience.animtype = Audience.Type.All;
		this.audience.constantjump = new Vector2(1f, 0.1f);
		this.audience.transform.parent = this.map.transform;
		this.audience.transform.position = new Vector3(0f, 0.1f, 3f);
		this.audience.spawnarea = new Vector2(15f, 2.5f);
		this.audience.transform.eulerAngles = new Vector3(0f, 180f, 0f);
		this.paperoverlay = new GameObject().AddComponent<SpriteRenderer>();
		this.paperoverlay.sprite = Resources.Load<Sprite>("Sprites/GUI/cardbattle");
		this.paperoverlay.gameObject.layer = 5;
		this.paperoverlay.sortingOrder = -500;
		this.paperoverlay.color = new Color(1f, 1f, 1f, 0.5f);
		this.paperoverlay.transform.parent = MainManager.GUICamera.transform;
		this.paperoverlay.transform.localPosition = new Vector3(0f, 1f, 40f);
		this.paperoverlay.transform.localScale = Vector3.one * 10f;
		this.entities = new EntityControl[4];
		Vector3[] array = new Vector3[]
		{
			new Vector3(-5.7f, 0f, -1.75f),
			new Vector3(-6.4f, 0f, -0.1f),
			new Vector3(-3.75f, 0f, 0f)
		};
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			this.entities[i] = EntityControl.CreateNewEntity("party" + i, i, array[i]);
			this.entities[i].flip = true;
			this.entities[i].transform.parent = this.map.transform;
		}
		this.entities[3] = EntityControl.CreateNewEntity("opponent", opponentid - 1, new Vector3(3.75f, 0f, 0f));
		this.entities[3].transform.parent = this.map.transform;
		this.entities[3].animstate = 13;
	}

	// Token: 0x06000173 RID: 371 RVA: 0x000111B4 File Offset: 0x0000F3B4
	private void LoadCardData()
	{
		string[] array = Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/CardText").ToString().Split(new char[]
		{
			'\n'
		});
		string[] array2 = Resources.Load<TextAsset>("Data/CardData").ToString().Split(new char[]
		{
			'\n'
		});
		this.carddata = new CardGame.CardData[array2.Length];
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(new char[]
			{
				','
			});
			this.carddata[i].noid = i;
			this.carddata[i].tp = Convert.ToInt32(array3[0]);
			this.carddata[i].attack = Convert.ToInt32(array3[1]);
			this.carddata[i].enemyid = Convert.ToInt32(array3[2]);
			if (MainManager.AsianLang())
			{
				if (MainManager.enemynames[this.carddata[i].enemyid].Length <= 4)
				{
					this.carddata[i].namesizeX = 1f;
				}
				else
				{
					this.carddata[i].namesizeX = Mathf.Lerp(0.45f, 1f, 1f - (float)MainManager.enemynames[this.carddata[i].enemyid].Length / 10f);
				}
			}
			else
			{
				this.carddata[i].namesizeX = Convert.ToSingle(array[i].Split(new char[]
				{
					'@'
				})[1]);
			}
			this.carddata[i].type = (CardGame.Type)Convert.ToInt32(array3[4]);
			if (array3[5].Length > 0)
			{
				string[] array4 = array3[5].Split(new char[]
				{
					'@'
				});
				this.carddata[i].effects = new int[array4.Length, 3];
				for (int j = 0; j < array4.Length; j++)
				{
					string[] array5 = array4[j].Split(new char[]
					{
						'#'
					});
					for (int k = 0; k < array5.Length; k++)
					{
						this.carddata[i].effects[j, k] = Convert.ToInt32(array5[k]);
					}
				}
			}
			else
			{
				this.carddata[i].effects = null;
			}
			string[] array6 = array3[6].Split(new char[]
			{
				'@'
			});
			this.carddata[i].tribe = new CardGame.Tribe[array6.Length];
			for (int l = 0; l < array6.Length; l++)
			{
				this.carddata[i].tribe[l] = (CardGame.Tribe)Convert.ToInt32(array6[l]);
			}
			if (this.carddata[i].type == CardGame.Type.Boss)
			{
				list.Add(i);
			}
			else if (this.carddata[i].type == CardGame.Type.Miniboss)
			{
				list2.Add(i);
			}
			this.carddata[i].desc = array[i].Split(new char[]
			{
				'@'
			})[0];
		}
		this.boss = list.ToArray();
		this.miniboss = list2.ToArray();
	}

	// Token: 0x06000174 RID: 372 RVA: 0x00011534 File Offset: 0x0000F734
	private int[] GetRandomDeck(int opponent)
	{
		List<int> list = new List<int>();
		int[] avaliableCardsExcept = this.GetAvaliableCardsExcept(new int[]
		{
			-1
		}, false, true);
		int num;
		if (opponent + 1 == 32 && MainManager.instance.enemyencounter[this.carddata[79].enemyid, 1] > 0 && Random.Range(0, 100) > 50)
		{
			num = 79;
		}
		else
		{
			do
			{
				num = this.boss[Random.Range(0, this.boss.Length)];
			}
			while (num == 79 || !avaliableCardsExcept.Contains(num));
		}
		list.Add(num);
		for (int i = 0; i < 2; i++)
		{
			do
			{
				num = this.miniboss[Random.Range(0, this.miniboss.Length)];
			}
			while (list.Contains(num) || !avaliableCardsExcept.Contains(num));
			list.Add(num);
		}
		List<int> list2 = new List<int>(this.boss);
		list2.AddRange(this.miniboss);
		avaliableCardsExcept = this.GetAvaliableCardsExcept(list2.ToArray(), false, true);
		for (int j = 0; j < 12; j++)
		{
			list.Add(avaliableCardsExcept[Random.Range(0, avaliableCardsExcept.Length)]);
		}
		return list.ToArray();
	}

	// Token: 0x06000175 RID: 373 RVA: 0x00011654 File Offset: 0x0000F854
	private void Update()
	{
		if (MainManager.instance.inevent)
		{
			MainManager.instance.hudcooldown = -1f;
			MainManager.instance.showmoney = -1f;
			if (this.caninput)
			{
				if (this.keyhelp == null)
				{
					this.CreateKeyHelp();
				}
				this.GetInput();
			}
		}
	}

	// Token: 0x06000176 RID: 374 RVA: 0x000116B0 File Offset: 0x0000F8B0
	private void CreateKeyHelp()
	{
		this.keyhelp = new GameObject("keyholder").transform;
		this.keyhelp.transform.parent = MainManager.GUICamera.transform;
		this.keyhelp.transform.localPosition = new Vector3(0f, 0f, 5f);
		new GameObject().AddComponent<ButtonSprite>().SetUp(4, -1, MainManager.menutext[42], new Vector3(-3f, 4.15f), Vector3.one * 0.4f, 10, this.keyhelp.transform);
		new GameObject().AddComponent<ButtonSprite>().SetUp(5, -1, MainManager.menutext[43], new Vector3(0.5f, 4.15f), Vector3.one * 0.4f, 10, this.keyhelp.transform);
		new GameObject().AddComponent<ButtonSprite>().SetUp(6, -1, MainManager.menutext[188], new Vector3(-1.5f, 3.65f), Vector3.one * 0.4f, 10, this.keyhelp.transform);
	}

	// Token: 0x06000177 RID: 375 RVA: 0x000117E4 File Offset: 0x0000F9E4
	private void LateUpdate()
	{
		if (this.huds != null && this.huds[0, 0] != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					this.huds[i, j].transform.localPosition = Vector3.Lerp(this.huds[i, j].transform.localPosition, new Vector3(this.huds[i, j].transform.localPosition.x, MainManager.instance.message ? 10f : 4.2f, this.huds[i, j].transform.localPosition.z), MainManager.TieFramerate(0.2f));
					if (this.hudfont != null)
					{
						this.hudfont[i, 0].text = this.hp[i] + "/" + 5;
						this.hudfont[i, 1].text = this.tp[i].ToString().PadLeft(2, '0');
					}
				}
			}
		}
		if (this.cards != null && this.handcards != null && !this.cardanim)
		{
			float num = MainManager.TieFramerate(0.2f);
			for (int k = 0; k < 2; k++)
			{
				float num2 = 0f;
				if (this.handcards[0].Count > 0 && this.caninput)
				{
					Transform cardobj = this.handcards[0].ToArray()[this.option].cardobj;
					this.cursor.transform.localPosition = Vector3.Lerp(this.cursor.transform.localPosition, new Vector3(cardobj.transform.position.x, -0.5f, 2f), (this.cursor.transform.position.y > 20f) ? 1f : num);
				}
				else
				{
					this.cursor.transform.localPosition = new Vector3(0f, 999f);
				}
				for (int l = 0; l < this.handcards[k].Count; l++)
				{
					if (this.windowid == 1)
					{
						float y = (k == 1 || this.option != l) ? -4f : -2.75f;
						Transform cardobj2 = this.handcards[k].ToArray()[l].cardobj;
						cardobj2.transform.localPosition = Vector3.Lerp(cardobj2.transform.localPosition, new Vector3((float)((k == 0) ? -1 : 1) * (7.15f + num2) + (float)((k == 1) ? 1 : 0), y, (l == this.option && k == 0) ? 5f : (5.1f + (float)l * 0.8f)), num);
						if (k == 0)
						{
							cardobj2.transform.localScale = Vector3.Lerp(cardobj2.transform.localScale, (this.option == l) ? new Vector3(1.5f, 1.5f, 0.15f) : new Vector3(1f, 1f, 0.15f), num);
						}
						else if (k == 1)
						{
							cardobj2.transform.localScale = Vector3.one * 0.8f;
						}
						cardobj2.transform.localEulerAngles = Vector3.Lerp(cardobj2.transform.localEulerAngles, this.handcards[k].ToArray()[l].flipped ? new Vector3(0f, 180f) : Vector3.zero, num);
						num2 += -1.5f;
					}
					else
					{
						Transform cardobj3 = this.handcards[k].ToArray()[l].cardobj;
						cardobj3.transform.localPosition = Vector3.Lerp(cardobj3.transform.localPosition, new Vector3((float)(((k == 0) ? -1 : 1) * 20), -5f, 10f), num);
					}
				}
				int num3 = 0;
				int num4 = 0;
				for (int m = 0; m < this.playedcards[k].Count; m++)
				{
					Transform cardobj4 = this.playedcards[k].ToArray()[m].cardobj;
					if (cardobj4 != null)
					{
						int cardid = this.playedcards[k].ToArray()[m].cardid;
						cardobj4.transform.localScale = Vector3.Lerp(cardobj4.transform.localScale, new Vector3(1f, 1f, 0.15f), num);
						cardobj4.transform.localEulerAngles = Vector3.Lerp(cardobj4.transform.localEulerAngles, this.playedcards[k].ToArray()[m].flipped ? new Vector3(0f, 180f) : Vector3.zero, num);
						float num5 = (float)((k == 0) ? -1 : 1);
						switch (this.carddata[cardid].type)
						{
						case CardGame.Type.Attacker:
							num3++;
							cardobj4.transform.localPosition = Vector3.Lerp(cardobj4.transform.localPosition, new Vector3((0.8f + (float)num3 * 0.8f) * num5, 0f, (this.temproutine == null) ? (15.1f + (float)m * 0.8f) : cardobj4.transform.localPosition.z), num);
							break;
						case CardGame.Type.Effect:
						case CardGame.Type.Miniboss:
							num4++;
							cardobj4.transform.localPosition = Vector3.Lerp(cardobj4.transform.localPosition, new Vector3((3.25f + (float)num4 * 0.7f) * num5, 2f, (this.temproutine == null) ? (20f + (float)m * 0.8f) : cardobj4.transform.localPosition.z), num);
							break;
						case CardGame.Type.Boss:
							cardobj4.transform.localPosition = Vector3.Lerp(cardobj4.transform.localPosition, new Vector3(7.75f * num5, -0.25f, (this.temproutine == null) ? (25.1f + (float)m * 0.8f) : cardobj4.transform.localPosition.z), num);
							break;
						}
					}
				}
			}
		}
		if (this.started)
		{
			MainManager.instance.camangleoffset = new Vector3(5f + Mathf.Sin(Time.time / 7.5f) * 2f, Mathf.Sin(Time.time / 5f) * 5f);
		}
	}

	// Token: 0x06000178 RID: 376 RVA: 0x00011EC8 File Offset: 0x000100C8
	private void PlayEnemyCards()
	{
		for (int i = this.handcards[1].Count - 1; i >= 0; i--)
		{
			if (this.tp[1] >= this.carddata[this.handcards[1].ToArray()[i].cardid].tp)
			{
				this.tp[1] -= this.carddata[this.handcards[1].ToArray()[i].cardid].tp;
				this.playedcards[1].Add(this.handcards[1].ToArray()[i]);
				this.handcards[1].RemoveAt(i);
			}
			int num = this.handcards[1].Count - 1;
		}
	}

	// Token: 0x06000179 RID: 377 RVA: 0x00011FA0 File Offset: 0x000101A0
	private void GetInput()
	{
		int num = this.windowid;
		if (num == 1)
		{
			if (MainManager.GetKey(2))
			{
				this.option--;
				MainManager.PlaySound("PageFlip");
				if (this.option < 0)
				{
					this.option = this.maxoptions - 1;
					return;
				}
			}
			else if (MainManager.GetKey(3))
			{
				this.option++;
				MainManager.PlaySound("PageFlip");
				if (this.option >= this.maxoptions)
				{
					this.option = 0;
					return;
				}
			}
			else if (MainManager.GetKey(4) && this.maxoptions > 0)
			{
				int cardid = this.handcards[0].ToArray()[this.option].cardid;
				if (this.tp[0] >= this.carddata[cardid].tp)
				{
					MainManager.PlaySound("Confirm");
					this.tp[0] -= this.carddata[cardid].tp;
					this.playedcards[0].Add(this.handcards[0].ToArray()[this.option]);
					this.handcards[0].RemoveAt(this.option);
					this.option = 0;
					this.maxoptions = this.handcards[0].Count;
					return;
				}
				MainManager.PlayBuzzer();
				return;
			}
			else
			{
				if (MainManager.GetKey(5) && this.playedcards[0].Count > 0)
				{
					MainManager.PlaySound("PageFlip", -1, 0.7f, 1f);
					int num2 = this.playedcards[0].Count - 1;
					int cardid2 = this.playedcards[0].ToArray()[num2].cardid;
					this.tp[0] += this.carddata[cardid2].tp;
					this.handcards[0].Add(this.playedcards[0].ToArray()[num2]);
					this.playedcards[0].RemoveAt(num2);
					this.option = 0;
					this.maxoptions = this.handcards[0].Count;
					return;
				}
				if (MainManager.GetKey(6))
				{
					MainManager.PlaySound(Resources.Load<AudioClip>("Audio/Sounds/Confirm1"), -1, 0.4f, 0.5f);
					this.caninput = false;
					this.windowid = 2;
					this.keyhelp.transform.localPosition = new Vector3(0f, 999f);
					this.PlayEnemyCards();
					base.StartCoroutine(this.BuildWindow());
				}
			}
		}
	}

	// Token: 0x0600017A RID: 378 RVA: 0x00012230 File Offset: 0x00010430
	private int[] GetAvaliableCards(int[] pool, bool spiedonly, bool limitseen)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < pool.Length; i++)
		{
			if ((!limitseen || MainManager.instance.enemyencounter[this.carddata[pool[i]].enemyid, 0] > 0) && (!spiedonly || MainManager.instance.librarystuff[1, this.carddata[pool[i]].enemyid]))
			{
				list.Add(pool[i]);
			}
		}
		return list.ToArray();
	}

	// Token: 0x0600017B RID: 379 RVA: 0x000122B4 File Offset: 0x000104B4
	private int[] GetAvaliableCardsExcept(int[] exclude, bool spiedonly, bool limitseen)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>(exclude);
		for (int i = 0; i < this.carddata.Length; i++)
		{
			if (!list2.Contains(i) && (!limitseen || MainManager.instance.enemyencounter[this.carddata[i].enemyid, 0] > 0) && (!spiedonly || MainManager.instance.librarystuff[1, this.carddata[i].enemyid]))
			{
				list.Add(i);
			}
		}
		return list.ToArray();
	}

	// Token: 0x0600017C RID: 380 RVA: 0x00012344 File Offset: 0x00010544
	private Transform CreateCard(int id, Vector2 pos, bool flipped)
	{
		SpriteRenderer spriteRenderer = new GameObject("card " + MainManager.enemynames[this.carddata[id].enemyid]).AddComponent<SpriteRenderer>();
		spriteRenderer.gameObject.layer = 5;
		spriteRenderer.material = MainManager.spritemat;
		spriteRenderer.sprite = MainManager.guisprites[91];
		SpriteRenderer spriteRenderer2 = new GameObject("back").AddComponent<SpriteRenderer>();
		spriteRenderer2.material = MainManager.spritemat;
		spriteRenderer2.transform.parent = spriteRenderer.transform;
		spriteRenderer2.transform.localPosition = new Vector3(0f, 0f, 0.05f);
		spriteRenderer2.gameObject.layer = 5;
		spriteRenderer2.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		if (this.carddata[id].type == CardGame.Type.Boss)
		{
			spriteRenderer2.sprite = MainManager.guisprites[117];
		}
		else if (this.carddata[id].type == CardGame.Type.Miniboss)
		{
			spriteRenderer2.sprite = MainManager.guisprites[116];
		}
		else
		{
			spriteRenderer2.sprite = MainManager.guisprites[90];
		}
		SpriteRenderer spriteRenderer3 = new GameObject("tp").AddComponent<SpriteRenderer>();
		spriteRenderer3.sprite = MainManager.guisprites[28];
		spriteRenderer3.material = MainManager.spritemat;
		spriteRenderer3.gameObject.layer = 5;
		spriteRenderer3.transform.parent = spriteRenderer.transform;
		spriteRenderer3.transform.localPosition = new Vector3(0.9f, 1.3f, -0.05f);
		spriteRenderer3.transform.localScale = Vector3.one * 0.4f;
		spriteRenderer3 = new GameObject("tpcost").AddComponent<SpriteRenderer>();
		spriteRenderer3.sprite = MainManager.guisprites[48 + this.carddata[id].tp];
		spriteRenderer3.material = MainManager.spritemat;
		spriteRenderer3.gameObject.layer = 5;
		spriteRenderer3.transform.parent = spriteRenderer.transform.GetChild(1);
		spriteRenderer3.transform.localPosition = new Vector3(0f, 0f, -0.05f);
		spriteRenderer3.transform.localScale = Vector3.one;
		spriteRenderer3 = new GameObject("enemyportrait").AddComponent<SpriteRenderer>();
		spriteRenderer3.sprite = MainManager.librarysprites[MainManager.GetEnemyPortrait(this.carddata[id].enemyid)];
		spriteRenderer3.material = MainManager.spritemat;
		spriteRenderer3.gameObject.layer = 5;
		spriteRenderer3.transform.parent = spriteRenderer.transform;
		spriteRenderer3.transform.localPosition = new Vector3(0f, 0.45f, -0.05f);
		spriteRenderer3.transform.localScale = Vector3.one;
		spriteRenderer3 = new GameObject("descbox").AddComponent<SpriteRenderer>();
		spriteRenderer3.sprite = MainManager.guisprites[118];
		spriteRenderer3.material = MainManager.spritemat;
		spriteRenderer3.gameObject.layer = 5;
		spriteRenderer3.transform.parent = spriteRenderer.transform;
		spriteRenderer3.transform.localScale = new Vector3(0.8f, 0.75f, 1f);
		spriteRenderer3.transform.localPosition = new Vector3(0f, -0.825f, -0.05f);
		if (this.carddata[id].type == CardGame.Type.Attacker)
		{
			spriteRenderer3 = new GameObject("attackicon").AddComponent<SpriteRenderer>();
			spriteRenderer3.sprite = MainManager.guisprites[25];
			spriteRenderer3.material = MainManager.spritemat;
			spriteRenderer3.gameObject.layer = 5;
			spriteRenderer3.transform.parent = spriteRenderer.transform;
			spriteRenderer3.transform.localPosition = new Vector3(-0.35f, -0.8f, -0.1f);
			spriteRenderer3.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
			spriteRenderer3 = new GameObject("attackammount").AddComponent<SpriteRenderer>();
			spriteRenderer3.sprite = MainManager.guisprites[48 + this.carddata[id].attack];
			spriteRenderer3.material = MainManager.spritemat;
			spriteRenderer3.gameObject.layer = 5;
			spriteRenderer3.transform.parent = spriteRenderer.transform;
			spriteRenderer3.transform.localPosition = new Vector3(0.5f, -0.8f, -0.1f);
		}
		else
		{
			base.StartCoroutine(MainManager.SetText("|single|" + this.carddata[id].desc, 0, new float?(2.5f), false, false, new Vector3(-1.1f, 0.4f, -0.05f), Vector3.zero, new Vector2(MainManager.AsianLang() ? 0.3f : 0.4f, 0.5f), spriteRenderer3.transform, null));
		}
		switch (this.carddata[id].type)
		{
		case CardGame.Type.Attacker:
			spriteRenderer.material.color = Color.Lerp(Color.green, Color.black, 0.3f);
			break;
		case CardGame.Type.Effect:
			spriteRenderer.material.color = Color.Lerp(Color.red, Color.yellow, 0.5f);
			break;
		case CardGame.Type.Miniboss:
			spriteRenderer.material.color = Color.gray;
			break;
		case CardGame.Type.Boss:
			spriteRenderer.material.color = Color.Lerp(Color.yellow, Color.red, 0.3f);
			break;
		}
		spriteRenderer3 = new GameObject("whitebar").AddComponent<SpriteRenderer>();
		spriteRenderer3.sprite = MainManager.guisprites[0];
		spriteRenderer3.material = MainManager.spritemat;
		spriteRenderer3.gameObject.layer = 5;
		spriteRenderer3.transform.parent = spriteRenderer.transform;
		spriteRenderer3.transform.localScale = new Vector3(0.35f, 0.6f, 1f);
		spriteRenderer3.transform.localPosition = new Vector3(-0.05f, 1.3f, -0.025f);
		base.StartCoroutine(MainManager.SetText("|single|" + MainManager.enemynames[this.carddata[id].enemyid], 0, null, false, false, new Vector3(-1f, 1.2f, -0.05f), Vector3.zero, new Vector2(0.5f * this.carddata[id].namesizeX, 0.5f), spriteRenderer.transform, null));
		spriteRenderer.transform.parent = MainManager.GUICamera.transform;
		spriteRenderer.transform.localEulerAngles = (flipped ? new Vector3(0f, 180f, 0f) : Vector3.zero);
		spriteRenderer.transform.localPosition = new Vector3(pos.x, pos.y, 5f);
		return spriteRenderer.transform;
	}

	// Token: 0x0600017D RID: 381 RVA: 0x00012A0C File Offset: 0x00010C0C
	private void ShowSelectedCard(ref Transform card, ref int oldselection, Vector2 pos, Vector2 size)
	{
		if (oldselection != MainManager.instance.option && MainManager.instance.itemlist != null)
		{
			if (card != null)
			{
				Object.Destroy(card.gameObject);
			}
			card = this.CreateCard(MainManager.listvar[MainManager.instance.option], pos, false);
			card.transform.localScale = new Vector3(size.x, size.y, 0.15f);
			oldselection = MainManager.instance.option;
		}
	}

	// Token: 0x0600017E RID: 382 RVA: 0x00012A9C File Offset: 0x00010C9C
	private void RefreshDeckIndicator(int[] deck)
	{
		if (this.cardpreview != null)
		{
			for (int i = 0; i < this.cardpreview.Length; i++)
			{
				if (this.cardpreview != null)
				{
					Object.Destroy(this.cardpreview[i].gameObject);
				}
			}
		}
		this.cardpreview = new Transform[deck.Length];
		float num = 0f;
		for (int j = 0; j < deck.Length; j++)
		{
			this.cardpreview[j] = MainManager.NewUIObject("deck" + j, MainManager.GUICamera.transform, new Vector3((j >= 9) ? -6.75f : -7.75f, 4.25f + num, 10f), Vector3.one, MainManager.librarysprites[MainManager.GetEnemyPortrait(this.carddata[deck[j]].enemyid)], (j >= 9) ? (-30 + j) : j).transform;
			num -= 1f;
			if (j == 8)
			{
				num = -3.45f;
			}
		}
	}

	// Token: 0x0600017F RID: 383 RVA: 0x00012B95 File Offset: 0x00010D95
	private IEnumerator BuildWindow()
	{
		int num = this.windowid;
		List<int> deck;
		int[] atk;
		int[] def;
		DynamicFont[] atkfont;
		DynamicFont[] deffont;
		GameObject[] ai;
		GameObject[] di;
		Transform[] cardmoves;
		bool? winstate;
		switch (num)
		{
		case 0:
		{
			int state = 0;
			deck = new List<int>();
			bool skipconfirm = false;
			MainManager.overridedlist = null;
			MainManager.savelastlist = false;
			if (MainManager.instance.flags[181])
			{
				base.StartCoroutine(MainManager.SetText(this.carddiag[14], true, Vector3.zero, this.entities[3].transform, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				int[] cards = new int[]
				{
					13,
					14,
					4,
					2
				};
				Transform[] cardobj = new Transform[cards.Length];
				for (int l = 0; l < cards.Length; l++)
				{
					cardobj[l] = this.CreateCard(cards[l], new Vector2(20f, -2f), true);
					cardobj[l].localScale = Vector3.one * 1.4f;
				}
				float a3 = 0f;
				float b = 30f;
				float[] xx = new float[]
				{
					-6f,
					-2f,
					2f,
					6f
				};
				do
				{
					for (int m = 0; m < cards.Length; m++)
					{
						cardobj[m].transform.localPosition = MainManager.SmoothLerp(new Vector2(20f, -2f), new Vector3(xx[m], -2f, 10f), a3 / b);
					}
					a3 += MainManager.TieFramerate(1f);
					yield return null;
				}
				while (a3 < b + 1f);
				yield return new WaitForSeconds(0.5f);
				for (int i = 0; i < cards.Length; i = num + 1)
				{
					a3 = 0f;
					b = 20f;
					do
					{
						cardobj[i].transform.eulerAngles = Vector3.Lerp(new Vector3(0f, 180f), Vector3.zero, a3 / b);
						a3 += MainManager.TieFramerate(1f);
						yield return null;
					}
					while (a3 < b + 1f);
					num = i;
				}
				base.StartCoroutine(MainManager.SetText(this.carddiag[15], true, Vector3.zero, this.entities[3].transform, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				for (int i = 0; i < cards.Length; i = num + 1)
				{
					base.StartCoroutine(MainManager.SetText(this.carddiag[16 + i], true, Vector3.zero, this.entities[3].transform, null));
					while (MainManager.instance.message)
					{
						MainManager.templetter = base.StartCoroutine(MainManager.TempColor(Color.white, 15f, cardobj[i].GetComponent<SpriteRenderer>()));
						while (MainManager.templetter != null)
						{
							yield return null;
						}
						yield return null;
					}
					num = i;
				}
				base.StartCoroutine(MainManager.SetText(this.carddiag[20], true, Vector3.zero, this.entities[3].transform, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				a3 = 0f;
				do
				{
					for (int n = 0; n < cards.Length; n++)
					{
						cardobj[n].transform.localPosition = MainManager.SmoothLerp(new Vector3(xx[n], -2f, 10f), new Vector3(-20f, -2f), a3 / b);
					}
					a3 += MainManager.TieFramerate(1f);
					yield return null;
				}
				while (a3 < b + 1f);
				for (int num2 = 0; num2 < cards.Length; num2++)
				{
					Object.Destroy(cardobj[num2].gameObject);
				}
				base.StartCoroutine(MainManager.SetText(this.carddiag[21], true, Vector3.zero, this.entities[3].transform, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				yield return new WaitForSeconds(0.5f);
				cards = null;
				cardobj = null;
				xx = null;
			}
			if (MainManager.instance.flags[400])
			{
				string[] array = MainManager.instance.flagstring[12].Split(new char[]
				{
					','
				});
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					deck.Add(Convert.ToInt32(array[num3]));
				}
				this.RefreshDeckIndicator(deck.ToArray());
				skipconfirm = true;
				state = 3;
			}
			else if (MainManager.instance.flagstring.Length - 1 >= 7 && MainManager.instance.flagstring[7] != null && MainManager.instance.flagstring[7].Length > 1)
			{
				string[] array2 = MainManager.instance.flagstring[7].Split(new char[]
				{
					','
				});
				try
				{
					for (int num4 = 0; num4 < array2.Length; num4++)
					{
						deck.Add(Convert.ToInt32(array2[num4]));
					}
				}
				catch
				{
					MonoBehaviour.print("deck data corrupted, starting over");
					deck = new List<int>();
					goto IL_859;
				}
				this.RefreshDeckIndicator(deck.ToArray());
				base.StartCoroutine(MainManager.SetText("|boxstyle,4||spd,0|" + this.carddiag[2] + "|prompt,card,0.5,2,-11,-11,6,7|", true, Vector3.zero, null, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				if (MainManager.instance.option == 0)
				{
					state = 3;
					skipconfirm = true;
				}
				else
				{
					deck = new List<int>();
				}
				this.RefreshDeckIndicator(deck.ToArray());
			}
			IL_859:
			MainManager.listredirect = new int?(-1);
			int oldselection = -1;
			do
			{
				if (state == 0)
				{
					MainManager.instance.multilist = this.GetAvaliableCards(this.boss, true, false);
					base.StartCoroutine(MainManager.SetText("|boxstyle,4||spd,0|" + this.carddiag[0] + "|fwait,0.1||pickitem,33,0,false,false,-11,-11,8|", true, Vector3.zero, null, null));
					Transform card = null;
					while (MainManager.instance.message)
					{
						this.ShowSelectedCard(ref card, ref oldselection, new Vector2(-2f, -1.7f), Vector2.one * 1.5f);
						yield return null;
					}
					if (card != null)
					{
						Object.Destroy(card.gameObject);
					}
					if (!MainManager.listcanceled)
					{
						deck.Add(MainManager.instance.flagvar[0]);
						this.RefreshDeckIndicator(deck.ToArray());
						oldselection = -1;
						state = 1;
					}
					card = null;
				}
				if (state == 1)
				{
					for (int i = 0; i < 2; i = num + 1)
					{
						MainManager.instance.multilist = this.GetAvaliableCards(this.miniboss, true, false);
						base.StartCoroutine(MainManager.SetText("|boxstyle,4||boxspeed,1||spd,0|" + this.carddiag[8] + "|fwait,0.1||pickitem,33,0,false,false,-11,-11,8|", true, Vector3.zero, null, null));
						Transform card = null;
						while (MainManager.instance.message)
						{
							this.ShowSelectedCard(ref card, ref oldselection, new Vector2(-2f, -1.7f), Vector2.one * 1.5f);
							yield return null;
						}
						if (card != null)
						{
							Object.Destroy(card.gameObject);
						}
						if (MainManager.listcanceled)
						{
							if (MainManager.listcanceled)
							{
								MainManager.instance.flagvar[0] = -1;
							}
							oldselection = -1;
							if (i == 0)
							{
								deck = new List<int>();
								state = 0;
								this.RefreshDeckIndicator(deck.ToArray());
								break;
							}
							i -= 2;
							deck.RemoveAt(deck.Count - 1);
							this.RefreshDeckIndicator(deck.ToArray());
						}
						else if (deck.Contains(MainManager.instance.flagvar[0]))
						{
							num = i;
							i = num - 1;
							oldselection = -1;
							MainManager.PlayBuzzer();
						}
						else
						{
							deck.Add(MainManager.instance.flagvar[0]);
							this.RefreshDeckIndicator(deck.ToArray());
							oldselection = -1;
							if (i == 1)
							{
								state = 2;
							}
						}
						card = null;
						num = i;
					}
				}
				if (state == 2)
				{
					int i = 12;
					for (int j = 0; j < i; j = num + 1)
					{
						List<int> list = new List<int>(this.boss);
						list.AddRange(this.miniboss);
						MainManager.instance.multilist = this.GetAvaliableCardsExcept(list.ToArray(), true, false);
						MainManager.instance.flagvar[4] = 12 - j;
						MainManager.savelastlist = true;
						base.StartCoroutine(MainManager.SetText("|boxstyle,4||boxspeed,1||spd,0|" + this.carddiag[1] + "|fwait,0.1||pickitem,33,0,false,false,-11,-11,8|", true, Vector3.zero, null, null));
						Transform card = null;
						while (MainManager.instance.message)
						{
							this.ShowSelectedCard(ref card, ref oldselection, new Vector2(-2f, -1.7f), Vector2.one * 1.5f);
							yield return null;
						}
						if (card != null)
						{
							Object.Destroy(card.gameObject);
						}
						if (MainManager.listcanceled)
						{
							if (j == 0)
							{
								state = 1;
								deck.RemoveRange(1, 2);
								this.RefreshDeckIndicator(deck.ToArray());
								break;
							}
							j -= 2;
							deck.RemoveAt(deck.Count - 1);
							this.RefreshDeckIndicator(deck.ToArray());
						}
						else
						{
							deck.Add(MainManager.instance.flagvar[0]);
							this.RefreshDeckIndicator(deck.ToArray());
							oldselection = -1;
							if (MainManager.instance.flagvar[4] == 1)
							{
								state = 3;
								break;
							}
						}
						card = null;
						num = j;
					}
				}
				this.RefreshDeckIndicator(deck.ToArray());
				yield return null;
			}
			while (state < 3);
			if (!skipconfirm)
			{
				base.StartCoroutine(MainManager.SetText("|boxstyle,4||spd,0|" + this.carddiag[10] + "|fwait,0.1||prompt,card,0.5,2,-11,-11,6,7|", true, Vector3.zero, null, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				if (MainManager.instance.option == 0)
				{
					MainManager.instance.flagstring[7] = "";
					int[] array3 = deck.ToArray();
					for (int num5 = 0; num5 < array3.Length; num5++)
					{
						string[] array4 = MainManager.instance.flagstring;
						int num6 = 7;
						array4[num6] += array3[num5].ToString();
						if (num5 < array3.Length - 1)
						{
							string[] array5 = MainManager.instance.flagstring;
							int num7 = 7;
							array5[num7] += ",";
						}
					}
				}
			}
			this.SaveLastDeck(deck.ToArray());
			this.cards[0] = new List<int>();
			this.cards[0].AddRange(deck.ToArray());
			for (int num8 = 0; num8 < this.cardpreview.Length; num8++)
			{
				Object.Destroy(this.cardpreview[num8].gameObject);
			}
			this.windowid = 1;
			base.StartCoroutine(this.BuildWindow());
			break;
		}
		case 1:
			yield return new WaitForSeconds(0.25f);
			MainManager.instance.flagvar[6] = this.turn;
			MainManager.DialogueText(this.args + "|rainbow|" + this.carddiag[11] + "|fwait,2||end|", null, null);
			while (MainManager.instance.message)
			{
				yield return null;
			}
			if (this.huds == null)
			{
				this.CreateHUD();
			}
			base.StartCoroutine(this.PullCard(this.turn == 1));
			break;
		case 2:
		{
			this.LoadCardData();
			atk = new int[2];
			def = new int[2];
			this.entities[2].animstate = 13;
			this.entities[3].animstate = 13;
			atkfont = new DynamicFont[2];
			deffont = new DynamicFont[2];
			ai = new GameObject[2];
			di = new GameObject[2];
			this.selectedcards = new List<int>[2];
			yield return new WaitForSeconds(0.25f);
			for (int num9 = 0; num9 < this.playedcards[1].Count; num9++)
			{
				CardGame.Cards item = this.playedcards[1].ToArray()[num9];
				item.flipped = false;
				this.playedcards[1].Insert(num9, item);
				this.playedcards[1].RemoveAt(num9 + 1);
			}
			for (int num10 = 0; num10 < 2; num10++)
			{
				this.selectedcards[num10] = new List<int>();
				atkfont[num10] = DynamicFont.SetUp("00", false, true, 2f, 2, 10, Vector2.one * 2f, MainManager.GUICamera.transform, new Vector3((float)((num10 == 0) ? -1 : 1) * 6.75f, -4f, 10f), Color.white);
				deffont[num10] = DynamicFont.SetUp("00", false, true, 2f, 2, 10, Vector2.one * 2f, MainManager.GUICamera.transform, new Vector3((float)((num10 == 0) ? -1 : 1) * 3.25f, -4f, 10f), Color.white);
				ai[num10] = MainManager.NewUIObject("atkicon", atkfont[num10].transform, atkfont[num10].transform.position + new Vector3(-1f, 0.5f), Vector3.one, MainManager.guisprites[25]);
				di[num10] = MainManager.NewUIObject("deficon", deffont[num10].transform, deffont[num10].transform.position + new Vector3(-1f, 0.5f), Vector3.one, MainManager.guisprites[26]);
			}
			for (int i = 0; i < 2; i = num + 1)
			{
				this.playedcards[i] = (from a in this.playedcards[i]
				orderby (int)this.carddata[a.cardid].type
				select a).ToList<CardGame.Cards>();
				CardGame.Cards[] hand = this.playedcards[i].ToArray();
				int j = (i == 0) ? 1 : 0;
				for (int k = 0; k < hand.Length; k = num + 1)
				{
					this.selectedcards[i].Add(hand[k].cardid);
					CardGame.CardData cd = this.carddata[hand[k].cardid];
					if (cd.type != CardGame.Type.Attacker && cd.effects.Length > 0)
					{
						bool shine = true;
						int a2 = 0;
						while (a2 < cd.effects.GetLength(0))
						{
							CardGame.Effects effects = (CardGame.Effects)cd.effects[a2, 0];
							bool[] coin;
							if (effects <= CardGame.Effects.Summon)
							{
								if (effects != CardGame.Effects.RandomMiniboss)
								{
									if (effects != CardGame.Effects.Summon)
									{
										goto IL_1353;
									}
									goto IL_1431;
								}
								else
								{
									shine = false;
									int g = 0;
									do
									{
										g = this.miniboss[Random.Range(0, this.miniboss.Length)];
									}
									while (g == 28 || MainManager.instance.enemyencounter[this.carddata[g].enemyid, 0] == 0);
									CardGame.Cards c = hand[k];
									Vector3 cpos = c.cardobj.transform.localPosition;
									c.flipped = true;
									int f = 0;
									for (f = 0; f < this.playedcards[i].ToArray().Length; f = num + 1)
									{
										if (this.playedcards[i].ToArray()[f].cardobj == c.cardobj)
										{
											this.playedcards[i].RemoveAt(f);
											break;
										}
										num = f;
									}
									yield return new WaitForSeconds(0.3f);
									Object.Destroy(c.cardobj.gameObject);
									c = default(CardGame.Cards);
									c.cardobj = this.CreateCard(g, cpos, true);
									c.cardid = g;
									c.flipped = false;
									yield return new WaitForSeconds(0.3f);
									this.playedcards[i].Insert(f, c);
									cd = this.carddata[c.cardid];
									a2 = -1;
									yield return new WaitForSeconds(0.6f);
									hand = this.playedcards[i].ToArray();
									c = default(CardGame.Cards);
									cpos = default(Vector3);
								}
							}
							else if (effects != CardGame.Effects.SummonOnCoin)
							{
								if (effects != CardGame.Effects.SummonRandomFromTribe)
								{
									goto IL_1353;
								}
								List<int> list2 = new List<int>();
								for (int num11 = 0; num11 < this.carddata.Length; num11++)
								{
									if (this.carddata[num11].type != CardGame.Type.Boss && this.carddata[num11].type != CardGame.Type.Miniboss && this.carddata[num11].tribe.Contains((CardGame.Tribe)cd.effects[a2, 2]))
									{
										list2.Add(num11);
									}
								}
								if (list2.Count > 0)
								{
									cd.effects[a2, 1] = 1;
									cd.effects[a2, 2] = list2.ToArray()[Random.Range(0, list2.Count)];
									goto IL_1431;
								}
							}
							else
							{
								coin = new bool[cd.effects[a2, 1]];
								for (int g = 0; g < coin.Length; g = num + 1)
								{
									coin[g] = (Random.Range(0, 10) >= 5);
									base.StartCoroutine(this.CoinEffect(hand[k].cardobj.transform.position, coin[g], false));
									yield return (coin.Length > 1) ? EventControl.sec : EventControl.halfsec;
									if (coin[g])
									{
										MainManager.PlaySound("Charge");
										CardGame.Cards item2 = default(CardGame.Cards);
										item2.cardobj = this.CreateCard(cd.effects[a2, 2], hand[k].cardobj.transform.position, false);
										item2.cardid = cd.effects[a2, 2];
										item2.flipped = false;
										this.playedcards[i].Add(item2);
										yield return EventControl.halfsec;
									}
									num = g;
								}
							}
							IL_191E:
							coin = null;
							num = a2;
							a2 = num + 1;
							continue;
							IL_1353:
							shine = false;
							goto IL_191E;
							IL_1431:
							for (int g = 0; g < cd.effects[a2, 1]; g = num + 1)
							{
								MainManager.PlaySound("Charge");
								CardGame.Cards item3 = default(CardGame.Cards);
								item3.cardobj = this.CreateCard(cd.effects[a2, 2], hand[k].cardobj.transform.position, false);
								item3.cardid = cd.effects[a2, 2];
								item3.flipped = false;
								this.playedcards[i].Add(item3);
								yield return EventControl.halfsec;
								num = g;
							}
							goto IL_191E;
						}
						if (shine)
						{
							this.temproutine = base.StartCoroutine(this.Shine(hand[k]));
							while (this.temproutine != null)
							{
								yield return null;
							}
						}
					}
					cd = default(CardGame.CardData);
					num = k;
				}
				hand = this.playedcards[i].ToArray();
				atk[i] += this.attacknextturn[i];
				this.attacknextturn[i] = 0;
				for (int k = 0; k < hand.Length; k = num + 1)
				{
					CardGame.CardData cd = this.carddata[hand[k].cardid];
					bool shine = true;
					if (cd.type == CardGame.Type.Attacker)
					{
						atk[i] += cd.attack;
					}
					else
					{
						int a2 = 0;
						while (a2 < cd.effects.GetLength(0))
						{
							CardGame.Effects effects = (CardGame.Effects)cd.effects[a2, 0];
							bool[] coin;
							if (effects <= CardGame.Effects.Defense)
							{
								if (effects != CardGame.Effects.Attack)
								{
									if (effects != CardGame.Effects.Defense)
									{
										goto IL_1B3A;
									}
									goto IL_1FF7;
								}
								else
								{
									atk[i] += cd.effects[a2, 1];
								}
							}
							else
							{
								switch (effects)
								{
								case CardGame.Effects.AttackOnCoin:
								case CardGame.Effects.DefenseOnCoin:
									coin = new bool[cd.effects[a2, 2]];
									for (int f = 0; f < coin.Length; f = num + 1)
									{
										coin[f] = (Random.Range(0, 10) >= 5);
										base.StartCoroutine(this.CoinEffect(hand[k].cardobj.transform.position, coin[f], false));
										yield return new WaitForSeconds(0.15f);
										if (coin[f])
										{
											if (cd.effects[a2, 0] == 6)
											{
												atk[i] += cd.effects[a2, 1];
											}
											else
											{
												def[i] += cd.effects[a2, 1];
											}
										}
										num = f;
									}
									break;
								case CardGame.Effects.SummonOnCoin:
								case CardGame.Effects.NumbFront:
								case CardGame.Effects.NumbFrontCoin:
								case CardGame.Effects.MultiplyPerTribe:
								case CardGame.Effects.MultiplyAttackPerID:
								case CardGame.Effects.NumbAll:
								case CardGame.Effects.NumbAllCoin:
								case CardGame.Effects.DamageOnWin:
								case CardGame.Effects.HealIfOtherCard:
								case CardGame.Effects.MultiplyIfOpponentID:
								case CardGame.Effects.MultiplyIfOpponentTribe:
								case CardGame.Effects.IgnoreDefense:
								case CardGame.Effects.HealIfAttackAmmount:
									goto IL_1B3A;
								case CardGame.Effects.AttackPerTribe:
									atk[i] += this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], i) * cd.effects[a2, 1];
									shine = true;
									break;
								case CardGame.Effects.AttackPerID:
									atk[i] += this.GetCardQuantityID(cd.effects[a2, 2], i) * cd.effects[a2, 1];
									shine = true;
									break;
								case CardGame.Effects.AttackIfOtherCard:
									if (this.GetCardQuantityID(cd.effects[a2, 2], i) > 0)
									{
										atk[i] += cd.effects[a2, 1];
										shine = true;
									}
									break;
								case CardGame.Effects.AttackIfOpponentID:
									if (this.GetCardQuantityID(cd.effects[a2, 2], j) > 0)
									{
										atk[i] += cd.effects[a2, 1];
									}
									break;
								case CardGame.Effects.AttackIfOpponentTribe:
									if (this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], j) > 0)
									{
										atk[i] += cd.effects[a2, 1];
									}
									break;
								case CardGame.Effects.DefenseIfOpponentID:
									if (this.GetCardQuantityID(cd.effects[a2, 2], j) > 0)
									{
										def[i] += cd.effects[a2, 1];
									}
									break;
								case CardGame.Effects.DefenseIfOpponentTribe:
									if (this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], j) > 0)
									{
										def[i] += cd.effects[a2, 1];
									}
									break;
								case CardGame.Effects.AttackPerOpponentTribe:
									atk[i] += this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], j) * cd.effects[a2, 1];
									shine = true;
									break;
								case CardGame.Effects.AttackPerOpponentID:
									atk[i] += this.GetCardQuantityID(cd.effects[a2, 2], j) * cd.effects[a2, 1];
									shine = true;
									break;
								case CardGame.Effects.DefensePerOpponentTribe:
									def[i] += this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], j) * cd.effects[a2, 1];
									break;
								case CardGame.Effects.DefensePerOpponentID:
									def[i] += this.GetCardQuantityID(cd.effects[a2, 2], j) * cd.effects[a2, 1];
									break;
								case CardGame.Effects.AttackOrDefenseCoin:
									coin = new bool[cd.effects[a2, 2]];
									for (int f = 0; f < coin.Length; f = num + 1)
									{
										coin[f] = (Random.Range(0, 10) >= 5);
										base.StartCoroutine(this.CoinEffect(hand[k].cardobj.transform.position, coin[f], true));
										yield return new WaitForSeconds(0.15f);
										if (coin[f])
										{
											atk[i] += cd.effects[a2, 1];
										}
										else
										{
											def[i] += cd.effects[a2, 1];
										}
										num = f;
									}
									break;
								default:
									if (effects != CardGame.Effects.DefenseOnOtherCard)
									{
										goto IL_1B3A;
									}
									if (this.GetCardQuantityID(cd.effects[a2, 2], i) > 0)
									{
										goto IL_1FF7;
									}
									break;
								}
							}
							IL_222F:
							coin = null;
							num = a2;
							a2 = num + 1;
							continue;
							IL_1B3A:
							shine = false;
							goto IL_222F;
							IL_1FF7:
							def[i] += cd.effects[a2, 1];
							goto IL_222F;
						}
					}
					atkfont[i].text = atk[i].ToString().PadLeft(2, '0');
					deffont[i].text = def[i].ToString().PadLeft(2, '0');
					if (shine)
					{
						this.temproutine = base.StartCoroutine(this.Shine(hand[k]));
						while (this.temproutine != null)
						{
							yield return null;
						}
					}
					cd = default(CardGame.CardData);
					num = k;
				}
				hand = null;
				num = i;
			}
			for (int i = 0; i < 2; i = num + 1)
			{
				int j = (i == 0) ? 1 : 0;
				CardGame.Cards[] hand = this.playedcards[i].ToArray();
				List<int>[] once = new List<int>[2];
				for (int num12 = 0; num12 < once.Length; num12++)
				{
					once[num12] = new List<int>();
				}
				for (int k = 0; k < hand.Length; k = num + 1)
				{
					CardGame.CardData cd = this.carddata[hand[k].cardid];
					if (cd.type != CardGame.Type.Attacker)
					{
						bool shine = true;
						int a2 = 0;
						while (a2 < cd.effects.GetLength(0))
						{
							CardGame.Effects effects = (CardGame.Effects)cd.effects[a2, 0];
							switch (effects)
							{
							case CardGame.Effects.Heal:
								goto IL_2847;
							case CardGame.Effects.HealIfWin:
							case CardGame.Effects.Summon:
							case CardGame.Effects.AttackOnCoin:
							case CardGame.Effects.DefenseOnCoin:
							case CardGame.Effects.SummonOnCoin:
							case CardGame.Effects.AttackPerTribe:
								goto IL_24CB;
							case CardGame.Effects.NumbFront:
							case CardGame.Effects.NumbAll:
								goto IL_25E2;
							case CardGame.Effects.NumbFrontCoin:
							case CardGame.Effects.NumbAllCoin:
							{
								bool coin2 = Random.Range(0, 10) <= 5;
								base.StartCoroutine(this.CoinEffect(hand[k].cardobj.transform.position, coin2, false));
								yield return EventControl.halfsec;
								if (coin2)
								{
									cd.effects[a2, 0] = ((cd.effects[a2, 0] == 15) ? 14 : 9);
									goto IL_25E2;
								}
								break;
							}
							case CardGame.Effects.MultiplyPerTribe:
								shine = true;
								atk[i] += this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], i) * cd.effects[a2, 1];
								break;
							case CardGame.Effects.MultiplyAttackPerID:
								shine = true;
								atk[i] += this.GetCardQuantityID(cd.effects[a2, 2], i) * cd.effects[a2, 1];
								break;
							default:
								if (effects != CardGame.Effects.HealIfOtherCard)
								{
									switch (effects)
									{
									case CardGame.Effects.IgnoreDefense:
										def[j] = Mathf.Clamp(def[j] - cd.effects[a2, 1], 0, 99);
										deffont[j].text = def[j].ToString().PadLeft(2, '0');
										break;
									case CardGame.Effects.HealIfAttackAmmount:
										if (atk[i] >= cd.effects[a2, 2])
										{
											MainManager.HealParticle(this.entities[2 + i].transform, Vector3.one, Vector3.up);
											MainManager.PlaySound("Heal");
											this.hp[i] = Mathf.Clamp(this.hp[i] + cd.effects[a2, 1], 0, 5);
										}
										break;
									default:
										goto IL_24CB;
									case CardGame.Effects.AttackPerTribeOnce:
										if (!once[0].Contains(hand[k].cardid))
										{
											atk[i] += this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], i) * cd.effects[a2, 1];
											once[0].Add(hand[k].cardid);
											shine = true;
										}
										break;
									case CardGame.Effects.AttackPerIDOnce:
										if (!once[1].Contains(hand[k].cardid))
										{
											shine = true;
											atk[i] += this.GetCardQuantityID(cd.effects[a2, 2], i) * cd.effects[a2, 1];
											once[1].Add(hand[k].cardid);
										}
										break;
									case CardGame.Effects.AttackOnce:
										if (!once[0].Contains(hand[k].cardid))
										{
											atk[i] += cd.effects[a2, 1];
											once[0].Add(hand[k].cardid);
										}
										break;
									case CardGame.Effects.Heal1OnTribeQuanity:
										if (this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], i) > cd.effects[a2, 1])
										{
											cd.effects[a2, 1] = 1;
											goto IL_2847;
										}
										break;
									case CardGame.Effects.NumbIfTribeAmmount:
										if (this.GetCardQuantityTribe((CardGame.Tribe)cd.effects[a2, 2], i) >= cd.effects[a2, 1])
										{
											cd.effects[a2, 0] = 9;
											goto IL_25E2;
										}
										break;
									case CardGame.Effects.NumbIfOtherCard:
										if (this.GetCardQuantityID(cd.effects[a2, 2], i) > 0)
										{
											cd.effects[a2, 0] = 9;
											goto IL_25E2;
										}
										break;
									case CardGame.Effects.AttackNextTurn:
										this.attacknextturn[i] = cd.effects[a2, 1];
										break;
									}
								}
								else if (this.GetCardQuantityID(cd.effects[a2, 2], i) > 0)
								{
									MainManager.HealParticle(this.entities[2 + i].transform, Vector3.one, Vector3.up);
									MainManager.PlaySound("Heal");
									this.hp[i] = Mathf.Clamp(this.hp[i] + cd.effects[a2, 1], 0, 5);
								}
								break;
							}
							IL_2CDC:
							num = a2;
							a2 = num + 1;
							continue;
							IL_24CB:
							shine = false;
							goto IL_2CDC;
							IL_25E2:
							for (int f = 0; f < this.playedcards[j].Count; f = num + 1)
							{
								CardGame.Cards cards2 = this.playedcards[j].ToArray()[f];
								if (this.carddata[cards2.cardid].type == CardGame.Type.Attacker && !cards2.flipped)
								{
									MainManager.PlaySound("Lazer");
									cards2.flipped = true;
									atk[j] = Mathf.Clamp(atk[j] - this.carddata[cards2.cardid].attack, 0, 99);
									this.playedcards[j].Insert(f, cards2);
									this.playedcards[j].RemoveAt(f + 1);
									this.temproutine = base.StartCoroutine(this.Shine(hand[k]));
									while (this.temproutine != null)
									{
										yield return null;
									}
									if (cd.effects[a2, 0] == 9)
									{
										break;
									}
									yield return EventControl.halfsec;
								}
								num = f;
							}
							shine = false;
							atkfont[j].text = atk[j].ToString().PadLeft(2, '0');
							goto IL_2CDC;
							IL_2847:
							MainManager.HealParticle(this.entities[2 + i].transform, Vector3.one, Vector3.up);
							MainManager.PlaySound("Heal");
							this.hp[i] = Mathf.Clamp(this.hp[i] + cd.effects[a2, 1], 0, 5);
							goto IL_2CDC;
						}
						atkfont[i].text = atk[i].ToString().PadLeft(2, '0');
						deffont[i].text = def[i].ToString().PadLeft(2, '0');
						if (shine)
						{
							this.temproutine = base.StartCoroutine(this.Shine(hand[k]));
							while (this.temproutine != null)
							{
								yield return null;
							}
						}
					}
					cd = default(CardGame.CardData);
					num = k;
				}
				hand = null;
				once = null;
				num = i;
			}
			cardmoves = new Transform[2];
			yield return new WaitForSeconds(0.5f);
			this.cardanim = true;
			for (int i = 0; i < 2; i = num + 1)
			{
				int j = (i == 0) ? 1 : 0;
				Transform card = deffont[j].transform;
				deffont[j].enabled = false;
				di[j].transform.parent = card;
				float b = 0f;
				float a3 = 20f;
				Vector3 cpos = card.localPosition;
				do
				{
					card.localPosition = Vector3.Lerp(cpos, atkfont[i].transform.localPosition + new Vector3(0f, 0f, -1f), b / a3);
					b += MainManager.TieFramerate(1f);
					yield return null;
				}
				while (b < a3);
				card.transform.localPosition = new Vector3(0f, 999f);
				atk[i] = Mathf.Clamp(atk[i] - def[j], 0, 99);
				def[j] = 0;
				atkfont[i].text = atk[i].ToString().PadLeft(2, '0');
				yield return new WaitForSeconds(0.1f);
				cardmoves[i] = new GameObject().transform;
				cardmoves[i].parent = MainManager.GUICamera.transform;
				cardmoves[i].localPosition = Vector3.zero;
				if (i == 0)
				{
					cardmoves[i].tag = "Player";
				}
				for (int num13 = 0; num13 < this.playedcards[i].Count; num13++)
				{
					if (this.playedcards[i].ToArray()[num13].cardobj != null)
					{
						this.playedcards[i].ToArray()[num13].cardobj.parent = cardmoves[i];
					}
				}
				card = null;
				cpos = default(Vector3);
				num = i;
			}
			float x = 0f;
			float y = 50f;
			if (this.selectedcards[0].Count > 0 || this.selectedcards[1].Count > 0)
			{
				MainManager.PlaySound("Toss11");
				do
				{
					for (int num14 = 0; num14 < 2; num14++)
					{
						cardmoves[num14].transform.localPosition = new Vector3(MainManager.BeizierCurve3(Vector3.zero, Vector3.down, 3f, x / y).y * (float)((num14 == 0) ? -1 : 1), 0f, 0f);
					}
					x += MainManager.TieFramerate(1f);
					yield return null;
				}
				while (x < y + 1f);
			}
			winstate = null;
			if (atk[0] > atk[1])
			{
				winstate = new bool?(true);
			}
			else if (atk[1] > atk[0])
			{
				winstate = new bool?(false);
			}
			x = 0f;
			y = 30f;
			MainManager.ShakeScreen(0.1f, 0.3f, true);
			if (winstate != null)
			{
				float a3 = winstate.Value ? cardmoves[0].transform.localPosition.x : cardmoves[1].transform.localPosition.x;
				base.StartCoroutine(this.BreakCards(cardmoves[winstate.Value ? 1 : 0]));
				if (winstate.Value)
				{
					MainManager.PlaySound("CrowdCheer2", -1, 1.2f, 1f);
					MainManager.PlaySound("Damage0");
				}
				else
				{
					MainManager.PlaySound("CrowdGasp");
					MainManager.PlaySound("Death3");
				}
				int i = winstate.Value ? 0 : 1;
				this.hp[winstate.Value ? 1 : 0]--;
				this.audience.Jump();
				do
				{
					cardmoves[i].transform.localPosition = new Vector3(Mathf.Lerp(a3, (float)(20 * ((i == 0) ? 1 : -1)), x / y), 0f, 0f);
					x += MainManager.TieFramerate(1f);
					yield return null;
				}
				while (x < y + 1f);
				if (winstate.Value)
				{
					this.entities[2].animstate = 102;
					this.entities[0].animstate = 8;
					this.entities[1].animstate = 8;
					this.entities[3].animstate = 11;
				}
				else
				{
					this.entities[2].animstate = 11;
					this.entities[0].animstate = 5;
					this.entities[1].animstate = 105;
				}
				if (this.hp[0] <= 0 || this.hp[1] <= 0)
				{
					yield return new WaitForSeconds(0.5f);
					this.windowid = 10;
					MainManager.battleresult = winstate.Value;
					for (int num15 = 0; num15 < 2; num15++)
					{
						if (cardmoves[num15] != null)
						{
							Object.Destroy(cardmoves[num15].gameObject);
						}
						Object.Destroy(atkfont[num15].gameObject);
						Object.Destroy(deffont[num15].gameObject);
						Object.Destroy(ai[num15].gameObject);
						Object.Destroy(di[num15].gameObject);
						Object.Destroy(this.cursor.gameObject);
					}
					if (MainManager.battleresult)
					{
						this.entities[2].animstate = 4;
						this.entities[0].animstate = 8;
						this.entities[1].animstate = 8;
						if (!MainManager.instance.flags[181])
						{
							this.entities[3].animstate = 18;
						}
						else
						{
							this.entities[3].animstate = 0;
						}
					}
					else
					{
						this.entities[2].animstate = 17;
						this.entities[0].animstate = 10;
						this.entities[1].animstate = 17;
					}
					Object.Destroy(this.keyhelp.gameObject);
					base.StartCoroutine(this.BuildWindow());
					yield break;
				}
			}
			else
			{
				MainManager.PlaySound("Fail");
				base.StartCoroutine(this.BreakCards(cardmoves[0]));
				base.StartCoroutine(this.BreakCards(cardmoves[1]));
				yield return new WaitForSeconds(1f);
			}
			if (winstate != null)
			{
				int num16 = 0;
				int num17 = winstate.Value ? 0 : 1;
				cardmoves[num17].transform.position = new Vector3(0f, 999f);
				CardGame.Cards[] array6 = this.playedcards[num17].ToArray();
				for (int num18 = 0; num18 < array6.Length; num18++)
				{
					CardGame.CardData cardData = this.carddata[array6[num18].cardid];
					if (cardData.type != CardGame.Type.Attacker)
					{
						for (int num19 = 0; num19 < cardData.effects.GetLength(0); num19++)
						{
							CardGame.Effects effects = (CardGame.Effects)cardData.effects[num19, 0];
							if (effects != CardGame.Effects.HealIfWin)
							{
								if (effects != CardGame.Effects.DamageOnWin)
								{
									if (effects == CardGame.Effects.HealIfWinOnce)
									{
										if (cardData.effects[num19, 1] > num16)
										{
											num16 = cardData.effects[num19, 1];
											MainManager.PlaySound("Heal");
											MainManager.HealParticle(this.entities[2 + num17].transform, Vector3.one, Vector3.up);
										}
									}
								}
							}
							else
							{
								this.hp[num17] = Mathf.Clamp(this.hp[num17] + cardData.effects[num19, 1], 0, 5);
								MainManager.PlaySound("Heal");
								MainManager.HealParticle(this.entities[2 + num17].transform, Vector3.one, Vector3.up);
							}
						}
					}
				}
				if (num16 > 0)
				{
					this.hp[num17] = Mathf.Clamp(this.hp[num17] + num16, 0, 5);
				}
			}
			for (int num20 = 0; num20 < 2; num20++)
			{
				if (cardmoves[num20] != null)
				{
					Object.Destroy(cardmoves[num20].gameObject);
				}
				this.playedcards[num20] = new List<CardGame.Cards>();
				Object.Destroy(atkfont[num20].gameObject);
				Object.Destroy(deffont[num20].gameObject);
				Object.Destroy(ai[num20].gameObject);
				Object.Destroy(di[num20].gameObject);
			}
			this.windowid = 1;
			this.turn++;
			base.StartCoroutine(this.BuildWindow());
			break;
		}
		default:
			if (num == 10)
			{
				this.SaveLastDeck(this.cards[0].ToArray());
				yield return new WaitForSeconds(0.5f);
				MainManager.PlaySound(MainManager.battleresult ? "CrowdClap" : "CrowdGasp", -1, MainManager.battleresult ? 1f : 0.75f, 1f);
				MainManager.DialogueText(this.args + (MainManager.battleresult ? "|rainbow|" : "|color,4|") + this.carddiag[MainManager.battleresult ? 13 : 12] + "|fwait,2||end|", null, null);
				while (MainManager.instance.message)
				{
					yield return null;
				}
				if (MainManager.instance.flags[181])
				{
					base.StartCoroutine(MainManager.SetText(this.carddiag[MainManager.battleresult ? 24 : 25], true, Vector3.zero, this.entities[3].transform, null));
					while (MainManager.instance.message)
					{
						yield return null;
					}
				}
				MainManager.instance.message = true;
				for (int num21 = 0; num21 < 2; num21++)
				{
					Object.Destroy(this.huds[num21, 0].gameObject);
					Object.Destroy(this.huds[num21, 1].gameObject);
					for (int num22 = 0; num22 < this.handcards[num21].Count; num22++)
					{
						Object.Destroy(this.handcards[num21].ToArray()[num22].cardobj.gameObject);
					}
				}
				MainManager.instance.message = false;
				MainManager.FadeIn();
				MainManager.FadeMusic(0.03f);
				yield return new WaitForSeconds(1.25f);
				Object.Destroy(this.map.gameObject);
				Object.Destroy(this.paperoverlay.gameObject);
				this.started = false;
				yield return null;
				MainManager.ResetCamera(true);
				yield return null;
				MainManager.LoadCameraPosition();
				MainManager.map.gameObject.SetActive(true);
				for (int num23 = 0; num23 < MainManager.instance.playerdata.Length; num23++)
				{
					MainManager.instance.playerdata[num23].entity.gameObject.SetActive(true);
				}
				yield return new WaitForSeconds(0.5f);
				if (!this.pausev[1])
				{
					MainManager.FadeOut();
					if (MainManager.map.music.Length != 0 && MainManager.map.music[0] != null)
					{
						MainManager.ChangeMusic(MainManager.map.music[0]);
					}
					yield return new WaitForSeconds(1f);
					while (MainManager.musiccoroutine != null)
					{
						yield return null;
					}
					MainManager.instance.minipause = this.pausev[0];
					MainManager.instance.inevent = this.pausev[1];
				}
				MainManager.overridedlist = null;
				MainManager.savelastlist = false;
				if (!MainManager.instance.librarystuff[0, 45])
				{
					MainManager.UpdateJounal(MainManager.Library.Discovery, 45);
				}
				Object.Destroy(this);
			}
			break;
		}
		deck = null;
		atk = null;
		def = null;
		atkfont = null;
		deffont = null;
		ai = null;
		di = null;
		cardmoves = null;
		winstate = null;
		yield break;
	}

	// Token: 0x06000180 RID: 384 RVA: 0x00012BA4 File Offset: 0x00010DA4
	private void SaveLastDeck(int[] d)
	{
		MainManager.instance.flagstring[12] = "";
		for (int i = 0; i < d.Length; i++)
		{
			string[] array = MainManager.instance.flagstring;
			int num = 12;
			array[num] += d[i].ToString();
			if (i < d.Length - 1)
			{
				string[] array2 = MainManager.instance.flagstring;
				int num2 = 12;
				array2[num2] += ",";
			}
		}
	}

	// Token: 0x06000181 RID: 385 RVA: 0x00012C1C File Offset: 0x00010E1C
	private IEnumerator BreakCards(Transform holder)
	{
		Vector3[] p = new Vector3[holder.childCount];
		Vector3[] s = new Vector3[holder.childCount];
		for (int i = 0; i < p.Length; i++)
		{
			s[i] = holder.GetChild(i).transform.position;
			float num = (float)(holder.CompareTag("Player") ? -1 : 1);
			p[i] = new Vector3(Random.Range(5f * num, 20f * num), (float)Random.Range(-15, 15), s[i].z);
		}
		float a = 0f;
		float b = 15f;
		while (holder != null)
		{
			for (int j = 0; j < p.Length; j++)
			{
				holder.GetChild(j).position = Vector3.Lerp(s[j], p[j], a / b);
				holder.GetChild(j).eulerAngles += new Vector3(0f, 0f, MainManager.TieFramerate(20f));
			}
			a += MainManager.TieFramerate(1f);
			yield return null;
			if (a >= b + 1f)
			{
				if (holder != null)
				{
					Object.Destroy(holder.gameObject);
				}
				yield return null;
				yield break;
			}
		}
		yield break;
	}

	// Token: 0x06000182 RID: 386 RVA: 0x00012C2B File Offset: 0x00010E2B
	private IEnumerator CoinEffect(Vector3 position, bool heads, bool atkdefeffect)
	{
		float a = (float)(heads ? 0 : 180);
		float time = 0f;
		float spin = 540f + a;
		MainManager.PlaySound("Coin");
		GameObject t = Object.Instantiate(Resources.Load("Prefabs/Objects/Coin"), position + new Vector3(0f, 0f, -0.2f), Quaternion.Euler(0f, a, 0f), MainManager.GUICamera.transform) as GameObject;
		t.transform.localScale *= 0.75f;
		t.transform.localPosition = new Vector3(t.transform.localPosition.x, t.transform.localPosition.y, 1.5f);
		do
		{
			t.transform.eulerAngles = new Vector3(0f, Mathf.Lerp(spin, a, time / 30f));
			time += MainManager.framestep;
			yield return null;
		}
		while (time < 30f);
		if (!atkdefeffect)
		{
			MainManager.PlaySound(heads ? "AtkSuccess" : "AtkFail");
		}
		Object.Destroy(t.gameObject, 0.5f);
		yield return null;
		yield break;
	}

	// Token: 0x06000183 RID: 387 RVA: 0x00012C48 File Offset: 0x00010E48
	private IEnumerator Shine(CardGame.Cards card)
	{
		int num = MainManager.SoundIsPlaying("CardSound2");
		if (num == -1 || MainManager.sounds[num].time > 0.2f)
		{
			MainManager.PlaySound("CardSound2", -1, 1.2f, 0.5f);
		}
		Vector3 p = card.cardobj.transform.localPosition;
		card.cardobj.transform.localPosition = new Vector3(card.cardobj.transform.localPosition.x, card.cardobj.transform.localPosition.y, 5f);
		MainManager.templetter = base.StartCoroutine(MainManager.TempColor(Color.white, 15f, card.cardobj.GetComponent<SpriteRenderer>()));
		while (MainManager.templetter != null)
		{
			yield return null;
		}
		card.cardobj.transform.localPosition = p;
		this.temproutine = null;
		yield break;
	}

	// Token: 0x06000184 RID: 388 RVA: 0x00012C60 File Offset: 0x00010E60
	private int GetCardQuantityID(int id, int playedid)
	{
		int num = 0;
		CardGame.Cards[] array = this.playedcards[playedid].ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (this.carddata[array[i].cardid].noid == id)
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000185 RID: 389 RVA: 0x00012CB0 File Offset: 0x00010EB0
	private int GetCardQuantityTribe(CardGame.Tribe tribe, int playedid)
	{
		int num = 0;
		CardGame.Cards[] array = this.playedcards[playedid].ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (this.carddata[array[i].cardid].tribe.ToArray<CardGame.Tribe>().Contains(tribe))
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x06000186 RID: 390 RVA: 0x00012D0C File Offset: 0x00010F0C
	private void CreateHUD()
	{
		this.huds = new Transform[2, 2];
		this.hp = new int[]
		{
			5,
			5
		};
		this.hudfont = new DynamicFont[2, 2];
		for (int i = 0; i < 2; i++)
		{
			float num = (float)((i == 0) ? -1 : 1);
			for (int j = 0; j < 2; j++)
			{
				SpriteRenderer component = MainManager.NewUIObject("hud", MainManager.GUICamera.transform, new Vector3(((j == 0) ? 7.65f : 5.25f) * num, 10f, 10f), new Vector3(0.4f, 0.65f, 1f), MainManager.guisprites[4], 10).GetComponent<SpriteRenderer>();
				this.huds[i, j] = component.transform;
				component.color = MainManager.instance.menucolors[(j == 0) ? 0 : 3];
				MainManager.NewUIObject("icon", component.transform, new Vector3(-1.8f, 0f, 0f), (j == 0) ? new Vector3(1.5f, 1f, 1f) : new Vector3(2f, 1.3f, 1f), MainManager.guisprites[(j == 0) ? 24 : 28], component.sortingOrder + 1);
				this.hudfont[i, j] = DynamicFont.SetUp((j == 0) ? "0/0" : "00", false, true, 20f, 2, component.sortingOrder + 2, new Vector2(2f, 1.8f), component.transform, new Vector3(0f, -0.6f), Color.white);
			}
		}
	}

	// Token: 0x06000187 RID: 391 RVA: 0x00012EBD File Offset: 0x000110BD
	private IEnumerator PullCard(bool startdeck)
	{
		this.option = 0;
		this.tp = new int[]
		{
			Mathf.Clamp(this.turn + 1, 2, 10),
			Mathf.Clamp(this.turn + 1, 2, 10)
		};
		for (int i = 0; i < 2; i++)
		{
			if (this.handcards[i].Count < 5)
			{
				List<int> list = new List<int>(this.cards[i]);
				if (this.selectedcards != null)
				{
					for (int j = 0; j < this.selectedcards[i].Count; j++)
					{
						list.Remove(this.selectedcards[i].ToArray()[j]);
					}
				}
				for (int k = 0; k < this.handcards[i].Count; k++)
				{
					list.Remove(this.handcards[i].ToArray()[k].cardid);
				}
				MainManager.RandomSort(ref list);
				int[] array = list.ToArray();
				int num = Mathf.Clamp((startdeck || this.handcards[i].Count == 0) ? 3 : 2, 0, array.Length - 1);
				for (int l = 0; l < num; l++)
				{
					if (this.handcards[i].Count < 5)
					{
						if (MainManager.instance.flags[181] && this.tutorial == 0 && i == 1)
						{
							if (l < 2)
							{
								array[l] = 13;
							}
							else if (l == 2)
							{
								array[l] = 4;
							}
						}
						CardGame.Cards cards = default(CardGame.Cards);
						cards.cardid = array[l];
						cards.cardobj = this.CreateCard(array[l], new Vector2((float)((i == 0) ? -10 : 10), 0f), i == 1);
						cards.cardobj.transform.localScale = new Vector3(1f, 1f, 0.15f);
						cards.flipped = (i == 1);
						this.handcards[i].Add(cards);
					}
				}
			}
		}
		if (startdeck)
		{
			this.fulldeck = new List<int>[2];
			for (int m = 0; m < 2; m++)
			{
				this.fulldeck[m] = new List<int>(this.cards[m].ToArray());
			}
		}
		this.maxoptions = this.handcards[0].Count;
		yield return null;
		this.entities[2].animstate = 19;
		this.entities[3].animstate = 13;
		this.cardanim = false;
		if (MainManager.instance.flags[181])
		{
			if (this.tutorial == 0)
			{
				base.StartCoroutine(MainManager.SetText(this.carddiag[22], true, Vector3.zero, this.entities[3].transform, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				this.tutorial = 1;
			}
			else if (this.tutorial == 1)
			{
				base.StartCoroutine(MainManager.SetText(this.carddiag[23], true, Vector3.zero, this.entities[3].transform, null));
				while (MainManager.instance.message)
				{
					yield return null;
				}
				this.tutorial = 2;
			}
		}
		this.caninput = true;
		if (this.keyhelp != null)
		{
			this.keyhelp.transform.localPosition = new Vector3(0f, 0f, 10f);
		}
		yield break;
	}

	// Token: 0x040000DD RID: 221
	public string[] carddiag;

	// Token: 0x040000DE RID: 222
	private bool caninput;

	// Token: 0x040000DF RID: 223
	private bool cardanim;

	// Token: 0x040000E0 RID: 224
	private int option;

	// Token: 0x040000E1 RID: 225
	private int maxoptions;

	// Token: 0x040000E2 RID: 226
	private int windowid;

	// Token: 0x040000E3 RID: 227
	private int turn = 1;

	// Token: 0x040000E4 RID: 228
	private int tutorial;

	// Token: 0x040000E5 RID: 229
	private int[] hp;

	// Token: 0x040000E6 RID: 230
	private int[] tp;

	// Token: 0x040000E7 RID: 231
	private int[] attacknextturn;

	// Token: 0x040000E8 RID: 232
	private int[] boss;

	// Token: 0x040000E9 RID: 233
	private int[] miniboss;

	// Token: 0x040000EA RID: 234
	public bool finalboss;

	// Token: 0x040000EB RID: 235
	private string args = "|boxstyle,-1||size,2||dropshadow,-0.1||center||line||line|";

	// Token: 0x040000EC RID: 236
	private List<int>[] selectedcards;

	// Token: 0x040000ED RID: 237
	private const int flagstring = 7;

	// Token: 0x040000EE RID: 238
	public static int[] order;

	// Token: 0x040000EF RID: 239
	public static readonly int[][] pdecks = new int[][]
	{
		new int[]
		{
			39,
			5,
			4,
			24,
			24,
			24,
			24,
			11,
			11,
			11,
			11,
			12,
			12,
			12,
			12,
			10,
			10,
			10,
			9,
			9
		},
		new int[]
		{
			35,
			33,
			5,
			25,
			25,
			25,
			27,
			27,
			27,
			30,
			30,
			29,
			29,
			29,
			34
		},
		new int[]
		{
			2,
			4,
			5,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			13,
			8,
			8,
			14,
			14
		},
		new int[]
		{
			36,
			7,
			28,
			21,
			21,
			21,
			21,
			21,
			21,
			21,
			8,
			8,
			8,
			22,
			22,
			22,
			20,
			20,
			20,
			20
		},
		new int[]
		{
			40,
			4,
			5,
			13,
			13,
			13,
			13,
			13,
			15,
			15,
			15,
			15,
			15,
			22,
			22,
			22,
			22,
			22,
			23,
			23
		},
		new int[]
		{
			26,
			5,
			6,
			18,
			18,
			18,
			18,
			18,
			18,
			18,
			16,
			16,
			16,
			17,
			17,
			14,
			14,
			14,
			15,
			15
		},
		new int[]
		{
			3,
			65,
			6,
			17,
			17,
			18,
			62,
			62,
			62,
			58,
			58,
			18,
			16,
			16,
			14,
			14,
			14,
			15,
			15,
			58
		},
		new int[]
		{
			80,
			81,
			82,
			70,
			70,
			59,
			59,
			56,
			56,
			22,
			22,
			14,
			14,
			71,
			9,
			9,
			30,
			30,
			30,
			51
		},
		new int[]
		{
			45,
			90,
			91,
			51,
			51,
			51,
			8,
			52,
			52,
			52,
			8,
			8,
			73,
			73,
			23,
			31,
			31,
			27,
			27,
			27
		},
		new int[]
		{
			83,
			33,
			19,
			42,
			42,
			42,
			43,
			43,
			43,
			1,
			1,
			1,
			46,
			46,
			46,
			0,
			14,
			14,
			0,
			0
		},
		new int[]
		{
			66,
			63,
			64,
			67,
			67,
			67,
			67,
			68,
			68,
			68,
			68,
			68,
			69,
			69,
			69,
			8,
			8,
			70,
			52,
			52
		},
		new int[]
		{
			61,
			60,
			76,
			20,
			20,
			20,
			31,
			31,
			31,
			31,
			31,
			86,
			86,
			86,
			87,
			87,
			11,
			12,
			24,
			71
		},
		new int[]
		{
			50,
			77,
			78,
			20,
			20,
			20,
			31,
			31,
			31,
			31,
			31,
			86,
			86,
			86,
			87,
			87,
			11,
			12,
			24,
			71
		},
		new int[]
		{
			57,
			19,
			84,
			53,
			53,
			53,
			53,
			54,
			54,
			54,
			54,
			55,
			55,
			55,
			55,
			56,
			59,
			59,
			89,
			89
		},
		new int[]
		{
			79,
			4,
			5,
			13,
			13,
			13,
			13,
			13,
			15,
			15,
			15,
			15,
			15,
			22,
			22,
			22,
			22,
			22,
			23,
			23
		}
	};

	// Token: 0x040000F0 RID: 240
	private Transform[] cardpreview;

	// Token: 0x040000F1 RID: 241
	private Transform[,] huds;

	// Token: 0x040000F2 RID: 242
	private DynamicFont[,] hudfont;

	// Token: 0x040000F3 RID: 243
	private SpriteRenderer paperoverlay;

	// Token: 0x040000F4 RID: 244
	private bool started;

	// Token: 0x040000F5 RID: 245
	private Transform map;

	// Token: 0x040000F6 RID: 246
	private Transform cursor;

	// Token: 0x040000F7 RID: 247
	private Transform keyhelp;

	// Token: 0x040000F8 RID: 248
	public List<CardGame.Cards>[] handcards;

	// Token: 0x040000F9 RID: 249
	public List<CardGame.Cards>[] playedcards;

	// Token: 0x040000FA RID: 250
	public CardGame.CardData[] carddata;

	// Token: 0x040000FB RID: 251
	private List<int>[] cards;

	// Token: 0x040000FC RID: 252
	private List<int>[] lastcards;

	// Token: 0x040000FD RID: 253
	private List<int>[] fulldeck;

	// Token: 0x040000FE RID: 254
	private const int decklimit = 15;

	// Token: 0x040000FF RID: 255
	private const int handlimit = 5;

	// Token: 0x04000100 RID: 256
	private const int tplimit = 10;

	// Token: 0x04000101 RID: 257
	private const int hplimit = 5;

	// Token: 0x04000102 RID: 258
	private const int minibossammount = 2;

	// Token: 0x04000103 RID: 259
	private const int carddraws = 2;

	// Token: 0x04000104 RID: 260
	private const int startcardamt = 3;

	// Token: 0x04000105 RID: 261
	private Audience audience;

	// Token: 0x04000106 RID: 262
	private EntityControl[] entities;

	// Token: 0x04000107 RID: 263
	private bool[] pausev;

	// Token: 0x04000108 RID: 264
	private Coroutine temproutine;

	// Token: 0x020000CB RID: 203
	public enum Effects
	{
		// Token: 0x04000D69 RID: 3433
		Attack,
		// Token: 0x04000D6A RID: 3434
		Defense,
		// Token: 0x04000D6B RID: 3435
		RandomMiniboss,
		// Token: 0x04000D6C RID: 3436
		Heal,
		// Token: 0x04000D6D RID: 3437
		HealIfWin,
		// Token: 0x04000D6E RID: 3438
		Summon,
		// Token: 0x04000D6F RID: 3439
		AttackOnCoin,
		// Token: 0x04000D70 RID: 3440
		DefenseOnCoin,
		// Token: 0x04000D71 RID: 3441
		SummonOnCoin,
		// Token: 0x04000D72 RID: 3442
		NumbFront,
		// Token: 0x04000D73 RID: 3443
		NumbFrontCoin,
		// Token: 0x04000D74 RID: 3444
		MultiplyPerTribe,
		// Token: 0x04000D75 RID: 3445
		AttackPerTribe,
		// Token: 0x04000D76 RID: 3446
		MultiplyAttackPerID,
		// Token: 0x04000D77 RID: 3447
		NumbAll,
		// Token: 0x04000D78 RID: 3448
		NumbAllCoin,
		// Token: 0x04000D79 RID: 3449
		DamageOnWin,
		// Token: 0x04000D7A RID: 3450
		AttackPerID,
		// Token: 0x04000D7B RID: 3451
		AttackIfOtherCard,
		// Token: 0x04000D7C RID: 3452
		HealIfOtherCard,
		// Token: 0x04000D7D RID: 3453
		AttackIfOpponentID,
		// Token: 0x04000D7E RID: 3454
		AttackIfOpponentTribe,
		// Token: 0x04000D7F RID: 3455
		MultiplyIfOpponentID,
		// Token: 0x04000D80 RID: 3456
		MultiplyIfOpponentTribe,
		// Token: 0x04000D81 RID: 3457
		DefenseIfOpponentID,
		// Token: 0x04000D82 RID: 3458
		DefenseIfOpponentTribe,
		// Token: 0x04000D83 RID: 3459
		IgnoreDefense,
		// Token: 0x04000D84 RID: 3460
		HealIfAttackAmmount,
		// Token: 0x04000D85 RID: 3461
		AttackPerOpponentTribe,
		// Token: 0x04000D86 RID: 3462
		AttackPerOpponentID,
		// Token: 0x04000D87 RID: 3463
		DefensePerOpponentTribe,
		// Token: 0x04000D88 RID: 3464
		DefensePerOpponentID,
		// Token: 0x04000D89 RID: 3465
		AttackOrDefenseCoin,
		// Token: 0x04000D8A RID: 3466
		HealIfWinOnce,
		// Token: 0x04000D8B RID: 3467
		AttackPerTribeOnce,
		// Token: 0x04000D8C RID: 3468
		AttackPerIDOnce,
		// Token: 0x04000D8D RID: 3469
		AttackOnce,
		// Token: 0x04000D8E RID: 3470
		Heal1OnTribeQuanity,
		// Token: 0x04000D8F RID: 3471
		NumbIfTribeAmmount,
		// Token: 0x04000D90 RID: 3472
		SummonRandomFromTribe,
		// Token: 0x04000D91 RID: 3473
		NumbIfOtherCard,
		// Token: 0x04000D92 RID: 3474
		DefenseOnOtherCard,
		// Token: 0x04000D93 RID: 3475
		AttackNextTurn
	}

	// Token: 0x020000CC RID: 204
	public enum Type
	{
		// Token: 0x04000D95 RID: 3477
		Attacker,
		// Token: 0x04000D96 RID: 3478
		Effect,
		// Token: 0x04000D97 RID: 3479
		Miniboss,
		// Token: 0x04000D98 RID: 3480
		Boss
	}

	// Token: 0x020000CD RID: 205
	public enum Tribe
	{
		// Token: 0x04000D9A RID: 3482
		Seedling,
		// Token: 0x04000D9B RID: 3483
		Wasp,
		// Token: 0x04000D9C RID: 3484
		Fungi,
		// Token: 0x04000D9D RID: 3485
		Zombie,
		// Token: 0x04000D9E RID: 3486
		Plant,
		// Token: 0x04000D9F RID: 3487
		Bug,
		// Token: 0x04000DA0 RID: 3488
		Machine,
		// Token: 0x04000DA1 RID: 3489
		Thief,
		// Token: 0x04000DA2 RID: 3490
		Unknown,
		// Token: 0x04000DA3 RID: 3491
		Chomper,
		// Token: 0x04000DA4 RID: 3492
		Leafbug,
		// Token: 0x04000DA5 RID: 3493
		DeadLander,
		// Token: 0x04000DA6 RID: 3494
		Mothfly,
		// Token: 0x04000DA7 RID: 3495
		Spider
	}

	// Token: 0x020000CE RID: 206
	public struct CardData
	{
		// Token: 0x04000DA8 RID: 3496
		public int tp;

		// Token: 0x04000DA9 RID: 3497
		public int attack;

		// Token: 0x04000DAA RID: 3498
		public int enemyid;

		// Token: 0x04000DAB RID: 3499
		public int noid;

		// Token: 0x04000DAC RID: 3500
		public float namesizeX;

		// Token: 0x04000DAD RID: 3501
		public int[,] effects;

		// Token: 0x04000DAE RID: 3502
		public CardGame.Type type;

		// Token: 0x04000DAF RID: 3503
		public CardGame.Tribe[] tribe;

		// Token: 0x04000DB0 RID: 3504
		public string desc;
	}

	// Token: 0x020000CF RID: 207
	public struct Cards
	{
		// Token: 0x04000DB1 RID: 3505
		public Transform cardobj;

		// Token: 0x04000DB2 RID: 3506
		public int cardid;

		// Token: 0x04000DB3 RID: 3507
		public bool flipped;
	}
}
