using Badventure.Scripts.Csharp.Solution.Controllers;
using Badventure.Scripts.Csharp.Solution.Models;
using Godot;

public partial class PlayerMovement : CharacterBody3D
{
    [Export] public float Speed = 5.0f;
    [Export] public float JumpForce = 6.5f;
    [Export] public float MouseSensitivity = 0.2f;

    private Vector2 _mouseDelta;
    private Camera3D _camera;
    private const float Gravity = -9.8f;
    private Vector3 _velocity = Vector3.Zero;
    private Label _screenLabel;
    private CanvasLayer _canvasLayer;
    private PlayerController _playerController;

    public override void _Ready()
    {
        _playerController = new PlayerController(new PlayerModel("Player", 100, 5, 10, 5, 15, 1, 5, 2, 10, 5, null, null, null));

        _camera = GetNode<Camera3D>("Camera3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;

        CallDeferred(nameof(SetupDebugLabel));
    }

    private void SetupDebugLabel()
    {
        _canvasLayer = new CanvasLayer { Layer = 100 };
        GetTree().Root.AddChild(_canvasLayer);

        _screenLabel = new Label { Position = new Vector2(10, 10) };
        _screenLabel.AddThemeFontSizeOverride("font_size", 20);

        _canvasLayer.AddChild(_screenLabel);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            _mouseDelta += mouseMotion.Relative * MouseSensitivity;
            _mouseDelta.Y = Mathf.Clamp(_mouseDelta.Y, -89.9f, 89.9f);

            _camera.RotationDegrees = new Vector3(-_mouseDelta.Y, 0, 0);
            RotationDegrees = new Vector3(0, -_mouseDelta.X, 0);
        }

        if (@event.IsActionPressed("open_menu"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement();

        HandleGravitation(delta);

        HandleJump();

        ValidatePosition();

        UpdatePositionLabel();

        Velocity = _velocity;

        MoveAndSlide();
    }

    private void HandleGravitation(double delta)
    {
        if (!IsOnFloor())
        {
            _velocity.Y += Gravity * (float)delta;
        }
        else
        {
            _velocity.Y = 0;
        }
    }

    private void ValidatePosition()
    {
        if (!IsOnFloor() && GlobalPosition.Y < -20)
        {
            Translate(new Vector3(Position.X, Position.Y + 60, Position.Z));
        }
    }

    private void HandleMovement()
    {
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

        Vector3 direction = Transform.Basis * new Vector3(input.X, 0, input.Y);
        direction = direction.Normalized();

        _velocity.X = Input.IsActionPressed("sprint") ? direction.X * Speed * 1.5F : direction.X * Speed;
        _velocity.Z = Input.IsActionPressed("sprint") ? direction.Z * Speed * 1.5F : direction.Z * Speed;
    }

    private void HandleJump()
    {
        if (Input.IsActionPressed("jump") && IsOnFloor())
        {
            _velocity.Y = JumpForce;
        }
    }

    private void UpdatePositionLabel()
    {
        if (_screenLabel != null)
        {
            _screenLabel.Text = $"""  
                Позиция:   
                X: {GlobalPosition.X:F2}  
                Y: {GlobalPosition.Y:F2}  
                Z: {GlobalPosition.Z:F2}  

                Скорость:  
                X: {Velocity.X:F2}  
                Y: {Velocity.Y:F2}  
                Z: {Velocity.Z:F2}  
                """;
        }
    }
}