using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200005C RID: 92
public class WackaWorm : MonoBehaviour
{
	// Token: 0x060007A0 RID: 1952 RVA: 0x00069B08 File Offset: 0x00067D08
	public void DestroyThis()
	{
		for (int i = 0; i < this.dfonts.Length; i++)
		{
			Object.Destroy(this.dfonts[i].gameObject);
		}
		for (int j = 0; j < this.worms.Length; j++)
		{
			Object.Destroy(this.worms[j].arrow.gameObject);
		}
		WackaWorm.disablehold = false;
		Object.Destroy(this.clock.gameObject);
		Object.Destroy(base.gameObject);
	}

	// Token: 0x060007A1 RID: 1953 RVA: 0x00069B89 File Offset: 0x00067D89
	public static void New(int totaltime, int wormammount, int frequency, int endevent, float maxradius, Vector3 pos)
	{
		new GameObject("wackacontrol").AddComponent<WackaWorm>().StartUp(totaltime, wormammount, frequency, endevent, maxradius, pos);
	}

	// Token: 0x060007A2 RID: 1954 RVA: 0x00069BA8 File Offset: 0x00067DA8
	public void StartUp(int totaltime, int wormammount, int frequency, int endevent, float maxradius, Vector3 pos)
	{
		base.transform.position = pos;
		this.start = true;
		WackaWorm.disablehold = true;
		this.main = true;
		this.timer = totaltime;
		this.wfreq = frequency;
		this.radius = maxradius;
		this.eventtocall = endevent;
		MainManager.instance.camoffset = new Vector3(0f, 2.5f, -12f);
		MainManager.instance.camangleoffset = new Vector3(20f, 0f);
		MainManager.player.canpause = false;
		MainManager.instance.flagvar[1] = 0;
		this.dfonts = new DynamicFont[2];
		for (int i = 0; i < this.dfonts.Length; i++)
		{
			this.dfonts[i] = DynamicFont.SetUp(true, 20f, 2, 100, Vector2.one * 2f, MainManager.GUICamera.transform, new Vector3((i == 0) ? -7f : 6.5f, 3.5f, 10f));
			this.dfonts[i].dropshadow = true;
		}
		this.clock = MainManager.NewUIObject("clock", MainManager.GUICamera.transform, new Vector3(-8f, 4.1f, 10f), Vector3.one, MainManager.guisprites[84], 100).transform;
		this.clock.gameObject.AddComponent<SpriteBounce>().MessageBounce();
		this.CreateWorms(wormammount);
		this.positions = new List<Vector3>();
	}

	// Token: 0x060007A3 RID: 1955 RVA: 0x00069D28 File Offset: 0x00067F28
	private void CreateWorms(int qtd)
	{
		this.worms = new WackaWorm.WormData[qtd];
		for (int i = 0; i < qtd; i++)
		{
			this.worms[i].anim = (Object.Instantiate(Resources.Load("Prefabs/Objects/Worm"), WackaWorm.offscreen, Quaternion.identity) as GameObject).GetComponent<Animator>();
			this.worms[i].time = (float)Random.Range(10, this.wfreq / 2);
			this.worms[i].controller = this.worms[i].anim.gameObject.AddComponent<WackaWorm>();
			this.worms[i].controller.eventtocall = i;
			this.worms[i].controller.parent = this;
			this.worms[i].controller.wfreq = this.wfreq;
			this.worms[i].controller.radius = 4f;
			this.worms[i].controller.transform.parent = base.transform;
			this.worms[i].controller.box = this.worms[i].anim.GetComponent<BoxCollider>();
			this.worms[i].arrow = MainManager.NewUIObject("arrow", MainManager.GUICamera.transform, Vector3.zero, new Vector3(0.2f, 1.5f, 1f), MainManager.guisprites[3], -99).GetComponent<SpriteRenderer>();
			this.sprite = base.GetComponent<SpriteRenderer>();
			this.worms[i].digging = base.StartCoroutine(this.worms[i].controller.Dig(true));
		}
	}

	// Token: 0x060007A4 RID: 1956 RVA: 0x00069F0C File Offset: 0x0006810C
	private void ClockDown()
	{
		this.timer--;
		if (this.timer <= -1)
		{
			this.start = false;
			MainManager.events.StartEvent(this.eventtocall, null);
		}
		this.clocktime = 60f;
	}

