using System;
using InputIOManager;
using UnityEngine;

// Token: 0x02000007 RID: 7
public class ButtonSprite : MonoBehaviour
{
	// Token: 0x06000157 RID: 343 RVA: 0x0000FE50 File Offset: 0x0000E050
	public ButtonSprite SetUp(int buttonid, int onlytype, string description, Vector3 position, Vector3 iconsize, int sortorder, Transform parentobj)
	{
		return this.SetUp(buttonid, onlytype, description, position, iconsize, sortorder, parentobj, Color.white);
	}

	// Token: 0x06000158 RID: 344 RVA: 0x0000FE74 File Offset: 0x0000E074
	public ButtonSprite SetUp(int buttonid, int onlytype, string description, Vector3 position, Vector3 iconsize, int sortorder, Transform parentobj, Vector3 label_offset)
	{
		return this.SetUp(buttonid, onlytype, description, position, iconsize, sortorder, parentobj, Color.white, label_offset);
	}

	// Token: 0x06000159 RID: 345 RVA: 0x0000FE9C File Offset: 0x0000E09C
	public ButtonSprite SetUp(int buttonid, int onlytype, string description, Vector3 position, Vector3 iconsize, int sortorder, Transform parentobj, Color startcolor)
	{
		return this.SetUp(buttonid, onlytype, description, position, iconsize, sortorder, parentobj, Color.white, Vector3.zero);
	}

	// Token: 0x0600015A RID: 346 RVA: 0x0000FEC4 File Offset: 0x0000E0C4
	public ButtonSprite SetUp(int buttonid, int onlytype, string description, Vector3 position, Vector3 iconsize, int sortorder, Transform parentobj, Color startcolor, Vector3 label_offset)
	{
		this.overridesortorder = sortorder;
		this.tposition = new Vector3?(position);
		this.size = new Vector3?(iconsize);
		this.parent = parentobj;
		this.basecolor = startcolor;
		this.labeloffset = label_offset;
		return this.SetUp(buttonid, onlytype, description);
	}

	// Token: 0x0600015B RID: 347 RVA: 0x0000FF14 File Offset: 0x0000E114
	public ButtonSprite SetUp(int buttonid, int onlytype, string description)
	{
		this.id = buttonid;
		this.labeltext = description;
		this.onlyone = onlytype;
		return this;
	}

	// Token: 0x0600015C RID: 348 RVA: 0x0000FF2C File Offset: 0x0000E12C
	private void Start()
	{
		this.basesprite = base.GetComponent<SpriteRenderer>();
		if (this.basesprite == null)
		{
			this.basesprite = base.gameObject.AddComponent<SpriteRenderer>();
		}
		this.basesprite.color = this.basecolor;
		this.SetText();
		if (!this.tridimentional)
		{
			base.gameObject.layer = this.layer;
		}
		this.centerx = MainManager.lasttextcenter;
		this.textsize = MainManager.textwidth;
		this.SetBase();
		if (this.parent != null)
		{
			base.transform.parent = this.parent;
			base.transform.localEulerAngles = Vector3.zero;
		}
		if (this.tposition != null)
		{
			base.transform.localPosition = this.tposition.Value;
		}
		if (this.size != null)
		{
			base.transform.localScale = this.size.Value;
		}
		if (this.labeltext != null && this.labeltext.Length > 0)
		{
			float x = 1f;
			if (!Application.isConsolePlatform && this.text.childCount > 1)
			{
				x = 2f;
			}
			base.StartCoroutine(MainManager.SetText(((MainManager.languageid == 4) ? "|single|" : "") + "|quarterline|" + this.labeltext, 0, new float?((float)999999), false, this.tridimentional, new Vector3(x, -0.15f) + this.labeloffset, Vector3.zero, Vector2.one + base.transform.localScale, base.transform, null));
		}
		if (base.transform.childCount >= 2)
		{
			this.label = base.transform.GetChild(1);
		}
		this.basesprite.sortingOrder = this.overridesortorder;
	}

