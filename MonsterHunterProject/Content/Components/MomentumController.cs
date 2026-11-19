using EntityStates;
using RoR2;
using UnityEngine;

namespace MonsterHunter.Content.Components
{
    public class MomentumController : MonoBehaviour
    {
        public bool shouldCalc = false;

        public float momentum = 0; // from -1 to 1

        public float desiredMomentum = 0;

        private bool calced = false;

        private int frame = 0;
        private static int framesBetweenRecalc = 20;

        public static float speedMultiplier = 1f;

        private CharacterBody body;
        private static float groundDecay = 0.5f;
        private static float fastDecay = 0.8f;
        private static float slowDecay = 1.35f;

        private void Start()
        {
            this.body = GetComponent<CharacterBody>();
        }

        private void FixedUpdate()
        {
            if (this.shouldCalc)
            {
                this.frame = (this.frame + 1) % framesBetweenRecalc;
                if (this.frame == 0) this.CalculateMomentum();
            }
            else if (!this.calced)
            {
                this.calced = true;
                this.momentum = 0;
                this.body.MarkAllStatsDirty();
            }
        }

        private void CalculateMomentum()
        {
            if (this.body.characterMotor.velocity != Vector3.zero && this.body.characterMotor.isGrounded)
            {
                this.calced = false;
                Vector3 forward = VelocityOnGround(this.body.characterMotor.velocity); //body.characterMotor.moveDirection.normalized;
                float dot = Vector3.Dot(forward, Vector3.down);
                this.desiredMomentum = Mathf.Clamp(dot * 2f, -1f, 1f);
                this.MomentumCalculation(slowDecay, fastDecay);
            }
            else
            {
                this.desiredMomentum = 0;
                float momentumDecay = groundDecay;
                this.MomentumCalculation(momentumDecay, momentumDecay);
            }
        }

        private Vector3 VelocityOnGround(Vector3 velocity)
        {
            velocity.y = 0;
            return Vector3.ProjectOnPlane(velocity, this.body.characterMotor.estimatedGroundNormal).normalized;
        }

        private void MomentumCalculation(float slowDecay, float fastDecay)
        {
            if (Mathf.Abs(this.desiredMomentum - this.momentum) > 0.1f)
            {
                if (this.desiredMomentum > this.momentum)
                {
                    this.momentum = Mathf.Clamp(this.momentum + ((this.desiredMomentum - this.momentum) * Time.fixedDeltaTime * slowDecay * framesBetweenRecalc), -1f, this.desiredMomentum);
                }
                else
                {
                    this.momentum = Mathf.Clamp(this.momentum + ((this.desiredMomentum - this.momentum) * Time.fixedDeltaTime * fastDecay * framesBetweenRecalc), this.desiredMomentum, 1f);
                }
                //this.body.MarkAllStatsDirty();
                //Chat.AddMessage("mom " + momentum.ToString() + " des " + desiredMomentum.ToString() + " dot " + dot.ToString());
            }
            else
            {
                this.momentum = this.desiredMomentum;
                if (!this.calced)
                {
                    this.calced = true;
                    //this.body.MarkAllStatsDirty();
                }
            }
        }
    }
}