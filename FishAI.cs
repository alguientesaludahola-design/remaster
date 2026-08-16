using System;
using UnityEngine;

// Token: 0x0200001F RID: 31
public class FishAI : MonoBehaviour
{
	// Token: 0x06000384 RID: 900 RVA: 0x00023B68 File Offset: 0x00021D68
	private void CreatePos()
	{
		this.currentPos = 0;
		this.pos = new Vector3[this.segments.Length * 30];
		for (int i = 0; i < this.pos.Length; i++)
		{
			this.pos[i] = this.transform.position;
		}
	}

	// Token: 0x06000385 RID: 901 RVA: 0x00023BBC File Offset: 0x00021DBC
	private void Start()
	{
		this.transform = base.gameObject.transform;
		this.CreatePos();
		this.flip = (this.transform.position.x < FishingMain.instance.hook.transform.position.x);
		this.startY = this.transform.localPosition.y;
		this.spdMult = Random.Range(0.8f, 1.1f);
		this.scale = Random.Range(0.5f, 1.5f);
		this.speed = FishingMain.instance.fishData[this.id].speed * Random.Range(0.8f, 1.1f);
		for (int i = 0; i < this.segments.Length; i++)
		{
			this.segments[i].sortingOrder = -i;
		}
	}

	// Token: 0x06000386 RID: 902 RVA: 0x00023CA4 File Offset: 0x00021EA4
	private void Update()
	{
		if (FishingMain.instance != null && (FishingMain.instance.state == FishingMain.State.Pause || FishingMain.instance.routine != null))
		{
			if (this.state == FishAI.State.Fleeing)
			{
				this.DoFlee();
				this.UpdateScale(false);
				return;
			}
		}
		else
		{
			if (this.state == FishAI.State.Fleeing)
			{
				this.DoFlee();
			}
			else
			{
				bool flag = Mathf.Abs(FishingMain.instance.hook.transform.position.y - this.transform.position.y) > 12.5f || this.bounce > 5;
				this.liveTime += MainManager.framestep;
				if (this.liveTime > 600f && flag)
				{
					this.Kill();
					return;
				}
				this.UpdatePos(false);
				this.DoAI();
			}
			this.UpdateScale(false);
		}
	}

	// Token: 0x06000387 RID: 903 RVA: 0x00023D84 File Offset: 0x00021F84
	public void UpdateScale(bool instant = false)
	{
		this.transform.localScale = new Vector3(Mathf.Lerp(this.transform.localScale.x, (this.flip ? (-this.scale) : this.scale) * FishingMain.instance.fishData[this.id].size, instant ? 1f : (MainManager.framestep * 0.2f)), this.scale * FishingMain.instance.fishData[this.id].size, 1f);
	}

