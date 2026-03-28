using UnityEngine;
using Fusion;
public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local {  get; set; }
    public void PlayerLeft(PlayerRef player)
    {
        if(player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }
    public override void Spawned()
    {
        // Коли додається новий гравець, оновлюємо список і підписки
        LeaderboardUIHandler uiHandler = GameObject.FindGameObjectWithTag("Leaderboard").GetComponent<LeaderboardUIHandler>();
        if (uiHandler != null)
        {
            uiHandler.ReloadList();
        }

        if (Object.HasInputAuthority)
        {
            Local = this;

            print("Spawned own car");
        }
        else print("Spawned other player car");
    }
}
