using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    /*－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
      
     　StageManager.cs
     
     　作成日　10月10日
     　作成者　工藤優斗
     
     －－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－*/


    [SerializeField]
    private TextAsset _stageFile; //ステージのTEXTをいれる

    [SerializeField]
    private GameObject[] _prefabs;//生成するオブジェクト

    [SerializeField]
    private TextAsset[] _stages;//ステージの番号

    enum TILE_TYPE
    {
        WALL,　　　　　　　 // 壁
        GROUND,             // 地面
        BLOCK_POINT,　　　　// ゴール
        BLOCK,　　　　　　　// 動かすブロック
        PLAYER,　　　　　　　// プレイヤー
        POINT_ON_BLOCK,　　　// ゴールの上のブロック
        POINT_ON_PLAYER,　　　// ゴールの上のプレイヤー
    }
    TILE_TYPE[,] _tileTable;

    private float _tileSize;

    private Vector2 _centerPos;

    public PlayerManager _player;

    public int _blockcount;
    // 検索条件
    public Dictionary<GameObject, Vector2Int> moveObjpos = new Dictionary<GameObject, Vector2Int>();

    private void Awake()
    {
        // ランダム0～2の中から選ぶ
        int stagenumber = Random.Range(0, 3);

        // 何番のステージで遊ぶかえらぶ
        _stageFile = _stages[stagenumber];
    }

    public void LoadTileData()
    {
        // 空白を入れない　System.StringSplitOptions.RemoveEmptyEntries
        // textデータ上の改行で列を判断
        string[] lines = _stageFile.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        // コンマで区切り次の文字を読み込む
        int columns = lines[0].Split(new[] { ',' }).Length;

        // 配列の長さ格納
        int rows = lines.Length;

        // 読み込んだデータの番号を_tileTableに格納
        _tileTable = new TILE_TYPE[columns, rows];

        for (int y = 0; y < rows; y++)
        {
            // 指定された番号yを取り出して格納
            string[] values = lines[y].Split(new[] { ',' });

            for (int x = 0; x < columns; x++)
            {
                // 指定された番号xを取り出して格納
                _tileTable[x, y] = (TILE_TYPE)int.Parse(values[x]);

            }
        }
    }
    //タイルの生成
    public void CreateStage()
    {
        // ステージの大きさを図って中心の取得
        _tileSize = _prefabs[0].GetComponent<SpriteRenderer>().bounds.size.x;
        _centerPos.x = (_tileTable.GetLength(0) / 2) * _tileSize;
        _centerPos.y = (_tileTable.GetLength(1) / 2) * _tileSize;

        // 行
        for (int y = 0; y < _tileTable.GetLength(1); y++)
        {
            for (int x = 0; x < _tileTable.GetLength(0); x++)// 列
            {
                // 生成する場所
                Vector2Int position = new Vector2Int(x, y);

                // 背景の配置
                GameObject ground = Instantiate(_prefabs[(int)TILE_TYPE.GROUND]);//生成
                ground.transform.position = GetScreenPositionFromTileTable(position);//配置

                // タイルを配置する
                TILE_TYPE tileType = _tileTable[x, y];
                // 一致したブロックの生成
                GameObject obj = Instantiate(_prefabs[(int)tileType]);
                // 指定の場所に配置
                obj.transform.position = GetScreenPositionFromTileTable(position);

                //Playerだったら
                if (tileType == TILE_TYPE.PLAYER)
                {

                    _player = obj.GetComponent<PlayerManager>();

                    // Playerの座標格納
                    moveObjpos.Add(obj, position);

                }
                // ブロックだったら
                if (tileType == TILE_TYPE.BLOCK)
                {
                    _blockcount++;
                    // ブロックの座標格納
                    moveObjpos.Add(obj, position);
                }


            }
        }
    }

    /// <summary>
    /// 配列のブロック座標と実際の配置されたブロックの同期
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 GetScreenPositionFromTileTable(Vector2Int position)
    {
        // 反転の修正
        return new Vector2(position.x * _tileSize - _centerPos.x, -(position.y * _tileSize - _centerPos.y));
    }

    /// <summary>
    /// 壁だった時の判定
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool nextWall(Vector2Int pos)
    {
        // 次進むところが壁だったら
        if (_tileTable[pos.x, pos.y] == TILE_TYPE.WALL)
        {
            return true;
        }
        // 壁だったら
        return false;
    }

    /// <summary>
    /// 進めない場所
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool nextBlock(Vector2Int pos)
    {
        // 目の前がブロックだった時
        if (_tileTable[pos.x, pos.y] == TILE_TYPE.BLOCK)
        {
            return true;
        }
        // ニマス先がブロック
        if (_tileTable[pos.x, pos.y] == TILE_TYPE.POINT_ON_BLOCK)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// ぶつかったブロックを取得
    /// </summary>
    GameObject GetNextObj(Vector2Int pos)
    {
        foreach (var pair in moveObjpos) // pairにはObjと位置が入っている
        {
            if (pair.Value == pos)// 位置が一致されてることを確認
            {
                // returnで値をかえす
                return pair.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// ブロックを移動させる
    /// </summary>
    /// <param name="nowBlockpos"></param>
    /// <param name="nextBlockpos"></param>
    public void updateBlockpos(Vector2Int nowBlockpos, Vector2Int nextBlockpos)
    {
        // 今の座標取得
        GameObject block = GetNextObj(nowBlockpos);
        // タイル座標に変換して動かす
        block.transform.position = GetScreenPositionFromTileTable(nextBlockpos);
        // 位置情報の更新
        moveObjpos[block] = nextBlockpos;

        // tileTableの更新
        // 次にブロックがおかれる場所を指定のブロックにする

        if (_tileTable[nextBlockpos.x, nextBlockpos.y] == TILE_TYPE.BLOCK_POINT)
        {
            _tileTable[nextBlockpos.x, nextBlockpos.y] = TILE_TYPE.POINT_ON_BLOCK;
        }
        else
        {
            _tileTable[nextBlockpos.x, nextBlockpos.y] = TILE_TYPE.BLOCK;
        }


    }
    /// <summary>
    /// プレイヤーの一座標の更新
    /// </summary>
    /// <param name="nowPos"></param>
    /// <param name="nextPos"></param>
    public void updatePlayerpos(Vector2Int nowPos, Vector2Int nextPos)
    {
        // 次にプレイヤーが置かれる位置をプレイヤーとする
        // tileTable[nextPos.x, nextPos.y] = TILE_TYPE.PLAYER;
        // 現在の位置を地面とする(ひとつ前の位置)
        if (_tileTable[nextPos.x, nextPos.y] == TILE_TYPE.BLOCK_POINT)
        {
            // TILE_TYPEの変更
            _tileTable[nextPos.x, nextPos.y] = TILE_TYPE.POINT_ON_PLAYER;
        }
        else
        {
            // TILE_TYPEの変更
            _tileTable[nowPos.x, nowPos.y] = TILE_TYPE.PLAYER;
        }

        // 次にプレイヤーが置かれる位置をプレイヤーとする
        // tileTable[nextPos.x, nextPos.y] = TILE_TYPE.PLAYER;
        // 現在の位置を地面とする(ひとつ前の位置)
        if (_tileTable[nowPos.x, nowPos.y] == TILE_TYPE.POINT_ON_PLAYER)
        {
            // TILE_TYPEの変更
            _tileTable[nowPos.x, nowPos.y] = TILE_TYPE.BLOCK_POINT;
        }
        else
        {
            // TILE_TYPEの変更
            _tileTable[nowPos.x, nowPos.y] = TILE_TYPE.GROUND;
        }

    }
    /// <summary>
    /// ゴール判定
    /// </summary>
    /// <returns></returns>
    public bool Clear()
    {
        int clearcount = 0;

        // 配列上でゴールとブロックがかさなってる位置を検索
        for (int y = 0; y < _tileTable.GetLength(1); y++)
        {
            for (int x = 0; x < _tileTable.GetLength(0); x++)
            {
                // 一致したらカウント
                if (_tileTable[x, y] == TILE_TYPE.POINT_ON_BLOCK)
                {
                    clearcount++;

                }

            }
        }

        if (_blockcount == clearcount)
        {
            return true;
        }
        return false;
    }
}
