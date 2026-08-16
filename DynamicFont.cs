using System;
using UnityEngine;

// Token: 0x02000015 RID: 21
public class DynamicFont : MonoBehaviour
{
	// Token: 0x060001B6 RID: 438 RVA: 0x00014BB0 File Offset: 0x00012DB0
	public static DynamicFont SetUp(string displaytext, bool centralized, bool nokerning, float frequency, int fonttype, int sortorder, Vector2 fontsize, Transform parent, Vector3 position, Color color)
	{
		return DynamicFont.SetUp(displaytext, centralized, nokerning, frequency, fonttype, sortorder, fontsize, parent, position, color, null);
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x00014BDC File Offset: 0x00012DDC
	public static DynamicFont SetUp(bool nokerning, float frequency, int fonttype, int sortorder, Vector2 fontsize, Transform parent, Vector3 position)
	{
		return DynamicFont.SetUp("", false, nokerning, frequency, fonttype, sortorder, fontsize, parent, position, Color.white, null);
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x00014C0C File Offset: 0x00012E0C
	public static DynamicFont SetUp(string displaytext, bool centralized, bool nokerning, float frequency, int fonttype, int sortorder, Vector2 fontsize, Transform parent, Vector3 position, Color color, Vector2? small)
	{
		DynamicFont dynamicFont = new GameObject("DynFont - " + displaytext).AddComponent<DynamicFont>();
		dynamicFont.startpos = position;
		dynamicFont.text = displaytext;
		dynamicFont.center = centralized;
		dynamicFont.monospace = nokerning;
		dynamicFont.updatefrequency = frequency;
		dynamicFont.fontindex = fonttype;
		dynamicFont.fontcolor = color;
		dynamicFont.size = fontsize;
		dynamicFont.sort = sortorder;
		dynamicFont.smallersize = small;
		if (parent == null)
		{
			dynamicFont.transform.parent = MainManager.GUICamera.transform;
		}
		else
		{
			dynamicFont.transform.parent = parent;
		}
		dynamicFont.transform.localScale = Vector3.one;
		return dynamicFont;
	}

	// Token: 0x060001B9 RID: 441 RVA: 0x00014CBC File Offset: 0x00012EBC
	private void Start()
	{
		this.fontindex = MainManager.FontID(this.fontindex);
		if (base.transform.parent == null)
		{
			base.transform.parent = MainManager.GUICamera.transform;
		}
		base.transform.localEulerAngles = Vector3.zero;
		base.transform.localPosition = this.startpos;
		this.UpdateLetters();
	}

	// Token: 0x060001BA RID: 442 RVA: 0x00014D2C File Offset: 0x00012F2C
	private void Update()
	{
		this.cooldown -= MainManager.framestep;
		if (this.cooldown <= 0f)
		{
			for (int i = 0; i < this.letters.Length; i++)
			{
				this.letters[i].text = this.text;
			}
			this.cooldown = this.updatefrequency;
		}
	}

	// Token: 0x060001BB RID: 443 RVA: 0x00014D8C File Offset: 0x00012F8C
	private void UpdateLetters()
	{
		if (this.letters == null)
		{
			this.letters = new TextMesh[this.dropshadow ? 2 : 1];
			for (int i = 0; i < this.letters.Length; i++)
			{
				this.letters[i] = new GameObject().AddComponent<TextMesh>();
				MainManager.SetFont(this.letters[i], this.fontindex);
				this.letters[i].transform.parent = base.transform;
				this.letters[i].anchor = TextAnchor.LowerLeft;
				if (!this.tridimentional)
				{
					this.letters[i].gameObject.layer = this.layer;
				}
				if (this.triui)
				{
					this.letters[i].gameObject.layer = 15;
				}
				this.letters[i].tag = "Text";
				this.letters[i].transform.localEulerAngles = Vector3.zero;
				this.letters[i].transform.localPosition = Vector3.zero + ((i == 1) ? this.dropoffset : Vector3.zero);
				this.letters[i].color = ((i == 1) ? new Color(0f, 0f, 0f, 0.5f) : this.fontcolor);
				MeshRenderer component = this.letters[i].GetComponent<MeshRenderer>();
				component.sortingOrder = this.sort + ((i == 1) ? 0 : 1);
				component.material.color = this.letters[i].color;
				this.letters[i].transform.localScale = new Vector3(this.size.x, this.size.y, 1f) * 0.07f;
			}
		}
	}

	// Token: 0x04000165 RID: 357
	public bool center;

	// Token: 0x04000166 RID: 358
	public bool monospace;

	// Token: 0x04000167 RID: 359
	public bool dropshadow;

	// Token: 0x04000168 RID: 360
	public bool tridimentional;

	// Token: 0x04000169 RID: 361
	public bool triui;

	// Token: 0x0400016A RID: 362
	public string text;

	// Token: 0x0400016B RID: 363
	public float updatefrequency = 20f;

	// Token: 0x0400016C RID: 364
	public int fontindex = 1;

	// Token: 0x0400016D RID: 365
	public int sort;

	// Token: 0x0400016E RID: 366
	public int layer = 5;

	// Token: 0x0400016F RID: 367
	public Vector2 size;

	// Token: 0x04000170 RID: 368
	public Vector2 dropoffset = new Vector2(0.1f, -0.1f);

	// Token: 0x04000171 RID: 369
	private Vector2? smallersize;

	// Token: 0x04000172 RID: 370
	private int oldlenght = -1;

	// Token: 0x04000173 RID: 371
	private float offset;

	// Token: 0x04000174 RID: 372
	private float maxsize;

	// Token: 0x04000175 RID: 373
	private float cooldown;

	// Token: 0x04000176 RID: 374
	private TextMesh[] letters;

	// Token: 0x04000177 RID: 375
	private Renderer[] renders;

	// Token: 0x04000178 RID: 376
	private static float[] monooffset = new float[]
	{
		0.35f,
		0.35f,
		0.35f
	};

	// Token: 0x04000179 RID: 377
	public Vector3 startpos = new Vector3(0f, 0f, 10f);

	// Token: 0x0400017A RID: 378
	public Color fontcolor;
}
