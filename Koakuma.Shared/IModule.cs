using Koakuma.Shared.Messages;

namespace Koakuma.Shared
{
    public interface IModule
    {
        ModuleFeatures Features { get; }
        string ID { get; }

        IKoakuma Koakuma { get; set; }

        ModuleConfig Config { get; set; }

        void Load();

        void Reload();

        void Unload();

        void OnMessage(ModuleID from, BaseMessage msg, byte[] payload);
    }
}