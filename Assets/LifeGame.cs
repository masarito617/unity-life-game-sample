using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// ライフゲーム
/// </summary>
public class LifeGame : MonoBehaviour
{
    /// <summary>
    /// 生成するセル数
    /// </summary>
    [SerializeField] private int widthCount = 32;
    [SerializeField] private int heightCount = 32;
    
    /// <summary>
    /// カメラに収まる基準セル数
    /// </summary>
    private const int BaseWidthCount = 16;
    private const int BaseHeightCount = 16;

    /// <summary>
    /// セルの状態
    /// </summary>
    private enum CellStatus
    {
        Dead = 0, // 死亡
        Life = 1, // 生存
    }
    
    /// <summary>
    /// セル情報
    /// </summary>
    private CellStatus[] _cells; // セル配列
    private MeshRenderer[] _cellMeshRenderers; // セルが持つMeshRenderer配列
    private float _cellScale = 1.0f; // セルの大きさ

    /// <summary>
    /// マテリアル
    /// </summary>
    private Material _lifeMaterial;
    private Material _deadMaterial;

    private void Awake()
    {
        // マテリアル生成
        _lifeMaterial = new Material(Shader.Find("Unlit/Color"));
        _lifeMaterial.color = Color.white;
        _deadMaterial = new Material(Shader.Find("Unlit/Color"));
        _deadMaterial.color = Color.black;
        
        // カメラに収まるようスケールを調整
        _cellScale = Mathf.Min((float) BaseWidthCount / widthCount, (float) BaseHeightCount / heightCount);
        
        // セル生成
        _cells = new CellStatus[widthCount * heightCount];
        _cellMeshRenderers = new MeshRenderer[widthCount * heightCount];
        for (var i = 0; i < widthCount * heightCount; i++)
        {
            var x = i % widthCount;
            var y = i / widthCount;
            
            // セルをCubeオブジェクトとして生成
            var cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.transform.parent = gameObject.transform;
            cell.transform.localScale = _cellScale * Vector3.one;
            cell.transform.localPosition = _cellScale * new Vector3(
                x - (widthCount / 2.0f - _cellScale / 2.0f),
                y - (heightCount / 2.0f - _cellScale / 2.0f),
                0);

            // 初回は死亡している状態で設定
            _cells[i] = CellStatus.Dead;
            
            // MeshRendererも保持しておく
            var meshRenderer = cell.gameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _deadMaterial;
            _cellMeshRenderers[i] = meshRenderer;
            
        }
    }

    /// <summary>
    /// 次の世帯
    /// </summary>
    public void NextLife()
    {
        // セル更新処理
        var nextCells = new CellStatus[widthCount * heightCount];
        for (var i = 0; i < widthCount * heightCount; i++)
        {
            var x = i % widthCount;
            var y = i / widthCount;
            
            // 隣接するセルのindexを取得
            var xl = x == 0 ? widthCount - 1 : x - 1;  // 左
            var xr = x == widthCount - 1 ? 0 : x + 1;  // 右
            var yt = y == 0 ? heightCount - 1 : y - 1; // 上
            var yb = y == heightCount - 1 ? 0 : y + 1; // 下
            
            // 隣接するセルの生存数をカウント
            var lifeCount = 0; 
            if (IsLifeCell(xl + yt * widthCount)) lifeCount++; // 左上
            if (IsLifeCell(x  + yt * widthCount)) lifeCount++; // 上
            if (IsLifeCell(xr + yt * widthCount)) lifeCount++; // 右上
            if (IsLifeCell(xl + y  * widthCount)) lifeCount++; // 左
            if (IsLifeCell(xr + y  * widthCount)) lifeCount++; // 右
            if (IsLifeCell(xl + yb * widthCount)) lifeCount++; // 左下
            if (IsLifeCell(x  + yb * widthCount)) lifeCount++; // 下
            if (IsLifeCell(xr + yb * widthCount)) lifeCount++; // 右下
            
            // 生存判定
            var isLife = false;
            if (!IsLifeCell(i) && lifeCount == 3)
            {
                // 死亡セルに隣接せる生存セルが3つあれば誕生
                isLife= true;
            }
            else if (IsLifeCell(i) && (lifeCount == 3 || lifeCount == 2))
            {
                // 生存セルに隣接する生存セルが2つか3つならば次の世代でも生存
                isLife= true;
            }
            nextCells[i] = isLife ? CellStatus.Life : CellStatus.Dead;
        }
        
        // 次の世帯のセルに更新
        _cells = nextCells;
        
        // 再描画
        UpdateDrawMesh();
    }
    
