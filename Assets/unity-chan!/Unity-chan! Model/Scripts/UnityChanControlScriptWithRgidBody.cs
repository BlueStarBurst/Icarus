﻿//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;
using System;

namespace UnityChan
{
    // 必要なコンポーネントの列記
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]

    public class UnityChanControlScriptWithRgidBody : MonoBehaviour
    {

        public float animSpeed = 1.5f;              // アニメーション再生速度設定
        public float lookSmoother = 3.0f;           // a smoothing setting for camera motion
        public bool useCurves = true;               // Mecanimでカーブ調整を使うか設定する
                                                    // このスイッチが入っていないとカーブは使われない
        public float useCurvesHeight = 0.5f;        // カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

        // 以下キャラクターコントローラ用パラメタ
        // 前進速度
        public float forwardSpeed = 7.0f;
        // 後退速度
        public float backwardSpeed = 2.0f;
        // 旋回速度
        public float rotateSpeed = 2.0f;
        // ジャンプ威力
        public float jumpPower = 3.0f;
        // キャラクターコントローラ（カプセルコライダ）の参照
        private CapsuleCollider col;
        private Rigidbody rb;
        // キャラクターコントローラ（カプセルコライダ）の移動量
        private Vector3 velocity;
        // CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
        private float orgColHight;
        private Vector3 orgVectColCenter;
        private Animator anim;                          // キャラにアタッチされるアニメーターへの参照
        private AnimatorStateInfo currentBaseState;         // base layerで使われる、アニメーターの現在の状態の参照

        private GameObject cameraObject;    // メインカメラへの参照

        // アニメーター各ステートへの参照
        static int idleState = Animator.StringToHash("Base Layer.Idle");
        static int locoState = Animator.StringToHash("Base Layer.Locomotion");
        static int jumpState = Animator.StringToHash("Base Layer.Jump");
        static int restState = Animator.StringToHash("Base Layer.Rest");

        public float hIn = 0.0f;
        public float vIn = 0.0f;

        public float minSpeed = 1f;
        public bool jumpIn = false;

        public Vector3 targetPosition = new Vector3(0, 0, 0);
        public float angleThreshold = 0.5f;
        public float distanceThreshold = 0.15f;

        public float decDistance = 0.5f;

        public float xBound = 10.0f;
        public float zBound = 10.0f;
        public float yBound = 10.0f;

        // 初期化
        void Start()
        {
            // Animatorコンポーネントを取得する
            anim = GetComponent<Animator>();
            // CapsuleColliderコンポーネントを取得する（カプセル型コリジョン）
            col = GetComponent<CapsuleCollider>();
            rb = GetComponent<Rigidbody>();
            //メインカメラを取得する
            cameraObject = GameObject.FindWithTag("MainCamera");
            // CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
            orgColHight = col.height;
            orgVectColCenter = col.center;
            targetPosition = transform.position;


        }

        float vf = 1.0f;
        void MoveTowardsTargetPosition(bool isRunning = false, bool isBackwards = false)
        {
            // this method is called every fixed update
            // this will turn the character towards the target position
            // and move it forward

            // get the direction to the target position
            Vector3 direction = targetPosition - transform.position;
            if (isBackwards)
            {
                direction = transform.position - targetPosition;
            }
            // get the angle to the target position
            float angle = Vector3.Angle(direction, transform.forward);
            // if the angle is greater than 1, then we need to turn
            if (angle > angleThreshold)
            {
                // get the rotation to the target position
                Quaternion rotation = Quaternion.LookRotation(direction);
                // rotate the character
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5.0f);
                // set anim direction to -1.0f or 1.0f
                anim.SetFloat("Speed", angle / 180.0f);
                anim.SetFloat("Direction", Mathf.Sign(direction.x));
                anim.speed = animSpeed;
            }
            else if ((transform.position - targetPosition).magnitude > distanceThreshold)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation, Time.deltaTime * 5.0f);
                // velocity based on distance to target position
                // speeds up to terminal velocity and slows down as it approaches
                if ((transform.position - targetPosition).magnitude < decDistance)
                {
                    vf -= 0.1f;
                }
                else if (Mathf.Abs(vf) < forwardSpeed)
                {
                    vf += 0.1f;
                }
                vf = Mathf.Clamp(vf, minSpeed, forwardSpeed);

                anim.SetFloat("Speed", vf + 0.5f);
                anim.SetFloat("Direction", 0.0f);
                anim.speed = animSpeed;
                // move the character forward lerp with velocity (vf)
                // transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * vf * (isBackwards ? -1.0f : 1.0f));
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * vf * (isBackwards ? -1.0f : 1.0f));
            }
            else
            {
                anim.SetFloat("Speed", 0.0f);
                anim.SetFloat("Direction", 0.0f);
                anim.speed = animSpeed;
            }
        }

        // 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
        void FixedUpdate()
        {
            targetPosition.y = transform.position.y;
            MoveTowardsTargetPosition();

            if (transform.position.x > xBound)
            {
                transform.position = transform.position + new Vector3(-2 * xBound, 0, 0);
                targetPosition = targetPosition + new Vector3(-2 * xBound, 0, 0);
            }
            if (transform.position.x < -xBound)
            {
                transform.position = transform.position + new Vector3(2 * xBound, 0, 0);
                targetPosition = targetPosition + new Vector3(2 * xBound, 0, 0);
            }

            if (transform.position.y > yBound)
            {
                transform.position = transform.position + new Vector3(0, -2 * yBound, 0);
                targetPosition = targetPosition + new Vector3(0, -2 * yBound, 0);
                if (transform.position.z > zBound)
                {
                    transform.position = transform.position + new Vector3(0, 0, -1 * zBound);
                    targetPosition = targetPosition + new Vector3(0, 0, -1 * zBound);
                }
                if (transform.position.z < -zBound)
                {
                    transform.position = transform.position + new Vector3(0, 0, 1 * zBound);
                    targetPosition = targetPosition + new Vector3(0, 0, 1 * zBound);
                }
            }
            if (transform.position.y < -yBound)
            {
                transform.position = transform.position + new Vector3(0, 2 * yBound, 0);
                targetPosition = targetPosition + new Vector3(0, 2 * yBound, 0);
                if (transform.position.z > zBound)
                {
                    transform.position = transform.position + new Vector3(0, 0, -1 * zBound);
                    targetPosition = targetPosition + new Vector3(0, 0, -1 * zBound);
                }
                if (transform.position.z < -zBound)
                {
                    transform.position = transform.position + new Vector3(0, 0, 1 * zBound);
                    targetPosition = targetPosition + new Vector3(0, 0, 1 * zBound);
                }
            }


            // float h = Input.GetAxis ("Horizontal");				// 入力デバイスの水平軸をhで定義
            // float v = Input.GetAxis ("Vertical");				// 入力デバイスの垂直軸をvで定義
            // float h = hIn;
            // float v = vIn;

            // anim.SetFloat("Speed", v);                          // Animator側で設定している"Speed"パラメタにvを渡す
            // anim.SetFloat("Direction", h);                      // Animator側で設定している"Direction"パラメタにhを渡す
            // anim.speed = animSpeed;                             // Animatorのモーション再生速度に animSpeedを設定する
            // currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
            // rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする



            // // 以下、キャラクターの移動処理
            // velocity = new Vector3(0, 0, v);        // 上下のキー入力からZ軸方向の移動量を取得
            //                                         // キャラクターのローカル空間での方向に変換
            // velocity = transform.TransformDirection(velocity);
            // //以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
            // if (v > 0.1)
            // {
            //     velocity *= forwardSpeed;       // 移動速度を掛ける
            // }
            // else if (v < -0.1)
            // {
            //     velocity *= backwardSpeed;  // 移動速度を掛ける
            // }

            if (jumpIn)
            {   // スペースキーを入力したら

                //アニメーションのステートがLocomotionの最中のみジャンプできる

                // rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
                jumpIn = false;
            }
            else
            {
                // set jump to false when the clip is not playing
                if (anim.GetAnimatorTransitionInfo(0).IsName("Idle -> Jump") || anim.GetAnimatorTransitionInfo(0).IsName("Locomotion -> Jump"))
                {
                    if (anim.GetBool("Jump") == true)
                    {
                        // wait 1 sec before rb add force
                        // yield return new WaitForSeconds(1.0f);
                        Invoke("WaitJump", 0.1f);

                        anim.SetBool("Jump", false);
                    }
                }
            }


            // 上下のキー入力でキャラクターを移動させる
            // transform.localPosition += velocity * Time.fixedDeltaTime;

            // 左右のキー入力でキャラクタをY軸で旋回させる
            // transform.Rotate(0, h * rotateSpeed, 0);


            // 以下、Animatorの各ステート中での処理
            // Locomotion中
            // 現在のベースレイヤーがlocoStateの時
            if (currentBaseState.nameHash == locoState)
            {
                //カーブでコライダ調整をしている時は、念のためにリセットする
                if (useCurves)
                {
                    resetCollider();
                }
            }
            // JUMP中の処理
            // 現在のベースレイヤーがjumpStateの時
            else if (currentBaseState.nameHash == jumpState)
            {
                cameraObject.SendMessage("setCameraPositionJumpView");  // ジャンプ中のカメラに変更
                                                                        // ステートがトランジション中でない場合
                if (!anim.IsInTransition(0))
                {

                    // 以下、カーブ調整をする場合の処理
                    if (useCurves)
                    {
                        // 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
                        // JumpHeight:JUMP00でのジャンプの高さ（0〜1）
                        // GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
                        float jumpHeight = anim.GetFloat("JumpHeight");
                        float gravityControl = anim.GetFloat("GravityControl");
                        if (gravityControl > 0)
                            rb.useGravity = false;  //ジャンプ中の重力の影響を切る

                        // レイキャストをキャラクターのセンターから落とす
                        Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
                        RaycastHit hitInfo = new RaycastHit();
                        // 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
                        if (Physics.Raycast(ray, out hitInfo))
                        {
                            if (hitInfo.distance > useCurvesHeight)
                            {
                                col.height = orgColHight - jumpHeight;          // 調整されたコライダーの高さ
                                float adjCenterY = orgVectColCenter.y + jumpHeight;
                                col.center = new Vector3(0, adjCenterY, 0); // 調整されたコライダーのセンター
                            }
                            else
                            {
                                // 閾値よりも低い時には初期値に戻す（念のため）					
                                resetCollider();
                            }
                        }
                    }
                    // Jump bool値をリセットする（ループしないようにする）				
                    anim.SetBool("Jump", false);
                }
            }
            // IDLE中の処理
            // 現在のベースレイヤーがidleStateの時
            else if (currentBaseState.nameHash == idleState)
            {
                //カーブでコライダ調整をしている時は、念のためにリセットする
                if (useCurves)
                {
                    resetCollider();
                }
                // スペースキーを入力したらRest状態になる
                if (Input.GetButtonDown("Jump"))
                {
                    anim.SetBool("Rest", true);
                }
            }
            // REST中の処理
            // 現在のベースレイヤーがrestStateの時
            else if (currentBaseState.nameHash == restState)
            {
                //cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
                // ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
                if (!anim.IsInTransition(0))
                {
                    anim.SetBool("Rest", false);
                }
            }
        }

        void WaitJump()
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
        }

        // void OnGUI ()
        // {
        // 	GUI.Box (new Rect (Screen.width - 260, 10, 250, 150), "Interaction");
        // 	GUI.Label (new Rect (Screen.width - 245, 30, 250, 30), "Up/Down Arrow : Go Forwald/Go Back");
        // 	GUI.Label (new Rect (Screen.width - 245, 50, 250, 30), "Left/Right Arrow : Turn Left/Turn Right");
        // 	GUI.Label (new Rect (Screen.width - 245, 70, 250, 30), "Hit Space key while Running : Jump");
        // 	GUI.Label (new Rect (Screen.width - 245, 90, 250, 30), "Hit Spase key while Stopping : Rest");
        // 	GUI.Label (new Rect (Screen.width - 245, 110, 250, 30), "Left Control : Front Camera");
        // 	GUI.Label (new Rect (Screen.width - 245, 130, 250, 30), "Alt : LookAt Camera");
        // }


        // キャラクターのコライダーサイズのリセット関数
        void resetCollider()
        {
            // コンポーネントのHeight、Centerの初期値を戻す
            col.height = orgColHight;
            col.center = orgVectColCenter;
        }



        public void Action(string verb, string direction, float quantity)
        {
            if (verb == "walk")
            {
                Walk(direction, quantity);
            }
            else if (verb == "jump")
            {
                jumpIn = true;
            }
        }

        void Walk(string direction, float quantity)
        {
            // set a new target position based on the direction and quantity
            if (direction == "forward")
            {
                targetPosition = transform.position + transform.forward * quantity;
            }
            else if (direction == "backward")
            {
                targetPosition = transform.position - transform.forward * quantity;
            }
            else if (direction == "left")
            {
                targetPosition = transform.position - transform.right * quantity;
            }
            else if (direction == "right")
            {
                targetPosition = transform.position + transform.right * quantity;
            }
        }
    }
}