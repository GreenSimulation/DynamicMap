using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using GridGIS;

public class MakeTerrain : MonoBehaviour {
	private DownloadTexture DOWNTEXTURE;
	private Tile[] TILE;
	private GameObject[] gameTile;

	public int iColumn;
	public int iRow;
	private double dWest;
	private double dEast;
	private double dSouth;
	private double dNorth;
	private Vector2 vecCellSize;

	private double[] adMercator = new double[2]; 
	public Vector2 vecLatLaong;
	private int iHeightIndexX;
	private int iHeightIndexY;
	private int iTextureTileIndexX;
	private int iTextureTileIndexY;
	private int iChildTextureTileIndexX;
	private int iChildTextureTileIndexY;
	private int iTextureTileSize = 256;

	private int iZoom;
	public int iScale = 32;
	public int iTileNumX = 6;
	public int iTileNumY = 6;
	public int iFieldSizeX = 80;
	public int iFieldSizeY = 80;

	private int iStartChildX = 0;
	private int iStartChildY = 0;
	private int iChildOffsetX = 3;
	private int iChildOffsetY = 3;

	private int iLOD = 2;

	private GameObject gameField;
	private GameObject gameTerrain;

	public string strDEMPath = null;
	private StringBuilder sbPath = null;


	enum TILETYPE { TILE, CHILD };

	public int TileX {
		get {return this.iTileNumX;}
	}

	public int TileY {
		get {return this.iTileNumY;}
	}

	public int Scale {
		get {return this.iScale;}
	}
	

