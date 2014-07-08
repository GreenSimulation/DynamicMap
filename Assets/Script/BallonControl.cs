using UnityEngine;
using System.Collections;

public class BallonControl : MonoBehaviour {
	private GameObject[] gRedBallon;
	private GameObject[] gBlueBallon;
	private GameObject[] gYellowBallon;
	private GameObject GOplayer;
	private Vector3[] v3RandomPos = new Vector3[3] ;
	private int iScore;
	public float fScale;
	public int iBallonNumber;

	public float fMaxHeigth;
	public float fMaxRandomRange;
	public float fMinRandomRange;
	public float fMaxRandomHeight;

	// Use this for initialization
	void Start () {
		this.Init();
	}
	
	// Update is called once per frame
	void Update () {
		this.BallonMove();

		this.CheckBallon();
	}

	void Init()
	{
		this.GOplayer = GameObject.Find("Main Camera");
		
		GameObject red = GameObject.Find("Red");
		GameObject blue = GameObject.Find("Blue");
		GameObject yellow = GameObject.Find("Yellow");

		this.gRedBallon = new GameObject[this.iBallonNumber];
		this.gBlueBallon = new GameObject[this.iBallonNumber];
		this.gYellowBallon = new GameObject[this.iBallonNumber];

		for(int i = 0; i < this.iBallonNumber; ++i)
		{
			this.gRedBallon[i] = GameObject.Instantiate(red) as GameObject;
			this.gBlueBallon[i] = GameObject.Instantiate(blue) as GameObject;
			this.gYellowBallon[i] = GameObject.Instantiate(yellow) as GameObject;

			this.gRedBallon[i].transform.parent = this.transform;
			this.gBlueBallon[i].transform.parent = this.transform;
			this.gYellowBallon[i].transform.parent = this.transform;
		}

		this.SetBallon();
	}


	void SetBallon()
	{
		this.iScore = 0;

		for(int i = 0; i < this.iBallonNumber; ++i)
		{
			this.gRedBallon[i].renderer.enabled = true;
			this.gBlueBallon[i].renderer.enabled = true;
			this.gYellowBallon[i].renderer.enabled = true;
			
			this.gRedBallon[i].transform.localScale = new Vector3(this.fScale, this.fScale, this.fScale);
			this.gBlueBallon[i].transform.localScale = new Vector3(this.fScale, this.fScale, this.fScale);
			this.gYellowBallon[i].transform.localScale = new Vector3(this.fScale, this.fScale, this.fScale);
			
			for(int n = 0; n < 3; ++n)
			{
				this.v3RandomPos[n].x = Random.Range(this.fMinRandomRange, this.fMaxRandomRange);
				this.v3RandomPos[n].y = Random.Range(0, this.fMaxRandomHeight - this.GOplayer.transform.position.y);
				this.v3RandomPos[n].z = Random.Range(this.fMinRandomRange, this.fMaxRandomRange);
			}
			
			this.gRedBallon[i].transform.position = this.v3RandomPos[0] + this.GOplayer.transform.position;
			this.gRedBallon[i].transform.Rotate(0, Random.Range(0, 360), 0);
			
			this.gBlueBallon[i].transform.position = this.v3RandomPos[1] + this.GOplayer.transform.position;
			this.gBlueBallon[i].transform.Rotate(0, Random.Range(0, 360), 0);
			
			this.gYellowBallon[i].transform.position = this.v3RandomPos[2] + this.GOplayer.transform.position;
			this.gYellowBallon[i].transform.Rotate(0, Random.Range(0, 360), 0);
		}
	}


	void BallonMove() {
		for(int i = 0; i < this.iBallonNumber; ++i)
		{
			if(this.gRedBallon[i].renderer.enabled) {
				if(this.gRedBallon[i].transform.position.y < this.fMaxHeigth) {
					this.gRedBallon[i].transform.position = new Vector3(this.gRedBallon[i].transform.position.x, this.gRedBallon[i].transform.position.y + 0.04f,
					                                                    this.gRedBallon[i].transform.position.z);
				}
			}

			if(this.gBlueBallon[i].renderer.enabled) {
				if(this.gBlueBallon[i].transform.position.y < this.fMaxHeigth) {
					this.gBlueBallon[i].transform.position = new Vector3(this.gBlueBallon[i].transform.position.x, this.gBlueBallon[i].transform.position.y + 0.03f,
					                                                     this.gBlueBallon[i].transform.position.z);
				}
			}

			if(this.gYellowBallon[i].renderer.enabled) {
				if(this.gYellowBallon[i].transform.position.y < this.fMaxHeigth) {
					this.gYellowBallon[i].transform.position = new Vector3(this.gYellowBallon[i].transform.position.x, this.gYellowBallon[i].transform.position.y + 0.02f,
					                                                       this.gYellowBallon[i].transform.position.z);
				}
			}
		}
	}


	void CheckBallon()
	{
		Vector3 playerPos = this.GOplayer.transform.position;

		for(int i = 0; i < this.iBallonNumber; ++i)
		{
			if(this.gRedBallon[i].renderer.enabled) {
				if((playerPos.x > this.gRedBallon[i].transform.position.x - this.fScale * 0.5f && playerPos.x < this.gRedBallon[i].transform.position.x + this.fScale * 0.5f) &&
				   (playerPos.y > this.gRedBallon[i].transform.position.y - this.fScale * 0.5f && playerPos.y < this.gRedBallon[i].transform.position.y + this.fScale * 0.5f) &&
				   (playerPos.z > this.gRedBallon[i].transform.position.z - this.fScale * 0.5f && playerPos.z < this.gRedBallon[i].transform.position.z + this.fScale * 0.5f)) {
					this.gRedBallon[i].renderer.enabled = false;
					this.iScore += 300;

					this.ScoreUp();
				}
			}

			if(this.gYellowBallon[i].renderer.enabled) {
				if((playerPos.x > this.gYellowBallon[i].transform.position.x - this.fScale * 0.5f && playerPos.x < this.gYellowBallon[i].transform.position.x + this.fScale * 0.5f) &&
				   (playerPos.y > this.gYellowBallon[i].transform.position.y - this.fScale * 0.5f && playerPos.y < this.gYellowBallon[i].transform.position.y + this.fScale * 0.5f) &&
				   (playerPos.z > this.gYellowBallon[i].transform.position.z - this.fScale * 0.5f && playerPos.z < this.gYellowBallon[i].transform.position.z + this.fScale * 0.5f)) {
					this.gYellowBallon[i].renderer.enabled = false;
					this.iScore += 200;

					this.ScoreUp();
				}
			}

			if(this.gBlueBallon[i].renderer.enabled) {
				if((playerPos.x > this.gBlueBallon[i].transform.position.x - this.fScale * 0.5f && playerPos.x < this.gBlueBallon[i].transform.position.x + this.fScale * 0.5f) &&
				   (playerPos.y > this.gBlueBallon[i].transform.position.y - this.fScale * 0.5f && playerPos.y < this.gBlueBallon[i].transform.position.y + this.fScale * 0.5f) &&
				   (playerPos.z > this.gBlueBallon[i].transform.position.z - this.fScale * 0.5f && playerPos.z < this.gBlueBallon[i].transform.position.z + this.fScale * 0.5f)) {
					this.gBlueBallon[i].renderer.enabled = false;
					this.iScore += 100;

					this.ScoreUp();
				}
			}
		}

		if(this.iScore >= this.iBallonNumber * 600) this.ClearGame();
	}


	void ScoreUp()
	{
		Debug.Log(this.iScore);
	}


	void ClearGame()
	{
		Debug.Log("Congratulation!!");
	}
}
