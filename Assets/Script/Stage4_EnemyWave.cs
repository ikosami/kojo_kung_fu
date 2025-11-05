using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージ4の敵ウェーブ管理システム
/// 
/// 機能概要:
/// - ステージデータに基づいて敵を段階的にスポーン（ウェーブシステム）
/// - 現在のウェーブの敵をすべて倒すと次のウェーブが自動開始
/// - 各ウェーブ内での敵のスポーンタイミングと位置を制御
/// - ゲームの進行状態（現在のウェーブ、残敵数）を追跡・管理
/// 
/// 使用方法:
/// 1. StageManagerへの参照をInspectorで設定
/// 2. SaveDataManagerで現在のステージとエリアを設定
/// 3. Start時に自動的にステージデータが初期化され、ウェーブが開始される
/// </summary>
public class Stage4_EnemyWave : MonoBehaviour
{
    // ========================================
    // 定数
    // ========================================
    /// <summary>ゲーム開始から最初のウェーブが始まるまでの遅延時間（秒）</summary>
    private const float INITIAL_DELAY = 1f;

    // ========================================
    // 参照
    // ========================================
    /// <summary>敵のスポーンを実際に実行するStageManager</summary>
    [SerializeField]private StageManager stageManager;
    
    /// <summary>現在のステージの全ウェーブデータのリスト（各ウェーブの敵構成を含む）</summary>
    private List<StageEntityData> stageDataList;

    // ========================================
    // 状態管理
    // ========================================
    /// <summary>現在実行中（または次に実行する）ウェーブのインデックス（0始まり）</summary>
    private int currentWaveIndex = 0;
    
    /// <summary>現在のウェーブでスポーンされた敵のリスト（倒されると削除される）</summary>
    private List<Enemy> currentEnemies = new List<Enemy>();
    
    /// <summary>現在ウェーブのスポーン処理中かどうか（スポーン中は次のウェーブ開始をブロック）</summary>
    private bool isSpawningWave = false;
    
    /// <summary>ゲーム開始からの経過時間を測定するタイマー（初回ウェーブの遅延に使用）</summary>
    private float initialTimer = 0f;
    
    /// <summary>最初のウェーブが開始されたかどうかのフラグ</summary>
    private bool hasStartedFirstWave = false;

    // ========================================
    // Unityライフサイクル
    // ========================================

    /// <summary>
    /// 初期化処理
    /// ステージデータを読み込み、ウェーブシステムを準備する
    /// </summary>
    void Start()
    {
        // ステージデータの初期化
        InitializeStageData();
    }

    /// <summary>
    /// フレーム毎の更新処理
    /// - ウェーブの完了状態をチェック
    /// </summary>
    void Update()
    {
        // ウェーブが開始されている場合のみ、完了チェックを行う
        if (hasStartedFirstWave)
        {
            // 現在のウェーブの敵がすべて倒されたかチェックし、次のウェーブへ進む
            CheckWaveCompletion();
        }
    }

    /// <summary>
    /// ウェーブシステムを開始する（外部から呼び出される）
    /// Stage4ChairのMoveIE完了後などから呼び出される
    /// </summary>
    public void StartWaveSystem()
    {
        if (!hasStartedFirstWave)
        {
            hasStartedFirstWave = true;
            StartCoroutine(StartNextWave());
            Debug.Log("ウェーブシステムを開始しました");
        }
    }

    // ========================================
    // 初期化
    // ========================================
    /// <summary>
    /// StageManagerと同じ方法でステージデータを取得・初期化
    /// 
    /// 処理の流れ:
    /// 1. Referenceシングルトンから現在のステージ番号に対応するデータを検索
    /// 2. ステージデータが見つかったら、ウェーブデータリストを取得
    /// 3. SaveDataManagerから現在のエリア（ウェーブ）番号を取得し、開始位置を設定
    /// 
    /// 注意: SaveDataManager.NowStageとNowAreaが正しく設定されている必要がある
    /// </summary>
    private void InitializeStageData()
    {
        // Referenceシングルトンから現在のステージのデータを検索
        var stageData = Reference.Instance.stageDataList.List.Find(x => x.StageNum == SaveDataManager.NowStage);
        if (stageData != null)
        {
            // ウェーブデータリストを取得
            stageDataList = stageData.stageDataList;
            // セーブデータから現在のエリア（ウェーブ）インデックスを設定
            currentWaveIndex = SaveDataManager.NowArea;
            Debug.Log($"ステージデータを初期化しました。ウェーブ数: {stageDataList.Count}");
        }
        else
        {
            Debug.LogError($"ステージ {SaveDataManager.NowStage} のデータが見つかりません！");
        }
    }

    // ========================================
    // ウェーブ管理
    // ========================================
    /// <summary>
    /// ウェーブの完了状態をチェックし、次のウェーブを開始
    /// 
    /// 次のウェーブが開始される条件:
    /// 1. 現在スポーン処理中でない（isSpawningWave == false）
    /// 2. 現在のウェーブの敵がすべて倒されている（currentEnemies.Count == 0）
    /// 3. まだ実行していないウェーブが残っている（currentWaveIndex < stageDataList.Count）
    /// 
    /// この3つの条件がすべて満たされた時、StartNextWave()コルーチンを開始
    /// </summary>
    private void CheckWaveCompletion()
    {
        Debug.Log("CheckWaveCompletion" + currentEnemies.Count);
        // 全条件を満たしている場合のみ次のウェーブを開始
        if (!isSpawningWave && currentEnemies.Count == 0 && currentWaveIndex < stageDataList.Count)
        {
            StartCoroutine(StartNextWave());
        }
    }

