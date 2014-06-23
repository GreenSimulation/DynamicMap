using UnityEngine;
using System.Collections;

public class DynamicMap : MonoBehaviour {
	MakeTerrain TERRAIN; //지형 클래스

	private int iZoom = 14; //텍스쳐의 줌 레벨

	//맵이 움직인 횟수
	private int iMovingX = 0;
	private int iMovingY = 0;

	//맵의 움직임에 따른 타일 인덱스의 움직임 변수
	private int iNumMovingUp = 0;
	private int iNumMovingRight = 0;

	public Camera camera; //카메라 오브젝트

	enum MOVING { UP, DOWN, RIGHT, LEFT }; //맵의 움직임을 제어하는 변수

	// Use this for initialization
	void Start () {
		this.Init();	

		StartCoroutine(this.CheckTileMoving());
	}
	
	// Update is called once per frame
	void Update () {
//		this.CheckTileMoving();
	}

	//초기화
	void Init() {
		//지형 클래스 초기화
		TERRAIN = this.gameObject.GetComponent<MakeTerrain>();
		TERRAIN.Init(this.iZoom);

		//카메라 위치 초기화(맵 가운데)
		this.camera.transform.position = new Vector3((TERRAIN.TileX / 2) * 80 * TERRAIN.Scale, 32 * TERRAIN.Scale,
		                                             (TERRAIN.TileY / 2) * 80 * TERRAIN.Scale);
	}


	//카메라의 위치에 따라 맵의 이동 방향을 결정하는 함수
	IEnumerator CheckTileMoving()
	{
		while(true) {

			if(Input.GetKey("q")) {
				this.camera.GetComponent<FlightCamera>().m_fSpeed += 50 * TERRAIN.Scale;
			}
			else if(Input.GetKey("e")) {
				this.camera.GetComponent<FlightCamera>().m_fSpeed -= 50 * TERRAIN.Scale;
			}
			
			//		if(Input.GetKeyDown("1")) {
			//			this.ControlTile((int) MOVING.UP);
			//		}
			//		if(Input.GetKeyDown("2")) {
			//			this.ControlTile((int) MOVING.DOWN);
			//		}
			//		if(Input.GetKeyDown("3")) {
			//			this.ControlTile((int) MOVING.RIGHT);
			//		}
			//		if(Input.GetKeyDown("4")) {
			//			this.ControlTile((int) MOVING.LEFT);
			//		}
			
			//카메라의 위치를 체크해서 일정 이상 위치에 다다르면 맵을 움직이고, 카메라 역시 그만큼 이동
			//동시에 이동하기 때문에 실제로는 계속 가는 것으로 보임
			if(this.camera.transform.position.z >= ((TERRAIN.TileY / 2 ) + 1) * 78 * TERRAIN.Scale)
			{
				this.camera.transform.position = new Vector3(this.camera.transform.position.x, this.camera.transform.position.y,
				                                             this.camera.transform.position.z - 78 * TERRAIN.Scale);
				
				Debug.Log("up");
				
				this.ControlTile((int) MOVING.UP);
			}
			
			if(this.camera.transform.position.z <= ((TERRAIN.TileY / 2 ) - 1) * 78 * TERRAIN.Scale)
			{
				this.camera.transform.position = new Vector3(this.camera.transform.position.x, this.camera.transform.position.y,
				                                             this.camera.transform.position.z + 78 * TERRAIN.Scale);
				
				Debug.Log("down");
				
				this.ControlTile((int) MOVING.DOWN);
			}
			
			if(this.camera.transform.position.x >= ((TERRAIN.TileX / 2 ) + 1) * 78 * TERRAIN.Scale)
			{
				this.camera.transform.position = new Vector3(this.camera.transform.position.x - 78 * TERRAIN.Scale, this.camera.transform.position.y,
				                                             this.camera.transform.position.z);
				
				Debug.Log("right");
				
				this.ControlTile((int) MOVING.RIGHT);
			}
			
			if(this.camera.transform.position.x <= ((TERRAIN.TileX / 2 ) - 1) * 78 * TERRAIN.Scale)
			{
				this.camera.transform.position = new Vector3(this.camera.transform.position.x + 78 * TERRAIN.Scale, this.camera.transform.position.y,
				                                             this.camera.transform.position.z);
				
				Debug.Log("left");
				
				this.ControlTile((int) MOVING.LEFT);
			}
			
			yield return null;
		}
	}


