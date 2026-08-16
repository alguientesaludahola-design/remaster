using System;
using UnityEngine;

// Token: 0x02000040 RID: 64
public class MiniBubble : MonoBehaviour
{
	// Token: 0x06000698 RID: 1688 RVA: 0x0004A705 File Offset: 0x00048905
	public static MiniBubble SetUp(string text, EntityControl target, Vector3 pos, int sortingorder)
	{
		return MiniBubble.SetUp(text, target, pos, sortingorder, 1.5f);
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x0004A718 File Offset: 0x00048918
	public static MiniBubble SetUp(string text, EntityControl target, Vector3 pos, int sortingorder, float timer)
	{
		MiniBubble miniBubble = new GameObject("MiniBubble-" + text).AddComponent<MiniBubble>();
		miniBubble.basesort = sortingorder;
		miniBubble.text = string.Concat(new object[]
		{
			"|minibubble||sort,",
			miniBubble.basesort + 1,
			"|",
			text,
			"|fwait,",
			timer,
			"|"
		});
		miniBubble.pos = pos;
		miniBubble.target = target;
		return miniBubble;
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x0004A7A0 File Offset: 0x000489A0
	private void Start()
	{
		Color color = new Color(0.9f, 0.85f, 0.75f);
		SpriteRenderer component = MainManager.NewUIObject("Bubble", base.transform, default(Vector3), new Vector3(0.8f, 0.65f, 1f), MainManager.guisprites[72]).GetComponent<SpriteRenderer>();
		component.sortingOrder = this.basesort - 1;
		component.color = color;
		component = MainManager.NewUIObject("Tail", base.transform, default(Vector3), Vector3.one * 0.75f, MainManager.guisprites[73]).GetComponent<SpriteRenderer>();
		component.sortingOrder = this.basesort;
		component.color = color;
		this.tail = component.transform;
		component = MainManager.NewUIObject("Tailback", this.tail, default(Vector3), Vector3.one * 1.2f, MainManager.guisprites[73]).GetComponent<SpriteRenderer>();
		component.sortingOrder = this.basesort - 2;
		component.color = Color.black;
		base.transform.parent = MainManager.GUICamera.transform;
		base.transform.localPosition = this.pos;
		this.anim = base.gameObject.AddComponent<DialogueAnim>();
		this.anim.shrinkspeed = 0.3f;
		base.transform.localScale = Vector3.zero;
		base.StartCoroutine(MainManager.SetText(this.text, 0, new float?((MainManager.languageid > 0) ? 3.25f : 3.5f), false, false, new Vector3(-1.55f, 0.15f), Vector3.zero, new Vector2(0.75f, 0.75f), base.transform, null));
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x0004A968 File Offset: 0x00048B68
	private void LateUpdate()
	{
		base.transform.localEulerAngles = Vector3.zero;
		if (this.tail != null)
		{
			this.tail.gameObject.SetActive(this.target != null);
			if (this.target != null)
			{
				float num = Mathf.Clamp((MainManager.MainCamera.WorldToViewportPoint(this.tail.transform.position).x - MainManager.MainCamera.WorldToViewportPoint(this.target.transform.position).x) * -400f, -45f, 45f);
				this.tail.transform.localEulerAngles = new Vector3(0f, 0f, num);
				this.tail.transform.localPosition = new Vector3(0f, Mathf.Clamp(0.35f - Mathf.Abs(num) / 100f, 0f, 1f));
				this.target.talking = true;
			}
		}
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x0004AA7F File Offset: 0x00048C7F
	public void DestroyThis()
	{
		if (this.target != null)
		{
			this.target.talking = false;
		}
		this.anim.shrink = true;
		Object.Destroy(base.gameObject, 1f);
		Object.Destroy(this);
	}

	// Token: 0x040005F2 RID: 1522
	private string text;

	// Token: 0x040005F3 RID: 1523
	private Vector3 pos;

	// Token: 0x040005F4 RID: 1524
	public EntityControl target;

	// Token: 0x040005F5 RID: 1525
	private Transform tail;

	// Token: 0x040005F6 RID: 1526
	private DialogueAnim anim;

	// Token: 0x040005F7 RID: 1527
	private const float defaulttimer = 1.5f;

	// Token: 0x040005F8 RID: 1528
	private int basesort = 10;
}
