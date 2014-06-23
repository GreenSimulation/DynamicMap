using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

public class DownloadTexture : MonoBehaviour {
	private int iTileNumX = 1;
	private int iTileNumY = 1;
	private int iZoom;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void Setup(int ix, int iy, int zoom)
	{
		this.iTileNumX = ix;
		this.iTileNumY = iy;
		this.iZoom = zoom;
	}


	public IEnumerator StreamingTexture(int tilex, int tiley, GameObject[] tile)
	{
		StringBuilder url = new StringBuilder();
		StringBuilder sbTexturePath = new StringBuilder();

		for(int y = 0; y < this.iTileNumY; ++y)
		{
			for(int x = 0; x < this.iTileNumX; ++x)
			{
				int tempx = tilex + x;
				int tempy = tiley - y;

				sbTexturePath.Length = 0;
				sbTexturePath.AppendFormat("Texture/x_{0}_y_{1}_z_{2}.png", tempx, tempy, this.iZoom);
				string strFilePath = Path.Combine(Application.streamingAssetsPath, sbTexturePath.ToString());
				url.Length = 0;

				if(File.Exists(strFilePath)) {
					url.AppendFormat("file://{0}", strFilePath);
//					StartCoroutine(this.ReadTextureFromFile(url.ToString(), tile[x + y * this.iTileNumX]));	
					tile[x + y * this.iTileNumX].renderer.material.mainTexture = Resources.LoadAssetAtPath("Assets/StreamingAssets/" + sbTexturePath.ToString(), typeof(Texture2D)) as Texture2D;
					tile[x + y * this.iTileNumX].renderer.material.mainTexture.mipMapBias = -2.0f;
				}
				else {
					url.AppendFormat("http://1.maps.nlp.nokia.com/maptile/2.1/maptile/newest/satellite.day/{0}/{1}/{2}/256/png8?lg=ENG&token=10KBSE6DwHtRnKzeu6Ohkw&app_id=tXMk8FVbu9QmIHL3eMtl"
					                 , this.iZoom, tempx, tempy);
										
					StartCoroutine(this.DownloadTextureFromWWW(url.ToString(), tile[x + y * this.iTileNumX], tempx, tempy));
				}
			}

			yield return null;;
		}
	}

	IEnumerator DownloadTextureFromWWW(string url, GameObject tile, int x, int y)
	{
		WWW www = new WWW(url);
		
		yield return www;
		
		if(www.size == 0) {
			StopCoroutine("DownloadTextureFromWWW");
		}
		else {			
			Texture2D tempTex = new Texture2D(256, 256, TextureFormat.ARGB32, true);
			www.LoadImageIntoTexture(tempTex);
			tempTex.SetPixels(0, 0, 256, 256, www.texture.GetPixels());
			tempTex.Apply();
			byte[] bytes = tempTex.EncodeToPNG();

			StringBuilder sbTexturePath = new StringBuilder();
			sbTexturePath.Length = 0;
			sbTexturePath.AppendFormat("Texture/x_{0}_y_{1}_z_{2}.png", x, y, this.iZoom);
			string filePath = Path.Combine(Application.streamingAssetsPath, sbTexturePath.ToString());
			File.WriteAllBytes(filePath, bytes);

			tile.renderer.material.mainTexture = www.texture;

			Debug.Log("Texture Save Complete!");
		}
	}


	IEnumerator ReadTextureFromFile(string url, GameObject tile)
	{
		WWW www = new WWW(url);
	
		yield return www;
		
		if(www.size == 0) {
			StopCoroutine("ReadTextureFromFile");
		}
		else {
			Texture2D tempTex = new Texture2D(256, 256, TextureFormat.ARGB32, true);
			www.LoadImageIntoTexture(tempTex);
			tempTex.SetPixels(0, 0, 256, 256, www.texture.GetPixels());
			tempTex.Apply();

			tile.renderer.material.mainTexture = www.texture;

			Debug.Log("Texture Load Complete!");
		}
	}