	//맵을 움직이는 함수
	//int move : 어디로 움직이는지 정하는 변수
	void ControlTile(int move)
	{
		int index = 0;
		int centerX = (int) (0.5 * TERRAIN.TileX - 2);
		int centerY = (int) (0.5 * TERRAIN.TileY - 2);
		int tempY = 0;
		int tempY2 = 0;
		int tempX = 0;
		int tempX2 = 0;

		switch(move)
		{
		case (int) MOVING.UP:
			this.iMovingY += 1;
			this.iNumMovingUp += 1;

			//인덱스의 움직임이 타일의 세로크기보다 크면 0으로 변환
			if(this.iNumMovingUp >= TERRAIN.TileY) this.iNumMovingUp = 0;

			//자식 타일 인덱스의 예외 처리
			tempY = centerY + 4 + (this.iNumMovingUp - 1);
			if(tempY >= TERRAIN.TileY) tempY -= TERRAIN.TileY;

			//자식 타일 동적 생성
			for(int i = 0; i < 4; ++i)
			{
				//x축 이동에 따른 인덱스 보정
				int centerX2 = centerX + i + this.iNumMovingRight;
				int posX = centerX2 - this.iNumMovingRight;
				if(centerX2 >= TERRAIN.TileX) centerX2 -= TERRAIN.TileX;

				//움직이는 반대방향의 자식 타일은 해제하고 움직이는 방향의 자식 타일 활성화
				TERRAIN.DisableChildTile(centerX2, centerY + this.iNumMovingUp - 1);
				TERRAIN.MakeNewChildTile(centerX + i + this.iMovingX, centerY + 4 + (this.iMovingY - 1), 
				                         centerX2, tempY, posX, 0, (int) MOVING.UP);
			}

			//맵 타일 동적 생성
			for(int x = 0; x < TERRAIN.TileX; ++x)
			{
				int tempNum = 0;

				//인덱스의 예외 처리(인덱스는 끝에 가서는 다시 원래되로 돌아와야 한다.) 예)0-1-2-3-0-1-2-3
				if(this.iNumMovingUp == 0) {
					index = this.iNumMovingRight + x + (TERRAIN.TileY - 1) * TERRAIN.TileX;
					tempNum = (TERRAIN.TileY - 1);
				}
				else {
					index = this.iNumMovingRight + x + (this.iNumMovingUp - 1) * TERRAIN.TileX;
					tempNum = this.iNumMovingUp - 1;
				}

				if(x >= TERRAIN.TileX - this.iNumMovingRight) index -= TERRAIN.TileX;

				//움직이는 반대 방향의 타일은 제거하고, 움직이는 방향의 타일을 생성
				TERRAIN.DestroyTile(index);
				TERRAIN.MakeNewTile(index, x, tempNum);
				TERRAIN.MakeNewField(this.iMovingX + x, this.iMovingY + TERRAIN.TileY - 1, index, x, TERRAIN.TileY);
			}

			//전체 타일을 움직이는 방향의 반대방향으로 이동 -> 타일의 위치는 제자리
			TERRAIN.MovingTerrain(0, 1);
			
			break;

		case (int) MOVING.DOWN:
			this.iMovingY -= 1;
			this.iNumMovingUp -= 1;

			//인덱스가 0보다 작으면 끝으로 이동
			if(this.iNumMovingUp < 0) this.iNumMovingUp = TERRAIN.TileY - 1;

			//자식 타일 인덱스의 예외 처리
			tempY = centerY + 4 - (TERRAIN.TileY - this.iNumMovingUp);
			if(tempY < 0) tempY += TERRAIN.TileY;

			//자식 타일 인덱스의 예외 처리2
			tempY2 = centerY - (TERRAIN.TileY - this.iNumMovingUp);
			if(tempY2 < 0) tempY2 += TERRAIN.TileY;

			//자식 타일 동적 생성
			for(int i = 0; i < 4; ++i)
			{
				//x축 이동에 따른 인덱스 보정
				int centerX2 = centerX + i + this.iNumMovingRight;
				int posX = centerX2 - this.iNumMovingRight;
				if(centerX2 >= TERRAIN.TileX) centerX2 -= TERRAIN.TileX;

				//움직이는 반대방향의 자식 타일은 해제하고 움직이는 방향의 자식 타일 활성화
				TERRAIN.DisableChildTile(centerX2, tempY);
				TERRAIN.MakeNewChildTile(centerX + i + this.iMovingX, centerY + this.iMovingY, 
				                         centerX2, tempY2, posX, 0, (int) MOVING.DOWN);
			}

			//맵 타일 동적 생성
			for(int x = 0; x < TERRAIN.TileX; ++x)
			{
				//인덱스의 예외 처리(인덱스는 끝에 가서는 다시 원래되로 돌아와야 한다.) 예)0-1-2-3-0-1-2-3
				index = this.iNumMovingRight + x + TERRAIN.TileX * this.iNumMovingUp;
				if(x >= TERRAIN.TileX - this.iNumMovingRight) index -= TERRAIN.TileX;
						
				//움직이는 반대 방향의 타일은 제거하고, 움직이는 방향의 타일을 생성
				TERRAIN.DestroyTile(index);
				TERRAIN.MakeNewTile(index, x, this.iNumMovingUp);
				TERRAIN.MakeNewField(this.iMovingX + x, this.iMovingY, index, x, -1);
			}

			//전체 타일을 움직이는 방향의 반대방향으로 이동 -> 타일의 위치는 제자리
			TERRAIN.MovingTerrain(0, -1);
			
			break;

		case (int) MOVING.RIGHT:
			this.iMovingX += 1;
			this.iNumMovingRight += 1;

			//인덱스가 타일의 가로크기보다 크면 처음으로 이동
			if(this.iNumMovingRight >= TERRAIN.TileX) this.iNumMovingRight = 0;

			//자식 타일 인덱스의 예외 처리
			tempX = centerX + 4 + (this.iNumMovingRight - 1);
			if(tempX >= TERRAIN.TileX) tempX -= TERRAIN.TileX;

			//자식 타일 동적 생성
			for(int i = 0; i < 4; ++i)
			{
				//y축 이동에 따른 인덱스 보정
				int centerY2 = centerY + i + this.iNumMovingUp;
				int posY = centerY2 - this.iNumMovingUp;
				if(centerY2 >= TERRAIN.TileY) centerY2 -= TERRAIN.TileY;

				//움직이는 반대방향의 자식 타일은 해제하고 움직이는 방향의 자식 타일 활성화
				TERRAIN.DisableChildTile(centerX + (this.iNumMovingRight - 1), centerY2);
				TERRAIN.MakeNewChildTile(centerX + 4 + (this.iMovingX - 1), centerY + i + this.iMovingY, 
				                         tempX, centerY2, 0, posY, (int) MOVING.RIGHT);
			}

			//맵 타일 동적 생성
			for(int y = 0; y < TERRAIN.TileY; ++y)
			{
				int tempNum = 0;

				//인덱스의 예외 처리(인덱스는 끝에 가서는 다시 원래되로 돌아와야 한다.) 예)0-1-2-3-0-1-2-3
				if(this.iNumMovingRight == 0) {
					index = (this.iNumMovingUp * TERRAIN.TileX) + y * TERRAIN.TileX + (TERRAIN.TileX - 1);
					tempNum = (TERRAIN.TileX - 1);
				}
				else {
					index = (this.iNumMovingUp * TERRAIN.TileX) + y * TERRAIN.TileX + (this.iNumMovingRight - 1);
					tempNum = this.iNumMovingRight - 1;
				}

				if(y >= TERRAIN.TileY - this.iNumMovingUp) index -= TERRAIN.TileX * TERRAIN.TileY;

				//움직이는 반대 방향의 타일은 제거하고, 움직이는 방향의 타일을 생성
				TERRAIN.DestroyTile(index);
				TERRAIN.MakeNewTile(index, tempNum, y);
				TERRAIN.MakeNewField(this.iMovingX + TERRAIN.TileY - 1, this.iMovingY + y, index, TERRAIN.TileX, y);
			}

			//전체 타일을 움직이는 방향의 반대방향으로 이동 -> 타일의 위치는 제자리
			TERRAIN.MovingTerrain(1, 0);
			
			break;

		case (int) MOVING.LEFT:
			this.iMovingX -= 1;
			this.iNumMovingRight -= 1;

			//인덱스가 0보다 작으면 끝으로 이동
			if(this.iNumMovingRight < 0) this.iNumMovingRight = TERRAIN.TileX - 1;

			//자식 타일 인덱스의 예외 처리
			tempX = centerX + 4 - (TERRAIN.TileX - this.iNumMovingRight);
			if(tempX < 0) tempX += TERRAIN.TileX;

			//자식 타일 인덱스의 예외 처리2
			tempX2 = centerX - (TERRAIN.TileX - this.iNumMovingRight);
			if(tempX2 < 0) tempX2 += TERRAIN.TileX;

			//자식 타일 동적 생성
			for(int i = 0; i < 4; ++i)
			{
				//y축 이동에 따른 인덱스 보정
				int centerY2 = centerY + i + this.iNumMovingUp;
				int posY = centerY2 - this.iNumMovingUp;
				if(centerY2 >= TERRAIN.TileY) centerY2 -= TERRAIN.TileY;

				//움직이는 반대방향의 자식 타일은 해제하고 움직이는 방향의 자식 타일 활성화
				TERRAIN.DisableChildTile(tempX, centerY2);
				TERRAIN.MakeNewChildTile(centerX + this.iMovingX, centerY + i + this.iMovingY,
				                         tempX2, centerY2, 0, posY, (int) MOVING.LEFT);
			}

			//맵 타일 동적 생성
			for(int y = 0; y < TERRAIN.TileY; ++y)
			{
				index = this.iNumMovingUp * TERRAIN.TileX + y * TERRAIN.TileY + this.iNumMovingRight;

				//인덱스의 예외 처리(인덱스는 끝에 가서는 다시 원래되로 돌아와야 한다.) 예)0-1-2-3-0-1-2-3
				if(y >= TERRAIN.TileY - this.iNumMovingUp) index -= TERRAIN.TileY * TERRAIN.TileX;
								
				//움직이는 반대 방향의 타일은 제거하고, 움직이는 방향의 타일을 생성
				TERRAIN.DestroyTile(index);
				TERRAIN.MakeNewTile(index, this.iNumMovingRight, y);
				TERRAIN.MakeNewField(this.iMovingX, this.iMovingY + y, index, -1, y);
			}

			//전체 타일을 움직이는 방향의 반대방향으로 이동 -> 타일의 위치는 제자리
			TERRAIN.MovingTerrain(-1, 0);
			
			break;
		}
	}
}
