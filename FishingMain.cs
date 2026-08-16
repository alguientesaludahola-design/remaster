using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000022 RID: 34
public class FishingMain : MonoBehaviour
{
	// Token: 0x06000394 RID: 916 RVA: 0x00024934 File Offset: 0x00022B34
	public static float[][] GetFishRecords()
	{
		float[][] array = new float[10][];
		string[] array2 = MainManager.instance.flagstring[14].Split(new char[]
		{
			'@'
		});
		for (int i = 0; i < FishingMain.idKeys.Count; i++)
		{
			string[] array3 = array2[i].Split(new char[]
			{
				':'
			});
			array[i] = new float[]
			{
				(float)Convert.ToInt32(array3[0]),
				Convert.ToSingle(array3[1])
			};
		}
		return array;
	}

	// Token: 0x06000395 RID: 917 RVA: 0x000249B4 File Offset: 0x00022BB4
	public static string[] LoadText()
	{
		return Resources.Load<TextAsset>("Data/Dialogues" + MainManager.languageid + "/Fishing").ToString().Replace("\r\n", "\n").Split(new char[]
		{
			'\n'
		});
	}

	// Token: 0x06000396 RID: 918 RVA: 0x00024A04 File Offset: 0x00022C04
	public static void CreateString()
	{
		if (MainManager.instance.flagstring[14].Length < 3)
		{
			string text = "";
			for (int i = 0; i < FishingMain.idKeys.Count; i++)
			{
				text += "0:0@";
			}
			MainManager.instance.flagstring[14] = text;
		}
	}

	// Token: 0x06000397 RID: 919 RVA: 0x00024A5C File Offset: 0x00022C5C
	private void Start()
	{
		FishingMain.instance = this;
		this.lineSound = base.GetComponent<AudioSource>();
		this.lineSound.volume *= MainManager.soundvolume;
		MainManager.ChangeMusic(this.musicPreload[0]);
		MainManager.instance.flagvar[1] = 0;
		MainManager.instance.flagvar[2] = 0;
		FishingMain.CreateString();
		this.record = FishingMain.GetFishRecords();
		this.hook.gameObject.SetActive(false);
		this.hookBubble.Stop();
		this.hookBubble.Clear();
		this.line.sortingOrder = 109;
		if (!MainManager.instance.flags[402])
		{
			this.party[3].gameObject.SetActive(false);
		}
		else
		{
			SpriteRenderer component = this.party[3].transform.GetChild(0).GetComponent<SpriteRenderer>();
			if (MainManager.instance.flags[404])
			{
				EntityControl.ChompyRibbon(component);
			}
			else
			{
				component.enabled = false;
			}
		}
		this.partySprite = new SpriteRenderer[this.party.Length];
		for (int i = 0; i < this.party.Length; i++)
		{
			this.partySprite[i] = this.party[i].GetComponentInChildren<SpriteRenderer>();
			if (i < 3 && MainManager.player.entity.hologram)
			{
				this.partySprite[i].material = MainManager.holosprite;
			}
		}
		FishData[] array = Resources.LoadAll<FishData>("Data/Fishing/Fish/");
		for (int j = 0; j < array.Length; j++)
		{
			this.fishData.Add(array[j].id, array[j]);
		}
		this.fishingItems = Resources.Load<FishingItems>("Data/Fishing/Items");
		this.text = FishingMain.LoadText();
		this.letter = DynamicFont.SetUp(this.GetDepth(), false, false, 20f, 0, 10, Vector2.one, this.box.transform, new Vector3(-2.5f, 1.7f, -0.2f), Color.black);
		this.letter.layer = 20;
		this.help = new GameObject[]
		{
			new GameObject("help 1"),
			new GameObject("help 2")
		};
		for (int k = 0; k < this.help.Length; k++)
		{
			MainManager.SetParenting(this.help[k].transform, this.box.transform);
		}
		this.AddButton(4, this.text[1], this.help[0].transform, new Vector3(-2f, 1.1f), 0.5f);
		this.AddButton(5, this.text[2], this.help[0].transform, new Vector3(-2f, 0.25f), 0.5f);
		this.AddButton(8, this.text[5], this.help[0].transform, new Vector3(-2f, -0.65f), 0.5f);
		MainManager.SystemText("|layer,20||sort,100||center||button,2|/|button,3| + |button,4||line|" + this.text[1], this.help[1].transform, new Vector3(0f, 0.5f));
		this.AddButton(4, this.text[4], this.castBox, new Vector3(-2.25f, 0.9f), 0.5f);
		this.AddButton(6, this.text[9], this.castBox, new Vector3(-2.25f, 0f), 0.5f);
		this.AddButton(5, this.text[5], this.castBox, new Vector3(-2.25f, -0.9f), 0.5f);
		base.Invoke("LateStart", 0.016666668f);
	}

	// Token: 0x06000398 RID: 920 RVA: 0x00024E08 File Offset: 0x00023008
	private void LateStart()
	{
		this.help[0].SetActive(false);
		this.help[1].SetActive(false);
		this.castBox.gameObject.SetActive(false);
		if (this.text[0].Length > 10)
		{
			this.letter.transform.localScale = new Vector3(0.8f, 1f, 1f);
		}
	}

	// Token: 0x06000399 RID: 921 RVA: 0x00024E78 File Offset: 0x00023078
	private void AddButton(int id, string label, Transform parent, Vector3 offset, float size = 0.5f)
	{
		ButtonSprite buttonSprite = new GameObject("button " + id).AddComponent<ButtonSprite>();
		buttonSprite.SetUp(id, -1, "|layer,20||single||sort,100|" + label, offset, Vector3.one * size, 100, parent);
		buttonSprite.layer = 20;
	}

	// Token: 0x0600039A RID: 922 RVA: 0x00024ECB File Offset: 0x000230CB
	private string GetDepth()
	{
		return this.text[0].Replace("@", this.text[3].Replace("@VAR@", this.depth.ToString("0.00") ?? ""));
	}

