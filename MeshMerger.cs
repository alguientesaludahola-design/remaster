using System;
using UnityEngine;

// Token: 0x0200003E RID: 62
public class MeshMerger : MonoBehaviour
{
	// Token: 0x06000692 RID: 1682 RVA: 0x0004A2EC File Offset: 0x000484EC
	private void Start()
	{
		this.filter = new MeshFilter[this.meshes.Length];
		for (int i = 0; i < this.meshes.Length; i++)
		{
			this.filter[i] = this.meshes[i].gameObject.GetComponent<MeshFilter>();
		}
		CombineInstance[] array = new CombineInstance[this.meshes.Length];
		Material[] materials = null;
		for (int j = 0; j < this.filter.Length; j++)
		{
			if (this.filter[j] == null)
			{
				this.filter[j] = this.meshes[j].GetComponent<MeshFilter>();
			}
			array[j].mesh = this.filter[j].mesh;
			array[j].transform = this.meshes[j].transform.localToWorldMatrix;
			if (j == 0 && this.meshes[j] != null)
			{
				materials = this.meshes[j].materials;
			}
			Object.Destroy(this.meshes[j].gameObject);
		}
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		meshFilter.mesh.CombineMeshes(array);
		meshFilter.transform.localEulerAngles = this.rotatefix;
		meshFilter.gameObject.AddComponent<MeshRenderer>().materials = materials;
		if (this.hasCollider)
		{
			meshFilter.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
		}
	}

	// Token: 0x040005E4 RID: 1508
	public MeshRenderer[] meshes;

	// Token: 0x040005E5 RID: 1509
	private MeshFilter[] filter;

	// Token: 0x040005E6 RID: 1510
	public bool hasCollider;

	// Token: 0x040005E7 RID: 1511
	public Vector3 rotatefix = new Vector3(-90f, 180f);
}
