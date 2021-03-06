#if UNITY_EDITOR || UNITY_SWITCH
using System;
using System.Runtime.InteropServices;
using UnityEngine.Experimental.Input.Controls;
using UnityEngine.Experimental.Input.LowLevel;
using UnityEngine.Experimental.Input.Plugins.Switch.LowLevel;
using UnityEngine.Experimental.Input.Utilities;

////TODO: gyro and accelerometer support

namespace UnityEngine.Experimental.Input.Plugins.Switch.LowLevel
{
    /// <summary>
    /// Structure of HID input reports for Switch NPad controllers.
    /// </summary>
    /// <rem
    /// <seealso href="http://en-americas-support.nintendo.com/app/answers/detail/a_id/22634/~/joy-con-controller-diagram"/>
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct NPadInputState : IInputStateTypeInfo
    {
        public FourCC GetFormat()
        {
            return new FourCC('N', 'P', 'A', 'D');
        }

        [InputControl(name = "dpad")]
        [InputControl(name = "buttonNorth", displayName = "X", bit = (uint)Button.North)]
        [InputControl(name = "buttonSouth", displayName = "B", bit = (uint)Button.South, usage = "Back")]
        [InputControl(name = "buttonWest", displayName = "Y", bit = (uint)Button.West, usage = "SecondaryAction")]
        [InputControl(name = "buttonEast", displayName = "A", bit = (uint)Button.East, usage = "PrimaryAction")]
        [InputControl(name = "leftStickPress", displayName = "Left Stick", bit = (uint)Button.StickL)]
        [InputControl(name = "rightStickPress", displayName = "Right Stick", bit = (uint)Button.StickR)]
        [InputControl(name = "leftShoulder", displayName = "L", bit = (uint)Button.L)]
        [InputControl(name = "rightShoulder", displayName = "R", bit = (uint)Button.R)]
        [InputControl(name = "leftTrigger", displayName = "ZL", format = "BIT", bit = (uint)Button.ZL)]
        [InputControl(name = "rightTrigger", displayName = "ZR", format = "BIT", bit = (uint)Button.ZR)]
        [InputControl(name = "start", displayName = "Plus", bit = (uint)Button.Plus, usage = "Menu")]
        [InputControl(name = "select", displayName = "Minus", bit = (uint)Button.Minus)]
        [InputControl(name = "leftSL", displayName = "SL (Left)", layout = "Button", bit = (uint)Button.LSL)]
        [InputControl(name = "leftSR", displayName = "SR (Left)", layout = "Button", bit = (uint)Button.LSR)]
        [InputControl(name = "rightSL", displayName = "SL (Right)", layout = "Button", bit = (uint)Button.RSL)]
        [InputControl(name = "rightSR", displayName = "SR (Right)", layout = "Button", bit = (uint)Button.RSR)]
        ////TODO: clarify meaning of these buttons
        [InputControl(name = "leftVK", layout = "Dpad")]
        [InputControl(name = "leftVK/up", bit = (uint)Button.VKey_LUp)]
        [InputControl(name = "leftVK/down", bit = (uint)Button.VKey_LDown)]
        [InputControl(name = "leftVK/left", bit = (uint)Button.VKey_LLeft)]
        [InputControl(name = "leftVK/right", bit = (uint)Button.VKey_LRight)]
        [InputControl(name = "rightVK", layout = "Dpad")]
        [InputControl(name = "rightVK/up", bit = (uint)Button.VKey_RUp)]
        [InputControl(name = "rightVK/down", bit = (uint)Button.VKey_RDown)]
        [InputControl(name = "rightVK/left", bit = (uint)Button.VKey_RLeft)]
        [InputControl(name = "rightVK/right", bit = (uint)Button.VKey_RRight)]
        [FieldOffset(0)]
        public uint buttons;

        ////FIXME: it's so counterintuitive that we have to set "layout" here even though we inherit the control from Gamepad
        [InputControl(name = "leftStick", layout = "Stick")]
        [FieldOffset(4)]
        public Vector2 leftStick;

        [InputControl(name = "rightStick", layout = "Stick")]
        [FieldOffset(12)]
        public Vector2 rightStick;

        public enum Button
        {
            // Dpad buttons. Important to be first in the bitfield as we'll
            // point the DpadControl to it.
            // IMPORTANT: Order has to match what is expected by DpadControl.
            Up,
            Down,
            Left,
            Right,

            North,
            South,
            West,
            East,

            StickL,
            StickR,
            L,
            R,

            ZL,
            ZR,
            Plus,
            Minus,

            LSL,
            LSR,
            RSL,
            RSR,

            VKey_LUp,
            VKey_LDown,
            VKey_LLeft,
            VKey_LRight,

            VKey_RUp,
            VKey_RDown,
            VKey_RLeft,
            VKey_RRight,

            X = North,
            B = South,
            Y = West,
            A = East,
        }

        public NPadInputState WithButton(Button button, bool value = true)
        {
            var bit = (uint)1 << (int)button;
            if (value)
                buttons |= bit;
            else
                buttons &= ~bit;
            return this;
        }

        public NPadInputState WithLeftStick(Vector2 vector)
        {
            leftStick = vector;
            return this;
        }

        public NPadInputState WithRightStick(Vector2 vector)
        {
            rightStick = vector;
            return this;
        }
    }

