using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
	private GameObject gameTile;
	private GameObject[] gameChildTile;
	private Mesh meshField;
	private Mesh[] meshChildField;
	private MeshFilter mfTileMeshFilter;
	private MeshFilter[] mfChildMeshFilter;
	private int iIndexX;
	private int iIndexY;

	public GameObject GameTile
	{
		get	{return this.gameTile;}
		set {this.gameTile = GameTile;}
	}

	public Mesh mesh
	{
		get	{return this.meshField;}
		set {this.meshField = mesh;}
	}


	public Mesh[] CHILDMESH {
		get {return this.meshChildField;}
	}

	public GameObject[] CHILDTILE {
		get {return this.gameChildTile;}
	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init()
	{

	}

	public void SetTile(GameObject field, GameObject terrain, string str)
	{
		this.meshField = Mesh.Instantiate(terrain.GetComponent<MeshFilter>().mesh) as Mesh;
		this.mfTileMeshFilter = (MeshFilter) field.GetComponent("MeshFilter");
		this.mfTileMeshFilter.mesh = this.mesh;

		this.gameChildTile = new GameObject[4];
		this.meshChildField = new Mesh[4];
		this.mfChildMeshFilter = new MeshFilter[4];

		GameObject tempField = GameObject.Instantiate(field) as GameObject;

		for(int i = 0; i < 4; ++i)
		{
			this.gameChildTile[i] = GameObject.Instantiate(tempField) as GameObject;
			this.gameChildTile[i].name = str + "_child_" + i;
			this.gameChildTile[i].transform.parent = field.transform;
			this.meshChildField[i] = Mesh.Instantiate(terrain.GetComponent<MeshFilter>().mesh) as Mesh;

			this.mfChildMeshFilter[i] = (MeshFilter) this.gameChildTile[i].GetComponent("MeshFilter");
			this.mfChildMeshFilter[i].mesh = this.meshChildField[i];

			this.mfChildMeshFilter[i].renderer.enabled = false;
		}

		Destroy(tempField);
	}

}
