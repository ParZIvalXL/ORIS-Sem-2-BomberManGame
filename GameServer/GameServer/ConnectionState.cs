namespace GameServer;

public enum ConnectionState
{
    GameStarted = 100,
    GameFinished = 101,
    Successful = 200,
    FullLobby = 400,
    PlayerAlreadyExists = 404,
    GameIsNotStarted = 500,
}