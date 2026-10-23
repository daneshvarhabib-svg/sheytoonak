using System;
using System.Collections;
using Sheytoonak.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using InputGyroscope = UnityEngine.InputSystem.Gyroscope;

namespace Sheytoonak
{
    /// <summary>
    /// Phone-motion controls (portrait): tilt/slide the phone left-right to move to the ball,
    /// swing the bottom edge of the phone forward to kick; a bigger swing = a harder kick.
    /// Reads Unity Input System sensors and hands raw samples to the pure <see cref="MotionGestureDetector"/>.
    /// </summary>
    public class MotionKickInput : MonoBehaviour, IKickInput
    {
        public GameConfig config;
        [Tooltip("Set to 9.81 if the debug overlay shows linear acceleration in g (values around 0.1-1 while shaking) instead of m/s².")]
        public float linearAccelerationScale = 1f;

        // "v2": the game went landscape on 2026-10-23; a calibration saved by the portrait build must not carry over.
        const string PrefAxis = "sheytoonak.kickAxis.v2", PrefSign = "sheytoonak.kickSign.v2";

        public event Action<KickGesture> Kicked;
        public bool IsAvailable => gyro != null && gyro.enabled;
        public float Lateral => detector != null ? detector.Lateral : 0f;

        // Exposed for the debug overlay and the calibration screen.
        public Vector3 RawGyro { get; private set; }
        public Vector3 RawLinear { get; private set; }
        public Vector3 RawGravity { get; private set; } = new Vector3(0f, -9.81f, 0f);
        public KickGesture LastKick { get; private set; }
        public MotionGestureDetector Detector => detector;
        public bool Calibrating { get; private set; }

        MotionGestureDetector detector;
        MotionTuning tuning;
        InputGyroscope gyro;
        LinearAccelerationSensor linear;
        GravitySensor gravity;
        Accelerometer accel;

        void OnEnable()
        {
            // A copy: calibration must not overwrite the shared asset while playing in the editor.
            tuning = config != null ? JsonUtility.FromJson<MotionTuning>(JsonUtility.ToJson(config.motion)) : new MotionTuning();
            if (PlayerPrefs.HasKey(PrefAxis))
            {
                tuning.KickAxis = (DeviceAxis)PlayerPrefs.GetInt(PrefAxis);
                tuning.KickAxisSign = PlayerPrefs.GetFloat(PrefSign, 1f);
            }
            detector = new MotionGestureDetector(tuning);
            AcquireSensors();
        }

        float nextAcquire;

        // On Android the sensor devices can be added after the first frame, so a null .current in
        // OnEnable does not mean "no sensor". Look again (once a second) until the gyro shows up.
        void AcquireSensors()
        {
            gyro = gyro ?? Enable(InputGyroscope.current);
            linear = linear ?? Enable(LinearAccelerationSensor.current);
            gravity = gravity ?? Enable(GravitySensor.current);
            accel = accel ?? Enable(Accelerometer.current);
        }

        void OnDisable()
        {
            Disable(gyro); Disable(linear); Disable(gravity); Disable(accel);
            gyro = null; linear = null; gravity = null; accel = null;
        }

        static T Enable<T>(T device) where T : InputDevice
        {
            if (device != null && !device.enabled) InputSystem.EnableDevice(device);
            return device;
        }

        static void Disable(InputDevice device)
        {
            if (device != null && device.enabled) InputSystem.DisableDevice(device);
        }

        void Update()
        {
            if (gyro == null)
            {
                if (Time.unscaledTime < nextAcquire) return;
                nextAcquire = Time.unscaledTime + 1f;
                AcquireSensors();
                if (gyro == null) return;
            }
            RawGyro = gyro.angularVelocity.ReadValue();

            Vector3 rawAccel = accel != null ? accel.acceleration.ReadValue() * linearAccelerationScale : Vector3.zero;
            if (gravity != null) RawGravity = gravity.gravity.ReadValue();
            else if (accel != null) RawGravity = Vector3.Lerp(RawGravity, rawAccel, 0.1f); // low-pass = gravity

            if (linear != null) RawLinear = linear.acceleration.ReadValue() * linearAccelerationScale;
            else if (accel != null) RawLinear = rawAccel - RawGravity;

            if (Calibrating) return;
            if (detector.Feed(Time.deltaTime, ToV(RawGyro), ToV(RawLinear), ToV(RawGravity), out var kick))
            {
                LastKick = kick;
                Kicked?.Invoke(kick);
            }
        }

        public void Recenter() => detector?.Calibrate(ToV(RawGravity));

        /// <summary>
        /// Calibration screen: "swing the phone like a kick, once". Finds which gyro axis and sign
        /// that swing produces on this phone and remembers it. Calls back with true on success.
        /// </summary>
        public void CalibrateKick(float seconds, Action<bool> done)
        {
            StartCoroutine(CalibrateRoutine(seconds, done));
        }

        IEnumerator CalibrateRoutine(float seconds, Action<bool> done)
        {
            Calibrating = true;
            Vector3 peak = Vector3.zero;
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                Vector3 g = RawGyro;
                if (Mathf.Abs(g.x) > Mathf.Abs(peak.x)) peak.x = g.x;
                if (Mathf.Abs(g.y) > Mathf.Abs(peak.y)) peak.y = g.y;
                if (Mathf.Abs(g.z) > Mathf.Abs(peak.z)) peak.z = g.z;
                yield return null;
            }
            Calibrating = false;

            DeviceAxis axis = DeviceAxis.X;
            float best = Mathf.Abs(peak.x);
            if (Mathf.Abs(peak.y) > best) { axis = DeviceAxis.Y; best = Mathf.Abs(peak.y); }
            if (Mathf.Abs(peak.z) > best) { axis = DeviceAxis.Z; best = Mathf.Abs(peak.z); }
            bool ok = best > tuning.KickStartRate;
            if (ok)
            {
                float v = axis == DeviceAxis.X ? peak.x : (axis == DeviceAxis.Y ? peak.y : peak.z);
                tuning.KickAxis = axis;
                tuning.KickAxisSign = Mathf.Sign(v);
                PlayerPrefs.SetInt(PrefAxis, (int)axis);
                PlayerPrefs.SetFloat(PrefSign, tuning.KickAxisSign);
                PlayerPrefs.Save();
                detector = new MotionGestureDetector(tuning);
                Recenter();
            }
            done?.Invoke(ok);
        }

        static Vec3 ToV(Vector3 v) => new Vec3(v.x, v.y, v.z);
    }
}
