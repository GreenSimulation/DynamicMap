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
	public float fMainScaleX = 1.0f;
	public float fMainScaleY = 1.0f;
	public float fScaleX = 32.0f;
	public float fScaleY = 32.0f;
	public int iTileNumX = 0;
	public int iTileNumY = 0;
	public int iFieldSizeX = 0;
	public int iFieldSizeY = 0;

	private int iStartChildX = 0;
	private int iStartChildY = 0;
	private int iChildOffsetX = 3;
	private int iChildOffsetY = 3;

	public float fWidth = 0.0f;
	public float fHeight = 0.0f;
	public double[] DaCharacterPos = new double[2];
	private float fStartPosX;
	private float fStartPosY;

	public int iLOD = 4;

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

	public int SizeX {
		get {return this.iFieldSizeX;}
	}
	
	public int SizeY {
		get {return this.iFieldSizeY;}
	}

	public float ScaleX {
		get {return this.fScaleX;}
	}

	public float ScaleY {
		get {return this.fScaleY;}
	}

	public float StartX {
		get {return this.fStartPosX;}
	}

	public float StartY {
		get {return this.fStartPosY;}
	}

	public int LOD {
		get {return this.iLOD;}
	}
	

	// Use this for initialization
	void Start () {
					
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void Init(int zoom, Vector3 charactorPos)
	{
		this.sbPath = new StringBuilder();
		this.sbPath.Length = 0;
		this.sbPath.AppendFormat("{0}/{1}", Application.streamingAssetsPath, this.strDEMPath);

		this.iZoom = zoom;

		this.ReadBTHeader(charactorPos);

//		this.fStartPosX = (float) ((DaCharacterPos[0] - this.dWest) / (this.dEast - this.dWest) * this.fWidth - this.fWidth * 0.5f);
//		this.fStartPosY = (float) ((DaCharacterPos[1] - this.dSouth) / (this.dNorth - this.dSouth) * this.fHeight - this.fHeight * 0.5f);

		this.fStartPosX = charactorPos.x - (this.iTileNumX * 0.5f) * (this.iFieldSizeX -2) * this.fScaleX - 20.0f;;
		this.fStartPosY = charactorPos.z - (this.iTileNumY * 0.5f) * (this.iFieldSizeY -2) * this.fScaleY - 35.0f;; 

		this.gameField = GameObject.Find("Field");

		DOWNTEXTURE = this.gameObject.GetComponent<DownloadTexture>();
		DOWNTEXTURE.Setup(this.iTileNumX, this.iTileNumY, this.iZoom);

		TILE = new Tile[this.iTileNumX * this.iTileNumY];

		this.gameTile = new GameObject[this.iTileNumX * this.iTileNumY];

		this.MakeTile();
		this.MakeField();
	}

	void ReadBTHeader(Vector3 charactorPos)
	{
		BinaryReader br = new BinaryReader(File.Open(this.strDEMPath, FileMode.Open));
		
		br.BaseStream.Seek(10, SeekOrigin.Begin);
		this.iColumn = br.ReadInt32();
		this.iRow = br.ReadInt32();

		this.fScaleX = this.fMainScaleX * this.fWidth / this.iColumn;
		this.fScaleY = this.fMainScaleY * this.fHeight / this.iRow;

		br.BaseStream.Seek(28, SeekOrigin.Begin);
		this.dWest = br.ReadDouble();
		this.dEast = br.ReadDouble();
		this.dSouth = br.ReadDouble();
		this.dNorth = br.ReadDouble();

//		this.vecCellSize.x = 32.0f;
//		this.vecCellSize.y = 32.0f;
		this.vecCellSize.x = 128.0f;
		this.vecCellSize.y = 128.0f;

		this.DaCharacterPos[0] = ((charactorPos.x / this.fWidth + 0.5f) * (this.dEast - this.dWest) + this.dWest);
		this.DaCharacterPos[1] = ((charactorPos.z / this.fHeight + 0.5f) * (this.dNorth - this.dSouth) + this.dSouth);

//		double test = ((14360214.31233 - this.dEast) / (this.dWest - this.dEast) - 0.5f) * this.fWidth;
//		double test = ((14360214.31233 - this.dEast) / (this.dWest - this.dEast));
//		Debug.Log(test);	

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

		Coordinate start_coord = Coordinate.zero;

//		start_coord.x = target_coord.x - ((int) (this.iTileNumX * 0.5)) * this.vecCellSize.x * (this.iFieldSizeX - 2) * this.iLOD;
//		start_coord.y = target_coord.y - ((int) (this.iTileNumY * 0.5)) * this.vecCellSize.y * (this.iFieldSizeY - 2) * this.iLOD;
		start_coord.x = DaCharacterPos[0] - ((int) (this.iTileNumX * 0.5)) * this.vecCellSize.x * (this.iFieldSizeX - 2) * this.iLOD;
		start_coord.y = DaCharacterPos[1] - ((int) (this.iTileNumY * 0.5)) * this.vecCellSize.y * (this.iFieldSizeY - 2) * this.iLOD;

		this.adMercator[0] = start_coord.x;
		this.adMercator[1] = start_coord.y;
//		this.adMercator[0] = target_coord.x;
//		this.adMercator[1] = target_coord.y;

//		Debug.Log(this.adMercator[0]);

		Coordinate texture_coord = Coordinate.zero;
		texture_coord = coord_convert.inverse(start_coord);

		this.iHeightIndexX = (int)((this.adMercator[0] - this.dWest) / this.vecCellSize.x - 25);
		this.iHeightIndexY = (int)((this.adMercator[1] - this.dSouth) / this.vecCellSize.y - 10);

//		Debug.Log(this.iHeightIndexX);

//		this.iHeightIndexX = (int)((this.adMercator[0] - this.dWest) / this.vecCellSize.x) - 2200;
//		this.iHeightIndexY = (int)((this.adMercator[1] - this.dSouth) / this.vecCellSize.y) - 2300;

		this.MakeTextureTileIndex(texture_coord);

		br.Close();
	}


	void MakeTextureTileIndex(Coordinate coord)
	{
		uint mapSize = (uint) this.iTextureTileSize << this.iZoom;
		
//		Vector2 pos = LatLongToMercat(this.vecLatLaong.x, this.vecLatLaong.y);
		Vector2 pos = LatLongToMercat((float) coord.x, (float) coord.y);
		
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
		BinaryReader br = new BinaryReader(File.Open(this.strDEMPath, FileMode.Open));

//		this.iStartChildX = (int) (0.5 * this.iTileNumX - 2);
//		this.iStartChildY = (int) (0.5 * this.iTileNumY - 2);

		for(int y = 0; y < this.iTileNumY; ++y)
		{
			for(int x = 0; x < this.iTileNumX; ++x)
			{
//				this.iLOD = 2;
				this.MakeMesh(br, this.iLOD * x, this.iLOD * y, x + y * this.iTileNumX, 0, (int) TILETYPE.TILE);

				this.gameTile[x + y * this.iTileNumX].transform.position = new Vector3(this.fStartPosX + x * (this.iFieldSizeX - 2) * this.fScaleX * this.iLOD, 0, 
				                                                                       this.fStartPosY + y * (this.iFieldSizeY - 2) * this.fScaleY * this.iLOD);

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
				vertices[index].y = (float) br.ReadSingle() * (0.015f * this.fScaleX * this.iLOD);
				vertices[index].z = (float) y;
				
				normal[index].x = 0.0f;
				normal[index].y = 1.0f;
				normal[index].z = 0.0f;
				
				uv[index].x = (float) x / (float) this.iFieldSizeX;
				uv[index].y = (float) y / (float) this.iFieldSizeY;
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
			
			this.gameTile[findex].transform.localScale = new Vector3(this.iLOD * this.fScaleX, 1, this.iLOD * this.fScaleY);
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
					ix * (this.iFieldSizeX - 1) * this.fScaleX * 2 + x * (this.iFieldSizeX - 1) * this.fScaleX, 0, 
					iy * (this.iFieldSizeY - 1) * this.fScaleY * 2 + y * (this.iFieldSizeY - 1) * this.fScaleY);
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
		BinaryReader br = new BinaryReader(File.Open(this.strDEMPath, FileMode.Open));

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
						posX * (this.iFieldSizeX - 1) * this.fScaleX * 2 + x * (this.iFieldSizeX - 1) * this.fScaleX, 0, 
						(this.iStartChildY + 4) * (this.iFieldSizeY - 1) * this.fScaleY * 2 + y * (this.iFieldSizeY - 1) * this.fScaleY);

					break;

				case 1:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						posX * (this.iFieldSizeX - 1) * this.fScaleX * 2 + x * (this.iFieldSizeX - 1) * this.fScaleX, 0, 
						(this.iStartChildY - 1) * (this.iFieldSizeY - 1) * this.fScaleY * 2 + y * (this.iFieldSizeY - 1) * this.fScaleY);
					
					break;

				case 2:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						(this.iStartChildX + 4) * (this.iFieldSizeX - 1) * this.fScaleX * 2 + x * (this.iFieldSizeX - 1) * this.fScaleX, 0, 
						posY * (this.iFieldSizeY - 1) * this.fScaleX * 2 + y * (this.iFieldSizeY - 1) * this.fScaleX);
					
					break;

				case 3:
					TILE[tileX + tileY * this.iTileNumX].CHILDTILE[x + y * 2].transform.position = new Vector3(
						(this.iStartChildX - 1) * (this.iFieldSizeX - 1) * this.fScaleX * 2 + x * (this.iFieldSizeX - 1) * this.fScaleX, 0, 
						posY * (this.iFieldSizeY - 1) * this.fScaleY * 2 + y * (this.iFieldSizeY - 1) * this.fScaleY);
					
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
		BinaryReader br = new BinaryReader(File.Open(this.strDEMPath, FileMode.Open));
		
