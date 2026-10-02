using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS_Komachi_Onozuka.STS_Komachi_OnozukaCode.Commands
{
    public static class KomachiHooks
    {
        

        private static async Task Dispatch<T>(PlayerChoiceContext choiceContext, ICombatState? combatState, Func<T, Task> invoke) where T : class
        {
            if (combatState == null)
            {
                return;
            }

            foreach (T item in combatState.IterateHookListeners().OfType<T>().ToArray())
            {
                AbstractModel? model = item as AbstractModel;
                choiceContext.PushModel(model);
                try
                {
                    await invoke(item);
                }
                finally
                {
                    choiceContext.PopModel(model);
                }
            }
        }

        public static Task OnDistanceChanged(PlayerChoiceContext choiceContext, DistanceChangedEventArgs args)
        {
            var dis = Dispatch(choiceContext, args.Target.CombatState, (IOnDistanceChangedListener m) => m.OnDistanceChanged(choiceContext, args));
            EnemyIntentDistancePreviewController.OnDistanceChanged(args.Target);
            return dis;
        }
        public static Task OnReleasing(PlayerChoiceContext choiceContext, ReleaseArgs args)
        {
            return Dispatch(choiceContext, args.creature.CombatState,
                (IOnReleasingListener m) => m.OnReleasing(choiceContext, args));
        }
        public static Task OnReleased(PlayerChoiceContext choiceContext, ReleaseArgs args)
        {
            return Dispatch(choiceContext, args.creature.CombatState,
                (IOnReleasedListener m) => m.OnReleased(choiceContext, args));
        }
        public static Task OnDetonating(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            return Dispatch(choiceContext, args.Target.CombatState,
                (IOnDetonatingListener m) => m.OnDetonating(choiceContext, args));
        }

        // Helper to get CombatState from Target, or fallback to Dealer if Target died
        private static ICombatState? ResolveCombatState(DetonationEventArgs args)
        {
            return args.Target?.CombatState ?? args.Dealer?.CombatState;
        }

        public static Task OnDetonatedEarly(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            return Dispatch(choiceContext, ResolveCombatState(args),
                (IOnDetonatedEarlyListener m) => m.OnDetonatedEarly(choiceContext, args));
        }

        public static Task OnDetonated(PlayerChoiceContext choiceContext, DetonationEventArgs args)
        {
            return Dispatch(choiceContext, ResolveCombatState(args),
                (IOnDetonatedListener m) => m.OnDetonated(choiceContext, args));
        }
    }
}
