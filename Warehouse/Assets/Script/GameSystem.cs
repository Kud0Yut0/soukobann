using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : MonoBehaviour
{
    [SerializeField]
    private StageManager _stageManager;

    private PlayerManager _player;

    // プレイヤーの移動
    enum DIRECTION
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    void Start()
    {
        _stageManager.LoadTileData();
        _stageManager.CreateStage();
        _player = _stageManager._player;
    }

    private void Update()
    {
        // プレイヤー入力処理
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {

            Moveto(DIRECTION.LEFT);

        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            Moveto(DIRECTION.RIGHT);

        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            Moveto(DIRECTION.UP);

        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {

            Moveto(DIRECTION.DOWN);

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            //シーンの再読み込みをして、ゲームの再スタート。
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        CheckClear();

    }

    void CheckClear()
    {

        if (_stageManager.Clear())
        {
            // シーンの再読み込みをして、ゲームの再スタート。
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    /// <summary>
    ///　プレイヤーの移動による座標の更新
    /// </summary>
    /// <param name="direction"></param>
    void Moveto(DIRECTION direction)
    {
        // プレイヤーを検索ワードにタイル状の位置情報検索
        // 現在の位置を取得
        Vector2Int nowPlayerpos = _stageManager.moveObjpos[_player.gameObject];

        // 次の位置の取得
        Vector2Int nextPlayerpos = Nextpos(nowPlayerpos, direction);

        // 移動先が壁だったら
        if (_stageManager.nextWall(nextPlayerpos))
        {
            return;//　移動前に終了
        }

        // 移動先がブロックだったら
        if (_stageManager.nextBlock(nextPlayerpos))
        {

            // プレイヤーの二つ先に移動
            Vector2Int nextBlockPos = Nextpos(nextPlayerpos, direction);

            // ブロックの移動先が壁かどうか
            if (_stageManager.nextBlock(nextBlockPos) || _stageManager.nextWall(nextBlockPos))
            {
                return;
            }

            // ニマス先のブロックのの更新
            _stageManager.updateBlockpos(nextPlayerpos, nextBlockPos);
        }

        // プレイヤーの更新
        _stageManager.updatePlayerpos(nowPlayerpos, nextPlayerpos);

        // 次の位置に移動
        _player.Move(_stageManager.GetScreenPositionFromTileTable(nextPlayerpos));

        // タイル情報の更新
        _stageManager.moveObjpos[_player.gameObject] = nextPlayerpos;

    }
    /// <summary>
    /// 入力処理をまとめたもの
    /// </summary>
    /// <param name="currentPos"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    Vector2Int Nextpos(Vector2Int currentPos, DIRECTION direction)
    {
        switch (direction)
        {
            case DIRECTION.UP:
                return currentPos + Vector2Int.down;
            case DIRECTION.DOWN:
                return currentPos + Vector2Int.up;
            case DIRECTION.LEFT:
                return currentPos + Vector2Int.left;
            case DIRECTION.RIGHT:
                return currentPos + Vector2Int.right;
        }
        return currentPos;
    }

}

