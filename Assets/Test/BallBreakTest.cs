using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ボールでライフゲームを破壊して遊ぶ用
/// </summary>
public class BallBreakTest : MonoBehaviour
{
    /// <summary>
    /// ボール
    /// </summary>
    [SerializeField] private GameObject ballPrefab;
    
    private Vector3 _initBallPosition; // 初期位置
    private Vector3 _addVec;           // 加える力

    private void Start()
    {
        _initBallPosition = new Vector3(0.0f, -3.0f, -15.0f);
        _addVec = new Vector3(0.0f, 500.0f, 1000.0f);
        
        // ライフゲームのセルにRigidbodyを追加
        var lifeGame = GameObject.FindObjectOfType<LifeGame>();
        var meshRenderers = lifeGame.GetCellMeshRenderers();
        foreach (var meshRenderer in meshRenderers)
        {
            var rigidbody = meshRenderer.gameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.mass = 0.05f;
        }
        
    }

    private void Update()
    {
        // マウスクリックでボール発射
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            // 生成
            var ball = Instantiate(ballPrefab);
            ball.transform.localPosition = _initBallPosition;
            
            // 発射
            var rigidbody = ball.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.AddForce(_addVec);
            rigidbody.AddTorque(Vector3.up * -50000.0f);
            rigidbody.AddTorque(Vector3.right * 20000.0f);
            rigidbody.mass = 1.0f;
        }
    }
}
