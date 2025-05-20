using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Onthesys;
using Unity.VisualScripting;
using Onthesys.ExeBuild; // IEnumerator를 사용하기 위해 추가

internal class ButtonWithPointAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    internal int id;
    internal List<GameObject> points;
    internal GameObject targetObject; // 애니메이션이 적용된 게임 오브젝트
    internal GameObject focus;
    private Animator animator;
    PanelController panelController;


    private bool isActiveFocus = false;

    private void Start()
    {
        panelController = GetComponentInParent<PanelController>();

        panelController.onHome.AddListener(this.OnHome);
        panelController.onSelectArea.AddListener(this.OnFocus);
        panelController.onAlarmUpdateMap.AddListener(this.OnUpdate);
        this.isActiveFocus = false;
    }

    private void OnFocus(int idx)
    {
        this.isActiveFocus = true;
        if (this.isActiveFocus)
        {
            this.focus.SetActive(idx == this.id);
        }
    }

    private void OnHome()
    {
        this.isActiveFocus = false;
        this.focus.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator != null)
        {
            animator.SetTrigger("Play"); // Play 트리거를 설정
        }
        this.focus.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator != null)
        {
            StartCoroutine(StopAnimationAfterDelay(1.0f)); // 1초 후에 Stop 트리거를 설정
        }
        this.focus.SetActive(false);
    }

    private IEnumerator StopAnimationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetTrigger("Stop"); // Stop 트리거를 설정
        }
    }

    internal void OnUpdate(Dictionary<int, AlarmCount> mapAlarms)
    {
        points.ForEach(point => 
        {
            point.gameObject.SetActive(false);
        });

        var ala = mapAlarms[id + 1];

        if (ala.GetRed() > 0)
        {
            this.points[1].gameObject.SetActive(true);
            this.targetObject = this.points[1].gameObject;
        }
        else if (ala.GetYellow() > 0)
        {
            this.points[2].gameObject.SetActive(true);
            this.targetObject = this.points[2].gameObject;
        }
        else
        {
            this.points[0].gameObject.SetActive(true);
            this.targetObject = this.points[0].gameObject;
        }

        animator = targetObject.GetComponent<Animator>();
    }

}
