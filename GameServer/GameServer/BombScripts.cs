using GameServer.Packages;
using Newtonsoft.Json;

namespace GameServer;

public class BombScripts
{
    public void ExplodeBomb(TileType[,] map, int bombX, int bombY)
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

        var players = ClientHandler.GetPlayersList()
            .Where(p => Math.Abs(p.PositionX - (float)bombX) < 0.01 || Math.Abs(p.PositionY - (float)bombY) < 0.01).ToList();
        Console.WriteLine("Players in range: " + players.Count);

        foreach (var player in players)
        {
            var coordinates = PlayerHandler.GetPlayerCoordinates(player);
            if ((left <= coordinates.Item1 && coordinates.Item1 <= right && coordinates.Item2 == bombY) || (bottom <= coordinates.Item2 && coordinates.Item2 <= top && coordinates.Item1 == bombX))
            {
                player.Health -= 25;
                for (int i = 0; i < ClientHandler._playersListPackage.Count; i++)
                {
                    var pl = JsonConvert.DeserializeObject<PlayerPackage>(ClientHandler._playersListPackage[i]);
                    if (pl.Nickname.Equals(player.Nickname))
                    {
                        pl.Health = player.Health;

                        ClientHandler._playersListPackage[i] = JsonConvert.SerializeObject(pl);
                    }
                }
                Console.WriteLine($"Игрок {player.Nickname} попал под взрыв, его здоровье = {player.Health}");
            }
        }
    }
}