	// Token: 0x060007A5 RID: 1957 RVA: 0x00069F48 File Offset: 0x00068148
	private void Update()
	{
		if (!this.main)
		{
			if (!this.start)
			{
				if (this.parent != null)
				{
					Vector3? vector = this.FindPos(true);
					base.transform.position = vector.Value;
					this.start = true;
					return;
				}
			}
			else
			{
				if (this.sprite != null)
				{
					this.sprite.sortingOrder = (int)(MainManager.MainCamera.WorldToViewportPoint(base.transform.position).z * 1000f);
				}
				if (this.box == null)
				{
					base.gameObject.AddComponent<BoxCollider>();
					this.box.size = new Vector3(1f, 2f, 1f);
					this.box.center = new Vector3(0f, 1f);
				}
				this.box.enabled = this.parent.worms[this.eventtocall].above;
				if (MainManager.player.beemerang == null)
				{
					this.timer--;
				}
				if (this.timer <= 0)
				{
					if (this.parent.worms[this.eventtocall].above && (this.parent.worms[this.eventtocall].time <= 0f || Vector3.Distance(base.transform.position, MainManager.player.transform.position) < this.radius))
					{
						this.parent.worms[this.eventtocall].above = false;
						this.parent.worms[this.eventtocall].digging = base.StartCoroutine(this.Dig(false));
						this.parent.worms[this.eventtocall].time = (float)Random.Range(this.wfreq / 2, this.wfreq);
					}
					this.timer = 2;
				}
				if (this.parent.worms[this.eventtocall].time > 0f)
				{
					WackaWorm.WormData[] array = this.parent.worms;
					int num = this.eventtocall;
					array[num].time = array[num].time - MainManager.framestep;
					return;
				}
			}
		}
		else
		{
			if (this.start)
			{
				if (this.clocktime > 0f)
				{
					this.clocktime -= MainManager.framestep;
				}
				else
				{
					this.ClockDown();
				}
			}
			if (this.dfonts != null)
			{
				this.dfonts[0].text = ":" + this.timer.ToString().PadLeft(2, '0');
				this.dfonts[1].text = MainManager.instance.flagvar[1].ToString().PadLeft(3, '0');
			}
			for (int i = 0; i < this.worms.Length; i++)
			{
				if (!this.worms[i].above)
				{
					this.worms[i].arrow.enabled = false;
					if (this.worms[i].time <= 0f)
					{
						Vector3? vector2 = this.worms[i].controller.FindPos(false);
						if (vector2 != null)
						{
							this.worms[i].above = true;
							this.worms[i].time = (float)Random.Range(this.wfreq / 2, this.wfreq);
							this.worms[i].anim.transform.position = vector2.Value;
							this.worms[i].digging = base.StartCoroutine(this.worms[i].controller.Dig(true));
						}
					}
					else if (!this.worms[i].controller.hurt && this.worms[i].digging == null)
					{
						this.worms[i].anim.transform.position = WackaWorm.offscreen;
					}
				}
				else if (MainManager.GUICamera.WorldToViewportPoint(new Vector3(this.worms[i].anim.transform.position.x, 0f, this.worms[i].anim.transform.position.z)).y < 0.065f && this.worms[i].anim.transform.position.y > -0.65f)
				{
					this.worms[i].arrow.enabled = true;
					this.worms[i].arrow.transform.position = this.worms[i].anim.transform.position;
					this.worms[i].arrow.transform.localPosition = new Vector3(Mathf.Clamp(this.worms[i].arrow.transform.localPosition.x, -8.25f, 8.25f), -4.7f, 1f);
					this.worms[i].arrow.color = Color.Lerp(Color.white, Color.red, Mathf.Abs(Mathf.Sin(Time.time * 5f)));
				}
				else
				{
					this.worms[i].arrow.enabled = false;
				}
			}
		}
	}

	// Token: 0x060007A6 RID: 1958 RVA: 0x0006A520 File Offset: 0x00068720
	private Vector3? FindPos(bool continuous)
	{
		Vector3 zero = Vector3.zero;
		Vector3 end = this.parent.transform.position + new Vector3(0f, 3f);
		Vector3 vector = Vector3.zero;
		if (this.parent == null)
		{
			this.parent = base.transform.parent.GetComponent<WackaWorm>();
		}
		vector = MainManager.LimitRadius(this.parent.transform.position + new Vector3(Random.Range(-this.parent.radius, this.parent.radius), -5f, Random.Range(-this.parent.radius, this.parent.radius)), this.parent.transform.position, this.parent.radius);
		zero = new Vector3(vector.x, 5f, vector.z);
		RaycastHit raycastHit;
		Physics.Linecast(zero, end, out raycastHit, 8448);
		if (MainManager.GetDistance(vector, MainManager.player.transform.position, true) >= 5f && !(raycastHit.transform != null) && Physics.OverlapSphere(zero, 2f, 8448).Length == 0)
		{
			return new Vector3?(vector);
		}
		if (continuous)
		{
			return this.FindPos(true);
		}
		return null;
	}