	// Token: 0x06000388 RID: 904 RVA: 0x00023E23 File Offset: 0x00022023
	private void Kill()
	{
		FishingMain.instance.fishes.Remove(this);
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000389 RID: 905 RVA: 0x00023E44 File Offset: 0x00022044
	private void DoAI()
	{
		if (this.actionTime > 0f)
		{
			this.actionTime -= MainManager.framestep;
		}
		switch (this.state)
		{
		case FishAI.State.Idle:
			this.UpdateDistance();
			if (this.liveTime > 60f && this.segments[0].isVisible && this.distance < FishingMain.instance.fishData[this.id].radius / 3f && ((this.flip && FishingMain.instance.hook.transform.position.x > this.transform.position.x) || (!this.flip && FishingMain.instance.hook.transform.position.x < this.transform.position.x)) && Mathf.Abs(FishingMain.instance.baitIcon.transform.position.y - this.transform.position.y) < 0.5f)
			{
				FishingMain.instance.noticed = true;
				this.state = FishAI.State.Checking;
				this.actionTime = (float)Random.Range(60, 180);
				this.startY = FishingMain.instance.depth;
				return;
			}
			if (Mathf.Abs(FishingMain.instance.hook.transform.position.x - this.transform.position.x) > 15f && this.actionTime <= 0f)
			{
				this.flip = !this.flip;
				this.bounce++;
				this.actionTime = 60f;
				return;
			}
			this.transform.localPosition = new Vector3(this.transform.localPosition.x + MainManager.framestep * (float)(this.flip ? 1 : -1) * this.speed, this.startY + Mathf.Sin(Time.time * 1f * this.spdMult) * 1.5f, -0.1f);
			return;
		case FishAI.State.Nibbling:
		{
			this.UpdateDistance();
			Vector3 position = FishingMain.instance.baitIcon.transform.position;
			this.LookAt(position);
			if (Mathf.Abs(this.startY - FishingMain.instance.depth) > 1.25f)
			{
				this.state = FishAI.State.Fleeing;
				FishingMain.instance.noticed = false;
				return;
			}
			if (this.distance >= 0.25f)
			{
				Vector2 vector = this.transform.position;
				Vector2 vector2 = FishingMain.instance.baitIcon.transform.position;
				Vector2 v = MainManager.GetDirection(vector, vector2);
				this.transform.position += Mathf.Clamp01(this.distance) * this.speed * -1.5f * v;
				this.FixZ();
				return;
			}
			FishingMain.instance.shake = 45f;
			this.nibbleTimes++;
			if (this.nibbleTimes == 3 || Random.Range(0, 100) < ((FishingMain.instance.bait != MainManager.Items.None) ? 40 : 30))
			{
				FishingMain.instance.Particle("HitPart", false);
				MainManager.PlaySound("Hit3");
				MainManager.PlaySound("Bite2");
				FishingMain.instance.routine = FishingMain.instance.StartCoroutine(FishingMain.HookUp(this));
				return;
			}
			MainManager.PlaySound("Ping2", 1f + (float)(this.nibbleTimes - 1) * 0.1f, 1f);
			this.actionTime = (float)Random.Range(40, 100);
			if (FishingMain.instance.bait != MainManager.Items.None)
			{
				this.actionTime /= 2f;
			}
			this.state = FishAI.State.Checking;
			this.segments[0].sprite = this.headSprites[0];
			return;
		}
		case FishAI.State.Checking:
		{
			this.UpdateDistance();
			Vector3 position = FishingMain.instance.baitIcon.transform.position;
			this.LookAt(position);
			if (Mathf.Abs(this.startY - FishingMain.instance.depth) > 1.25f)
			{
				this.state = FishAI.State.Fleeing;
				FishingMain.instance.noticed = false;
				return;
			}
			if (this.actionTime <= 0f && this.distance > 2.3f)
			{
				this.state = FishAI.State.Nibbling;
				this.segments[0].sprite = this.headSprites[1];
				return;
			}
			Vector2 vector = this.transform.position;
			Vector2 vector2 = FishingMain.instance.baitIcon.transform.position;
			Vector2 v2 = MainManager.GetDirection(vector, vector2);
			this.transform.position += this.speed / 2f * v2;
			this.FixZ();
			return;
		}
		case FishAI.State.Fleeing:
			break;
		case FishAI.State.Reeling:
			if (this.dizzyTime > 0f)
			{
				this.dizzyTime -= MainManager.framestep;
			}
			if (this.dizzyTime <= 0f && (this.direction == -1 || this.actionTime <= 0f) && Time.frameCount % 3 == 0 && Random.Range(0, 100) < 10)
			{
				this.direction = Random.Range(1, 3);
				this.actionTime = (float)Random.Range(40, 140);
			}
			if (FishingMain.instance.depth < this.startY)
			{
				this.startY = FishingMain.instance.depth;
			}
			break;
		default:
			return;
		}
	}

	// Token: 0x0600038A RID: 906 RVA: 0x000243EC File Offset: 0x000225EC
	private void FixZ()
	{
		this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, FishingMain.instance.hook.transform.localPosition.z);
	}

	// Token: 0x0600038B RID: 907 RVA: 0x00024442 File Offset: 0x00022642
	private void UpdateDistance()
	{
		if (Time.frameCount % 2 == 0)
		{
			this.distance = Vector2.Distance(this.transform.position, FishingMain.instance.baitIcon.transform.position);
		}
	}

	// Token: 0x0600038C RID: 908 RVA: 0x00024484 File Offset: 0x00022684
	private void DoFlee()
	{
		this.transform.localEulerAngles = Vector3.zero;
		this.segments[0].sprite = this.headSprites[0];
		this.UpdateDistance();
		if (this.distance > 15f)
		{
			this.Kill();
			return;
		}
		Vector2 vector = new Vector2(this.transform.position.x, 0f);
		Vector2 vector2 = new Vector2(FishingMain.instance.hook.transform.position.x, 0f);
		Vector3 a = MainManager.GetDirection(vector, vector2);
		this.flip = (FishingMain.instance.hook.transform.position.x < this.transform.position.x);
		this.transform.position += MainManager.framestep * this.speed * a * 3.5f;
		this.FixZ();
	}

	// Token: 0x0600038D RID: 909 RVA: 0x00024587 File Offset: 0x00022787
	public void LookAt(in Vector3 point)
	{
		this.flip = (point.x > this.transform.position.x);
	}

	// Token: 0x0600038E RID: 910 RVA: 0x000245A8 File Offset: 0x000227A8
	private void UpdatePos(bool force = false)
	{
		if (Time.frameCount % 2 == 0 && (force || MainManager.GetSqrDistance(this.transform.position, this.pos[(int)Mathf.Repeat((float)(this.currentPos - 1), (float)this.pos.Length)]) > 0.05f))
		{
			this.pos[this.currentPos] = this.transform.position;
			this.currentPos++;
			if (this.currentPos >= this.pos.Length)
			{
				this.currentPos = 0;
			}
		}
	}

