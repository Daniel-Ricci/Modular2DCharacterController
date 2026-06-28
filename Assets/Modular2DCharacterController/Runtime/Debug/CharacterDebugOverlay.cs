using System.Text;
using Modular2DCharacterController.Runtime.Core;
using Modular2DCharacterController.Runtime.Features;
using Modular2DCharacterController.Runtime.Input;
using UnityEngine;

namespace Modular2DCharacterController.Runtime.Debug
{
    /// <summary>
    /// Draws a lightweight runtime debug overlay for the character controller.
    ///
    /// This component is intentionally read-only: it observes the motor,
    /// detectors, input provider, and optional features, but it does not
    /// submit movement requests or change gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterDebugOverlay : MonoBehaviour
    {
        [Header("Visibility")]

        [Tooltip("If disabled, the overlay and gizmos will not be drawn.")]
        [SerializeField]
        private bool debugEnabled = true;

        [Tooltip("Key used to toggle the overlay at runtime.")]
        [SerializeField]
        private KeyCode toggleKey = KeyCode.BackQuote;

        [Tooltip(
            "If disabled, the overlay is automatically hidden in non-development builds. " +
            "The Unity Editor always allows it.")]
        [SerializeField]
        private bool showInReleaseBuild = false;

        [Header("Overlay Layout")]

        [Tooltip("Top-left screen position of the overlay.")]
        [SerializeField]
        private Vector2 screenOffset = new(12f, 12f);

        [Tooltip("Overlay width in pixels.")]
        [SerializeField]
        [Min(180f)]
        private float overlayWidth = 430f;

        [Tooltip("Font size used by the overlay text.")]
        [SerializeField]
        [Min(8)]
        private int fontSize = 13;

        [Tooltip("How many decimals should be shown for numeric values.")]
        [SerializeField]
        [Range(0, 4)]
        private int decimalPlaces = 2;

        [Header("Sections")]

        [SerializeField]
        private bool showMotor = true;

        [SerializeField]
        private bool showDetectors = true;

        [SerializeField]
        private bool showFeatures = true;

        [SerializeField]
        private bool showInput = true;

        [Header("Gizmos")]

        [Tooltip("Draws velocity and normal vectors in the Scene view.")]
        [SerializeField]
        private bool drawGizmos = true;

        [Tooltip("Multiplier used to make velocity vectors easier to inspect.")]
        [SerializeField]
        [Min(0f)]
        private float velocityGizmoScale = 0.1f;

        [Tooltip("Length used when drawing ground and wall normal vectors.")]
        [SerializeField]
        [Min(0f)]
        private float normalGizmoLength = 0.75f;

        [Header("Runtime Vectors")]

        [Tooltip("Draws debug vectors in the Game view using LineRenderer components.")]
        [SerializeField]
        private bool drawRuntimeVectors = true;

        [Tooltip("Multiplier used to make runtime velocity vectors easier to inspect.")]
        [SerializeField]
        [Min(0f)]
        private float runtimeVelocityScale = 0.1f;

        [Tooltip("Length used when drawing runtime ground and wall normal vectors.")]
        [SerializeField]
        [Min(0f)]
        private float runtimeNormalLength = 0.75f;

        [Tooltip("Width of the runtime debug vector lines.")]
        [SerializeField]
        [Min(0.001f)]
        private float runtimeLineWidth = 0.035f;

        [Tooltip("Sorting order used by the runtime debug vector lines.")]
        [SerializeField]
        private int runtimeLineSortingOrder = 1000;

        [Header("Runtime Vector Colors")]

        [SerializeField]
        private Color rigidbodyVelocityColor = Color.white;

        [SerializeField]
        private Color selfVelocityColor = Color.cyan;

        [SerializeField]
        private Color externalVelocityColor = Color.yellow;

        [SerializeField]
        private Color groundNormalColor = Color.green;

        [SerializeField]
        private Color wallNormalColor = Color.red;

        private readonly StringBuilder _builder = new();

        private Rigidbody2D _rigidbody;
        private CharacterMotor _motor;
        private GroundDetector _groundDetector;
        private WallDetector _wallDetector;
        private ICharacterInput _input;
        private HorizontalMovementFeature _horizontalMovementFeature;
        private JumpFeature _jumpFeature;
        private DashFeature _dashFeature;
        private RunFeature _runFeature;
        private WallSlideFeature _wallSlideFeature;
        private WallJumpFeature _wallJumpFeature;
        private GlideFeature _glideFeature;
        private PlatformMotionTransferFeature _platformMotionTransferFeature;

        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;
        private Texture2D _backgroundTexture;
        private Material _runtimeLineMaterial;
        private LineRenderer _rigidbodyVelocityLine;
        private LineRenderer _selfVelocityLine;
        private LineRenderer _externalVelocityLine;
        private LineRenderer _groundNormalLine;
        private LineRenderer _wallNormalLine;

        private void Awake()
        {
            CacheComponents();
        }

        private void Update()
        {
            if (toggleKey != KeyCode.None &&
                UnityEngine.Input.GetKeyDown(toggleKey))
            {
                debugEnabled = !debugEnabled;
            }

            UpdateRuntimeVectors();
        }

        private void OnDisable()
        {
            SetRuntimeVectorsEnabled(false);
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
            {
                Destroy(_backgroundTexture);
            }

            if (_runtimeLineMaterial != null)
            {
                Destroy(_runtimeLineMaterial);
            }
        }

        private void OnGUI()
        {
            if (!ShouldDrawOverlay())
                return;

            EnsureStyles();

            _builder.Clear();
            BuildOverlayText();

            GUIContent content =
                new(_builder.ToString());

            float height =
                _labelStyle.CalcHeight(content, overlayWidth - 20f) + 20f;

            Rect rect =
                new(screenOffset.x, screenOffset.y, overlayWidth, height);

            GUI.Box(rect, GUIContent.none, _boxStyle);

            GUI.Label(
                new Rect(
                    rect.x + 10f,
                    rect.y + 10f,
                    rect.width - 20f,
                    rect.height - 20f),
                content,
                _labelStyle);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos || !debugEnabled)
                return;

            if (_motor == null)
            {
                CacheComponents();
            }

            Vector3 origin =
                transform.position;

            if (_rigidbody != null)
            {
                DrawVector(
                    origin,
                    _rigidbody.linearVelocity * velocityGizmoScale,
                    Color.white);
            }

            if (_motor != null)
            {
                DrawVector(
                    origin + Vector3.up * 0.15f,
                    _motor.LastResolvedSelfVelocity * velocityGizmoScale,
                    Color.cyan);

                DrawVector(
                    origin + Vector3.up * 0.3f,
                    _motor.LastResolvedExternalVelocity * velocityGizmoScale,
                    Color.yellow);
            }

            if (_groundDetector != null && _groundDetector.IsGrounded)
            {
                DrawVector(
                    origin,
                    _groundDetector.GroundNormal * normalGizmoLength,
                    Color.green);
            }

            if (_wallDetector != null && _wallDetector.IsTouchingWall)
            {
                DrawVector(
                    origin,
                    _wallDetector.WallNormal * normalGizmoLength,
                    Color.red);
            }
        }

