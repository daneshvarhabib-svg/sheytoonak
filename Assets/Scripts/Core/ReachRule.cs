using System;

namespace Sheytoonak.Core
{
    /// <summary>
    /// Can the player reach the ball? Pure geometry so the EditMode tests can pin it.
    /// Rule from the first phone test (user, 2026-10-23): if the player is in front of the ball, a kick
    /// must be possible. A returning ball crosses the player in a fraction of a second, so a small box
    /// around a fixed spot was true for only a few frames and the ball seemed to fly over the player.
    /// Now the ball may be anywhere from <c>reachBehind</c> behind the strike spot to <c>reachAhead</c>
    /// in front (a lunge), within lateral reach, and below head height. World axes: x right, y up,
    /// z toward the facade.
    /// </summary>
    public static class ReachRule
    {
        public static bool InReach(Vec3 ball, float ballRadius, Vec3 strikeSpot,
                                   float reachX, float reachBehind, float reachAhead, float maxKickHeight)
        {
            float dz = ball.Z - strikeSpot.Z;
            return Math.Abs(ball.X - strikeSpot.X) <= reachX
                && dz >= -reachBehind && dz <= reachAhead
                && ball.Y - ballRadius <= maxKickHeight;
        }
    }
}