	// Token: 0x060007A7 RID: 1959 RVA: 0x0006A67C File Offset: 0x0006887C
	private bool NearAnother(Vector3 pos)
	{
		if (this.parent == null || this.parent.worms == null || this.parent.worms.Length == 0)
		{
			return false;
		}
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < this.parent.worms.Length; i++)
		{
			if (i != this.eventtocall && this.worms[i].anim != null)
			{
				list.Add(this.worms[i].anim.transform.position);
			}
		}
		Vector3[] array = list.ToArray();
		if (array.Length == 0)
		{
			return false;
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (j != this.eventtocall && MainManager.GetSqrDistance(array[j], pos) < 5f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060007A8 RID: 1960 RVA: 0x0006A750 File Offset: 0x00068950
	private IEnumerator Dig(bool digabove)
	{
		float a = 60f;
		if (this.hurt)
		{
			do
			{
				base.transform.Rotate(Vector3.up * MainManager.TieFramerate(20f));
				a -= MainManager.TieFramerate(1f);
				yield return null;
			}
			while (a > 0f);
		}
		this.hurt = false;
		Vector3 p = base.transform.position;
		Vector3 tp = new Vector3(p.x, (float)(digabove ? 0 : -5), p.z);
		this.parent.worms[this.eventtocall].anim.Play("Sprout");
		a = 0f;
		do
		{
			base.transform.position = Vector3.Lerp(p, tp, a / 30f);
			base.transform.Rotate(Vector3.up * MainManager.TieFramerate(15f));
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < 30f);
		this.parent.worms[this.eventtocall].anim.Play("Idle");
		this.parent.worms[this.eventtocall].anim.gameObject.GetComponent<SpriteRenderer>().flipX = Convert.ToBoolean(Random.Range(0, 2));
		this.parent.worms[this.eventtocall].anim.transform.localEulerAngles = Vector3.zero;
		this.parent.worms[this.eventtocall].digging = null;
		yield return null;
		yield break;
	}

	// Token: 0x060007A9 RID: 1961 RVA: 0x0006A768 File Offset: 0x00068968
	private void OnTriggerEnter(Collider other)
	{
		if (!this.main && MainManager.player != null && MainManager.player.beemerang != null && other.transform == MainManager.player.beemerang.transform)
		{
			MainManager.HurtParticle(base.transform.position + Vector3.up, false);
			MainManager.PlaySound("WoodHit");
			this.parent.worms[this.eventtocall].anim.Play("Hurt");
			this.parent.worms[this.eventtocall].above = false;
			MainManager.instance.flagvar[1]++;
			this.hurt = true;
			this.parent.StartCoroutine(this.Dig(false));
		}
	}

	// Token: 0x040007EC RID: 2028
	private int timer;

	// Token: 0x040007ED RID: 2029
	private int eventtocall;

	// Token: 0x040007EE RID: 2030
	private int wfreq;

	// Token: 0x040007EF RID: 2031
	public bool start;

	// Token: 0x040007F0 RID: 2032
	public bool main;

	// Token: 0x040007F1 RID: 2033
	private bool hurt;

	// Token: 0x040007F2 RID: 2034
	private float radius;

	// Token: 0x040007F3 RID: 2035
	private float clocktime;

	// Token: 0x040007F4 RID: 2036
	private BoxCollider box;

	// Token: 0x040007F5 RID: 2037
	private WackaWorm parent;

	// Token: 0x040007F6 RID: 2038
	private DynamicFont[] dfonts;

	// Token: 0x040007F7 RID: 2039
	private List<Vector3> positions;

	// Token: 0x040007F8 RID: 2040
	public static bool disablehold;

	// Token: 0x040007F9 RID: 2041
	private static readonly Vector3 offscreen = new Vector3(0f, -99f);

	// Token: 0x040007FA RID: 2042
	private Transform clock;

	// Token: 0x040007FB RID: 2043
	private SpriteRenderer sprite;

	// Token: 0x040007FC RID: 2044
	private WackaWorm.WormData[] worms;

	// Token: 0x040007FD RID: 2045
	private const float hiderange = 4f;

	// Token: 0x040007FE RID: 2046
	private const float sqrdistance = 5f;

	// Token: 0x040007FF RID: 2047
	private const int maxpositions = 30;

	// Token: 0x02000281 RID: 641
	private struct WormData
	{
		// Token: 0x04002128 RID: 8488
		public Animator anim;

		// Token: 0x04002129 RID: 8489
		public float time;

		// Token: 0x0400212A RID: 8490
		public bool above;

		// Token: 0x0400212B RID: 8491
		public WackaWorm controller;

		// Token: 0x0400212C RID: 8492
		public Coroutine digging;

		// Token: 0x0400212D RID: 8493
		public SpriteRenderer arrow;
	}
}
