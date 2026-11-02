using UnityEngine;

namespace Sheytoonak
{
    /// <summary>
    /// The football. Real Rigidbody physics plus wind and Magnus (curl from side spin).
    /// It only reports what it touched; <see cref="GameManager"/> applies the rules.
    /// Prefab needs: SphereCollider (radius = GameConfig.ballRadius) with a bouncy PhysicsMaterial
    /// (bounciness ~0.6, bounce combine Maximum), and the football model as a child.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class BallController : MonoBehaviour
    {
        [Tooltip("Continuous ground contact longer than this counts as a bounce (a ball that stops hopping and rolls).")]
        public float rollCountsAsBounceAfter = 0.3f;

        public Rigidbody Body { get; private set; }
        public float Radius { get; private set; } = 0.11f;
        public bool InPlay => Body != null && !Body.isKinematic && gameObject.activeSelf;

        IBallListener gm;
        float spinY, wind, magnus;
        float groundStay;
        bool rollReported;

        public void Init(IBallListener manager, GameConfig cfg)
        {
            gm = manager;
            Body = GetComponent<Rigidbody>();
            Body.mass = cfg.ballMass;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // 28 m/s against a thin facade
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            Radius = cfg.ballRadius;
            GetComponent<SphereCollider>().radius = cfg.ballRadius / Mathf.Max(0.0001f, transform.lossyScale.x);
            magnus = cfg.magnusCoefficient;
        }

        /// <summary>Ball waits motionless (serve position).</summary>
        public void PlaceAtRest(Vector3 position)
        {
            gameObject.SetActive(true);
            Body.isKinematic = true;
            transform.SetPositionAndRotation(position, Quaternion.identity);
            spinY = 0f;
            groundStay = 0f;
            rollReported = false;
        }

        public void Launch(Vector3 velocity, float sideSpin, float windAccel)
        {
            gameObject.SetActive(true);
            Body.isKinematic = false;
            Body.linearVelocity = velocity;
            Body.angularVelocity = new Vector3(velocity.z / Radius, sideSpin, 0f); // forward roll + curl, for the visuals
            spinY = sideSpin;
            wind = windAccel;
            groundStay = 0f;
            rollReported = false;
        }

        /// <summary>Re-aim a flying ball (facade return assist, owner's throw).</summary>
        public void SetVelocity(Vector3 velocity, bool keepSpin = false)
        {
            Body.linearVelocity = velocity;
            if (!keepSpin) spinY = 0f;
        }

        public void Hide()
        {
            if (Body != null) Body.isKinematic = true;
            gameObject.SetActive(false);
        }

        void FixedUpdate()
        {
            if (Body == null || Body.isKinematic) return;
            Vector3 v = Body.linearVelocity;
            // Magnus: F = k (w x v) with w = (0, spinY, 0)  ->  (spinY * vz, 0, -spinY * vx)
            Vector3 curl = magnus * new Vector3(spinY * v.z, 0f, -spinY * v.x);
            Body.AddForce(curl + new Vector3(wind * Body.mass, 0f, 0f), ForceMode.Force);
            spinY *= 1f - 0.4f * Time.fixedDeltaTime; // spin dies off slowly
        }

        void OnCollisionEnter(Collision c)
        {
            if (gm == null) return;
            Vector3 p = c.contactCount > 0 ? c.GetContact(0).point : transform.position;
            if (c.collider.GetComponent<GroundSurface>() != null)
            {
                groundStay = 0f;
                gm.OnBallGround(p);
            }
            else if (c.collider.GetComponent<FacadeSurface>() != null)
            {
                gm.OnBallFacade(p);
            }
        }

        void OnCollisionStay(Collision c)
        {
            if (gm == null || rollReported) return;
            if (c.collider.GetComponent<GroundSurface>() == null) return;
            groundStay += Time.fixedDeltaTime;
            if (groundStay >= rollCountsAsBounceAfter)
            {
                rollReported = true;
                gm.OnBallRolling(transform.position);
            }
        }

        void OnCollisionExit(Collision c)
        {
            if (c.collider.GetComponent<GroundSurface>() != null) { groundStay = 0f; rollReported = false; }
        }

        void OnTriggerEnter(Collider other)
        {
            if (gm == null) return;
            var kill = other.GetComponent<KillZone>();
            if (kill != null) gm.OnBallKilled(kill.reason);
        }
    }
}
