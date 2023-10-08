using Godot;
using System;

public class InputStates
{
    [Export]
    public const int INPUT_RELEASED = 0;

    [Export]
    public const int INPUT_JUST_PRESSED = 1;

    [Export]
    public const int INPUT_PRESSED = 2;

    [Export]
    public const int INPUT_JUST_RELEASED = 3;

    private String _inputName;
    private bool input;
    private bool previusState;
    private bool currentState;
    private int outputState;
    private int oldState;

    public InputStates(String inputName) => _inputName = inputName;

    public int CheckState()
    {
        input = Input.IsActionJustPressed(_inputName);
        previusState = currentState;
        currentState = input;

        oldState = outputState;

        if (!previusState && !currentState)
            outputState = INPUT_RELEASED;
        if (!previusState && currentState)
            outputState = INPUT_JUST_PRESSED;
        if (previusState && currentState)
            outputState = INPUT_PRESSED;
        if (previusState && !currentState)
            outputState = INPUT_JUST_RELEASED;

        return outputState;
    }
}