	public IEnumerator StreamingChildTexture(int tilex, int tiley, Tile tile)
	{
		StringBuilder url = new StringBuilder();
		StringBuilder sbTexturePath = new StringBuilder();
		int tempZoom = this.iZoom + 1;
		
		for(int y = 0; y < 2; ++y)
		{
			for(int x = 0; x < 2; ++x)
			{
				int tempx = tilex + x - 1;
				int tempy = tiley - y + 1;
				
				sbTexturePath.Length = 0;
				sbTexturePath.AppendFormat("Texture/x_{0}_y_{1}_z_{2}.png", tempx, tempy, tempZoom);
				string strFilePath = Path.Combine(Application.streamingAssetsPath, sbTexturePath.ToString());
				url.Length = 0;
				
				if(File.Exists(strFilePath)) {
					url.AppendFormat("file://{0}", strFilePath);
					StartCoroutine(this.ReadChildTextureFromFile(url.ToString(), tile, x + y * 2));		
//					Texture2D tempTex = new Texture2D(256, 256, TextureFormat.ARGB32, true);
//					tempTex = Resources.LoadAssetAtPath("Assets/StreamingAssets/" + sbTexturePath.ToString(), typeof(Texture2D)) as Texture2D;
//					tempTex.filterMode = FilterMode.Bilinear;
//					
//					tile.renderer.material.mainTexture = tempTex;

				}
				else {
					url.AppendFormat("http://1.maps.nlp.nokia.com/maptile/2.1/maptile/newest/satellite.day/{0}/{1}/{2}/256/png8?lg=ENG&token=10KBSE6DwHtRnKzeu6Ohkw&app_id=tXMk8FVbu9QmIHL3eMtl"
					                 , tempZoom, tempx, tempy);
					
					StartCoroutine(this.DownloadChildTextureFromWWW(url.ToString(), tile, x + y * 2, tempx, tempy, tempZoom));
				}

				yield return null;
			}
		}
		
		yield return new WaitForSeconds(0.3f);
		
		for(int i = 0; i < 4; ++i) {
			tile.CHILDTILE[i].renderer.enabled = true;
		}

		tile.renderer.enabled = false;
	}
	
	IEnumerator DownloadChildTextureFromWWW(string url, Tile tile, int index, int x, int y, int zoom)
	{
		WWW www = new WWW(url);
		
		yield return www;
		
		if(www.size == 0) {
			StopCoroutine("DownloadChildTextureFromWWW");
		}
		else {
			Texture2D tempTex = new Texture2D(256, 256, TextureFormat.ARGB32, true);
			www.LoadImageIntoTexture(tempTex);
			tempTex.SetPixels(0, 0, 256, 256, www.texture.GetPixels());
			tempTex.Apply();
			
			byte[] bytes = tempTex.EncodeToPNG();	
			StringBuilder sbTexturePath = new StringBuilder();
			sbTexturePath.Length = 0;
			sbTexturePath.AppendFormat("Texture/x_{0}_y_{1}_z_{2}.png", x, y, zoom);
			string filePath = Path.Combine(Application.streamingAssetsPath, sbTexturePath.ToString());
			File.WriteAllBytes(filePath, bytes);
			
			tile.CHILDTILE[index].renderer.material.mainTexture = www.texture;

			Debug.Log("Child Texture Save Complete!");
		}
	}
	
	IEnumerator ReadChildTextureFromFile(string url, Tile tile, int index)
	{
		WWW www = new WWW(url);
		
		yield return www;
		
		if(www.size == 0) {
			StopCoroutine("ReadTextureFromFile");
		}
		else {
			Texture2D tempTex = new Texture2D(256, 256, TextureFormat.ARGB32, true);
			www.LoadImageIntoTexture(tempTex);
			tempTex.SetPixels(0, 0, 256, 256, www.texture.GetPixels());
			tempTex.Apply();

			tile.CHILDTILE[index].renderer.material.mainTexture = www.texture;

			Debug.Log("Child Texture Load Complete!");
		}
	}

	public IEnumerator StreamingNewTexture(int tilex, int tiley, GameObject tile)
	{
		StringBuilder url = new StringBuilder();
		StringBuilder sbTexturePath = new StringBuilder();
			
		sbTexturePath.Length = 0;
		sbTexturePath.AppendFormat("Texture/x_{0}_y_{1}_z_{2}.png", tilex, tiley, this.iZoom);
		string strFilePath = Path.Combine(Application.streamingAssetsPath, sbTexturePath.ToString());
		url.Length = 0;
				
		if(File.Exists(strFilePath)) {
			url.AppendFormat("file://{0}", strFilePath);
//			StartCoroutine(this.ReadTextureFromFile(url.ToString(), tile));			
			tile.renderer.material.mainTexture = renderer.material.mainTexture = Resources.LoadAssetAtPath("Assets/StreamingAssets/" + sbTexturePath.ToString(), typeof(Texture2D)) as Texture2D;
			tile.renderer.material.mainTexture.mipMapBias = -2.0f;
		}
		else {
			url.AppendFormat("http://1.maps.nlp.nokia.com/maptile/2.1/maptile/newest/satellite.day/{0}/{1}/{2}/256/png8?lg=ENG&token=10KBSE6DwHtRnKzeu6Ohkw&app_id=tXMk8FVbu9QmIHL3eMtl"
			                 , this.iZoom, tilex, tiley);
					
			StartCoroutine(this.DownloadTextureFromWWW(url.ToString(), tile, tilex, tiley));
		}

		tile.renderer.enabled = false;

		yield return new WaitForSeconds(0.0f);

		tile.renderer.enabled = true;
	}
}