	// Use this for initialization
	void Start () {
					
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void Init(int zoom)
	{
		this.sbPath = new StringBuilder();
		this.sbPath.Length = 0;
		this.sbPath.AppendFormat("{0}/{1}", Application.streamingAssetsPath, this.strDEMPath);

		this.iZoom = zoom;
		this.gameField = GameObject.Find("Field");

		DOWNTEXTURE = this.gameObject.GetComponent<DownloadTexture>();
		DOWNTEXTURE.Setup(this.iTileNumX, this.iTileNumY, this.iZoom);

		TILE = new Tile[this.iTileNumX * this.iTileNumY];

		this.gameTile = new GameObject[this.iTileNumX * this.iTileNumY];

		this.ReadBTHeader();
		this.MakeTile();
		this.MakeField();
	}

	void ReadBTHeader()
	{
		BinaryReader br = new BinaryReader(File.Open(this.sbPath.ToString(), FileMode.Open));
		
		br.BaseStream.Seek(10, SeekOrigin.Begin);
		this.iColumn = br.ReadInt32();
		this.iRow = br.ReadInt32();

		br.BaseStream.Seek(28, SeekOrigin.Begin);
		this.dWest = br.ReadDouble();
		this.dEast = br.ReadDouble();
		this.dSouth = br.ReadDouble();
		this.dNorth = br.ReadDouble();

		this.vecCellSize.x = 32.0f;
		this.vecCellSize.y = 32.0f;

//		this.vecLatLaong.x = 129.059989f;
//		this.vecLatLaong.y = 35.191264f;

		Converter coord_convert = Converter.GenerateConverter(ConvertType.PROJ4_NET);
		Coordinate origin_coord = Coordinate.make(this.vecLatLaong.x, this.vecLatLaong.y);
		Coordinate target_coord = Coordinate.zero;
		
		string mercator = "+proj=merc +lon_0=0 +k=1 +x_0=0 +y_0=0 +ellps=WGS84 +datum=WGS84 +units=m +no_defs";
		
		if(coord_convert.setup(coord_convert.WGS84(), mercator))
		{
			target_coord = coord_convert.transform(origin_coord);
		}

		this.adMercator[0] = target_coord.x;
		this.adMercator[1] = target_coord.y;

		this.iHeightIndexX = (int)((this.adMercator[0] - this.dWest) / this.vecCellSize.x) - 70;
		this.iHeightIndexY = (int)((this.adMercator[1] - this.dSouth) / this.vecCellSize.y) - 35;
//		this.iHeightIndexX = (int)((this.adMercator[0] - this.dWest) / this.vecCellSize.x) - 2200;
//		this.iHeightIndexY = (int)((this.adMercator[1] - this.dSouth) / this.vecCellSize.y) - 2300;

		this.MakeTextureTileIndex();

		br.Close();
	}


	void MakeTextureTileIndex()
	{
		uint mapSize = (uint) this.iTextureTileSize << this.iZoom;
		
		Vector2 pos = LatLongToMercat(this.vecLatLaong.x, this.vecLatLaong.y);
		
		int px = (int) Clip(pos.x * mapSize + 0.5, 0, mapSize - 1);
		int py = (int) Clip(pos.y * mapSize + 0.5, 0, mapSize - 1);
		
		this.iTextureTileIndexX = px / this.iTextureTileSize;
		this.iTextureTileIndexY = py / this.iTextureTileSize;

		mapSize = (uint) this.iTextureTileSize << (this.iZoom + 1);

		px = (int) Clip(pos.x * mapSize + 0.5, 0, mapSize - 1);
		py = (int) Clip(pos.y * mapSize + 0.5, 0, mapSize - 1);
		
		this.iChildTextureTileIndexX = px / this.iTextureTileSize;
		this.iChildTextureTileIndexY = py / this.iTextureTileSize;
	}


	void MakeTile()
	{
		this.gameTerrain = GameObject.Find("Terrain");

		StringBuilder sbObjectName = new StringBuilder();

		for(int y = 0; y < this.iTileNumY; ++y)
		{
			for(int x = 0; x < this.iTileNumX; ++x)
			{
				int index = x + y * this.iTileNumX;

				this.gameTile[index] = GameObject.Instantiate(this.gameField) as GameObject;
				sbObjectName.Length = 0;
				sbObjectName.AppendFormat("Tile({0},{1})", x, y);
				this.gameTile[index].name = sbObjectName.ToString();
				this.gameTile[index].transform.parent = this.transform;

				TILE[index] = this.gameTile[index].AddComponent<Tile>();
				TILE[index].SetTile(this.gameTile[index], this.gameTerrain, sbObjectName.ToString());

				//this.m_mcFieldCollider[i] = transform.FindChild(childname).GetComponent<MeshCollider>();
			}
		}
	}


	void MakeField()
	{
		BinaryReader br = new BinaryReader(File.Open(this.sbPath.ToString(), FileMode.Open));

		this.iStartChildX = (int) (0.5 * this.iTileNumX - 2);
		this.iStartChildY = (int) (0.5 * this.iTileNumY - 2);

		for(int y = 0; y < this.iTileNumY; ++y)
		{
			for(int x = 0; x < this.iTileNumX; ++x)
			{
				this.iLOD = 2;
				this.MakeMesh(br, this.iLOD * x, this.iLOD * y, x + y * this.iTileNumX, 0, (int) TILETYPE.TILE);

				this.gameTile[x + y * this.iTileNumX].transform.position = new Vector3(x * (this.iFieldSizeX - 2) * this.iScale * this.iLOD, 0, 
				                                                                       y * (this.iFieldSizeY - 2) * this.iScale * this.iLOD);

//				if((x >= this.iStartChildX && x <= this.iStartChildX + this.iChildOffsetX) &&
//				   (y >= this.iStartChildY && y <= this.iStartChildY + this.iChildOffsetY)){
//					this.iLOD = 1;
//					
//					this.MakeChildField(br, x, y);
//				}
			}
		}

		br.Close();

		StartCoroutine(DOWNTEXTURE.StreamingTexture(this.iTextureTileIndexX, this.iTextureTileIndexY, this.gameTile));
	}


	void MakeMesh(BinaryReader br, int ix, int iy, int findex, int childindex, int type)
	{
		int index = 0; int triNum = 0;
		long temp = 0; long gridindex = 0;

		Vector3[] vertices = new Vector3[this.iFieldSizeX * this.iFieldSizeY];
		Vector3[] normal = new Vector3[this.iFieldSizeX * this.iFieldSizeY];
		Vector2[] uv = new Vector2[this.iFieldSizeX * this.iFieldSizeY];
		int[] triangles = new int[3 * 2 * (this.iFieldSizeX - 1) * (this.iFieldSizeY - 1)];

		for(int y = 0; y < this.iFieldSizeY; ++y)
		{
			for(int x = 0; x < this.iFieldSizeX; ++x)
			{
				index = this.iFieldSizeX * y + x;

				temp = (long) this.iRow * (this.iHeightIndexX + (this.iLOD * x) + ix * (this.iFieldSizeX - 2));
				gridindex = 256 + (temp + (this.iHeightIndexY + (this.iLOD * y) + iy * (this.iFieldSizeY - 2))) * 4;

				br.BaseStream.Seek(gridindex, SeekOrigin.Begin);
				
				vertices[index].x = (float) x;
				vertices[index].y = (float) br.ReadSingle() * (1.0f / 32.0f * this.iScale);
				vertices[index].z = (float) y;
				
				normal[index].x = 0.0f;
				normal[index].y = 1.0f;
				normal[index].z = 0.0f;
				
				uv[index].x = (float) x / (float)this.iFieldSizeX;
				uv[index].y = (float) y / (float)this.iFieldSizeY;
			}
		}

		for(int y = 0; y < (this.iFieldSizeY - 2); ++y)
		{
			for(int x = 0; x < (this.iFieldSizeX - 2); ++x)
			{
				triangles[triNum + 0] = this.iFieldSizeX * y + x;
				triangles[triNum + 2] = this.iFieldSizeX * y + (x + 1);
				triangles[triNum + 1] = this.iFieldSizeX * (y + 1) + (x + 1);
				
				triangles[triNum + 3] = this.iFieldSizeX * (y + 1) + (x + 1);
				triangles[triNum + 5] = this.iFieldSizeX * (y + 1) + x;
				triangles[triNum + 4] = this.iFieldSizeX * y + x;
				
				triNum += 6;
			}
		}

		if(type == (int) TILETYPE.TILE) {
			TILE[findex].mesh.Clear();
			TILE[findex].mesh.vertices = vertices;
			TILE[findex].mesh.uv = uv;
			TILE[findex].mesh.triangles = triangles;
			TILE[findex].mesh.normals = normal;
			
			this.gameTile[findex].transform.localScale = new Vector3(this.iLOD * this.iScale, 1, this.iLOD * this.iScale);
		}
		else if(type == (int) TILETYPE.CHILD) {
			TILE[findex].CHILDMESH[childindex].Clear();
			TILE[findex].CHILDMESH[childindex].vertices = vertices;
			TILE[findex].CHILDMESH[childindex].uv = uv;
			TILE[findex].CHILDMESH[childindex].triangles = triangles;
			TILE[findex].CHILDMESH[childindex].normals = normal;
			
			TILE[findex].CHILDTILE[childindex].transform.localScale = new Vector3(0.5f, 1.0f, 0.5f);
		}
	}


	void MakeChildField(BinaryReader br, int ix, int iy)
	{
		for(int y = 0; y < 2; ++y)
		{
			for(int x = 0; x < 2; ++x)
			{
				this.MakeMesh(br, 1 * (2 * ix + x), 1 * (2 * iy + y), ix + iy * this.iTileNumX, x + y * 2, (int) TILETYPE.CHILD);
				
				TILE[ix + iy * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
					ix * (this.iFieldSizeX - 1) * this.iScale * 2 + x * (this.iFieldSizeX - 1) * this.iScale, 0, 
					iy * (this.iFieldSizeY - 1) * this.iScale * 2 + y * (this.iFieldSizeY - 1) * this.iScale);
			}
		}
		
		StartCoroutine(DOWNTEXTURE.StreamingChildTexture(this.iChildTextureTileIndexX + 2 * ix, this.iChildTextureTileIndexY - 2 * iy, TILE[ix + iy * this.iTileNumX]));
	}


	public void DisableChildTile(int x, int y)
	{
		if(y >= this.iTileNumY) { y = 0; }

		for(int i = 0; i < 4; ++i)
		{
			TILE[x + y * this.iTileNumX].CHILDTILE[i].renderer.enabled = false;
		}

		this.gameTile[x + y * this.iTileNumX].renderer.enabled = true;
	}


	public void MakeNewChildTile(int ix, int iy, int tileX, int tileY, int posX, int posY, int whereMoving)
	{
		BinaryReader br = new BinaryReader(File.Open(this.sbPath.ToString(), FileMode.Open));

		this.iLOD = 1;

		for(int y = 0; y < 2; ++y)
		{
			for(int x = 0; x < 2; ++x)
			{
				this.MakeMesh(br, 2 * ix + x, 2 * iy + y, tileX + tileY * this.iTileNumX, x + y * 2, (int) TILETYPE.CHILD);

				switch(whereMoving)
				{
				case 0:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						posX * (this.iFieldSizeX - 1) * this.iScale * 2 + x * (this.iFieldSizeX - 1) * this.iScale, 0, 
						(this.iStartChildY + 4) * (this.iFieldSizeY - 1) * this.iScale * 2 + y * (this.iFieldSizeY - 1) * this.iScale);

					break;

				case 1:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						posX * (this.iFieldSizeX - 1) * this.iScale * 2 + x * (this.iFieldSizeX - 1) * this.iScale, 0, 
						(this.iStartChildY - 1) * (this.iFieldSizeY - 1) * this.iScale * 2 + y * (this.iFieldSizeY - 1) * this.iScale);
					
					break;

				case 2:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						(this.iStartChildX + 4) * (this.iFieldSizeX - 1) * this.iScale * 2 + x * (this.iFieldSizeX - 1) * this.iScale, 0, 
						posY * (this.iFieldSizeY - 1) * this.iScale * 2 + y * (this.iFieldSizeY - 1) * this.iScale);
					
					break;

				case 3:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						(this.iStartChildX - 1) * (this.iFieldSizeX - 1) * this.iScale * 2 + x * (this.iFieldSizeX - 1) * this.iScale, 0, 
						posY * (this.iFieldSizeY - 1) * this.iScale * 2 + y * (this.iFieldSizeY - 1) * this.iScale);
					
					break;
				}
			}
		}
		
		StartCoroutine(DOWNTEXTURE.StreamingChildTexture(this.iChildTextureTileIndexX + 2 * ix, this.iChildTextureTileIndexY - 2 * iy, TILE[tileX + tileY * this.iTileNumX]));
		
		this.iLOD = 2;
		
		br.Close();
	}

	

	public void DestroyTile(int index)
	{
//		Destroy(this.gameTile[index]);
		this.gameTile[index].renderer.enabled = false;
		this.gameTile[index].renderer.material.mainTexture = null;
	}


	public void MakeNewTile(int index, int x, int y)
	{
		this.gameTile[index] = GameObject.Instantiate(this.gameField) as GameObject;

		StringBuilder sbObjectName = new StringBuilder();
		sbObjectName.Length = 0;
		sbObjectName.AppendFormat("Tile({0},{1})", x, y);
		this.gameTile[index].name = sbObjectName.ToString();
		this.gameTile[index].transform.parent = this.transform;
		
		TILE[index] = this.gameTile[index].AddComponent<Tile>();
		TILE[index].SetTile(this.gameTile[index], this.gameTerrain, sbObjectName.ToString());
	}


	public void MakeNewField(int ix, int iy, int index, int posX, int posY)
	{
		string strFilePath = Path.Combine(Application.streamingAssetsPath, "dem_terrain_32m.bt");
		BinaryReader br = new BinaryReader(File.Open(strFilePath, FileMode.Open));
		
		this.iLOD = 2;
		this.ChangeHeightMap(br, this.iLOD * ix, this.iLOD * iy, index, 0, (int) TILETYPE.TILE);

		this.gameTile[index].transform.position = new Vector3(posX * (this.iFieldSizeX - 2) * this.iScale * this.iLOD, 0, 
		                                                      posY * (this.iFieldSizeY - 2) * this.iScale * this.iLOD);

		br.Close();

		StartCoroutine(DOWNTEXTURE.StreamingNewTexture(this.iTextureTileIndexX + ix, this.iTextureTileIndexY - iy, this.gameTile[index]));
	}


	void ChangeHeightMap(BinaryReader br, int ix, int iy, int findex, int childindex, int type)
	{
		int index = 0;
		long temp = 0; long gridindex = 0;
		
		Vector3[] vertices = new Vector3[this.iFieldSizeX * this.iFieldSizeY];
		
		for(int y = 0; y < this.iFieldSizeY; ++y)
		{
			for(int x = 0; x < this.iFieldSizeX; ++x)
			{
				index = this.iFieldSizeX * y + x;
				
				temp = (long) this.iRow * (this.iHeightIndexX + (this.iLOD * x) + ix * (this.iFieldSizeX - 2));
				gridindex = 256 + (temp + (this.iHeightIndexY + (this.iLOD * y) + iy * (this.iFieldSizeY - 2))) * 4;
				
				br.BaseStream.Seek(gridindex, SeekOrigin.Begin);
				
				vertices[index].x = (float) x;
				vertices[index].y = (float) br.ReadSingle() * (1.0f / 64.0f * this.iScale * this.iLOD);
				vertices[index].z = (float) y;
			}
		}
		
		TILE[findex].mesh.vertices = vertices;
		
		this.gameTile[findex].transform.localScale = new Vector3(this.iLOD * this.iScale, 1, this.iLOD * this.iScale);
	}



	public void MovingTerrain(int ix, int iy)
	{
		for(int y = 0; y < this.iTileNumY; ++y)
		{
			for(int x = 0; x < this.iTileNumX; ++x)
			{
				int index = x + y * this.iTileNumX;

				this.gameTile[index].transform.position = new Vector3(
					this.gameTile[index].transform.position.x - this.iLOD * this.iScale * (this.iFieldSizeY - 2) * ix, 0,
					this.gameTile[index].transform.position.z - this.iLOD * this.iScale * (this.iFieldSizeY - 2) * iy);
			}
		}
	}


	private static double Clip(double n, double minValue, double maxValue)
	{
		return System.Math.Min(System.Math.Max(n, minValue), maxValue);
	}
	
	public static Vector2 LatLongToMercat(float x, float y)
	{
		float sy = Mathf.Sin(y * Mathf.Deg2Rad);
		return new Vector2((x + 180) / 360, 0.5f - Mathf.Log((1 + sy) / (1 - sy)) / (4 * Mathf.PI));
	}

	public void SetLOD(int lod, int startX, int startY, int endX, int endY)
	{
		this.iLOD = lod;

	}
}
