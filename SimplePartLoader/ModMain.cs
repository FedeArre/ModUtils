
using UnityEngine;

namespace SimplePartLoader
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "SimplePartLoader";
        public override string Name => "SimplePartLoader";
        public override string Author => "Federico Arredondo";
        public override string Version => "dev";

        public override void OnLoad()
        {
            PartManager.OnLoadCalled();
        }

        string ads = "";

        public override void OnGUI()
        {
            ads = GUI.TextArea(new Rect(50, 50, 200, 600), ads);
        }

        public override void Update()
        {
            RaycastHit raycastHit;

            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit, 1.4f, Physics.AllLayers)){
                ads = raycastHit.collider.name + " - " + raycastHit.collider.tag + " - " + LayerMask.LayerToName(raycastHit.collider.gameObject.layer);

                foreach(Component c in raycastHit.collider.GetComponents<Component>())
                {
                    ads += "\n" + c.GetType();
                }
            }
        }
    }
}