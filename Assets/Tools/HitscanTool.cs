using UnityEngine;

namespace CyroHazard.Tools
{
    public interface IInteractableTool
    {
        public virtual void OnFire() { }
        public virtual void OnFire2() { }
    }

    public class HitscanTool : BaseTool, IInteractableTool
    {
        public void OnFire()
        {
            Vector3 origin = Controller.transform.position,
            dir = Controller.GetAimDirection();

            Debug.DrawRay(origin, dir * 60, Color.green, 3.0f);
        }
    }
}