	// Token: 0x0600015D RID: 349 RVA: 0x00010114 File Offset: 0x0000E314
	private void SetText()
	{
		this.buttonname = InputIO.ButtonIsLong(this.id);
		if (this.buttonname.Contains("Arrow"))
		{
			this.arrow = new GameObject("Arrow").AddComponent<SpriteRenderer>();
			this.arrow.transform.parent = base.transform;
			this.arrow.transform.localPosition = Vector3.zero;
			this.arrow.transform.localEulerAngles = Vector3.zero;
			this.arrow.sprite = MainManager.guisprites[11];
			this.arrow.sortingOrder = this.overridesortorder + 1;
			this.arrow.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			if (!this.tridimentional)
			{
				this.arrow.gameObject.layer = base.gameObject.layer;
			}
			this.isarrow = true;
			this.arrow.gameObject.layer = this.layer;
			string a = this.buttonname;
			if (!(a == "DownArrow"))
			{
				if (!(a == "LeftArrow"))
				{
					if (a == "RightArrow")
					{
						this.arrow.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
					}
				}
				else
				{
					this.arrow.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				}
			}
			else
			{
				this.arrow.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
			}
		}
		else
		{
			string text = "";
			if (this.layer != 5)
			{
				text = "|layer," + this.layer + "|";
			}
			base.StartCoroutine(MainManager.SetText(string.Concat(new object[]
			{
				text,
				"|sort,",
				this.overridesortorder + 1,
				"||center||textangle||font,0|",
				this.buttonname
			}), 0, null, false, this.tridimentional, new Vector3(0f, -0.45f), Vector3.zero, new Vector2((this.buttonname.Length >= 7) ? 1f : 1.5f, 1.5f), base.transform, null));
		}
		this.text = base.transform.GetChild(0);
	}

	// Token: 0x0600015E RID: 350 RVA: 0x000103A4 File Offset: 0x0000E5A4
	public void ChangeButton(int newid)
	{
		if (this.text != null)
		{
			Object.Destroy(this.text.gameObject);
		}
		this.id = newid;
		this.Start();
	}

	// Token: 0x0600015F RID: 351 RVA: 0x000103D1 File Offset: 0x0000E5D1
	private void LateUpdate()
	{
		this.SetBase();
	}

