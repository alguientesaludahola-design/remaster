using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003D RID: 61
public class MazeGame : MonoBehaviour
{
	// Token: 0x06000666 RID: 1638 RVA: 0x000477D4 File Offset: 0x000459D4
	private void Start()
	{
		this.sprites = Resources.LoadAll<Sprite>("Sprites/Misc/dungeongame");
		this.holder = base.transform;
		MainManager.battleresult = false;
		base.StartCoroutine(this.Generate(this.mapsize[0], this.mapsize[1], true));
		RenderSettings.fogEndDistance = 5f;
		RenderSettings.fogColor = Color.black;
	}

	// Token: 0x06000667 RID: 1639 RVA: 0x00047835 File Offset: 0x00045A35
	private IEnumerator Generate(int x, int y, bool rebuild)
	{
		if (this.floor == 0)
		{
			MainManager.FadeMusic(0.05f);
			MainManager.SetCameraInstant(new Vector3(0f, 70f));
			this.antialias = MainManager.MainCamera.GetComponent<FXAA>().enabled;
			MainManager.MainCamera.GetComponent<FXAA>().enabled = true;
			MainManager.SetRenderTexture(2);
			SpriteRenderer t = MainManager.NewSpriteObject(new Vector3(0f, 70f), null, Resources.Load<Sprite>("Sprites/Misc/Game1"));
			t.material = MainManager.spritedefaultunity;
			Transform tt = new GameObject().transform;
			tt.transform.position = t.transform.position;
			base.StartCoroutine(MainManager.SetText(FlappyBee.args + MainManager.menutext[210], 1, null, false, true, new Vector3(0f, 4.8f), Vector3.one, Vector3.one * 2f, t.transform, null));
			t.sortingOrder = -100;
			base.StartCoroutine(MainManager.SetText(FlappyBee.args + MainManager.menutext[209], 1, null, false, true, new Vector3(0f, 22f), Vector3.one, Vector3.one * 1.5f, tt, null));
			while (MainManager.musiccoroutine != null)
			{
				yield return null;
			}
			MainManager.PlaySound("MiteKnight");
			MainManager.FadeOut();
			yield return EventControl.sec;
			while (!MainManager.GetKey(4))
			{
				tt.gameObject.transform.position = ((Mathf.Sin(Time.time * 5f) > 0f) ? new Vector3(0f, 46f) : new Vector3(0f, -999f));
				yield return null;
			}
			MainManager.PlaySound("MKKey");
			Object.Destroy(tt.gameObject);
			yield return null;
			MainManager.FadeIn();
			yield return EventControl.sec;
			Object.Destroy(t.gameObject);
			MainManager.ChangeMusic("MiteKnight");
			MainManager.CheckSamira("MiteKnight");
			yield return null;
			t = null;
			tt = null;
		}
		if (rebuild)
		{
			for (int i = 0; i < this.holder.childCount; i++)
			{
				Object.Destroy(this.holder.GetChild(i).gameObject);
			}
		}
		this.enemies = new MazeGame.Entities[0];
		int num = 8;
		this.roomdata = new List<int[]>();
		this.roomdata.Add(new int[]
		{
			1,
			1,
			2,
			2
		});
		this.roomdata.Add(new int[]
		{
			x - 6,
			y - 5,
			x - 2,
			y - 2
		});
		this.floormap = new MazeGame.Tiles[x, y];
		for (int j = 2; j < num; j++)
		{
			int[] array = new int[4];
			array[0] = Random.Range(1, x - (2 + this.maxroom[0]));
			array[1] = Random.Range(1, y - (2 + this.maxroom[1]));
			array[2] = Random.Range(array[0] + 1, array[0] + 2 + this.maxroom[0]);
			array[3] = Random.Range(array[1] + 1, array[1] + 2 + this.maxroom[1]);
			int[][] array2 = this.roomdata.ToArray();
			bool flag = false;
			for (int k = 0; k < 20; k++)
			{
				for (int l = 0; l < this.roomdata.Count; l++)
				{
					if (!MainManager.IsWithin(array[0], array2[l][0], array2[l][2]) && !MainManager.IsWithin(array[2], array2[l][0], array2[l][2]) && !MainManager.IsWithin(array[1], array2[l][1], array2[l][3]) && !MainManager.IsWithin(array[3], array2[l][1], array2[l][3]))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				this.roomdata.Add(array);
			}
		}
		num = this.roomdata.Count;
		for (int m = 0; m < num; m++)
		{
			int[] array3 = this.roomdata.ToArray()[m];
			for (int n = array3[0]; n <= array3[2]; n++)
			{
				for (int num2 = array3[1]; num2 <= array3[3]; num2++)
				{
					this.floormap[n, num2] = MazeGame.Tiles.Free;
				}
			}
		}
		this.SetCorridors(x, y);
		this.SetExtras(x, y);
		this.HideUnseen(x, y);
		this.CreateEntities(x, y);
		this.compasstarget = new Vector3[2];
		List<SpriteRenderer> list = new List<SpriteRenderer>();
		for (int num3 = y - 1; num3 >= 0; num3--)
		{
			for (int num4 = 0; num4 < x; num4++)
			{
				if (this.floormap[num4, num3] != MazeGame.Tiles.None)
				{
					string str;
					if (this.floormap[num4, num3] == MazeGame.Tiles.Key || this.floormap[num4, num3] == MazeGame.Tiles.Potion || this.floormap[num4, num3] == MazeGame.Tiles.Free)
					{
						str = "Floor";
						if (this.floormap[num4, num3] == MazeGame.Tiles.Key || this.floormap[num4, num3] == MazeGame.Tiles.Potion)
						{
							SpriteRenderer spriteRenderer = new GameObject(this.floormap[num4, num3].ToString()).AddComponent<SpriteRenderer>();
							spriteRenderer.sprite = this.sprites[(this.floormap[num4, num3] == MazeGame.Tiles.Key) ? 0 : 1];
							spriteRenderer.transform.parent = this.holder.transform;
							spriteRenderer.transform.localPosition = new Vector3((float)num4, 0.5f, (float)num3);
							spriteRenderer.gameObject.AddComponent<FaceCamera>().billboard = true;
							spriteRenderer.color = ((this.floormap[num4, num3] == MazeGame.Tiles.Key) ? Color.yellow : Color.green);
							list.Add(spriteRenderer);
						}
					}
					else
					{
						str = this.floormap[num4, num3].ToString();
					}
					Transform transform = (Object.Instantiate(Resources.Load("Prefabs/DungeonGame/" + str)) as GameObject).transform;
					transform.parent = this.holder.transform;
					transform.transform.localPosition = new Vector3((float)num4, 0f, (float)num3);
					if (this.floormap[num4, num3] == MazeGame.Tiles.Door)
					{
						transform.GetComponent<MeshRenderer>().material.color = Color.yellow;
						this.door = transform;
						transform = (Object.Instantiate(Resources.Load("Prefabs/DungeonGame/Floor")) as GameObject).transform;
						transform.parent = this.holder.transform;
						transform.transform.localPosition = new Vector3((float)num4, 0f, (float)num3);
						transform.GetComponent<MeshRenderer>().material.color = Color.yellow;
						this.InsertNewEnemy(this.NewEnemy(MazeGame.EnemyType.Wizard, new Vector2Int?(new Vector2Int(num4 + 1, num3 + 2))));
						if (this.floor == 2)
						{
							this.InsertNewEnemy(this.NewEnemy(MazeGame.EnemyType.Wizard, new Vector2Int?(new Vector2Int(num4 + 1, num3 + 1))));
						}
					}
					else
					{
						switch (this.floormap[num4, num3])
						{
						case MazeGame.Tiles.Wall:
						case MazeGame.Tiles.Free:
						case MazeGame.Tiles.Potion:
							break;
						case MazeGame.Tiles.Key:
							this.compasstarget[0] = transform.transform.position;
							break;
						case MazeGame.Tiles.Door:
							goto IL_927;
						case MazeGame.Tiles.Stairs:
							transform.GetComponent<MeshRenderer>().material.color = Color.red;
							this.compasstarget[1] = transform.transform.position;
							goto IL_927;
						default:
							goto IL_927;
						}
						transform.GetComponent<MeshRenderer>().material.color = new Color(0.9f, 0.4f, 0f);
					}
				}
				IL_927:;
			}
		}
		this.items = list.ToArray();
		if (this.floor == 0)
		{
			this.CreateBar();
			this.textholder = MainManager.NewUIObject("dghudstuff", MainManager.GUICamera.transform, Vector3.zero).transform;
			this.stext = DynamicFont.SetUp(true, 20f, 2, 0, Vector2.one * 1.25f, this.textholder, new Vector3(-10f, -5.5f, 10f));
			this.stext.tridimentional = true;
			this.stext.triui = true;
			this.floortext = DynamicFont.SetUp(true, 20f, 2, 0, Vector2.one * 1.25f, this.textholder, new Vector3(6.5f, -5.5f, 10f));
			this.floortext.tridimentional = true;
			this.floortext.triui = true;
			this.timetext = DynamicFont.SetUp(true, 20f, 2, 0, Vector2.one * 1.5f, this.textholder, new Vector3(-1f, 4.35f, 10f));
			this.timetext.tridimentional = true;
			this.timetext.triui = true;
			yield return null;
			MainManager.FadeOut();
			yield return EventControl.sec;
			base.InvokeRepeating("Clock", 0f, 1f);
		}
		this.caninput = true;
		if (this.floortext != null)
		{
			this.floortext.text = MainManager.menutext[216].Replace("@VAR@", (this.floor + 1).ToString());
		}
		yield break;
	}

	// Token: 0x06000668 RID: 1640 RVA: 0x00047859 File Offset: 0x00045A59
	private MainManager.Directions AngleToDir(float angle)
	{
		if (angle > 45f && angle < 135f)
		{
			return MainManager.Directions.Right;
		}
		if (angle >= 135f && angle < 225f)
		{
			return MainManager.Directions.Down;
		}
		if (angle >= 225f && angle < 315f)
		{
			return MainManager.Directions.Left;
		}
		return MainManager.Directions.Up;
	}

	// Token: 0x06000669 RID: 1641 RVA: 0x00047892 File Offset: 0x00045A92
	private bool PlayerInFront(int x, int y)
	{
		return this.player.x == x && this.player.y == y;
	}

	// Token: 0x0600066A RID: 1642 RVA: 0x000478B4 File Offset: 0x00045AB4
	private void Clock()
	{
		if (!this.pause && this.caninput)
		{
			this.timesec--;
			if (this.timesec < 0 && this.timemin > 0)
			{
				this.timemin--;
				this.timesec = 59;
			}
			if (this.timemin <= 0 && this.timesec < 0 && this.caninput)
			{
				this.caninput = false;
				base.CancelInvoke();
				this.player.hp = 0;
				this.DoDamage(ref this.player);
			}
			if (this.timemin <= 0 && this.timesec <= 0)
			{
				this.timemin = 0;
				this.timesec = 0;
			}
			this.timetext.text = this.timemin + ":" + this.timesec.ToString().PadLeft(2, '0');
		}
	}

	// Token: 0x0600066B RID: 1643 RVA: 0x000479A0 File Offset: 0x00045BA0
	private void EnemyAI()
	{
		for (int i = 0; i < this.enemies.Length; i++)
		{
			if (this.enemies[i].iframes > 0f)
			{
				MazeGame.Entities[] array = this.enemies;
				int num = i;
				array[num].iframes = array[num].iframes - MainManager.framestep;
			}
			if (this.enemies[i].cooldown <= 0f && this.enemies[i].iframes <= 0f)
			{
				if (!this.enemies[i].dead)
				{
					switch (this.enemies[i].animid)
					{
					case 0:
						if (Vector3.Distance(this.enemies[i].obj.transform.localPosition, this.player.obj.transform.localPosition) < 3.5f)
						{
							this.enemies[i].obj.transform.LookAt(this.player.obj.transform.position);
							this.enemies[i].direction = this.AngleToDir(MainManager.CardinalSnap(this.enemies[i].obj.transform.eulerAngles.y));
							this.enemies[i].cooldown = (float)Random.Range(25, 35);
						}
						else
						{
							this.enemies[i].direction = (MainManager.Directions)Random.Range(0, 4);
							this.enemies[i].cooldown = (float)Random.Range(25, 70);
						}
						if (this.IsFrontFree(this.enemies[i].x, this.enemies[i].y, this.enemies[i].direction))
						{
							base.StartCoroutine(this.MoveForward(this.enemies[i]));
						}
						else
						{
							int[] frontPos = this.GetFrontPos(this.enemies[i]);
							if (this.PlayerInFront(frontPos[0], frontPos[1]) && this.player.iframes <= 0f && !this.player.blocking)
							{
								this.DoDamage(ref this.player);
								MazeGame.Entities[] array2 = this.enemies;
								int num2 = i;
								array2[num2].cooldown = array2[num2].cooldown + (float)Random.Range(25, 35);
							}
						}
						break;
					case 1:
						if (!this.enemies[i].special)
						{
							this.enemies[i].obj.transform.LookAt(this.player.obj.transform.position);
							this.enemies[i].direction = this.AngleToDir(MainManager.CardinalSnap(this.enemies[i].obj.transform.eulerAngles.y));
							int[] frontPos = this.GetFrontPos(this.enemies[i]);
							if (this.PlayerInFront(frontPos[0], frontPos[1]))
							{
								if (this.player.iframes <= 0f && !this.player.blocking)
								{
									this.DoDamage(ref this.player);
								}
								base.StartCoroutine(this.TempBounce(this.enemies[i]));
								this.enemies[i].cooldown = 30f;
								this.enemies[i].special = true;
							}
							else
							{
								if (this.enemies[i].child == -1)
								{
									MazeGame.Entities data = this.NewEnemy(MazeGame.EnemyType.Fireball, new Vector2Int?(new Vector2Int(-2, -2)));
									data.dead = true;
									this.InsertNewEnemy(data);
									this.enemies[i].child = this.enemies.Length - 1;
								}
								if (this.enemies[this.enemies[i].child].dead && this.player.iframes <= 0f && Vector3.Distance(this.enemies[i].obj.transform.localPosition, this.player.obj.transform.localPosition) < 5.5f)
								{
									base.StartCoroutine(this.TempBounce(this.enemies[i]));
									this.enemies[i].special = true;
									this.enemies[i].cooldown = 50f;
									MainManager.PlaySound("Shot2");
									this.enemies[this.enemies[i].child].direction = this.enemies[i].direction;
									this.enemies[this.enemies[i].child].obj.transform.position = this.enemies[i].obj.transform.position + Vector3.up * 0.5f;
									this.enemies[this.enemies[i].child].dead = false;
									this.enemies[this.enemies[i].child].cooldown = 0f;
									this.enemies[this.enemies[i].child].hp = 1;
									this.enemies[this.enemies[i].child].render.enabled = true;
									this.enemies[this.enemies[i].child].x = this.enemies[i].x;
									this.enemies[this.enemies[i].child].y = this.enemies[i].y;
								}
							}
						}
						else
						{
							this.enemies[i].special = false;
							this.enemies[i].cooldown = (float)Random.Range(25, 50);
						}
						break;
					case 2:
						if (this.IsFrontFree(this.enemies[i].x, this.enemies[i].y, this.enemies[i].direction))
						{
							base.StartCoroutine(this.MoveForward(this.enemies[i]));
						}
						else
						{
							int[] frontPos = this.GetFrontPos(this.enemies[i]);
							if (this.PlayerInFront(frontPos[0], frontPos[1]) && this.player.iframes <= 0f && !this.player.blocking)
							{
								this.DoDamage(ref this.player);
							}
							this.enemies[i].hp = 0;
							this.DoDamage(ref this.enemies[i]);
						}
						this.enemies[i].cooldown = 10f;
						break;
					}
					this.enemies[i].lastdir = this.enemies[i].direction;
				}
			}
			else
			{
				MazeGame.Entities[] array3 = this.enemies;
				int num3 = i;
				array3[num3].cooldown = array3[num3].cooldown - MainManager.framestep;
			}
			if (!this.enemies[i].dead && this.enemies[i].render != null)
			{
				MainManager.Directions directions = this.DirectionToPlayer(this.enemies[i].direction);
				if (directions > MainManager.Directions.Left)
				{
					directions = MainManager.Directions.Left;
					this.enemies[i].render.flipX = false;
				}
				else
				{
					this.enemies[i].render.flipX = true;
				}
				switch (this.enemies[i].animid)
				{
				case 0:
					this.enemies[i].render.sprite = this.sprites[this.esprites[0][(int)directions]];
					break;
				case 1:
					this.enemies[i].render.sprite = this.sprites[this.esprites[1][(int)(directions + (this.enemies[i].special ? 3 : 0))]];
					break;
				case 2:
					this.enemies[i].render.transform.Rotate(0f, 0f, MainManager.framestep * 10f);
					break;
				}
			}
		}
	}

	// Token: 0x0600066C RID: 1644 RVA: 0x00048284 File Offset: 0x00046484
	private void InsertNewEnemy(MazeGame.Entities data)
	{
		this.enemies = new List<MazeGame.Entities>(this.enemies)
		{
			data
		}.ToArray();
	}

	// Token: 0x0600066D RID: 1645 RVA: 0x000482B0 File Offset: 0x000464B0
	private void RemoveEnemy(MazeGame.Entities data)
	{
		List<MazeGame.Entities> list = new List<MazeGame.Entities>(this.enemies);
		list.Remove(data);
		this.enemies = list.ToArray();
	}

	// Token: 0x0600066E RID: 1646 RVA: 0x000482E0 File Offset: 0x000464E0
	private void CreateBar()
	{
		this.healthbar = new SpriteRenderer[this.player.maxhp];
		float num = 0f;
		for (int i = 0; i < this.healthbar.Length; i++)
		{
			this.healthbar[i] = MainManager.NewUIObject("hp", MainManager.GUICamera.transform, new Vector3(-9f + num, 4.8f, 10f), Vector3.one * 3f, this.sprites[4]).GetComponent<SpriteRenderer>();
			this.healthbar[i].sortingOrder = -i;
			this.healthbar[i].gameObject.layer = 15;
			num += 1.25f;
		}
	}

	// Token: 0x0600066F RID: 1647 RVA: 0x00048398 File Offset: 0x00046598
	private void CreateEntities(int x, int y)
	{
		this.player = default(MazeGame.Entities);
		this.player.obj = new GameObject("player").AddComponent<Animator>();
		this.player.obj.transform.parent = this.holder;
		this.player.obj.transform.localPosition = new Vector3(1f, 0f, 1f);
		this.player.bounce = this.player.obj.gameObject.AddComponent<SpriteBounce>();
		this.player.bounce.MessageBounce(0.2f);
		this.player.x = 1;
		this.player.y = 1;
		this.player.hp = 6;
		this.player.maxhp = this.player.hp;
		SpriteRenderer spriteRenderer = this.player.obj.gameObject.AddComponent<SpriteRenderer>();
		this.player.render = spriteRenderer;
		spriteRenderer.sprite = this.sprites[2];
		spriteRenderer.color = this.playercolor;
		spriteRenderer.flipX = true;
		spriteRenderer.gameObject.layer = 15;
		this.player.bounce.basescale = Vector3.one * 0.75f;
		spriteRenderer.gameObject.AddComponent<FaceCamera>().billboard = true;
		MainManager.instance.camtarget = this.player.obj.transform;
		MainManager.instance.camoffset = new Vector3(0f, 0.5f, -0.5f);
		MainManager.instance.camangleoffset = Vector3.zero;
		List<MazeGame.Entities> list = new List<MazeGame.Entities>();
		this.engenerated = new List<Vector2>();
		for (int i = 0; i < this.roomdata.Count + this.floor; i++)
		{
			list.Add(this.NewEnemy());
		}
		this.enemies = list.ToArray();
		this.compass = MainManager.NewSpriteObject(Vector3.zero, this.holder, this.sprites[38]);
		this.compass.material = MainManager.spritedefaultunity;
		this.compass.color = Color.yellow;
		this.compass.gameObject.layer = 15;
		this.compass.transform.localEulerAngles = new Vector3(90f, 0f);
		this.compass.enabled = false;
		this.compass.transform.localScale = Vector3.one * 0.55f;
		this.compass.transform.localPosition = new Vector3(0f, 2f);
	}

	// Token: 0x06000670 RID: 1648 RVA: 0x00048648 File Offset: 0x00046848
	private MazeGame.Entities NewEnemy()
	{
		return this.NewEnemy(MazeGame.EnemyType.Random, null);
	}

	// Token: 0x06000671 RID: 1649 RVA: 0x00048668 File Offset: 0x00046868
	private MazeGame.Entities NewEnemy(MazeGame.EnemyType type, Vector2Int? pos)
	{
		MazeGame.Entities entities = default(MazeGame.Entities);
		entities.hp = 2;
		entities.maxhp = entities.hp;
		entities.animid = ((type == MazeGame.EnemyType.Random) ? this.GetRandomEnemy() : ((int)type));
		entities.obj = new GameObject(string.Concat((MazeGame.EnemyType)entities.animid)).AddComponent<Animator>();
		entities.obj.transform.parent = this.holder;
		if (pos != null)
		{
			entities.obj.transform.localPosition = new Vector3((float)pos.Value.x, 0f, (float)pos.Value.y);
		}
		else
		{
			int[] randomPointInRoom;
			do
			{
				randomPointInRoom = this.GetRandomPointInRoom(Random.Range(2, this.roomdata.Count));
				entities.obj.transform.localPosition = new Vector3((float)randomPointInRoom[0], 0f, (float)randomPointInRoom[1]);
			}
			while (this.floormap[randomPointInRoom[0], randomPointInRoom[1]] != MazeGame.Tiles.Free || this.GetEntityInPos(randomPointInRoom[0], randomPointInRoom[1]) != null || MainManager.GetDistance(entities.obj.transform.localPosition, Vector3.zero) < 5f || this.engenerated.Contains(new Vector2((float)randomPointInRoom[0], (float)randomPointInRoom[1])));
			this.engenerated.Add(new Vector2((float)randomPointInRoom[0], (float)randomPointInRoom[1]));
		}
		entities.x = (int)entities.obj.transform.localPosition.x;
		entities.y = (int)entities.obj.transform.localPosition.z;
		SpriteRenderer spriteRenderer = entities.obj.gameObject.AddComponent<SpriteRenderer>();
		entities.render = spriteRenderer;
		switch (entities.animid)
		{
		case 0:
			spriteRenderer.sprite = this.sprites[3];
			break;
		case 1:
			spriteRenderer.sprite = this.sprites[32];
			entities.child = -1;
			break;
		case 2:
			spriteRenderer.sprite = this.sprites[31];
			entities.obj.transform.localPosition += Vector3.up * 0.5f;
			entities.hp = 1;
			break;
		}
		entities.lastdir = MainManager.Directions.Cancel;
		entities.bounce = spriteRenderer.gameObject.AddComponent<SpriteBounce>();
		entities.bounce.MessageBounce(0.3f);
		spriteRenderer.color = this.enemycolor[entities.animid];
		spriteRenderer.flipX = true;
		spriteRenderer.gameObject.AddComponent<FaceCamera>().billboard = true;
		return entities;
	}

	// Token: 0x06000672 RID: 1650 RVA: 0x0004891C File Offset: 0x00046B1C
	private int GetRandomEnemy()
	{
		if (this.floor > 0)
		{
			int[] array = new int[]
			{
				0,
				0,
				0,
				1,
				1
			};
			return array[Random.Range(0, array.Length)];
		}
		return 0;
	}

	// Token: 0x06000673 RID: 1651 RVA: 0x00048950 File Offset: 0x00046B50
	private void HideUnseen(int x, int y)
	{
		List<int[]> list = new List<int[]>();
		for (int i = 1; i < x - 1; i++)
		{
			for (int j = 1; j < y - 1; j++)
			{
				if (this.floormap[i + 1, j] == MazeGame.Tiles.Wall && this.floormap[i, j + 1] == MazeGame.Tiles.Wall && this.floormap[i - 1, j] == MazeGame.Tiles.Wall && this.floormap[i, j - 1] == MazeGame.Tiles.Wall)
				{
					list.Add(new int[]
					{
						i,
						j
					});
				}
			}
		}
		int[][] array = list.ToArray();
		for (int k = 0; k < list.Count; k++)
		{
			this.floormap[array[k][0], array[k][1]] = MazeGame.Tiles.None;
		}
	}

	// Token: 0x06000674 RID: 1652 RVA: 0x00048A0C File Offset: 0x00046C0C
	private void SetExtras(int x, int y)
	{
		int[] array = null;
		for (int i = 2; i < 2 + this.roomdata.Count / Mathf.Clamp(this.floor, 2, 3); i++)
		{
			array = this.GetRandomPointInRoom(i);
			if (this.TileIsInRoomID(array[0], array[1]) > 0)
			{
				this.floormap[array[0], array[1]] = MazeGame.Tiles.Potion;
			}
		}
		bool flag = false;
		for (int j = 0; j < 20; j++)
		{
			array = this.GetRandomPointInRoom(Random.Range(2, this.roomdata.Count));
			if (this.TileIsInRoomID(array[0], array[1]) > 1)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			array = new int[]
			{
				x / 2,
				y / 2
			};
			this.floormap[array[0] + 1, array[1]] = MazeGame.Tiles.Free;
			this.floormap[array[0] + 1, array[1] + 1] = MazeGame.Tiles.Free;
			this.floormap[array[0] + 1, array[1] - 1] = MazeGame.Tiles.Free;
			this.floormap[array[0], array[1] + 1] = MazeGame.Tiles.Free;
			this.floormap[array[0], array[1] - 1] = MazeGame.Tiles.Free;
			this.floormap[array[0] - 1, array[1] + 1] = MazeGame.Tiles.Free;
			this.floormap[array[0] - 1, array[1]] = MazeGame.Tiles.Free;
			this.floormap[array[0] - 1, array[1] - 1] = MazeGame.Tiles.Free;
			for (int k = 1; k < x - 1; k++)
			{
				this.floormap[k, array[1]] = MazeGame.Tiles.Free;
			}
			for (int l = 1; l < y - 1; l++)
			{
				this.floormap[array[0], l] = MazeGame.Tiles.Free;
			}
		}
		this.floormap[x - 5, y - 3] = MazeGame.Tiles.Wall2;
		this.floormap[x - 5, y - 2] = MazeGame.Tiles.Wall2;
		this.floormap[x - 5, y - 4] = MazeGame.Tiles.Wall2;
		this.floormap[x - 4, y - 4] = MazeGame.Tiles.Door;
		this.floormap[x - 3, y - 4] = MazeGame.Tiles.Wall2;
		this.floormap[x - 2, y - 4] = MazeGame.Tiles.Wall2;
		this.floormap[x - 1, y - 2] = MazeGame.Tiles.Stairs;
		this.floormap[array[0], array[1]] = MazeGame.Tiles.Key;
	}

	// Token: 0x06000675 RID: 1653 RVA: 0x00048C3C File Offset: 0x00046E3C
	private int TileIsInRoomID(int x, int y)
	{
		int[][] array = this.roomdata.ToArray();
		for (int i = 0; i < this.roomdata.Count; i++)
		{
			if (MainManager.IsWithin(x, array[i][0], array[i][2]) && MainManager.IsWithin(y, array[i][1], array[i][3]))
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000676 RID: 1654 RVA: 0x00048C94 File Offset: 0x00046E94
	private int[] GetRandomPointInRoom(int roomid)
	{
		int[] array = this.roomdata.ToArray()[roomid];
		return new int[]
		{
			Random.Range(array[0], array[2] + 1),
			Random.Range(array[1], array[3] + 1)
		};
	}

	// Token: 0x06000677 RID: 1655 RVA: 0x00048CD8 File Offset: 0x00046ED8
	private void Update()
	{
		if (this.items != null)
		{
			for (int i = 0; i < this.items.Length; i++)
			{
				this.items[i].transform.localPosition = new Vector3(this.items[i].transform.localPosition.x, Mathf.Sin(Time.time) * 0.1f + 0.5f, this.items[i].transform.localPosition.z);
			}
		}
		if (this.healthbar != null)
		{
			for (int j = 0; j < this.healthbar.Length; j++)
			{
				if (this.healthbar[j] != null)
				{
					bool flag = this.player.hp > j;
					this.healthbar[j].sprite = this.sprites[4 + (flag ? 0 : 1)];
					this.healthbar[j].color = (flag ? Color.red : Color.gray);
				}
			}
		}
		if (this.stext != null)
		{
			this.stext.text = MainManager.menutext[189] + " " + this.score.ToString().PadLeft(5, '0');
		}
		if (this.caninput)
		{
			if (!this.pause)
			{
				if (this.enemies != null)
				{
					this.EnemyAI();
				}
				if (this.player.render != null && MainManager.GetSqrDistance(this.player.render.transform.position, MainManager.MainCamera.transform.position) > 10f)
				{
					MainManager.MainCamera.transform.position = this.player.render.transform.position;
				}
				this.GetInput();
				this.score = Mathf.Clamp(this.score, 0, 99999);
				return;
			}
			if (MainManager.GetKey(4))
			{
				this.pause = false;
				MainManager.DestroyText(this.textholder);
				return;
			}
			if (MainManager.GetKey(5))
			{
				base.StartCoroutine(this.EndGame());
			}
		}
	}

	// Token: 0x06000678 RID: 1656 RVA: 0x00048EE8 File Offset: 0x000470E8
	private IEnumerator MoveForward(MazeGame.Entities entity)
	{
		base.StartCoroutine(this.MoveForward(entity, entity.direction));
		yield return null;
		yield break;
	}

	// Token: 0x06000679 RID: 1657 RVA: 0x00048EFE File Offset: 0x000470FE
	private IEnumerator TempBounce(MazeGame.Entities entity)
	{
		entity.bounce.MessageBounce(1.5f);
		yield return EventControl.halfsec;
		entity.bounce.MessageBounce(0.2f);
		yield break;
	}

	// Token: 0x0600067A RID: 1658 RVA: 0x00048F0D File Offset: 0x0004710D
	private IEnumerator MoveForward(MazeGame.Entities entity, MainManager.Directions dir)
	{
		float a = 0f;
		float b = 15f;
		int[] p = this.GetFrontPos(entity.x, entity.y, dir);
		this.ApplyPosition(p[0], p[1], entity.obj.transform);
		Vector3 sp = entity.obj.transform.localPosition;
		Vector3 tp = new Vector3((float)p[0], entity.obj.transform.localPosition.y, (float)p[1]);
		entity.bounce.MessageBounce(1.5f);
		if (entity.obj.transform == this.player.obj.transform && !this.player.blocking)
		{
			entity.render.flipX = !entity.render.flipX;
			this.DoAction(MazeGame.Action.Moving, b);
		}
		do
		{
			entity.obj.transform.localPosition = Vector3.Lerp(sp, tp, a / b);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < b + 1f);
		entity.obj.transform.localPosition = tp;
		if (entity.obj.transform == this.player.obj.transform && this.player.hp > 0)
		{
			MazeGame.Tiles tiles = this.floormap[p[0], p[1]];
			if (tiles != MazeGame.Tiles.Key)
			{
				if (tiles == MazeGame.Tiles.Potion)
				{
					if (this.player.hp < this.player.maxhp)
					{
						MainManager.PlaySound("MKPotion");
						this.player.hp = this.player.hp + 1;
						this.DestroyItem(p[0], p[1]);
						this.floormap[p[0], p[1]] = MazeGame.Tiles.Free;
					}
				}
			}
			else
			{
				MainManager.PlaySound("MKKey");
				this.haskey = true;
				this.gotkey = true;
				this.floormap[p[0], p[1]] = MazeGame.Tiles.Free;
				this.DestroyItem(p[0], p[1]);
				this.keyicon = MainManager.NewUIObject("key", MainManager.GUICamera.transform, new Vector3(9f, 4.5f, 10f), Vector3.one * 3f, this.sprites[0]).transform;
				this.keyicon.GetComponent<SpriteRenderer>().color = Color.yellow;
				this.keyicon.gameObject.layer = 15;
				this.score += 500;
			}
		}
		yield return null;
		if (entity.hp <= 0)
		{
			MainManager.DeathSmoke(entity.obj.transform.position, Vector3.one * 0.5f);
			entity.obj.transform.localPosition = new Vector3(0f, 2f);
		}
		entity.bounce.MessageBounce(0.2f);
		if (entity.obj.transform == this.player.obj.transform)
		{
			this.moving = null;
		}
		yield break;
	}

	// Token: 0x0600067B RID: 1659 RVA: 0x00048F2C File Offset: 0x0004712C
	private void DestroyItem(int x, int y)
	{
		List<SpriteRenderer> list = new List<SpriteRenderer>();
		for (int i = 0; i < this.items.Length; i++)
		{
			if (Mathf.RoundToInt(this.items[i].transform.localPosition.x) == x && Mathf.RoundToInt(this.items[i].transform.localPosition.z) == y)
			{
				Object.Destroy(this.items[i].gameObject);
			}
			else
			{
				list.Add(this.items[i]);
			}
		}
		this.items = list.ToArray();
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x00048FC0 File Offset: 0x000471C0
	private MainManager.Directions DirectionToPlayer(MainManager.Directions facing)
	{
		switch (this.player.direction)
		{
		case MainManager.Directions.Up:
			switch (facing)
			{
			case MainManager.Directions.Up:
				return MainManager.Directions.Down;
			case MainManager.Directions.Down:
				return MainManager.Directions.Up;
			case MainManager.Directions.Left:
				return MainManager.Directions.Left;
			case MainManager.Directions.Right:
				return MainManager.Directions.Right;
			}
			break;
		case MainManager.Directions.Down:
			switch (facing)
			{
			case MainManager.Directions.Up:
				return MainManager.Directions.Up;
			case MainManager.Directions.Down:
				return MainManager.Directions.Down;
			case MainManager.Directions.Left:
				return MainManager.Directions.Right;
			case MainManager.Directions.Right:
				return MainManager.Directions.Left;
			}
			break;
		case MainManager.Directions.Left:
			switch (facing)
			{
			case MainManager.Directions.Up:
				return MainManager.Directions.Right;
			case MainManager.Directions.Down:
				return MainManager.Directions.Left;
			case MainManager.Directions.Left:
				return MainManager.Directions.Down;
			case MainManager.Directions.Right:
				return MainManager.Directions.Up;
			}
			break;
		case MainManager.Directions.Right:
			switch (facing)
			{
			case MainManager.Directions.Up:
				return MainManager.Directions.Left;
			case MainManager.Directions.Down:
				return MainManager.Directions.Right;
			case MainManager.Directions.Left:
				return MainManager.Directions.Up;
			case MainManager.Directions.Right:
				return MainManager.Directions.Down;
			}
			break;
		}
		return facing;
	}

	// Token: 0x0600067D RID: 1661 RVA: 0x00049078 File Offset: 0x00047278
	private void ApplyPosition(int x, int y, Transform obj)
	{
		if (obj == this.player.obj.transform)
		{
			this.player.x = x;
			this.player.y = y;
			return;
		}
		for (int i = 0; i < this.enemies.Length; i++)
		{
			if (this.enemies[i].obj.transform == obj)
			{
				this.enemies[i].x = x;
				this.enemies[i].y = y;
				return;
			}
		}
	}

	// Token: 0x0600067E RID: 1662 RVA: 0x0004910C File Offset: 0x0004730C
	private int[] GetFrontPos(MazeGame.Entities entity)
	{
		return this.GetFrontPos(entity.x, entity.y, entity.direction);
	}

	// Token: 0x0600067F RID: 1663 RVA: 0x00049128 File Offset: 0x00047328
	private int[] GetFrontPos(int x, int y, MainManager.Directions dir)
	{
		switch (dir)
		{
		case MainManager.Directions.Up:
			return new int[]
			{
				x,
				y + 1
			};
		case MainManager.Directions.Down:
			return new int[]
			{
				x,
				y - 1
			};
		case MainManager.Directions.Left:
			return new int[]
			{
				x - 1,
				y
			};
		case MainManager.Directions.Right:
			return new int[]
			{
				x + 1,
				y
			};
		default:
			return new int[]
			{
				x,
				y
			};
		}
	}

	// Token: 0x06000680 RID: 1664 RVA: 0x000491A0 File Offset: 0x000473A0
	private bool IsFrontFree(int x, int y, MainManager.Directions dir)
	{
		int[] frontPos = this.GetFrontPos(x, y, dir);
		return (this.floormap[frontPos[0], frontPos[1]] == MazeGame.Tiles.Free || this.floormap[frontPos[0], frontPos[1]] == MazeGame.Tiles.Key || this.floormap[frontPos[0], frontPos[1]] == MazeGame.Tiles.Potion) && !this.IsEntityInPos(frontPos[0], frontPos[1]);
	}

	// Token: 0x06000681 RID: 1665 RVA: 0x00049204 File Offset: 0x00047404
	private bool IsEntityInPos(int x, int y)
	{
		if (this.player.x == x && this.player.y == y)
		{
			return true;
		}
		for (int i = 0; i < this.enemies.Length; i++)
		{
			if (!this.enemies[i].dead && this.enemies[i].x == x && this.enemies[i].y == y)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000682 RID: 1666 RVA: 0x00049280 File Offset: 0x00047480
	private void SwitchDirection(bool left)
	{
		this.DoAction(MazeGame.Action.Turning, 20f);
		this.player.render.flipX = left;
		switch (this.player.direction)
		{
		case MainManager.Directions.Up:
			if (left)
			{
				this.player.direction = MainManager.Directions.Left;
			}
			else
			{
				this.player.direction = MainManager.Directions.Right;
			}
			break;
		case MainManager.Directions.Down:
			if (left)
			{
				this.player.direction = MainManager.Directions.Right;
			}
			else
			{
				this.player.direction = MainManager.Directions.Left;
			}
			break;
		case MainManager.Directions.Left:
			if (left)
			{
				this.player.direction = MainManager.Directions.Down;
			}
			else
			{
				this.player.direction = MainManager.Directions.Up;
			}
			break;
		case MainManager.Directions.Right:
			if (left)
			{
				this.player.direction = MainManager.Directions.Up;
			}
			else
			{
				this.player.direction = MainManager.Directions.Down;
			}
			break;
		}
		switch (this.player.direction)
		{
		case MainManager.Directions.Up:
			MainManager.instance.camangleoffset = Vector3.zero;
			break;
		case MainManager.Directions.Down:
			MainManager.instance.camangleoffset = new Vector3(0f, 180f);
			break;
		case MainManager.Directions.Left:
			MainManager.instance.camangleoffset = new Vector3(0f, 270f);
			break;
		case MainManager.Directions.Right:
			MainManager.instance.camangleoffset = new Vector3(0f, 90f);
			break;
		}
		this.player.iframes = 15f;
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x000493DC File Offset: 0x000475DC
	private MainManager.Directions[] DirArray(MainManager.Directions dir)
	{
		switch (dir)
		{
		case MainManager.Directions.Up:
			return new MainManager.Directions[]
			{
				MainManager.Directions.Up,
				MainManager.Directions.Down,
				MainManager.Directions.Left,
				MainManager.Directions.Right
			};
		case MainManager.Directions.Down:
			return new MainManager.Directions[]
			{
				MainManager.Directions.Down,
				MainManager.Directions.Up,
				MainManager.Directions.Right,
				MainManager.Directions.Left
			};
		case MainManager.Directions.Left:
			return new MainManager.Directions[]
			{
				MainManager.Directions.Left,
				MainManager.Directions.Right,
				MainManager.Directions.Down,
				MainManager.Directions.Up
			};
		case MainManager.Directions.Right:
			return new MainManager.Directions[]
			{
				MainManager.Directions.Right,
				MainManager.Directions.Left,
				MainManager.Directions.Up,
				MainManager.Directions.Down
			};
		default:
			return null;
		}
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x0004944A File Offset: 0x0004764A
	private MazeGame.Entities? GetEntityInPos(int[] coords)
	{
		return this.GetEntityInPos(coords[0], coords[1]);
	}

	// Token: 0x06000685 RID: 1669 RVA: 0x00049458 File Offset: 0x00047658
	private MazeGame.Entities? GetEntityInPos(int x, int y)
	{
		if (this.player.x == x && this.player.y == y)
		{
			return new MazeGame.Entities?(this.player);
		}
		for (int i = 0; i < this.enemies.Length; i++)
		{
			if (this.enemies[i].x == x && this.enemies[i].y == y)
			{
				return new MazeGame.Entities?(this.enemies[i]);
			}
		}
		return null;
	}

	// Token: 0x06000686 RID: 1670 RVA: 0x000494E4 File Offset: 0x000476E4
	private int EnemyIDFromTransform(Transform transf)
	{
		for (int i = 0; i < this.enemies.Length; i++)
		{
			if (this.enemies[i].obj != null && this.enemies[i].obj.transform == transf)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000687 RID: 1671 RVA: 0x0004953E File Offset: 0x0004773E
	private MainManager.Directions InvertDirection(MainManager.Directions dir)
	{
		switch (dir)
		{
		case MainManager.Directions.Up:
			return MainManager.Directions.Down;
		case MainManager.Directions.Down:
			return MainManager.Directions.Up;
		case MainManager.Directions.Left:
			return MainManager.Directions.Right;
		case MainManager.Directions.Right:
			return MainManager.Directions.Left;
		default:
			return MainManager.Directions.Up;
		}
	}

	// Token: 0x06000688 RID: 1672 RVA: 0x00049564 File Offset: 0x00047764
	private void DoDamage(ref MazeGame.Entities entity)
	{
		bool flag = entity.obj.transform == this.player.obj.transform;
		bool flag2 = !flag && entity.animid == 2;
		entity.hp--;
		if (flag2)
		{
			MainManager.DeathSmoke(entity.obj.transform.position);
			entity.obj.transform.localPosition = new Vector3(0f, 2f);
			entity.render.enabled = false;
			entity.dead = true;
			return;
		}
		MainManager.HitPart(entity.obj.transform.position);
		if (!flag)
		{
			MainManager.PlaySound("MKHit2");
			this.score += 10;
			entity.iframes = 60f;
		}
		else
		{
			this.compass.enabled = false;
			this.stopdelay = 0f;
			entity.iframes = 80f;
			MainManager.PlaySound((entity.hp == 0) ? "MKDeath" : "MKHit");
		}
		MainManager.Directions dir = this.InvertDirection(entity.direction);
		if (this.IsFrontFree(entity.x, entity.y, dir))
		{
			base.StartCoroutine(this.MoveForward(entity, dir));
		}
		else if (entity.hp <= 0)
		{
			entity.obj.transform.localPosition = new Vector3(0f, 2f);
		}
		if (entity.hp > 0)
		{
			base.StartCoroutine(this.Blink(entity));
			return;
		}
		if (flag)
		{
			base.StartCoroutine(this.EndGame());
			return;
		}
		entity.dead = true;
		entity.x = -1;
		entity.y = -1;
		entity.cooldown = 600f;
		this.score += 100;
	}

	// Token: 0x06000689 RID: 1673 RVA: 0x00049734 File Offset: 0x00047934
	private void RefreshEnemyList()
	{
		List<MazeGame.Entities> list = new List<MazeGame.Entities>();
		for (int i = 0; i < this.enemies.Length; i++)
		{
			if (this.enemies[i].obj != null)
			{
				list.Add(this.enemies[i]);
			}
		}
		this.enemies = list.ToArray();
	}

	// Token: 0x0600068A RID: 1674 RVA: 0x00049791 File Offset: 0x00047991
	private IEnumerator Blink(MazeGame.Entities entity)
	{
		SpriteRenderer r = entity.obj.GetComponent<SpriteRenderer>();
		Color ic = r.color;
		int num;
		for (int i = 0; i < 2; i = num + 1)
		{
			r.color = Color.white;
			yield return new WaitForSeconds(0.1f);
			r.color = ic;
			yield return new WaitForSeconds(0.1f);
			num = i;
		}
		yield break;
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x000497A0 File Offset: 0x000479A0
	private IEnumerator FloorChange()
	{
		MainManager.PlaySound("MKStairs");
		this.floor++;
		this.caninput = false;
		this.gotkey = false;
		if (this.floor >= this.maxfloors)
		{
			MainManager.battleresult = true;
			base.StartCoroutine(this.EndGame());
		}
		else
		{
			int hp = this.player.hp;
			MainManager.FadeIn();
			yield return new WaitForSeconds(1f);
			base.StartCoroutine(this.Generate(this.mapsize[0] + 5 * this.floor, this.mapsize[1] + 5 * this.floor, true));
			yield return null;
			MainManager.instance.camspeed = 1f;
			yield return new WaitForSeconds(0.1f);
			MainManager.instance.camspeed = 0.1f;
			this.player.hp = hp;
			MainManager.FadeOut();
			yield return new WaitForSeconds(0.75f);
			this.canpause = true;
			this.caninput = true;
		}
		yield break;
	}

	// Token: 0x0600068C RID: 1676 RVA: 0x000497B0 File Offset: 0x000479B0
	private void GetInput()
	{
		if (this.stopdelay < 120f)
		{
			this.stopdelay += MainManager.framestep;
			this.compass.enabled = false;
		}
		else if (!this.compass.enabled)
		{
			this.compass.transform.localPosition = new Vector3(this.player.render.transform.position.x, 0.65f, this.player.render.transform.position.z);
			MainManager.LookAt(this.compass.transform, this.compasstarget[this.gotkey ? 1 : 0], true);
			this.compass.enabled = true;
		}
		this.player.blocking = MainManager.GetKey(4, true);
		if (this.player.blocking != this.oldblock)
		{
			if (this.player.blocking)
			{
				MainManager.PlaySound("MKKey", -1, 0.9f, 1f);
			}
			else
			{
				MainManager.PlaySound("MKKey");
			}
			this.oldblock = this.player.blocking;
		}
		if (this.player.actioncooldown <= 0f)
		{
			if (this.player.blocking)
			{
				this.player.render.sprite = this.sprites[6];
			}
			else
			{
				this.player.render.sprite = this.sprites[2];
			}
		}
		else
		{
			this.player.actioncooldown = this.player.actioncooldown - MainManager.framestep;
		}
		if (this.player.iframes > 0f)
		{
			this.player.iframes = this.player.iframes - MainManager.framestep;
		}
		if (this.inputdelay > 0f)
		{
			this.inputdelay -= MainManager.framestep;
			return;
		}
		if (this.moving == null && this.inputdelay <= 0f)
		{
			MainManager.Directions[] array = this.DirArray(this.player.direction);
			if (MainManager.GetKey(0, true))
			{
				if (this.IsFrontFree(this.player.x, this.player.y, this.player.direction))
				{
					MainManager.PlaySound("MKWalk");
					this.moving = base.StartCoroutine(this.MoveForward(this.player));
					this.stopdelay = 0f;
					return;
				}
				MazeGame.Entities? entityInPos = this.GetEntityInPos(this.GetFrontPos(this.player));
				if (entityInPos != null)
				{
					if (!this.player.blocking)
					{
						if (this.boss.obj != null && entityInPos.Value.obj.transform == this.boss.obj.transform)
						{
							if (!this.boss.blocking)
							{
								this.DoDamage(ref this.boss);
								this.DoAction(MazeGame.Action.Attacking, 15f);
							}
						}
						else
						{
							int num = this.EnemyIDFromTransform(entityInPos.Value.obj.transform);
							if (!this.enemies[num].blocking)
							{
								this.DoDamage(ref this.enemies[num]);
								this.DoAction(MazeGame.Action.Attacking, 15f);
							}
						}
						this.inputdelay = 15f;
					}
				}
				else if (this.haskey)
				{
					int[] frontPos = this.GetFrontPos(this.player.x, this.player.y, this.player.direction);
					if (this.floormap[frontPos[0], frontPos[1]] == MazeGame.Tiles.Door)
					{
						MainManager.PlaySound("MKOpen");
						this.score += 400;
						this.floormap[frontPos[0], frontPos[1]] = MazeGame.Tiles.Free;
						Object.Destroy(this.door.gameObject);
						Object.Destroy(this.keyicon.gameObject);
						this.haskey = false;
					}
				}
				else
				{
					int[] frontPos2 = this.GetFrontPos(this.player.x, this.player.y, this.player.direction);
					if (this.floormap[frontPos2[0], frontPos2[1]] == MazeGame.Tiles.Stairs && (this.timemin > 0 || this.timesec > 1))
					{
						this.canpause = false;
						base.StartCoroutine(this.FloorChange());
					}
				}
				this.player.iframes = 25f;
				return;
			}
			else if (MainManager.GetKey(1, true))
			{
				if (this.IsFrontFree(this.player.x, this.player.y, array[1]))
				{
					this.moving = base.StartCoroutine(this.MoveForward(this.player, array[1]));
					MainManager.PlaySound("MKWalk");
					this.stopdelay = 0f;
					return;
				}
			}
			else
			{
				if (MainManager.GetKey(8) && this.player.hp > 0 && this.canpause)
				{
					this.pause = true;
					this.stopdelay = 0f;
					this.compass.enabled = false;
					MainManager.PlaySound("PeacockSpiderNPCSummonSuccess");
					base.StartCoroutine(MainManager.SetText(FlappyBee.args + "|center||sort,10|" + MainManager.menutext[213], 1, null, false, true, new Vector3(-0.25f, 0.2f, 0.7f), Vector3.one, Vector3.one, this.textholder, null));
					this.textholder.GetChild(this.textholder.childCount - 1).localScale = Vector3.one * 0.075f;
					return;
				}
				if (this.player.blocking)
				{
					if (MainManager.GetKey(4, true))
					{
						if (MainManager.GetKey(2, true))
						{
							if (this.IsFrontFree(this.player.x, this.player.y, array[2]))
							{
								this.stopdelay = 0f;
								this.moving = base.StartCoroutine(this.MoveForward(this.player, array[2]));
								MainManager.PlaySound("MKWalk");
								return;
							}
						}
						else if (MainManager.GetKey(3, true) && this.IsFrontFree(this.player.x, this.player.y, array[3]))
						{
							this.stopdelay = 0f;
							this.moving = base.StartCoroutine(this.MoveForward(this.player, array[3]));
							MainManager.PlaySound("MKWalk");
							return;
						}
					}
				}
				else
				{
					if (MainManager.GetKey(2))
					{
						this.SwitchDirection(true);
						this.inputdelay = 10f;
						this.stopdelay = 0f;
						MainManager.PlaySound("MKWalk", -1, 0.9f, 1f);
						return;
					}
					if (MainManager.GetKey(3))
					{
						this.SwitchDirection(false);
						this.inputdelay = 10f;
						this.stopdelay = 0f;
						MainManager.PlaySound("MKWalk", -1, 0.9f, 1f);
					}
				}
			}
		}
	}

	// Token: 0x0600068D RID: 1677 RVA: 0x00049EB4 File Offset: 0x000480B4
	private void DoAction(MazeGame.Action type, float time)
	{
		this.player.action = type;
		this.player.actioncooldown = time;
		if (type == MazeGame.Action.Attacking)
		{
			this.player.render.sprite = this.sprites[8];
			return;
		}
		if (type - MazeGame.Action.Turning > 1)
		{
			return;
		}
		this.player.render.sprite = this.sprites[7];
	}

	// Token: 0x0600068E RID: 1678 RVA: 0x00049F15 File Offset: 0x00048115
	private IEnumerator EndGame()
	{
		this.caninput = false;
		this.DestroyHUD();
		base.CancelInvoke();
		MainManager.FadeMusic(0.05f);
		MainManager.FadeIn();
		yield return EventControl.sec;
		while (MainManager.musiccoroutine != null)
		{
			yield return null;
		}
		if (!this.pause)
		{
			SpriteRenderer back = MainManager.NewSolidColor("back", Color.black);
			back.gameObject.layer = 15;
			MainManager.SetCamera(new Vector3(0f, 90f), Vector3.zero, 1f);
			back.color = Color.black;
			back.transform.localScale = Vector3.one * 3000f;
			back.transform.position = new Vector3(0f, 92f);
			back.transform.parent = base.transform;
			yield return null;
			if (MainManager.battleresult)
			{
				back.sortingOrder = -99;
				back.gameObject.layer = 0;
				MainManager.FadeOut();
				MainManager.PlaySound("MiteKnight", -1, 1.05f, 1f);
				base.StartCoroutine(MainManager.SetText(FlappyBee.args + "|rainbow||triui|" + MainManager.menutext[264], 1, null, false, true, new Vector3(0f, 25.5f), Vector3.one, Vector3.one * 2f, base.transform, null));
				yield return EventControl.sec;
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					FlappyBee.args,
					"|triui|",
					MainManager.menutext[265],
					" |font,1|",
					this.timemin,
					":",
					this.timesec.ToString().PadLeft(2, '0')
				}), 1, null, false, true, new Vector3(0f, 23.85f), Vector3.one, Vector3.one * 2f, base.transform, null));
				yield return EventControl.halfsec;
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					FlappyBee.args,
					"|triui|",
					MainManager.menutext[266],
					" |font,1||size,1.35,2|[ ",
					this.player.hp,
					" / ",
					this.player.maxhp,
					" ]"
				}), 1, null, false, true, new Vector3(0f, 22.33f), Vector3.one, Vector3.one * 2f, base.transform, null));
				yield return EventControl.sec;
				this.score = Mathf.CeilToInt((float)this.score * (1f + ((float)this.timemin * 60f + (float)this.timesec) / 300f / 2f) * (1f + (float)this.player.hp / (float)this.player.maxhp / 2f));
				base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
				{
					FlappyBee.args,
					"|triui|",
					MainManager.menutext[189],
					" |font,1|",
					this.score
				}), 1, null, false, true, new Vector3(0f, 20f), Vector3.one, Vector3.one * 2f, base.transform, null));
				yield return EventControl.halfsec;
				base.StartCoroutine(MainManager.SetText(FlappyBee.args + "|triui|" + MainManager.menutext[238], 1, null, false, true, new Vector3(0f, 17.85f, -0.1f), Vector3.one, Vector3.one * 2f, base.transform, null));
				while (!MainManager.GetKey(4))
				{
					if (MainManager.GetKey(5))
					{
						break;
					}
					yield return null;
				}
			}
			else
			{
				MainManager.PlaySound("MKGameOver", -1, 0.9f, 1f);
				base.StartCoroutine(MainManager.SetText(FlappyBee.args + "|triui|" + MainManager.menutext[215], 1, null, false, true, new Vector3(0f, 21.75f), Vector3.one, Vector3.one * 2f, base.transform, null));
				yield return null;
				MainManager.FadeOut();
				yield return EventControl.sec;
				while (!MainManager.GetKey(4))
				{
					yield return null;
				}
			}
			MainManager.FadeIn();
			yield return EventControl.sec;
			back = null;
		}
		if (this.score > MainManager.instance.flagvar[28])
		{
			MainManager.instance.flagvar[28] = this.score;
		}
		if (this.keyicon != null)
		{
			Object.Destroy(this.keyicon.gameObject);
		}
		MainManager.EndMiniGame(this.antialias, this.score);
		yield return null;
		Object.Destroy(base.gameObject);
		yield break;
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x00049F24 File Offset: 0x00048124
	private void DestroyHUD()
	{
		for (int i = 0; i < this.healthbar.Length; i++)
		{
			Object.Destroy(this.healthbar[i].gameObject, 1f);
		}
		Object.Destroy(this.textholder.gameObject, 1f);
		if (this.stext != null)
		{
			Object.Destroy(this.stext.gameObject);
		}
		if (this.keyicon != null)
		{
			Object.Destroy(this.keyicon.gameObject);
		}
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x00049FAC File Offset: 0x000481AC
	private void SetCorridors(int x, int y)
	{
		int count = this.roomdata.Count;
		int num = count - 1;
		bool[] array = new bool[num];
		int[][] array2 = this.roomdata.ToArray();
		for (int i = 1; i < num; i++)
		{
			for (int j = array2[0][0]; j < x; j++)
			{
				for (int k = array2[0][1]; k <= array2[0][3]; k++)
				{
					if (this.floormap[j, k] == MazeGame.Tiles.Free && !array[i])
					{
						for (int l = 0; l < array.Length; l++)
						{
							if (this.TileIsInRoomID(j, k) == i - 1)
							{
								array[i] = true;
								break;
							}
						}
						for (int m = array2[0][0]; m < j; m++)
						{
							this.floormap[m, k] = MazeGame.Tiles.Free;
						}
						break;
					}
				}
			}
			for (int n = array2[0][1]; n < y; n++)
			{
				for (int num2 = array2[0][0]; num2 <= array2[0][2]; num2++)
				{
					if (this.floormap[num2, n] == MazeGame.Tiles.Free && !array[i])
					{
						for (int num3 = 0; num3 < array.Length; num3++)
						{
							if (this.TileIsInRoomID(num2, n) == i - 1)
							{
								array[i] = true;
								break;
							}
						}
						for (int num4 = array2[0][1]; num4 < n; num4++)
						{
							this.floormap[num2, num4] = MazeGame.Tiles.Free;
						}
						break;
					}
				}
			}
			for (int num5 = 1; num5 < count; num5++)
			{
				int[] randomPointInRoom = this.GetRandomPointInRoom(num5);
				bool flag = Random.Range(0, 2) == 0;
				int num6 = array2[0][2];
				int num7 = array2[0][1];
				if (!flag)
				{
					for (num6 = array2[0][2]; num6 < randomPointInRoom[0]; num6++)
					{
						this.floormap[num6, num7] = MazeGame.Tiles.Free;
					}
					for (num7 = array2[0][1]; num7 < randomPointInRoom[1]; num7++)
					{
						this.floormap[num6, num7] = MazeGame.Tiles.Free;
					}
				}
				else
				{
					for (num7 = array2[0][1]; num7 < randomPointInRoom[1]; num7++)
					{
						this.floormap[num6, num7] = MazeGame.Tiles.Free;
					}
					for (num6 = array2[0][2]; num6 < randomPointInRoom[0]; num6++)
					{
						this.floormap[num6, num7] = MazeGame.Tiles.Free;
					}
				}
			}
		}
	}

	// Token: 0x040005BD RID: 1469
	private int floor;

	// Token: 0x040005BE RID: 1470
	private int maxfloors = 3;

	// Token: 0x040005BF RID: 1471
	private int score;

	// Token: 0x040005C0 RID: 1472
	private int timemin = 5;

	// Token: 0x040005C1 RID: 1473
	private int timesec;

	// Token: 0x040005C2 RID: 1474
	private int[] mapsize = new int[]
	{
		25,
		25
	};

	// Token: 0x040005C3 RID: 1475
	private int[] maxroom = new int[]
	{
		3,
		3
	};

	// Token: 0x040005C4 RID: 1476
	private bool haskey;

	// Token: 0x040005C5 RID: 1477
	private bool pause;

	// Token: 0x040005C6 RID: 1478
	private bool antialias;

	// Token: 0x040005C7 RID: 1479
	private bool oldblock;

	// Token: 0x040005C8 RID: 1480
	private bool gotkey;

	// Token: 0x040005C9 RID: 1481
	private bool canpause = true;

	// Token: 0x040005CA RID: 1482
	public bool caninput;

	// Token: 0x040005CB RID: 1483
	private float inputdelay;

	// Token: 0x040005CC RID: 1484
	private float stopdelay;

	// Token: 0x040005CD RID: 1485
	private Vector3[] compasstarget;

	// Token: 0x040005CE RID: 1486
	private List<Vector2> engenerated;

	// Token: 0x040005CF RID: 1487
	private MazeGame.Tiles[,] floormap;

	// Token: 0x040005D0 RID: 1488
	private List<int[]> roomdata;

	// Token: 0x040005D1 RID: 1489
	private Sprite[] sprites;

	// Token: 0x040005D2 RID: 1490
	private SpriteRenderer compass;

	// Token: 0x040005D3 RID: 1491
	private Color basecolor = Color.green;

	// Token: 0x040005D4 RID: 1492
	private Color playercolor = new Color(0.5f, 0.5f, 1f);

	// Token: 0x040005D5 RID: 1493
	private Color[] enemycolor = new Color[]
	{
		Color.red,
		new Color(0.5f, 0f, 0.65f),
		Color.yellow
	};

	// Token: 0x040005D6 RID: 1494
	private int[][] esprites = new int[][]
	{
		new int[]
		{
			3,
			10,
			9
		},
		new int[]
		{
			32,
			34,
			33,
			35,
			37,
			36
		}
	};

	// Token: 0x040005D7 RID: 1495
	private Transform holder;

	// Token: 0x040005D8 RID: 1496
	private Transform textholder;

	// Token: 0x040005D9 RID: 1497
	private Transform door;

	// Token: 0x040005DA RID: 1498
	private Transform keyicon;

	// Token: 0x040005DB RID: 1499
	private DynamicFont stext;

	// Token: 0x040005DC RID: 1500
	private DynamicFont floortext;

	// Token: 0x040005DD RID: 1501
	private DynamicFont timetext;

	// Token: 0x040005DE RID: 1502
	private MazeGame.Entities player;

	// Token: 0x040005DF RID: 1503
	private MazeGame.Entities boss;

	// Token: 0x040005E0 RID: 1504
	private MazeGame.Entities[] enemies;

	// Token: 0x040005E1 RID: 1505
	private SpriteRenderer[] items;

	// Token: 0x040005E2 RID: 1506
	private SpriteRenderer[] healthbar;

	// Token: 0x040005E3 RID: 1507
	private Coroutine moving;

	// Token: 0x0200024F RID: 591
	private enum Tiles
	{
		// Token: 0x04001FAC RID: 8108
		Wall,
		// Token: 0x04001FAD RID: 8109
		Free,
		// Token: 0x04001FAE RID: 8110
		Key,
		// Token: 0x04001FAF RID: 8111
		Potion,
		// Token: 0x04001FB0 RID: 8112
		Door,
		// Token: 0x04001FB1 RID: 8113
		Stairs,
		// Token: 0x04001FB2 RID: 8114
		Wall2,
		// Token: 0x04001FB3 RID: 8115
		None
	}

	// Token: 0x02000250 RID: 592
	private enum AnimID
	{
		// Token: 0x04001FB5 RID: 8117
		Player,
		// Token: 0x04001FB6 RID: 8118
		Enemy1,
		// Token: 0x04001FB7 RID: 8119
		Enemy2,
		// Token: 0x04001FB8 RID: 8120
		Boss
	}

	// Token: 0x02000251 RID: 593
	private enum Action
	{
		// Token: 0x04001FBA RID: 8122
		None,
		// Token: 0x04001FBB RID: 8123
		Attacking,
		// Token: 0x04001FBC RID: 8124
		Turning,
		// Token: 0x04001FBD RID: 8125
		Moving
	}

	// Token: 0x02000252 RID: 594
	private enum EnemyType
	{
		// Token: 0x04001FBF RID: 8127
		Ant,
		// Token: 0x04001FC0 RID: 8128
		Wizard,
		// Token: 0x04001FC1 RID: 8129
		Fireball,
		// Token: 0x04001FC2 RID: 8130
		Random
	}

	// Token: 0x02000253 RID: 595
	private struct Entities
	{
		// Token: 0x04001FC3 RID: 8131
		public int hp;

		// Token: 0x04001FC4 RID: 8132
		public int maxhp;

		// Token: 0x04001FC5 RID: 8133
		public int score;

		// Token: 0x04001FC6 RID: 8134
		public int x;

		// Token: 0x04001FC7 RID: 8135
		public int y;

		// Token: 0x04001FC8 RID: 8136
		public int animid;

		// Token: 0x04001FC9 RID: 8137
		public int animstate;

		// Token: 0x04001FCA RID: 8138
		public int child;

		// Token: 0x04001FCB RID: 8139
		public Animator obj;

		// Token: 0x04001FCC RID: 8140
		public SpriteBounce bounce;

		// Token: 0x04001FCD RID: 8141
		public SpriteRenderer render;

		// Token: 0x04001FCE RID: 8142
		public float cooldown;

		// Token: 0x04001FCF RID: 8143
		public float iframes;

		// Token: 0x04001FD0 RID: 8144
		public float actioncooldown;

		// Token: 0x04001FD1 RID: 8145
		public bool blocking;

		// Token: 0x04001FD2 RID: 8146
		public bool dead;

		// Token: 0x04001FD3 RID: 8147
		public bool special;

		// Token: 0x04001FD4 RID: 8148
		public MainManager.Directions direction;

		// Token: 0x04001FD5 RID: 8149
		public MainManager.Directions lastdir;

		// Token: 0x04001FD6 RID: 8150
		public MazeGame.Action action;
	}
}
