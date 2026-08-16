using System;
using UnityEngine;

// Token: 0x02000004 RID: 4
public class AnimationFunctions : MonoBehaviour
{
	// Token: 0x06000003 RID: 3 RVA: 0x00002058 File Offset: 0x00000258
	private void Start()
	{
		if (this.getentity && this.model)
		{
			this.entity = base.transform.parent.parent.parent.GetComponent<EntityControl>();
		}
	}

	// Token: 0x06000004 RID: 4 RVA: 0x0000208A File Offset: 0x0000028A
	public void PlayChildParticle()
	{
		if (this.part == null)
		{
			this.part = base.GetComponentInChildren<ParticleSystem>();
		}
		if (this.part != null)
		{
			this.part.Play();
		}
	}

	// Token: 0x06000005 RID: 5 RVA: 0x000020BF File Offset: 0x000002BF
	public void SetUp(EntityControl parent)
	{
		this.entity = parent;
		this.getentity = false;
		this.model = parent.model;
	}

	// Token: 0x06000006 RID: 6 RVA: 0x000020E0 File Offset: 0x000002E0
	public void SetUp(bool findentity, bool ismodel)
	{
		this.getentity = findentity;
		this.model = ismodel;
	}

	// Token: 0x06000007 RID: 7 RVA: 0x000020F0 File Offset: 0x000002F0
	public void Shrink(float frametime)
	{
		base.StartCoroutine(MainManager.Shrink(base.transform, frametime, false));
	}

	// Token: 0x06000008 RID: 8 RVA: 0x00002106 File Offset: 0x00000306
	public void ShrinkDelete(float frametime)
	{
		base.StartCoroutine(MainManager.Shrink(base.transform, frametime, true));
	}

	// Token: 0x06000009 RID: 9 RVA: 0x0000211C File Offset: 0x0000031C
	public void WeakShakeScreen(float time)
	{
		MainManager.ShakeScreen(Vector3.one * 0.1f, time);
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00002133 File Offset: 0x00000333
	public void MidShakeScreen(float time)
	{
		MainManager.ShakeScreen(Vector3.one * 0.25f, time);
	}

	// Token: 0x0600000B RID: 11 RVA: 0x0000214A File Offset: 0x0000034A
	public void StrongShakeScreen(float time)
	{
		MainManager.ShakeScreen(Vector3.one * 0.5f, time);
	}

	// Token: 0x0600000C RID: 12 RVA: 0x00002161 File Offset: 0x00000361
	public void ExtraAnims(string anims)
	{
		if (this.entity != null && !this.entity.overrideanimfunc)
		{
			this.entity.ExtraAnimPlay(anims);
		}
	}

	// Token: 0x0600000D RID: 13 RVA: 0x0000218C File Offset: 0x0000038C
	public void CustomShakeScreen(string args)
	{
		string[] array = args.Split(new char[]
		{
			','
		});
		MainManager.ShakeScreen(Vector3.one * Convert.ToSingle(array[0]), Convert.ToSingle(array[1]));
	}

	// Token: 0x0600000E RID: 14 RVA: 0x000021CC File Offset: 0x000003CC
	public void PlayerAnim(int id)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].entity.animstate = id;
		}
	}

	// Token: 0x0600000F RID: 15 RVA: 0x0000220C File Offset: 0x0000040C
	public void PlayerAnimString(string id)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].entity.animstate = (int)Enum.Parse(typeof(MainManager.Animations), id);
		}
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00002260 File Offset: 0x00000460
	public void PlayerJump()
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].entity.Jump();
		}
	}

	// Token: 0x06000011 RID: 17 RVA: 0x000022A0 File Offset: 0x000004A0
	public void PlayerJumpAmmount(float ammount)
	{
		for (int i = 0; i < MainManager.instance.playerdata.Length; i++)
		{
			MainManager.instance.playerdata[i].entity.Jump(ammount);
		}
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000022DF File Offset: 0x000004DF
	public void ChangeAnim(float state)
	{
		if (this.entity != null)
		{
			this.entity.animstate = (int)state;
		}
	}

	// Token: 0x06000013 RID: 19 RVA: 0x000022FC File Offset: 0x000004FC
	public void StopHalt()
	{
		MainManager.halt = false;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x00002304 File Offset: 0x00000504
	public void Halt()
	{
		MainManager.halt = true;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x0000230C File Offset: 0x0000050C
	public void PlaySound(string sound)
	{
		if (this.entity != null)
		{
			this.entity.PlaySound(sound);
			return;
		}
		MainManager.PlaySound(sound);
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002330 File Offset: 0x00000530
	public void PlayUISound(string sound)
	{
		MainManager.PlaySound(sound);
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002339 File Offset: 0x00000539
	public void PlayParticleSimple(string particle)
	{
		MainManager.PlayParticle(particle, base.transform.position);
	}

	// Token: 0x06000018 RID: 24 RVA: 0x00002350 File Offset: 0x00000550
	public void PlayParticlePos(string ParticleXYZLocal)
	{
		string[] array = ParticleXYZLocal.Split(new char[]
		{
			','
		});
		Vector3 vector = new Vector3(Convert.ToSingle(array[1]), Convert.ToSingle(array[2]), Convert.ToSingle(array[3]));
		MainManager.PlayParticle(array[0], (array[4] == "true") ? (base.transform.position + vector) : vector);
	}

	// Token: 0x06000019 RID: 25 RVA: 0x000023BA File Offset: 0x000005BA
	public void Flip()
	{
		this.entity.FlipSimple();
	}

	// Token: 0x04000001 RID: 1
	private EntityControl entity;

	// Token: 0x04000002 RID: 2
	public bool getentity;

	// Token: 0x04000003 RID: 3
	public bool model;

	// Token: 0x04000004 RID: 4
	private ParticleSystem part;
}
