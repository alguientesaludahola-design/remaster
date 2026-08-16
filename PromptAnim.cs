using System;
using UnityEngine;

// Token: 0x0200004A RID: 74
public class PromptAnim : MonoBehaviour
{
	// Token: 0x06000743 RID: 1859 RVA: 0x00064BBA File Offset: 0x00062DBA
	public void SetUp(int selectid, bool underlines)
	{
		this.option = selectid;
		this.underline = underlines;
		if (MainManager.instance.letterprompt == 1)
		{
			this.underOffset = -0.2f;
		}
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x00064BE4 File Offset: 0x00062DE4
	private void Start()
	{
		this.letters = new PromptAnim.Letters[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			this.letters[i].letterSprite = base.transform.GetChild(i).GetComponent<TextMesh>();
			this.letters[i].lm = this.letters[i].letterSprite.GetComponent<MeshRenderer>();
			this.letters[i].transform = this.letters[i].letterSprite.transform;
			this.letters[i].letterpos = this.letters[i].transform.localPosition;
			if (this.underline)
			{
				this.letters[i].underscore = new GameObject().AddComponent<TextMesh>();
				MainManager.SetFont(this.letters[i].underscore, 0);
				this.letters[i].underscore.gameObject.layer = 5;
				this.letters[i].underscore.transform.parent = base.transform.GetChild(i);
				this.letters[i].underscore.transform.localPosition = new Vector3(-0.5f, -2f + this.underOffset);
				this.letters[i].underscore.transform.localEulerAngles = Vector3.zero;
				this.letters[i].underscore.anchor = TextAnchor.LowerLeft;
				this.letters[i].underscore.transform.localScale = new Vector3(Mathf.Clamp(MainManager.GetLetterOffset(this.letters[i].letterSprite.text[0], 0, 1f) * 3f, 0.75f, float.PositiveInfinity), 1f, 1f);
				this.letters[i].underscore.color = Color.red;
				this.letters[i].m = this.letters[i].underscore.GetComponent<MeshRenderer>();
				this.letters[i].m.sortingOrder = this.letters[i].letterSprite.GetComponent<MeshRenderer>().sortingOrder;
				this.letters[i].m.material.color = this.letters[i].underscore.color;
			}
		}
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x00064EB4 File Offset: 0x000630B4
	private void FixedUpdate()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			if (MainManager.instance.option == this.option)
			{
				this.letters[i].transform.localPosition = new Vector2(this.letters[i].letterpos.x, this.letters[i].letterpos.y + Mathf.Sin((Time.time + (float)i) * 5f) / 20f);
				if (this.underline)
				{
					this.letters[i].underscore.text = PromptAnim.utext[0];
				}
				else
				{
					this.letters[i].lm.material.color = Color.red;
				}
			}
			else
			{
				this.letters[i].letterSprite.transform.localPosition = this.letters[i].letterpos;
				if (this.underline)
				{
					this.letters[i].underscore.text = PromptAnim.utext[1];
				}
				else
				{
					this.letters[i].lm.material.color = Color.black;
				}
			}
			this.letters[i].letterSprite.color = this.letters[i].lm.material.color;
		}
	}

	// Token: 0x0400073D RID: 1853
	private int option;

	// Token: 0x0400073E RID: 1854
	private float underOffset;

	// Token: 0x0400073F RID: 1855
	private bool underline;

	// Token: 0x04000740 RID: 1856
	private PromptAnim.Letters[] letters;

	// Token: 0x04000741 RID: 1857
	private static readonly string[] utext = new string[]
	{
		"_",
		""
	};

	// Token: 0x02000277 RID: 631
	public struct Letters
	{
		// Token: 0x040020FA RID: 8442
		public TextMesh underscore;

		// Token: 0x040020FB RID: 8443
		public TextMesh letterSprite;

		// Token: 0x040020FC RID: 8444
		public Transform transform;

		// Token: 0x040020FD RID: 8445
		public MeshRenderer m;

		// Token: 0x040020FE RID: 8446
		public MeshRenderer lm;

		// Token: 0x040020FF RID: 8447
		public Vector2 letterpos;
	}
}