//		this.iLOD = 2;
		this.ChangeHeightMap(br, this.iLOD * ix, this.iLOD * iy, index, 0, (int) TILETYPE.TILE);

		this.gameTile[index].transform.position = new Vector3(this.fStartPosX + posX * (this.iFieldSizeX - 2) * this.fScaleX * this.iLOD, 0, 
		                                                      this.fStartPosY + posY * (this.iFieldSizeY - 2) * this.fScaleY * this.iLOD);

		br.Close();

		StartCoroutine(DOWNTEXTURE.StreamingNewTexture(this.iTextureTileIndexX + ix, this.iTextureTileIndexY - iy, this.gameTile[index]));
//		DOWNTEXTURE.StreamingNewTexture(this.iTextureTileIndexX + ix, this.iTextureTileIndexY - iy, this.gameTile[index]);
//		this.gameTile[index].renderer.enabled = true;
	}


	void ChangeHeightMap(BinaryReader br, int ix, int iy, int findex, int childindex, int type)
	{
		int index = 0;
		long temp = 0; long gridindex = 0;
		
		Vector3[] vertices = new Vector3[this.iFieldSizeX * this.iFieldSizeY];
		byte[] bytes = new byte[this.iFieldSizeX * 4];

		for(int x = 0; x < this.iFieldSizeX; ++x)
		{
			temp = (long) this.iRow * (this.iHeightIndexX + (this.iLOD * x) + ix * (this.iFieldSizeX - 2));
			gridindex = 256 + (temp + (this.iHeightIndexY + iy * (this.iFieldSizeY - 2))) * 4;
			br.BaseStream.Seek(gridindex, SeekOrigin.Begin);
			bytes = br.ReadBytes(this.iFieldSizeX * 4);

			for(int y = 0; y < this.iFieldSizeY; ++y)
			{
				index = this.iFieldSizeX * y + x;

				float hY = System.BitConverter.ToSingle(bytes, y * 4);

				vertices[index].x = (float) x;
				vertices[index].y = (float) hY * (0.015f * this.fScaleX * this.iLOD);
				vertices[index].z = (float) y;
			}
		}
		
		TILE[findex].mesh.vertices = vertices;
		TILE[findex].mesh.RecalculateBounds();

		this.gameTile[findex].transform.localScale = new Vector3(this.iLOD * this.fScaleX, 1, this.iLOD * this.fScaleY);
	}


	public void MovingTerrain(int ix, int iy)
	{
		for(int y = 0; y < this.iTileNumY; ++y)
		{
			for(int x = 0; x < this.iTileNumX; ++x)
			{
				int index = x + y * this.iTileNumX;

				this.gameTile[index].transform.position = new Vector3(
					this.gameTile[index].transform.position.x - this.iLOD * this.fScaleX * (this.iFieldSizeY - 2) * ix, 0,
					this.gameTile[index].transform.position.z - this.iLOD * this.fScaleY * (this.iFieldSizeY - 2) * iy);
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
}