    /// <summary>
    /// 生存セルか？
    /// </summary>
    private bool IsLifeCell(int index)
    {
        return _cells[index] == CellStatus.Life;
    }
    
    /// <summary>
    /// メッシュ再描画
    /// </summary>
    private void UpdateDrawMesh()
    {
        for (var i = 0; i < widthCount * heightCount; i++)
        {
            _cellMeshRenderers[i].sharedMaterial = _cells[i] == CellStatus.Life ? _lifeMaterial : _deadMaterial;
        }
    }
    
    /// <summary>
    /// MeshRenderer配列を返却する
    /// </summary>
    public MeshRenderer[] GetCellMeshRenderers()
    {
        return _cellMeshRenderers;
    }
    
    // ----- 以下はボタン押下時の処理 -----

    /// <summary>
    /// ループ中か？
    /// </summary>
    private bool _isLoop = false;
    
    /// <summary>
    /// ループ開始
    /// </summary>
    public void LifeLoopStart()
    {
        if (_isLoop) return;
        StartCoroutine(LifeLoopCoroutine());
    }
    
    /// <summary>
    /// ループ停止
    /// </summary>
    public void LifeLoopStop()
    {
        _isLoop = false;
    }
    
    /// <summary>
    /// ループコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator LifeLoopCoroutine()
    {
        // ループ停止されるまで一定間隔で次の世帯を呼び出す
        _isLoop = true;
        while (_isLoop)
        {
            yield return new WaitForSeconds(0.1f);
            NextLife();
        }
    }
    
    /// <summary>
    /// セルのリセット
    /// </summary>
    public void ResetCell()
    {
        // セルをクリア
        _cells = new CellStatus[widthCount * heightCount];
        
        // 再描画
        UpdateDrawMesh();
    }
    
    /// <summary>
    /// セルをランダムに生成する
    /// </summary>
    public void GenerateRandomCell()
    {
        // 一定確率で命を与える
        for (var i = 0; i < widthCount * heightCount; i++)
        {
            _cells[i] = Random.Range(0, 100) < 12 ? CellStatus.Life : CellStatus.Dead;
        }
        
        // 再描画
        UpdateDrawMesh();
    }
    
    /// <summary>
    /// サンプル生成
    /// </summary>
    public void GenerateSample()
    {
        // サンプルセル取得
        var sampleCell = LifeCellSample.GetSampleDrawCell();
        
        // 一辺あたりのセル数を取得
        // ※両辺の個数が同じである前提
        var edgeCellCount = Mathf.RoundToInt(Mathf.Sqrt(sampleCell.Length));
        if (edgeCellCount > widthCount || edgeCellCount > heightCount)
        {
            Debug.LogError("not enough cell count!!");
            return;
        }
        
        // 中央に設定されるようオフセットを設定
        var offsetX = widthCount / 2 - edgeCellCount / 2;
        var offsetY = heightCount / 2 - edgeCellCount / 2;

        // サンプルで用意されたCellを生成
        for (var i = 0; i < sampleCell.Length; i++)
        {
            var x = i % edgeCellCount + offsetX;
            var y = heightCount - (i / edgeCellCount + offsetY); // 下からのカウントになっていたので上下反転
            _cells[x + y * widthCount] = (CellStatus) sampleCell[i];
        }
        
        // 再描画
        UpdateDrawMesh();
    }
}
