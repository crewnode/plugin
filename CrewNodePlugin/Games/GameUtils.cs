using Impostor.Api.Innersloth;
using Impostor.Api.Net.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrewNodePlugin.Games
{
    public class GameUtils
    {
        public static IMessageWriterProvider Provider { get; set; }

        /// <summary>
        /// This is used to generate individual GameOptionsData packets.
        /// </summary>
        /// <param name="data">The options to serialize.</param>
        /// <param name="game">The Game's ID.</param>
        /// <param name="netId">The NetId of the player (as found in the Character.NetId property).</param>
        /// <returns>A packet that can be sent to the client directly.</returns>
        public static IMessageWriter GenerateDataPacket(GameOptionsData data, int game, uint netId)
        {
            var writer = Provider.Get(MessageType.Reliable);
            writer.StartMessage(Impostor.Api.Net.Messages.MessageFlags.GameData);
            writer.Write(game);
            writer.StartMessage((byte)2);
            writer.WritePacked(netId);
            writer.Write((byte)2);
            using (var stream = new MemoryStream())
            using (BinaryWriter bWriter = new BinaryWriter(stream))
            {
                data.Serialize(bWriter, 4);
                writer.WriteBytesAndSize(stream.ToArray());
            }
            writer.EndMessage();
            writer.EndMessage();
            return writer;
        }
    }
}