        private void CacheComponents()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _motor = GetComponent<CharacterMotor>();
            _groundDetector = GetComponent<GroundDetector>();
            _wallDetector = GetComponent<WallDetector>();
            _input = GetComponent<ICharacterInput>();
            _horizontalMovementFeature = GetComponent<HorizontalMovementFeature>();
            _jumpFeature = GetComponent<JumpFeature>();
            _dashFeature = GetComponent<DashFeature>();
            _runFeature = GetComponent<RunFeature>();
            _wallSlideFeature = GetComponent<WallSlideFeature>();
            _wallJumpFeature = GetComponent<WallJumpFeature>();
            _glideFeature = GetComponent<GlideFeature>();
            _platformMotionTransferFeature = GetComponent<PlatformMotionTransferFeature>();
        }

        private void UpdateRuntimeVectors()
        {
            if (!ShouldDrawRuntimeVectors())
            {
                SetRuntimeVectorsEnabled(false);
                return;
            }

            EnsureRuntimeVectorLines();
            SetRuntimeVectorsEnabled(true);

            Vector3 origin =
                transform.position;

            if (_rigidbody != null)
            {
                UpdateRuntimeVector(
                    _rigidbodyVelocityLine,
                    origin,
                    _rigidbody.linearVelocity * runtimeVelocityScale);
            }
            else
            {
                SetLineEnabled(_rigidbodyVelocityLine, false);
            }

            if (_motor != null)
            {
                UpdateRuntimeVector(
                    _selfVelocityLine,
                    origin + Vector3.up * 0.15f,
                    _motor.LastResolvedSelfVelocity * runtimeVelocityScale);

                UpdateRuntimeVector(
                    _externalVelocityLine,
                    origin + Vector3.up * 0.3f,
                    _motor.LastResolvedExternalVelocity * runtimeVelocityScale);
            }
            else
            {
                SetLineEnabled(_selfVelocityLine, false);
                SetLineEnabled(_externalVelocityLine, false);
            }

            if (_groundDetector != null && _groundDetector.IsGrounded)
            {
                UpdateRuntimeVector(
                    _groundNormalLine,
                    origin,
                    _groundDetector.GroundNormal * runtimeNormalLength);
            }
            else
            {
                SetLineEnabled(_groundNormalLine, false);
            }

            if (_wallDetector != null && _wallDetector.IsTouchingWall)
            {
                UpdateRuntimeVector(
                    _wallNormalLine,
                    origin,
                    _wallDetector.WallNormal * runtimeNormalLength);
            }
            else
            {
                SetLineEnabled(_wallNormalLine, false);
            }
        }

        private bool ShouldDrawRuntimeVectors()
        {
            if (!drawRuntimeVectors)
                return false;

            return ShouldDrawOverlay();
        }

        private void EnsureRuntimeVectorLines()
        {
            EnsureRuntimeLineMaterial();

            _rigidbodyVelocityLine = EnsureRuntimeVectorLine(
                _rigidbodyVelocityLine,
                "Rigidbody Velocity",
                rigidbodyVelocityColor);

            _selfVelocityLine = EnsureRuntimeVectorLine(
                _selfVelocityLine,
                "Self Velocity",
                selfVelocityColor);

            _externalVelocityLine = EnsureRuntimeVectorLine(
                _externalVelocityLine,
                "External Velocity",
                externalVelocityColor);

            _groundNormalLine = EnsureRuntimeVectorLine(
                _groundNormalLine,
                "Ground Normal",
                groundNormalColor);

            _wallNormalLine = EnsureRuntimeVectorLine(
                _wallNormalLine,
                "Wall Normal",
                wallNormalColor);
        }

        private void EnsureRuntimeLineMaterial()
        {
            if (_runtimeLineMaterial != null)
                return;

            Shader shader =
                Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            _runtimeLineMaterial =
                new Material(shader)
                {
                    name = "Character Debug Runtime Line Material"
                };
        }

        private LineRenderer EnsureRuntimeVectorLine(
            LineRenderer line,
            string lineName,
            Color color)
        {
            if (line == null)
            {
                GameObject lineObject =
                    new($"Debug Vector - {lineName}");

                lineObject.transform.SetParent(transform, false);

                line =
                    lineObject.AddComponent<LineRenderer>();

                line.positionCount = 2;
                line.useWorldSpace = true;
                line.numCapVertices = 4;
            }

            line.sharedMaterial = _runtimeLineMaterial;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = runtimeLineWidth;
            line.endWidth = runtimeLineWidth;
            line.sortingOrder = runtimeLineSortingOrder;

            return line;
        }

        private void UpdateRuntimeVector(
            LineRenderer line,
            Vector3 origin,
            Vector2 vector)
        {
            if (line == null)
                return;

            if (vector == Vector2.zero)
            {
                SetLineEnabled(line, false);
                return;
            }

            SetLineEnabled(line, true);

            line.SetPosition(0, origin);
            line.SetPosition(1, origin + (Vector3)vector);
        }

        private void SetRuntimeVectorsEnabled(bool enabled)
        {
            SetLineEnabled(_rigidbodyVelocityLine, enabled);
            SetLineEnabled(_selfVelocityLine, enabled);
            SetLineEnabled(_externalVelocityLine, enabled);
            SetLineEnabled(_groundNormalLine, enabled);
            SetLineEnabled(_wallNormalLine, enabled);
        }

        private static void SetLineEnabled(
            LineRenderer line,
            bool enabled)
        {
            if (line == null)
                return;

            line.enabled = enabled;
        }

        private bool ShouldDrawOverlay()
        {
            if (!debugEnabled)
                return false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#else
            return showInReleaseBuild;
#endif
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null && _labelStyle.fontSize == fontSize)
                return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                richText = false,
                wordWrap = false,
                normal =
                {
                    textColor = Color.white
                }
            };

            if (_backgroundTexture != null)
            {
                Destroy(_backgroundTexture);
            }

            _backgroundTexture =
                new Texture2D(1, 1);

            _backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.72f));
            _backgroundTexture.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal =
                {
                    background = _backgroundTexture
                }
            };
        }

        private void BuildOverlayText()
        {
            AppendLine("Character Debug Overlay");
            AppendLine($"GameObject: {gameObject.name}");
            AppendLine($"Toggle: {toggleKey}");
            AppendLine();

            if (showMotor)
            {
                AppendMotorSection();
            }

            if (showDetectors)
            {
                AppendDetectorsSection();
            }

            if (showFeatures)
            {
                AppendFeaturesSection();
            }

            if (showInput)
            {
                AppendInputSection();
            }
        }

        private void AppendMotorSection()
        {
            AppendLine("[Motor]");

            if (_rigidbody == null)
            {
                AppendLine("Rigidbody2D: missing");
            }
            else
            {
                AppendLine($"Rigidbody Velocity: {FormatVector(_rigidbody.linearVelocity)}");
            }

            if (_motor == null)
            {
                AppendLine("CharacterMotor: missing");
                AppendLine();
                return;
            }

            AppendLine($"Self Velocity: {FormatVector(_motor.LastResolvedSelfVelocity)}");
            AppendLine($"External Velocity: {FormatVector(_motor.LastResolvedExternalVelocity)}");
            AppendLine($"Final Velocity: {FormatVector(_motor.LastResolvedFinalVelocity)}");
            AppendLine($"Last External Applied: {FormatVector(_motor.LastAppliedExternalVelocity)}");
            AppendLine($"Custom Gravity: {_motor.UseCustomGravity}");
            AppendLine($"Gravity Acceleration: {FormatFloat(_motor.GravityAcceleration)}");
            AppendLine($"Gravity Multiplier: {FormatFloat(_motor.LastResolvedGravityMultiplier)}");
            AppendLine($"Gravity Suppressed: {_motor.LastResolvedGravitySuppressed}");
            AppendLine($"External Suppressed: {_motor.LastResolvedExternalVelocitySuppressed}");
            AppendLine($"Max Fall Speed: {FormatFloat(_motor.MaxFallSpeed)}");
            AppendLine();
        }

        private void AppendDetectorsSection()
        {
            AppendLine("[Detectors]");

            if (_groundDetector == null)
            {
                AppendLine("GroundDetector: missing");
            }
            else
            {
                AppendLine($"Grounded: {_groundDetector.IsGrounded}");
                AppendLine($"Ground Normal: {FormatVector(_groundDetector.GroundNormal)}");
                AppendLine($"Ground Angle: {FormatFloat(_groundDetector.GroundAngle)}");
                AppendLine($"Ground Velocity: {FormatVector(_groundDetector.GroundVelocity)}");
                AppendLine($"Ground Delta: {FormatVector(_groundDetector.GroundDelta)}");
                AppendLine($"Ground Object: {FormatTransform(_groundDetector.CurrentGroundTransform)}");
            }

            if (_wallDetector == null)
            {
                AppendLine("WallDetector: missing");
            }
            else
            {
                AppendLine($"Touching Wall: {_wallDetector.IsTouchingWall}");
                AppendLine($"Wall Normal: {FormatVector(_wallDetector.WallNormal)}");
            }

            AppendLine();
        }

        private void AppendFeaturesSection()
        {
            AppendLine("[Features]");

            if (_horizontalMovementFeature != null)
            {
                AppendLine($"Facing Direction: {_horizontalMovementFeature.FacingDirection}");
                AppendLine($"Movement Profile: {FormatObjectName(_horizontalMovementFeature.CurrentMovementProfile)}");
            }

            if (_runFeature != null)
            {
                AppendLine($"Running: {_runFeature.IsRunning}");
            }

            if (_jumpFeature != null)
            {
                AppendLine($"Jump Profile: {FormatObjectName(_jumpFeature.CurrentJumpProfile)}");
                AppendLine($"Jump Active: {_jumpFeature.IsJumpActive}");
                AppendLine($"Jump Ascending: {_jumpFeature.IsJumpAscending}");
                AppendLine($"Remaining Air Jumps: {_jumpFeature.RemainingAirJumps}");
                AppendLine($"Coyote Timer: {FormatFloat(_jumpFeature.CoyoteTimer)}");
                AppendLine($"Jump Buffer Timer: {FormatFloat(_jumpFeature.JumpBufferTimer)}");
                AppendLine($"Jump After Dash Timer: {FormatFloat(_jumpFeature.JumpAfterDashTimer)}");
                AppendLine($"Jump Velocity: {FormatFloat(_jumpFeature.JumpVelocity)}");
                AppendLine($"Ascent Gravity Multiplier: {FormatFloat(_jumpFeature.AscentGravityMultiplier)}");
            }

            if (_dashFeature != null)
            {
                AppendLine($"Dash Profile: {FormatObjectName(_dashFeature.CurrentDashProfile)}");
                AppendLine($"Dashing: {_dashFeature.IsDashing}");
                AppendLine($"Remaining Dashes: {_dashFeature.RemainingDashes}");
                AppendLine($"Dash Direction: {FormatVector(_dashFeature.DashDirection)}");
                AppendLine($"Dash Timer: {FormatFloat(_dashFeature.DashTimer)}");
                AppendLine($"Dash Cooldown: {FormatFloat(_dashFeature.CooldownTimer)}");
            }

            if (_wallSlideFeature != null)
            {
                AppendLine($"Wall Sliding: {_wallSlideFeature.IsWallSliding}");
            }

            if (_wallJumpFeature != null)
            {
                AppendLine($"Wall Jump Profile: {FormatObjectName(_wallJumpFeature.CurrentWallJumpProfile)}");
                AppendLine($"Wall Jump Locked: {_wallJumpFeature.IsMovementLocked}");
                AppendLine($"Wall Jump Lock Timer: {FormatFloat(_wallJumpFeature.MovementLockTimer)}");
            }

            if (_glideFeature != null)
            {
                AppendLine($"Gliding: {_glideFeature.IsGliding}");
            }

            if (_platformMotionTransferFeature != null)
            {
                AppendLine("Platform Motion Transfer: present");
            }

            AppendLine();
        }

        private void AppendInputSection()
        {
            AppendLine("[Input]");

            if (_input == null)
            {
                AppendLine("Input Provider: missing");
                AppendLine();
                return;
            }

            AppendLine($"Move Input: {FormatFloat(_input.MoveInput)}");
            AppendLine($"Jump Pressed: {_input.JumpPressed}");
            AppendLine($"Jump Held: {_input.JumpHeld}");
            AppendLine($"Run Held: {_input.RunHeld}");
            AppendLine($"Dash Pressed: {_input.DashPressed}");
            AppendLine($"Dash Held: {_input.DashHeld}");
            AppendLine($"Crouch Pressed: {_input.CrouchPressed}");
            AppendLine($"Crouch Held: {_input.CrouchHeld}");
            AppendLine();
        }

        private void DrawVector(
            Vector3 origin,
            Vector2 vector,
            Color color)
        {
            if (vector == Vector2.zero)
                return;

            Gizmos.color = color;
            Gizmos.DrawLine(origin, origin + (Vector3)vector);
            Gizmos.DrawSphere(origin + (Vector3)vector, 0.035f);
        }

        private void AppendLine()
        {
            _builder.AppendLine();
        }

        private void AppendLine(string line)
        {
            _builder.AppendLine(line);
        }

        private string FormatFloat(float value)
        {
            return value.ToString($"F{decimalPlaces}");
        }

        private string FormatVector(Vector2 value)
        {
            return $"({FormatFloat(value.x)}, {FormatFloat(value.y)})";
        }

        private static string FormatObjectName(Object target)
        {
            return target != null ? target.name : "None";
        }

        private static string FormatTransform(Transform target)
        {
            return target != null ? target.name : "None";
        }
    }
}
