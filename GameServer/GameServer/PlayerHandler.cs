using GameServer.Packages;
using System;

namespace GameServer
{
    public class PlayerHandler
    {
        private const float Tolerance = 0.05f;

        public static int[] GetPlayerCoordinates(PlayerPackage player)
        {
            int gridX = GetGridCoordinate(player.PositionX);
            int gridY = GetGridCoordinate(player.PositionY);

            return new []{ gridX, gridY };
        }

        private static int GetGridCoordinate(float position)
        {
            float adjustedPosition = position + Tolerance;
            int gridCoordinate = (int)Math.Floor(adjustedPosition);
            return gridCoordinate;
        }
    }
}