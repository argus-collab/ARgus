using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialManager : MonoBehaviour
{
    int indexViews = 0;
    bool tutoDone = false;

    public List<GameObject> views;

    public int minIndexPrevious = 1;
    public int indexLastTuto;
    public Text tutoIndicator;

    public List<GameObject> desactivateOnNextStep;


    public bool IsTutoDone()
    {
        return tutoDone;
    }

    public void StartViews()
    {
        indexViews = 0;
        UpdateView();
    }

    public void HideAll()
    {
        for (int i = 0; i < views.Count; ++i)
            views[i].SetActive(false);
    }

    public void ResetTutoDone()
    {
        tutoIndicator.gameObject.SetActive(true);
        tutoDone = false;
    }

    public void SetTutoDone()
    {
        for (int i = 0; i < desactivateOnNextStep.Count; ++i)
            desactivateOnNextStep[i].SetActive(false);

        indexViews = views.Count;
        UpdateView();

        tutoIndicator.gameObject.SetActive(false);
        tutoDone = true;
    }

    void UpdateView()
    {
        if (!tutoDone && indexViews < views.Count)
            tutoIndicator.text = (indexViews + 1) + " / " + views.Count;

        for (int i = 0; i < views.Count; ++i)
        {
            if (i == indexViews)
                views[i].SetActive(true);
            else
                views[i].SetActive(false);
        }
    }

    public void GoNextView()
    {
        indexViews++;

        for (int i = 0; i < desactivateOnNextStep.Count; ++i)
            desactivateOnNextStep[i].SetActive(false);

        UpdateView();
    }

    public void GoPreviousView()
    {
        indexViews = indexViews > minIndexPrevious ? indexViews - 1 : minIndexPrevious;

        for (int i = 0; i < desactivateOnNextStep.Count; ++i)
            desactivateOnNextStep[i].SetActive(false);

        UpdateView();
    }

}
