using GameServer.Packages;
using System;

namespace GameServer
{
    public class PlayerHandler
    {
        private const float Tolerance = 0.05f;

        public static (int, int) GetPlayerCoordinates(PlayerPackage player)
        {
            int gridX = GetGridCoordinate(player.PositionX);
            int gridY = GetGridCoordinate(player.PositionY);
            return (gridX, gridY);
        }

        private static int GetGridCoordinate(double position)
        {
            var adjustedPosition = position + Tolerance;
            return (int)Math.Floor(adjustedPosition);
        }
    }
}