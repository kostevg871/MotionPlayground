using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MotionPlayground.Views
{
    public partial class Model3DView : ContentView
    {
        private bool _ready;

        public Model3DView()
        {
            InitializeComponent();
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                using var s = await FileSystem.OpenAppPackageFileAsync("rubiks_cube.glb");
                using var ms = new MemoryStream();
                await s.CopyToAsync(ms);

                var base64 = Convert.ToBase64String(ms.ToArray());
                var src = $"data:model/gltf-binary;base64,{base64}";

                var html = $@"
<!doctype html>
<html>
<head>
  <meta charset='utf-8'/>
  <meta name='viewport' content='width=device-width, initial-scale=1'/>
  <script type='module' src='https://unpkg.com/@google/model-viewer/dist/model-viewer.min.js'></script>
  <style>
    html, body {{ margin:0; padding:0; width:100%; height:100%; background:transparent; overflow:hidden; }}
    model-viewer {{ width:100%; height:100%; background:transparent; }}
  </style>
</head>
<body>
  <model-viewer id='mv'
    src='{src}'
    camera-controls
    interaction-prompt='none'
    shadow-intensity='0'
    exposure='1.0'
    style='background: transparent;'>
  </model-viewer>

  <script>
    window.setYaw = function(deg) {{
      const mv = document.getElementById('mv');
      if (!mv) return;
      mv.cameraOrbit = deg + 'deg 75deg 2.5m';
    }};
    window.resetYaw = function() {{ window.setYaw(0); }};
  </script>
</body>
</html>";

                ModelWebView.Source = new HtmlWebViewSource { Html = html };
                ErrorLabel.IsVisible = false;
                _ready = true;
            }
            catch (Exception ex)
            {
                ErrorLabel.IsVisible = true;
                ErrorLabel.Text = "3D не запустился: " + ex.Message;
                _ready = false;
            }
        }

        public void SetYaw(double degrees)
        {
            if (!_ready) return;

            var d = degrees.ToString("0.###", CultureInfo.InvariantCulture);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await ModelWebView.EvaluateJavaScriptAsync($"setYaw({d});"); }
                catch { }
            });
        }

        public void ResetYaw()
        {
            if (!_ready) return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try { await ModelWebView.EvaluateJavaScriptAsync("resetYaw();"); }
                catch { }
            });
        }
    }
}
