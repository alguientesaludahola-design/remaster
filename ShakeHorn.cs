using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000052 RID: 82
public class ShakeHorn : MonoBehaviour
{
	// Token: 0x06000762 RID: 1890 RVA: 0x000662B8 File Offset: 0x000644B8
	private void Start()
	{
		this.parts = base.GetComponentsInChildren<ParticleSystem>();
		this.starteuler = base.transform.eulerAngles;
		base.gameObject.isStatic = false;
		if (base.GetComponent<Rigidbody>() == null)
		{
			Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
			rigidbody.useGravity = false;
			rigidbody.isKinematic = true;
			rigidbody.constraints = RigidbodyConstraints.FreezePosition;
		}
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x0006631C File Offset: 0x0006451C
	private IEnumerator Shake()
	{
		if (this.sound != null)
		{
			MainManager.PlaySoundAt(this.sound.name, 1f, base.transform.position);
		}
		if (this.parts != null)
		{
			for (int i = 0; i < this.parts.Length; i++)
			{
				this.parts[i].Emit(this.emission);
			}
		}
		float a = 0f;
		float b = 60f;
		do
		{
			base.transform.eulerAngles = this.starteuler + this.axis * Mathf.Sin(Time.time * 30f) * (1f - a / b);
			a += MainManager.TieFramerate(1f);
			yield return null;
		}
		while (a < b + 1f);
		this.shake = null;
		yield break;
	}

	// Token: 0x06000764 RID: 1892 RVA: 0x0006632C File Offset: 0x0006452C
	private void OnTriggerEnter(Collider other)
	{
		bool flag = other.tag == "BeetleHorn" || other.tag == "BeetleDash";
		if ((this.playermoveshake && MainManager.player != null && other.transform == MainManager.player.transform) || flag)
		{
			if (flag)
			{
				MainManager.HitPart(other.transform.position + Vector3.up);
			}
			if (this.shake != null)
			{
				base.StopCoroutine(this.shake);
			}
			this.shake = base.StartCoroutine(this.Shake());
		}
	}

	// Token: 0x04000786 RID: 1926
	public bool playermoveshake;

	// Token: 0x04000787 RID: 1927
	public int emission = 15;

	// Token: 0x04000788 RID: 1928
	public Vector3 axis = new Vector3(0f, 2f);

	// Token: 0x04000789 RID: 1929
	private Coroutine shake;

	// Token: 0x0400078A RID: 1930
	private ParticleSystem[] parts;

	// Token: 0x0400078B RID: 1931
	public AudioClip sound;

	// Token: 0x0400078C RID: 1932
	private Vector3 starteuler;
}
