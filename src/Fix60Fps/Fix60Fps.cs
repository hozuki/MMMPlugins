using System;
using System.Drawing;
using System.Windows.Forms;
using MikuMikuPlugin;

namespace OpenMLTD.MMMPlugins.Fix60Fps {
    public sealed class Fix60Fps : ICommandPlugin {

        public void Dispose() {
        }

        public Guid GUID => PluginGuid;

        public string Description => "Fix imported 60 fps VMD motion frame number by hozuki";

        public IWin32Window ApplicationForm { get; set; }

        public string Text => "60 fps を修正";

        public string EnglishText => "Fix 60 fps";

        public Image Image { get; } = null;

        public Image SmallImage { get; } = null;

        public Scene Scene { get; set; }

        public void Run(CommandArgs e) {
            var scene = Scene;

            if (scene == null) {
                MessageBox.Show("Scene is missing.", Description, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
                return;
            }

            var needsConversion = NeedsConversion(out var message);

            if (!needsConversion) {
                if (message != null) {
                    MessageBox.Show(message, Description, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    return;
                }
            }

            foreach (var model in scene.Models) {
                foreach (var bone in model.Bones) {
                    foreach (var layer in bone.Layers) {
                        foreach (var frame in layer.Frames) {
                            frame.FrameNumber = frame.FrameNumber / 2;
                        }
                    }
                }

                foreach (var morph in model.Morphs) {
                    foreach (var frame in morph.Frames) {
                        frame.FrameNumber = frame.FrameNumber / 2;
                    }
                }
            }

            foreach (var camera in scene.Cameras) {
                foreach (var layer in camera.Layers) {
                    foreach (var frame in layer.Frames) {
                        frame.FrameNumber = frame.FrameNumber / 2;
                    }
                }
            }
        }

        private bool NeedsConversion(out string message) {
            var scene = Scene;

            if (scene.Models.Count == 0) {
                message = "There is no model in the scene.";
                return false;
            }

            foreach (var model in scene.Models) {
                foreach (var bone in model.Bones) {
                    foreach (var layer in bone.Layers) {
                        if (layer.Frames.Count > 2) {
                            var f0 = layer.Frames[0];
                            var f1 = layer.Frames[1];
                            var f2 = layer.Frames[2];

                            if (f1.FrameNumber - f0.FrameNumber == 2 && f2.FrameNumber - f1.FrameNumber == 2) {
                                message = null;
                                return true;
                            }
                        }
                    }
                }
            }

            message = "No 30 fps signature is found in frames. The motions may already be in 60 fps.";
            return false;
        }

        // 69f4cacb-c525-4fbd-b84e-58cb9021f2e5
        private static readonly Guid PluginGuid = new Guid(0x69f4cacb, 0xc525, 0x4fbd, 0xb8, 0x4e, 0x58, 0xcb, 0x90, 0x21, 0xf2, 0xe5);

    }
}