    /// <summary>
    /// 次のウェーブを開始するコルーチン
    /// 
    /// 処理の流れ:
    /// 1. ウェーブインデックスが範囲内かチェック
    /// 2. スポーン中フラグを立てる（重複実行を防ぐ）
    /// 3. 現在のウェーブの敵データを取得
    /// 4. 各敵をSpawnTime（ウェーブ開始からの経過時間）に基づいてスポーン
    /// 5. 全敵のスポーン完了後、フラグを下ろしてウェーブインデックスを進める
    /// 
    /// 注意: 
    /// - SpawnTimeはウェーブ開始からの絶対時間（例：1秒後、2秒後）
    /// - 同じSpawnTimeを持つ敵は同時にスポーンされる
    /// - スポーン完了後も敵が倒されるまで次のウェーブは開始しない（CheckWaveCompletionで制御）
    /// </summary>
    private IEnumerator StartNextWave()
    {
        // すべてのウェーブが完了している場合は処理を終了
        if (currentWaveIndex >= stageDataList.Count)
        {
            Debug.Log("すべてのウェーブが完了しました！");
            yield break;
        }

        // スポーン中フラグを立てる（次のウェーブ開始をブロック）
        isSpawningWave = true;
        var stageData = stageDataList[currentWaveIndex];

        Debug.Log($"ウェーブ {currentWaveIndex + 1} を開始します");

        // ウェーブ開始時刻を記録
        float waveStartTime = Time.time;
        
        // 敵データを取得
        var enemyDataList = Reference.Instance.enemyDataList.enemyDataList;
        
        // 各敵をスポーン時間順にソート
        var sortedEnemies = new List<PopData>(stageData.popEnemy);
        sortedEnemies.Sort((a, b) => a.SpawnTime.CompareTo(b.SpawnTime));
        
        // 敵を順次スポーン（ウェーブ開始からの経過時間で管理）
        foreach (var popEnemy in sortedEnemies)
        {
            // ウェーブ開始からの目標時間まで待機
            float targetTime = waveStartTime + popEnemy.SpawnTime;
            float waitTime = targetTime - Time.time;
            
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
            
            // 敵のインデックスから実際の敵データ（プレハブ等）を取得
            var enemyPopData = enemyDataList[popEnemy.EnemyIndex];
            // StageManagerに敵のスポーンを依頼（位置オフセット付き）
            var enemy = stageManager.SpawnEnemy(enemyPopData.prefab, popEnemy.SpanwOffset);
            currentEnemies.Add(enemy);
            enemy.OnDestroyed += OnEnemyDestroyed;
            
            Debug.Log($"敵をスポーンしました。経過時間: {Time.time - waveStartTime}秒");
        }

        // ウェーブのスポーン完了処理
        isSpawningWave = false;
        currentWaveIndex++;

        Debug.Log($"ウェーブ {currentWaveIndex} のスポーンが完了しました");
    }

    // ========================================
    // パブリックAPI（外部から呼び出される）
    // ========================================
    /// <summary>
    /// 敵が死亡した時に呼ばれるメソッド
    /// 
    /// Enemyクラスやダメージ処理から呼び出されることを想定
    /// currentEnemiesリストから敵を削除し、ウェーブの完了判定に使用される
    /// 
    /// パラメータ:
    /// - enemy: 倒された敵のインスタンス
    /// 
    /// 注意: 
    /// - リストに含まれていない敵（他のウェーブの敵など）は無視される
    /// - 全敵が倒されると、Update()のCheckWaveCompletion()で次のウェーブが開始される
    /// </summary>
    public void OnEnemyDestroyed(Enemy enemy)
    {
        if (currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(enemy);
            Debug.Log($"敵が倒されました。残り敵数: {currentEnemies.Count}");
        }
    }

    /// <summary>
    /// 現在のウェーブインデックスを取得
    /// 
    /// 戻り値: 現在のウェーブ番号（0始まり）
    /// 用途: UI表示、進行状況の保存などに使用
    /// </summary>
    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    /// <summary>
    /// 残りの敵の数を取得
    /// 
    /// 戻り値: 現在のウェーブで生存している敵の数
    /// 用途: UI表示（「残り○体」など）やゲームオーバー条件の判定に使用
    /// </summary>
    public int GetRemainingEnemies()
    {
        return currentEnemies.Count;
    }

    /// <summary>
    /// ウェーブがスポーン中かどうかを取得
    /// 
    /// 戻り値: true=スポーン処理実行中、false=待機中または完了
    /// 用途: スポーン中のUI表示や、特定の処理の制御に使用
    /// 
    /// 注意: スポーンが完了しても敵が全滅するまで次のウェーブは始まらない
    /// </summary>
    public bool IsWaveSpawning()
    {
        return isSpawningWave;
    }
}
