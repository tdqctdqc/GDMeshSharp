using Godot;
using System;

public class ControlPanel : Panel
{
    protected Container _container;
    private void Resize()
    {
        var containerSize = _container.RectSize;
        var panelSize = containerSize * 1.2f;
        RectMinSize = panelSize;
        RectSize = panelSize;
        var middleX = (RectSize.x - containerSize.x) / 2f;
        var middleY = (RectSize.y - containerSize.y) / 2f;
        _container.RectPosition = new Vector2(middleX, middleY);
    }
    private void AddButton(string name, Action action)
    {
        var button = new ActionButton();
        button.Setup(name, action);
        _container.AddChild(button);
        Resize();
    }
    private void AddSlider(Action<float> action, Func<float, string> labelFunc,
        int min, int max)
    {
        var slider = SliderEntry.GetSliderEntry(
            labelFunc,
            action,
            min, max);
        _container.AddChild(slider);
        Resize();
    }
    private void AddCheckBox(string name, Action<bool> toggleAction)
    {
        var check = CheckBoxEntry.GetCheckBoxEntry(name, toggleAction);
        _container.AddChild(check);
        Resize();
    }
}