	// Token: 0x0600039B RID: 923 RVA: 0x00024F0A File Offset: 0x0002310A
	public void SetUp()
	{
		this.routine = base.StartCoroutine(this.ChooseBait());
	}

	// Token: 0x0600039C RID: 924 RVA: 0x00024F1E File Offset: 0x0002311E
	private IEnumerator ItemPrompt()
	{
		MainManager.DialogueText("|boxstyle,1,10||spd,0||sort,11|" + this.text[7] + "|pickitem,0,-11,-11|", null, null);
		while (MainManager.instance.message)
		{
			this.castBox.transform.localScale = Vector3.Lerp(this.castBox.transform.localScale, Vector3.zero, MainManager.framestep * 0.25f);
			yield return null;
		}
		if (!MainManager.listcanceled)
		{
			this.bait = (MainManager.Items)MainManager.instance.flagvar[0];
		}
		else
		{
			this.bait = MainManager.Items.None;
		}
		this.UpdateItem();
		yield break;
	}

	// Token: 0x0600039D RID: 925 RVA: 0x00024F30 File Offset: 0x00023130
	private void UpdateItem()
	{
		if (this.bait == MainManager.Items.None)
		{
			this.baitIcon.enabled = false;
			this.baitIcon2.enabled = false;
			return;
		}
		this.baitIcon.enabled = true;
		this.baitIcon.sprite = MainManager.itemsprites[0, (int)this.bait];
		this.baitIcon2.enabled = true;
		this.baitIcon2.sprite = this.baitIcon.sprite;
	}

	// Token: 0x0600039E RID: 926 RVA: 0x00024FA9 File Offset: 0x000231A9
	private IEnumerator Exit(bool skipFade = false)
	{
		if (!skipFade)
		{
			MainManager.FadeIn(0.05f);
			MainManager.FadeMusic(0.05f);
			yield return EventControl.sec;
			yield return EventControl.sec;
		}
		MainManager.instance.flagstring[14] = "";
		for (int i = 0; i < 10; i++)
		{
			ref string ptr = ref MainManager.instance.flagstring[14];
			ptr = string.Concat(new object[]
			{
				ptr,
				this.record[i][0],
				":",
				this.record[i][1],
				"@"
			});
		}
		MainManager.ChangeMusic(MainManager.map.music[0]);
		Object.Destroy(base.gameObject);
		yield break;
	}

	// Token: 0x0600039F RID: 927 RVA: 0x00024FBF File Offset: 0x000231BF
	private IEnumerator ChooseBait()
	{
		this.bait = MainManager.Items.None;
		this.hookFlap.transform.localEulerAngles = Vector3.zero;
		this.UpdateItem();
		yield return null;
		this.help[0].gameObject.SetActive(false);
		this.noticed = false;
		this.state = FishingMain.State.None;
		this.camera.localPosition = this.camStart;
		this.depth = 0f;
		this.letter.text = this.GetDepth();
		this.hook.gameObject.SetActive(false);
		this.hook.transform.position = this.startPoint;
		yield return null;
		this.castBox.gameObject.SetActive(true);
		this.castBox.transform.localScale = Vector3.zero;
		while (!MainManager.GetKey(4))
		{
			if (MainManager.GetKey(5))
			{
				MainManager.PlaySound("Cancel");
				MainManager.DialogueText("|boxstyle,1,10||spd,0||sort,11|" + this.text[6] + "|prompt,yesno,-11,-11,-1|", null, null);
				while (MainManager.instance.message)
				{
					this.castBox.transform.localScale = Vector3.Lerp(this.castBox.transform.localScale, Vector3.zero, MainManager.framestep * 0.25f);
					yield return null;
				}
				if (MainManager.instance.lastPrompt == 0)
				{
					FishingMain.instance.StartCoroutine(this.Exit(false));
					yield break;
				}
			}
			else if (MainManager.GetKey(6))
			{
				MainManager.PlaySound("Confirm");
				if (MainManager.instance.items[0].Count > 0)
				{
					yield return this.ItemPrompt();
				}
				else
				{
					MainManager.PlayBuzzer();
				}
			}
			this.castBox.transform.localScale = Vector3.Lerp(this.castBox.transform.localScale, Vector3.one, MainManager.framestep * 0.25f);
			yield return null;
		}
		MainManager.PlaySound("Confirm");
		this.hook.gameObject.SetActive(true);
		Animator t = this.hook.GetComponent<Animator>();
		t.enabled = true;
		MainManager.PlaySound("Woosh4");
		t.Play("HookStart", 0, 0f);
		this.party[2].Play(this.anims[5]);
		bool splash = false;
		float a = 0f;
		float b = 140f;
		while (a < b)
		{
			this.castBox.transform.localScale = Vector3.Lerp(this.castBox.transform.localScale, Vector3.zero, MainManager.framestep * 0.25f);
			if (a > 10f && this.hook.transform.localPosition.y < 5.35f && !splash)
			{
				splash = true;
				MainManager.ChangeMusic(this.musicPreload[1], 0.1f);
				MainManager.PlaySound("WaterSplash2");
				MainManager.ChangeLayer(MainManager.WaterSplash(this.hook.transform.position, Vector3.one).transform, 20);
			}
			yield return null;
			a += MainManager.framestep;
		}
		this.party[2].Play(this.anims[4]);
		this.help[0].gameObject.SetActive(true);
		this.hookBubble.Play();
		this.castBox.gameObject.SetActive(false);
		t.enabled = false;
		this.lastY = this.hook.transform.position.y;
		this.state = FishingMain.State.Started;
		this.routine = null;
		yield break;
	}