	// Token: 0x06000160 RID: 352 RVA: 0x000103DC File Offset: 0x0000E5DC
	private void SetBase()
	{
		if (this.text == null)
		{
			this.SetText();
		}
		bool flag = false;
		if (this.onlyone == -1)
		{
			flag = MainManager.joystick;
		}
		else if (this.onlyone >= 1)
		{
			flag = true;
		}
		if (this.size == null)
		{
			this.size = new Vector3?(Vector3.one);
		}
		if (flag)
		{
			if (this.text != null && this.text.gameObject.activeSelf)
			{
				this.text.gameObject.SetActive(false);
			}
			int joyid = MainManager.joyid;
			switch (joyid)
			{
			case 0:
				goto IL_367;
			case 1:
			case 5:
				break;
			case 2:
				goto IL_335;
			case 3:
			case 4:
				goto IL_1E7;
			case 6:
			case 7:
			case 8:
			case 9:
				goto IL_492;
			case 10:
				if (this.id >= 8)
				{
					this.basesprite.sprite = MainManager.guisprites[(this.id == 8) ? 136 : 135];
					goto IL_4AE;
				}
				goto IL_1E7;
			default:
				switch (joyid)
				{
				case 100:
					goto IL_46A;
				case 101:
				case 102:
					goto IL_367;
				case 103:
					goto IL_428;
				case 104:
					if (this.id < 4)
					{
						goto IL_46A;
					}
					goto IL_428;
				case 105:
					break;
				case 106:
					if (this.id < 4)
					{
						goto IL_46A;
					}
					break;
				case 107:
					if (this.id < 4)
					{
						goto IL_46A;
					}
					goto IL_14D;
				case 108:
					if (this.id < 4)
					{
						goto IL_46A;
					}
					goto IL_335;
				case 109:
					goto IL_3D4;
				case 110:
					if (this.id < 4)
					{
						goto IL_46A;
					}
					goto IL_3D4;
				default:
					goto IL_492;
				}
				if (this.id == 8)
				{
					this.basesprite.sprite = MainManager.guisprites[206];
					goto IL_4AE;
				}
				if (this.id == 9)
				{
					this.basesprite.sprite = MainManager.guisprites[207];
					goto IL_4AE;
				}
				break;
				IL_3D4:
				if (this.id == 8)
				{
					this.basesprite.sprite = MainManager.guisprites[211];
					goto IL_4AE;
				}
				if (this.id == 9)
				{
					this.basesprite.sprite = MainManager.guisprites[210];
					goto IL_4AE;
				}
				IL_428:
				if (this.id == 6)
				{
					this.basesprite.sprite = MainManager.guisprites[208];
					goto IL_4AE;
				}
				if (this.id == 7)
				{
					this.basesprite.sprite = MainManager.guisprites[209];
					goto IL_4AE;
				}
				goto IL_492;
			}
			IL_14D:
			if (Application.isConsolePlatform && this.id == 9)
			{
				this.basesprite.sprite = MainManager.guisprites[126];
				goto IL_4AE;
			}
			if (this.id < 4)
			{
				goto IL_492;
			}
			if (MainManager.languageid == 3 && (this.id == 4 || this.id == 5))
			{
				this.basesprite.sprite = MainManager.instance.joybuttonsps[(this.id == 4) ? 1 : 0];
				goto IL_4AE;
			}
			this.basesprite.sprite = MainManager.instance.joybuttonsps[this.id - 4];
			goto IL_4AE;
			IL_1E7:
			if (this.id <= 3)
			{
				this.basesprite.sprite = MainManager.guisprites[164 + this.id];
				goto IL_4AE;
			}
			switch (this.id)
			{
			case 4:
				this.basesprite.sprite = MainManager.guisprites[141];
				goto IL_4AE;
			case 5:
				this.basesprite.sprite = MainManager.guisprites[142];
				goto IL_4AE;
			case 6:
				this.basesprite.sprite = MainManager.guisprites[143];
				goto IL_4AE;
			case 7:
				this.basesprite.sprite = MainManager.guisprites[144];
				goto IL_4AE;
			case 8:
				this.basesprite.sprite = MainManager.guisprites[(MainManager.joyid == 3) ? 138 : 137];
				goto IL_4AE;
			case 9:
				this.basesprite.sprite = MainManager.guisprites[(MainManager.joyid == 3) ? 136 : 135];
				goto IL_4AE;
			default:
				goto IL_4AE;
			}
			IL_335:
			this.basesprite.sprite = MainManager.instance.joybuttons[this.id + 10];
			goto IL_4AE;
			IL_367:
			if (this.id >= 8 && (Application.platform == RuntimePlatform.XboxOne || MainManager.joyid >= 101))
			{
				this.basesprite.sprite = MainManager.guisprites[this.id + 131];
				goto IL_4AE;
			}
			if (MainManager.joyid != 102)
			{
				goto IL_492;
			}
			IL_46A:
			if (this.id <= 3)
			{
				this.basesprite.sprite = MainManager.guisprites[164 + this.id];
				goto IL_4AE;
			}
			IL_492:
			this.basesprite.sprite = MainManager.instance.joybuttons[this.id];
			IL_4AE:
			base.transform.localScale = this.size.Value;
			return;
		}
		if (this.text != null && !this.text.gameObject.activeSelf)
		{
			this.text.gameObject.SetActive(true);
		}
		if (this.buttonname.Length == 1 || this.isarrow)
		{
			this.basesprite.sprite = MainManager.guisprites[9];
			return;
		}
		this.basesprite.sprite = MainManager.guisprites[10];
		if (this.shrunkkey)
		{
			base.transform.localScale = MainManager.MultiplyVector(ButtonSprite.shrunk, this.size.Value);
		}
	}

	// Token: 0x040000B9 RID: 185
	private int id;

	// Token: 0x040000BA RID: 186
	private int onlyone = -1;

	// Token: 0x040000BB RID: 187
	private int overridesortorder;

	// Token: 0x040000BC RID: 188
	public SpriteRenderer basesprite;

	// Token: 0x040000BD RID: 189
	private SpriteRenderer arrow;

	// Token: 0x040000BE RID: 190
	private Transform text;

	// Token: 0x040000BF RID: 191
	private Transform label;

	// Token: 0x040000C0 RID: 192
	private Transform parent;

	// Token: 0x040000C1 RID: 193
	private Vector3 labeloffset;

	// Token: 0x040000C2 RID: 194
	public float centerx;

	// Token: 0x040000C3 RID: 195
	public float textsize;

	// Token: 0x040000C4 RID: 196
	public int layer = 5;

	// Token: 0x040000C5 RID: 197
	private string labeltext;

	// Token: 0x040000C6 RID: 198
	public string buttonname;

	// Token: 0x040000C7 RID: 199
	private Color basecolor = Color.white;

	// Token: 0x040000C8 RID: 200
	private bool isarrow;

	// Token: 0x040000C9 RID: 201
	private static Vector3 shrunk = new Vector3(0.55f, 1f, 1f);

	// Token: 0x040000CA RID: 202
	public bool tridimentional;

	// Token: 0x040000CB RID: 203
	public bool shrunkkey;

	// Token: 0x040000CC RID: 204
	private Vector3? tposition;

	// Token: 0x040000CD RID: 205
	private Vector3? size;
}
