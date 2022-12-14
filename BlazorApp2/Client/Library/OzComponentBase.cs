using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorApp2.Client.Library
{
    public class OzComponentBase : ComponentBase, IDisposable
    {
        [Inject]
        protected NavigationManager? nav { get; set; }
        [Inject]
        protected EventAggregator? eve { get; set; }

        Subscription<NavigationContext>? sub;
        protected override void OnInitialized()
        {
            if (eve != null)
            {
                sub = eve.Subscribe<NavigationContext>(OnNavigationChanged);
            }

            base.OnInitialized();
        }

        /// <summary>
        /// Navigation이 바뀌었음을 대응합니다.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnNavigationChanged(NavigationContext args)
        {

        }

        /// <summary>
        /// Dispose 처리합니다.
        /// </summary>
        public virtual void Dispose()
        {
            if (eve != null && sub != null)
                eve.UnSbscribe(sub);
        }
    }
}
