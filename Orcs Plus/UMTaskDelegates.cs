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
        Map map;

        public UMTaskDelegates(Map map)
        {
            this.map = map;
        }

        public static void onClick_RazeOutpost(World world, UM um, UIE_Challenge challenge)
        {
            Task_RazeOutpost task_RazeOutpost = um.task as Task_RazeOutpost;
            if (task_RazeOutpost != null )
            {
                world.prefabStore.popMsg("Already razing settlement", false, false);
            }
            else
            {
                um.task = new Task_RazeOutpost();
                world.ui.checkData();
            }
        }
    }
}
