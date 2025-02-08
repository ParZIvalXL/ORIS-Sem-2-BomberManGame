using GameServer.Packages;
using Newtonsoft.Json;

namespace GameServer;

public class BombScripts
{
    public void ExplodeBomb(TileType[,] map, int bombX, int bombY, string playerNickname)
    {
        var top = bombY;
        var bottom = bombY;
        var left = bombX;
        var right = bombX;

        while (top != 20)
        {
            if (map[bombX, top] == TileType.B)
            {
                map[bombX, top] = TileType.Ex;
                break;
            }

            if (map[bombX, top] == TileType.D)
            {
                break;
            }

            top++;
        }
        while (bottom != 0)
        {
            if (map[bombX, bottom] == TileType.B)
            {
                map[bombX, bottom] = TileType.Ex;
                break;
            }

            if (map[bombX, bottom] == TileType.D)
            {
                break;
            }

            bottom -= 1;
        }

        while (left != 0)
        {
            if (map[left, bombY] == TileType.B)
            {
                map[left, bombY] = TileType.Ex;
                break;
            }

            if (map[left, bombY] == TileType.D)
            {
                break;
            }

            left -= 1;
        }
        
        while (right != 20)
        {
            if (map[right, bombY] == TileType.B)
            {
                map[right, bombY] = TileType.Ex;
                break;
            }

            if (map[right, bombY] == TileType.D)
            {
                break;
            }

            right += 1;
        }

        var players = ClientHandler.server._playersListPackage;
        
        players.Sort((player1, player2) =>
        {
            var (x1, y1) = PlayerHandler.GetPlayerCoordinates(player1);
            var (x2, y2) = PlayerHandler.GetPlayerCoordinates(player2);

            bool isNearBomb1 = x1 == bombX || y1 == bombY;
            bool isNearBomb2 = x2 == bombX || y2 == bombY;

            if (isNearBomb1 && !isNearBomb2) return -1;
            if (!isNearBomb1 && isNearBomb2) return 1;
            return 0;
        });
        
        foreach (var player in players)
        {
            var coordinates = PlayerHandler.GetPlayerCoordinates(player);
            if ((left <= coordinates.Item1 && coordinates.Item1 <= right && coordinates.Item2 == bombY) || (bottom <= coordinates.Item2 && coordinates.Item2 <= top && coordinates.Item1 == bombX))
            {
                player.Health -= 25;
                if (player.Health == 0)
                {
                    
                }
                
                for (int i = 0; i < ClientHandler.server._playersListPackage.Count; i++)
                {
                    
                    if (ClientHandler.server._playersListPackage[i].Nickname == player.Nickname)
                    {
                        ClientHandler.server._playersListPackage[i].Health = player.Health;
                    }
                }
                Console.WriteLine($"Игрок {player.Nickname} попал под взрыв бомбы игрока {playerNickname}, его здоровье = {player.Health}");
            }
        }
    }
}