using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Ctrl : MonoBehaviour
{
    CharacterController Char_Cotroller = null;

    //--- �̵� ���� ����
    float h = 0.0f;     //�÷��̾� �¿�
    float v = 0.0f;     //�÷��̾� ����

    [SerializeField] private float m_MoveSpeed = 0.0f;
    
    Vector3 Move_Dir = Vector3.zero;
    //--- �̵� ���� ����

    //--- ���� ���� ����
    [SerializeField] private float m_JumpPower = 0.0f;
    [SerializeField] private float m_gravity = 0.0f;
    private int m_JumpCount = 2;

    float _y; // dir�� �� y�� ���� �ӽú���
    //--- ���� ���� ����

    //--- �����̵� ���� ����
    [SerializeField] private float m_Slidspeed = 0.0f;
    [SerializeField] private float m_SlidTime = 0.0f;
    float m_CurTime = 0.0f;
    bool is_Sliding = false;

    //--- �����̵� ���� ����

    //--- �ִ� ���� ����
    [SerializeField] private Animator m_animator;
    bool is_Ground = false;
    //--- �ִ� ���� ����



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

        // �ٴ��� ���� ����
        if (!IsCheckGrounded())
        {
            // �߷�
            _y += m_gravity * Time.deltaTime * Physics.gravity.y;
            is_Ground = false;
        }
        // �ٴ��� ���� �� �ʱ�ȭ
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

        // �̵��� ����
        Move_Dir = new Vector3(h, 0, v);

        // ����ȭ (=���⸸ �����)
        Move_Dir.Normalize(); //dir = dir.normalized;

        // �÷��̾ �ٶ󺸴� ������ ��������
        Move_Dir = transform.TransformDirection(Move_Dir);

        // ����� y�� �Ҵ�
        Move_Dir.y = _y;

        // Ű���尡 ������� �̵�
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

    #region --------------- �ִϸ��̼�

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
        // CharacterController.IsGrounded�� true��� Raycast�� ������� �ʰ� ���� ����
        if (Char_Cotroller.isGrounded) return true;
        // �߻��ϴ� ������ �ʱ� ��ġ�� ����
        // �ణ ��ü�� ���� �ִ� ��ġ�κ��� �߻����� ������ ����� ������ �� ���� ���� �ִ�.
        var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        // Ž�� �Ÿ�
        var maxDistance = -0.1f;
        // ���� ����� �뵵
        Debug.DrawRay(transform.position + Vector3.up, Vector3.down * maxDistance, Color.red);
        // Raycast�� hit ���η� ����
        // ���󿡸� �浹�� ���̾ ����
        return Physics.Raycast(ray, maxDistance);
    }
}