    /// <summary>
    /// Switch output report sent as command to the backend.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct NPadOutputReport : IInputDeviceCommandInfo
    {
        public static FourCC Type { get { return new FourCC('N', 'P', 'D', 'O'); } }

        public const int kSize = InputDeviceCommand.kBaseCommandSize + 16;

        [Flags]
        public enum Flags
        {
            SetPosition = (1 << 0),
        }

        [FieldOffset(0)] public InputDeviceCommand baseCommand;

        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 0)] public uint flags;
        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 4)] public byte controllerId;
        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 5)] public byte npadId;
        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 6)] public byte position;
        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 7)] public byte pudding0;
        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 8)] public uint styleMask;
        [FieldOffset(InputDeviceCommand.kBaseCommandSize + 12)] public int color;

        public FourCC GetTypeStatic()
        {
            return Type;
        }

        public void SetPosition(NPad.Position pos)
        {
            flags |= (byte)Flags.SetPosition;
            position = (byte)pos;
        }

        public static NPadOutputReport Create()
        {
            return new NPadOutputReport
            {
                baseCommand = new InputDeviceCommand(Type, kSize),
            };
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    public struct NPadShowControllerSupportUICommand : IInputDeviceCommandInfo
    {
        public static FourCC Type { get { return new FourCC('N', 'P', 'D', 'U'); } }

        public const int kSize = InputDeviceCommand.kBaseCommandSize;

        [FieldOffset(0)]
        public InputDeviceCommand baseCommand;

        public FourCC GetTypeStatic()
        {
            return Type;
        }

        public static NPadShowControllerSupportUICommand Create()
        {
            return new NPadShowControllerSupportUICommand
            {
                baseCommand = new InputDeviceCommand(Type, kSize),
            };
        }
    }
}

namespace UnityEngine.Experimental.Input.Plugins.Switch
{
    /// <summary>
    /// An NPad controller for Switch, which can be a Joy-Con.
    /// </summary>
    /// <seealso cref="NPadInputState"/>
    [InputControlLayout(stateType = typeof(NPadInputState))]
    public class NPad : Gamepad
    {
        public ButtonControl leftSL { get; private set; }
        public ButtonControl leftSR { get; private set; }
        public ButtonControl rightSL { get; private set; }
        public ButtonControl rightSR { get; private set; }
        public DpadControl leftVK { get; private set; }
        public DpadControl rightVK { get; private set; }

        public enum Position
        {
            Vertical,
            Sideways,
            Default = Vertical,
        }

        public enum NpadId : int
        {
            No1 = 0x00,
            No2 = 0x01,
            No3 = 0x02,
            No4 = 0x03,
            No5 = 0x04,
            No6 = 0x05,
            No7 = 0x06,
            No8 = 0x07,
            Handheld = 0x20,
            Debug = 0xFF,
        }

        [Flags]
        public enum NpadStyle
        {
            FullKey = (1 << 0),
            Handheld = (1 << 1),
            JoyDual = (1 << 2),
            JoyLeft = (1 << 3),
            JoyRight = (1 << 4),
        }

        public long ShowControllerSupportUI()
        {
            var command = NPadShowControllerSupportUICommand.Create();

            return ExecuteCommand(ref command);
        }

        ////REVIEW: this should probably use layout variants; if not, should be turned into an 'orientation' property
        public void SetPosition(Position position)
        {
            var command = NPadOutputReport.Create();

            command.SetPosition(position);
            ExecuteCommand(ref command);
        }

        public Position GetPosition()
        {
            var command = NPadOutputReport.Create();

            if (ExecuteCommand(ref command) < 0)
                return Position.Default;
            return (Position)command.position;
        }

        protected override void FinishSetup(InputDeviceBuilder builder)
        {
            leftSL = builder.GetControl<ButtonControl>("leftSL");
            leftSR = builder.GetControl<ButtonControl>("leftSR");
            rightSL = builder.GetControl<ButtonControl>("rightSL");
            rightSR = builder.GetControl<ButtonControl>("rightSR");
            leftVK = builder.GetControl<DpadControl>("leftVK");
            rightVK = builder.GetControl<DpadControl>("rightVK");

            base.FinishSetup(builder);
        }
    }
}
#endif // UNITY_EDITOR || UNITY_SWITCH