	// Token: 0x060003A0 RID: 928 RVA: 0x00024FD0 File Offset: 0x000231D0
	private void PartyAnim()
	{
		FishingMain.State state = this.state;
		if (state == FishingMain.State.None)
		{
			this.party[0].Play(this.anims[8]);
			this.partySprite[0].flipX = false;
			this.party[1].Play(this.anims[16]);
			this.partySprite[1].flipX = false;
			if (this.routine == null)
			{
				this.party[2].Play(this.anims[3]);
				this.partySprite[2].flipX = false;
			}
			this.party[3].Play(this.anims[17]);
			return;
		}
		if (state != FishingMain.State.Reeling)
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			if (this.hookTime < ((i == 0) ? 2000f : 1000f))
			{
				this.party[i].Play(this.anims[0]);
				if (i == 0 && this.pull != 4)
				{
					this.party[3].Play(this.anims[18]);
				}
			}
			else if (this.pull == 0)
			{
				this.party[i].Play(this.anims[2]);
			}
			else if (this.pull == 4)
			{
				this.party[i].Play(this.anims[14]);
				this.partySprite[i].flipX = true;
			}
			else if (this.pull == 3)
			{
				this.party[i].Play(this.anims[15]);
				this.partySprite[i].flipX = true;
			}
			else
			{
				this.party[i].Play(this.anims[1]);
			}
		}
		if (this.pull == 4)
		{
			this.party[3].Play(this.anims[19]);
		}
		if (this.pull == 0)
		{
			this.party[2].Play(this.anims[9]);
			return;
		}
		if (this.pull == 1)
		{
			this.party[2].Play(this.anims[12]);
			return;
		}
		if (this.pull == 2 || this.pull == -1)
		{
			this.party[2].Play(this.anims[11]);
		}
	}

	// Token: 0x060003A1 RID: 929 RVA: 0x000251FC File Offset: 0x000233FC
	private void Update()
	{
		this.PartyAnim();
		if (this.routine != null)
		{
			if (this.lineSound.isPlaying)
			{
				this.lineSound.Stop();
			}
			return;
		}
		switch (this.state)
		{
		case FishingMain.State.Started:
		{
			if (this.spawnCheck > 0f)
			{
				this.spawnCheck -= MainManager.framestep;
			}
			else if (this.fishes.Count < 5)
			{
				this.SpawnFish();
			}
			if (this.hook.transform.localPosition.y > -382f)
			{
				this.hook.transform.localPosition += MainManager.framestep * new Vector3(Mathf.Sin(Time.time * 0.75f) * 0.015f, -(this.noticed ? 0.005f : 0.015f));
			}
			this.DoInputs();
			float num = 0.25f;
			this.UpdateCamera(num);
			break;
		}
		case FishingMain.State.Reeling:
		{
			this.hookTime += MainManager.framestep;
			this.DoInputs();
			float num = 0.15f;
			this.UpdateCamera(num);
			if (this.fleeTime > 350f || this.tension >= 200f || this.depth >= 100f || Mathf.Abs(this.hooked.startY - this.depth) > 12.5f)
			{
				if (this.tension >= 200f)
				{
					MainManager.PlaySound("RopeSnap");
				}
				else
				{
					MainManager.PlaySound("Flee", -1, 0.9f, 0.9f);
				}
				this.routine = base.StartCoroutine(this.Fail(false, false));
				return;
			}
			if (this.depth <= 0f)
			{
				this.routine = base.StartCoroutine(this.Win());
			}
			break;
		}
		}
		if (Time.frameCount % 5 == 0)
		{
			this.letter.text = this.GetDepth();
		}
	}

	// Token: 0x060003A2 RID: 930 RVA: 0x000253F8 File Offset: 0x000235F8
	private void UpdateCamera(in float speed = 0.25f)
	{
		Vector3 vector = new Vector3(Mathf.Clamp(this.hook.transform.localPosition.x, -1f, 1f) + this.camOffset.x, Mathf.Clamp(this.hookY, -381f, 3f) + this.camOffset.y, -10f);
		if (speed >= 1f)
		{
			this.camera.localPosition = vector;
			return;
		}
		float t = MainManager.framestep * speed;
		this.camera.localPosition = Vector3.Lerp(this.camera.localPosition, vector, t);
	}

	// Token: 0x060003A3 RID: 931 RVA: 0x000254A0 File Offset: 0x000236A0
	private void DoInputs()
	{
		FishingMain.State state = this.state;
		float t;
		float b2;
		if (state == FishingMain.State.Started)
		{
			t = MainManager.framestep * 0.1f;
			float b = 0f;
			b2 = 0f;
			if (MainManager.GetKey(0, true) || MainManager.GetKey(4, true))
			{
				if (!this.lineSound.isPlaying)
				{
					this.lineSound.Play();
				}
				this.lineSound.pitch = 0.8f;
				if (this.holding < 600f)
				{
					this.holding += MainManager.framestep;
				}
				float num = Mathf.Clamp(this.holding / 120f, 1f, 5f);
				this.hook.transform.localPosition += MainManager.framestep * new Vector3(Mathf.Sin(Time.time) * 0.015f * num, 0.075f * num);
				b = Mathf.Sin(Time.time) * 15f;
				b2 = 90f;
			}
			else if (MainManager.GetKey(1, true) || MainManager.GetKey(5, true))
			{
				if (!this.lineSound.isPlaying)
				{
					this.lineSound.Play();
				}
				this.lineSound.pitch = 1.2f;
				if (this.holding < 600f)
				{
					this.holding += MainManager.framestep;
				}
				float num2 = Mathf.Clamp(this.holding / 120f, 1f, 5f);
				this.hook.transform.localPosition += MainManager.framestep * new Vector3(Mathf.Cos(Time.time) * 0.015f * num2, -0.075f * num2);
				b = 110f;
				b2 = -140f;
			}
			else
			{
				if (MainManager.GetKey(8))
				{
					this.state = FishingMain.State.Pause;
					for (int i = 0; i < this.fishes.Count; i++)
					{
						this.fishes[i].enabled = false;
					}
					this.routine = base.StartCoroutine(this.Fail(true, true));
					return;
				}
				this.holding = 0f;
				if (this.lineSound.isPlaying)
				{
					this.lineSound.Stop();
				}
			}
			this.hook.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(this.hook.transform.localEulerAngles.z, b, t));
			this.hook.transform.localPosition = new Vector3(Mathf.Lerp(this.hook.transform.localPosition.x, this.startPoint.x, t), Mathf.Clamp(this.hook.transform.localPosition.y, (!MainManager.instance.flags[722]) ? -35f : -382.25f, 4f));
			MainManager.music[0].volume = MainManager.musicvolume * (1f - this.depth / 100f);
			this.hookY = this.hook.transform.localPosition.y;
			this.hookFlap.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(this.hookFlap.transform.localEulerAngles.z, b2, t));
			return;
		}
		if (state != FishingMain.State.Reeling)
		{
			return;
		}
		b2 = 0f;
		float num3 = this.hookY;
		float num4 = this.startPoint.x;
		if (this.hooked.direction == 0)
		{
			this.hooked.direction = Random.Range(1, 3);
		}
		this.hooked.flip = (this.dirOffset[this.hooked.direction][3].x == 0f);
		if (MainManager.GetKey(4, true))
		{
			bool[] array = new bool[]
			{
				MainManager.GetKey(1, true),
				MainManager.GetKey(2, true),
				MainManager.GetKey(3, true)
			};
			if (!this.lineSound.isPlaying)
			{
				this.lineSound.Play();
			}
			if (this.hooked.dizzyTime > 0f || this.hooked.direction == -1 || array[this.hooked.direction])
			{
				if (this.fleeTime > 0f)
				{
					this.fleeTime -= MainManager.framestep * 1.5f;
				}
				if (this.hooked.dizzyTime <= 0f)
				{
					this.tension += MainManager.framestep * this.fishData[this.hooked.id].strength * 0.35f * this.hooked.strMod;
				}
				this.hookY += MainManager.framestep * 0.15f * ((this.hooked.dizzyTime > 0f) ? 1.5f : 1f);
				num4 += this.dirOffset[this.hooked.direction][1].x;
				num3 += this.dirOffset[this.hooked.direction][1].y;
				this.hooked.transform.localEulerAngles = new Vector3(0f, 0f, this.dirOffset[this.hooked.direction][3].y);
				this.lineSound.pitch = 1.2f;
				this.pull = this.hooked.direction;
				b2 = 90f;
			}
			else if (array[1] || array[2])
			{
				this.tension += MainManager.framestep * this.fishData[this.hooked.id].strength * 0.75f * this.hooked.strMod * Mathf.Clamp(this.hooked.scale, 0.75f, 1.25f);
				this.hookY += MainManager.framestep * -0.2f;
				num4 += this.dirOffset[this.hooked.direction][0].x;
				num3 += this.dirOffset[this.hooked.direction][0].y;
				this.hooked.transform.localEulerAngles = Vector3.zero;
				this.lineSound.pitch = 1f;
				this.pull = 0;
				b2 = -140f;
			}
			else
			{
				this.tension += MainManager.framestep * 0.1f;
				if (this.fleeTime > 0f)
				{
					this.fleeTime -= MainManager.framestep * 2f;
				}
				this.hookY += MainManager.framestep * 0.015f;
				num4 += this.dirOffset[this.hooked.direction][0].x;
				num3 += this.dirOffset[this.hooked.direction][0].y;
				this.hooked.transform.localEulerAngles = Vector3.zero;
				this.lineSound.pitch = 0.8f;
				this.pull = 0;
			}
		}
		else
		{
			if (this.tension > 0f)
			{
				this.tension -= MainManager.framestep * Mathf.Clamp(this.fishData[this.hooked.id].strength, 1.15f, 1.85f) * 1.075f;
			}
			this.hookY += MainManager.framestep * -0.115f * this.fishData[this.hooked.id].strength * this.hooked.strMod * Mathf.Clamp(this.hooked.scale, 0.5f, 1f);
			this.fleeTime += MainManager.framestep;
			num4 += this.dirOffset[this.hooked.direction][0].x;
			num3 += this.dirOffset[this.hooked.direction][0].y;
			this.hooked.transform.localEulerAngles = Vector3.zero;
			if (this.lineSound.isPlaying)
			{
				this.lineSound.Stop();
			}
			this.pull = 0;
			this.hooked.actionTime -= MainManager.framestep * 0.5f;
		}
		if (this.hooked.dizzyTime > 0f)
		{
			this.hooked.direction = -1;
			this.hooked.flip = false;
			this.hooked.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time) * 5f + -90f);
		}
		else if (this.hooked.effect != null)
		{
			MainManager.DestroyTemp(this.hooked.effect, 2f);
			this.hooked.effect = null;
			MainManager.PlaySound("Confirm2");
		}
		Vector3 b3 = new Vector3(num4, num3, this.hook.transform.localPosition.z);
		t = MainManager.framestep * 0.05f;
		this.hook.transform.localPosition = Vector3.Lerp(this.hook.transform.localPosition, b3, t);
		this.hookFlap.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(this.hookFlap.transform.localEulerAngles.z, b2, t));
	}

	// Token: 0x060003A4 RID: 932 RVA: 0x00025ED0 File Offset: 0x000240D0
	private void LateUpdate()
	{
		this.line.SetPositions(new Vector3[]
		{
			this.hookPoint.position,
			this.hook.transform.position
		});
		this.tensionBar.transform.localScale = new Vector3(Mathf.Lerp(0f, 0.58f, this.tension / 200f), 1f, 1f);
		this.tensionBar.color = Color.Lerp(Color.yellow, Color.red, this.tension / 200f);
		if (this.shake > 0f)
		{
			this.shake -= MainManager.framestep * 0.85f;
			this.hook.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * 3f) * this.shake);
		}
		if (this.state != FishingMain.State.None)
		{
			this.hookSpeed = Mathf.Abs(this.lastY - this.hookY) * 5f;
			this.lastY = this.hookY;
			this.depth = 100f * Mathf.InverseLerp(4f, -380f, this.hookY);
		}
	}

	// Token: 0x060003A5 RID: 933 RVA: 0x00026028 File Offset: 0x00024228
	private void SpawnFish()
	{
		this.fishes.RemoveAll((FishAI x) => x == null);
		this.possible.Clear();
		Vector3 vector = this.hook.transform.position + new Vector3((float)((Random.Range(0, 10) > 5) ? -10 : 10), (float)Random.Range(-5, 5), -0.1f);
		foreach (FishingMain.FishIDs fishIDs in FishingMain.idKeys.Values)
		{
			if (this.depth >= this.fishData[fishIDs].startDepth && this.depth <= this.fishData[fishIDs].maxDepth && this.combo >= this.fishData[fishIDs].reqCombo && (this.fishData[fishIDs].reqFlag == -1 || MainManager.instance.flags[this.fishData[fishIDs].reqFlag]))
			{
				bool flag = false;
				for (int i = 0; i < this.fishes.Count; i++)
				{
					if (Mathf.Abs(this.fishes[i].transform.position.y - vector.y) < 7f)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					int num = this.fishData[fishIDs].weight;
					if (fishIDs == FishingMain.FishIDs.Delta && this.bait == MainManager.Items.BlackCherry)
					{
						num *= 2;
					}
					for (int j = 0; j < num; j++)
					{
						this.possible.Add(fishIDs);
					}
				}
			}
		}
		if (this.possible.Count > 0)
		{
			FishingMain.FishIDs fishIDs2 = this.possible[Random.Range(0, this.possible.Count)];
			FishAI component = Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Fishing/Fish/" + fishIDs2)).GetComponent<FishAI>();
			this.fishes.Add(component);
			component.transform.parent = base.transform;
			component.transform.position = vector;
			component.transform.localPosition = new Vector3(component.transform.localPosition.x, Mathf.Clamp(component.transform.localPosition.y, -378f, 0f));
		}
		this.spawnCheck = (float)Random.Range(100, 300);
	}

	// Token: 0x060003A6 RID: 934 RVA: 0x000262F0 File Offset: 0x000244F0
	private IEnumerator Win()
	{
		this.combo++;
		if (this.combo > MainManager.instance.flagvar[2])
		{
			MainManager.instance.flagvar[2] = this.combo;
		}
		MainManager.instance.flagvar[1] += Mathf.Clamp(Mathf.FloorToInt((float)this.fishData[this.hooked.id].money * this.hooked.scale), 1, 10);
		this.pull = 3;
		this.record[this.fishInt[this.hooked.id]][0] += 1f;
		float size = this.fishData[this.hooked.id].cmSize * this.hooked.scale;
		if (size > this.record[this.fishInt[this.hooked.id]][1])
		{
			this.record[this.fishInt[this.hooked.id]][1] = size;
		}
		this.help[1].SetActive(false);
		MainManager.FadeMusic(0.025f);
		if (this.lineSound.isPlaying)
		{
			this.lineSound.Stop();
		}
		this.box.gameObject.SetActive(false);
		Vector3 p = new Vector3(0f, -1.4f, 8f);
		if (this.hooked.extraPart != null)
		{
			this.hooked.extraPart.Stop();
		}
		float a;
		for (a = 0f; a < 30f; a += MainManager.framestep)
		{
			float d = Mathf.SmoothStep(4f, 15f, a / 30f);
			this.spriteMask.localScale = Vector3.one * d;
			this.spriteMask.localPosition = Vector3.Lerp(this.spriteMask.localPosition, p, MainManager.framestep * 0.15f);
			this.partyBg.localScale = Vector3.one / d;
			this.hook.transform.localPosition = Vector3.Lerp(this.hook.transform.localPosition, this.startPoint, MainManager.framestep * 0.15f);
			yield return null;
		}
		this.party[2].Play(this.anims[10]);
		MainManager.PlaySound("Woosh4", 0.5f, 1f);
		this.spriteMask.localPosition = p;
		yield return EventControl.halfsec;
		this.hooked.enabled = false;
		this.hooked.transform.parent = this.partyBg.transform;
		this.hooked.segments[0].sprite = this.hooked.headSprites[0];
		p = new Vector3(7f, -3.85f);
		Vector3 tp = new Vector3(1.25f, 1.5f);
		this.hooked.transform.localScale = new Vector3(Mathf.Abs(this.hooked.transform.localScale.x), this.hooked.transform.localScale.y, this.hooked.transform.localScale.z);
		for (int i = 0; i < this.hooked.segments.Length; i++)
		{
			this.hooked.segments[i].color = Color.white;
			if (i > 1)
			{
				this.hooked.segments[i].transform.localEulerAngles = new Vector3(0f, 0f, -15f);
			}
		}
		this.partySprite[2].flipX = true;
		this.party[2].Play(this.anims[14]);
		this.kabbuLine.gameObject.SetActive(false);
		this.pull = 4;
		this.hooked.bubbles.gameObject.SetActive(false);
		this.hooked.flip = false;
		this.hooked.UpdateScale(true);
		this.hooked.GetComponent<SortingGroup>().sortingOrder = 105;
		this.hooked.transform.localPosition = p;
		MainManager.PlaySound("WaterOut");
		MainManager.ChangeLayer(MainManager.WaterSplash(this.hooked.transform.position, Vector3.one).transform, 20);
		if (this.hooked.extraPart != null)
		{
			this.hooked.extraPart.Play();
		}
		a = 0f;
		float b = 60f;
		while (a <= b + 1f)
		{
			this.hooked.transform.localPosition = MainManager.BeizierCurve(p, tp, 5f, a / b);
			this.hooked.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(-80f, -10f, a / b));
			yield return null;
			a += MainManager.framestep;
		}
		MainManager.PlaySound("ItemGet0");
		MainManager.DialogueText("|boxstyle,4||spd,0||center|" + this.text[8].Replace("@NAME@", this.text[13 + this.fishInt[this.hooked.id]]).Replace("@SIZE@", this.text[3].Replace("@VAR@", size.ToString("0.00"))), null, null);
		while (MainManager.instance.message)
		{
			yield return null;
		}
		if ((MainManager.instance.flagvar[68] & (int)this.hooked.id) == 0)
		{
			MainManager.instance.flagvar[68] = (MainManager.instance.flagvar[68] | (int)this.hooked.id);
		}
		if (!MainManager.instance.flags[722])
		{
			FishingMain.instance.StartCoroutine(this.Exit(false));
		}
		else
		{
			FishingMain.instance.routine = FishingMain.instance.StartCoroutine(FishingMain.instance.Restart());
		}
		yield break;
	}

	// Token: 0x060003A7 RID: 935 RVA: 0x000262FF File Offset: 0x000244FF
	public Transform Particle(string part, bool infinite = false)
	{
		Transform transform = MainManager.PlayParticle(part, null, this.baitIcon.transform.position, Vector3.one, (float)(infinite ? -1 : 5)).transform;
		transform.GetComponent<ParticleSystem>().Play();
		MainManager.ChangeLayer(transform, 20);
		return transform;
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x0002633D File Offset: 0x0002453D
	public static IEnumerator HookUp(FishAI target)
	{
		bool fail = false;
		float hookTime = 35f;
		target.segments[0].sprite = target.headSprites[2];
		MainManager.ShakeScreen(0.1f);
		if (FishingMain.instance.bait != MainManager.Items.None)
		{
			int value = Convert.ToInt32(MainManager.itemdata[0, (int)FishingMain.instance.bait, 4]) + MainManager.GetItemUse((int)FishingMain.instance.bait, 0).values.Length * 10;
			MainManager.PlaySound("Clomp");
			FishingMain.instance.baitIcon.enabled = false;
			target.dizzyTime = (float)Mathf.Clamp(value * 2, 90, 300);
			target.strMod = Mathf.Clamp(1f - (float)value / 25f, 0.5f, 1f);
			yield return EventControl.thirdsec;
			FishingItems.Groups groups = FishingItems.Groups.None;
			MainManager.instance.items[0].Remove((int)FishingMain.instance.bait);
			for (int i = 0; i < FishingMain.instance.fishingItems.groups.Length; i++)
			{
				if (FishingMain.instance.fishingItems.groups[i].items.Contains(FishingMain.instance.bait))
				{
					groups = FishingMain.instance.fishingItems.groups[i].type;
					break;
				}
			}
			switch (groups)
			{
			case FishingItems.Groups.Bombs:
				MainManager.PlaySound("Explosion5");
				MainManager.ShakeScreen(1f);
				FishingMain.instance.Particle("explosion", false).localScale *= 2f;
				if (FishingMain.instance.fishData[target.id].explodable)
				{
					Object.Destroy(target.gameObject);
					yield return EventControl.sec;
					FishingMain.instance.routine = FishingMain.instance.StartCoroutine(FishingMain.instance.Fail(false, false));
					yield break;
				}
				target.dizzyTime = (float)(500 + value * 2);
				target.strMod = 0.5f;
				break;
			case FishingItems.Groups.Poison:
				MainManager.PlaySound("Poison");
				target.strMod = 0.85f;
				FishingMain.instance.Particle("PoisonEffect", false);
				target.effect = FishingMain.instance.Particle("PoisonEffect2", true).gameObject;
				target.dizzyTime += 150f;
				break;
			case FishingItems.Groups.Freeze:
				MainManager.PlaySound("Freeze");
				target.dizzyTime += 200f;
				target.strMod = 0.75f;
				FishingMain.instance.Particle("mothicenormal", false);
				target.effect = FishingMain.instance.Particle("Snowflakes", true).gameObject;
				break;
			case FishingItems.Groups.Numb:
				MainManager.PlaySound("Numb");
				target.dizzyTime += 200f;
				FishingMain.instance.Particle("ElecFast", false);
				target.effect = FishingMain.instance.Particle("Elec", true).gameObject;
				target.strMod = 0.8f;
				break;
			case FishingItems.Groups.Sleep:
				MainManager.PlaySound("Sleep");
				target.dizzyTime += 210f;
				target.strMod = 0.65f;
				FishingMain.instance.Particle("deathsmoke", false);
				break;
			}
			hookTime = 55f;
			if (target.effect != null)
			{
				target.effect.transform.parent = target.transform;
			}
			FishingMain.instance.bait = MainManager.Items.None;
		}
		fail = true;
		for (float a = 0f; a < hookTime; a += MainManager.framestep)
		{
			target.transform.position = FishingMain.instance.baitIcon.transform.position;
			if (MainManager.GetKey(4) || target.dizzyTime > 50f)
			{
				MainManager.ChangeMusic(FishingMain.instance.musicPreload[FishingMain.instance.fishData[target.id].music]);
				MainManager.music[0].pitch = FishingMain.instance.fishData[target.id].musicPitch;
				FishingMain.instance.hooked = target;
				FishingMain.instance.help[0].SetActive(false);
				FishingMain.instance.help[1].SetActive(true);
				target.state = FishAI.State.Reeling;
				FishingMain.instance.state = FishingMain.State.Reeling;
				FishingMain.instance.routine = null;
				target.direction = Random.Range(0, 3);
				for (int j = 0; j < FishingMain.instance.fishes.Count; j++)
				{
					if (FishingMain.instance.fishes[j] != target)
					{
						FishingMain.instance.fishes[j].state = FishAI.State.Fleeing;
					}
				}
				yield break;
			}
			yield return null;
		}
		target.state = FishAI.State.Fleeing;
		target.segments[0].sprite = target.headSprites[0];
		if (fail)
		{
			FishingMain.instance.routine = FishingMain.instance.StartCoroutine(FishingMain.instance.Fail(false, false));
		}
		else
		{
			FishingMain.instance.state = FishingMain.State.Started;
			FishingMain.instance.routine = null;
		}
		yield break;
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x0002634C File Offset: 0x0002454C
	private IEnumerator Fail(bool keepCombo = false, bool fast = false)
	{
		if (this.lineSound.isPlaying)
		{
			this.lineSound.Stop();
		}
		if (!keepCombo)
		{
			this.combo = 0;
		}
		if (this.hooked != null)
		{
			this.hooked.state = FishAI.State.Fleeing;
		}
		if (!fast)
		{
			this.pull = 5;
			this.party[2].Play(this.anims[13]);
			MainManager.FadeMusic(0.05f);
			MainManager.PlaySound("AtkFail");
			for (float a = 0f; a < 60f; a += MainManager.framestep)
			{
				this.hook.transform.localPosition = Vector3.Lerp(this.hook.transform.localPosition, new Vector3(this.startPoint.x, this.hook.transform.localPosition.y, this.hook.transform.localPosition.z), MainManager.framestep * 0.05f);
				yield return null;
			}
		}
		FishingMain.instance.routine = FishingMain.instance.StartCoroutine(FishingMain.instance.Restart());
		yield break;
	}

	// Token: 0x060003AA RID: 938 RVA: 0x00026369 File Offset: 0x00024569
	private IEnumerator Restart()
	{
		MainManager.FadeIn(0.05f);
		yield return EventControl.sec;
		yield return EventControl.sec;
		this.state = FishingMain.State.None;
		this.hook.gameObject.SetActive(false);
		this.help[0].SetActive(false);
		this.help[1].SetActive(false);
		this.hookBubble.Stop();
		this.hookBubble.Clear();
		this.depth = 0f;
		this.letter.text = this.GetDepth();
		this.baitIcon2.enabled = false;
		this.tension = 0f;
		this.fleeTime = 0f;
		this.hookTime = 0f;
		this.spriteMask.localScale = Vector3.one * 4f;
		this.spriteMask.localPosition = this.maskPos;
		this.partySprite[2].flipX = false;
		this.party[2].Play(this.anims[3]);
		this.kabbuLine.gameObject.SetActive(true);
		MainManager.ChangeMusic(this.musicPreload[0], 0.1f);
		MainManager.music[0].pitch = 1f;
		this.partyBg.localScale = Vector3.one / this.spriteMask.localScale.x;
		if (this.hooked != null)
		{
			Object.Destroy(this.hooked.gameObject);
		}
		this.ClearFish();
		this.box.gameObject.SetActive(true);
		this.camera.localPosition = this.camStart;
		yield return null;
		MainManager.FadeOut(0.05f);
		yield return EventControl.sec;
		this.SetUp();
		yield break;
	}

	// Token: 0x060003AB RID: 939 RVA: 0x00026378 File Offset: 0x00024578
	private void ClearFish()
	{
		for (int i = 0; i < this.fishes.Count; i++)
		{
			if (this.fishes[i] != null)
			{
				Object.Destroy(this.fishes[i].gameObject);
			}
		}
		this.fishes.Clear();
		this.spawnCheck = (float)Random.Range(50, 300);
	}

	// Token: 0x040002D6 RID: 726
	public AudioClip[] musicPreload;

	// Token: 0x040002D7 RID: 727
	public static readonly Dictionary<int, FishingMain.FishIDs> idKeys = new Dictionary<int, FishingMain.FishIDs>
	{
		{
			0,
			FishingMain.FishIDs.Wormling
		},
		{
			1,
			FishingMain.FishIDs.LakeWorm
		},
		{
			2,
			FishingMain.FishIDs.GoldenWorm
		},
		{
			3,
			FishingMain.FishIDs.LakeWyrm
		},
		{
			4,
			FishingMain.FishIDs.LongFin
		},
		{
			5,
			FishingMain.FishIDs.StrippedWorm
		},
		{
			6,
			FishingMain.FishIDs.Biter
		},
		{
			7,
			FishingMain.FishIDs.HornedWyrm
		},
		{
			8,
			FishingMain.FishIDs.RoundOne
		},
		{
			9,
			FishingMain.FishIDs.Delta
		}
	};

	// Token: 0x040002D8 RID: 728
	public readonly Dictionary<FishingMain.FishIDs, int> fishInt = new Dictionary<FishingMain.FishIDs, int>
	{
		{
			FishingMain.FishIDs.Wormling,
			0
		},
		{
			FishingMain.FishIDs.LakeWorm,
			1
		},
		{
			FishingMain.FishIDs.GoldenWorm,
			2
		},
		{
			FishingMain.FishIDs.LakeWyrm,
			3
		},
		{
			FishingMain.FishIDs.LongFin,
			4
		},
		{
			FishingMain.FishIDs.StrippedWorm,
			5
		},
		{
			FishingMain.FishIDs.Biter,
			6
		},
		{
			FishingMain.FishIDs.HornedWyrm,
			7
		},
		{
			FishingMain.FishIDs.RoundOne,
			8
		},
		{
			FishingMain.FishIDs.Delta,
			9
		}
	};

	// Token: 0x040002D9 RID: 729
	public Dictionary<FishingMain.FishIDs, FishData> fishData = new Dictionary<FishingMain.FishIDs, FishData>();

	// Token: 0x040002DA RID: 730
	private FishingItems fishingItems;

	// Token: 0x040002DB RID: 731
	public static FishingMain instance;

	// Token: 0x040002DC RID: 732
	public const int varID = 68;

	// Token: 0x040002DD RID: 733
	public const int maxFish = 5;

	// Token: 0x040002DE RID: 734
	public const int fishName = 13;

	// Token: 0x040002DF RID: 735
	public const int fishAmt = 10;

	// Token: 0x040002E0 RID: 736
	private const float tensionAmt = 200f;

	// Token: 0x040002E1 RID: 737
	private const float barMax = 0.58f;

	// Token: 0x040002E2 RID: 738
	private const float itemRange = 1.35f;

	// Token: 0x040002E3 RID: 739
	private const float playerTime = 1000f;

	// Token: 0x040002E4 RID: 740
	private readonly Vector3 startPoint = new Vector3(3f, 10f);

	// Token: 0x040002E5 RID: 741
	private readonly Vector3 camStart = new Vector3(1f, 3f, -10f);

	// Token: 0x040002E6 RID: 742
	private readonly Vector3 maskPos = new Vector3(-6f, 3.25f, 8f);

	// Token: 0x040002E7 RID: 743
	private float[][] record;

	// Token: 0x040002E8 RID: 744
	private float tension;

	// Token: 0x040002E9 RID: 745
	private float spawnCheck;

	// Token: 0x040002EA RID: 746
	private float hookTime;

	// Token: 0x040002EB RID: 747
	private float fleeTime;

	// Token: 0x040002EC RID: 748
	private float hookY;

	// Token: 0x040002ED RID: 749
	private float lastY;

	// Token: 0x040002EE RID: 750
	private float holding;

	// Token: 0x040002EF RID: 751
	private int combo;

	// Token: 0x040002F0 RID: 752
	private int pull;

	// Token: 0x040002F1 RID: 753
	private string[] text;

	// Token: 0x040002F2 RID: 754
	private Vector3 camOffset;

	// Token: 0x040002F3 RID: 755
	private GameObject[] help;

	// Token: 0x040002F4 RID: 756
	private SpriteRenderer[] partySprite;

	// Token: 0x040002F5 RID: 757
	private AudioSource lineSound;

	// Token: 0x040002F6 RID: 758
	[HideInInspector]
	public MainManager.Items bait = MainManager.Items.None;

	// Token: 0x040002F7 RID: 759
	[HideInInspector]
	public FishingMain.State state;

	// Token: 0x040002F8 RID: 760
	[HideInInspector]
	public FishAI hooked;

	// Token: 0x040002F9 RID: 761
	[HideInInspector]
	public bool noticed;

	// Token: 0x040002FA RID: 762
	[HideInInspector]
	public float shake;

	// Token: 0x040002FB RID: 763
	[HideInInspector]
	public float depth;

	// Token: 0x040002FC RID: 764
	public float hookSpeed;

	// Token: 0x040002FD RID: 765
	public Coroutine routine;

	// Token: 0x040002FE RID: 766
	[HideInInspector]
	public List<FishAI> fishes = new List<FishAI>();

	// Token: 0x040002FF RID: 767
	private List<FishingMain.FishIDs> possible = new List<FishingMain.FishIDs>();

	// Token: 0x04000300 RID: 768
	private DynamicFont letter;

	// Token: 0x04000301 RID: 769
	public Animator[] party = new Animator[3];

	// Token: 0x04000302 RID: 770
	public SpriteRenderer hook;

	// Token: 0x04000303 RID: 771
	public SpriteRenderer hookFlap;

	// Token: 0x04000304 RID: 772
	public SpriteRenderer box;

	// Token: 0x04000305 RID: 773
	public SpriteRenderer tensionBar;

	// Token: 0x04000306 RID: 774
	public SpriteRenderer baitIcon;

	// Token: 0x04000307 RID: 775
	public SpriteRenderer baitIcon2;

	// Token: 0x04000308 RID: 776
	public SpriteRenderer kabbuLine;

	// Token: 0x04000309 RID: 777
	public Transform hookPoint;

	// Token: 0x0400030A RID: 778
	public Transform background;

	// Token: 0x0400030B RID: 779
	public Transform partyBg;

	// Token: 0x0400030C RID: 780
	public Transform spriteMask;

	// Token: 0x0400030D RID: 781
	public Transform castBox;

	// Token: 0x0400030E RID: 782
	public Transform camera;

	// Token: 0x0400030F RID: 783
	public LineRenderer line;

	// Token: 0x04000310 RID: 784
	public ParticleSystem hookBubble;

	// Token: 0x04000311 RID: 785
	private const string systemBox = "|boxstyle,1,10||spd,0||sort,11|";

	// Token: 0x04000312 RID: 786
	private readonly string[] anims = new string[]
	{
		"FishAllyLook",
		"FishAllyPull",
		"FishAllyRelax",
		"BaitSelect",
		"HookWait",
		"HookCast",
		"Idle",
		"Walk",
		"Angry",
		"HookSurprise",
		"HookPull",
		"HookPullL",
		"HookPullR",
		"HookLost",
		"ItemGet",
		"Hurt",
		"114",
		"Sleep",
		"101",
		"Happy"
	};

	// Token: 0x04000313 RID: 787
	private const float camFix = 5f;

	// Token: 0x04000314 RID: 788
	private readonly Dictionary<int, Vector3[]> dirOffset = new Dictionary<int, Vector3[]>
	{
		{
			-1,
			new Vector3[]
			{
				Vector3.zero,
				Vector3.zero,
				Vector3.up * 5f,
				Vector3.zero
			}
		},
		{
			1,
			new Vector3[]
			{
				new Vector3(4f, -1f),
				new Vector3(1.5f, 0.5f),
				new Vector3(-1.5f, 5f),
				new Vector3(0f, 45f)
			}
		},
		{
			2,
			new Vector3[]
			{
				new Vector3(-4f, -1f),
				new Vector3(-1.5f, 0.5f),
				new Vector3(1.5f, 5f),
				new Vector3(1f, -45f)
			}
		}
	};

	// Token: 0x020001E3 RID: 483
	[Flags]
	public enum FishIDs
	{
		// Token: 0x0400162A RID: 5674
		Wormling = 1,
		// Token: 0x0400162B RID: 5675
		LakeWorm = 2,
		// Token: 0x0400162C RID: 5676
		GoldenWorm = 4,
		// Token: 0x0400162D RID: 5677
		LakeWyrm = 8,
		// Token: 0x0400162E RID: 5678
		LongFin = 16,
		// Token: 0x0400162F RID: 5679
		StrippedWorm = 32,
		// Token: 0x04001630 RID: 5680
		Biter = 64,
		// Token: 0x04001631 RID: 5681
		HornedWyrm = 128,
		// Token: 0x04001632 RID: 5682
		RoundOne = 256,
		// Token: 0x04001633 RID: 5683
		Delta = 512
	}

	// Token: 0x020001E4 RID: 484
	public enum State
	{
		// Token: 0x04001635 RID: 5685
		None,
		// Token: 0x04001636 RID: 5686
		Started,
		// Token: 0x04001637 RID: 5687
		Pause,
		// Token: 0x04001638 RID: 5688
		Reeling
	}
}
