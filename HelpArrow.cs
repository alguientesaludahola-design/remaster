using System;
using UnityEngine;

// Token: 0x02000030 RID: 48
public class HelpArrow : MonoBehaviour
{
	// Token: 0x06000411 RID: 1041 RVA: 0x0002A010 File Offset: 0x00028210
	public static HelpArrow NewArrow(Transform parent, Vector3 offset, Color color, float distance, float size)
	{
		HelpArrow helpArrow = new GameObject("Arrow").AddComponent<HelpArrow>();
		helpArrow.distance = distance;
		helpArrow.color = color;
		helpArrow.transform.parent = parent;
		helpArrow.transform.localPosition = offset;
		helpArrow.size = size;
		return helpArrow;
	}

	// Token: 0x06000412 RID: 1042 RVA: 0x0002A050 File Offset: 0x00028250
	private void Start()
	{
		this.arrow = base.gameObject.AddComponent<SpriteRenderer>();
		this.arrow.sprite = MainManager.guisprites[196];
		this.arrow.material = MainManager.spritedefaultunity;
		this.arrow.gameObject.layer = 15;
		this.arrow.transform.localScale = new Vector3(0.45f, 0.6f, 1f) * this.size;
		this.arrow.enabled = false;
	}

	// Token: 0x06000413 RID: 1043 RVA: 0x0002A0E4 File Offset: 0x000282E4
	private void Update()
	{
		if (MainManager.player == null || this.lockarrow || !MainManager.FreePlayer())
		{
			this.playerinrange = false;
		}
		else if (Time.frameCount % 3 == 0)
		{
			this.playerinrange = (Vector3.Distance(MainManager.player.transform.position, base.transform.position) < this.distance);
		}
		if (MainManager.player != null && this.playerinrange && MainManager.player.entity.animid == 1 && MainManager.player.transform.position.y < this.arrow.transform.position.y)
		{
			MainManager.LookAt(this.arrow.transform, MainManager.player.transform.position);
			this.arrow.transform.eulerAngles += HelpArrow.arrowfix;
			this.arrow.color = Color.Lerp(Color.white, this.color, Mathf.Abs(Mathf.Sin(Time.time * 5f)));
			this.arrow.enabled = true;
			return;
		}
		this.arrow.enabled = false;
	}

	// Token: 0x040003B2 RID: 946
	private SpriteRenderer arrow;

	// Token: 0x040003B3 RID: 947
	private Color color;

	// Token: 0x040003B4 RID: 948
	private static readonly Vector3 arrowfix = new Vector3(90f, 180f);

	// Token: 0x040003B5 RID: 949
	private bool playerinrange;

	// Token: 0x040003B6 RID: 950
	public bool lockarrow;

	// Token: 0x040003B7 RID: 951
	private float distance;

	// Token: 0x040003B8 RID: 952
	private float size;
}
