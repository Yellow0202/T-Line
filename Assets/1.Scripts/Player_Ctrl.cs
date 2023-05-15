using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Ctrl : MonoBehaviour
{
    CharacterController Char_Cotroller = null;

    //--- 이동 관련 변수
    float h = 0.0f;     //플레이어 좌우
    float v = 0.0f;     //플레이어 상하

    [SerializeField] private float m_MoveSpeed = 0.0f;
    
    Vector3 Move_Dir = Vector3.zero;
    //--- 이동 관련 변수

    //--- 점프 관련 변수
    [SerializeField] private float m_JumpPower = 0.0f;
    [SerializeField] private float m_gravity = 0.0f;
    private int m_JumpCount = 2;

    float _y; // dir에 들어갈 y값 관리 임시변수
    //--- 점프 관련 변수

    //--- 슬라이딩 관련 변수
    [SerializeField] private float m_Slidspeed = 0.0f;
    [SerializeField] private float m_SlidTime = 0.0f;
    float m_CurTime = 0.0f;
    bool is_Sliding = false;

    //--- 슬라이딩 관련 변수

    //--- 애니 관련 변수
    [SerializeField] private Animator m_animator;
    bool is_Ground = false;
    //--- 애니 관련 변수



    // Start is called before the first frame update
    void Start()
    {
        Char_Cotroller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        // 바닥이 없을 때만
        if (!IsCheckGrounded())
        {
            // 중력
            _y += m_gravity * Time.deltaTime * Physics.gravity.y;
            is_Ground = false;
        }
        // 바닥이 있을 때 초기화
        else
        {
            m_JumpCount = 2;
            is_Ground = true;
        }

        Player_Jump();
        Dash_Update();
        Attack_Update();
        Move_Update();
        Animation_Update();
    }

    private void Move_Update()
    {
        if (is_Sliding == true)
            return;

        if (0.0f < h)
            transform.localScale = new Vector3(0.5f, transform.localScale.y, transform.localScale.z);
        else if (h < 0.0f)
            transform.localScale = new Vector3(-0.5f, transform.localScale.y, transform.localScale.z);

        // 이동할 방향
        Move_Dir = new Vector3(h, 0, v);

        // 정규화 (=방향만 남기기)
        Move_Dir.Normalize(); //dir = dir.normalized;

        // 플레이어가 바라보는 방향을 기준으로
        Move_Dir = transform.TransformDirection(Move_Dir);

        // 변경된 y값 할당
        Move_Dir.y = _y;

        // 키보드가 눌린대로 이동
        Char_Cotroller.Move(Move_Dir * m_MoveSpeed * Time.deltaTime);

    }

    private void Player_Jump()
    {
        if (is_Sliding == true)
            return;

        if (Input.GetButtonDown("Jump") && 0 < m_JumpCount)
        {
            m_JumpCount--;
            _y = m_JumpPower;
        }
    }

    private void Dash_Update()
    {
        if (!Input.GetKeyDown(KeyCode.LeftShift))
            return;

        if(is_Sliding != true)
            is_Sliding = true;

        StartCoroutine(Co_Dash());
    }

    IEnumerator Co_Dash()
    {
        m_CurTime = Time.time;

        while(Time.time < m_CurTime + m_SlidTime)
        {
            m_animator.SetBool("Slide", true);

            Char_Cotroller.Move(Move_Dir * m_Slidspeed * Time.deltaTime);
            yield return null;
        }

        is_Sliding = false;
        yield return null;
    }

    private void Attack_Update()
    {
        if (is_Sliding == true)
            return;

        if (!Input.GetKeyDown(KeyCode.Z))
            return;

        m_animator.SetBool("IsAttack", true);
    }

    #region --------------- 애니메이션

    private void Animation_Update()
    {
        //m_animator.SetFloat("AirSpeedY", _y);

        if (h != 0 || v != 0)
            m_animator.SetInteger("AnimState", 1);
        else
            m_animator.SetInteger("AnimState", 0);

    }
    #endregion

    private bool IsCheckGrounded()
    {
        // CharacterController.IsGrounded가 true라면 Raycast를 사용하지 않고 판정 종료
        if (Char_Cotroller.isGrounded) return true;
        // 발사하는 광선의 초기 위치와 방향
        // 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
        var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        // 탐색 거리
        var maxDistance = -0.1f;
        // 광선 디버그 용도
        Debug.DrawRay(transform.position + Vector3.up, Vector3.down * maxDistance, Color.red);
        // Raycast의 hit 여부로 판정
        // 지상에만 충돌로 레이어를 지정
        return Physics.Raycast(ray, maxDistance);
    }
}
