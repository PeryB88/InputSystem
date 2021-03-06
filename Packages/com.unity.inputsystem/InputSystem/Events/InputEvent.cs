using System.Runtime.InteropServices;
using UnityEngine.Experimental.Input.Utilities;

////REVIEW: should this really implement IInputEventTypeInfo? it's essentially an "abstract" struct

namespace UnityEngine.Experimental.Input.LowLevel
{
    /// <summary>
    /// A chunk of memory signaling a data transfer in the input system.
    /// </summary>
    // NOTE: This has to be layout compatible with native events.
    [StructLayout(LayoutKind.Explicit, Size = kBaseEventSize)]
    public struct InputEvent : IInputEventTypeInfo
    {
        private const uint kHandledMask = 0x80000000;
        private const uint kIdMask = 0x7FFFFFFF;

        public const int kBaseEventSize = 20;
        public const int kInvalidId = 0;

        [FieldOffset(0)] private FourCC m_Type;
        [FieldOffset(4)] private ushort m_SizeInBytes;
        [FieldOffset(6)] private ushort m_DeviceId;
        [FieldOffset(8)] internal uint m_EventId;
        ////REVIEW: does this really need to be a double? float would save us a 4 bytes
        [FieldOffset(12)] private double m_Time;

        /// <summary>
        /// Type code for the event.
        /// </summary>
        public FourCC type
        {
            get { return m_Type; }
        }

        /// <summary>
        /// Total size of the event in bytes.
        /// </summary>
        /// <remarks>
        /// Events are variable-size structs. This field denotes the total size of the event
        /// as stored in memory. This includes the full size of this struct and not just the
        /// "payload" of the event.
        /// </remarks>
        /// <example>
        /// Store event in private buffer:
        /// <code>
        /// unsafe byte[] CopyEventData(InputEventPtr eventPtr)
        /// {
        ///     var sizeInBytes = eventPtr.sizeInBytes;
        ///     var buffer = new byte[sizeInBytes];
        ///     fixed (byte* bufferPtr = buffer)
        ///     {
        ///         UnsafeUtility.MemCpy(new IntPtr(bufferPtr), eventPtr.data, sizeInBytes);
        ///     }
        ///     return buffer;
        /// }
        /// </code>
        /// </example>
        public uint sizeInBytes
        {
            get { return m_SizeInBytes; }
        }

        /// <summary>
        /// Unique serial ID of the event.
        /// </summary>
        /// <remarks>
        /// Events are assigned running IDs when they are put on an event queue.
        /// </remarks>
        public int eventId
        {
            get { return (int)(m_EventId & kIdMask); }
        }

        /// <summary>
        /// ID of the device that the event is for.
        /// </summary>
        /// <seealso cref="InputDevice.id"/>
        public int deviceId
        {
            get { return m_DeviceId; }
            set { m_DeviceId = (ushort)value; }
        }

        ////TODO: clarify timeline that this sits on
        /// <summary>
        /// Time that the event was generated.
        /// </summary>
        public double time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        public InputEvent(FourCC type, int sizeInBytes, int deviceId, double time)
        {
            m_Type = type;
            m_SizeInBytes = (ushort)sizeInBytes;
            m_DeviceId = (ushort)deviceId;
            m_Time = time;
            m_EventId = kInvalidId;
        }

        public FourCC GetTypeStatic()
        {
            return new FourCC(); // No valid type code; InputEvent is considered abstract.
        }

        // We internally use bits inside m_EventId as flags. IDs are linearly counted up by the
        // native input system starting at 1 so we have plenty room.
        // NOTE: The native system assigns IDs when events are queued so if our handled flag
        //       will implicitly get overwritten. Having events go back to unhandled state
        //       when they go on the queue makes sense in itself, though, so this is fine.
        public bool handled
        {
            get { return (m_EventId & kHandledMask) == kHandledMask; }
            set { m_EventId |= kHandledMask; }
        }

        public override string ToString()
        {
            return string.Format("id={0} type={1} device={2} size={3} time={4}",
                eventId, type, deviceId, sizeInBytes, time);
        }

        internal static unsafe InputEvent* GetNextInMemory(InputEvent* current)
        {
            return (InputEvent*)((byte*)current + current->sizeInBytes);
        }
    }
}
