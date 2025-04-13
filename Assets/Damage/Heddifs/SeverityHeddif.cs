
namespace CyroHazard.Character.Damage.Hediffs
{
    public interface ISeverityHediff
    {
        public float Severity { get; set; }

        public void OnMaxSeverity();
    }
}
