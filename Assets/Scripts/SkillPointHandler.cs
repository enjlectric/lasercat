using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPointHandler : MonoBehaviour
{
    public List<Image> skillIcons;

    private int _value;

    public void SetValue(int value)
    {
        _value = Mathf.Clamp(value, 0, skillIcons.Count);

        int i = 0;
        foreach (Image img in skillIcons)
        {
            img.color = i < _value ? Manager.instance.refs.colorOrange : Manager.instance.refs.colorGrey;
            i++;
        }
    }

    public void IncrementValue()
    {
        if (_value < skillIcons.Count && Manager.instance.GetUnassignedSkillPoints() > 0)
        {
            SetValue(_value + 1);
            Manager.instance.UpdateUnassignedSkillPoints(-1);
        }
    }

    public void DecrementValue()
    {
        if (_value > 0)
        {
            SetValue(_value - 1);
            Manager.instance.UpdateUnassignedSkillPoints(1);
        }
    }

    public int GetValue()
    {
        return _value;
    }
}