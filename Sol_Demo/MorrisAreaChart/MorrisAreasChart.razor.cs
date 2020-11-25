using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorrisAreaChart
{
    public partial class MorrisAreasChart
    {
        #region Declaration

        private Task<IJSObjectReference> _module = null;

        #endregion Declaration

        #region Public Property

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public String ItemSourceJson { get; set; }

        [Parameter]
        public String Xkey { get; set; }

        [Parameter]
        public String YKeysJson { get; set; }

        [Parameter]
        public String LablesJson { get; set; }

        [Parameter]
        public EventCallback<dynamic> OnHover { get; set; }

        [Parameter]
        public int Height { get; set; }

        #endregion Public Property

        #region Private Property

        private ElementReference DivAreaChartElement { get; set; }

        private static Action<Task<dynamic>> ActionJs { get; set; }

        #endregion Private Property

        #region Private Method

        private void LoadJsModules()
        {
            _module = JSRuntime
                            .InvokeAsync<IJSObjectReference>("import", "./_content/MorrisAreaChart/AreaChart.js")
                            .AsTask();
        }

        private String SetHeightStyle()
        {
            return
                new StringBuilder()
                .Append("height:")
                .Append(Height)
                .Append("px;")
                .ToString();
        }

        private async Task OnLoadAreaChartJs()
        {
            await (await _module).InvokeVoidAsync(identifier: "drawAreaChart", DivAreaChartElement, ItemSourceJson, Xkey, YKeysJson, LablesJson);
        }

        #endregion Private Method

        #region Public & Protected Method

        [JSInvokable]
        public static Task OnHoverJs(string datajson)
        {
            return Task.Run(() =>
            {
                var data = JsonConvert.DeserializeObject<dynamic>(datajson);

                ActionJs.Invoke(Task.FromResult<dynamic>(data));
            });
        }

        public async Task SetAreaChartAsync()
        {
            LoadJsModules();

            await this.OnLoadAreaChartJs();

            base.StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            if (_module != null)
            {
                await (await _module).DisposeAsync();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                ActionJs = async (data) =>
                  {
                      await base.InvokeAsync(async () =>
                      {
                          await this.OnHover.InvokeAsync(await data);
                          this.StateHasChanged();
                      });
                  };
            }
        }

        #endregion Public & Protected Method
    }
}