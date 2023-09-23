using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orcs_Plus
{
    public class UMTaskDelegates
    {
        public Map map;

        public UMTaskDelegates(Map map)
        {
            this.map = map;
        }

        public static void onClick_RazeOutpost(World world, UM um, UIE_Challenge challenge)
        {
            if (um.task is Task_RazeOutpost)
            {
                world.prefabStore.popMsg("Already razing outpost", false, false);
            }
            else
            {
                um.task = new Task_RazeOutpost();
                world.ui.checkData();
            }
        }
    }
}
