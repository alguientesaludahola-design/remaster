using System;
using UnityEngine;

// Token: 0x02000027 RID: 39
public class FontEffects : MonoBehaviour
{
	// Token: 0x060003C3 RID: 963 RVA: 0x00027680 File Offset: 0x00025880
	public void SetEffects(bool s, bool w, bool r, bool g, bool f, int id, int i)
	{
		this.wavy = w;
		this.shaky = s;
		this.rainbow = r;
		this.glitchy = g;
		this.fontid = id;
		this.variant = i;
		this.fadein = f;
		this.superglitch = false;
	}

	// Token: 0x060003C4 RID: 964 RVA: 0x000276C0 File Offset: 0x000258C0
	private void Start()
	{
		this.transform = base.gameObject.transform;
		this.render = base.GetComponent<TextMesh>();
		this.r = base.GetComponent<Renderer>();
		this.originalsprite = ((this.render != null) ? (this.render.text[0].ToString() ?? "") : "");
		this.startpos = this.transform.localPosition;
		if (this.fadein)
		{
			this.tcolor = this.r.material.color;
			this.r.material.color = Color.clear;
		}
	}

	// Token: 0x060003C5 RID: 965 RVA: 0x00027778 File Offset: 0x00025978
	private void Update()
	{
		if (this.rotate)
		{
			if (this.cooldown <= 0f)
			{
				this.r.transform.eulerAngles = new Vector3(0f, 0f, (float)(90 * Random.Range(0, 4)));
				this.cooldown = 3f;
			}
			else
			{
				this.cooldown -= MainManager.framestep;
			}
		}
		if (this.wavy)
		{
			this.transform.localPosition = this.startpos + new Vector3(-Mathf.Cos((Time.time + (float)this.variant / 10f) * 10f) / 30f, Mathf.Sin((Time.time + (float)this.variant / 10f) * 10f) / 30f);
		}
		if (this.shaky)
		{
			this.transform.localPosition = new Vector3(this.startpos.x + Random.Range(-0.025f, 0.025f), this.startpos.y + Random.Range(-0.025f, 0.025f), this.startpos.z);
		}
		if (this.rainbow)
		{
			this.r.material.color = MainManager.RainbowColor(this.variant);
			if (this.render != null)
			{
				this.render.color = this.r.material.color;
			}
		}
		if (this.fadein)
		{
			this.r.material.color = Color.Lerp(this.r.material.color, this.tcolor, MainManager.TieFramerate(0.01f));
		}
		if (this.glitchy && this.fontid >= 0)
		{
			if (this.changed)
			{
				if (Random.Range(0, 10) > 8)
				{
					this.render.text = this.originalsprite;
					this.changed = false;
					return;
				}
			}
			else if (Random.Range(0f, 1000f) >= (this.superglitch ? 800f : 998.75f))
			{
				this.render.text = (FontEffects.letters[Random.Range(0, FontEffects.letters.Length)].ToString() ?? "");
				this.changed = true;
			}
		}
	}

	// Token: 0x04000342 RID: 834
	public bool wavy;

	// Token: 0x04000343 RID: 835
	public bool shaky;

	// Token: 0x04000344 RID: 836
	public bool rainbow;

	// Token: 0x04000345 RID: 837
	public bool glitchy;

	// Token: 0x04000346 RID: 838
	public bool changed;

	// Token: 0x04000347 RID: 839
	public bool fadein;

	// Token: 0x04000348 RID: 840
	public bool rotate;

	// Token: 0x04000349 RID: 841
	public bool superglitch;

	// Token: 0x0400034A RID: 842
	private int fontid;

	// Token: 0x0400034B RID: 843
	private int variant;

	// Token: 0x0400034C RID: 844
	private int color;

	// Token: 0x0400034D RID: 845
	private static readonly char[] letters = new char[]
	{
		'@',
		'?',
		'!',
		'#',
		'*',
		'+',
		'-',
		'$',
		'&'
	};

	// Token: 0x0400034E RID: 846
	private new Transform transform;

	// Token: 0x0400034F RID: 847
	private float cooldown;

	// Token: 0x04000350 RID: 848
	private Color tcolor;

	// Token: 0x04000351 RID: 849
	private TextMesh render;

	// Token: 0x04000352 RID: 850
	private TextMesh waveobj;

	// Token: 0x04000353 RID: 851
	private string originalsprite;

	// Token: 0x04000354 RID: 852
	private Vector3 startpos;

	// Token: 0x04000355 RID: 853
	private Renderer r;

	// Token: 0x04000356 RID: 854
	private const float shakeintensity = 0.025f;

	// Token: 0x04000357 RID: 855
	private const float colorspeed = 0.05f;

	// Token: 0x04000358 RID: 856
	private const float multiplier = 10f;

	// Token: 0x04000359 RID: 857
	private const float limiter = 30f;
}
