using CyroHazard.Character;

namespace CyroHazard.Damage.Hediffs.Interface
{
    public interface IHediffPain
    {
        public float Pain { get; }
    }

    public interface IHediffBleeding
    {
        public float Bleeding { get; }
    }

    public struct HediffCapability
    {
        public enum ECapabilityMode : byte
        {
            Additive,
            Multiple,
            Max
        }

        public ECapabilityMode CapabilityMode;
        public float Factor;
    }

    public interface IHediffCapabilityMod
    {
        public HediffCapability GetHediffCapability(ECapability capability);
    }
}