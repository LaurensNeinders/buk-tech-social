using System;
using System.Linq;
using Photon.Voice.Unity;
using Photon.Voice.Unity.UtilityScripts;
using UnityEngine;

namespace Buk.Multiplayer
{
  public class BukNetworkManagerVoice : Buk.Multiplayer.BukNetworkManager
  {
    public override void OnServerChangeScene(string _)
    {
      var voiceConnection = GameObject.FindObjectsOfType<VoiceConnection>().SingleOrDefault();
      if (voiceConnection != null && voiceConnection.Client.IsConnected)
      {
        voiceConnection.Client.OpLeaveRoom(false);
      }
    }

    public override void OnServerSceneChanged(string newSceneName)
    {
      var connectAndJoin = GameObject.FindObjectsOfType<ConnectAndJoin>().SingleOrDefault();
      if (connectAndJoin == null)
      {
        throw new ApplicationException("No voiceconnection, could not join room");
      }
      else if (!connectAndJoin.IsConnected)
      {
        connectAndJoin.RoomName = $"{name}/${newSceneName}";
        connectAndJoin.ConnectNow();
      }
    }
  }
}