	// Token: 0x0600038F RID: 911 RVA: 0x0002463C File Offset: 0x0002283C
	private void UpdateSegments()
	{
		Color color = (FishingMain.instance != null) ? Color.Lerp(Color.white, Color.black, Mathf.Lerp(0.45f, 1f, FishingMain.instance.depth / 100f)) : Color.white;
		if (this.extras != null && this.extras.Length != 0)
		{
			for (int i = 0; i < this.extras.Length; i++)
			{
				this.extras[i].color = color;
			}
		}
		for (int j = 0; j < this.segments.Length; j++)
		{
			if (this.segments[j].isVisible)
			{
				this.segments[j].color = color;
				if (j > 0)
				{
					float num = (float)((FishingMain.instance != null && FishingMain.instance.hooked == this) ? 5 : 1);
					if (this.segments.Length > 4)
					{
						float value = (float)(this.segments.Length - j) / (float)this.segments.Length * 2f;
						this.segments[j].transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Cos(((float)j + Time.time) * num * Mathf.Clamp(value, 0.25f, 1f)) * 4f * ((float)(j + 1) * 0.75f));
					}
					else
					{
						this.segments[j].transform.localEulerAngles = new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * Mathf.Clamp(5f - (float)j, 0.15f, 5f * num) * this.scale) * 25f));
					}
				}
			}
		}
	}

	// Token: 0x06000390 RID: 912 RVA: 0x000247F4 File Offset: 0x000229F4
	private void LateUpdate()
	{
		if (FishingMain.instance == null || FishingMain.instance.state != FishingMain.State.Pause)
		{
			this.UpdateSegments();
			if (this.state == FishAI.State.Reeling || this.state == FishAI.State.Tired)
			{
				this.transform.position = FishingMain.instance.baitIcon.transform.position;
				Vector3 position = FishingMain.instance.hook.transform.position;
				this.LookAt(position);
			}
		}
	}

	// Token: 0x040002AD RID: 685
	public FishingMain.FishIDs id;

	// Token: 0x040002AE RID: 686
	public SpriteRenderer[] segments;

	// Token: 0x040002AF RID: 687
	public SpriteRenderer[] extras;

	// Token: 0x040002B0 RID: 688
	public Sprite[] headSprites;

	// Token: 0x040002B1 RID: 689
	public new Transform transform;

	// Token: 0x040002B2 RID: 690
	public ParticleSystem bubbles;

	// Token: 0x040002B3 RID: 691
	public ParticleSystem extraPart;

	// Token: 0x040002B4 RID: 692
	[HideInInspector]
	public float distance = float.PositiveInfinity;

	// Token: 0x040002B5 RID: 693
	[HideInInspector]
	public float liveTime;

	// Token: 0x040002B6 RID: 694
	[HideInInspector]
	public float dizzyTime;

	// Token: 0x040002B7 RID: 695
	[HideInInspector]
	public float scale = 1f;

	// Token: 0x040002B8 RID: 696
	[HideInInspector]
	public float actionTime = 60f;

	// Token: 0x040002B9 RID: 697
	[HideInInspector]
	public float strMod = 1f;

	// Token: 0x040002BA RID: 698
	[HideInInspector]
	public float startY;

	// Token: 0x040002BB RID: 699
	[HideInInspector]
	public FishAI.State state;

	// Token: 0x040002BC RID: 700
	[HideInInspector]
	public bool flip;

	// Token: 0x040002BD RID: 701
	[HideInInspector]
	public int direction = -1;

	// Token: 0x040002BE RID: 702
	[HideInInspector]
	public GameObject effect;

	// Token: 0x040002BF RID: 703
	private Vector3[] pos;

	// Token: 0x040002C0 RID: 704
	private int currentPos;

	// Token: 0x040002C1 RID: 705
	private int nibbleTimes;

	// Token: 0x040002C2 RID: 706
	private int bounce;

	// Token: 0x040002C3 RID: 707
	private float spdMult;

	// Token: 0x040002C4 RID: 708
	private float speed;

	// Token: 0x040002C5 RID: 709
	private const float hookDistance = 1.25f;

	// Token: 0x020001DF RID: 479
	public enum State
	{
		// Token: 0x04001616 RID: 5654
		Idle,
		// Token: 0x04001617 RID: 5655
		Nibbling,
		// Token: 0x04001618 RID: 5656
		Checking,
		// Token: 0x04001619 RID: 5657
		Fleeing,
		// Token: 0x0400161A RID: 5658
		Reeling,
		// Token: 0x0400161B RID: 5659
		Tired
	}
}
