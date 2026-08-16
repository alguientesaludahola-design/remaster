using System;
using UnityEngine;

// Token: 0x02000049 RID: 73
public class PrefabParticle : MonoBehaviour
{
	// Token: 0x0600073F RID: 1855 RVA: 0x000647FC File Offset: 0x000629FC
	private void Start()
	{
		if (MainManager.particlelevel == 0)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (MainManager.particlelevel == 1)
		{
			this.maxammount /= 2;
		}
		this.prefabs = new PrefabParticle.PrefabData[this.maxammount];
		for (int i = 0; i < this.prefabs.Length; i++)
		{
			this.prefabs[i].prefab = Object.Instantiate<GameObject>(this.prefabpart).GetComponent<MeshRenderer>();
			this.prefabs[i].prefab.transform.parent = base.transform;
			this.prefabs[i].lifetime = Random.Range(this.liveframes / 2f, this.liveframes);
			this.prefabs[i].prefab.transform.localScale = this.maxsize;
			this.SetRandom(i);
		}
	}

	// Token: 0x06000740 RID: 1856 RVA: 0x000648F0 File Offset: 0x00062AF0
	private void LateUpdate()
	{
		for (int i = 0; i < this.prefabs.Length; i++)
		{
			PrefabParticle.PrefabData[] array = this.prefabs;
			int num = i;
			array[num].lifetime = array[num].lifetime - 1f;
			if (this.childspin != Vector3.zero)
			{
				this.prefabs[i].prefab.transform.GetChild(0).Rotate(this.childspin);
			}
			this.prefabs[i].prefab.transform.position += this.prefabs[i].prefab.transform.forward * this.speed;
			if (this.prefabs[i].lifetime > 0f)
			{
				this.prefabs[i].prefab.transform.localScale = Vector3.Lerp(this.prefabs[i].prefab.transform.localScale, this.maxsize, MainManager.TieFramerate(this.shrinkspeed / 2f));
			}
			else if (this.prefabs[i].lifetime > -this.cooldown)
			{
				this.prefabs[i].prefab.transform.localScale = Vector3.Lerp(this.prefabs[i].prefab.transform.localScale, Vector3.zero, MainManager.TieFramerate(this.shrinkspeed));
			}
			else
			{
				this.SetRandom(i);
				this.prefabs[i].lifetime = Random.Range(this.liveframes / 2f, this.liveframes);
			}
		}
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x00064ABC File Offset: 0x00062CBC
	private void SetRandom(int id)
	{
		this.prefabs[id].prefab.transform.position = base.transform.position + MainManager.RandomVector(this.limits);
		this.prefabs[id].targetdir = this.prefabs[id].prefab.transform.position + MainManager.RandomVector(1f);
		this.prefabs[id].prefab.transform.LookAt(this.prefabs[id].targetdir);
	}

	// Token: 0x04000733 RID: 1843
	public GameObject prefabpart;

	// Token: 0x04000734 RID: 1844
	public int maxammount = 30;

	// Token: 0x04000735 RID: 1845
	public float speed = 0.01f;

	// Token: 0x04000736 RID: 1846
	public float liveframes = 250f;

	// Token: 0x04000737 RID: 1847
	public float cooldown = 120f;

	// Token: 0x04000738 RID: 1848
	public float shrinkspeed = 0.02f;

	// Token: 0x04000739 RID: 1849
	public Vector3 limits;

	// Token: 0x0400073A RID: 1850
	public Vector3 maxsize = Vector3.one;

	// Token: 0x0400073B RID: 1851
	public Vector3 childspin;

	// Token: 0x0400073C RID: 1852
	private PrefabParticle.PrefabData[] prefabs;

	// Token: 0x02000276 RID: 630
	private struct PrefabData
	{
		// Token: 0x040020F7 RID: 8439
		public float lifetime;

		// Token: 0x040020F8 RID: 8440
		public MeshRenderer prefab;

		// Token: 0x040020F9 RID: 8441
		public Vector3 targetdir;
	}
}
